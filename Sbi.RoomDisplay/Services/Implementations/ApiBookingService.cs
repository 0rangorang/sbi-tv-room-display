using System.Collections.Concurrent;
using System.Globalization;
using HtmlAgilityPack;
using Sbi.RoomDisplay.Models;
using Sbi.RoomDisplay.Services.Interfaces;

namespace Sbi.RoomDisplay.Services.Implementations
{
    public class ApiBookingService : IBookingService
    {
        private const string BaseUrl = "http://YOUR-EBOOKING-HOST/";
        private const string GridUrl = "http://YOUR-EBOOKING-HOST/all.php";

        private sealed class CachedRoomSnapshot
        {
            public List<Booking> Bookings { get; init; } = new();
            public DateTime LastSuccessAt { get; init; }
        }

        public sealed class RoomFetchState
        {
            public bool UsedFallbackData { get; init; }
            public bool HasAnyData { get; init; }
            public DateTime? LastSuccessAt { get; init; }
            public string ErrorMessage { get; init; } = string.Empty;
        }

        private static readonly ConcurrentDictionary<string, CachedRoomSnapshot> RoomCache =
            new(StringComparer.OrdinalIgnoreCase);

        private static readonly ConcurrentDictionary<string, RoomFetchState> RoomStates =
            new(StringComparer.OrdinalIgnoreCase);

        // Mapping room code internal app -> label room di header e-booking
        private static readonly Dictionary<string, string> RoomAliases =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "FICO1", "Fico 1 - 12th floor" },
                { "FICO2", "Fico 2 - 12th floor" },
                { "FICO3", "Fico 3 - 12th floor" },

                { "BOD15", "BOD Room - 15th floor" },
                { "MEDB15", "Medium.Room B - 15th floor" },
                { "SMALLM115", "Small M1 - 15th floor" },
                { "SMALLM215", "Small M2 - 15th floor" },
                { "VSTDIR15", "Vst Dir - 15th floor" },

