// Representasi 1 baris timeline per jam
// Ini yang nanti ditampilkan di TV

//Kenapa kita buat slot per jam? Karena:
//1. Tampilan TV lebih mudah dibaca
//2. Lebih stabil dibanding minute-level timeline
//3. Performance ringan

namespace Sbi.RoomDisplay.Models
{
    public class HourlySlot
    {
        // Jam slot (contoh: 07:00)
        public DateTime Time { get; set; }

        // Deskripsi yang akan tampil
        public string Description { get; set; }

        // Status visual (Past / Active / Future / Available)
        public SlotStatus Status { get; set; }
    }
}