namespace CarRental.Application.Bookings.Dtos;

public class BookingDto
{
    public int Id { get; set; }
    public int CarId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
