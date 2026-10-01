namespace MotshwaneConsortiumGroup.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending";
        public string? ProofOfPaymentPath { get; set; }
        public DateTime? PaymentDate { get; set; }
        public Booking Booking { get; set; } = null!;
    }
}
