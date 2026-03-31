const REFRESH_INTERVAL_MS = 10000;
const FETCH_TIMEOUT_MS = 8000;
const ERROR_RETRY_INTERVAL_MS = 5000;
const UPDATED_BADGE_VISIBLE_MS = 2500;

let refreshTimerId = null;
let syncStatusTimerId = null;
let isRefreshing = false;

function formatTime(value) {
    return new Date(value).toLocaleTimeString([], {
        hour: "2-digit",
        minute: "2-digit"
    });
}

function formatTimeWithSeconds(value) {
    return new Date(value).toLocaleTimeString([], {
        hour: "2-digit",
        minute: "2-digit",
        second: "2-digit"
    });
}

function getBookingTitle(booking) {
    if (!booking) return "";

    if (booking.displayTitle) return booking.displayTitle;

    const division = booking.division?.trim();
    const activity = booking.activity?.trim();

    if (division && activity) return `${division} - ${activity}`;
    if (division) return division;
    if (activity) return activity;

    return "(Untitled Meeting)";
}

function setText(id, value) {
    const el = document.getElementById(id);
    if (!el) return;

    el.textContent = value ?? "";
}

function setHidden(id, hidden) {
    const el = document.getElementById(id);
    if (!el) return;

    el.classList.toggle("is-hidden", hidden);
}

function setStatusBadge(isInUse) {
    const badge = document.getElementById("roomStatusBadge");
    if (!badge) return;

    badge.textContent = isInUse ? "IN USE" : "AVAILABLE";
    badge.classList.toggle("in-use", isInUse);
    badge.classList.toggle("available", !isInUse);
}

function setSyncStatus(state, text) {
    const el = document.getElementById("syncStatusText");
    if (!el) return;

    el.className = `sync-status ${state}`;
    el.textContent = text;
}

function clearSyncStatusTimer() {
    if (syncStatusTimerId) {
        clearTimeout(syncStatusTimerId);
        syncStatusTimerId = null;
    }
}

function scheduleIdleSyncStatus() {
    clearSyncStatusTimer();

    syncStatusTimerId = setTimeout(() => {
        if (!isRefreshing) {
            setSyncStatus("idle", `AUTO REFRESH ${REFRESH_INTERVAL_MS / 1000}S`);
        }
    }, UPDATED_BADGE_VISIBLE_MS);
}

function setDataFreshness(statusClass, statusText, statusDetail) {
    const badge = document.getElementById("dataFreshnessBadge");
    const meta = document.getElementById("dataFreshnessMeta");

    if (badge) {
        badge.className = `data-freshness ${statusClass || "live"}`;
        badge.textContent = statusText || "LIVE DATA";
    }

    if (meta) {
        meta.textContent = statusDetail || "";
        meta.classList.toggle("is-hidden", !statusDetail);
    }
}

function getMeetingTypeText(meetingType) {
    const value = (meetingType || "").toUpperCase();

    if (value === "EXTERNAL") return "EXTERNAL";
    if (value === "INTERNAL") return "INTERNAL";
    if (value === "BOD_BOC" || value === "BOD/BOC") return "BOD/BOC";

    return "";
}

function getMeetingTypeClass(meetingType) {
    const value = (meetingType || "").toUpperCase();

    if (value === "EXTERNAL") return "external";
    if (value === "INTERNAL") return "internal";
    if (value === "BOD_BOC" || value === "BOD/BOC") return "bod-boc";

    return "";
}

function setMeetingTypeBadge(id, meetingType, hidden) {
    const el = document.getElementById(id);
    if (!el) return;

    const text = getMeetingTypeText(meetingType);
    const cssClass = getMeetingTypeClass(meetingType);

    if (hidden || !text || !cssClass) {
        el.textContent = "";
        el.className = "meeting-type-badge is-hidden";
        return;
    }

    el.textContent = text;
    el.className = `meeting-type-badge ${cssClass}`;
}

function setNextCardMode(isCompact) {
    const el = document.getElementById("nextBookingBlock");
    if (!el) return;

    el.classList.toggle("compact", isCompact);
    el.classList.toggle("expanded", !isCompact);
}

