"""
Migracion idempotente: agrega el campo FAILURE a LYNXCORE_OPERATIONS y crea la
tabla LYNXCORE_OPERATION_CAUSES (una operacion puede acumular varias causas).

Parte de la mejora de proceso Sintoma -> Falla -> Causa: la Falla se registra
al completar la operacion (tecnico), y la(s) Causa(s) tambien en ese momento.

- Solo agrega la columna FAILURE si falta (aditivo, nullable, no destructivo).
- Solo crea LYNXCORE_OPERATION_CAUSES si no existe.
- Modo thick (el servidor exige Native Network Encryption).

Uso:  IBERO_DB_PASS debe estar en el entorno.
      python scripts/migrate_add_operation_failure_causes.py
"""
import os
import oracledb

DSN = "193.122.157.114:1521/lemopdb1.sub03121625300.lemovcn.oraclevcn.com"
USER = "IBEROPHARMAPROD"
INSTANT_CLIENT_DIR = r"C:\oracle\instantclient_21_15"

OPERATIONS_TABLE = "LYNXCORE_OPERATIONS"
CAUSES_TABLE = "LYNXCORE_OPERATION_CAUSES"


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

    # 1. Columna FAILURE en LYNXCORE_OPERATIONS
    cur.execute(
        "SELECT COUNT(*) FROM user_tab_columns WHERE table_name = :t AND column_name = 'FAILURE'",
        t=OPERATIONS_TABLE,
    )
    if cur.fetchone()[0] > 0:
        print(f"{OPERATIONS_TABLE}.FAILURE ya existe (no-op).")
    else:
        print(f"Agregando columna FAILURE a {OPERATIONS_TABLE}...")
        cur.execute(f"ALTER TABLE {OPERATIONS_TABLE} ADD (FAILURE VARCHAR2(1000 CHAR))")
        print("OK: columna FAILURE agregada.")

    # 2. Tabla LYNXCORE_OPERATION_CAUSES
    cur.execute(
        "SELECT COUNT(*) FROM user_tables WHERE table_name = :t", t=CAUSES_TABLE
    )
    if cur.fetchone()[0] > 0:
        print(f"{CAUSES_TABLE} ya existe (no-op).")
    else:
        print(f"Creando {CAUSES_TABLE}...")
        cur.execute(f"""
            CREATE TABLE {CAUSES_TABLE} (
                ID           RAW(16)       DEFAULT SYS_GUID() NOT NULL,
                OPERATION_ID RAW(16)       NOT NULL,
                CODE         VARCHAR2(20)  NOT NULL,
                TEXT         VARCHAR2(500),
                CREATED_AT   TIMESTAMP     DEFAULT SYSTIMESTAMP NOT NULL,
                UPDATED_AT   TIMESTAMP     DEFAULT SYSTIMESTAMP NOT NULL,
                IS_DELETED   NUMBER(1)     DEFAULT 0 NOT NULL,
                CONSTRAINT PK_LYNXCORE_OP_CAUSES PRIMARY KEY (ID),
                CONSTRAINT FK_LYNXCORE_OP_CAUSES_OP FOREIGN KEY (OPERATION_ID)
                    REFERENCES {OPERATIONS_TABLE}(ID)
            )
        """)
        cur.execute(f"CREATE INDEX IDX_LYNXCORE_OP_CAUSES_OP ON {CAUSES_TABLE}(OPERATION_ID)")
        print(f"OK: {CAUSES_TABLE} creada, con indice por OPERATION_ID.")

    conn.commit()

    # 3. Verificacion
    cur.execute(
        "SELECT column_name, data_type, char_length FROM user_tab_columns "
        "WHERE table_name = :t AND column_name = 'FAILURE'",
        t=OPERATIONS_TABLE,
    )
    for name, dtype, length in cur.fetchall():
        print(f"  verificado: {OPERATIONS_TABLE}.{name} {dtype}({length})")

    cur.execute("SELECT table_name FROM user_tables WHERE table_name = :t", t=CAUSES_TABLE)
    if cur.fetchone():
        print(f"  verificado: tabla {CAUSES_TABLE} presente")

    conn.close()
    print("Migracion completada.")


if __name__ == "__main__":
    main()
