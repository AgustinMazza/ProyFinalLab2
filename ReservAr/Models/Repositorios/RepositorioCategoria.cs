using MySqlConnector;
using ReservAr.Models;

namespace ReservAr.Models.Repositorios;

public class RepositorioCategoria
{
    private readonly string _connectionString;

    public RepositorioCategoria(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public List<Categoria> Listar()
    {
        var lista = new List<Categoria>();

        using var connection = new MySqlConnection(_connectionString);
        var sql = "SELECT IdCategoria, Nombre, Descripcion, Icono FROM categorias ORDER BY Nombre";
        using var command = new MySqlCommand(sql, connection);

        connection.Open();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            lista.Add(new Categoria
            {
                IdCategoria = reader.GetInt32("IdCategoria"),
                Nombre = reader.GetString("Nombre"),
                Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString("Descripcion"),
                Icono = reader.IsDBNull(reader.GetOrdinal("Icono")) ? null : reader.GetString("Icono")
            });
        }

        return lista;
    }

    public Categoria? ObtenerPorId(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = "SELECT IdCategoria, Nombre, Descripcion, Icono FROM categorias WHERE IdCategoria = @id";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        using var reader = command.ExecuteReader();

        if (reader.Read())
        {
            return new Categoria
            {
                IdCategoria = reader.GetInt32("IdCategoria"),
                Nombre = reader.GetString("Nombre"),
                Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString("Descripcion"),
                Icono = reader.IsDBNull(reader.GetOrdinal("Icono")) ? null : reader.GetString("Icono")
            };
        }

        return null;
    }

    public int Crear(Categoria categoria)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = "INSERT INTO categorias (Nombre, Descripcion, Icono) " +
                  "VALUES (@nombre, @descripcion, @icono); " +
                  "SELECT LAST_INSERT_ID();";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@nombre", categoria.Nombre);
        command.Parameters.AddWithValue("@descripcion", (object?)categoria.Descripcion ?? DBNull.Value);
        command.Parameters.AddWithValue("@icono", (object?)categoria.Icono ?? DBNull.Value);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public int Modificar(Categoria categoria)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = "UPDATE categorias " +
                  "SET Nombre = @nombre, Descripcion = @descripcion, Icono = @icono " +
                  "WHERE IdCategoria = @id";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", categoria.IdCategoria);
        command.Parameters.AddWithValue("@nombre", categoria.Nombre);
        command.Parameters.AddWithValue("@descripcion", (object?)categoria.Descripcion ?? DBNull.Value);
        command.Parameters.AddWithValue("@icono", (object?)categoria.Icono ?? DBNull.Value);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    public int Eliminar(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = "DELETE FROM categorias WHERE IdCategoria = @id";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        return command.ExecuteNonQuery();
    }
}