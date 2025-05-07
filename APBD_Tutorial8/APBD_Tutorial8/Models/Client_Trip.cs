namespace APBD_Tutorial8.Models;

public class Client_Trip
{
    public int idClient { get; set; }
    public int idTrip { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? PaymentDate { get; set; }
}