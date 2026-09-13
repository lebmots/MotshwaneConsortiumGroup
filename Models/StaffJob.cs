namespace MotshwaneConsortiumGroup.Models;
public class StaffJob
{
    public int Id { get; set; }
    public string Reference { get; set; } = "";
    public string Service { get; set; } = "";
    public string Location { get; set; } = "";
    public DateTime Date { get; set; }
    public string Status { get; set; } = "Assigned";
}
