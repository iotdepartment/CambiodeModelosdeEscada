Imports System.Data.SqlClient
Imports MaterialSkin
Imports MaterialSkin.Controls

Public Class Form1
    Public Sub New()
        InitializeComponent()

        ' Configurar MaterialSkinManager
        Dim skinManager As MaterialSkinManager = MaterialSkinManager.Instance
        skinManager.AddFormToManage(Me)

        ' Tema claro estilo dashboard
        skinManager.Theme = MaterialSkinManager.Themes.LIGHT

        ' Paleta de colores personalizada (azules)
        skinManager.ColorScheme = New ColorScheme(
            Primary.Blue600,       ' Color primario
            Primary.Blue700,       ' Color primario oscuro
            Primary.Blue200,       ' Color primario claro
            Accent.LightBlue200,   ' Color de acento
            TextShade.BLACK        ' Texto oscuro para fondo claro
        )

        ' Botón verde destacado
        Button1.BackColor = Color.Green
        Button1.ForeColor = Color.White
        Button1.UseAccentColor = False   ' Forzar uso del color personalizado

    End Sub

    Private connectionString As String = "Server=RMX-D4LZZV2;Database=GaficadoreTest;User Id=Manu;Password=2022.Tgram2;"
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        ' Queries con orden ascendente
        Dim queryEmpleados As String = "SELECT Nombre FROM Empleados ORDER BY Nombre ASC"
        Dim queryFamilias As String = "SELECT DISTINCT Familia FROM Mandriles WHERE Familia IS NOT NULL ORDER BY Familia ASC"
        Dim queryExtruders As String = "SELECT Extruder FROM Extruders ORDER BY Extruder ASC"

        Using connection As New SqlConnection(connectionString)
            connection.Open()

            ' Empleados → ComboBox1
            Using cmdEmpleados As New SqlCommand(queryEmpleados, connection)
                Dim readerEmp As SqlDataReader = cmdEmpleados.ExecuteReader()
                While readerEmp.Read()
                    ComboBox1.Items.Add(readerEmp("Nombre").ToString())
                End While
                readerEmp.Close()
            End Using

            ' Familias → ComboBox2
            Using cmdFamilias As New SqlCommand(queryFamilias, connection)
                Dim readerFam As SqlDataReader = cmdFamilias.ExecuteReader()
                While readerFam.Read()
                    ComboBox2.Items.Add(readerFam("Familia").ToString())
                End While
                readerFam.Close()
            End Using

            ' Extruders → ComboBox4
            Using cmdExtruders As New SqlCommand(queryExtruders, connection)
                Dim readerExt As SqlDataReader = cmdExtruders.ExecuteReader()
                While readerExt.Read()
                    ComboBox4.Items.Add(readerExt("Extruder").ToString())
                End While
                readerExt.Close()
            End Using

            ' Ahora leer Estado y seleccionar valores
            Dim queryEstado As String = "SELECT TOP 1 Empleado, Mandril, Extruder FROM Estado ORDER BY ID DESC"
            Using cmdEstado As New SqlCommand(queryEstado, connection)
                Dim readerEstado As SqlDataReader = cmdEstado.ExecuteReader()
                If readerEstado.Read() Then
                    ' Obtener IDs
                    Dim empleadoID As Integer = Convert.ToInt32(readerEstado("Empleado"))
                    Dim mandrilID As Integer = Convert.ToInt32(readerEstado("Mandril"))
                    Dim extruderID As Integer = Convert.ToInt32(readerEstado("Extruder"))
                    readerEstado.Close()

                    ' Buscar nombres correspondientes
                    ' Empleado
                    Using cmdEmpName As New SqlCommand("SELECT Nombre FROM Empleados WHERE ID=@ID", connection)
                        cmdEmpName.Parameters.AddWithValue("@ID", empleadoID)
                        Dim nombreEmp As String = Convert.ToString(cmdEmpName.ExecuteScalar())
                        ComboBox1.SelectedItem = nombreEmp
                    End Using

                    ' Mandril y Familia
                    Using cmdMandrilName As New SqlCommand("SELECT Mandril, Familia FROM Mandriles WHERE ID=@ID", connection)
                        cmdMandrilName.Parameters.AddWithValue("@ID", mandrilID)
                        Dim readerMandril As SqlDataReader = cmdMandrilName.ExecuteReader()
                        If readerMandril.Read() Then
                            Dim mandrilName As String = readerMandril("Mandril").ToString()
                            Dim familiaName As String = readerMandril("Familia").ToString()
                            ComboBox2.SelectedItem = familiaName
                            ' Llenar ComboBox3 con mandriles de esa familia
                            ComboBox3.Items.Clear()
                            readerMandril.Close()

                            Using cmdMandrilesFam As New SqlCommand("SELECT Mandril FROM Mandriles WHERE Familia=@Familia ORDER BY Mandril ASC", connection)
                                cmdMandrilesFam.Parameters.AddWithValue("@Familia", familiaName)
                                Dim readerMandrilesFam As SqlDataReader = cmdMandrilesFam.ExecuteReader()
                                While readerMandrilesFam.Read()
                                    ComboBox3.Items.Add(readerMandrilesFam("Mandril").ToString())
                                End While
                                readerMandrilesFam.Close()
                            End Using

                            ComboBox3.SelectedItem = mandrilName
                        End If
                    End Using

                    ' Extruder
                    Using cmdExtName As New SqlCommand("SELECT Extruder FROM Extruders WHERE ID=@ID", connection)
                        cmdExtName.Parameters.AddWithValue("@ID", extruderID)
                        Dim extruderName As String = Convert.ToString(cmdExtName.ExecuteScalar())
                        ComboBox4.SelectedItem = extruderName
                    End Using
                End If
            End Using
        End Using

        ' Recuperar último estado
        Dim queryEstad1o As String = "SELECT TOP 1 Tubo1, Tubo2, Cover FROM Estado ORDER BY ID DESC"
        Using conn As New SqlConnection("Server=RMX-D4LZZV2;Database=GaficadoreTest;User Id=Manu;Password=2022.Tgram2;")
            conn.Open()
            Using cmdEstado As New SqlCommand(queryEstad1o, conn)
                Dim readerEstado As SqlDataReader = cmdEstado.ExecuteReader()
                If readerEstado.Read() Then
                    Dim idTubo1 As Integer = Convert.ToInt32(readerEstado("Tubo1"))
                    Dim idTubo2 As Integer = Convert.ToInt32(readerEstado("Tubo2"))
                    Dim idCover As Integer = Convert.ToInt32(readerEstado("Cover"))
                    readerEstado.Close()

                    ' Ahora consultar los Batch en Materiales
                    Texbox1.Text = ObtenerBatch(conn, idTubo1)
                    Texbox2.Text = ObtenerBatch(conn, idTubo2)
                    Texbox3.Text = ObtenerBatch(conn, idCover)
                End If
            End Using
        End Using

    End Sub
    Private Function ObtenerBatch(conn As SqlConnection, idMaterial As Integer) As String
        Dim query As String = "SELECT Batch FROM Materiales WHERE ID = @ID"
        Using cmd As New SqlCommand(query, conn)
            cmd.Parameters.AddWithValue("@ID", idMaterial)
            Dim result As Object = cmd.ExecuteScalar()
            If result IsNot Nothing Then
                Return result.ToString()
            Else
                Return ""
            End If
        End Using
    End Function


    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        ' Validar selección
        If ComboBox1.SelectedItem Is Nothing OrElse ComboBox3.SelectedItem Is Nothing OrElse ComboBox4.SelectedItem Is Nothing Then
            MessageBox.Show("Selecciona un empleado, mandril y extrusor antes de continuar.")
            Exit Sub
        End If

        ' Obtener valores seleccionados
        Dim empleadoNombre As String = ComboBox1.SelectedItem.ToString()
        Dim mandrilNombre As String = ComboBox3.SelectedItem.ToString()
        Dim extruderNombre As String = ComboBox4.SelectedItem.ToString()

        Dim empleadoID As Integer
        Dim mandrilID As Integer
        Dim extruderID As Integer

        Using connection As New SqlConnection(connectionString)
            connection.Open()

            ' Obtener IDs
            Using cmdEmp As New SqlCommand("SELECT ID FROM Empleados WHERE Nombre = @Nombre", connection)
                cmdEmp.Parameters.AddWithValue("@Nombre", empleadoNombre)
                empleadoID = Convert.ToInt32(cmdEmp.ExecuteScalar())
            End Using

            Using cmdMandril As New SqlCommand("SELECT ID FROM Mandriles WHERE Mandril = @Mandril", connection)
                cmdMandril.Parameters.AddWithValue("@Mandril", mandrilNombre)
                mandrilID = Convert.ToInt32(cmdMandril.ExecuteScalar())
            End Using

            Using cmdExtruder As New SqlCommand("SELECT ID FROM Extruders WHERE Extruder = @Extruder", connection)
                cmdExtruder.Parameters.AddWithValue("@Extruder", extruderNombre)
                extruderID = Convert.ToInt32(cmdExtruder.ExecuteScalar())
            End Using

            ' Actualizar Estado (ejemplo: registro con ID=1)
            Using cmdUpdate As New SqlCommand("UPDATE Estado SET Empleado=@Empleado, Mandril=@Mandril, Extruder=@Extruder WHERE ID=@ID", connection)
                cmdUpdate.Parameters.AddWithValue("@Empleado", empleadoID)
                cmdUpdate.Parameters.AddWithValue("@Mandril", mandrilID)
                cmdUpdate.Parameters.AddWithValue("@Extruder", extruderID)
                cmdUpdate.Parameters.AddWithValue("@ID", 1) ' Ajusta según el registro que quieras actualizar
                cmdUpdate.ExecuteNonQuery()
            End Using
        End Using
        actualizarFamilia()
        Dim result As DialogResult = MessageBox.Show(
    "Tabla Estado actualizada correctamente.",
    "OK",
    MessageBoxButtons.OK,
    MessageBoxIcon.Information
)

        If result = DialogResult.OK Then
            Application.Exit()
        End If

    End Sub
    Private Sub ComboBox2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox2.SelectedIndexChanged
        ComboBox3.Items.Clear()

        Dim selectedFamilia As String = ComboBox2.SelectedItem.ToString()
        Dim queryMandriles As String = "SELECT Mandril FROM Mandriles WHERE Familia = @Familia ORDER BY Mandril ASC"

        Using connection As New SqlConnection(connectionString)
            connection.Open()
            Using cmdMandriles As New SqlCommand(queryMandriles, connection)
                cmdMandriles.Parameters.AddWithValue("@Familia", selectedFamilia)
                Dim readerMandril As SqlDataReader = cmdMandriles.ExecuteReader()
                While readerMandril.Read()
                    ComboBox3.Items.Add(readerMandril("Mandril").ToString())
                End While
                readerMandril.Close()
            End Using
        End Using
    End Sub

    Private Sub actualizarFamilia()
        If ComboBox5.SelectedItem Is Nothing Then
            MessageBox.Show("Seleccione un material antes de continuar.")
            Exit Sub
        End If

        Using conn As New SqlConnection("Server=RMX-D4LZZV2;Database=GaficadoreTest;User Id=Manu;Password=2022.Tgram2;")
            conn.Open()

            ' Recuperar último Estado
            Dim queryEstado As String = "SELECT TOP 1 ID, Tubo1, Tubo2, Cover FROM Estado ORDER BY ID DESC"
            Dim cmdEstado As New SqlCommand(queryEstado, conn)
            Dim readerEstado As SqlDataReader = cmdEstado.ExecuteReader()

            If readerEstado.Read() Then
                Dim idEstado As Integer = Convert.ToInt32(readerEstado("ID"))
                Dim idTubo1 As Integer = Convert.ToInt32(readerEstado("Tubo1"))
                Dim idTubo2 As Integer = Convert.ToInt32(readerEstado("Tubo2"))
                Dim idCover As Integer = Convert.ToInt32(readerEstado("Cover"))
                readerEstado.Close()

                ' Validar cada batch: si cambió, insertar nuevo
                Dim nuevoTubo1 As Integer = ValidarOInsertar(conn, idTubo1, Texbox1.Text, ComboBox5.SelectedItem.ToString())
                Dim nuevoTubo2 As Integer = ValidarOInsertar(conn, idTubo2, Texbox2.Text, ComboBox5.SelectedItem.ToString())
                Dim nuevoCover As Integer = ValidarOInsertar(conn, idCover, Texbox3.Text, ComboBox5.SelectedItem.ToString())

                ' Actualizar Estado con mezcla de IDs
                Dim updateEstado As String = "UPDATE Estado SET Tubo1=@T1, Tubo2=@T2, Cover=@C WHERE ID=@ID"
                Using cmdUpdate As New SqlCommand(updateEstado, conn)
                    cmdUpdate.Parameters.AddWithValue("@T1", nuevoTubo1)
                    cmdUpdate.Parameters.AddWithValue("@T2", nuevoTubo2)
                    cmdUpdate.Parameters.AddWithValue("@C", nuevoCover)
                    cmdUpdate.Parameters.AddWithValue("@ID", idEstado)
                    cmdUpdate.ExecuteNonQuery()
                End Using

                MessageBox.Show("Familia actualizada correctamente.")
            End If
        End Using
    End Sub

    Private Function ValidarOInsertar(conn As SqlConnection, idActual As Integer, nuevoBatch As String, material As String) As Integer
        ' Obtener batch actual
        Dim query As String = "SELECT Batch FROM Materiales WHERE ID=@ID"
        Using cmd As New SqlCommand(query, conn)
            cmd.Parameters.AddWithValue("@ID", idActual)
            Dim actualBatch As Object = cmd.ExecuteScalar()

            ' Si el batch es igual, conservar el mismo ID
            If actualBatch IsNot Nothing AndAlso actualBatch.ToString() = nuevoBatch Then
                Return idActual
            End If
        End Using

        ' Si cambió, insertar nuevo registro
        Dim insertQuery As String = "INSERT INTO Materiales (Batch, Material) OUTPUT INSERTED.ID VALUES (@Batch, @Material)"
        Using cmdInsert As New SqlCommand(insertQuery, conn)
            cmdInsert.Parameters.AddWithValue("@Batch", nuevoBatch)
            cmdInsert.Parameters.AddWithValue("@Material", material)
            Return Convert.ToInt32(cmdInsert.ExecuteScalar())
        End Using
    End Function

    Private Sub Texbox1_Click(sender As Object, e As EventArgs) Handles Texbox1.Click
        Texbox1.Text = ""

    End Sub
End Class