function renderCurrentSection(current, next) {
    const currentMeetingType =
        current?.meetingType || current?.MeetingType || "";

    const nextMeetingType =
        next?.meetingType || next?.MeetingType || "";

    if (current) {
        setStatusBadge(true);

        setText("currentTitleText", getBookingTitle(current));
        setText("currentPicRow", `Created by: ${current.pic || "-"}`);
        setText(
            "currentTimeRow",
            `${formatTime(current.startTime)} - ${formatTime(current.endTime)}`
        );

        setMeetingTypeBadge("currentMeetingTypeBadge", currentMeetingType, false);

        setHidden("currentPicRow", false);
        setHidden("currentTimeRow", false);

        if (next) {
            setText("nextTitleText", getBookingTitle(next));
            setText("nextPicRow", `Created by: ${next.pic || "-"}`);
            setText(
                "nextTimeRow",
                `${formatTime(next.startTime)} - ${formatTime(next.endTime)}`
            );

            setMeetingTypeBadge("nextMeetingTypeBadge", nextMeetingType, false);
            setNextCardMode(true);
            setHidden("nextBookingBlock", false);
            setHidden("nextPicRow", true);
        } else {
            setText("nextTitleText", "");
            setText("nextPicRow", "");
            setText("nextTimeRow", "");
            setMeetingTypeBadge("nextMeetingTypeBadge", "", true);
            setHidden("nextBookingBlock", true);
        }

        return;
    }

    setStatusBadge(false);

    setText("currentTitleText", "Room Available");
    setText("currentPicRow", "");
    setText("currentTimeRow", "");
    setMeetingTypeBadge("currentMeetingTypeBadge", "", true);

    setHidden("currentPicRow", true);
    setHidden("currentTimeRow", true);

    if (next) {
        setText("nextTitleText", getBookingTitle(next));
        setText("nextPicRow", `Created by: ${next.pic || "-"}`);
        setText(
            "nextTimeRow",
            `${formatTime(next.startTime)} - ${formatTime(next.endTime)}`
        );

        setMeetingTypeBadge("nextMeetingTypeBadge", nextMeetingType, false);
        setNextCardMode(false);
        setHidden("nextBookingBlock", false);
        setHidden("nextPicRow", false);
    } else {
        setText("nextTitleText", "");
        setText("nextPicRow", "");
        setText("nextTimeRow", "");
        setMeetingTypeBadge("nextMeetingTypeBadge", "", true);

        setHidden("nextBookingBlock", true);
    }
}

function renderTimeline(slots) {
    const timeline = document.getElementById("timeline");
    const indicator = document.getElementById("time-indicator");

    if (!timeline || !indicator) return;

    timeline.querySelectorAll(".slot-row")
        .forEach(e => e.remove());

    const fragment = document.createDocumentFragment();

    slots.forEach(slot => {
        const row = document.createElement("div");
        row.className = `slot-row ${slot.status.toLowerCase()}`;

        const timeDiv = document.createElement("div");
        timeDiv.className = "schedule-time";
        timeDiv.textContent = slot.time;

        const descDiv = document.createElement("div");
        descDiv.className = "schedule-desc";
        descDiv.textContent = slot.description;
        descDiv.title = slot.description;

        row.appendChild(timeDiv);
        row.appendChild(descDiv);

        fragment.appendChild(row);
    });

    timeline.insertBefore(fragment, indicator);

    if (typeof updateTimeIndicator === "function") {
        requestAnimationFrame(updateTimeIndicator);
    }
}

async function fetchJson(url) {
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), FETCH_TIMEOUT_MS);

    try {
        const response = await fetch(url, {
            method: "GET",
            cache: "no-store",
            headers: {
                "Cache-Control": "no-cache",
                "Pragma": "no-cache"
            },
            signal: controller.signal
        });

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        return await response.json();
    } finally {
        clearTimeout(timeoutId);
    }
}

function scheduleNextRefresh(delayMs = REFRESH_INTERVAL_MS) {
    if (refreshTimerId) {
        clearTimeout(refreshTimerId);
    }

    refreshTimerId = setTimeout(refreshTv, delayMs);
}

async function refreshTv() {
    if (isRefreshing) return;

    clearSyncStatusTimer();
    isRefreshing = true;
    setSyncStatus("syncing", "SYNCING...");

    try {
        const roomCode =
            window.location.pathname.replace("/", "").trim();

        const data = await fetchJson(`/schedule/${roomCode}`);

        const serverNow = data.serverNow ?? data.ServerNow;
        if (serverNow) {
            window.serverNow = new Date(serverNow);
        }

        const current = data.current ?? data.Current;
        const next = data.next ?? data.Next;
        const slots = data.slots ?? data.Slots ?? [];

        const dataStatusClass =
            data.dataStatusClass ?? data.DataStatusClass ?? "live";
        const dataStatusText =
            data.dataStatusText ?? data.DataStatusText ?? "LIVE DATA";
        const dataStatusDetail =
            data.dataStatusDetail ?? data.DataStatusDetail ?? "";

        renderCurrentSection(current, next);
        renderTimeline(slots);
        setDataFreshness(dataStatusClass, dataStatusText, dataStatusDetail);

        const syncTime = window.serverNow ?? new Date();
        setSyncStatus("ok", `UPDATED ${formatTimeWithSeconds(syncTime)}`);
        scheduleIdleSyncStatus();

        scheduleNextRefresh(REFRESH_INTERVAL_MS);
    }
    catch (e) {
        console.error("TV refresh error:", e);
        clearSyncStatusTimer();
        setSyncStatus("error", "REFRESH FAILED");

        // Jangan rusak UI lama; cukup retry lebih cepat
        scheduleNextRefresh(ERROR_RETRY_INTERVAL_MS);
    }
    finally {
        isRefreshing = false;
    }
}

window.addEventListener("focus", () => {
    refreshTv();
});

window.addEventListener("online", () => {
    refreshTv();
});

window.addEventListener("offline", () => {
    clearSyncStatusTimer();
    setSyncStatus("error", "OFFLINE");
});

document.addEventListener("visibilitychange", () => {
    if (!document.hidden) {
        refreshTv();
    }
});

setSyncStatus("idle", "AUTO REFRESH 10S");
refreshTv();