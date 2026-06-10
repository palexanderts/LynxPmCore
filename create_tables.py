import os
import oracledb

oracledb.init_oracle_client()
dsn = "193.122.157.114:1521/lemopdb1.sub03121625300.lemovcn.oraclevcn.com"
conn = oracledb.connect(user="IBEROPHARMAPROD", password=os.environ["IBERO_DB_PASS"], dsn=dsn)
cur = conn.cursor()

ddl_statements = [
    """
    CREATE TABLE LYNX_PM_AVISO (
        ID            RAW(16)      DEFAULT SYS_GUID() NOT NULL,
        NOTICE_NUMBER VARCHAR2(50) NOT NULL,
        EQUIPMENT_CODE VARCHAR2(50) NOT NULL,
        DESCRIPTION   VARCHAR2(500),
        STATUS        NUMBER(3)    DEFAULT 1 NOT NULL,
        IS_APPROVED   NUMBER(1)    DEFAULT 0 NOT NULL,
        APEX_ID       VARCHAR2(100),
        IS_SYNCHRONIZED NUMBER(1)  DEFAULT 0 NOT NULL,
        SYNCHRONIZED_AT TIMESTAMP,
        CREATED_BY    VARCHAR2(100) NOT NULL,
        APPROVED_BY   VARCHAR2(100),
        REJECTION_REASON VARCHAR2(500),
        LOCATION      VARCHAR2(200),
        CUSTOMER      VARCHAR2(100),
        PRIORITY      NUMBER(2)    DEFAULT 1 NOT NULL,
        CREATED_AT    TIMESTAMP    DEFAULT SYSTIMESTAMP NOT NULL,
        UPDATED_AT    TIMESTAMP    DEFAULT SYSTIMESTAMP NOT NULL,
        IS_DELETED    NUMBER(1)    DEFAULT 0 NOT NULL,
        CONSTRAINT PK_LYNX_PM_AVISO PRIMARY KEY (ID),
        CONSTRAINT UQ_LYNX_PM_AVISO_NUMBER UNIQUE (NOTICE_NUMBER)
    )
    """,
    """
    CREATE TABLE LYNXCORE_OPERATIONS (
        ID                    RAW(16)      DEFAULT SYS_GUID() NOT NULL,
        NOTICE_ID             RAW(16)      NOT NULL,
        CODE                  VARCHAR2(50) NOT NULL,
        DESCRIPTION           VARCHAR2(500) NOT NULL,
        TYPE                  NUMBER(3)    DEFAULT 1 NOT NULL,
        STATUS                NUMBER(3)    DEFAULT 1 NOT NULL,
        POSITION              NUMBER(5)    DEFAULT 1 NOT NULL,
        STARTED_AT            TIMESTAMP,
        COMPLETED_AT          TIMESTAMP,
        NOTES                 VARCHAR2(1000),
        SCANNED_EQUIPMENT_CODE VARCHAR2(50),
        PHOTO_CONFIRMED       NUMBER(1)    DEFAULT 0 NOT NULL,
        ASSIGNED_TECHNICIAN   VARCHAR2(100),
        CREATED_AT            TIMESTAMP    DEFAULT SYSTIMESTAMP NOT NULL,
        UPDATED_AT            TIMESTAMP    DEFAULT SYSTIMESTAMP NOT NULL,
        IS_DELETED            NUMBER(1)    DEFAULT 0 NOT NULL,
        CONSTRAINT PK_LYNXCORE_OPERATIONS PRIMARY KEY (ID),
        CONSTRAINT FK_LYNXCORE_OPS_AVISO FOREIGN KEY (NOTICE_ID) REFERENCES LYNX_PM_AVISO(ID)
    )
    """,
    """
    CREATE TABLE LYNX_PM_EQUIPMENTS (
        ID          RAW(16)      DEFAULT SYS_GUID() NOT NULL,
        CODE        VARCHAR2(50) NOT NULL,
        DESCRIPTION VARCHAR2(500) NOT NULL,
        LOCATION    VARCHAR2(200),
        CUSTOMER    VARCHAR2(100),
        PARENT_CODE VARCHAR2(50),
        IS_ACTIVE   NUMBER(1)    DEFAULT 1 NOT NULL,
        LAST_SYNC_AT TIMESTAMP,
        CREATED_AT  TIMESTAMP    DEFAULT SYSTIMESTAMP NOT NULL,
        UPDATED_AT  TIMESTAMP    DEFAULT SYSTIMESTAMP NOT NULL,
        IS_DELETED  NUMBER(1)    DEFAULT 0 NOT NULL,
        CONSTRAINT PK_LYNX_PM_EQUIP PRIMARY KEY (ID),
        CONSTRAINT UQ_LYNX_PM_EQUIP_CODE UNIQUE (CODE)
    )
    """,
    """
    CREATE TABLE LYNX_PM_CUSTOMERS (
        ID         RAW(16)      DEFAULT SYS_GUID() NOT NULL,
        CODE       VARCHAR2(50) NOT NULL,
        NAME       VARCHAR2(200) NOT NULL,
        ADDRESS    VARCHAR2(500),
        PHONE      VARCHAR2(50),
        IS_ACTIVE  NUMBER(1)    DEFAULT 1 NOT NULL,
        CREATED_AT TIMESTAMP    DEFAULT SYSTIMESTAMP NOT NULL,
        UPDATED_AT TIMESTAMP    DEFAULT SYSTIMESTAMP NOT NULL,
        IS_DELETED NUMBER(1)    DEFAULT 0 NOT NULL,
        CONSTRAINT PK_LYNX_PM_CUSTOMERS PRIMARY KEY (ID),
        CONSTRAINT UQ_LYNX_PM_CUST_CODE UNIQUE (CODE)
    )
    """,
    """
    CREATE TABLE LYNX_PM_OUTBOX (
        ID               RAW(16)       DEFAULT SYS_GUID() NOT NULL,
        TYPE             VARCHAR2(500) NOT NULL,
        CONTENT          CLOB          NOT NULL,
        OCCURRED_ON_UTC  TIMESTAMP     DEFAULT SYSTIMESTAMP NOT NULL,
        PROCESSED_ON_UTC TIMESTAMP,
        ERROR            VARCHAR2(2000),
        RETRY_COUNT      NUMBER(3)     DEFAULT 0 NOT NULL,
        CONSTRAINT PK_LYNX_PM_OUTBOX PRIMARY KEY (ID)
    )
    """,
    "CREATE INDEX IDX_LYNX_PM_AVISO_STATUS ON LYNX_PM_AVISO(STATUS)",
    "CREATE INDEX IDX_LYNX_PM_AVISO_EQ ON LYNX_PM_AVISO(EQUIPMENT_CODE)",
    "CREATE INDEX IDX_LYNXCORE_OPS_NOTICE ON LYNXCORE_OPERATIONS(NOTICE_ID)",
    "CREATE INDEX IDX_LYNX_PM_EQUIP_CUST ON LYNX_PM_EQUIPMENTS(CUSTOMER)",
    "CREATE INDEX IDX_LYNX_PM_OUTBOX_PROC ON LYNX_PM_OUTBOX(PROCESSED_ON_UTC)",
]

