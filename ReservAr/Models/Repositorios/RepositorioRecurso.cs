using MySqlConnector;
using ReservAr.Models;

namespace ReservAr.Models.Repositorios;

public class RepositorioRecurso
{
    private readonly string _connectionString;

    public RepositorioRecurso(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public List<Recurso> Listar()
    {
        var lista = new List<Recurso>();

        using var connection = new MySqlConnection(_connectionString);
        var sql = "SELECT idRecurso, idNegocio, Nombre, Descripcion, DuracionMin, Activo from recursos ORDER BY Nombre";
        using var command = new MySqlCommand(sql, connection);

        connection.Open();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            lista.Add(new Recurso
            {
                IdRecurso = reader.GetInt32("idRecurso"),
                IdNegocio = reader.GetInt32("idNegocio"),
                Nombre = reader.GetString("Nombre"),
                Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString("Descripcion"),
                DuracionMin = reader.GetInt32("DuracionMin"),
                Activo = reader.GetBoolean("Activo")




            });

        }

        return lista;

    }


    public Recurso? ObtenerPorId(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = "SELECT idRecurso, idNegocio, Nombre, Descripcion, DuracionMin, Activo FROM recursos WHERE IdRecurso = @id ";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new Recurso
            {
                IdRecurso = reader.GetInt32("idRecurso"),
                IdNegocio = reader.GetInt32("idNegocio"),
                Nombre = reader.GetString("Nombre"),
                Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString("Descripcion"),
                DuracionMin = reader.GetInt32("DuracionMin"),
                Activo = reader.GetBoolean("Activo")


            };
        }




        return null;

    }


    public int Crear(Recurso recurso)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = "INSERT INTO recursos (IdNegocio, Nombre , Descripcion, DuracionMin, Activo) " +
        "VALUES (@idnegocio, @nombre, @descripcion, @duracionMin, @Activo);" +
        "SELECT LAST_INSERT_ID();";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@idnegocio", recurso.IdNegocio);
        command.Parameters.AddWithValue("@nombre", recurso.Nombre);
        command.Parameters.AddWithValue("@descripcion", (object?)recurso.Descripcion ?? DBNull.Value);
        command.Parameters.AddWithValue("@duracionMin", recurso.DuracionMin);
        command.Parameters.AddWithValue("@Activo", recurso.Activo);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());



    }



    public int Modificar(Recurso recurso)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = "UPDATE recursos " +
                  "SET Nombre = @nombre, Descripcion = @descripcion, DuracionMin = @duracionMin, Activo = @activo " +
                  "WHERE IdRecurso = @id";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", recurso.IdRecurso);
        command.Parameters.AddWithValue("@nombre", recurso.Nombre);
        command.Parameters.AddWithValue("@descripcion", (object?)recurso.Descripcion ?? DBNull.Value);
        command.Parameters.AddWithValue("@duracionMin", recurso.DuracionMin);
        command.Parameters.AddWithValue("@activo", recurso.Activo);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    // Baja lógica: evita el error de FK con Reservas/Tarifas/Horarios
public int Eliminar(int id)
{
    using var connection = new MySqlConnection(_connectionString);
    var sql = "UPDATE recursos SET Activo = 0 WHERE IdRecurso = @id";
    using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@id", id);
    connection.Open();
    return command.ExecuteNonQuery();
}



}