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

print("=== Columnas de LYNX_PM_COMPONENTS ===")
cur.execute("""
    SELECT column_name, data_type, data_length, nullable
    FROM user_tab_columns WHERE table_name = 'LYNX_PM_COMPONENTS'
    ORDER BY column_id
""")
cols = cur.fetchall()
for name, dtype, length, nullable in cols:
    print(f"  {name}: {dtype}({length}) nullable={nullable}")

print("\n=== Muestra de datos (primeras 5 filas) ===")
col_names = [c[0] for c in cols]
cur.execute(f"SELECT * FROM LYNX_PM_COMPONENTS FETCH FIRST 5 ROWS ONLY")
for row in cur.fetchall():
    print(dict(zip(col_names, row)))

print("\n=== Total de filas ===")
cur.execute("SELECT COUNT(*) FROM LYNX_PM_COMPONENTS")
print(cur.fetchone()[0])

conn.close()
