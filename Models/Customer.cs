using System.ComponentModel.DataAnnotations.Schema;

namespace MotshwaneConsortiumGroup.Models;

public class Customer
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Phone { get; set; } = "";
    public User User { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    // Convenience accessors so existing views/controllers that expect Customer.Name/Email
    // (written before accounts existed) keep working without edits — these read from the
    // linked User, they're never written to or stored as their own columns.
    [NotMapped] public string Name => User?.FullName ?? "";
    [NotMapped] public string Email => User?.Email ?? "";
}
