"""
Crea tabla LYNX_PM_EQUIPMENT_MEDIA y procedimientos ORDS
para asociar imagenes y videos a equipos.
"""
import oracledb
import os

oracledb.init_oracle_client(lib_dir=r"C:\oracle\instantclient_23_0")

conn = oracledb.connect(
    user="IBEROPHARMAPROD",
    password=os.environ["IBERO_DB_PASS"],
    dsn="193.122.157.114:1521/lemopdb1.sub03121625300.lemovcn.oraclevcn.com"
)
cur = conn.cursor()

# ── 1. Tabla ──────────────────────────────────────────────────────────────────
print("Creando tabla LYNX_PM_EQUIPMENT_MEDIA...")
cur.execute("""
BEGIN
    EXECUTE IMMEDIATE '
        CREATE TABLE LYNX_PM_EQUIPMENT_MEDIA (
            ID              RAW(16)        DEFAULT SYS_GUID() NOT NULL,
            EQUIPMENT_CODE  VARCHAR2(50)   NOT NULL,
            MEDIA_TYPE      VARCHAR2(10)   NOT NULL,
            URL             VARCHAR2(2000) NOT NULL,
            THUMBNAIL_URL   VARCHAR2(2000),
            TITLE           VARCHAR2(200),
            POSITION        NUMBER(5)      DEFAULT 0 NOT NULL,
            CREATED_BY      VARCHAR2(100),
            CREATED_AT      TIMESTAMP      DEFAULT SYSTIMESTAMP NOT NULL,
            UPDATED_AT      TIMESTAMP      DEFAULT SYSTIMESTAMP NOT NULL,
            IS_DELETED      NUMBER(1)      DEFAULT 0 NOT NULL,
            CONSTRAINT PK_LYNX_PM_EQ_MEDIA  PRIMARY KEY (ID),
            CONSTRAINT CK_LYNX_PM_EQ_MTYPE  CHECK (MEDIA_TYPE IN (''IMAGE'',''VIDEO'')),
            CONSTRAINT FK_LYNX_PM_EQ_MEDIA_EQ
                FOREIGN KEY (EQUIPMENT_CODE)
                REFERENCES LYNX_PM_EQUIPMENTS(CODE)
        )
    ';
    DBMS_OUTPUT.PUT_LINE('Tabla creada.');
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE = -955 THEN
            DBMS_OUTPUT.PUT_LINE('Tabla ya existe.');
        ELSE RAISE;
        END IF;
END;
""")
conn.commit()
print("  OK")

# ── 2. Índices ────────────────────────────────────────────────────────────────
print("Creando índices...")
for ddl in [
    "CREATE INDEX IDX_LYNX_EQ_MEDIA_CODE ON LYNX_PM_EQUIPMENT_MEDIA(EQUIPMENT_CODE)",
    "CREATE INDEX IDX_LYNX_EQ_MEDIA_TYPE ON LYNX_PM_EQUIPMENT_MEDIA(MEDIA_TYPE)",
]:
    try:
        cur.execute(ddl)
        conn.commit()
        print(f"  OK: {ddl[:60]}")
    except oracledb.DatabaseError as e:
        if "ORA-00955" in str(e):
            print(f"  Ya existe: {ddl[:60]}")
        else:
            raise

# ── 3. Procedimiento GET_EQUIPMENT_MEDIA ──────────────────────────────────────
print("Creando procedimiento GET_EQUIPMENT_MEDIA...")
cur.execute("""
CREATE OR REPLACE PROCEDURE GET_EQUIPMENT_MEDIA(
    p_equipment_code IN VARCHAR2,
    p_result         OUT SYS_REFCURSOR
) AS
BEGIN
    OPEN p_result FOR
        SELECT
            RAWTOHEX(ID)      AS ID,
            EQUIPMENT_CODE,
            MEDIA_TYPE,
            URL,
            THUMBNAIL_URL,
            TITLE,
            POSITION,
            CREATED_BY,
            TO_CHAR(CREATED_AT,'YYYY-MM-DD"T"HH24:MI:SS') AS CREATED_AT
        FROM LYNX_PM_EQUIPMENT_MEDIA
        WHERE EQUIPMENT_CODE = UPPER(TRIM(p_equipment_code))
          AND IS_DELETED      = 0
        ORDER BY POSITION ASC, CREATED_AT ASC;
END GET_EQUIPMENT_MEDIA;
""")
conn.commit()
print("  OK")

