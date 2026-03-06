using MeetingRoomDashboard.Models;

namespace MeetingRoomDashboard.Services
{
    // Class ini implement IBookingService
    // Digunakan sementara sebelum terhubung ke API asli
    public class FakeBookingService : IBookingService
    {
        public List<MeetingRoom> GetRooms()
        {
            // Data dummy untuk simulasi
            return new List<MeetingRoom>
            {
                new MeetingRoom
                {
                    RoomName = "Ruang Meeting A",
                    CurrentUser = "HR Team",
                    Activity = "Recruitment Discussion",
                    Time = "09:00 - 10:00",
                    NextSchedule = "10:30 - Finance Meeting"
                },
                new MeetingRoom
                {
                    RoomName = "Ruang Meeting B",
                    CurrentUser = "-", // Tanda kosong
                    Activity = "-",
                    Time = "-",
                    NextSchedule = "11:00 - IT Planning"
                },
                new MeetingRoom
                {
                    RoomName = "Ruang Meeting C",
                    CurrentUser = "IT Team",
                    Activity = "System Sync",
                    Time = "08:30 - 09:30",
                    NextSchedule = "10:00 - Maintenance"
                }
            };
        }
    }
}
