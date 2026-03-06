using MeetingRoomDashboard.Models;

namespace MeetingRoomDashboard.Services
{
    // Service ini nanti digunakan untuk ambil data dari SBI e-booking
    public class ApiBookingService : IBookingService
    {
        public List<MeetingRoom> GetRooms()
        {
            // TODO:
            // 1. Panggil endpoint 10.227.1.1/all.php
            // 2. Parse JSON response
            // 3. Mapping ke List<MeetingRoom>
            // 4. Return hasilnya

            // Untuk sekarang return kosong
            return new List<MeetingRoom>();
        }
    }
}
