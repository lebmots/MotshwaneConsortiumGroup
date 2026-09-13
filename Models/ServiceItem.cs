namespace MotshwaneConsortiumGroup.Models;
public class ServiceItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal PriceFrom { get; set; }
    public bool Available { get; set; }
}
