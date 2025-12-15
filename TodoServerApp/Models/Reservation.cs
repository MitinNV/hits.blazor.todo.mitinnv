namespace TodoServerApp.Models;

public class Reservation
{
    public int Id { get; set; }
    public int TableId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
}
