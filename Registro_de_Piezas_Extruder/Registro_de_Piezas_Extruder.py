import pyodbc
import time
from datetime import datetime

# Conexión a SQL Server con tus credenciales
connection_string = (
    "DRIVER={ODBC Driver 17 for SQL Server};"
    "SERVER=RMX-D4LZZV2;"
    "DATABASE=GaficadoreTest;"
    "UID=Manu;"
    "PWD=2022.Tgram2;"
)

def mostrar_tabla_estado(cursor):
    cursor.execute("""
        SELECT ID, Extruder, Empleado, Mandril, Contador, Tubo1, Tubo2, Cover, Batch
        FROM Estado
        ORDER BY ID DESC
    """)
    registros = cursor.fetchall()

    print("\n=== Contenido actual de la tabla Estado ===")
    for row in registros:
        print(f"ID={row.ID}, Extruder={row.Extruder}, Empleado={row.Empleado}, "
              f"Mandril={row.Mandril}, Contador={row.Contador}, "
              f"Tubo1={row.Tubo1}, Tubo2={row.Tubo2}, Cover={row.Cover}, Batch={row.Batch}")
    print("==========================================")

def obtener_estado_actual(cursor):
    cursor.execute("""
        SELECT TOP 1 ID, Extruder, Empleado, Mandril, Contador, Tubo1, Tubo2, Cover, Batch
        FROM Estado
        ORDER BY ID DESC
    """)
    return cursor.fetchone()

def registrar_piezas(cursor, empleado, mandril, extruder, piezas, tubo1, tubo2, cover):
    fecha_hora = datetime.now()

    # Mostrar en consola lo que se va a insertar
    print("\n--- Insertando en ScadaRegistro ---")
    print(f"Empleado={empleado}, Mandril={mandril}, Extruder={extruder}")
    print(f"Piezas={piezas}, FechaHora={fecha_hora}")
    print(f"Tubo1={tubo1}, Tubo2={tubo2}, Cover={cover}")
    print("-----------------------------------")

    cursor.execute("""
        INSERT INTO ScadaRegistro (Empleado, Mandril, Extruder, Piezas, FechaHora, Tubo1, Tubo2, Cover)
        VALUES (?, ?, ?, ?, ?, ?, ?, ?)
    """, (empleado, mandril, extruder, piezas, fecha_hora, tubo1, tubo2, cover))
    cursor.commit()

def main():
    conn = pyodbc.connect(connection_string)
    cursor = conn.cursor()

    contador_anterior = None

    while True:
        try:
            # Mostrar todos los registros de Estado
            mostrar_tabla_estado(cursor)

            # Obtener el último registro
            estado = obtener_estado_actual(cursor)
            if estado:
                (id_estado, extruder, empleado, mandril, contador, tubo1, tubo2, cover, batch) = estado

                if contador_anterior is None:
                    contador_anterior = contador
                else:
                    diferencia = contador - contador_anterior
                    if diferencia > 0:
                        registrar_piezas(cursor, empleado, mandril, extruder, diferencia, tubo1, tubo2, cover)
                        print(f"✔ Registradas {diferencia} piezas en ScadaRegistro.")
                    contador_anterior = contador

        except Exception as e:
            print(f"❌ Error: {e}")

        time.sleep(10)

if __name__ == "__main__":
    main()