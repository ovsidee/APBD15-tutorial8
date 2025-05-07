namespace APBD_Tutorial8.Models.DTO_s;

public class TripDto
{
    public int IdTrip { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public string DateRange { get; set; }
    public int MaxPeople { get; set; }
    public List<string> Countries { get; set; }
}
