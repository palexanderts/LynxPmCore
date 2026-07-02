import os
import oracledb

DSN = "193.122.157.114:1521/lemopdb1.sub03121625300.lemovcn.oraclevcn.com"
USER = "IBEROPHARMAPROD"
INSTANT_CLIENT_DIR = r"C:\oracle\instantclient_21_15"

password = os.environ["IBERO_DB_PASS"]
if os.path.isdir(INSTANT_CLIENT_DIR):
    oracledb.init_oracle_client(lib_dir=INSTANT_CLIENT_DIR)
else:
    oracledb.init_oracle_client()

conn = oracledb.connect(user=USER, password=password, dsn=DSN)
cur = conn.cursor()

TABLES = [
    "LYNX_PM_COMPONENTS_UNITS",
    "LYNX_PM_COMPONENT_CENTER_STORE",
    "LYNX_PM_COMPONENT_NOTIFICATIONS",
    "STAGING_PM_COMPONENTS",
    "STAGING_PM_COMPONENTS_UNITS",
    "LYNX_PM_COMPONENT_CONSUMPTIONS",
]

for table in TABLES:
    print(f"\n{'='*70}\n{table}\n{'='*70}")
    cur.execute("""
        SELECT column_name, data_type, data_length, nullable
        FROM user_tab_columns WHERE table_name = :t
        ORDER BY column_id
    """, t=table)
    cols = cur.fetchall()
    if not cols:
        print("  (sin columnas / no existe)")
        continue
    for name, dtype, length, nullable in cols:
        print(f"  {name}: {dtype}({length}) nullable={nullable}")

    cur.execute(f"SELECT COUNT(*) FROM {table}")
    count = cur.fetchone()[0]
    print(f"  -- filas: {count}")

    if count > 0:
        col_names = [c[0] for c in cols]
        cur.execute(f"SELECT * FROM {table} FETCH FIRST 3 ROWS ONLY")
        for row in cur.fetchall():
            print(" ", dict(zip(col_names, row)))

conn.close()
