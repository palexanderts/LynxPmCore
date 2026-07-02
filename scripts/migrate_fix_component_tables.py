"""
Migracion idempotente de componentes — 3 acciones:

1. CREA LYNX_PM_COMPONENT_RECEIPTS (bug real: el handler de "recibir componente" ya
   estaba codificado y en uso, pero esta tabla NUNCA se habia creado en produccion —
   cada intento de recibir un componente fallaba con ORA-00942 tabla no existe).
2. AGREGA PK a LYNX_PM_COMPONENT_NOTIFICATIONS.ID (tabla legacy vacia sin constraints;
   se usa NOTIFY_OPERATION_SEQ para generar el ID desde el backend).
3. ELIMINA LYNX_PM_COMPONENT_CONSUMPTIONS (tabla vacia creada por un diseño descartado
   en esta misma sesion; se reemplazo por LYNX_PM_COMPONENT_NOTIFICATIONS, el esquema real).

Re-ejecutable sin efecto. Modo thick (el servidor exige Native Network Encryption).

Uso:  IBERO_DB_PASS debe estar en el entorno.
      python scripts/migrate_fix_component_tables.py
"""
import os
import oracledb

DSN = "193.122.157.114:1521/lemopdb1.sub03121625300.lemovcn.oraclevcn.com"
USER = "IBEROPHARMAPROD"
INSTANT_CLIENT_DIR = r"C:\oracle\instantclient_21_15"


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

    cur.execute("""
        SELECT table_name FROM user_tables
        WHERE table_name IN ('LYNX_PM_COMPONENT_RECEIPTS','LYNX_PM_COMPONENT_NOTIFICATIONS','LYNX_PM_COMPONENT_CONSUMPTIONS')
    """)
    existing = {row[0] for row in cur.fetchall()}

    # 1. Crear LYNX_PM_COMPONENT_RECEIPTS (bug fix real)
    if "LYNX_PM_COMPONENT_RECEIPTS" not in existing:
        print("Creando LYNX_PM_COMPONENT_RECEIPTS...")
        cur.execute("""
            CREATE TABLE LYNX_PM_COMPONENT_RECEIPTS (
                ID            RAW(16)       DEFAULT SYS_GUID() NOT NULL,
                RECEIPT_ID    VARCHAR2(50)  NOT NULL,
                COMPONENT_ID  VARCHAR2(200) NOT NULL,
                QUANTITY      NUMBER(10)    NOT NULL,
                OBSERVATIONS  VARCHAR2(1000),
                RECEIVED_BY   VARCHAR2(100) NOT NULL,
                RECEIVED_AT   TIMESTAMP     NOT NULL,
                CREATED_AT    TIMESTAMP     DEFAULT SYSTIMESTAMP NOT NULL,
                UPDATED_AT    TIMESTAMP     DEFAULT SYSTIMESTAMP NOT NULL,
                IS_DELETED    NUMBER(1)     DEFAULT 0 NOT NULL,
                CONSTRAINT PK_LYNX_PM_COMPONENT_RECEIPTS PRIMARY KEY (ID),
                CONSTRAINT UQ_LYNX_PM_COMP_RECEIPTS_RID UNIQUE (RECEIPT_ID)
            )
        """)
        cur.execute("CREATE INDEX IDX_LYNX_PM_COMP_RECEIPTS_CID ON LYNX_PM_COMPONENT_RECEIPTS(COMPONENT_ID)")
        cur.execute("""
            CREATE INDEX IDX_LYNX_PM_COMP_RECEIPTS_DUP
            ON LYNX_PM_COMPONENT_RECEIPTS(COMPONENT_ID, RECEIVED_AT, RECEIVED_BY)
        """)
        print("OK: LYNX_PM_COMPONENT_RECEIPTS creada (bug fix: tabla nunca existia).")
    else:
        print("LYNX_PM_COMPONENT_RECEIPTS ya existe (no-op).")

    # 2. Agregar PK a LYNX_PM_COMPONENT_NOTIFICATIONS
    cur.execute("""
        SELECT COUNT(*) FROM user_constraints
        WHERE table_name = 'LYNX_PM_COMPONENT_NOTIFICATIONS' AND constraint_type = 'P'
    """)
    if cur.fetchone()[0] == 0:
        print("Agregando PK a LYNX_PM_COMPONENT_NOTIFICATIONS...")
        cur.execute("""
            ALTER TABLE LYNX_PM_COMPONENT_NOTIFICATIONS
            ADD CONSTRAINT PK_LYNX_PM_COMPONENT_NOTIF PRIMARY KEY (ID)
        """)
        print("OK: PK agregada.")
    else:
        print("LYNX_PM_COMPONENT_NOTIFICATIONS ya tiene PK (no-op).")

    # 3. Eliminar tabla huerfana del diseño descartado
    if "LYNX_PM_COMPONENT_CONSUMPTIONS" in existing:
        cur.execute("SELECT COUNT(*) FROM LYNX_PM_COMPONENT_CONSUMPTIONS")
        count = cur.fetchone()[0]
        if count == 0:
            print("Eliminando LYNX_PM_COMPONENT_CONSUMPTIONS (vacia, diseño descartado)...")
            cur.execute("DROP TABLE LYNX_PM_COMPONENT_CONSUMPTIONS")
            print("OK: eliminada.")
        else:
            print(f"ADVERTENCIA: LYNX_PM_COMPONENT_CONSUMPTIONS tiene {count} filas, NO se elimina.")
    else:
        print("LYNX_PM_COMPONENT_CONSUMPTIONS ya no existe (no-op).")

    conn.commit()
    conn.close()
    print("Migracion completada.")


if __name__ == "__main__":
    main()
