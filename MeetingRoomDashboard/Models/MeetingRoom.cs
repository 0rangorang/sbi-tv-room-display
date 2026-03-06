namespace MeetingRoomDashboard.Models
{
    // Model ini merepresentasikan data 1 ruangan meeting
    // Biasanya data ini berasal dari database atau API
    public class MeetingRoom
    {
        // Nama ruangan meeting
        public string RoomName { get; set; }

        // Siapa yang sedang menggunakan ruangan
        public string CurrentUser { get; set; }

        // Aktivitas yang dilakukan
        public string Activity { get; set; }

        // Jam penggunaan sekarang
        public string Time { get; set; }

        // Jadwal berikutnya
        public string NextSchedule { get; set; }

        // Properti tambahan untuk membantu View
        // Logic ini dipindahkan ke Model agar View tetap bersih
        public bool IsOccupied
        {
            get
            {
                // Jika CurrentUser tidak kosong dan bukan "-"
                // maka ruangan dianggap sedang dipakai
                return !string.IsNullOrEmpty(CurrentUser) && CurrentUser != "-";
            }
        }
    }
}
