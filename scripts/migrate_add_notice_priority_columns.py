"""
Migración idempotente: agrega PRIORITY_CODE / PRIORITY_TEXT a la tabla de avisos.

- Autodetecta el nombre real de la tabla (LYNXCORE_AVISO o LYNX_PM_AVISO).
- Solo agrega columnas que falten (aditivo, nullable, no destructivo).
- Modo thin de python-oracledb (no requiere Instant Client).

Uso:  IBERO_DB_PASS debe estar en el entorno.
      python scripts/migrate_add_notice_priority_columns.py
"""
import os
import oracledb

DSN = "193.122.157.114:1521/lemopdb1.sub03121625300.lemovcn.oraclevcn.com"
USER = "IBEROPHARMAPROD"
# El servidor exige Native Network Encryption -> requiere modo thick (Instant Client).
INSTANT_CLIENT_DIR = r"C:\oracle\instantclient_21_15"

CANDIDATE_TABLES = ["LYNXCORE_AVISO", "LYNX_PM_AVISO"]
NEW_COLUMNS = {
    "PRIORITY_CODE": "VARCHAR2(10 CHAR)",
    "PRIORITY_TEXT": "VARCHAR2(100 CHAR)",
}


def main() -> None:
    password = os.environ.get("IBERO_DB_PASS")
    if not password:
        raise SystemExit("ERROR: variable de entorno IBERO_DB_PASS no definida.")

    if os.path.isdir(INSTANT_CLIENT_DIR):
        oracledb.init_oracle_client(lib_dir=INSTANT_CLIENT_DIR)  # modo thick
    else:
        oracledb.init_oracle_client()  # buscar client en PATH

    conn = oracledb.connect(user=USER, password=password, dsn=DSN)
    cur = conn.cursor()
    print(f"Conectado a {USER}@{DSN}")

    # 1. Detectar la tabla de avisos real (debe tener la columna NOTICE_NUMBER)
    cur.execute(
        "SELECT table_name FROM user_tables WHERE table_name IN ('LYNXCORE_AVISO','LYNX_PM_AVISO')"
    )
    existing = {row[0] for row in cur.fetchall()}
    print(f"Tablas candidatas encontradas: {sorted(existing) or 'ninguna'}")

    targets = []
    for name in CANDIDATE_TABLES:
        if name not in existing:
            continue
        cur.execute(
            "SELECT COUNT(*) FROM user_tab_columns WHERE table_name = :t AND column_name = 'NOTICE_NUMBER'",
            t=name,
        )
        if cur.fetchone()[0] > 0:
            targets.append(name)

    if not targets:
        raise SystemExit("ERROR: no se encontró la tabla de avisos (con columna NOTICE_NUMBER).")

    # Preferir la que usa EF en runtime (LYNXCORE_AVISO) si existen ambas.
    target = targets[0]
    print(f"Tabla objetivo: {target}"
          + (f"  (aviso: también existe {targets[1]})" if len(targets) > 1 else ""))

    # 2. Ver qué columnas ya existen
    cur.execute("SELECT column_name FROM user_tab_columns WHERE table_name = :t", t=target)
    present = {row[0] for row in cur.fetchall()}

    missing = {c: d for c, d in NEW_COLUMNS.items() if c not in present}
    if not missing:
        print("Todas las columnas ya existen. Nada que hacer (no-op).")
        conn.close()
        return

    cols_ddl = ", ".join(f"{c} {d}" for c, d in missing.items())
    ddl = f"ALTER TABLE {target} ADD ({cols_ddl})"
    print(f"Ejecutando: {ddl}")
    cur.execute(ddl)
    conn.commit()
    print(f"OK: columnas agregadas -> {list(missing.keys())}")

    # 3. Verificación
    cur.execute(
        "SELECT column_name, data_type, char_length FROM user_tab_columns "
        "WHERE table_name = :t AND column_name IN ('PRIORITY_CODE','PRIORITY_TEXT') "
        "ORDER BY column_name",
        t=target,
    )
    for name, dtype, length in cur.fetchall():
        print(f"  verificado: {name} {dtype}({length})")

    conn.close()
    print("Migración completada.")


if __name__ == "__main__":
    main()
