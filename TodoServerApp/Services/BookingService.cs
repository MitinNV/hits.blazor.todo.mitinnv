using System.Text;
using System.Text.Json;
using TodoServerApp.Models;

namespace TodoServerApp.Services;

public class BookingService
{
    private readonly string tablesPath;
    private readonly string reservationsPath;

    public BookingService()
    {
        var projectRoot = AppContext.BaseDirectory;
        var dataDir = Path.Combine(projectRoot, "..", "..", "..", "Data");
        tablesPath = Path.Combine(dataDir, "tables.json");
        reservationsPath = Path.Combine(dataDir, "reservations.json");
    }

    public List<Table> GetTables()
    {
        if (!File.Exists(tablesPath))
            return new List<Table>();

        var json = File.ReadAllText(tablesPath, Encoding.UTF8);
        if (string.IsNullOrWhiteSpace(json))
            return new List<Table>();

        try
        {
            var tables = JsonSerializer.Deserialize<List<Table>>(json);
            return tables ?? new List<Table>();
        }
        catch
        {
            return new List<Table>();
        }
    }

    public List<Reservation> GetReservations()
    {
        if (!File.Exists(reservationsPath))
            return new List<Reservation>();

        var json = File.ReadAllText(reservationsPath, Encoding.UTF8);
        if (string.IsNullOrWhiteSpace(json))
            return new List<Reservation>();

        try
        {
            var reservations = JsonSerializer.Deserialize<List<Reservation>>(json);
            return reservations ?? new List<Reservation>();
        }
        catch
        {
            return new List<Reservation>();
        }
    }

    // Проверка доступности с учётом 1 часа
    public bool IsAvailable(int tableId, DateTime dateTime)
    {
        return !GetReservations().Any(r =>
            r.TableId == tableId &&
            Math.Abs((r.DateTime - dateTime).TotalMinutes) < 60
        );
    }

    public string BookTable(int tableId, string clientName, DateTime dateTime)
    {
        if (string.IsNullOrWhiteSpace(clientName))
            return "Имя клиента не может быть пустым.";

        var reservations = GetReservations();

        if (!IsAvailable(tableId, dateTime))
            return "Столик уже забронирован в этот час.";

        var newRes = new Reservation
        {
            Id = reservations.Count > 0 ? reservations.Max(r => r.Id) + 1 : 1,
            TableId = tableId,
            ClientName = clientName,
            DateTime = dateTime
        };

        reservations.Add(newRes);

        var dir = Path.GetDirectoryName(reservationsPath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(reservationsPath,
            JsonSerializer.Serialize(reservations, new JsonSerializerOptions { WriteIndented = true }),
            Encoding.UTF8);

        // Формируем уведомление с интервалом 1 час
        DateTime endTime = dateTime.AddHours(1);
        string message = $"Столик забронирован для {clientName} на {dateTime:HH:mm} - {endTime:HH:mm} {dateTime:dd.MM.yyyy}";
        return message;
    }

    public void DeleteReservation(int id)
    {
        var reservations = GetReservations();
        var res = reservations.FirstOrDefault(r => r.Id == id);
        if (res != null)
        {
            reservations.Remove(res);
            File.WriteAllText(reservationsPath,
                JsonSerializer.Serialize(reservations, new JsonSerializerOptions { WriteIndented = true }),
                Encoding.UTF8);
        }
    }
}
