namespace APBD_Tutorial8.Models.DTO_s;

public class ClientTripDto
{
    public int IdTrip { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public string DateRange => $"{DateFrom:yyyy-MM-dd} - {DateTo:yyyy-MM-dd}";
    public DateTime RegisteredAt { get; set; }
    public DateTime? PaymentDate { get; set; }
}
