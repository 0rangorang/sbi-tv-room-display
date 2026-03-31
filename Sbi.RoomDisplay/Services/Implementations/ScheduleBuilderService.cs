using Sbi.RoomDisplay.Models;

namespace Sbi.RoomDisplay.Services.Implementations
{
    public class ScheduleBuilderService
    {
        private const int TimelineStartHour = 7;
        private const int TimelineStartMinute = 0;

        private const int TimelineEndHour = 18;
        private const int TimelineEndMinute = 30;

        private const int SlotMinutes = 30;

        public List<HourlySlot> BuildSlots(List<Booking> bookings)
        {
            bookings ??= new List<Booking>();

            var orderedBookings = bookings
                .Where(b => b != null && b.EndTime > b.StartTime)
                .OrderBy(b => b.StartTime)
                .ToList();

            var baseDate = orderedBookings.Count > 0
                ? orderedBookings.Min(b => b.StartTime).Date
                : SystemClock.Now.Date;

            var now = SystemClock.Now;
            var slots = new List<HourlySlot>();

            var timelineStart = baseDate
                .AddHours(TimelineStartHour)
                .AddMinutes(TimelineStartMinute);

            var timelineEnd = baseDate
                .AddHours(TimelineEndHour)
                .AddMinutes(TimelineEndMinute);

            for (var slotTime = timelineStart;
                 slotTime <= timelineEnd;
                 slotTime = slotTime.AddMinutes(SlotMinutes))
            {
                var booking = orderedBookings.FirstOrDefault(b =>
                    slotTime >= b.StartTime &&
                    slotTime < b.EndTime);

                if (booking == null)
                {
                    slots.Add(BuildAvailableSlot(slotTime, now));
                }
                else
                {
                    slots.Add(BuildBookedSlot(slotTime, booking, now));
                }
            }

            return slots;
        }

        private static HourlySlot BuildAvailableSlot(DateTime slotTime, DateTime now)
        {
            return new HourlySlot
            {
                Time = slotTime,
                Description = "Available",
                Status = ResolveAvailableSlotStatus(slotTime, now)
            };
        }

        private static HourlySlot BuildBookedSlot(DateTime slotTime, Booking booking, DateTime now)
        {
            return new HourlySlot
            {
                Time = slotTime,
                Description = booking.DisplayTitle,
                Status = ResolveBookedSlotStatus(booking, now)
            };
        }

        private static SlotStatus ResolveAvailableSlotStatus(DateTime slotTime, DateTime now)
        {
            var nextSlotTime = slotTime.AddMinutes(SlotMinutes);

            if (now >= nextSlotTime)
                return SlotStatus.Past;

            return SlotStatus.Available;
        }

        private static SlotStatus ResolveBookedSlotStatus(
            Booking booking,
            DateTime now)
        {
            // Semua row milik meeting yang sedang berlangsung
            // dianggap active, supaya panel kanan konsisten dengan panel kiri.
            if (now >= booking.StartTime && now < booking.EndTime)
                return SlotStatus.Active;

            if (now >= booking.EndTime)
                return SlotStatus.Past;

            return SlotStatus.Future;
        }
    }
}