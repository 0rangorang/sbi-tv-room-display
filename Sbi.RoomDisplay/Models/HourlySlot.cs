namespace Sbi.RoomDisplay.Models
{
    // Nama class dibiarkan HourlySlot supaya refactor Step 4 tetap kecil.
    // Setelah Step 4, class ini dipakai untuk slot timeline 30 menit.
    public class HourlySlot
    {
        // Waktu label slot, misalnya 09:00, 09:30, 10:00
        public DateTime Time { get; set; }

        // Teks yang tampil di timeline
        public string Description { get; set; } = string.Empty;

        // Status visual row
        public SlotStatus Status { get; set; }
    }
}