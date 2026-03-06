using Sbi.RoomDisplay.Models;
using Sbi.RoomDisplay.Services.Interfaces;

namespace Sbi.RoomDisplay.Services.Implementations
{
    /*public class ApiBookingService : IBookingService
    {
        private readonly HttpClient _http;

        public ApiBookingService(HttpClient http)
        {
            _http = http;
        }

        public List<Booking> GetBookingsByRoom(string roomCode)
        {
            var response =
                _http.GetFromJsonAsync<List<BookingDto>>(
                    $"http://10.227.1.18/all.php?room={roomCode}"
                ).Result;

            return response.Select(r => new Booking
            {
                RoomCode = roomCode,
                Division = r.division,
                Activity = r.activity,
                PIC = r.pic,
                StartTime = r.start,
                EndTime = r.end
            }).ToList();
        }
    }*/
}