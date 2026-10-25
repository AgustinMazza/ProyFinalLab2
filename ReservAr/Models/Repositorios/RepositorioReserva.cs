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


