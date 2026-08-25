const STORAGE_KEY = "aa_bookings";

const packageNames = {
    basic: { name: "Basic Package", price: 650, badge: "Starter" },
    standard: { name: "Standard Package", price: 850, badge: "Popular" },
    premium: { name: "Premium Package", price: 1000, badge: "Premium" }
};

const addOnDetails = {
    balloons: { name: "Extra Balloon Bouquet", price: 50, icon: "🎈" },
    confetti: { name: "Confetti Cannon", price: 80, icon: "🎊" },
    lights: { name: "LED Fairy Lights", price: 120, icon: "✨" },
    flowerwall: { name: "Flower Wall Backdrop", price: 200, icon: "🌸" },
    letters: { name: "Custom Letter/Number Balloons", price: 150, icon: "🔡" },
    candles: { name: "Scented Candle Set", price: 90, icon: "🕯️" }
};

const occasionIcons = {
    Birthday: "🎂",
    Anniversary: "💍",
    Graduation: "🎓",
    "Valentine's Day": "❤️",
    "Baby Shower": "🍼",
    Other: "🎉"
};

let bookings = [];
let expanded = null;
let filter = "upcoming";

function seedDemoBookings() {
    return [
        {
            id: "BK1000001",
            firstName: "Lerato",
            lastName: "Dlamini",
            email: "lerato@example.com",
            phone: "082 456 7890",
            occasion: "Anniversary",
            date: "2026-09-14",
            time: "15:00",
            address: "45 Rose Avenue, Sandton",
            city: "Johannesburg",
            notes: "Red and gold colour theme please",
            packageId: "premium",
            addOns: ["lights", "candles"],
            createdAt: "2026-08-20T10:30:00.000Z",
            status: "Confirmed"
        },
        {
            id: "BK1000002",
            firstName: "Thandi",
            lastName: "Nkosi",
            email: "thandi@example.com",
            phone: "073 321 9876",
            occasion: "Birthday",
            date: "2026-09-28",
            time: "13:00",
            address: "8 Celebration Street, Rosebank",
            city: "Johannesburg",
            notes: "",
            packageId: "standard",
            addOns: ["balloons", "confetti"],
            createdAt: "2026-08-21T14:00:00.000Z",
            status: "Confirmed"
        }
    ];
}

