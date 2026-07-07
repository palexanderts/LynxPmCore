"""
Migracion idempotente: agrega FAILURE a LYNX_PM_AVISO_OPERATIONS.

Parte del corte de Notice/Operation hacia las tablas legacy: Operation.Failure
(falla registrada por el tecnico al completar una operacion) no tiene columna
nativa en LYNX_PM_AVISO_OPERATIONS, asi que se agrega de forma aditiva. No
afecta a los formularios web/APEX, que nunca leen ni escriben esta columna.

Uso:  IBERO_DB_PASS debe estar en el entorno.
      python scripts/migrate_add_operation_failure_column_legacy.py
"""
import os
import oracledb

DSN = "193.122.157.114:1521/lemopdb1.sub03121625300.lemovcn.oraclevcn.com"
USER = "IBEROPHARMAPROD"
INSTANT_CLIENT_DIR = r"C:\oracle\instantclient_21_15"

TABLE = "LYNX_PM_AVISO_OPERATIONS"
COLUMN = "FAILURE"


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
        cur.execute(f"ALTER TABLE {TABLE} ADD ({COLUMN} VARCHAR2(1000 CHAR))")
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
