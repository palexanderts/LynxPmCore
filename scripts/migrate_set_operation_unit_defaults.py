"""
Migracion idempotente: agrega DEFAULT 'H' a LYNX_PM_AVISO_OPERATIONS.UNIT y
.WORKUNIT (NOT NULL, sin default hoy). El dominio Operation no modela estas
columnas (unidad de tiempo de mano de obra, no usada por el corte a legacy),
asi que un INSERT desde la API las omite y Oracle intenta escribir NULL,
violando el NOT NULL. El default solo aplica cuando la columna se omite del
INSERT — los formularios web que ya envian su propio valor no se ven afectados.

'H' (horas) se eligio porque ya es uno de los valores reales existentes en
ambas columnas (junto con 'MIN' y, en UNIT, un caso atipico 'EQ003').

Uso:  IBERO_DB_PASS debe estar en el entorno.
      python scripts/migrate_set_operation_unit_defaults.py
"""
import os
import oracledb

DSN = "193.122.157.114:1521/lemopdb1.sub03121625300.lemovcn.oraclevcn.com"
USER = "IBEROPHARMAPROD"
INSTANT_CLIENT_DIR = r"C:\oracle\instantclient_21_15"

TABLE = "LYNX_PM_AVISO_OPERATIONS"
COLUMNS = ["UNIT", "WORKUNIT"]
DEFAULT_VALUE = "H"


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

    for column in COLUMNS:
        cur.execute(
            "SELECT data_default FROM user_tab_columns WHERE table_name = :t AND column_name = :c",
            t=TABLE, c=column,
        )
        current_default = cur.fetchone()[0]
        if current_default is not None and current_default.strip().strip("'") == DEFAULT_VALUE:
            print(f"{TABLE}.{column} ya tiene DEFAULT '{DEFAULT_VALUE}' (no-op).")
            continue

        print(f"Agregando DEFAULT '{DEFAULT_VALUE}' a {TABLE}.{column}...")
        cur.execute(f"ALTER TABLE {TABLE} MODIFY ({column} DEFAULT '{DEFAULT_VALUE}')")
        conn.commit()
        print(f"OK: {TABLE}.{column} ahora tiene DEFAULT '{DEFAULT_VALUE}'.")

    for column in COLUMNS:
        cur.execute(
            "SELECT column_name, data_default FROM user_tab_columns WHERE table_name = :t AND column_name = :c",
            t=TABLE, c=column,
        )
        row = cur.fetchone()
        print(f"  verificado: {TABLE}.{row[0]} DEFAULT {row[1]}")

    conn.close()
    print("Migracion completada.")


if __name__ == "__main__":
    main()
