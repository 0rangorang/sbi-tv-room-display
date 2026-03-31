# SBI TV Dashboard Occupancy

TV dashboard for room-occupancy monitoring and daily meeting visibility.

## Overview

This project displays room status for SBI meeting rooms in a TV-friendly layout.  
It reads schedule data from an internal e-booking source, transforms it into a room timeline, and highlights:

- current room occupancy
- next scheduled meeting
- full-day schedule visibility
- runtime data-health and sync states

## Main UI Example

![Main dashboard view](docs/images/dashboard-fico2-in-use-next-compact-sanitized.png)

## Source Schedule Example

![E-booking source example](docs/images/ebooking-schedule-source-sanitized.png)

## Data Status

- **Live Data**: latest source data loaded successfully.
- **Stale Data**: last known good data is shown because the newest fetch failed.
- **No Data**: no usable source data is available.

## Sync Status

- **Auto Refresh 10s**: waiting for the next automatic refresh cycle.
- **Syncing...**: refresh request is currently in progress.
- **Updated**: refresh succeeded and new data has been applied.
- **Refresh Failed**: refresh failed; the previous display remains visible and retry continues automatically.

## Status Handling Use Cases

### 1. Live Data + Syncing
![Live Data + Syncing](docs/images/usecase-live-data-syncing-sanitized.png)

### 2. Stale Data + Updated
![Stale Data + Updated](docs/images/usecase-stale-data-updated-sanitized.png)

### 3. No Data + Refresh Failed
![No Data + Refresh Failed](docs/images/usecase-no-data-refresh-failed-sanitized.png)

### 4. Live Data + Auto Refresh 10s
![Live Data + Auto Refresh 10s](docs/images/usecase-live-data-auto-refresh-idle-sanitized.png)
