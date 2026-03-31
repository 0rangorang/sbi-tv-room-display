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
                { "FICO3", "FICO 3" },

                { "BOD15", "BOD ROOM" },
                { "MEDB15", "MEDIUM ROOM B" },
                { "SMALLM115", "SMALL M1" },
                { "SMALLM215", "SMALL M2" },
                { "VSTDIR15", "VST DIR" },
                
                { "BIGM116", "BIG M1" },
                { "BIGM216", "BIG M2" }
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
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [HttpGet("{code}")]
        public IActionResult Room(string code)
        {
            var normalizedCode = NormalizeRoomCode(code);

            if (!TryBuildRoomViewModel(normalizedCode, out var vm))
                return NotFound();

            return View(vm);
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return Redirect("/FICO1");
        }

        // Endpoint untuk AJAX polling
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [HttpGet("schedule/{code}")]
        public IActionResult GetSchedule(string code)
        {
            var normalizedCode = NormalizeRoomCode(code);

            if (string.IsNullOrWhiteSpace(normalizedCode))
                return BadRequest();

            if (!TryBuildRoomViewModel(normalizedCode, out var vm))
                return NotFound();

            return Json(BuildScheduleResponse(vm));
        }

        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [HttpGet("time")]
        public IActionResult GetServerTime()
        {
            return Json(new
            {
                now = SystemClock.Now
            });
        }

        private bool TryBuildRoomViewModel(string code, out RoomScheduleViewModel vm)
        {
            vm = null;

            if (string.IsNullOrWhiteSpace(code))
                return false;

            if (!RoomNames.TryGetValue(code, out var roomName))
                return false;

            var bookings = _bookingService.GetBookingsByRoom(code)
                ?.Where(b => b != null)
                .OrderBy(b => b.StartTime)
                .ToList()
                ?? new List<Booking>();

            var now = SystemClock.Now;

            var current = GetCurrentBooking(bookings, now);
            var next = GetNextBooking(bookings, now);
            var slots = _scheduleBuilder.BuildSlots(bookings);

            var fetchState = ApiBookingService.GetRoomFetchState(code);
            var (dataStatusClass, dataStatusText, dataStatusDetail) =
                BuildDataStatus(fetchState);

            vm = new RoomScheduleViewModel
            {
                RoomCode = code,
                RoomName = roomName,
                CurrentBooking = current,
                NextBooking = next,
                Slots = slots,
                ServerNow = now,
                DataStatusClass = dataStatusClass,
                DataStatusText = dataStatusText,
                DataStatusDetail = dataStatusDetail,
                LastSuccessfulFetchAt = fetchState.LastSuccessAt
            };

            return true;
        }

        private static Booking GetCurrentBooking(List<Booking> bookings, DateTime now)
        {
            return bookings.FirstOrDefault(b =>
                now >= b.StartTime &&
                now < b.EndTime);
        }

        private static Booking GetNextBooking(List<Booking> bookings, DateTime now)
        {
            return bookings
                .Where(b => b.StartTime > now)
                .OrderBy(b => b.StartTime)
                .FirstOrDefault();
        }

        private static object BuildScheduleResponse(RoomScheduleViewModel vm)
        {
            return new
            {
                RoomCode = vm.RoomCode,
                RoomName = vm.RoomName,
                ServerNow = vm.ServerNow,
                IsRoomInUse = vm.IsRoomInUse,
                DataStatusClass = vm.DataStatusClass,
                DataStatusText = vm.DataStatusText,
                DataStatusDetail = vm.DataStatusDetail,
                LastSuccessfulFetchAt = vm.LastSuccessfulFetchAt,
                Current = vm.CurrentBooking,
                Next = vm.NextBooking,
                Slots = vm.Slots.Select(s => new
                {
                    Time = s.Time.ToString("HH:mm"),
                    Description = s.Description,
                    Status = s.Status.ToString()
                })
            };
        }

        private static (string CssClass, string Text, string Detail) BuildDataStatus(
            ApiBookingService.RoomFetchState fetchState)
        {
            if (fetchState.UsedFallbackData && fetchState.LastSuccessAt.HasValue)
            {
                return (
                    "stale",
                    "STALE DATA",
                    $"Last live data: {fetchState.LastSuccessAt.Value:HH:mm:ss}"
                );
            }

            if (!fetchState.HasAnyData && !string.IsNullOrWhiteSpace(fetchState.ErrorMessage))
            {
                return (
                    "error",
                    "NO DATA",
                    "E-booking unavailable"
                );
            }

            return (
                "live",
                "LIVE DATA",
                string.Empty
            );
        }

        private static string NormalizeRoomCode(string code)
        {
            return string.IsNullOrWhiteSpace(code)
                ? string.Empty
                : code.Trim().ToUpperInvariant();
        }
    }
}