// Enum untuk menentukan kondisi visual timeline
// Supaya tidak pakai string bebas seperti "active", "past"
// Menghindari typo dan bug UI

//Kenapa enum? Karena:
//1. Type-safe
//2. Mudah di-map ke CSS class
//3. Clean architecture

namespace Sbi.RoomDisplay.Models
{
    public enum SlotStatus
    {
        Past,       // Waktu sudah lewat
        Active,     // Sedang berlangsung
        Future,     // Akan datang
        Available   // Tidak ada booking
    }
}