using MySqlConnector;
using ReservAr.Models;

namespace ReservAr.Models.Repositorios;

public class RepositorioReserva
{
  private readonly string _connectionString;

  public RepositorioReserva(IConfiguration configuration)
  {
    _connectionString = configuration.GetConnectionString("DefaultConnection")!;
  }

  public Reserva? ObtenerPorId(int id)
  {
    using var connection = new MySqlConnection(_connectionString);
    var sql = "SELECT idReserva, idUsuario, idRecurso, idNegocio, Fecha, HoraInicio, HoraFin, Estado, Precio, Observaciones, Comprobante, FechaCreacion FROM reservas WHERE IdReserva = @id ";
    using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@id", id);

    connection.Open();
    using var reader = command.ExecuteReader();
    if (reader.Read())
    {
      return new Reserva
      {
        IdReserva = reader.GetInt32("idReserva"),
        IdUsuario = reader.GetInt32("idUsuario"),
        IdRecurso = reader.GetInt32("idRecurso"),
        IdNegocio = reader.GetInt32("idNegocio"),
        Fecha = reader.GetDateTime("Fecha"),
        HoraInicio = reader.GetTimeSpan("HoraInicio"),
        HoraFin = reader.GetTimeSpan("HoraFin"),
        Estado = (EstadoReserva)reader.GetInt32("Estado"),
        Precio = reader.GetDecimal("Precio"),
        Observaciones = reader.IsDBNull(reader.GetOrdinal("Observaciones")) ? null : reader.GetString("Observaciones"),
        Comprobante = reader.IsDBNull(reader.GetOrdinal("Comprobante")) ? null : reader.GetString("Comprobante"),
        FechaCreacion = reader.GetDateTime("FechaCreacion")
      };
    }
    return null;
  }

  public List<Reserva> ListarPorRecursoYFecha(int idRecurso, DateTime fecha)
  {
    var lista = new List<Reserva>();

    using var connection = new MySqlConnection(_connectionString);
    var sql = @"SELECT idReserva, idUsuario, idRecurso, idNegocio, Fecha, HoraInicio, HoraFin, Estado, Precio, Observaciones, Comprobante, FechaCreacion
                FROM reservas
                WHERE IdRecurso = @idRecurso AND Fecha = @fecha AND Estado <> 2
                ORDER BY HoraInicio";
    using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@idRecurso", idRecurso);
    command.Parameters.AddWithValue("@fecha", fecha.Date);

    connection.Open();
    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
      lista.Add(new Reserva
      {
        IdReserva = reader.GetInt32("idReserva"),
        IdUsuario = reader.GetInt32("idUsuario"),
        IdRecurso = reader.GetInt32("idRecurso"),
        IdNegocio = reader.GetInt32("idNegocio"),
        Fecha = reader.GetDateTime("Fecha"),
        HoraInicio = reader.GetTimeSpan("HoraInicio"),
        HoraFin = reader.GetTimeSpan("HoraFin"),
        Estado = (EstadoReserva)reader.GetInt32("Estado"),
        Precio = reader.GetDecimal("Precio"),
        Observaciones = reader.IsDBNull(reader.GetOrdinal("Observaciones")) ? null : reader.GetString("Observaciones"),
        Comprobante = reader.IsDBNull(reader.GetOrdinal("Comprobante")) ? null : reader.GetString("Comprobante"),
        FechaCreacion = reader.GetDateTime("FechaCreacion")
      });
    }

