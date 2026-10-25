using System.ComponentModel.DataAnnotations.Schema;

namespace MotshwaneConsortiumGroup.Models;

public class StaffJob
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int? StaffUserId { get; set; }
    public string Status { get; set; } = JobStatus.Assigned;
    public string? Notes { get; set; }
    public Booking Booking { get; set; } = null!;

    // Convenience accessors so the existing Staff views/controller (written against a flatter
    // model, before jobs were linked to a real Booking row) keep working unchanged.
    [NotMapped] public string Reference => Booking?.Reference ?? "";
    [NotMapped] public string Service => Booking?.Service ?? "";
    [NotMapped] public string Location => Booking?.Location ?? "";
    [NotMapped] public DateTime Date => Booking?.BookingDate ?? default;
    [NotMapped] public int StaffId { get => StaffUserId ?? 0; set => StaffUserId = value; }
}
