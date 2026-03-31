namespace Sbi.RoomDisplay.Models
{
    // ViewModel khusus untuk halaman TV
    // Bukan domain model
    public class RoomScheduleViewModel
    {
        // Nama ruangan untuk header
        public string RoomName { get; set; } = string.Empty;

        // Kode ruangan (untuk API lookup)
        public string RoomCode { get; set; } = string.Empty;

        // Booking yang sedang aktif sekarang
        public Booking CurrentBooking { get; set; }

        // Booking berikutnya setelah sekarang
        public Booking NextBooking { get; set; }

        // List slot timeline
        public List<HourlySlot> Slots { get; set; } = new();

        // Waktu server saat view dibentuk
        public DateTime ServerNow { get; set; }

        // Status sumber data: live / stale / error
        public string DataStatusClass { get; set; } = "live";

        // Teks badge sumber data
        public string DataStatusText { get; set; } = "LIVE DATA";

        // Detail kecil untuk status sumber data
        public string DataStatusDetail { get; set; } = string.Empty;

        // Kapan terakhir kali data live berhasil didapat
        public DateTime? LastSuccessfulFetchAt { get; set; }

        // Helper kecil untuk view / logic display
        public bool IsRoomInUse => CurrentBooking != null;
    }
}