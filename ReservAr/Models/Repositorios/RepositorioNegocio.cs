using MySqlConnector;
using ReservAr.Models;

namespace ReservAr.Models.Repositorios;

public class RepositorioNegocio
{
    private readonly string _connectionString;

    public RepositorioNegocio(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public List<Negocio> ObtenerTodos()
    {
        var lista = new List<Negocio>();

        using var connection = new MySqlConnection(_connectionString);
        var sql = "SELECT IdNegocio, IdCategoria, IdUsuario, Nombre, Descripcion, Direccion, Ciudad, Telefono, LogoUrl, Activo FROM negocios ORDER BY Nombre";
        using var command = new MySqlCommand(sql, connection);

        connection.Open();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            lista.Add(new Negocio
            {
                IdNegocio = reader.GetInt32("IdNegocio"),
                IdCategoria = reader.GetInt32("IdCategoria"),
                IdUsuario = reader.GetInt32("IdUsuario"),
                Nombre = reader.GetString("Nombre"),
                Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString("Descripcion"),
                Direccion = reader.IsDBNull(reader.GetOrdinal("Direccion")) ? null : reader.GetString("Direccion"),
                Ciudad = reader.IsDBNull(reader.GetOrdinal("Ciudad")) ? null : reader.GetString("Ciudad"),
                Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono")) ? null : reader.GetString("Telefono"),
                LogoUrl = reader.IsDBNull(reader.GetOrdinal("LogoUrl")) ? null : reader.GetString("LogoUrl"),
                Activo = reader.GetBoolean("Activo"),
            });
        }

        return lista;
    }

