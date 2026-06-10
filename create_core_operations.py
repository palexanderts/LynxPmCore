import os
import oracledb

oracledb.init_oracle_client()
dsn = "193.122.157.114:1521/lemopdb1.sub03121625300.lemovcn.oraclevcn.com"
conn = oracledb.connect(user="IBEROPHARMAPROD", password=os.environ["IBERO_DB_PASS"], dsn=dsn)
cur = conn.cursor()

stmts = [
    """
    CREATE TABLE LYNXCORE_OPERATIONS (
        ID                    RAW(16)       DEFAULT SYS_GUID() NOT NULL,
        NOTICE_ID             RAW(16)       NOT NULL,
        CODE                  VARCHAR2(50)  NOT NULL,
        DESCRIPTION           VARCHAR2(500) NOT NULL,
        TYPE                  NUMBER(3)     DEFAULT 1 NOT NULL,
        STATUS                NUMBER(3)     DEFAULT 1 NOT NULL,
        POSITION              NUMBER(5)     DEFAULT 1 NOT NULL,
        STARTED_AT            TIMESTAMP,
        COMPLETED_AT          TIMESTAMP,
        NOTES                 VARCHAR2(1000),
        SCANNED_EQUIPMENT_CODE VARCHAR2(50),
        PHOTO_CONFIRMED       NUMBER(1)     DEFAULT 0 NOT NULL,
        ASSIGNED_TECHNICIAN   VARCHAR2(100),
        CREATED_AT            TIMESTAMP     DEFAULT SYSTIMESTAMP NOT NULL,
        UPDATED_AT            TIMESTAMP     DEFAULT SYSTIMESTAMP NOT NULL,
        IS_DELETED            NUMBER(1)     DEFAULT 0 NOT NULL,
        CONSTRAINT PK_LYNXCORE_OPERATIONS PRIMARY KEY (ID),
        CONSTRAINT FK_LYNXCORE_OPS_NOTICE FOREIGN KEY (NOTICE_ID) REFERENCES LYNX_PM_AVISO(ID)
    )
    """,
    "CREATE INDEX IDX_LYNXCORE_OPS_NOTICE ON LYNXCORE_OPERATIONS(NOTICE_ID)",
]

for stmt in stmts:
    name = stmt.strip().split('\n')[0][:60].strip()
    try:
        cur.execute(stmt)
        print(f"OK: {name}")
    except oracledb.DatabaseError as e:
        if "ORA-00955" in str(e) or "ORA-01408" in str(e):
            print(f"SKIP (exists): {name}")
        else:
            print(f"ERROR: {name} -> {e}")

conn.commit()
cur.close()
conn.close()
print("DONE.")
