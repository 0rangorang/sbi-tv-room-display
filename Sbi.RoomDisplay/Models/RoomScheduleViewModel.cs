// ViewModel khusus untuk halaman TV
// Bukan domain model

// Kenapa pakai ViewModel? Karena:
// 1. Jangan kirim domain model mentah ke View
// 2. View hanya menerima data yang sudah siap tampil
// 3. Clean separation

namespace Sbi.RoomDisplay.Models
{
    public class RoomScheduleViewModel
    {
        // Nama ruangan untuk header
        public string RoomName { get; set; }

        // Kode ruangan (untuk API lookup)
        public string RoomCode { get; set; }

        // Booking yang sedang aktif sekarang
        public Booking CurrentBooking { get; set; }

        // List slot timeline
        public List<HourlySlot> Slots { get; set; }
    }
}