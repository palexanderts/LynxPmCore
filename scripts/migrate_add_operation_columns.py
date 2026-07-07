"""
Migracion idempotente: agrega columnas que Operation necesita y que
LYNX_PM_AVISO_OPERATIONS no tiene de forma nativa (scan de equipo, confirmacion
por foto, y UPDATED_AT que no existe en esta tabla legacy). Aditivo, no afecta
a los formularios web/APEX.

Uso:  IBERO_DB_PASS debe estar en el entorno.
      python scripts/migrate_add_operation_columns.py
"""
import os
import oracledb

DSN = "193.122.157.114:1521/lemopdb1.sub03121625300.lemovcn.oraclevcn.com"
USER = "IBEROPHARMAPROD"
INSTANT_CLIENT_DIR = r"C:\oracle\instantclient_21_15"

TABLE = "LYNX_PM_AVISO_OPERATIONS"
COLUMNS = [
    ("SCANNED_EQUIPMENT_CODE", "VARCHAR2(50 CHAR)"),
    ("PHOTO_CONFIRMED", "NUMBER(1) DEFAULT 0 NOT NULL"),
    ("UPDATED_AT", "TIMESTAMP(6)"),
]


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

    for column, ddl in COLUMNS:
        cur.execute(
            "SELECT COUNT(*) FROM user_tab_columns WHERE table_name = :t AND column_name = :c",
            t=TABLE, c=column,
        )
        if cur.fetchone()[0] > 0:
            print(f"{TABLE}.{column} ya existe (no-op).")
            continue

        print(f"Agregando columna {column} a {TABLE}...")
        cur.execute(f"ALTER TABLE {TABLE} ADD ({column} {ddl})")
        conn.commit()
        print(f"OK: columna {column} agregada.")

    for column, _ in COLUMNS:
        cur.execute(
            "SELECT column_name, data_type FROM user_tab_columns WHERE table_name = :t AND column_name = :c",
            t=TABLE, c=column,
        )
        row = cur.fetchone()
        print(f"  verificado: {TABLE}.{row[0]} {row[1]}")

    conn.close()
    print("Migracion completada.")


if __name__ == "__main__":
    main()
