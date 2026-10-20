using MySqlConnector;
using ReservAr.Models;

namespace ReservAr.Models.Repositorios;

public class RepositorioRol
{
    private readonly string _connectionString;

    public RepositorioRol(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public List<Rol> Listar()
    {
        var lista = new List<Rol>();

        using var connection = new MySqlConnection(_connectionString);
        var sql = "SELECT IdRol, Nombre FROM roles";
        using var command = new MySqlCommand(sql, connection);

        connection.Open();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            lista.Add(new Rol
            {
                IdRol = reader.GetInt32("IdRol"),
                Nombre = reader.GetString("Nombre")
            });
        }

        return lista;
    }
    public Rol? ObtenerPorId(int id)
{
    using var connection = new MySqlConnection(_connectionString);
    var sql = "SELECT IdRol, Nombre FROM roles WHERE IdRol = @id";
    using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@id", id);

    connection.Open();
    using var reader = command.ExecuteReader();

    return reader.Read()
        ? new Rol { IdRol = reader.GetInt32("IdRol"), Nombre = reader.GetString("Nombre") }
        : null;
}


public Rol? ObtenerPorNombre(string nombre)
{
    using var connection = new MySqlConnection(_connectionString);
    var sql = "SELECT IdRol, Nombre FROM roles WHERE Nombre = @nombre";
    using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@nombre", nombre);

    connection.Open();
    using var reader = command.ExecuteReader();

    return reader.Read()
        ? new Rol { IdRol = reader.GetInt32("IdRol"), Nombre = reader.GetString("Nombre") }
        : null;
}







}