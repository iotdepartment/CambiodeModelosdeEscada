import pyodbc
import time

connection_string = (
    "DRIVER={SQL Server};"
    "SERVER=RMX-D4LZZV2;"
    "DATABASE=GaficadoreTest;"
    "UID=Manu;"
    "PWD=2022.Tgram2;"
)

# Variable global para guardar el valor anterior
contador_anterior = None

def guardar_registro():
    global contador_anterior
    try:
        with pyodbc.connect(connection_string) as connection:
            with connection.cursor() as cursor:
                # 1. Leer el único registro de Estado
                cursor.execute("""
                    SELECT TOP 1 Empleado, Mandril, Extruder, Contador
                    FROM Estado
                    ORDER BY ID DESC
                """)
                estado = cursor.fetchone()
                if not estado:
                    print("No hay registros en Estado.")
                    return

                empleado, mandril, extruder, contador_actual = estado

                # 2. Primer ciclo: solo guardar valor
                if contador_anterior is None:
                    contador_anterior = contador_actual
                    print(f"Primer valor leído: {contador_actual}. No se inserta todavía.")
                    return

                # 3. Calcular diferencia
                diferencia = contador_actual - contador_anterior
                piezas = max(diferencia, 0)

                # 4. Insertar en ScadaRegistro
                cursor.execute("""
                    INSERT INTO ScadaRegistro (Empleado, Mandril, Extruder, Piezas, FechaHora)
                    VALUES (?, ?, ?, ?, GETDATE())
                """, (empleado, mandril, extruder, piezas))

                connection.commit()
                print(f"Registradas {piezas} piezas nuevas.")

                # 5. Actualizar variable
                contador_anterior = contador_actual

    except pyodbc.Error as e:
        print("Error de base de datos:", e)
    except Exception as e:
        print("Error general:", e)

# Bucle principal
try:
    while True:
        guardar_registro()
        time.sleep(25)
except KeyboardInterrupt:
    print("Proceso detenido manualmente.")