# ── 4. Procedimiento POST_EQUIPMENT_MEDIA ─────────────────────────────────────
print("Creando procedimiento POST_EQUIPMENT_MEDIA...")
cur.execute("""
CREATE OR REPLACE PROCEDURE POST_EQUIPMENT_MEDIA(
    p_equipment_code IN VARCHAR2,
    p_media_type     IN VARCHAR2,
    p_url            IN VARCHAR2,
    p_thumbnail_url  IN VARCHAR2 DEFAULT NULL,
    p_title          IN VARCHAR2 DEFAULT NULL,
    p_position       IN NUMBER   DEFAULT 0,
    p_created_by     IN VARCHAR2 DEFAULT NULL,
    p_id             OUT VARCHAR2,
    p_result         OUT VARCHAR2
) AS
    v_id RAW(16) := SYS_GUID();
BEGIN
    -- Validaciones básicas
    IF p_equipment_code IS NULL OR p_url IS NULL OR p_media_type IS NULL THEN
        p_result := 'ERROR:PARAMETROS_REQUERIDOS';
        RETURN;
    END IF;
    IF UPPER(TRIM(p_media_type)) NOT IN ('IMAGE','VIDEO') THEN
        p_result := 'ERROR:TIPO_INVALIDO';
        RETURN;
    END IF;

    INSERT INTO LYNX_PM_EQUIPMENT_MEDIA(
        ID, EQUIPMENT_CODE, MEDIA_TYPE, URL,
        THUMBNAIL_URL, TITLE, POSITION, CREATED_BY
    ) VALUES (
        v_id,
        UPPER(TRIM(p_equipment_code)),
        UPPER(TRIM(p_media_type)),
        TRIM(p_url),
        TRIM(p_thumbnail_url),
        TRIM(p_title),
        p_position,
        p_created_by
    );

    p_id     := RAWTOHEX(v_id);
    p_result := 'OK';
EXCEPTION
    WHEN DUP_VAL_ON_INDEX THEN
        p_result := 'ERROR:DUPLICADO';
    WHEN OTHERS THEN
        p_result := 'ERROR:' || SQLERRM;
END POST_EQUIPMENT_MEDIA;
""")
conn.commit()
print("  OK")

# ── 5. Procedimiento DELETE_EQUIPMENT_MEDIA ───────────────────────────────────
print("Creando procedimiento DELETE_EQUIPMENT_MEDIA...")
cur.execute("""
CREATE OR REPLACE PROCEDURE DELETE_EQUIPMENT_MEDIA(
    p_id     IN VARCHAR2,
    p_result OUT VARCHAR2
) AS
    v_rows NUMBER;
BEGIN
    UPDATE LYNX_PM_EQUIPMENT_MEDIA
    SET IS_DELETED = 1, UPDATED_AT = SYSTIMESTAMP
    WHERE ID = HEXTORAW(p_id)
      AND IS_DELETED = 0;

    v_rows := SQL%ROWCOUNT;
    p_result := CASE WHEN v_rows > 0 THEN 'OK' ELSE 'ERROR:NO_ENCONTRADO' END;
EXCEPTION
    WHEN OTHERS THEN
        p_result := 'ERROR:' || SQLERRM;
END DELETE_EQUIPMENT_MEDIA;
""")
conn.commit()
print("  OK")

# ── 6. Actualizar el router ORDS para incluir los 3 servicios ─────────────────
print("Registrando servicios en LYNX_PM_POST_ROUTER...")
router_cases = [
    ("GET_EQUIPMENT_MEDIA", "GET_EQUIPMENT_MEDIA"),
    ("POST_EQUIPMENT_MEDIA", "POST_EQUIPMENT_MEDIA"),
    ("DELETE_EQUIPMENT_MEDIA", "DELETE_EQUIPMENT_MEDIA"),
]
for svc, proc in router_cases:
    print(f"  Servicio {svc} → procedimiento {proc} (registrar en router manualmente si aplica)")

print()
print("========================================")
print("COMPLETADO:")
print("  Tabla  : LYNX_PM_EQUIPMENT_MEDIA")
print("  Procs  : GET_EQUIPMENT_MEDIA")
print("           POST_EQUIPMENT_MEDIA")
print("           DELETE_EQUIPMENT_MEDIA")
print("  Índices: EQUIPMENT_CODE, MEDIA_TYPE")
print("========================================")

cur.close()
conn.close()
