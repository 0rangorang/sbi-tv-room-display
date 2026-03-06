// Representasi 1 booking meeting
// Ini model domain utama dari sistem

namespace Sbi.RoomDisplay.Models
{
    public class Booking
    {
        // Kode ruangan (misalnya: FICO1)
        public string RoomCode { get; set; }

        // Nama divisi pemakai
        public string Division { get; set; }

        // Nama aktivitas meeting
        public string Activity { get; set; }

        // Person in Charge
        public string PIC { get; set; }

        // Waktu mulai meeting
        public DateTime StartTime { get; set; }

        // Waktu selesai meeting
        public DateTime EndTime { get; set; }

        // Property "DisplayTitle" tambahan untuk display
        // Tidak disimpan di database
        // Tidak disimpan di View
        // Agar menghindari formatting di View dan membuat View tetap clean
        public string DisplayTitle 
            => $"{Division} - {Activity}";
    }
}