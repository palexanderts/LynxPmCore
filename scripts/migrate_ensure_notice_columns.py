"""
Migración idempotente: asegura las columnas extendidas de la tabla de avisos
(prioridad y tipo de aviso SAP). Re-ejecutable sin efecto.

- Autodetecta el nombre real de la tabla (LYNXCORE_AVISO o LYNX_PM_AVISO).
- Solo agrega columnas que falten (aditivo, nullable, no destructivo).
- Modo thick de python-oracledb (el servidor exige Native Network Encryption).

Uso:  IBERO_DB_PASS debe estar en el entorno.
      python scripts/migrate_ensure_notice_columns.py
"""
import os
import oracledb

DSN = "193.122.157.114:1521/lemopdb1.sub03121625300.lemovcn.oraclevcn.com"
USER = "IBEROPHARMAPROD"
INSTANT_CLIENT_DIR = r"C:\oracle\instantclient_21_15"

CANDIDATE_TABLES = ["LYNXCORE_AVISO", "LYNX_PM_AVISO"]
NEW_COLUMNS = {
    "PRIORITY_CODE": "VARCHAR2(10 CHAR)",
    "PRIORITY_TEXT": "VARCHAR2(100 CHAR)",
    "NOTICE_TYPE_CODE": "VARCHAR2(10 CHAR)",
    "NOTICE_TYPE_TEXT": "VARCHAR2(100 CHAR)",
    "CENTER": "VARCHAR2(50 CHAR)",
}


def main() -> None:
    password = os.environ.get("IBERO_DB_PASS")
    if not password:
        raise SystemExit("ERROR: variable de entorno IBERO_DB_PASS no definida.")

    if os.path.isdir(INSTANT_CLIENT_DIR):
        oracledb.init_oracle_client(lib_dir=INSTANT_CLIENT_DIR)
    else:
        oracledb.init_oracle_client()

    conn = oracledb.connect(user=USER, password=password, dsn=DSN)
    cur = conn.cursor()
    print(f"Conectado a {USER}@{DSN}")

    cur.execute(
        "SELECT table_name FROM user_tables WHERE table_name IN ('LYNXCORE_AVISO','LYNX_PM_AVISO')"
    )
    existing = {row[0] for row in cur.fetchall()}

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

    target = targets[0]
    print(f"Tabla objetivo: {target}"
          + (f"  (aviso: también existe {targets[1]})" if len(targets) > 1 else ""))

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

    cur.execute(
        "SELECT column_name, data_type, char_length FROM user_tab_columns "
        "WHERE table_name = :t AND column_name IN "
        "('PRIORITY_CODE','PRIORITY_TEXT','NOTICE_TYPE_CODE','NOTICE_TYPE_TEXT','CENTER') "
        "ORDER BY column_name",
        t=target,
    )
    for name, dtype, length in cur.fetchall():
        print(f"  verificado: {name} {dtype}({length})")

    conn.close()
    print("Migracion completada.")


if __name__ == "__main__":
    main()
