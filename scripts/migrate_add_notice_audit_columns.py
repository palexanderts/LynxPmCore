"""
Migracion idempotente: agrega columnas de auditoria a LYNX_PM_AVISO y
LYNX_PM_AVISO_OPERATIONS que el dominio necesita para queries traducibles
por EF Core (IS_DELETED se usa en casi todos los predicados de AnalyticsRepository
y NoticeRepository; no puede quedar sin mapear a una columna real).

No afecta a los formularios web/APEX: son columnas nuevas, con DEFAULT 0/NULL,
que nadie mas lee ni escribe hoy.

Uso:  IBERO_DB_PASS debe estar en el entorno.
      python scripts/migrate_add_notice_audit_columns.py
"""
import os
import oracledb

DSN = "193.122.157.114:1521/lemopdb1.sub03121625300.lemovcn.oraclevcn.com"
USER = "IBEROPHARMAPROD"
INSTANT_CLIENT_DIR = r"C:\oracle\instantclient_21_15"

COLUMNS = [
    ("LYNX_PM_AVISO", "IS_DELETED", "NUMBER(1) DEFAULT 0 NOT NULL"),
    ("LYNX_PM_AVISO", "SYNCHRONIZED_AT", "TIMESTAMP(6)"),
    ("LYNX_PM_AVISO", "IS_APPROVED", "NUMBER(1) DEFAULT 0 NOT NULL"),
    ("LYNX_PM_AVISO_OPERATIONS", "IS_DELETED", "NUMBER(1) DEFAULT 0 NOT NULL"),
]


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

    for table, column, ddl in COLUMNS:
        cur.execute(
            "SELECT COUNT(*) FROM user_tab_columns WHERE table_name = :t AND column_name = :c",
            t=table, c=column,
        )
        if cur.fetchone()[0] > 0:
            print(f"{table}.{column} ya existe (no-op).")
            continue

        print(f"Agregando columna {column} a {table}...")
        cur.execute(f"ALTER TABLE {table} ADD ({column} {ddl})")
        conn.commit()
        print(f"OK: columna {column} agregada.")

    for table, column, _ in COLUMNS:
        cur.execute(
            "SELECT column_name, data_type FROM user_tab_columns WHERE table_name = :t AND column_name = :c",
            t=table, c=column,
        )
        row = cur.fetchone()
        print(f"  verificado: {table}.{row[0]} {row[1]}")

    conn.close()
    print("Migracion completada.")


if __name__ == "__main__":
    main()
