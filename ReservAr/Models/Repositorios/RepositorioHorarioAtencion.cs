using MySqlConnector;

namespace ReservAr.Models.Repositorios;

public class RepositorioHorarioAtencion
{
    private readonly string _connectionString;

    public RepositorioHorarioAtencion(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public List<HorarioAtencion> ListarPorRecurso(int idRecurso)
    {
        var lista = new List<HorarioAtencion>();

        using var connection = new MySqlConnection(_connectionString);
        var sql = "SELECT IdHorarioAtencion, IdRecurso, DiaSemana, HoraDesde, HoraHasta FROM horarios_atencion WHERE IdRecurso = @idRecurso ORDER BY DiaSemana, HoraDesde";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@idRecurso", idRecurso);

        connection.Open();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            lista.Add(new HorarioAtencion
            {
                IdHorarioAtencion = reader.GetInt32("IdHorarioAtencion"),
                IdRecurso = reader.GetInt32("IdRecurso"),
                DiaSemana = reader.GetInt32("DiaSemana"),
                HoraDesde = reader.GetTimeSpan("HoraDesde"),
                HoraHasta = reader.GetTimeSpan("HoraHasta"),
            });
        }
        return lista;
    }


    public int Crear(HorarioAtencion horarioAtencion)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = "INSERT INTO horarios_atencion (IdRecurso, DiaSemana, HoraDesde, HoraHasta) " +
                  "VALUES (@IdRecurso, @DiaSemana, @HoraDesde, @HoraHasta); " +
                  "SELECT LAST_INSERT_ID();";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@IdRecurso", horarioAtencion.IdRecurso);
        command.Parameters.AddWithValue("@DiaSemana", horarioAtencion.DiaSemana);
        command.Parameters.AddWithValue("@HoraDesde", horarioAtencion.HoraDesde);
        command.Parameters.AddWithValue("@HoraHasta", horarioAtencion.HoraHasta);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public int Modificar(HorarioAtencion horarioAtencion)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = "UPDATE horarios_atencion " +
                  "SET DiaSemana = @DiaSemana, HoraDesde = @HoraDesde, HoraHasta = @HoraHasta " +
                  "WHERE IdHorarioAtencion = @id";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@DiaSemana", horarioAtencion.DiaSemana);
        command.Parameters.AddWithValue("@HoraDesde", horarioAtencion.HoraDesde);
        command.Parameters.AddWithValue("@HoraHasta", horarioAtencion.HoraHasta);

        connection.Open();
        return command.ExecuteNonQuery();
    }

    public int Eliminar(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = "DELETE FROM horarios_atencion WHERE IdHorarioAtencion = @id";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        connection.Open();
        return command.ExecuteNonQuery();
    }
}