    public int Contar(string buscar, int idCategoria)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = @"SELECT COUNT(*) FROM negocios n
                WHERE n.Activo = 1
                  AND (@idCategoria = 0 OR n.IdCategoria = @idCategoria)
                  AND (n.Nombre LIKE @patron OR n.Ciudad LIKE @patron OR n.Direccion LIKE @patron)";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@idCategoria", idCategoria);
        command.Parameters.AddWithValue("@patron", "%" + buscar + "%");

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public List<NegocioListado> ListarPaginado(string buscar, int idCategoria, int pagina, int tamano)
    {
        var lista = new List<NegocioListado>();

        using var connection = new MySqlConnection(_connectionString);
        var sql = @"SELECT n.IdNegocio, n.Nombre, n.Descripcion, n.Direccion, n.Ciudad, n.Telefono, n.LogoUrl,
                       c.Nombre AS Categoria,
                       (SELECT COUNT(*) FROM recursos r
                         WHERE r.IdNegocio = n.IdNegocio AND r.Activo = 1) AS CantidadRecursos
                FROM negocios n
                INNER JOIN categorias c ON c.IdCategoria = n.IdCategoria
                WHERE n.Activo = 1
                  AND (@idCategoria = 0 OR n.IdCategoria = @idCategoria)
                  AND (n.Nombre LIKE @patron OR n.Ciudad LIKE @patron OR n.Direccion LIKE @patron)
                ORDER BY n.Nombre
                LIMIT @tamano OFFSET @salto";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@idCategoria", idCategoria);
        command.Parameters.AddWithValue("@patron", "%" + buscar + "%");
        command.Parameters.AddWithValue("@tamano", tamano);
        command.Parameters.AddWithValue("@salto", (pagina - 1) * tamano);

        connection.Open();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            lista.Add(new NegocioListado
            {
                IdNegocio = reader.GetInt32("IdNegocio"),
                Nombre = reader.GetString("Nombre"),
                Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString("Descripcion"),
                Direccion = reader.IsDBNull(reader.GetOrdinal("Direccion")) ? null : reader.GetString("Direccion"),
                Ciudad = reader.IsDBNull(reader.GetOrdinal("Ciudad")) ? null : reader.GetString("Ciudad"),
                Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono")) ? null : reader.GetString("Telefono"),
                LogoUrl = reader.IsDBNull(reader.GetOrdinal("LogoUrl")) ? null : reader.GetString("LogoUrl"),
                Categoria = reader.GetString("Categoria"),
                CantidadRecursos = Convert.ToInt32(reader["CantidadRecursos"]),
            });
        }

        return lista;
    }

    public Negocio? ObtenerPorId(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = "SELECT IdNegocio, IdCategoria, IdUsuario, Nombre, Descripcion, Direccion, Ciudad, Telefono, LogoUrl, Activo FROM negocios ORDER BY Nombre WHERE IdNegocio = @id";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        using var reader = command.ExecuteReader();

        if (reader.Read())
        {
            return new Negocio
            {
                IdNegocio = reader.GetInt32("IdNegocio"),
                IdCategoria = reader.GetInt32("IdCategoria"),
                IdUsuario = reader.GetInt32("IdUsuario"),
                Nombre = reader.GetString("Nombre"),
                Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString("Descripcion"),
                Direccion = reader.IsDBNull(reader.GetOrdinal("Direccion")) ? null : reader.GetString("Direccion"),
                Ciudad = reader.IsDBNull(reader.GetOrdinal("Ciudad")) ? null : reader.GetString("Ciudad"),
                Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono")) ? null : reader.GetString("Telefono"),
                LogoUrl = reader.IsDBNull(reader.GetOrdinal("LogoUrl")) ? null : reader.GetString("LogoUrl"),
                Activo = reader.GetBoolean("Activo"),
            };
        }

        return null;
    }

    public List<Negocio> ObtenerPorUsuario(int idUsuario)
    {
        var lista = new List<Negocio>();

        using var connection = new MySqlConnection(_connectionString);
        var sql = "SELECT IdNegocio, IdCategoria, IdUsuario, Nombre, Descripcion, Direccion, Ciudad, Telefono, LogoUrl, Activo " +
                  "FROM negocios WHERE IdUsuario = @idUsuario ORDER BY Nombre";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@idUsuario", idUsuario);

        connection.Open();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            lista.Add(new Negocio
            {
                IdNegocio = reader.GetInt32("IdNegocio"),
                IdCategoria = reader.GetInt32("IdCategoria"),
                IdUsuario = reader.GetInt32("IdUsuario"),
                Nombre = reader.GetString("Nombre"),
                Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString("Descripcion"),
                Direccion = reader.IsDBNull(reader.GetOrdinal("Direccion")) ? null : reader.GetString("Direccion"),
                Ciudad = reader.IsDBNull(reader.GetOrdinal("Ciudad")) ? null : reader.GetString("Ciudad"),
                Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono")) ? null : reader.GetString("Telefono"),
                LogoUrl = reader.IsDBNull(reader.GetOrdinal("LogoUrl")) ? null : reader.GetString("LogoUrl"),
                Activo = reader.GetBoolean("Activo"),
            });
        }

        return lista;
    }
    public List<Negocio> ObtenerPorCategoria(int idCategoria)
    {
        var lista = new List<Negocio>();

        using var connection = new MySqlConnection(_connectionString);
        var sql = "SELECT IdNegocio, IdCategoria, IdUsuario, Nombre, Descripcion, Direccion, Ciudad, Telefono, LogoUrl, Activo " +
                  "FROM negocios WHERE IdCategoria = @idCategoria ORDER BY Nombre";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@idCategoria", idCategoria);

        connection.Open();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            lista.Add(new Negocio
            {
                IdNegocio = reader.GetInt32("IdNegocio"),
                IdCategoria = reader.GetInt32("IdCategoria"),
                IdUsuario = reader.GetInt32("IdUsuario"),
                Nombre = reader.GetString("Nombre"),
                Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString("Descripcion"),
                Direccion = reader.IsDBNull(reader.GetOrdinal("Direccion")) ? null : reader.GetString("Direccion"),
                Ciudad = reader.IsDBNull(reader.GetOrdinal("Ciudad")) ? null : reader.GetString("Ciudad"),
                Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono")) ? null : reader.GetString("Telefono"),
                LogoUrl = reader.IsDBNull(reader.GetOrdinal("LogoUrl")) ? null : reader.GetString("LogoUrl"),
                Activo = reader.GetBoolean("Activo"),


            });
        }

        return lista;
    }

    public int Crear(Negocio negocio)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = "INSERT INTO negocios " +
                "(IdCategoria, IdUsuario, Nombre, Descripcion," +
                " Direccion, Ciudad, Telefono, LogoUrl, Activo)" +
                "VALUES " +
                "(@IdCategoria, @IdUsuario, @Nombre, @Descripcion, " +
                " @Direccion, @Ciudad, @Telefono, @LogoUrl, @Activo);" +
                "SELECT LAST_INSERT_ID();";

        using var command = new MySqlCommand(sql, connection);

        command.Parameters.AddWithValue("@IdCategoria", negocio.IdCategoria);
        command.Parameters.AddWithValue("@IdUsuario", negocio.IdUsuario);
        command.Parameters.AddWithValue("@Nombre", negocio.Nombre);
        command.Parameters.AddWithValue("@Descripcion", (object?)negocio.Descripcion ?? DBNull.Value);
        command.Parameters.AddWithValue("@Direccion", (object?)negocio.Direccion ?? DBNull.Value);
        command.Parameters.AddWithValue("@Ciudad", (object?)negocio.Ciudad ?? DBNull.Value);
        command.Parameters.AddWithValue("@Telefono", (object?)negocio.Telefono ?? DBNull.Value);
        command.Parameters.AddWithValue("@LogoUrl", (object?)negocio.LogoUrl ?? DBNull.Value);
        command.Parameters.AddWithValue("@Activo", negocio.Activo);

        connection.Open();

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public int Modificar(Negocio negocio)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = "UPDATE negocios " +
                "SET IdCategoria = @idCategoria, " +
                "Nombre = @nombre, Descripcion = @descripcion, " +
                "Direccion = @direccion, Ciudad = @ciudad, " +
                "Telefono = @telefono, LogoUrl = @logoUrl, Activo = @activo " +
                "WHERE IdNegocio = @id";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@idCategoria", negocio.IdCategoria);
        command.Parameters.AddWithValue("@nombre", negocio.Nombre);
        command.Parameters.AddWithValue("@descripcion", (object?)negocio.Descripcion ?? DBNull.Value);
        command.Parameters.AddWithValue("@direccion", (object?)negocio.Direccion ?? DBNull.Value);
        command.Parameters.AddWithValue("@ciudad", (object?)negocio.Ciudad ?? DBNull.Value);
        command.Parameters.AddWithValue("@telefono", (object?)negocio.Telefono ?? DBNull.Value);
        command.Parameters.AddWithValue("@logoUrl", (object?)negocio.LogoUrl ?? DBNull.Value);
        command.Parameters.AddWithValue("@activo", negocio.Activo);
        command.Parameters.AddWithValue("@id", negocio.IdNegocio);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    public int Eliminar(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = "DELETE FROM negocios WHERE IdNegocio = @id";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    public int CambiarEstado(int id, bool activo)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = "UPDATE negocios SET Activo = @activo WHERE IdNegocio = @id";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@activo", activo);

        connection.Open();

        return command.ExecuteNonQuery();
    }
}