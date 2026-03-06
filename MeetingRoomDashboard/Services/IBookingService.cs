using MeetingRoomDashboard.Models;

namespace MeetingRoomDashboard.Services
{
    // Interface ini mendefinisikan kontrak
    // Artinya: siapapun yang implement ini harus punya method GetRooms()
    public interface IBookingService
    {
        // Method untuk mengambil daftar ruangan
        List<MeetingRoom> GetRooms();
    }
}
