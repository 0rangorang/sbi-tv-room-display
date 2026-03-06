using Microsoft.AspNetCore.Mvc;
using MeetingRoomDashboard.Services;

namespace MeetingRoomDashboard.Controllers
{
    // Controller bertugas menerima request dari user
    // Lalu mengirim data ke View
    public class DashboardController : Controller
    {
        private readonly IBookingService _bookingService;

        // Constructor Injection
        // ASP.NET otomatis mengisi parameter ini dari DI container
        public DashboardController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // Method ini akan dipanggil saat user akses /Dashboard
        public IActionResult Index()
        {
            // Ambil data dari service
            var rooms = _bookingService.GetRooms();

            // Kirim data ke View
            return View(rooms);
        }
    }
}
