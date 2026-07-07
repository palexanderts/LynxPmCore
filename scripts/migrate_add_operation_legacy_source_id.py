"""
Migracion idempotente: agrega LEGACY_SOURCE_ID a LYNXCORE_OPERATIONS.

Parte del job de sincronizacion legacy -> nuevo (LYNX_PM_AVISO_OPERATIONS ->
LYNXCORE_OPERATIONS). Se usa junto con NOTICE.APEX_ID (ya existente) para que el
job sea idempotente (no duplicar en cada corrida).

Uso:  IBERO_DB_PASS debe estar en el entorno.
      python scripts/migrate_add_operation_legacy_source_id.py
"""
import os
import oracledb

DSN = "193.122.157.114:1521/lemopdb1.sub03121625300.lemovcn.oraclevcn.com"
USER = "IBEROPHARMAPROD"
INSTANT_CLIENT_DIR = r"C:\oracle\instantclient_21_15"

TABLE = "LYNXCORE_OPERATIONS"
COLUMN = "LEGACY_SOURCE_ID"


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
        "SELECT COUNT(*) FROM user_tab_columns WHERE table_name = :t AND column_name = :c",
        t=TABLE, c=COLUMN,
    )
    if cur.fetchone()[0] > 0:
        print(f"{TABLE}.{COLUMN} ya existe (no-op).")
    else:
        print(f"Agregando columna {COLUMN} a {TABLE}...")
        cur.execute(f"ALTER TABLE {TABLE} ADD ({COLUMN} VARCHAR2(50 CHAR))")
        conn.commit()
        print(f"OK: columna {COLUMN} agregada.")

    cur.execute(
        "SELECT column_name, data_type, char_length FROM user_tab_columns "
        "WHERE table_name = :t AND column_name = :c",
        t=TABLE, c=COLUMN,
    )
    for name, dtype, length in cur.fetchall():
        print(f"  verificado: {TABLE}.{name} {dtype}({length})")

    conn.close()
    print("Migracion completada.")


if __name__ == "__main__":
    main()
