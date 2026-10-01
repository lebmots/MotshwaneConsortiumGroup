namespace MotshwaneConsortiumGroup.Models;
public class Customer
{
    public int Id { get; set; }
    public int UserId { get; set; }

    // Name and Email are now part of the User model, so we only need to keep Phone here.
    //public string Name { get; set; } = "";
    //public string Email { get; set; } = ""; 
    public string Phone { get; set; } = "";
    public User User { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