                { "BIGM116", "Big M1 - 16th floor" },
                { "BIGM216", "Big M2 - 16th floor" }
            };

        private readonly HttpClient _http;

        public ApiBookingService(HttpClient http)
        {
            _http = http;
        }

        public List<Booking> GetBookingsByRoom(string roomCode)
        {
            var normalizedRoomCode = NormalizeRoomCode(roomCode);

            try
            {
                var freshBookings = FetchFreshBookingsByRoom(normalizedRoomCode);
                var fetchedAt = DateTime.Now;

                RoomCache[normalizedRoomCode] = new CachedRoomSnapshot
                {
                    Bookings = CloneBookings(freshBookings),
                    LastSuccessAt = fetchedAt
                };

                RoomStates[normalizedRoomCode] = new RoomFetchState
                {
                    UsedFallbackData = false,
                    HasAnyData = freshBookings.Count > 0,
                    LastSuccessAt = fetchedAt,
                    ErrorMessage = string.Empty
                };

                return freshBookings;
            }
            catch (Exception ex)
            {
                if (RoomCache.TryGetValue(normalizedRoomCode, out var cached))
                {
                    RoomStates[normalizedRoomCode] = new RoomFetchState
                    {
                        UsedFallbackData = true,
                        HasAnyData = cached.Bookings.Count > 0,
                        LastSuccessAt = cached.LastSuccessAt,
                        ErrorMessage = CleanText(ex.Message)
                    };

                    return CloneBookings(cached.Bookings);
                }

                RoomStates[normalizedRoomCode] = new RoomFetchState
                {
                    UsedFallbackData = false,
                    HasAnyData = false,
                    LastSuccessAt = null,
                    ErrorMessage = CleanText(ex.Message)
                };

                return new List<Booking>();
            }
        }

        public static RoomFetchState GetRoomFetchState(string roomCode)
        {
            var normalizedRoomCode = NormalizeRoomCode(roomCode);

            if (RoomStates.TryGetValue(normalizedRoomCode, out var state))
                return state;

            return new RoomFetchState
            {
                UsedFallbackData = false,
                HasAnyData = true,
                LastSuccessAt = null,
                ErrorMessage = string.Empty
            };
        }

        private List<Booking> FetchFreshBookingsByRoom(string roomCode)
        {
            var bookings = new List<Booking>();

            // 1. Ambil grid utama e-booking
            var html = _http.GetStringAsync(GridUrl).GetAwaiter().GetResult();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // 2. Ambil tabel day-view utama
            var gridTable = doc.DocumentNode.SelectSingleNode("//table[@id='day_main']");
            if (gridTable == null)
                return bookings;

            // 3. Ambil tanggal grid dari heading (jangan hardcode DateTime.Today)
            var gridDate = ParseGridDate(
                doc.DocumentNode.SelectSingleNode("//h2")?.InnerText);

            // 4. Ambil resolusi slot dari HTML (sample kamu = 1800 detik = 30 menit)
            var resolutionSeconds = ParseInt(
                gridTable.GetAttributeValue("data-resolution", "1800"),
                1800);

            // 5. Ambil header room, skip kolom "Time:"
            var roomHeaders = gridTable
                .SelectNodes(".//thead/tr/th")
                ?.Skip(1)
                .ToList();

            if (roomHeaders == null || roomHeaders.Count == 0)
                return bookings;

            // 6. Cari kolom target sesuai roomCode yang diminta controller
            var targetColumnIndex = FindRoomColumnIndex(roomHeaders, roomCode);
            if (targetColumnIndex < 0)
                return bookings;

            // 7. Ambil semua row data (skip header row pertama)
            var rows = gridTable.SelectNodes(".//tr")?.Skip(1).ToList();
            if (rows == null || rows.Count == 0)
                return bookings;

            // Menyimpan sisa rowspan aktif per kolom room
            var activeRowSpans = new int[roomHeaders.Count];

            for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var cells = rows[rowIndex].SelectNodes("./td")?.ToList();
                if (cells == null || cells.Count == 0)
                {
                    ConsumeActiveRowSpans(activeRowSpans);
                    continue;
                }

                var timeText = CleanText(cells[0].InnerText);
                if (!TryParseRowStart(gridDate, timeText, out var rowStart))
                {
                    ConsumeActiveRowSpans(activeRowSpans);
                    continue;
                }

                var logicalColumnIndex = 0;

                foreach (var cell in cells.Skip(1))
                {
                    while (logicalColumnIndex < roomHeaders.Count &&
                        activeRowSpans[logicalColumnIndex] > 0)
                    {
                        logicalColumnIndex++;
                    }

                    if (logicalColumnIndex >= roomHeaders.Count)
                        break;

                    var rowSpan = Math.Max(
                        1,
                        ParseInt(cell.GetAttributeValue("rowspan", "1"), 1));

                    if (rowSpan > 1)
                        activeRowSpans[logicalColumnIndex] = rowSpan;

                    if (logicalColumnIndex == targetColumnIndex &&
                        IsBookingCell(cell))
                    {
                        var booking = BuildBookingFromCell(
                            cell,
                            roomCode,
                            rowStart,
                            rowSpan,
                            resolutionSeconds);

                        if (booking != null)
                            bookings.Add(booking);
                    }

                    logicalColumnIndex++;
                }

                ConsumeActiveRowSpans(activeRowSpans);
            }

            return bookings
                .OrderBy(b => b.StartTime)
                .ToList();
        }

        private Booking BuildBookingFromCell(
            HtmlNode cell,
            string roomCode,
            DateTime rowStart,
            int rowSpan,
            int resolutionSeconds)
        {
            var anchor = cell.SelectSingleNode(".//a");

            // Fallback dari grid
            var fallbackTitle = ExtractGridTitle(anchor);
            var fallbackMeetingType = DetectMeetingType(cell);

            var detailHref = anchor?.GetAttributeValue("href", string.Empty) ?? string.Empty;
            var detailUrl = BuildAbsoluteUrl(detailHref);

            var detailTitle = string.Empty;
            var detailFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Coba baca detail page untuk metadata lengkap
            if (!string.IsNullOrWhiteSpace(detailUrl))
            {
                try
                {
                    (detailTitle, detailFields) = ReadEntryDetail(detailUrl);
                }
                catch
                {
                    // Kalau detail page gagal diambil, tetap pakai data dari grid
                }
            }

            var title = FirstNonEmpty(detailTitle, fallbackTitle, "(Untitled Meeting)");
            var description = GetField(detailFields, "Description");
            var pic = FirstNonEmpty(GetField(detailFields, "Created by"), "-");
            var meetingType = NormalizeMeetingType(
                GetField(detailFields, "Type"),
                fallbackMeetingType);

            return new Booking
            {
                RoomCode = roomCode,
                Division = title,
                Activity = description,
                PIC = pic,
                MeetingType = meetingType,
                StartTime = rowStart,
                EndTime = rowStart.AddSeconds(rowSpan * resolutionSeconds)
            };
        }

        private (string Title, Dictionary<string, string> Fields) ReadEntryDetail(string url)
        {
            var html = _http.GetStringAsync(url).GetAwaiter().GetResult();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var title = CleanText(
                doc.DocumentNode.SelectSingleNode("//div[@id='contents']/h3")?.InnerText);

            var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var rows = doc.DocumentNode.SelectNodes("//table[@id='entry']//tr");
            if (rows != null)
            {
                foreach (var row in rows)
                {
                    var cells = row.SelectNodes("./td");
                    if (cells == null || cells.Count < 2)
                        continue;

                    var key = CleanLabel(cells[0].InnerText);
                    var value = CleanText(cells[1].InnerText);

                    if (!string.IsNullOrWhiteSpace(key))
                        fields[key] = value;
                }
            }

            return (title, fields);
        }

        private static int FindRoomColumnIndex(List<HtmlNode> roomHeaders, string roomCode)
        {
            if (!RoomAliases.TryGetValue(roomCode, out var alias))
                return -1;

            var expected = NormalizeToken(alias);

            for (var i = 0; i < roomHeaders.Count; i++)
            {
                var headerText = NormalizeToken(CleanText(roomHeaders[i].InnerText));

                if (headerText.Contains(expected))
                    return i;
            }

            return -1;
        }

        private static void ConsumeActiveRowSpans(int[] activeRowSpans)
        {
            for (var i = 0; i < activeRowSpans.Length; i++)
            {
                if (activeRowSpans[i] > 0)
                    activeRowSpans[i]--;
            }
        }

        private static bool TryParseRowStart(DateTime gridDate, string timeText, out DateTime rowStart)
        {
            rowStart = default;

            if (!TimeOnly.TryParseExact(
                    timeText,
                    "HH:mm",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var time))
            {
                return false;
            }

            rowStart = gridDate.Date
                .AddHours(time.Hour)
                .AddMinutes(time.Minute);

            return true;
        }

        private static DateTime ParseGridDate(string rawHeading)
        {
            var heading = CleanText(rawHeading);

            if (DateTime.TryParseExact(
                    heading,
                    "dddd dd MMMM yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsed))
            {
                return parsed.Date;
            }

            return DateTime.Today;
        }

        private static bool IsBookingCell(HtmlNode cell)
        {
            var classes = SplitClasses(cell.GetAttributeValue("class", string.Empty));

            return classes.Contains("E")
                || classes.Contains("I")
                || classes.Contains("D")
                || classes.Contains("external")
                || classes.Contains("internal");
        }

        private static string DetectMeetingType(HtmlNode cell)
        {
            var classes = SplitClasses(cell.GetAttributeValue("class", string.Empty));

            if (classes.Contains("D"))
                return "BOD_BOC";
            if (classes.Contains("E"))
                return "EXTERNAL";
            if (classes.Contains("I"))
                return "INTERNAL";

            return "UNKNOWN";
        }

        private static string NormalizeMeetingType(string raw, string fallback)
        {
            var value = CleanText(raw).ToUpperInvariant();

            return value switch
            {
                "INTERNAL" => "INTERNAL",
                "EXTERNAL" => "EXTERNAL",
                "BOD/BOC" => "BOD_BOC",
                "BOD_BOC" => "BOD_BOC",
                _ => fallback
            };
        }

        private static string ExtractGridTitle(HtmlNode anchor)
        {
            if (anchor == null)
                return string.Empty;

            var title = CleanText(anchor.GetAttributeValue("title", string.Empty));

            while (title.EndsWith("-"))
                title = title[..^1].TrimEnd();

            return title;
        }

        private static string BuildAbsoluteUrl(string href)
        {
            if (string.IsNullOrWhiteSpace(href))
                return string.Empty;

            if (Uri.TryCreate(href, UriKind.Absolute, out var absoluteUri))
                return absoluteUri.ToString();

            return new Uri(new Uri(BaseUrl), href).ToString();
        }

        private static HashSet<string> SplitClasses(string raw)
        {
            return raw
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static int ParseInt(string raw, int fallback)
        {
            return int.TryParse(raw, out var value)
                ? value
                : fallback;
        }

        private static string CleanLabel(string raw)
        {
            return CleanText(raw).TrimEnd(':');
        }

        private static string CleanText(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return string.Empty;

            return HtmlEntity.DeEntitize(raw)
                .Replace("\u00A0", " ")
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Trim();
        }

        private static string NormalizeToken(string raw)
        {
            return new string(
                CleanText(raw)
                    .ToUpperInvariant()
                    .Where(char.IsLetterOrDigit)
                    .ToArray());
        }

        private static string GetField(
            Dictionary<string, string> fields,
            string key)
        {
            return fields.TryGetValue(key, out var value)
                ? value
                : string.Empty;
        }

        private static string FirstNonEmpty(params string[] values)
        {
            return values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))
                ?? string.Empty;
        }

        private static string NormalizeRoomCode(string roomCode)
        {
            return string.IsNullOrWhiteSpace(roomCode)
                ? string.Empty
                : roomCode.Trim().ToUpperInvariant();
        }

        private static List<Booking> CloneBookings(List<Booking> bookings)
        {
            return bookings
                .Select(CloneBooking)
                .ToList();
        }

        private static Booking CloneBooking(Booking booking)
        {
            return new Booking
            {
                RoomCode = booking.RoomCode,
                Division = booking.Division,
                Activity = booking.Activity,
                PIC = booking.PIC,
                MeetingType = booking.MeetingType,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime
            };
        }
    }
}