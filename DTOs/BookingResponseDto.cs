namespace MotshwaneConsortiumGroup.DTOs
{
    public class BookingResponseDto
    {
        public int Id { get; set; }

        public string Reference { get; set; } = "";

        public int CustomerId { get; set; }

        public string CustomerName { get; set; } = "";

        public int UnitId { get; set; }

        public string UnitName { get; set; } = "";

        public DateTime BookingDate { get; set; }

        public string Location { get; set; } = "";

        public string? Notes { get; set; }

        public string Status { get; set; } = "";

        public string PaymentStatus { get; set; } = "";
    }
}
