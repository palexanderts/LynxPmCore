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

TABLES = ["LYNX_PM_COMPONENT_NOTIFICATIONS", "LYNX_PM_COMPONENT_CENTER_STORE", "LYNX_PM_COMPONENTS_UNITS", "LYNX_PM_COMPONENTS"]

for t in TABLES:
    print(f"\n=== {t}: constraints ===")
    cur.execute("""
        SELECT constraint_name, constraint_type
        FROM user_constraints WHERE table_name = :t
    """, t=t)
    for name, ctype in cur.fetchall():
        print(f"  {name}: {ctype}")

    print(f"=== {t}: identity columns ===")
    try:
        cur.execute("""
            SELECT column_name, identity_column, generation_type
            FROM user_tab_identity_cols WHERE table_name = :t
        """, t=t)
        rows = cur.fetchall()
        print("  " + str(rows) if rows else "  (ninguna)")
    except Exception as e:
        print(f"  error: {e}")

    print(f"=== {t}: triggers ===")
    cur.execute("SELECT trigger_name, triggering_event FROM user_triggers WHERE table_name = :t", t=t)
    trigs = cur.fetchall()
    print("  " + str(trigs) if trigs else "  (ninguno)")

print("\n=== Secuencias con 'COMPONENT' o 'NOTIF' en el nombre ===")
cur.execute("SELECT sequence_name, last_number FROM user_sequences WHERE sequence_name LIKE '%COMPONENT%' OR sequence_name LIKE '%NOTIF%'")
print(cur.fetchall())

conn.close()
