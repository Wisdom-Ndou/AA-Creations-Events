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

// ----------------------------
// BOOKING STATUS HELPERS
// ----------------------------

function getStatusClass(status) {
    switch ((status || "").toLowerCase()) {

        case "confirmed":
        case "approved":
            return "confirmed";

        case "preparing":
            return "preparing";

        case "in progress":
            return "in-progress";

        case "setup completed":
            return "in-progress";

        case "completed":
            return "completed";

        case "declined":
        case "cancelled":
            return "cancelled";

        default:
            return "pending";
    }
}

function renderBookingProgress(status, staffAssigned, paymentStatus, amountPaid) {
    const normalized = (status || "Pending").toLowerCase();
    const normalizedPayment = (paymentStatus || "").toLowerCase();

    if (normalized === "declined") {
        return '<div class="booking-progress cancelled-progress"><div class="cancel-icon">✕</div><div><strong>Booking Declined</strong><p>AA Creations was unable to accept this booking.</p></div></div>';
    }

    if (normalized === "cancelled") {
        return '<div class="booking-progress cancelled-progress"><div class="cancel-icon">✕</div><div><strong>Booking Cancelled</strong><p>This booking is no longer active.</p></div></div>';
    }

    const paymentLabel = normalizedPayment === "fully paid"
        ? "Fully Paid"
        : (Number(amountPaid || 0) > 0 ? "Deposit / Payment Received" : "Payment Pending");

    const labels = ["Booking Submitted", paymentLabel, "Booking Confirmed", "Staff Assigned", "Setup Completed", "Booking Completed"];
    let currentIndex = Number(amountPaid || 0) > 0 ? 1 : 0;
    if (["approved", "setup completed", "completed"].includes(normalized)) currentIndex = 2;
    if (staffAssigned || ["setup completed", "completed"].includes(normalized)) currentIndex = 3;
    if (["setup completed", "completed"].includes(normalized)) currentIndex = 4;
    if (normalized === "completed") currentIndex = 5;

    return '<div class="booking-progress">' + labels.map(function (label, index) {
        const completed = index < currentIndex;
        const active = index === currentIndex;
        return '<div class="progress-row"><div class="progress-circle ' +
            (completed ? "completed " : "") + (active ? "active" : "") + '">' +
            (completed ? "✓" : index + 1) +
            '</div><div class="progress-content"><div class="progress-title">' + label + '</div>' +
            (active ? '<div class="progress-current">Current Status</div>' : '') +
            '</div></div>' +
            (index !== labels.length - 1 ? '<div class="progress-line ' + (completed ? "completed" : "") + '"></div>' : '');
    }).join("") + '</div>';
}

function getLocalToday() {
    const now = new Date();
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, "0");
    const day = String(now.getDate()).padStart(2, "0");
    return `${year}-${month}-${day}`;
}

