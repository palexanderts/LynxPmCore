import oracledb
import os

oracledb.init_oracle_client(lib_dir=r"C:\oracle\instantclient_23_0")

conn = oracledb.connect(
    user="IBEROPHARMAPROD",
    password=os.environ["IBERO_DB_PASS"],
    dsn="193.122.157.114:1521/lemopdb1.sub03121625300.lemovcn.oraclevcn.com"
)

cur = conn.cursor()

# ── 1. LYNX_PM_ERP_SYNC_CONFIG ────────────────────────────────────────────────
cur.execute("""
BEGIN
  EXECUTE IMMEDIATE '
    CREATE TABLE LYNX_PM_ERP_SYNC_CONFIG (
        ID                RAW(16)        DEFAULT SYS_GUID() PRIMARY KEY,
        CLIENT_CODE       VARCHAR2(100)  NOT NULL,
        PROCESS_CODE      NUMBER(2)      NOT NULL,
        PROCESS_NAME      VARCHAR2(200)  NOT NULL,
        IS_ENABLED        NUMBER(1)      DEFAULT 0  NOT NULL,
        ERP_URL           VARCHAR2(500),
        AUTH_HEADER       VARCHAR2(500),
        RETRY_MAX         NUMBER(3)      DEFAULT 3  NOT NULL,
        RETRY_DELAY_SEC   NUMBER(6)      DEFAULT 60 NOT NULL,
        PRIORITY          NUMBER(3)      DEFAULT 10 NOT NULL,
        CREATED_AT        TIMESTAMP      DEFAULT SYSTIMESTAMP NOT NULL,
        UPDATED_AT        TIMESTAMP      DEFAULT SYSTIMESTAMP NOT NULL,
        CONSTRAINT UQ_ERP_SYNC_CONFIG_CLIENT_PROC UNIQUE (CLIENT_CODE, PROCESS_CODE)
    )';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -955 THEN RAISE; END IF;
END;
""")
print("LYNX_PM_ERP_SYNC_CONFIG: OK")

# ── 2. LYNX_PM_ERP_SYNC_OUTBOX ────────────────────────────────────────────────
cur.execute("""
BEGIN
  EXECUTE IMMEDIATE '
    CREATE TABLE LYNX_PM_ERP_SYNC_OUTBOX (
        ID              RAW(16)       DEFAULT SYS_GUID() PRIMARY KEY,
        CLIENT_CODE     VARCHAR2(100) NOT NULL,
        PROCESS_CODE    NUMBER(2)     NOT NULL,
        ENTITY_ID       VARCHAR2(100) NOT NULL,
        PAYLOAD         CLOB          NOT NULL,
        STATUS          NUMBER(1)     DEFAULT 0 NOT NULL,
        ATTEMPT_COUNT   NUMBER(3)     DEFAULT 0 NOT NULL,
        LAST_ERROR      CLOB,
        SCHEDULED_AT    TIMESTAMP     DEFAULT SYSTIMESTAMP NOT NULL,
        PROCESSED_AT    TIMESTAMP,
        CREATED_AT      TIMESTAMP     DEFAULT SYSTIMESTAMP NOT NULL,
        UPDATED_AT      TIMESTAMP     DEFAULT SYSTIMESTAMP NOT NULL
    )';
EXCEPTION WHEN OTHERS THEN
  IF SQLCODE != -955 THEN RAISE; END IF;
END;
""")
print("LYNX_PM_ERP_SYNC_OUTBOX: OK")

# ── 3. Índices ─────────────────────────────────────────────────────────────────
indexes = [
    ("IX_ERP_OUTBOX_STATUS_SCHED",
     "CREATE INDEX IX_ERP_OUTBOX_STATUS_SCHED ON LYNX_PM_ERP_SYNC_OUTBOX(STATUS, SCHEDULED_AT)"),
    ("IX_ERP_OUTBOX_ENTITY",
     "CREATE INDEX IX_ERP_OUTBOX_ENTITY ON LYNX_PM_ERP_SYNC_OUTBOX(ENTITY_ID)"),
]
for name, ddl in indexes:
    try:
        cur.execute(ddl)
        print(f"Index {name}: OK")
    except oracledb.DatabaseError as e:
        if "955" in str(e) or "ORA-01408" in str(e):
            print(f"Index {name}: already exists")
        else:
            raise

# ── 4. Datos iniciales (IS_ENABLED=0 por defecto) ──────────────────────────────
processes = [
    (1, "SERVICE_ORDER",          "Crear Orden de Servicio"),
    (2, "HOUR_BILLING",           "Facturación de Horas"),
    (3, "INVENTORY_CONSUMPTION",  "Consumo de Inventario"),
    (4, "INVENTORY_MOVEMENT",     "Movimiento de Inventario"),
]

client_code = "iberofama"

for code, name_code, name in processes:
    cur.execute("""
        MERGE INTO LYNX_PM_ERP_SYNC_CONFIG t
        USING (SELECT :client AS CLIENT_CODE, :proc AS PROCESS_CODE FROM DUAL) s
        ON (t.CLIENT_CODE = s.CLIENT_CODE AND t.PROCESS_CODE = s.PROCESS_CODE)
        WHEN NOT MATCHED THEN
            INSERT (ID, CLIENT_CODE, PROCESS_CODE, PROCESS_NAME, IS_ENABLED, PRIORITY)
            VALUES (SYS_GUID(), :client, :proc, :pname, 0, :prio)
    """, client=client_code, proc=code, pname=name, prio=code * 10)
    print(f"  Config {name_code}: seeded")

conn.commit()
print("\nTablas ERP Sync creadas y datos iniciales insertados correctamente.")
cur.close()
conn.close()
