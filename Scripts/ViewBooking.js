let bookings = Array.isArray(window.databaseBookings)
    ? window.databaseBookings
    : [];

let expanded = null;
let filter = "upcoming";

const occasionIcons = {
    Birthday: "🎂",
    Anniversary: "💍",
    Graduation: "🎓",
    "Valentine's Day": "❤️",
    "Baby Shower": "🍼",
    Other: "🎉"
};

function escapeHtml(value) {
    return String(value ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}

function formatMoney(value) {
    return Number(value || 0).toLocaleString("en-ZA", {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });
}

function formatDate(value) {
    return new Date(value + "T00:00:00").toLocaleDateString("en-ZA", {
        weekday: "long",
        year: "numeric",
        month: "long",
        day: "numeric"
    });
}

function formatTime(value) {
    if (!value) return "";

    const [h, m] = value.split(":");

    const hour = parseInt(h, 10);

    const twelveHour =
        hour > 12
            ? hour - 12
            : hour === 0
                ? 12
                : hour;

    return `${twelveHour}:${m} ${hour >= 12 ? "PM" : "AM"}`;
}

function getDaysUntil(dateStr, today) {

    const diff = Math.ceil(
        (
            new Date(dateStr + "T00:00:00").getTime() -
            new Date(today + "T00:00:00").getTime()
        ) / 86400000
    );

    if (diff === 0) return "Today!";
    if (diff === 1) return "Tomorrow";
    if (diff > 0) return `In ${diff} days`;

    return `${Math.abs(diff)} days ago`;
}

function getVisibleBookings() {

    const today =
        new Date().toISOString().split("T")[0];

    const filtered = bookings.filter(booking => {

        if (filter === "upcoming") {
            return booking.date >= today;
        }

        if (filter === "past") {
            return booking.date < today;
        }

        return true;
    });

    return [...filtered].sort((a, b) =>
        a.date.localeCompare(b.date)
    );
}

function renderBookings() {

    const root =
        document.getElementById("bookingList");

    const empty =
        document.getElementById("emptyState");

    const count =
        document.getElementById("bookingCount");

    const today =
        new Date().toISOString().split("T")[0];

    const visible =
        getVisibleBookings();

    document
        .querySelectorAll(".filter-tab")
        .forEach(button => {

            button.classList.toggle(
                "active",
                button.dataset.filter === filter
            );

        });

    if (visible.length === 0) {

        root.innerHTML = "";

        empty.hidden = false;

        count.hidden = true;

        const title =
            document.getElementById("emptyTitle");

        const copy =
            document.getElementById("emptyCopy");

        if (filter === "past") {

            title.textContent =
                "No Past Bookings";

            copy.textContent =
                "You don't have any past bookings yet.";

        }
        else if (filter === "upcoming") {

            title.textContent =
                "No Upcoming Bookings";

            copy.textContent =
                "You don't have any upcoming bookings.";

        }
        else {

            title.textContent =
                "No Bookings";

            copy.textContent =
                "You haven't made any bookings yet.";

        }

        return;
    }

    empty.hidden = true;

    count.hidden = false;

    count.textContent =
        `Showing ${visible.length} booking${visible.length !== 1 ? "s" : ""}`;

    root.innerHTML = visible.map(booking => {

        const isPast =
            booking.date < today;

        const isOpen =
            expanded === booking.id;

        const statusClass =
            isPast ? "past" : "upcoming";

        const statusText =
            isPast
                ? "Completed"
                : (booking.status || "Pending");

        const icon =
            occasionIcons[booking.occasion] || "🎉";

        return `
            <article class="booking-card ${isPast ? "" : "upcoming"}">

                <button
                    class="booking-summary"
                    type="button"
                    data-toggle-booking="${booking.id}">

                    <div class="occasion-icon">
                        ${icon}
                    </div>

                    <div class="booking-summary-main">

                        <div class="booking-summary-title-row">

                            <h3 class="booking-summary-title">
                                ${escapeHtml(booking.occasion)}
                                —
                                ${escapeHtml(booking.firstName)}
                                ${escapeHtml(booking.lastName)}
                            </h3>

                            <span class="status-pill ${statusClass}">
                                ${escapeHtml(statusText)}
                            </span>

                        </div>

                        <p class="booking-summary-date">
                            ${formatDate(booking.date)}
                            at
                            ${formatTime(booking.time)}
                        </p>

                        <p class="booking-summary-address">
                            ${escapeHtml(booking.address)},
                            ${escapeHtml(booking.city)}
                        </p>

                    </div>

                    <div class="booking-summary-right">

                        <p class="booking-price">
                            R${formatMoney(booking.totalPrice)}
                        </p>

                        ${!isPast
                ? `
                                    <span class="days-pill">
                                        ${getDaysUntil(booking.date, today)}
                                    </span>
                                  `
                : ""
            }

                        <div class="details-toggle">
                            ${isOpen ? "▲ Less" : "▼ Details"}
                        </div>

                    </div>

                </button>

                ${isOpen
                ? renderDetails(
                    booking,
                    isPast
                )
                : ""
            }

            </article>
        `;

    }).join("");

    document
        .querySelectorAll("[data-toggle-booking]")
        .forEach(button => {

            button.addEventListener("click", () => {

                const id =
                    Number(button.dataset.toggleBooking);

                expanded =
                    expanded === id
                        ? null
                        : id;

                renderBookings();

            });

        });

    document
        .querySelectorAll("[data-cancel-booking]")
        .forEach(button => {

            button.addEventListener("click", event => {

                event.stopPropagation();

                cancelBooking(
                    Number(button.dataset.cancelBooking)
                );

            });

        });
}

function renderDetails(booking, isPast) {

    const addOns =
        Array.isArray(booking.addOns)
            ? booking.addOns
            : [];

    return `
        <div class="booking-details">

            <div class="details-grid">

                <div class="detail-box">

                    <h4>Package</h4>

                    <p class="detail-main">
                        ${escapeHtml(booking.packageName)}
                    </p>

                    <p class="detail-sub">
                        R${formatMoney(booking.packagePrice)}
                    </p>

                </div>

                <div class="detail-box">

                    <h4>Booking Reference</h4>

                    <p
                        class="detail-main"
                        style="font-family:monospace;">

                        #${escapeHtml(booking.id)}

                    </p>

                    <p class="detail-sub">

                        Booked
                        ${new Date(
        booking.createdAt
    ).toLocaleDateString("en-ZA")}

                    </p>

                </div>

                ${addOns.length
            ? `
                            <div class="detail-box full">

                                <h4>Add-Ons</h4>

                                <div class="addon-pills">

                                    ${addOns.map(addon => `

                                        <span class="addon-pill">
                                            ${escapeHtml(addon.name)}
                                            (+R${formatMoney(addon.price)})
                                        </span>

                                    `).join("")}

                                </div>

                            </div>
                          `
            : ""
        }

                ${booking.notes
            ? `
                            <div class="detail-box full">

                                <h4>Special Instructions</h4>

                                <p
                                    style="
                                        color:var(--muted-foreground);
                                        font-size:14px;
                                        font-style:italic;
                                    ">

                                    ${escapeHtml(booking.notes)}

                                </p>

                            </div>
                          `
            : ""
        }

                <div class="detail-total-row">

                    <div>

                        <div class="detail-total-label">
                            * Transport fee is quoted separately
                        </div>

                    </div>

                    <div>

                        <div
                            class="detail-total-label"
                            style="
                                text-align:right;
                                text-transform:uppercase;
                                letter-spacing:.08em;
                            ">

                            Total

                        </div>

                        <div class="detail-total-price">

                            R${formatMoney(booking.totalPrice)}

                        </div>

                    </div>

                </div>

            </div>

            ${!isPast
            ? `
                        <div class="booking-actions">

                            <a
                                class="btn btn-outline whatsapp"
                                href="https://wa.me/27731232660"
                                target="_blank"
                                rel="noreferrer">

                                📞 Contact via WhatsApp

                            </a>

                            <button
                                type="button"
                                class="btn btn-outline cancel"
                                data-cancel-booking="${booking.id}">

                                Cancel

                            </button>

                        </div>
                      `
            : ""
        }

        </div>
    `;
}

function cancelBooking(id) {

    /*
     * IMPORTANT:
     *
     * We are not deleting the database booking here yet.
     *
     * This button previously deleted localStorage data.
     * That is no longer appropriate now that SQL Server
     * is the source of truth.
     *
     * Database cancellation will be implemented through
     * a controller action.
     */

    alert(
        "Booking cancellation will be connected to the database in the next step."
    );
}

document.addEventListener(
    "DOMContentLoaded",
    () => {

        document
            .querySelectorAll(".filter-tab")
            .forEach(button => {

                button.addEventListener(
                    "click",
                    () => {

                        filter =
                            button.dataset.filter;

                        expanded = null;

                        renderBookings();

                    }
                );

            });

        renderBookings();

    }
);