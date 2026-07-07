"""
Migracion idempotente: crea LYNX_PM_AVISO_CAUSE.

Parte del corte de Notice/Operation hacia las tablas legacy: la asociacion de
causas es Aviso<->Causa (no Operacion<->Causa), y admite varias causas por
aviso. No se reutilizan las columnas nativas CAUSE/CAUSETEXT/FAIL/FAILTEXT de
LYNX_PM_AVISO porque parecen estar ligadas a la sincronizacion SAP
(STATUSSAP/SAPAVISOID/HASERROR/ERP_ERROR_MSG) y solo admiten un valor. Esta
tabla es enteramente nueva y aditiva: no la usan los formularios web/APEX.

ID generado por trigger+sequence, mismo patron que LYNX_PM_AVISO.ID.
AVISO_ID tiene FK real hacia LYNX_PM_AVISO(ID) (a diferencia de la FK de
LYNX_PM_AVISO_OPERATIONS.AVISOID, que es VARCHAR2 sin constraint real).

Uso:  IBERO_DB_PASS debe estar en el entorno.
      python scripts/migrate_create_aviso_cause_table.py
"""
import os
import oracledb

DSN = "193.122.157.114:1521/lemopdb1.sub03121625300.lemovcn.oraclevcn.com"
USER = "IBEROPHARMAPROD"
INSTANT_CLIENT_DIR = r"C:\oracle\instantclient_21_15"

TABLE = "LYNX_PM_AVISO_CAUSE"
SEQUENCE = "SEQ_LYNX_PM_AVISO_CAUSE_ID"
TRIGGER = "TRG_LYNX_PM_AVISO_CAUSE_ID"
FK_NAME = "FK_LYNX_PM_AVISO_CAUSE_AVISO"


def table_exists(cur, name) -> bool:
    cur.execute("SELECT COUNT(*) FROM user_tables WHERE table_name = :t", t=name)
    return cur.fetchone()[0] > 0


def sequence_exists(cur, name) -> bool:
    cur.execute("SELECT COUNT(*) FROM user_sequences WHERE sequence_name = :s", s=name)
    return cur.fetchone()[0] > 0


def trigger_exists(cur, name) -> bool:
    cur.execute("SELECT COUNT(*) FROM user_triggers WHERE trigger_name = :tr", tr=name)
    return cur.fetchone()[0] > 0


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

    if table_exists(cur, TABLE):
        print(f"{TABLE} ya existe (no-op).")
    else:
        print(f"Creando tabla {TABLE}...")
        cur.execute(f"""
            CREATE TABLE {TABLE} (
                ID          NUMBER PRIMARY KEY,
                AVISO_ID    NUMBER NOT NULL,
                CODE        VARCHAR2(20 CHAR) NOT NULL,
                TEXT        VARCHAR2(500 CHAR),
                CREATED_AT  TIMESTAMP(6) DEFAULT SYSTIMESTAMP,
                IS_DELETED  NUMBER(1) DEFAULT 0 NOT NULL,
                CONSTRAINT {FK_NAME} FOREIGN KEY (AVISO_ID) REFERENCES LYNX_PM_AVISO(ID)
            )
        """)
        conn.commit()
        print(f"OK: tabla {TABLE} creada.")

    if not sequence_exists(cur, SEQUENCE):
        print(f"Creando sequence {SEQUENCE}...")
        cur.execute(f"CREATE SEQUENCE {SEQUENCE} START WITH 1 INCREMENT BY 1 NOCACHE")
        conn.commit()
        print(f"OK: sequence {SEQUENCE} creada.")
    else:
        print(f"{SEQUENCE} ya existe (no-op).")

    if not trigger_exists(cur, TRIGGER):
        print(f"Creando trigger {TRIGGER}...")
        cur.execute(f"""
            CREATE OR REPLACE TRIGGER {TRIGGER}
            BEFORE INSERT ON {TABLE}
            FOR EACH ROW
            BEGIN
                IF :NEW.ID IS NULL THEN
                    :NEW.ID := {SEQUENCE}.NEXTVAL;
                END IF;
            END;
        """)
        conn.commit()
        print(f"OK: trigger {TRIGGER} creado.")
    else:
        print(f"{TRIGGER} ya existe (no-op).")

    cur.execute("SELECT column_name, data_type FROM user_tab_columns WHERE table_name = :t ORDER BY column_id", t=TABLE)
    print("Columnas verificadas:")
    for name, dtype in cur.fetchall():
        print(f"  {name} {dtype}")

    cur.execute("""
        SELECT b.table_name, c.column_name
        FROM user_constraints con
        JOIN user_constraints b ON con.r_constraint_name = b.constraint_name
        JOIN user_cons_columns c ON b.constraint_name = c.constraint_name
        WHERE con.constraint_name = :fk
    """, fk=FK_NAME)
    for ref_table, ref_column in cur.fetchall():
        print(f"  verificado: {FK_NAME} -> {ref_table}({ref_column})")

    conn.close()
    print("Migracion completada.")


if __name__ == "__main__":
    main()
