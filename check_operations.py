import os
import oracledb

oracledb.init_oracle_client()
dsn = "193.122.157.114:1521/lemopdb1.sub03121625300.lemovcn.oraclevcn.com"
conn = oracledb.connect(user="IBEROPHARMAPROD", password=os.environ["IBERO_DB_PASS"], dsn=dsn)
cur = conn.cursor()

cur.execute("SELECT COUNT(*) FROM LYNX_PM_OPERATIONS")
count = cur.fetchone()[0]
print(f"LYNX_PM_OPERATIONS rows: {count}")

# Check which other tables reference it
cur.execute("""
SELECT TABLE_NAME, CONSTRAINT_NAME, STATUS FROM USER_CONSTRAINTS
WHERE CONSTRAINT_TYPE = 'R'
  AND R_CONSTRAINT_NAME IN (
    SELECT CONSTRAINT_NAME FROM USER_CONSTRAINTS
    WHERE TABLE_NAME = 'LYNX_PM_OPERATIONS' AND CONSTRAINT_TYPE = 'P'
  )
""")
refs = cur.fetchall()
print(f"Foreign keys referencing it: {refs}")

cur.close()
conn.close()
