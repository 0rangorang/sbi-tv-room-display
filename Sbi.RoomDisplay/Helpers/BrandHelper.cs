// BrandHelper bertanggung jawab memilih logo yang benar
// Berdasarkan background mode
// Agar tidak ada developer yang salah pilih logo

// Kenapa pakai static class?
//1. Tidak perlu dependency injection
//2. Tidak ada state
//3. Pure utility

namespace Sbi.RoomDisplay.Helpers;

public static class BrandHelper
{
    // Mengembalikan path logo SBI sesuai mode
    public static string GetSbiLogo(BrandMode mode)
    {
        switch (mode)
        {
            case BrandMode.Dark:
                return "/images/brand/logo-sbi-white.png";

            case BrandMode.Accent:
                return "/images/brand/logo-sbi-white.png";

            default:
                return "/images/brand/logo-sbi-dark.png";
        }
    }

    // Mengembalikan path logo SIG sesuai mode
    public static string GetSigLogo(BrandMode mode)
    {
        switch (mode)
        {
            case BrandMode.Dark:
                // Untuk background hitam wajib pakai white + red accent
                return "/images/brand/logo-sig-white-redaccent.png";

            case BrandMode.Accent:
                // Untuk background merah pakai white full
                return "/images/brand/logo-sig-white-full.png";

            default:
                return "/images/brand/logo-sig-dark.png";
        }
    }
}