function getVisibleBookings() {

    const today =
        getLocalToday();

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
        getLocalToday();

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

        const isPast = booking.date < today;

        const isOpen = expanded === booking.id;

        // ALWAYS USE DATABASE STATUS
        const statusText = booking.status || "Pending";

        const statusClass = getStatusClass(statusText);

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

    document.querySelectorAll("[data-complete-booking]").forEach(button => {
        button.addEventListener("click", async event => {
            event.stopPropagation();
            const app = document.getElementById("bookingsApp");
            const token = document.querySelector('input[name="__RequestVerificationToken"]');
            const body = new URLSearchParams();
            body.append("bookingId", button.dataset.completeBooking);
            if (token) body.append("__RequestVerificationToken", token.value);

            const response = await fetch(app.dataset.completeUrl, {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8" },
                body: body.toString()
            });
            const result = await response.json();
            if (!result.success) {
                alert(result.message || "The booking could not be completed.");
                return;
            }
            const booking = bookings.find(item => item.id === Number(button.dataset.completeBooking));
            if (booking) booking.status = result.status;
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

            <h4 class="progress-heading">
                Booking Progress
            </h4>

            ${renderBookingProgress(booking.status, booking.staffAssigned, booking.paymentStatus, booking.amountPaid)}

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

                <div class="detail-box full">
                    <h4>Payment</h4>
                    <div class="review-grid">
                        <span class="label">Payment status</span><strong>${escapeHtml(booking.paymentStatus || "Unpaid")}</strong>
                        <span class="label">Amount paid</span><span>R${formatMoney(booking.amountPaid)}</span>
                        <span class="label">Balance outstanding</span><span>R${formatMoney(booking.balanceOutstanding)}</span>
                        ${booking.balanceDueDate ? '<span class="label">Balance due</span><span>' + new Date(booking.balanceDueDate).toLocaleDateString("en-ZA") + '</span>' : ''}
                        ${Number(booking.refundAmount || 0) > 0 ? '<span class="label">Refund due</span><span>R' + formatMoney(booking.refundAmount) + '</span>' : ''}
                    </div>
                </div>

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

                            <a class="btn btn-outline" href="/Cust/CustomerComplaints?bookingId=${booking.id}">
                                Submit Complaint
                            </a>

                            ${(booking.status || "").toLowerCase() === "setup completed" ? '<button type="button" class="btn btn-primary" data-complete-booking="' + booking.id + '">Confirm Arrival & Complete</button>' : ""}

                            ${Number(booking.balanceOutstanding || 0) > 0 && !["declined","cancelled","completed"].includes((booking.status || "").toLowerCase())
                                ? '<a class="btn btn-primary" href="/Cust/BalancePayment?bookingId=' + booking.id + '">Pay Remaining Balance</a>'
                                : ""}

                            ${["pending", "approved"].includes((booking.status || "Pending").toLowerCase()) ? `
                            <button
                                type="button"
                                class="btn btn-outline cancel"
                                data-cancel-booking="${booking.id}">

                                Cancel

                            </button>` : ""}

                        </div>
                      `
            : ""
        }

        </div>
    `;
}

let cancellationBookingId = null;

function openCancellationModal(id) {
    const booking = bookings.find(item => item.id === id);
    if (!booking) return;

    cancellationBookingId = id;

    const modal = document.getElementById("cancelBookingModal");
    const reason = document.getElementById("cancelReason");
    const error = document.getElementById("cancelReasonError");
    const counter = document.getElementById("cancelReasonCount");
    const summary = document.getElementById("cancelBookingSummary");
    const confirmButton = document.getElementById("cancelModalConfirm");

    summary.innerHTML =
        "<strong>" + escapeHtml(booking.occasion || "Booking") + "</strong><br>" +
        "Booking #" + escapeHtml(booking.id) + " · " +
        new Date(booking.date).toLocaleDateString("en-ZA");

    reason.value = "";
    reason.classList.remove("invalid");
    error.hidden = true;
    counter.textContent = "0 / 500";
    confirmButton.disabled = false;
    confirmButton.textContent = "Confirm Cancellation";

    modal.classList.add("is-open");
    modal.setAttribute("aria-hidden", "false");
    document.body.classList.add("cancel-modal-open");
    setTimeout(() => reason.focus(), 50);
}

function closeCancellationModal() {
    const modal = document.getElementById("cancelBookingModal");
    if (!modal) return;

    modal.classList.remove("is-open");
    modal.setAttribute("aria-hidden", "true");
    document.body.classList.remove("cancel-modal-open");
    cancellationBookingId = null;
}

async function submitCancellation() {
    if (!cancellationBookingId) return;

    const reason = document.getElementById("cancelReason");
    const error = document.getElementById("cancelReasonError");
    const confirmButton = document.getElementById("cancelModalConfirm");
    const cancellationReason = reason.value.trim();

    if (!cancellationReason) {
        reason.classList.add("invalid");
        error.hidden = false;
        reason.focus();
        return;
    }

    reason.classList.remove("invalid");
    error.hidden = true;
    confirmButton.disabled = true;
    confirmButton.textContent = "Cancelling...";

    const app = document.getElementById("bookingsApp");
    const token = document.querySelector('input[name="__RequestVerificationToken"]');
    const body = new URLSearchParams();
    body.append("bookingId", cancellationBookingId);
    body.append("cancellationReason", cancellationReason);
    if (token) body.append("__RequestVerificationToken", token.value);

    try {
        const response = await fetch(app.dataset.cancelUrl, {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8" },
            body: body.toString()
        });

        const result = await response.json();

        if (!result.success) {
            confirmButton.disabled = false;
            confirmButton.textContent = "Confirm Cancellation";
            alert(result.message || "The booking could not be cancelled.");
            return;
        }

        const booking = bookings.find(item => item.id === cancellationBookingId);
        if (booking) {
            booking.status = result.status;
            booking.cancellationCharge = Number(result.cancellationCharge || 0);
            booking.refundAmount = Number(result.refundAmount || 0);
            booking.paymentStatus = result.paymentStatus || booking.paymentStatus;
            booking.balanceOutstanding = 0;
            booking.balanceDueDate = null;
        }

        closeCancellationModal();
        renderBookings();

        if (result.refundAmount > 0) {
            alert(result.message + " Refund due: R" + formatMoney(result.refundAmount));
        } else {
            alert(result.message || "Booking cancelled.");
        }
    } catch (error) {
        confirmButton.disabled = false;
        confirmButton.textContent = "Confirm Cancellation";
        alert("We could not cancel this booking right now. Please try again.");
    }
}

function cancelBooking(id) {
    openCancellationModal(id);
}

document.addEventListener("DOMContentLoaded", function () {

    // Attach filter button listeners
    document.querySelectorAll(".filter-tab").forEach(button => {
        button.addEventListener("click", () => {
            filter = button.dataset.filter;
            renderBookings();
        });
    });

    // Initial render
    renderBookings();

    const cancelModal = document.getElementById("cancelBookingModal");
    const cancelModalClose = document.getElementById("cancelModalClose");
    const cancelModalKeep = document.getElementById("cancelModalKeep");
    const cancelModalConfirm = document.getElementById("cancelModalConfirm");
    const cancelReason = document.getElementById("cancelReason");
    const cancelReasonCount = document.getElementById("cancelReasonCount");

    if (cancelModalClose) cancelModalClose.addEventListener("click", closeCancellationModal);
    if (cancelModalKeep) cancelModalKeep.addEventListener("click", closeCancellationModal);
    if (cancelModalConfirm) cancelModalConfirm.addEventListener("click", submitCancellation);

    if (cancelReason) {
        cancelReason.addEventListener("input", function () {
            cancelReason.classList.remove("invalid");
            const error = document.getElementById("cancelReasonError");
            if (error) error.hidden = true;
            if (cancelReasonCount) {
                cancelReasonCount.textContent = cancelReason.value.length + " / 500";
            }
        });
    }

    if (cancelModal) {
        cancelModal.addEventListener("click", function (event) {
            if (event.target === cancelModal) closeCancellationModal();
        });
    }

    document.addEventListener("keydown", function (event) {
        if (event.key === "Escape" && cancelModal && cancelModal.classList.contains("is-open")) {
            closeCancellationModal();
        }
    });

});