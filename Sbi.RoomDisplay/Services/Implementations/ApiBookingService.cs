using HtmlAgilityPack;
using Sbi.RoomDisplay.Models;
using Sbi.RoomDisplay.Services.Interfaces;

namespace Sbi.RoomDisplay.Services.Implementations
{
    public class ApiBookingService : IBookingService
    {
        private readonly HttpClient _http;

        public ApiBookingService(HttpClient http)
        {
            _http = http;
        }

        public List<Booking> GetBookingsByRoom(string roomCode)
        {
            // 1️⃣ ambil HTML e-booking
            var html =
                _http.GetStringAsync("http://10.227.1.18/all.php")
                     .Result;

            // 2️⃣ load HTML
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var bookings = new List<Booking>();

            // 3️⃣ cari semua blok meeting
            var nodes = doc.DocumentNode
                .SelectNodes("//td[contains(@class,'internal') or contains(@class,'external')]");

            if (nodes == null)
                return bookings;

            // 4️⃣ convert jadi booking sederhana dulu
            foreach (var node in nodes)
            {
                var text = node.InnerText.Trim();

                if (string.IsNullOrWhiteSpace(text))
                    continue;

                bookings.Add(new Booking
                {
                    RoomCode = roomCode,
                    Division = "E-Booking",
                    Activity = text,
                    PIC = "-",
                    StartTime = DateTime.Today.AddHours(8),
                    EndTime = DateTime.Today.AddHours(9)
                });
            }

            return bookings;
        }
    }
}