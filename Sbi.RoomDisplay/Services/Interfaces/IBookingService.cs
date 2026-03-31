// Kenapa pakai interface? Karena:
// 1. Bisa ganti dari Mock → API tanpa ubah controller
// 2. Enterprise standard

using Sbi.RoomDisplay.Models;

namespace Sbi.RoomDisplay.Services.Interfaces
{
    // Interface untuk abstraction layer
    // Nanti bisa diganti implementasi API asli

    public interface IBookingService
    {
        // Ambil semua booking hari ini berdasarkan room code
        List<Booking> GetBookingsByRoom(string roomCode);
    }
}