# Seed test equipment
seed = [
    """
    INSERT INTO LYNX_PM_EQUIPMENTS (ID, CODE, DESCRIPTION, LOCATION, IS_ACTIVE)
    VALUES (SYS_GUID(), 'EQ-001', 'Compresor Principal', 'Planta 1', 1)
    """,
    """
    INSERT INTO LYNX_PM_EQUIPMENTS (ID, CODE, DESCRIPTION, LOCATION, IS_ACTIVE)
    VALUES (SYS_GUID(), 'EQ-002', 'Bomba Hidraulica', 'Planta 2', 1)
    """,
    """
    INSERT INTO LYNX_PM_CUSTOMERS (ID, CODE, NAME, ADDRESS)
    VALUES (SYS_GUID(), 'CUST-001', 'Cliente Demo S.A.', 'Calle Principal 123')
    """,
]

print("Creating tables...")
for stmt in ddl_statements:
    name = stmt.strip().split('\n')[0][:60].strip()
    try:
        cur.execute(stmt)
        print(f"  OK: {name}")
    except oracledb.DatabaseError as e:
        if "ORA-00955" in str(e):  # already exists
            print(f"  SKIP (exists): {name}")
        else:
            print(f"  ERROR: {name} -> {e}")

print("\nSeeding test data...")
for stmt in seed:
    name = stmt.strip().split('\n')[0][:60].strip()
    try:
        cur.execute(stmt)
        print(f"  OK: {name}")
    except oracledb.DatabaseError as e:
        print(f"  ERROR: {name} -> {e}")

conn.commit()
cur.close()
conn.close()
print("\nDONE.")