function escapeHtml(value) {
    return String(value ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}

function formatMoney(value) {
    return Number(value || 0).toLocaleString("en-ZA");
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
    const [h, m] = value.split(":");
    const hour = parseInt(h, 10);
    const twelveHour = hour > 12 ? hour - 12 : hour === 0 ? 12 : hour;
    return `${twelveHour}:${m} ${hour >= 12 ? "PM" : "AM"}`;
}

function getDaysUntil(dateStr, today) {
    const diff = Math.ceil(
        (new Date(dateStr + "T00:00:00").getTime() -
            new Date(today + "T00:00:00").getTime()) / 86400000
    );

    if (diff === 0) return "Today!";
    if (diff === 1) return "Tomorrow";
    if (diff > 0) return `In ${diff} days`;
    return `${Math.abs(diff)} days ago`;
}

function loadBookings() {
    const raw = localStorage.getItem(STORAGE_KEY);

    if (!raw) {
        bookings = seedDemoBookings();
        localStorage.setItem(STORAGE_KEY, JSON.stringify(bookings));
        return;
    }

    try {
        const parsed = JSON.parse(raw);
        bookings = Array.isArray(parsed) && parsed.length ? parsed : seedDemoBookings();
        if (!Array.isArray(parsed) || parsed.length === 0) {
            localStorage.setItem(STORAGE_KEY, JSON.stringify(bookings));
        }
    } catch {
        bookings = seedDemoBookings();
        localStorage.setItem(STORAGE_KEY, JSON.stringify(bookings));
    }
}

function getVisibleBookings() {
    const today = new Date().toISOString().split("T")[0];

    const filtered = bookings.filter(booking => {
        if (filter === "upcoming") return booking.date >= today;
        if (filter === "past") return booking.date < today;
        return true;
    });

    return [...filtered].sort((a, b) => a.date.localeCompare(b.date));
}

function bookingTotal(booking) {
    const pkg = packageNames[booking.packageId];
    const addonTotal = (booking.addOns || []).reduce(
        (sum, id) => sum + (addOnDetails[id]?.price || 0),
        0
    );

    return (pkg?.price || 0) + addonTotal;
}

function renderBookings() {
    const root = document.getElementById("bookingList");
    const empty = document.getElementById("emptyState");
    const count = document.getElementById("bookingCount");
    const today = new Date().toISOString().split("T")[0];
    const visible = getVisibleBookings();

    document.querySelectorAll(".filter-tab").forEach(button => {
        button.classList.toggle("active", button.dataset.filter === filter);
    });

    if (visible.length === 0) {
        root.innerHTML = "";
        empty.hidden = false;
        count.hidden = true;

        const title = document.getElementById("emptyTitle");
        const copy = document.getElementById("emptyCopy");

        title.textContent = filter === "past" ? "No Past Bookings" : "No Upcoming Bookings";
        copy.textContent = filter === "past"
            ? "You don't have any past bookings yet."
            : "You haven't made any bookings yet. Book your first celebration setup!";

        return;
    }

    empty.hidden = true;
    count.hidden = false;
    count.textContent = `Showing ${visible.length} booking${visible.length !== 1 ? "s" : ""}`;

    root.innerHTML = visible.map(booking => {
        const pkg = packageNames[booking.packageId];
        const total = bookingTotal(booking);
        const isPast = booking.date < today;
        const isOpen = expanded === booking.id;
        const statusClass = isPast ? "past" : "upcoming";
        const statusText = isPast ? "Completed" : booking.status;

        return `
      <article class="booking-card ${isPast ? "" : "upcoming"}">
        <button class="booking-summary" type="button" data-toggle-booking="${booking.id}">
          <div class="occasion-icon">${occasionIcons[booking.occasion] || "🎉"}</div>

          <div class="booking-summary-main">
            <div class="booking-summary-title-row">
              <h3 class="booking-summary-title">${escapeHtml(booking.occasion)} — ${escapeHtml(booking.firstName)} ${escapeHtml(booking.lastName)}</h3>
              <span class="status-pill ${statusClass}">${escapeHtml(statusText)}</span>
            </div>
            <p class="booking-summary-date">${formatDate(booking.date)} at ${formatTime(booking.time)}</p>
            <p class="booking-summary-address">${escapeHtml(booking.address)}, ${escapeHtml(booking.city)}</p>
          </div>

          <div class="booking-summary-right">
            <p class="booking-price">R${formatMoney(total)}</p>
            ${!isPast ? `<span class="days-pill">${getDaysUntil(booking.date, today)}</span>` : ""}
            <div class="details-toggle">${isOpen ? "▲ Less" : "▼ Details"}</div>
          </div>
        </button>

        ${isOpen ? renderDetails(booking, pkg, total, isPast) : ""}
      </article>
    `;
    }).join("");

    document.querySelectorAll("[data-toggle-booking]").forEach(button => {
        button.addEventListener("click", () => {
            expanded = expanded === button.dataset.toggleBooking ? null : button.dataset.toggleBooking;
            renderBookings();
        });
    });

    document.querySelectorAll("[data-cancel-booking]").forEach(button => {
        button.addEventListener("click", event => {
            event.stopPropagation();
            cancelBooking(button.dataset.cancelBooking);
        });
    });
}

function renderDetails(booking, pkg, total, isPast) {
    return `
    <div class="booking-details">
      <div class="details-grid">
        <div class="detail-box">
          <h4>Package</h4>
          <p class="detail-main">${pkg?.name || "Unknown package"}</p>
          <p class="detail-sub">R${formatMoney(pkg?.price)}</p>
        </div>

        <div class="detail-box">
          <h4>Booking Reference</h4>
          <p class="detail-main" style="font-family:monospace;">${escapeHtml(booking.id)}</p>
          <p class="detail-sub">Booked ${new Date(booking.createdAt).toLocaleDateString("en-ZA")}</p>
        </div>

        ${(booking.addOns || []).length ? `
          <div class="detail-box full">
            <h4>Add-Ons</h4>
            <div class="addon-pills">
              ${(booking.addOns || []).map(id => {
        const addon = addOnDetails[id];
        if (!addon) return "";
        return `<span class="addon-pill">${addon.icon} ${escapeHtml(addon.name)} (+R${addon.price})</span>`;
    }).join("")}
            </div>
          </div>
        ` : ""}

        ${booking.notes ? `
          <div class="detail-box full">
            <h4>Special Instructions</h4>
            <p style="color:var(--muted-foreground);font-size:14px;font-style:italic;">${escapeHtml(booking.notes)}</p>
          </div>
        ` : ""}

        <div class="detail-total-row">
          <div>
            <div class="detail-total-label">* Transport fee is quoted separately</div>
          </div>
          <div>
            <div class="detail-total-label" style="text-align:right;text-transform:uppercase;letter-spacing:.08em;">Total</div>
            <div class="detail-total-price">R${formatMoney(total)}</div>
          </div>
        </div>
      </div>

      ${!isPast ? `
        <div class="booking-actions">
          <a
            class="btn btn-outline whatsapp"
            href="https://wa.me/27731232660"
            target="_blank"
            rel="noreferrer"
          >
            📞 Contact via WhatsApp
          </a>
          <button type="button" class="btn btn-outline cancel" data-cancel-booking="${escapeHtml(booking.id)}">Cancel</button>
        </div>
      ` : ""}
    </div>
  `;
}

function cancelBooking(id) {
    const confirmed = window.confirm("Cancel this booking?");
    if (!confirmed) return;

    bookings = bookings.filter(booking => booking.id !== id);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(bookings));

    if (expanded === id) expanded = null;
    renderBookings();
}

document.addEventListener("DOMContentLoaded", () => {
    loadBookings();

    document.querySelectorAll(".filter-tab").forEach(button => {
        button.addEventListener("click", () => {
            filter = button.dataset.filter;
            expanded = null;
            renderBookings();
        });
    });

    renderBookings();
});
