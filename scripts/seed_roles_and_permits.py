"""
Siembra coherente de roles y permisos para LYNX_PM_ROLS / LYNX_PM_PERMITS /
LYNX_PM_ROLS_PERMITS, alineada con los ROL_ID que ya usan los 7 usuarios
reales de LYNX_PM_USERS (valores 2, 3, 4 y 5 -- no hay usuario con ROL_ID=1).

Jerarquia asumida (de menor a mayor nivel), coherente con la distribucion
real de usuarios:
  ROL_ID=2 TECNICO            (jrodriguez, mcastillo, rperez)
  ROL_ID=3 TECNICO_SENIOR     (agarcia)
  ROL_ID=4 SUPERVISOR         (lfernandez)
  ROL_ID=5 ADMINISTRADOR      (lmota, dcabrera)

Permisos (mismo criterio para los tres: son datos de planificacion/clasificacion
que el tecnico base no necesita capturar, pero si el resto de roles):
  AGREGAR_OPERACION  -- agregar una operacion a un aviso
  VER_CALIFICACION   -- campo "Calificacion" en la creacion del aviso
  VER_PARTE_OBJETO   -- campo "Parte Objeto" en la creacion del aviso

TECNICO base = false en los tres; TECNICO_SENIOR/SUPERVISOR/ADMINISTRADOR = true.
Ajustable despues con un UPDATE si no aplica.

Uso:  IBERO_DB_PASS debe estar en el entorno.
      python scripts/seed_roles_and_permits.py
"""
import os
import oracledb

DSN = "193.122.157.114:1521/lemopdb1.sub03121625300.lemovcn.oraclevcn.com"
USER = "IBEROPHARMAPROD"
INSTANT_CLIENT_DIR = r"C:\oracle\instantclient_21_15"

ROLES = [
    (2, "TECNICO"),
    (3, "TECNICO_SENIOR"),
    (4, "SUPERVISOR"),
    (5, "ADMINISTRADOR"),
]

# id de permiso -> (DESCRIPTION, {rol_id: value})
PERMITS = {
    1: ("AGREGAR_OPERACION", {2: "false", 3: "true", 4: "true", 5: "true"}),
    2: ("VER_CALIFICACION", {2: "false", 3: "true", 4: "true", 5: "true"}),
    3: ("VER_PARTE_OBJETO", {2: "false", 3: "true", 4: "true", 5: "true"}),
}


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

    print("--- LYNX_PM_ROLS ---")
    for rol_id, rol_name in ROLES:
        cur.execute("SELECT COUNT(*) FROM LYNX_PM_ROLS WHERE ID = :id", id=rol_id)
        if cur.fetchone()[0] > 0:
            print(f"  ROL_ID={rol_id} ya existe (no-op).")
            continue
        cur.execute("INSERT INTO LYNX_PM_ROLS (ID, ROL) VALUES (:id, :rol)", id=rol_id, rol=rol_name)
        print(f"  OK: ROL_ID={rol_id} ROL='{rol_name}' insertado.")

    print("--- LYNX_PM_PERMITS ---")
    for permit_id, (description, _) in PERMITS.items():
        cur.execute("SELECT COUNT(*) FROM LYNX_PM_PERMITS WHERE ID = :id", id=permit_id)
        if cur.fetchone()[0] > 0:
            print(f"  PERMIT ID={permit_id} ya existe (no-op).")
            continue
        cur.execute(
            "INSERT INTO LYNX_PM_PERMITS (ID, DESCRIPTION) VALUES (:id, :description)",
            id=permit_id, description=description,
        )
        print(f"  OK: PERMIT ID={permit_id} DESCRIPTION='{description}' insertado.")

    print("--- LYNX_PM_ROLS_PERMITS ---")
    for permit_id, (description, role_values) in PERMITS.items():
        for rol_id, value in role_values.items():
            cur.execute(
                "SELECT COUNT(*) FROM LYNX_PM_ROLS_PERMITS WHERE ROL_ID = :rol_id AND PERMIT = :permit",
                rol_id=rol_id, permit=permit_id,
            )
            if cur.fetchone()[0] > 0:
                print(f"  ROL_ID={rol_id} PERMIT={permit_id} ({description}) ya existe (no-op).")
                continue
            cur.execute(
                "INSERT INTO LYNX_PM_ROLS_PERMITS (ROL_ID, PERMIT, VALUE) VALUES (:rol_id, :permit, :value)",
                rol_id=rol_id, permit=permit_id, value=value,
            )
            print(f"  OK: ROL_ID={rol_id} PERMIT={permit_id} ({description}) VALUE='{value}' insertado.")

    conn.commit()

    print("--- verificacion final ---")
    for permit_id, (description, _) in PERMITS.items():
        cur.execute("""
            SELECT r.ID, r.ROL, rp.VALUE
            FROM LYNX_PM_ROLS r
            LEFT JOIN LYNX_PM_ROLS_PERMITS rp ON rp.ROL_ID = r.ID AND rp.PERMIT = :permit
            ORDER BY r.ID
        """, permit=permit_id)
        print(f"  {description}:")
        for row in cur.fetchall():
            print("   ", row)

    conn.close()
    print("Siembra completada.")


if __name__ == "__main__":
    main()
