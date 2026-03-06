//Inject semua services
using Microsoft.AspNetCore.Mvc;
using Sbi.RoomDisplay.Models;
using Sbi.RoomDisplay.Services.Interfaces;
using Sbi.RoomDisplay.Services.Implementations;

namespace Sbi.RoomDisplay.Controllers
{
    [Route("")]
    public class TvController : Controller
    {

        private static readonly Dictionary<string, string> RoomNames =
            new()
            {
                { "FICO1", "FICO 1" },
                { "FICO2", "FICO 2" },
                { "FICO3", "FICO 3" }
            };

        private readonly IBookingService _bookingService;
        private readonly ScheduleBuilderService _scheduleBuilder;

        public TvController(
            IBookingService bookingService,
            ScheduleBuilderService scheduleBuilder)
        {
            _bookingService = bookingService;
            _scheduleBuilder = scheduleBuilder;
        }


        // Halaman utama TV
        // Contoh akses:
        // /localhost:xxxx/FICO1
        [HttpGet("{code}")]
        public IActionResult Room(string code)
        {
            if (!RoomNames.ContainsKey(code))
                return NotFound();
            var name = RoomNames[code];

            var bookings = _bookingService.GetBookingsByRoom(code);

            var slots = _scheduleBuilder.BuildSlots(bookings);

            var current = bookings.FirstOrDefault(b =>
                SystemClock.Now >= b.StartTime &&
                SystemClock.Now <= b.EndTime);

            var vm = new RoomScheduleViewModel
            {
                RoomCode = code,
                RoomName = name,
                CurrentBooking = current,
                Slots = slots
            };

            return View(vm);
        }


        [HttpGet("")]
        public IActionResult Index()
        {
            return Redirect("/FICO1");
        }
        
        // Endpoint untuk AJAX polling
        // Return JSON saja (tanpa layout)

        //Kenapa kita return anonymous object? Karena:
        // 1. JSON ringan
        // 2. Tidak kirim property tidak perlu
        // 3. Clean separation dari ViewModel HTML

        [HttpGet("schedule/{code}")]
        public IActionResult GetSchedule(string code)
        {
            if (string.IsNullOrEmpty(code))
                return BadRequest();

           var bookings = _bookingService.GetBookingsByRoom(code);

            var slots = _scheduleBuilder.BuildSlots(bookings);

            var current = bookings.FirstOrDefault(b =>
                SystemClock.Now >= b.StartTime &&
                SystemClock.Now <= b.EndTime);

            var result = new
            {
                Current = current,
                Slots = slots.Select(s => new
                {
                    Time = s.Time.ToString("HH:mm"),
                    Description = s.Description,
                    Status = s.Status.ToString()
                })
            };

            return Json(result);
        }

        [HttpGet("time")]
        public IActionResult GetServerTime()
        {
            return Json(new
            {
                now = SystemClock.Now
            });
        }
    }
}

