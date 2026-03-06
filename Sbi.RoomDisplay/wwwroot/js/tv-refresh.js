// Refresh data setiap 10 detik
// Cukup untuk TV (tidak perlu real-time per detik)

async function refreshTv() {

    try {

        // ======================
        // SYNC SERVER TIME
        // ======================
        const timeRes = await fetch("/time");
        const timeData = await timeRes.json();

        window.serverNow = new Date(timeData.now);

        // ======================
        // GET ROOM CODE
        // ======================
        const roomCode =
            window.location.pathname.replace("/", "");

        const res = await fetch(`/schedule/${roomCode}`);
        const data = await res.json();

        const current = data.current ?? data.Current;
        const slots = data.slots ?? data.Slots;

        // ======================
        // UPDATE CURRENT ACTIVITY
        // ======================
        const currentDiv =
            document.getElementById("currentActivity");

        if (current) {
            currentDiv.innerHTML =
                `<div class="info">${current.division} - ${current.activity}</div>
                <div class="info">PIC: ${current.pic}</div>
                <div class="info">
                    ${new Date(current.startTime)
                        .toLocaleTimeString([], {hour:'2-digit',minute:'2-digit'})}
                    -
                    ${new Date(current.endTime)
                        .toLocaleTimeString([], {hour:'2-digit',minute:'2-digit'})}
                </div>`;
        }
        else {
            currentDiv.innerHTML =
                `<div class="info">Room Available</div>`;
        }

        // ======================
        // UPDATE TIMELINE
        // ======================
        const timeline =
            document.getElementById("timeline");

        const indicator =
            document.getElementById("time-indicator");

        timeline.querySelectorAll(".slot-row")
            .forEach(e => e.remove());

        const fragment = document.createDocumentFragment();

        slots.forEach(slot => {

            const row = document.createElement("div");

            row.className =
                `slot-row ${slot.status.toLowerCase()}`;

            row.innerHTML =
                `<div>${slot.time}</div>
                <div>${slot.description}</div>`;

            fragment.appendChild(row);
        });

        timeline.insertBefore(fragment, indicator);

    }
    catch (e) {
        console.error("TV refresh error:", e);
    }
}

// jalan pertama (TANPA tunggu 10 detik)
refreshTv();

// interval refresh
setInterval(refreshTv, 10000);