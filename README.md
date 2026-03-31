# SBI TV Dashboard Occupancy

A TV-friendly dashboard for monitoring meeting room occupancy and daily schedules at SBI Jakarta Branch.

## Overview

This project displays real-time room status for SBI meeting rooms in a large-screen dashboard layout.  
It reads booking data from an internal e-booking source, transforms it into a room-based schedule timeline, and presents key information in a format designed for TV visibility.

The dashboard highlights:

- current room occupancy
- next scheduled meeting
- full-day schedule visibility
- data freshness status
- sync / refresh activity status

## Main UI Example

![Main dashboard view](docs/images/dashboard-fico2-in-use-next-compact-sanitized.png)

## E-Booking Source Example

![E-booking source example](docs/images/ebooking-schedule-source-sanitized.png)

## Key Features

- TV-optimized layout for glanceable room occupancy monitoring
- current meeting and next meeting visibility in a single screen
- full-day meeting timeline for each room
- support for multiple meeting rooms across multiple floors
- fallback handling when fresh source data is unavailable
- runtime status indicators for both data quality and refresh activity
- responsive UI improvements for clearer viewing on different screen sizes

## Supported Scope

This version covers selected SBI Jakarta Branch meeting rooms across:

- 12th floor
- 15th floor
- 16th floor

## Runtime Status Indicators

### Data Status

The left-side data badge indicates the quality of the booking data currently being shown.

- **Live Data**  
  The dashboard is showing the latest booking data successfully retrieved from the e-booking source.

- **Stale Data**  
  The latest fetch did not return fresh data, so the dashboard is temporarily showing the last successfully retrieved dataset.

- **No Data**  
  No usable booking data is currently available from the source system, and no valid fallback dataset exists.

### Sync Status

The top-right sync badge indicates the current refresh activity of the dashboard.

- **Auto Refresh 10s**  
  The dashboard is waiting for the next scheduled refresh cycle.

- **Syncing...**  
  The dashboard is actively requesting the latest schedule data.

- **Updated**  
  A refresh has just completed successfully and the latest data has been applied.

- **Refresh Failed**  
  The latest refresh attempt failed. The previous display remains visible and retry continues automatically.

## Status Handling Examples

### Live Data + Syncing
![Live Data + Syncing](docs/images/usecase-live-data-syncing-sanitized.png)

### Stale Data + Updated
![Stale Data + Updated](docs/images/usecase-stale-data-updated-sanitized.png)

### No Data + Refresh Failed
![No Data + Refresh Failed](docs/images/usecase-no-data-refresh-failed-sanitized.png)

### Live Data + Auto Refresh 10s
![Live Data + Auto Refresh 10s](docs/images/usecase-live-data-auto-refresh-idle-sanitized.png)

## Tech Stack

- ASP.NET Core MVC
- C#
- JavaScript
- HTML / CSS
- HtmlAgilityPack

## How It Works

1. The application reads the daily schedule grid from the internal e-booking source.
2. It maps selected room headers into internal room codes.
3. Booking cells are parsed into structured room bookings.
4. Each room is rendered into:
   - current meeting state
   - next meeting state
   - full-day schedule slots
5. The UI refreshes automatically at a fixed interval.
6. If fresh data fails to load, the dashboard falls back to the last known valid dataset when available.

## Run Locally

1. Open the repository in Visual Studio or VS Code.
2. Navigate to the project folder if needed.
3. Restore dependencies, build, and run the application.

### Example commands

```bash
dotnet clean
dotnet build
dotnet run
```

4. Open the local URL shown by the ASP.NET runtime.
5. Access a room page using its route, for example:

- `/FICO1`
- `/FICO2`
- `/FICO3`
- `/BOD15`
- `/MEDB15`
- `/SMALLM115`
- `/SMALLM215`
- `/VSTDIR15`
- `/BIGM116`
- `/BIGM216`

## Public Repository Note

This public repository uses placeholder values for the internal e-booking host.  
To run the dashboard against a real environment, update the e-booking endpoint values in `ApiBookingService.cs` to match your internal source system.

## Notes

- Build artifacts such as `bin/`, `obj/`, `.vs/`, `publish/`, logs, and raw ZIP archives should not be committed.
- Public screenshots in this repository have been sanitized for documentation purposes.

## Author

Developed as an internal TV dashboard occupancy solution for SBI Jakarta Branch.
