using MySqlConnector;
using ReservAr.Models;

namespace ReservAr.Models.Repositorios;

public class RepositorioUsuario
{
    private readonly string _connectionString;

    public RepositorioUsuario(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public List<Usuario> Listar(bool incluirInactivos = false)
    {
        var lista = new List<Usuario>();

        using var connection = new MySqlConnection(_connectionString);
        var sql = "SELECT IdUsuario, IdRol, Nombre, Apellido, Email, Telefono, Avatar, Activo, FechaAlta FROM usuarios " +
                  (incluirInactivos ? "" : "WHERE Activo = 1 ") +
                  "ORDER BY FechaAlta ASC";
        using var command = new MySqlCommand(sql, connection);

        connection.Open();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            lista.Add(new Usuario
            {
                IdUsuario = reader.GetInt32("IdUsuario"),
                IdRol = reader.GetInt32("IdRol"),
                Nombre = reader.GetString("Nombre"),
                Apellido = reader.GetString("Apellido"),
                Email = reader.GetString("Email"),
                Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono")) ? null : reader.GetString("Telefono"),
                Avatar = reader.IsDBNull(reader.GetOrdinal("Avatar")) ? null : reader.GetString("Avatar"),
                Activo = reader.GetBoolean("Activo"),
                FechaAlta = reader.GetDateTime("FechaAlta")
            });
        }

        return lista;
    }


    public Usuario? ObtenerPorEmail(string email)
    { 
        using var connection = new MySqlConnection(_connectionString);
        var sql = "SELECT IdUsuario, IdRol, Nombre, Apellido, Email, PasswordHash, Telefono, Avatar, Activo, FechaAlta " + "From usuarios WHERE Email = @email";
        using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@email", email);

    connection.Open();
    using var reader = command.ExecuteReader();

    if (reader.Read())
    {
        return new Usuario
        {
            IdUsuario = reader.GetInt32("IdUsuario"),
            IdRol = reader.GetInt32("IdRol"),
            Nombre = reader.GetString("Nombre"),
            Apellido = reader.GetString("Apellido"),
            Email = reader.GetString("Email"),
            PasswordHash = reader.GetString("PasswordHash"),
            Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono")) ? null : reader.GetString("Telefono"),
            Avatar = reader.IsDBNull(reader.GetOrdinal("Avatar")) ? null : reader.GetString("Avatar"),
            Activo = reader.GetBoolean("Activo"),
            FechaAlta = reader.GetDateTime("FechaAlta")
        };
    }
    return null;


    }

    public Usuario? ObtenerPorId(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = "SELECT IdUsuario, IdRol, Nombre, Apellido, Email, Telefono, Avatar, Activo, FechaAlta FROM usuarios WHERE IdUsuario = @id";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        using var reader = command.ExecuteReader();

        if (reader.Read())
        {
            return new Usuario
            {
                IdUsuario = reader.GetInt32("IdUsuario"),
                IdRol = reader.GetInt32("IdRol"),
                Nombre = reader.GetString("Nombre"),
                Apellido = reader.GetString("Apellido"),
                Email = reader.GetString("Email"),
                Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono")) ? null : reader.GetString("Telefono"),
                Avatar = reader.IsDBNull(reader.GetOrdinal("Avatar")) ? null : reader.GetString("Avatar"),
                Activo = reader.GetBoolean("Activo"),
                FechaAlta = reader.GetDateTime("FechaAlta")
            };
        }

        return null;
    }

    public int Crear(Usuario usuario)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = "INSERT INTO usuarios (IdRol, Nombre, Apellido, Email, PasswordHash, Telefono, Avatar) " +
                  "VALUES (@idRol, @nombre, @apellido, @email, @passwordHash, @telefono, @avatar); " +
                  "SELECT LAST_INSERT_ID();";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@idRol", usuario.IdRol);
        command.Parameters.AddWithValue("@nombre", usuario.Nombre);
        command.Parameters.AddWithValue("@apellido", usuario.Apellido);
        command.Parameters.AddWithValue("@email", usuario.Email);
        command.Parameters.AddWithValue("@passwordHash", usuario.PasswordHash);
        command.Parameters.AddWithValue("@telefono", (object?)usuario.Telefono ?? DBNull.Value);
        command.Parameters.AddWithValue("@avatar", (object?)usuario.Avatar ?? DBNull.Value);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public int Modificar(Usuario usuario)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = "UPDATE usuarios " +
                  "SET IdRol = @idRol, Nombre = @nombre, Apellido = @apellido, Email = @email, Telefono = @telefono, Avatar = @avatar, Activo = @activo " +
                  "WHERE IdUsuario = @id";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", usuario.IdUsuario);
        command.Parameters.AddWithValue("@idRol", usuario.IdRol);
        command.Parameters.AddWithValue("@nombre", usuario.Nombre);
        command.Parameters.AddWithValue("@apellido", usuario.Apellido);
        command.Parameters.AddWithValue("@email", usuario.Email);
        command.Parameters.AddWithValue("@telefono", (object?)usuario.Telefono ?? DBNull.Value);
        command.Parameters.AddWithValue("@avatar", (object?)usuario.Avatar ?? DBNull.Value);
        command.Parameters.AddWithValue("@activo", usuario.Activo);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    public int Eliminar(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = "UPDATE usuarios SET Activo = 0 WHERE IdUsuario = @id";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    public int Reactivar(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = "UPDATE usuarios SET Activo = 1 WHERE IdUsuario = @id";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        return command.ExecuteNonQuery();
    }
}