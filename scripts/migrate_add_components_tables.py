"""
Migracion idempotente: crea las tablas de inventario real de componentes.

- LYNX_PM_COMPONENTS: catalogo real con stock (reemplaza el catalogo mock de IndexController).
- LYNX_PM_COMPONENT_CONSUMPTIONS: bitacora de consumo (paralela a LYNX_PM_COMPONENT_RECEIPTS).
- Siembra los 5 componentes que antes eran mock (CatalogSeeds.Components), con su stock inicial,
  para no perder continuidad con los codigos ya usados en pruebas.

Re-ejecutable sin efecto (solo crea lo que falte; no reinserta seeds si la tabla ya tiene datos).
Modo thick (el servidor exige Native Network Encryption).

Uso:  IBERO_DB_PASS debe estar en el entorno.
      python scripts/migrate_add_components_tables.py
"""
import os
import oracledb

DSN = "193.122.157.114:1521/lemopdb1.sub03121625300.lemovcn.oraclevcn.com"
USER = "IBEROPHARMAPROD"
INSTANT_CLIENT_DIR = r"C:\oracle\instantclient_21_15"

SEED_COMPONENTS = [
    ("COMP-001", "Rodamiento 6205-2RS", "UN", 10),
    ("COMP-002", "Filtro Aceite HF6553", "UN", 25),
    ("COMP-003", "Correa V-Belt B-75", "UN", 8),
    ("COMP-004", "Sello Mecanico 30mm", "UN", 15),
    ("COMP-005", "Aceite Hidraulico ISO46 5L", "LT", 50),
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

    cur.execute("SELECT table_name FROM user_tables WHERE table_name IN ('LYNX_PM_COMPONENTS','LYNX_PM_COMPONENT_CONSUMPTIONS')")
    existing = {row[0] for row in cur.fetchall()}

    if "LYNX_PM_COMPONENTS" not in existing:
        print("Creando LYNX_PM_COMPONENTS...")
        cur.execute("""
            CREATE TABLE LYNX_PM_COMPONENTS (
                ID               RAW(16)       DEFAULT SYS_GUID() NOT NULL,
                CODE             VARCHAR2(200) NOT NULL,
                DESCRIPTION      VARCHAR2(500) NOT NULL,
                UNIT_OF_MEASURE  VARCHAR2(20),
                STOCK_QUANTITY   NUMBER(10)    DEFAULT 0 NOT NULL,
                IS_ACTIVE        NUMBER(1)     DEFAULT 1 NOT NULL,
                CREATED_AT       TIMESTAMP     DEFAULT SYSTIMESTAMP NOT NULL,
                UPDATED_AT       TIMESTAMP     DEFAULT SYSTIMESTAMP NOT NULL,
                IS_DELETED       NUMBER(1)     DEFAULT 0 NOT NULL,
                CONSTRAINT PK_LYNX_PM_COMPONENTS PRIMARY KEY (ID),
                CONSTRAINT UQ_LYNX_PM_COMPONENTS_CODE UNIQUE (CODE)
            )
        """)
        print("OK: LYNX_PM_COMPONENTS creada.")
    else:
        print("LYNX_PM_COMPONENTS ya existe (no-op).")

    if "LYNX_PM_COMPONENT_CONSUMPTIONS" not in existing:
        print("Creando LYNX_PM_COMPONENT_CONSUMPTIONS...")
        cur.execute("""
            CREATE TABLE LYNX_PM_COMPONENT_CONSUMPTIONS (
                ID              RAW(16)        DEFAULT SYS_GUID() NOT NULL,
                CONSUMPTION_ID  VARCHAR2(50)   NOT NULL,
                COMPONENT_ID    VARCHAR2(200)  NOT NULL,
                QUANTITY        NUMBER(10)     NOT NULL,
                OBSERVATIONS    VARCHAR2(1000),
                CONSUMED_BY     VARCHAR2(100)  NOT NULL,
                CONSUMED_AT     TIMESTAMP      NOT NULL,
                CREATED_AT      TIMESTAMP      DEFAULT SYSTIMESTAMP NOT NULL,
                UPDATED_AT      TIMESTAMP      DEFAULT SYSTIMESTAMP NOT NULL,
                IS_DELETED      NUMBER(1)      DEFAULT 0 NOT NULL,
                CONSTRAINT PK_LYNX_PM_COMP_CONSUMPTIONS PRIMARY KEY (ID),
                CONSTRAINT UQ_LYNX_PM_COMP_CONSUMPTIONS UNIQUE (CONSUMPTION_ID)
            )
        """)
        print("OK: LYNX_PM_COMPONENT_CONSUMPTIONS creada.")
    else:
        print("LYNX_PM_COMPONENT_CONSUMPTIONS ya existe (no-op).")

    cur.execute("SELECT COUNT(*) FROM LYNX_PM_COMPONENTS")
    count = cur.fetchone()[0]
    if count == 0:
        print(f"Sembrando {len(SEED_COMPONENTS)} componentes iniciales...")
        for code, description, uom, stock in SEED_COMPONENTS:
            cur.execute(
                """
                INSERT INTO LYNX_PM_COMPONENTS (ID, CODE, DESCRIPTION, UNIT_OF_MEASURE, STOCK_QUANTITY)
                VALUES (SYS_GUID(), :code, :description, :uom, :stock)
                """,
                code=code, description=description, uom=uom, stock=stock,
            )
        print("OK: seed insertado.")
    else:
        print(f"LYNX_PM_COMPONENTS ya tiene {count} registros, no se siembra (no-op).")

    conn.commit()

    cur.execute("SELECT CODE, DESCRIPTION, STOCK_QUANTITY FROM LYNX_PM_COMPONENTS ORDER BY CODE")
    print("Estado actual de LYNX_PM_COMPONENTS:")
    for code, description, stock in cur.fetchall():
        print(f"  {code}: {description} (stock={stock})")

    conn.close()
    print("Migracion completada.")


if __name__ == "__main__":
    main()
