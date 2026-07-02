"""
Migracion idempotente: agrega CENTER_CODE a la tabla de equipos (LYNX_PM_EQUIPMENTS).
Re-ejecutable sin efecto. Modo thick (el servidor exige Native Network Encryption).

Uso:  IBERO_DB_PASS debe estar en el entorno.
      python scripts/migrate_add_equipment_center_code.py
"""
import os
import oracledb

DSN = "193.122.157.114:1521/lemopdb1.sub03121625300.lemovcn.oraclevcn.com"
USER = "IBEROPHARMAPROD"
INSTANT_CLIENT_DIR = r"C:\oracle\instantclient_21_15"

TABLE = "LYNX_PM_EQUIPMENTS"
NEW_COLUMNS = {
    "CENTER_CODE": "VARCHAR2(50 CHAR)",
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

    cur.execute("SELECT COUNT(*) FROM user_tables WHERE table_name = :t", t=TABLE)
    if cur.fetchone()[0] == 0:
        raise SystemExit(f"ERROR: no se encontró la tabla {TABLE}.")

    cur.execute("SELECT column_name FROM user_tab_columns WHERE table_name = :t", t=TABLE)
    present = {row[0] for row in cur.fetchall()}

    missing = {c: d for c, d in NEW_COLUMNS.items() if c not in present}
    if not missing:
        print("Todas las columnas ya existen. Nada que hacer (no-op).")
        conn.close()
        return

    cols_ddl = ", ".join(f"{c} {d}" for c, d in missing.items())
    ddl = f"ALTER TABLE {TABLE} ADD ({cols_ddl})"
    print(f"Ejecutando: {ddl}")
    cur.execute(ddl)
    conn.commit()
    print(f"OK: columnas agregadas -> {list(missing.keys())}")

    cur.execute(
        "SELECT column_name, data_type, char_length FROM user_tab_columns "
        "WHERE table_name = :t AND column_name = 'CENTER_CODE'",
        t=TABLE,
    )
    for name, dtype, length in cur.fetchall():
        print(f"  verificado: {name} {dtype}({length})")

    conn.close()
    print("Migracion completada.")


if __name__ == "__main__":
    main()