    return lista;
  }

  public int ContarDeUsuario(int idUsuario)
  {
    using var connection = new MySqlConnection(_connectionString);
    var sql = "SELECT COUNT(*) FROM reservas WHERE IdUsuario = @idUsuario";
    using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@idUsuario", idUsuario);

    connection.Open();
    return Convert.ToInt32(command.ExecuteScalar());
  }

  public List<ReservaListado> ListarDeUsuario(int idUsuario, int pagina, int tamano)
  {
    var lista = new List<ReservaListado>();

    using var connection = new MySqlConnection(_connectionString);
    var sql = @"SELECT r.IdReserva, r.Fecha, r.HoraInicio, r.HoraFin, r.Estado,
                      rc.Nombre AS RecursoNombre, n.Nombre AS NegocioNombre
                FROM reservas r
                INNER JOIN recursos rc ON rc.IdRecurso = r.IdRecurso
                INNER JOIN negocios n ON n.IdNegocio = r.IdNegocio
                WHERE r.IdUsuario = @idUsuario
                ORDER BY r.Fecha DESC, r.HoraInicio DESC
                LIMIT @tamano OFFSET @salto";
    using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@idUsuario", idUsuario);
    command.Parameters.AddWithValue("@tamano", tamano);
    command.Parameters.AddWithValue("@salto", (pagina - 1) * tamano);

    connection.Open();
    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
      lista.Add(new ReservaListado
      {
        IdReserva = reader.GetInt32("IdReserva"),
        Fecha = reader.GetDateTime("Fecha"),
        HoraInicio = reader.GetTimeSpan("HoraInicio"),
        HoraFin = reader.GetTimeSpan("HoraFin"),
        Estado = (EstadoReserva)reader.GetInt32("Estado"),
        RecursoNombre = reader.GetString("RecursoNombre"),
        NegocioNombre = reader.GetString("NegocioNombre")
      });
    }

    return lista;
  }

  public ReservaInfo? ObtenerInfoDeUsuario(int idReserva, int idUsuario)
  {
    using var connection = new MySqlConnection(_connectionString);
    var sql = @"SELECT n.Nombre AS Complejo, n.Direccion, n.Ciudad, n.Descripcion AS DescripcionComplejo,
                       n.Telefono, rc.Nombre AS Cancha, rc.Descripcion AS DescripcionCancha
                FROM reservas r
                INNER JOIN negocios n  ON n.IdNegocio  = r.IdNegocio
                INNER JOIN recursos rc ON rc.IdRecurso = r.IdRecurso
                WHERE r.IdReserva = @idReserva AND r.IdUsuario = @idUsuario";
    using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@idReserva", idReserva);
    command.Parameters.AddWithValue("@idUsuario", idUsuario);

    connection.Open();
    using var reader = command.ExecuteReader();
    if (reader.Read())
    {
      return new ReservaInfo
      {
        Complejo = reader.GetString("Complejo"),
        Direccion = reader.GetString("Direccion"),
        Ciudad = reader.GetString("Ciudad"),
        DescripcionComplejo = reader.IsDBNull(reader.GetOrdinal("DescripcionComplejo")) ? null : reader.GetString("DescripcionComplejo"),
        Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono")) ? null : reader.GetString("Telefono"),
        Cancha = reader.GetString("Cancha"),
        DescripcionCancha = reader.IsDBNull(reader.GetOrdinal("DescripcionCancha")) ? null : reader.GetString("DescripcionCancha")
      };
    }
    return null;
  }

  public ReservaTicket? ObtenerTicketDeUsuario(int idReserva, int idUsuario)
  {
    using var connection = new MySqlConnection(_connectionString);
    var sql = @"SELECT r.IdReserva, r.Fecha, r.HoraInicio, r.HoraFin, r.Estado, r.Precio,
                  n.Nombre AS Complejo, n.Direccion, n.Ciudad, n.Telefono,
                  rc.Nombre AS Cancha,
                  CONCAT(u.Nombre, ' ', u.Apellido) AS Cliente
                FROM reservas r
                INNER JOIN negocios n  ON n.IdNegocio  = r.IdNegocio
                INNER JOIN recursos rc ON rc.IdRecurso = r.IdRecurso
                INNER JOIN usuarios u  ON u.IdUsuario  = r.IdUsuario
                WHERE r.IdReserva = @idReserva AND r.IdUsuario = @idUsuario";
    using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@idReserva", idReserva);
    command.Parameters.AddWithValue("@idUsuario", idUsuario);

    connection.Open();
    using var reader = command.ExecuteReader();
    if (reader.Read())
    {
      return new ReservaTicket
      {
        IdReserva = reader.GetInt32("IdReserva"),
        Cliente = reader.GetString("Cliente"),
        Complejo = reader.GetString("Complejo"),
        Direccion = reader.GetString("Direccion"),
        Ciudad = reader.GetString("Ciudad"),
        Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono")) ? null : reader.GetString("Telefono"),
        Cancha = reader.GetString("Cancha"),
        Fecha = reader.GetDateTime("Fecha"),
        HoraInicio = reader.GetTimeSpan("HoraInicio"),
        HoraFin = reader.GetTimeSpan("HoraFin"),
        Precio = reader.GetDecimal("Precio"),
        Estado = (EstadoReserva)reader.GetInt32("Estado")
      };
    }
    return null;
  }

  private const string FiltroGestion = @"
    (@idDuenio IS NULL OR n.IdUsuario = @idDuenio)
    AND (@estado IS NULL OR r.Estado = @estado)
    AND (@buscar = '' OR n.Nombre LIKE @like OR rc.Nombre LIKE @like
         OR CONCAT(u.Nombre, ' ', u.Apellido) LIKE @like)";

  private static void AgregarFiltrosGestion(MySqlCommand command, string buscar, int? idDuenio, EstadoReserva? estado)
  {
    command.Parameters.AddWithValue("@idDuenio", (object?)idDuenio ?? DBNull.Value);
    command.Parameters.AddWithValue("@estado", estado.HasValue ? (object)(byte)estado.Value : DBNull.Value);
    command.Parameters.AddWithValue("@buscar", buscar);
    command.Parameters.AddWithValue("@like", "%" + buscar + "%");
  }

  public int ContarDeNegocios(string buscar, int? idDuenio, EstadoReserva? estado)
  {
    using var connection = new MySqlConnection(_connectionString);
    var sql = @"SELECT COUNT(*)
              FROM reservas r
              INNER JOIN negocios n  ON n.IdNegocio  = r.IdNegocio
              INNER JOIN recursos rc ON rc.IdRecurso = r.IdRecurso
              INNER JOIN usuarios u  ON u.IdUsuario  = r.IdUsuario
              WHERE " + FiltroGestion;
    using var command = new MySqlCommand(sql, connection);
    AgregarFiltrosGestion(command, buscar, idDuenio, estado);

    connection.Open();
    return Convert.ToInt32(command.ExecuteScalar());
  }

  public List<ReservaGestion> ListarDeNegocios(string buscar, int? idDuenio, EstadoReserva? estado, int pagina, int tamano)
  {
    var lista = new List<ReservaGestion>();

    using var connection = new MySqlConnection(_connectionString);
    var sql = @"SELECT r.IdReserva, r.Fecha, r.HoraInicio, r.HoraFin, r.Estado, r.Precio,
                     rc.Nombre AS RecursoNombre, n.Nombre AS NegocioNombre,
                     CONCAT(u.Nombre, ' ', u.Apellido) AS ClienteNombre,
                     u.Telefono AS ClienteTelefono
              FROM reservas r
              INNER JOIN negocios n  ON n.IdNegocio  = r.IdNegocio
              INNER JOIN recursos rc ON rc.IdRecurso = r.IdRecurso
              INNER JOIN usuarios u  ON u.IdUsuario  = r.IdUsuario
              WHERE " + FiltroGestion + @"
              ORDER BY r.Fecha DESC, r.HoraInicio DESC
              LIMIT @tamano OFFSET @salto";
    using var command = new MySqlCommand(sql, connection);
    AgregarFiltrosGestion(command, buscar, idDuenio, estado);
    command.Parameters.AddWithValue("@tamano", tamano);
    command.Parameters.AddWithValue("@salto", (pagina - 1) * tamano);

    connection.Open();
    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
      lista.Add(new ReservaGestion
      {
        IdReserva = reader.GetInt32("IdReserva"),
        Fecha = reader.GetDateTime("Fecha"),
        HoraInicio = reader.GetTimeSpan("HoraInicio"),
        HoraFin = reader.GetTimeSpan("HoraFin"),
        Estado = (EstadoReserva)reader.GetInt32("Estado"),
        Precio = reader.GetDecimal("Precio"),
        RecursoNombre = reader.GetString("RecursoNombre"),
        NegocioNombre = reader.GetString("NegocioNombre"),
        ClienteNombre = reader.GetString("ClienteNombre"),
        ClienteTelefono = reader.IsDBNull(reader.GetOrdinal("ClienteTelefono")) ? null : reader.GetString("ClienteTelefono")
      });
    }
    return lista;
  }

  public int Crear(Reserva reserva)
  {
    using var connection = new MySqlConnection(_connectionString);
    var sql = "INSERT INTO reservas (IdUsuario, IdRecurso, IdNegocio, Fecha, HoraInicio, HoraFin, Estado, Precio, Observaciones, Comprobante) " +
              "VALUES (@idUsuario, @idRecurso, @idNegocio, @fecha, @horaInicio, @horaFin, @estado, @precio, @observaciones, @comprobante);" +
              "SELECT LAST_INSERT_ID();";
    using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@idUsuario", reserva.IdUsuario);
    command.Parameters.AddWithValue("@idRecurso", reserva.IdRecurso);
    command.Parameters.AddWithValue("@idNegocio", reserva.IdNegocio);
    command.Parameters.AddWithValue("@fecha", reserva.Fecha.Date);
    command.Parameters.AddWithValue("@horaInicio", reserva.HoraInicio);
    command.Parameters.AddWithValue("@horaFin", reserva.HoraFin);
    command.Parameters.AddWithValue("@estado", (byte)reserva.Estado);
    command.Parameters.AddWithValue("@precio", reserva.Precio);
    command.Parameters.AddWithValue("@observaciones", (object?)reserva.Observaciones ?? DBNull.Value);
    command.Parameters.AddWithValue("@comprobante", (object?)reserva.Comprobante ?? DBNull.Value);

    connection.Open();
    return Convert.ToInt32(command.ExecuteScalar());
  }

  public int CambiarEstado(int id, EstadoReserva estado)
  {
    using var connection = new MySqlConnection(_connectionString);
    var sql = "UPDATE reservas SET Estado = @estado WHERE IdReserva = @id";
    using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@id", id);
    command.Parameters.AddWithValue("@estado", (byte)estado);

    connection.Open();
    return command.ExecuteNonQuery();
  }
}


