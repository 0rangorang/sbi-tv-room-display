namespace Sbi.RoomDisplay.Models
{
    public class Booking
    {
        // Kode ruangan internal app, misalnya FICO1 / FICO2 / FICO3
        public string RoomCode { get; set; } = string.Empty;

        // Untuk sekarang kita pakai sebagai judul utama meeting dari e-booking
        public string Division { get; set; } = string.Empty;

        // Kalau Description di detail meeting terisi, kita taruh di sini
        public string Activity { get; set; } = string.Empty;

        // Created by dari detail e-booking
        public string PIC { get; set; } = string.Empty;

        // EXTERNAL / INTERNAL / BOD_BOC / UNKNOWN
        public string MeetingType { get; set; } = "UNKNOWN";

        // Jam mulai meeting
        public DateTime StartTime { get; set; }

        // Jam selesai meeting
        public DateTime EndTime { get; set; }

        // Biar UI tetap aman walau salah satu field kosong
        public string DisplayTitle
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Division) &&
                    !string.IsNullOrWhiteSpace(Activity))
                {
                    return $"{Division} - {Activity}";
                }

                if (!string.IsNullOrWhiteSpace(Division))
                    return Division;

                if (!string.IsNullOrWhiteSpace(Activity))
                    return Activity;

                return "(Untitled Meeting)";
            }
        }
    }
}