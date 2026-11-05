using System.Security.Cryptography.X509Certificates;
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

  public int ContarPublico(string buscar, int idCategoria)
  {
    using var connection = new MySqlConnection(_connectionString);
    var sql = @"SELECT COUNT(*) FROM negocios n
                WHERE n.Activo = 1 AND n.Estado = 1
                AND (@idCategoria = 0 OR n.IdCategoria = @idCategoria)
                AND (n.Nombre LIKE @patron OR n.Ciudad LIKE @patron OR n.Direccion LIKE @patron)";
    using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@idCategoria", idCategoria);
    command.Parameters.AddWithValue("@patron", "%" + buscar + "%");

    connection.Open();
    return Convert.ToInt32(command.ExecuteScalar());
  }

  public List<NegocioListado> ListarPaginadoPublico(string buscar, int idCategoria, int pagina, int tamano)
  {
    var lista = new List<NegocioListado>();

    using var connection = new MySqlConnection(_connectionString);
    var sql = @"SELECT n.IdNegocio, n.Nombre, n.Descripcion, n.Direccion, n.Ciudad, n.Telefono, n.LogoUrl,
                  (SELECT GROUP_CONCAT(i.Ruta ORDER BY i.EsPortada DESC, i.Orden, i.IdImagenNegocio SEPARATOR '|')
                   FROM imagenes_negocio i WHERE i.IdNegocio = n.IdNegocio) AS Imagenes,
                      c.Nombre AS CategoriaNombre, c.Icono AS CategoriaIcono,
                      (SELECT COUNT(*) FROM recursos r
                WHERE r.IdNegocio = n.IdNegocio AND r.Activo = 1) AS CantidadRecursos
                FROM negocios n
                INNER JOIN categorias c ON c.IdCategoria = n.IdCategoria
                WHERE n.Activo = 1 AND n.Estado = 1
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
        Imagenes = reader.IsDBNull(reader.GetOrdinal("Imagenes"))? new List<string>(): reader.GetString("Imagenes").Split('|').ToList(),
        CategoriaNombre = reader.GetString("CategoriaNombre"),
        CategoriaIcono = reader.IsDBNull(reader.GetOrdinal("CategoriaIcono")) ? null : reader.GetString("CategoriaIcono"),
        CantidadRecursos = Convert.ToInt32(reader["CantidadRecursos"]),
      });
    }

    return lista;
  }

  public Negocio? ObtenerPorId(int id)
  {
    using var connection = new MySqlConnection(_connectionString);
    var sql = "SELECT IdNegocio, IdCategoria, IdUsuario, Nombre, Descripcion, Direccion, Ciudad, Telefono, LogoUrl, Activo, Estado FROM negocios WHERE IdNegocio = @id";
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
        Estado = (EstadoNegocio)reader.GetInt32("Estado"),
      };
    }

    return null;
  }

  public bool EstaActivo(int idNegocio)
  {
    using var connection = new MySqlConnection(_connectionString);
    var sql = "SELECT COUNT(*) FROM negocios WHERE IdNegocio = @id AND Activo = 1";
    using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@id", idNegocio);

    connection.Open();
    return Convert.ToInt32(command.ExecuteScalar()) > 0;
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
            " Direccion, Ciudad, Telefono, LogoUrl, Activo, Estado)" +
            "VALUES " +
            "(@IdCategoria, @IdUsuario, @Nombre, @Descripcion, " +
            " @Direccion, @Ciudad, @Telefono, @LogoUrl, @Activo, @Estado);" +
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
    command.Parameters.AddWithValue("@Estado", (byte)negocio.Estado);

    connection.Open();

    return Convert.ToInt32(command.ExecuteScalar());
  }

  public int Modificar(Negocio negocio)
  {
    using var connection = new MySqlConnection(_connectionString);
    var sql = "UPDATE negocios " +
            "SET IdCategoria = @idCategoria, " +
            "IdUsuario = @idUsuario, " +
            "Nombre = @nombre, Descripcion = @descripcion, " +
            "Direccion = @direccion, Ciudad = @ciudad, " +
            "Telefono = @telefono, LogoUrl = @logoUrl, Activo = @activo " +
            "WHERE IdNegocio = @id";
    using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@idCategoria", negocio.IdCategoria);
    command.Parameters.AddWithValue("@idUsuario", negocio.IdUsuario);
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

  public int CambiarActivo(int id, bool activo)
  {
    using var connection = new MySqlConnection(_connectionString);
    var sql = "UPDATE negocios SET Activo = @activo WHERE IdNegocio = @id AND Estado = @aprobado";
    using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@id", id);
    command.Parameters.AddWithValue("@activo", activo);
    command.Parameters.AddWithValue("@aprobado", (byte)EstadoNegocio.Aprobado);

    connection.Open();

    return command.ExecuteNonQuery();
  }

  public bool Aprobar(int idNegocio, int idRolAdmin)
  {
    using var connection = new MySqlConnection(_connectionString);
    var sql = "UPDATE negocios n " +
              "INNER JOIN usuarios u ON u.IdUsuario = n.IdUsuario " +
              "SET n.Estado = @aprobado, n.Activo = 1, u.IdRol = @idRolAdmin " +
              "WHERE n.IdNegocio = @id AND n.Estado = @pendiente";
    using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@aprobado", (byte)EstadoNegocio.Aprobado);
    command.Parameters.AddWithValue("@pendiente", (byte)EstadoNegocio.Pendiente);
    command.Parameters.AddWithValue("@idRolAdmin", idRolAdmin);
    command.Parameters.AddWithValue("@id", idNegocio);

    connection.Open();
    return command.ExecuteNonQuery() > 0;
  }

  public bool Rechazar(int idNegocio)
  {
    using var connection = new MySqlConnection(_connectionString);
    var sql = "UPDATE negocios SET Estado = @rechazado, Activo = 0 " +
              "WHERE IdNegocio = @id AND Estado = @pendiente";
    using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@rechazado", (byte)EstadoNegocio.Rechazado);
    command.Parameters.AddWithValue("@pendiente", (byte)EstadoNegocio.Pendiente);
    command.Parameters.AddWithValue("@id", idNegocio);

    connection.Open();
    return command.ExecuteNonQuery() > 0;
  }

  public List<NegocioListado> ListarSolicitudesDeUsuario(int idUsuario)
  {
    var lista = new List<NegocioListado>();

    using var connection = new MySqlConnection(_connectionString);
    var sql = "SELECT n.IdNegocio, n.Nombre, n.Direccion, n.Ciudad, n.Estado, " +
              "c.Nombre AS CategoriaNombre " +
              "FROM negocios n " +
              "INNER JOIN categorias c ON c.IdCategoria = n.IdCategoria " +
              "WHERE n.IdUsuario = @idUsuario " +
              "ORDER BY n.IdNegocio DESC " +
              "LIMIT 20";
    using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@idUsuario", idUsuario);

    connection.Open();
    using var reader = command.ExecuteReader();

    while (reader.Read())
    {
      lista.Add(new NegocioListado
      {
        IdNegocio = reader.GetInt32("IdNegocio"),
        Nombre = reader.GetString("Nombre"),
        Direccion = reader.GetString("Direccion"),
        Ciudad = reader.GetString("Ciudad"),
        Estado = (EstadoNegocio)reader.GetInt32("Estado"),
        CategoriaNombre = reader.GetString("CategoriaNombre")
      });
    }

    return lista;
  }

  // este metodo contar devuelve la cantidad de negocios q cumplen con la busqueda, es mas q todo para calcular el numero de paginas 

  public int Contar(string buscar, int? idUsuario, EstadoNegocio estado)
  {
    using var connection = new MySqlConnection(_connectionString);
    var sql = "SELECT COUNT(*) FROM negocios n " +
                "INNER JOIN usuarios u ON u.IdUsuario = n.IdUsuario " +
                "WHERE (n.Nombre LIKE @patron OR n.Direccion LIKE @patron OR n.Ciudad LIKE @patron " +
                "OR CONCAT(u.Nombre, ' ', u.Apellido) LIKE @patron) " +
                "AND n.Estado = @estado " +
                "AND (@idUsuario IS NULL OR n.IdUsuario = @idUsuario)";

    using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@estado", (byte)estado);
    command.Parameters.AddWithValue("@patron", "%" + buscar + "%");
    command.Parameters.AddWithValue("@idUsuario", (object?)idUsuario ?? DBNull.Value);

    connection.Open();
    return Convert.ToInt32(command.ExecuteScalar());



  }

  //ete metodo es pa el paginado de lo negocio, trae solo 10 negocios de la pagina actual los cuales ya estan filtados por busqueda, dueño 
  // y con el nombre  de la categorio y del dueño :d

  public List<NegocioListado> ListarPaginado(string buscar, int pagina, int tamano, int? idUsuario, EstadoNegocio estado)
  {
    var lista = new List<NegocioListado>();

    using var connection = new MySqlConnection(_connectionString);

    var sql = "SELECT n.IdNegocio, n.IdCategoria, n.IdUsuario, n.Nombre, n.Descripcion, n.Direccion, " +
    "n.Ciudad, n.Telefono, n.LogoUrl, n.Activo, " +
    "(SELECT GROUP_CONCAT(i.Ruta ORDER BY i.EsPortada DESC, i.Orden, i.IdImagenNegocio SEPARATOR '|') " +
    "FROM imagenes_negocio i WHERE i.IdNegocio = n.IdNegocio) AS Imagenes, " +
    "c.Nombre AS CategoriaNombre, " +
    "CONCAT (u.Apellido, ', ', u.Nombre) AS DuenioNombre " +
    "FROM negocios n " +
    "INNER JOIN categorias c ON c.IdCategoria = n.IdCategoria " +
    "INNER JOIN usuarios u ON u.IdUsuario = n.IdUsuario " +
    "WHERE (n.Nombre LIKE @patron OR n.Direccion LIKE @patron OR n.Ciudad LIKE @patron " +
    "OR CONCAT(u.Nombre, ' ', u.Apellido) LIKE @patron) " +
    "AND n.Estado = @estado " +
    "AND (@idUsuario IS NULL OR n.IdUsuario = @idUsuario) " +
    "ORDER BY n.Nombre " +
    "LIMIT @tamano OFFSET @salto";

    using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@estado", (byte)estado);
    command.Parameters.AddWithValue("@patron", "%" + buscar + "%");
    command.Parameters.AddWithValue("@idUsuario", (object?)idUsuario ?? DBNull.Value);
    command.Parameters.AddWithValue("@tamano", tamano);
    command.Parameters.AddWithValue("@salto", (pagina - 1) * tamano);

    connection.Open();
    using var reader = command.ExecuteReader();


    while (reader.Read())
    {
      lista.Add(new NegocioListado
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
        Imagenes = reader.IsDBNull(reader.GetOrdinal("Imagenes")) ? new List<string>() : reader.GetString("Imagenes").Split('|').ToList(),
        CategoriaNombre = reader.GetString("CategoriaNombre"),
        DuenioNombre = reader.GetString("DuenioNombre"),

      });
    }

    return lista;


  }

  public List<ImagenNegocio> ListarImagenes(int idNegocio)
  {
    var lista = new List<ImagenNegocio>();

    using var connection = new MySqlConnection(_connectionString);
    var sql = "SELECT IdImagenNegocio, IdNegocio, Ruta, Orden, EsPortada FROM imagenes_negocio " +
              "WHERE IdNegocio = @id ORDER BY EsPortada DESC, Orden, IdImagenNegocio";
    using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@id", idNegocio);

    connection.Open();
    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
      lista.Add(new ImagenNegocio
      {
        IdImagenNegocio = reader.GetInt32("IdImagenNegocio"),
        IdNegocio = reader.GetInt32("IdNegocio"),
        Ruta = reader.GetString("Ruta"),
        Orden = reader.GetInt32("Orden"),
        EsPortada = reader.GetBoolean("EsPortada"),
      });
    }
    return lista;
  }

  public ImagenNegocio? ObtenerImagen(int idImagen)
  {
    using var connection = new MySqlConnection(_connectionString);
    var sql = "SELECT IdImagenNegocio, IdNegocio, Ruta, Orden, EsPortada FROM imagenes_negocio WHERE IdImagenNegocio = @id";
    using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@id", idImagen);

    connection.Open();
    using var reader = command.ExecuteReader();
    if (reader.Read())
    {
      return new ImagenNegocio
      {
        IdImagenNegocio = reader.GetInt32("IdImagenNegocio"),
        IdNegocio = reader.GetInt32("IdNegocio"),
        Ruta = reader.GetString("Ruta"),
        Orden = reader.GetInt32("Orden"),
        EsPortada = reader.GetBoolean("EsPortada"),
      };
    }
    return null;
  }

  public int ContarImagenes(int idNegocio)
  {
    using var connection = new MySqlConnection(_connectionString);
    var sql = "SELECT COUNT(*) FROM imagenes_negocio WHERE IdNegocio = @id";
    using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@id", idNegocio);

    connection.Open();
    return Convert.ToInt32(command.ExecuteScalar());





  }


  public void AgregarImagen(int idNegocio, string ruta)
  {
    using var connection = new MySqlConnection(_connectionString);
    var sql = "INSERT INTO imagenes_negocio (IdNegocio, Ruta, Orden, EsPortada) " +
    "SELECT @id, @ruta, COALESCE(MAX(Orden), 0) + 1, COUNT(*) = 0 " +
    "FROM imagenes_negocio WHERE IdNegocio = @id";
    using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@id", idNegocio);
    command.Parameters.AddWithValue("@ruta", ruta);
    connection.Open();
    command.ExecuteNonQuery();

  }

  public void EliminarImagen(int idImagen)
  {
    using var connection = new MySqlConnection(_connectionString);
    connection.Open();
    using var tx = connection.BeginTransaction();

    int idNegocio;
    bool eraPortada;
    using (var leer = new MySqlCommand(
      "SELECT IdNegocio, EsPortada FROM imagenes_negocio WHERE IdImagenNegocio = @id", connection, tx))
    {
      leer.Parameters.AddWithValue("@id", idImagen);
      using var reader = leer.ExecuteReader();
      if (!reader.Read()) return;
      idNegocio = reader.GetInt32("IdNegocio");
      eraPortada = reader.GetBoolean("EsPortada");

    }

    using (var borrar = new MySqlCommand("DELETE FROM imagenes_negocio WHERE IdImagenNegocio = @id", connection, tx))
    {
      borrar.Parameters.AddWithValue("@id", idImagen);
      borrar.ExecuteNonQuery();
    }


    if (eraPortada)
    {
      using var promover = new MySqlCommand("UPDATE imagenes_negocio SET EsPortada = 1 WHERE IdNegocio = @idNegocio " +
      "ORDER BY Orden, IdImagenNegocio LIMIT 1", connection, tx);
      promover.Parameters.AddWithValue("@idNegocio", idNegocio);
      promover.ExecuteNonQuery();

    }

    tx.Commit();








  }
  public void MarcarPortada(int idNegocio, int idImagen)
  {
    using var connection = new MySqlConnection(_connectionString);
    var sql = "UPDATE imagenes_negocio SET EsPortada = (IdImagenNegocio = @idImagen) WHERE IdNegocio = @idNegocio";
    using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@idNegocio", idNegocio);
    command.Parameters.AddWithValue("@idImagen", idImagen);

    connection.Open();
    command.ExecuteNonQuery();



  }


}