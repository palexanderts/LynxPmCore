"""
Migracion idempotente: corrige la FK de LYNXCORE_OPERATIONS.NOTICE_ID.

Se descubrio que FK_LYNXCORE_OPS_NOTICE apuntaba a LYNX_PM_NOTICES (tabla huerfana
de 3 filas, probablemente un nombre anterior de LYNXCORE_AVISO que quedo de un
rename incompleto) en vez de apuntar a LYNXCORE_AVISO, que es la tabla real usada
por el dominio Notice. Esto provocaba ORA-02291 al insertar cualquier Operation
para un Notice real. LYNXCORE_OPERATIONS estaba vacia (0 filas) al momento de
este fix, por lo que no hay riesgo de perdida de datos.

Uso:  IBERO_DB_PASS debe estar en el entorno.
      python scripts/migrate_fix_operations_notice_fk.py
"""
import os
import oracledb

DSN = "193.122.157.114:1521/lemopdb1.sub03121625300.lemovcn.oraclevcn.com"
USER = "IBEROPHARMAPROD"
INSTANT_CLIENT_DIR = r"C:\oracle\instantclient_21_15"

TABLE = "LYNXCORE_OPERATIONS"
FK_NAME = "FK_LYNXCORE_OPS_NOTICE"
CORRECT_REF_TABLE = "LYNXCORE_AVISO"


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
        """
        SELECT b.table_name
        FROM user_constraints con
        JOIN user_constraints b ON con.r_constraint_name = b.constraint_name
        WHERE con.constraint_name = :fk AND con.table_name = :t
        """,
        fk=FK_NAME, t=TABLE,
    )
    row = cur.fetchone()

    if row is None:
        print(f"No se encontro la FK {FK_NAME} en {TABLE} (no-op).")
    elif row[0] == CORRECT_REF_TABLE:
        print(f"{FK_NAME} ya apunta a {CORRECT_REF_TABLE} (no-op).")
    else:
        print(f"{FK_NAME} apunta a {row[0]}, corrigiendo a {CORRECT_REF_TABLE}...")
        cur.execute(f"ALTER TABLE {TABLE} DROP CONSTRAINT {FK_NAME}")
        cur.execute(
            f"ALTER TABLE {TABLE} ADD CONSTRAINT {FK_NAME} "
            f"FOREIGN KEY (NOTICE_ID) REFERENCES {CORRECT_REF_TABLE}(ID)"
        )
        conn.commit()
        print(f"OK: {FK_NAME} ahora apunta a {CORRECT_REF_TABLE}.")

    cur.execute(
        """
        SELECT b.table_name, c.column_name
        FROM user_constraints con
        JOIN user_constraints b ON con.r_constraint_name = b.constraint_name
        JOIN user_cons_columns c ON b.constraint_name = c.constraint_name
        WHERE con.constraint_name = :fk AND con.table_name = :t
        """,
        fk=FK_NAME, t=TABLE,
    )
    for ref_table, ref_column in cur.fetchall():
        print(f"  verificado: {FK_NAME} -> {ref_table}({ref_column})")

    conn.close()
    print("Migracion completada.")


if __name__ == "__main__":
    main()
