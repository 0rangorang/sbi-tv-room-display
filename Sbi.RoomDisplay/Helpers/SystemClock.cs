namespace Sbi.RoomDisplay;

public static class SystemClock
{
    // ===== DEBUG MODE =====
    // uncomment untuk test waktu
    /*private static readonly DateTime DebugStart =
        new DateTime(2026, 2, 24, 10, 59, 58);

    private static readonly DateTime RealStart =
        DateTime.Now;

    public static DateTime Now =>
        DebugStart + (DateTime.Now - RealStart);
    */

    // ===== REAL TIME =====
    public static DateTime Now => DateTime.Now;
}