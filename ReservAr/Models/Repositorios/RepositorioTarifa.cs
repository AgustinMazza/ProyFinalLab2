using MySqlConnector;
using ReservAr.Models;


namespace ReservAr.Models.Repositorios;

public class RepositorioTarifa
{
    private readonly string _connectionString;

    public RepositorioTarifa(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }


    public List<Tarifa> ListarPorRecurso(int idRecurso)
    {

        var lista = new List<Tarifa>();

        using var connection = new MySqlConnection(_connectionString);
        var sql = @"SELECT IdTarifa, IdRecurso, Nombre, HoraDesde, HoraHasta, PrecioHora, Activo FROM tarifas 
        WHERE IdRecurso = @idRecurso 
        ORDER BY HoraDesde";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@idRecurso", idRecurso);

        connection.Open();
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            lista.Add(new Tarifa
            {
                IdTarifa = reader.GetInt32("IdTarifa"),
                IdRecurso = reader.GetInt32("IdRecurso"),
                Nombre = reader.GetString("Nombre"),
                HoraDesde = reader.GetTimeSpan("HoraDesde"),
                HoraHasta = reader.GetTimeSpan("HoraHasta"),
                PrecioHora = reader.GetDecimal("PrecioHora"),
                Activo = reader.GetBoolean("Activo")
            });
        }
        return lista;




    }


    public Tarifa? ObtenerPorId(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = "SELECT IdTarifa, IdRecurso, Nombre, HoraDesde, HoraHasta, PrecioHora, Activo FROM tarifas WHERE IdTarifa = @id";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);


        connection.Open();
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new Tarifa
            {
                IdTarifa = reader.GetInt32("IdTarifa"),
                IdRecurso = reader.GetInt32("IdRecurso"),
                Nombre = reader.GetString("Nombre"),
                HoraDesde = reader.GetTimeSpan("HoraDesde"),
                HoraHasta = reader.GetTimeSpan("HoraHasta"),
                PrecioHora = reader.GetDecimal("PrecioHora"),
                Activo = reader.GetBoolean("Activo")
            };



        }
        return null;

    }

    public int Crear(Tarifa tarifa)
    {

        using var connection = new MySqlConnection(_connectionString);
        var sql = "INSERT INTO tarifas (IdRecurso, Nombre, HoraDesde, HoraHasta ,PrecioHora, Activo) " +
        "VALUES (@idrecurso, @nombre, @horadesde, @horahasta, @preciohora, @activo); " +
        "SELECT LAST_INSERT_ID();";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@idrecurso", tarifa.IdRecurso);
        command.Parameters.AddWithValue("@nombre", tarifa.Nombre);
        command.Parameters.AddWithValue("@horaDesde", tarifa.HoraDesde);
        command.Parameters.AddWithValue("@horaHasta", tarifa.HoraHasta);
        command.Parameters.AddWithValue("@precioHora", tarifa.PrecioHora);
        command.Parameters.AddWithValue("@activo", tarifa.Activo);

        connection.Open();
        return Convert.ToInt32(command.ExecuteScalar());



    }


    public int Modificar(Tarifa tarifa)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = "UPDATE tarifas " +
        "SET Nombre = @nombre, HoraDesde = @horadesde, HoraHasta = @horahasta, PrecioHora = @preciohora, Activo = @activo " +
        "WHERE IdTarifa = @id";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", tarifa.IdTarifa);
        command.Parameters.AddWithValue("@nombre", tarifa.Nombre);
        command.Parameters.AddWithValue("@horaDesde", tarifa.HoraDesde);
        command.Parameters.AddWithValue("@horaHasta", tarifa.HoraHasta);
        command.Parameters.AddWithValue("@precioHora", tarifa.PrecioHora);
        command.Parameters.AddWithValue("@activo", tarifa.Activo);

        connection.Open();
        return command.ExecuteNonQuery();


    }

    public int Eliminar(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = "UPDATE tarifas SET Activo = 0 WHERE IdTarifa = @id";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        connection.Open();
        return command.ExecuteNonQuery();
    }

    public int Reactivar(int id)
    {
        using var connection = new MySqlConnection(_connectionString);
        var sql = "UPDATE tarifas SET Activo = 1 WHERE IdTarifa = @id";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        connection.Open();
        return command.ExecuteNonQuery();
    }




















}