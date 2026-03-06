//Kenapa dipisah dari controller? Karena:
// 1. Controller hanya orchestrator
// 2. Logic bisa di-test
// 3. Lebih bersih

using Sbi.RoomDisplay.Models;
using Sbi.RoomDisplay.Services.Interfaces;

namespace Sbi.RoomDisplay.Services.Implementations
{
    // Bertugas membangun timeline slot dari data booking
    // Logic utama sistem ada di sini

    public class ScheduleBuilderService
    {
        public List<HourlySlot> BuildSlots(
            List<Booking> bookings)
        {
            var slots = new List<HourlySlot>();

            var now = SystemClock.Now;

            // Buat slot dari jam 6 pagi sampai 8 malam
            for (int hour = 6; hour <= 20; hour++)
            {
                var slotTime = DateTime.Today.AddHours(hour);

                // Cek apakah ada booking di jam ini
                var booking = bookings
                    .FirstOrDefault(b =>
                        slotTime >= b.StartTime &&
                        slotTime < b.EndTime);

                var slot = new HourlySlot
                {
                    Time = slotTime
                };

                if (booking == null)
                {
                    slot.Description = "Available";

                    if (slotTime < now)
                        slot.Status = SlotStatus.Past;
                    else
                        slot.Status = SlotStatus.Available;
                }
                else
                {
                    slot.Description = booking.DisplayTitle;

                    if (now >= booking.StartTime &&
                        now <= booking.EndTime)
                    {
                        slot.Status = SlotStatus.Active;
                    }
                    else if (slotTime < now)
                    {
                        slot.Status = SlotStatus.Past;
                    }
                    else
                    {
                        slot.Status = SlotStatus.Future;
                    }
                }

                slots.Add(slot);
            }

            return slots;
        }
    }
}