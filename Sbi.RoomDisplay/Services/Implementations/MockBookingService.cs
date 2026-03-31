using Sbi.RoomDisplay.Models;
using Sbi.RoomDisplay.Services.Interfaces;

namespace Sbi.RoomDisplay.Services.Implementations
{
    // Mock data untuk development
    // Simulasi data dari sistem e-booking

    public class MockBookingService : IBookingService
    {
        public List<Booking> GetBookingsByRoom(string roomCode)
        {
            return new List<Booking>
            {
                /*new Booking
                {
                    Activity = "TEST ACTIVE MEETING",
                    StartTime = SystemClock.Now.AddMinutes(-30),
                    EndTime = SystemClock.Now.AddMinutes(30),
                    RoomCode = "NAR-R01"
                },*/
                new Booking
                {
                    RoomCode = roomCode,
                    Division = "Dev OP IT",
                    Activity = "OS Installation",
                    PIC = "Rizky",
                    StartTime = DateTime.Today.AddHours(7),
                    EndTime = DateTime.Today.AddHours(10)
                },
                new Booking
                {
                    RoomCode = roomCode,
                    Division = "Finance",
                    Activity = "Audit Internal",
                    PIC = "Budi",
                    StartTime = DateTime.Today.AddHours(11),
                    EndTime = DateTime.Today.AddHours(13)
                }
            };
        }
    }
}