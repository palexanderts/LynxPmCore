import os
import oracledb

oracledb.init_oracle_client()
dsn = "193.122.157.114:1521/lemopdb1.sub03121625300.lemovcn.oraclevcn.com"
conn = oracledb.connect(user="IBEROPHARMAPROD", password=os.environ["IBERO_DB_PASS"], dsn=dsn)
cur = conn.cursor()

# Check columns of LYNX_PM_OPERATIONS
cur.execute("SELECT COLUMN_NAME, DATA_TYPE FROM USER_TAB_COLUMNS WHERE TABLE_NAME='LYNX_PM_OPERATIONS' ORDER BY COLUMN_ID")
cols = cur.fetchall()
print("LYNX_PM_OPERATIONS columns:")
for c in cols:
    print(f"  {c[0]} {c[1]}")

# Add NOTICE_ID if missing
col_names = [c[0] for c in cols]
if "NOTICE_ID" not in col_names:
    print("\nAdding NOTICE_ID column...")
    try:
        cur.execute("ALTER TABLE LYNX_PM_OPERATIONS ADD (NOTICE_ID RAW(16))")
        print("  NOTICE_ID added")
    except Exception as e:
        print(f"  Error: {e}")

# Create index if needed
try:
    cur.execute("CREATE INDEX IDX_LYNX_PM_OPS_NOTICE ON LYNX_PM_OPERATIONS(NOTICE_ID)")
    print("  Index created")
except Exception as e:
    if "ORA-01408" in str(e) or "already" in str(e).lower():
        print("  Index already exists")
    else:
        print(f"  Index error: {e}")

conn.commit()
cur.close()
conn.close()
print("DONE.")
