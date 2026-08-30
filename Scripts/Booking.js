/* Booking.js
   Client-side booking form logic — four-step state machine.
   Step 4 (Banking Details) is a simulated payment step: fields are
   validated client-side but NEVER sent to the server. Only the real
   booking payload goes to /Cust/CreateBooking.
   Enforces phone: exactly 9 digits and must not start with 0.
   Enforces FirstName/LastName: letters only (A–Z / a–z).
*/

const packages = [
    { id: "basic", name: "Basic Package", price: 650, badge: "Starter" },
    { id: "standard", name: "Standard Package", price: 850, badge: "Popular" },
    { id: "premium", name: "Premium Package", price: 1000, badge: "Premium" }
];

const addOns = [
    { id: "balloons", name: "Extra Balloon Bouquet", price: 50, icon: "🎈" },
    { id: "confetti", name: "Confetti Cannon", price: 80, icon: "🎊" },
    { id: "lights", name: "LED Fairy Lights", price: 120, icon: "✨" },
    { id: "flowerwall", name: "Flower Wall Backdrop", price: 200, icon: "🌸" },
    { id: "letters", name: "Custom Letter/Number Balloons", price: 150, icon: "🔡" },
    { id: "candles", name: "Scented Candle Set", price: 90, icon: "🕯️" }
];

const state = {
    step: 1,
    submitted: false,
    serverTotalPrice: null,
    form: {
        firstName: "",
        lastName: "",
        email: "",
        phone: "",
        occasion: "",
        date: "",
        time: "",
        address: "",
        city: "",
        notes: "",
        packageId: "",
        addOns: []
    },
    // Step 4 — UI-only. Never sent to the server, never touches the booking payload.
    banking: {
        cardholderName: "",
        cardNumber: "",     // digits only, formatted for display at render time
        expiryDate: "",     // "MM/YY"
        cvv: "",
        streetAddress: "",
        billingCity: "",
        postalCode: ""
    }
};

function getSelectedPackage() {
    return packages.find(pkg => pkg.id === state.form.packageId);
}

function getSelectedAddOns() {
    return addOns.filter(addon => state.form.addOns.includes(addon.id));
}

function getTotal() {
    const packagePrice = getSelectedPackage()?.price || 0;
    return packagePrice + getSelectedAddOns().reduce((sum, addon) => sum + addon.price, 0);
}

function escapeHtml(value) {
    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}

function formatMoney(value) {
    return Number(value || 0).toLocaleString("en-ZA");
}

function loadQueryPackage() {
    const pkg = new URLSearchParams(window.location.search).get("package");
    if (packages.some(item => item.id === pkg)) {
        state.form.packageId = pkg;
    }
}

function isPhoneValid(value) {
    // Exactly 9 digits, first digit 1-9 (no leading 0)
    return /^[1-9][0-9]{8}$/.test(String(value || "").trim());
}

function isNameValid(value) {
    return /^[A-Za-z]+$/.test(String(value || "").trim());
}

function isStep1Valid() {
    return Boolean(
        isNameValid(state.form.firstName) &&
        isNameValid(state.form.lastName) &&
        state.form.email.includes("@") &&
        isPhoneValid(state.form.phone) &&
        state.form.occasion &&
        state.form.city && state.form.city.trim()
    );
}

function isStep2Valid() {
    return Boolean(
        state.form.packageId &&
        state.form.date &&
        state.form.time &&
        state.form.address.trim()
    );
}

// ---- Step 4 (banking) helpers ----

function digitsOnly(value) {
    return String(value || "").replace(/\D/g, "");
}

function formatCardNumberDisplay(digits) {
    const groups = digits.match(/.{1,4}/g);
    return groups ? groups.join(" ") : "";
}

function detectCardBrand(digits) {
    if (/^4/.test(digits)) return "Visa";
    if (/^5[1-5]/.test(digits)) return "Mastercard";
    if (/^3[47]/.test(digits)) return "Amex";
    return "";
}

function isExpiryValid(value) {
    const match = /^(\d{2})\/(\d{2})$/.exec(value || "");
    if (!match) return false;
    const month = parseInt(match[1], 10);
    return month >= 1 && month <= 12;
}

function isStep4Valid() {
    const b = state.banking;
    return Boolean(
        b.cardholderName.trim().length > 1 &&
        digitsOnly(b.cardNumber).length >= 13 &&
        isExpiryValid(b.expiryDate) &&
        (b.cvv.length === 3 || b.cvv.length === 4) &&
        b.streetAddress.trim().length > 0 &&
        b.billingCity.trim().length > 0 &&
        b.postalCode.length === 4
    );
}

function updateStep4Button() {
    const button = document.getElementById("confirmBookingFinal");
    const hint = document.getElementById("fillHint");
    const valid = isStep4Valid();
    if (button) button.disabled = !valid;
    if (hint) hint.hidden = valid;
}

// ---- Progress indicator ----

function renderProgress() {
    document.querySelectorAll(".step-circle").forEach((circle, index) => {
        circle.classList.toggle("active", state.step >= index + 1);
    });

    document.querySelectorAll(".step-line").forEach((line, index) => {
        line.classList.toggle("active", state.step > index + 1);
    });

    document.querySelectorAll(".step-labels span").forEach((label, index) => {
        label.classList.toggle("active", state.step >= index + 1);
    });
}

// ---- Main render ----

function renderBookingStep() {
    const root = document.getElementById("bookingStep");
    renderProgress();

    if (state.step === 1) {
        root.innerHTML = `
      <h2>Your Details</h2>
      <div class="form-grid two">
        <div class="form-group">
          <label class="form-label" for="firstName">First Name</label>
          <input class="form-control" id="firstName" name="firstName" inputmode="text" pattern="^[A-Za-z]+$" maxlength="50" value="${escapeHtml(state.form.firstName)}" placeholder="Nomsa" required>
          <small class="muted">Letters only (A–Z).</small>
        </div>
        <div class="form-group">
          <label class="form-label" for="lastName">Last Name</label>
          <input class="form-control" id="lastName" name="lastName" inputmode="text" pattern="^[A-Za-z]+$" maxlength="50" value="${escapeHtml(state.form.lastName)}" placeholder="Mabaso" required>
          <small class="muted">Letters only (A–Z).</small>
        </div>

        <div class="form-group">
          <label class="form-label" for="email">Email Address</label>
          <input class="form-control" id="email" name="email" type="email" value="${escapeHtml(state.form.email)}" placeholder="nomsa@example.com" required>
        </div>
        <div class="form-group">
          <label class="form-label" for="phone">Phone / WhatsApp</label>
          <div class="phone-row">
            <div class="phone-prefix">+27</div>
            <input class="form-control" id="phone" name="phone" inputmode="numeric" pattern="^[1-9][0-9]{8}$" maxlength="9" value="${escapeHtml(state.form.phone)}" placeholder="723456789" required>
          </div>
          <small class="muted">Enter 9 digits (do not include leading 0).</small>
        </div>

        <div class="form-group full">
          <label class="form-label" for="occasion">Occasion Type</label>
          <select class="form-control" id="occasion" name="occasion" required>
            <option value="">Select an occasion…</option>
            ${["Birthday", "Anniversary", "Graduation", "Valentine's Day", "Baby Shower", "Other"]
                .map(o => `<option value="${escapeHtml(o)}" ${state.form.occasion === o ? "selected" : ""}>${escapeHtml(o)}</option>`).join("")}
          </select>
        </div>

        <div class="form-group full">
          <label class="form-label" for="city">City / Town</label>
          <select class="form-control" id="city" name="city" required>
            <option value="">Select city…</option>
            <option value="Pietermaritzburg" ${state.form.city === "Pietermaritzburg" ? "selected" : ""}>Pietermaritzburg</option>
            <option value="Durban" ${state.form.city === "Durban" ? "selected" : ""}>Durban</option>
            <option value="Mandeni" ${state.form.city === "Mandeni" ? "selected" : ""}>Mandeni</option>
          </select>
         
        </div>
      </div>
      <div class="form-actions" style="justify-content:flex-end;">
        <button type="button" class="btn btn-primary" id="nextStep1" ${isStep1Valid() ? "" : "disabled"}>Next: Event Info →</button>
      </div>
    `;
    }

    if (state.step === 2) {
        const today = new Date().toISOString().split("T")[0];

        root.innerHTML = `
      <h2>Event Details</h2>

      <p class="form-label">Select Your Package</p>
      <div class="package-select-grid">
        ${packages.map(pkg => `
          <button type="button" class="package-choice ${state.form.packageId === pkg.id ? "selected" : ""}" data-package="${pkg.id}">
            <span class="choice-badge">${pkg.badge}</span>
            <p class="choice-title">${pkg.name}</p>
            <div class="choice-price">R${formatMoney(pkg.price)}</div>
          </button>
        `).join("")}
      </div>

      <div class="form-grid two">
        <div class="form-group">
          <label class="form-label" for="date">Event Date</label>
          <input class="form-control" id="date" name="date" type="date" min="${today}" value="${escapeHtml(state.form.date)}" required>
        </div>
        <div class="form-group">
          <label class="form-label" for="time">Setup Time</label>
          <input class="form-control" id="time" name="time" type="time" value="${escapeHtml(state.form.time)}" required>
        </div>
        <div class="form-group full">
          <label class="form-label" for="address">Event Address</label>
          <input class="form-control" id="address" name="address" value="${escapeHtml(state.form.address)}" placeholder="12 Celebration Street, Sandton" required>
        </div>
        <div class="form-group full">
          <label class="form-label" for="city">City / Town</label>
          <input class="form-control" id="city" name="city" value="${escapeHtml(state.form.city)}" placeholder="Johannesburg" required>
        </div>
      </div>

      <p class="form-label" style="margin-top:24px;">Optional Add-Ons</p>
      <div class="addon-select-grid">
        ${addOns.map(addon => {
            const selected = state.form.addOns.includes(addon.id);
            return `
            <button type="button" class="addon-choice ${selected ? "selected" : ""}" data-addon="${addon.id}">
              <span class="icon">${addon.icon}</span>
              <span>
                <div class="name">${addon.name}</div>
                <div class="price">+R${addon.price}</div>
              </span>
            </button>
          `;
        }).join("")}
      </div>

      <div class="form-group">
        <label class="form-label" for="notes">Special Instructions (optional)</label>
        <textarea class="form-control" id="notes" name="notes" rows="3" placeholder="Any colour preferences, theme, or special requests…">${escapeHtml(state.form.notes)}</textarea>
      </div>

      <div class="form-actions">
        <button type="button" class="btn btn-outline" id="backStep2">← Back</button>
        <button type="button" class="btn btn-primary" id="nextStep2" ${isStep2Valid() ? "" : "disabled"}>Review Booking →</button>
      </div>
    `;
    }

    if (state.step === 3) {
        const selectedPackage = getSelectedPackage();
        const selectedAddOns = getSelectedAddOns();
        const total = getTotal();

        root.innerHTML = `
      <h2>Review Your Booking</h2>

      <div class="review-stack">
        <div class="review-box">
          <div class="review-title">Your Details</div>
          <div class="review-grid">
            <span class="label">Name</span><span>${escapeHtml(state.form.firstName)} ${escapeHtml(state.form.lastName)}</span>
            <span class="label">Email</span><span>${escapeHtml(state.form.email)}</span>
            <span class="label">Phone</span><span>+27 ${escapeHtml(state.form.phone)}</span>
            <span class="label">Occasion</span><span>${escapeHtml(state.form.occasion)}</span>
          </div>
        </div>

        <div class="review-box">
          <div class="review-title">Event Details</div>
          <div class="review-grid">
            <span class="label">Date</span><span>${escapeHtml(state.form.date)}</span>
            <span class="label">Time</span><span>${escapeHtml(state.form.time)}</span>
            <span class="label">Address</span><span>${escapeHtml(state.form.address)}, ${escapeHtml(state.form.city)}</span>
          </div>
        </div>

        <div class="review-box">
          <div class="review-title">Package & Pricing</div>
          <div class="review-row">
            <span class="muted">${selectedPackage?.name || "No package selected"}</span>
            <span>R${formatMoney(selectedPackage?.price)}</span>
          </div>
          ${selectedAddOns.map(a => `
            <div class="review-row">
              <span class="muted">${a.icon} ${a.name}</span>
              <span>R${a.price}</span>
            </div>
          `).join("")}
          <div class="review-total">
            <span>Total</span>
            <span class="total-value">R${formatMoney(total)}</span>
          </div>
          <p class="transport-note">* Transport fee quoted separately upon confirmation</p>
        </div>

        ${state.form.notes ? `
          <div class="review-box">
            <div class="review-title">Special Instructions</div>
            <p style="margin:0;color:var(--muted-foreground);font-size:14px;font-style:italic;">${escapeHtml(state.form.notes)}</p>
          </div>
        ` : ""}
      </div>

      <div class="form-actions">
        <button type="button" class="btn btn-outline" id="backStep3">← Edit</button>
        <button type="button" class="btn btn-gradient" id="nextStep3">Continue to Banking Details →</button>
      </div>
    `;
    }

    if (state.step === 4) {
        const total = getTotal();
        const cardDigits = digitsOnly(state.banking.cardNumber);

        root.innerHTML = `
      <div class="payment-heading">Banking Details</div>
      <p class="payment-subtext">This is a simulated payment step for demonstration purposes — no real charge will be made.</p>

      <div class="order-summary-strip">
        <span class="summary-label">Estimated Total</span>
        <span class="summary-total">R${formatMoney(total)}</span>
      </div>

      <div class="field-group">
        <label class="field-label" for="cardholderName">Cardholder Name</label>
        <input class="field-input" id="cardholderName" name="cardholderName" value="${escapeHtml(state.banking.cardholderName)}" placeholder="Name as it appears on card" required>
      </div>

      <div class="field-group card-number-wrap">
        <label class="field-label" for="cardNumber">Card Number</label>
        <input class="field-input" id="cardNumber" name="cardNumber" inputmode="numeric" maxlength="19" value="${escapeHtml(formatCardNumberDisplay(cardDigits))}" placeholder="0000 0000 0000 0000" required>
        <span class="card-brand" id="cardBrandLabel">${detectCardBrand(cardDigits)}</span>
      </div>

      <div class="field-row">
        <div class="field-group">
          <label class="field-label" for="expiryDate">Expiry Date</label>
          <input class="field-input" id="expiryDate" name="expiryDate" inputmode="numeric" maxlength="5" value="${escapeHtml(state.banking.expiryDate)}" placeholder="MM/YY" required>
        </div>
        <div class="field-group">
          <label class="field-label" for="cvv">CVV</label>
          <input class="field-input" id="cvv" name="cvv" inputmode="numeric" maxlength="4" value="${escapeHtml(state.banking.cvv)}" placeholder="123" required>
        </div>
      </div>

      <div class="section-divider" style="height:1px;background:var(--border-pink,#f9d0e3);margin:6px 0 20px;"></div>

      <div class="billing-section">
        <div class="field-group">
          <label class="field-label" for="streetAddress">Street Address</label>
          <input class="field-input" id="streetAddress" name="streetAddress" value="${escapeHtml(state.banking.streetAddress)}" placeholder="123 Main Street" required>
        </div>
        <div class="field-row">
          <div class="field-group">
            <label class="field-label" for="billingCity">City</label>
            <input class="field-input" id="billingCity" name="billingCity" value="${escapeHtml(state.banking.billingCity)}" placeholder="Durban" required>
          </div>
          <div class="field-group">
            <label class="field-label" for="postalCode">Postal Code</label>
            <input class="field-input" id="postalCode" name="postalCode" inputmode="numeric" maxlength="4" value="${escapeHtml(state.banking.postalCode)}" placeholder="4001" required>
          </div>
        </div>
      </div>

      <div class="security-badges">
        <span>🔒 Simulated step — card details are never stored or sent anywhere.</span>
      </div>

      <div class="payment-actions">
        <button type="button" class="ghost-btn" id="backStep4">← Back to Review</button>
        <div class="confirm-wrap">
          <p class="fill-hint" id="fillHint" ${isStep4Valid() ? "hidden" : ""}>Fill all fields to confirm</p>
          <button type="button" class="pink-btn" id="confirmBookingFinal" ${isStep4Valid() ? "" : "disabled"}>Confirm Booking ✓</button>
        </div>
      </div>
    `;
    }

    attachStepHandlers();
}

function attachStepHandlers() {
    document.querySelectorAll("#bookingStep input, #bookingStep select, #bookingStep textarea").forEach(control => {
        control.addEventListener("input", handleFormInput);
        control.addEventListener("change", handleFormInput);
    });

    document.getElementById("nextStep1")?.addEventListener("click", () => {
        if (!isStep1Valid()) return;
        state.step = 2;
        renderBookingStep();
    });

    document.getElementById("nextStep2")?.addEventListener("click", () => {
        if (!isStep2Valid()) return;
        state.step = 3;
        renderBookingStep();
    });

    document.getElementById("backStep2")?.addEventListener("click", () => {
        state.step = 1;
        renderBookingStep();
    });

    document.getElementById("backStep3")?.addEventListener("click", () => {
        state.step = 2;
        renderBookingStep();
    });

    document.getElementById("nextStep3")?.addEventListener("click", () => {
        state.step = 4;
        renderBookingStep();
    });

    document.getElementById("backStep4")?.addEventListener("click", () => {
        state.step = 3;
        renderBookingStep();
    });

    document.getElementById("confirmBookingFinal")?.addEventListener("click", () => {
        if (!isStep4Valid()) return;
        submitBooking();
    });

    document.querySelectorAll("[data-package]").forEach(button => {
        button.addEventListener("click", () => {
            state.form.packageId = button.dataset.package;
            renderBookingStep();
        });
    });

    document.querySelectorAll("[data-addon]").forEach(button => {
        button.addEventListener("click", () => {
            const id = button.dataset.addon;
            state.form.addOns = state.form.addOns.includes(id)
                ? state.form.addOns.filter(item => item !== id)
                : [...state.form.addOns, id];

            renderBookingStep();
        });
    });
}

function handleFormInput(event) {
    const control = event.target;
    if (!control.name) return;

    if (control.name === "phone") {
        let v = digitsOnly(control.value).slice(0, 9);
        control.value = v;
        state.form.phone = v;
        const btn = document.getElementById("nextStep1");
        if (btn) btn.disabled = !isStep1Valid();
        return;
    }

    if (control.name === "firstName" || control.name === "lastName") {
        let v = String(control.value || "").replace(/[^A-Za-z]/g, "").slice(0, 50);
        control.value = v;
        state.form[control.name] = v;
        const btn = document.getElementById("nextStep1");
        if (btn) btn.disabled = !isStep1Valid();
        return;
    }

    // ---- Step 4 banking fields ----

    if (control.name === "cardNumber") {
        const digits = digitsOnly(control.value).slice(0, 16);
        control.value = formatCardNumberDisplay(digits);
        state.banking.cardNumber = digits;
        const brandLabel = document.getElementById("cardBrandLabel");
        if (brandLabel) brandLabel.textContent = detectCardBrand(digits);
        updateStep4Button();
        return;
    }

    if (control.name === "expiryDate") {
        const digits = digitsOnly(control.value).slice(0, 4);
        control.value = digits.length >= 3 ? digits.slice(0, 2) + "/" + digits.slice(2) : digits;
        state.banking.expiryDate = control.value;
        updateStep4Button();
        return;
    }

    if (control.name === "cvv") {
        const digits = digitsOnly(control.value).slice(0, 4);
        control.value = digits;
        state.banking.cvv = digits;
        updateStep4Button();
        return;
    }

    if (control.name === "postalCode") {
        const digits = digitsOnly(control.value).slice(0, 4);
        control.value = digits;
        state.banking.postalCode = digits;
        updateStep4Button();
        return;
    }

    if (control.name === "cardholderName" || control.name === "streetAddress" || control.name === "billingCity") {
        state.banking[control.name] = control.value;
        updateStep4Button();
        return;
    }

    // ---- Steps 1–2 generic fields ----

    state.form[control.name] = control.value;

    if (state.step === 1) {
        const button = document.getElementById("nextStep1");
        if (button) button.disabled = !isStep1Valid();
    }

    if (state.step === 2) {
        const button = document.getElementById("nextStep2");
        if (button) button.disabled = !isStep2Valid();
    }
}

async function submitBooking() {
    const bookingUrl = document.getElementById("bookingApp").dataset.bookingUrl;

    const confirmBtn = document.getElementById("confirmBookingFinal");
    if (confirmBtn) {
        confirmBtn.disabled = true;
        confirmBtn.textContent = "Confirming…";
    }

    // Only real booking data is sent — never card/banking fields.
    const booking = {
        firstName: state.form.firstName,
        lastName: state.form.lastName,
        email: state.form.email,
        phone: state.form.phone,
        occasion: state.form.occasion,

        eventDate: state.form.date,
        eventTime: state.form.time,

        address: state.form.address,
        city: state.form.city,
        notes: state.form.notes,

        packageId: state.form.packageId,
        addOns: state.form.addOns,
    };

    try {
        const response = await fetch(bookingUrl, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(booking)
        });

        const result = await response.json();

        if (!response.ok || !result.success) {
            throw new Error(result.message || "Unable to create booking.");
        }

        state.submitted = true;
        state.form.bookingId = result.bookingId;

        if (result.totalPrice === undefined || result.totalPrice === null) {
            throw new Error("The server did not return a booking total.");
        }

        state.serverTotalPrice = Number(result.totalPrice);

        renderConfirmation();

    } catch (error) {
        console.error("Booking submission failed:", error);

        alert(
            error.message ||
            "Something went wrong while submitting your booking. Please try again."
        );

        if (confirmBtn) {
            confirmBtn.disabled = !isStep4Valid();
            confirmBtn.textContent = "Confirm Booking ✓";
        }
    }
}

function renderConfirmation() {
    const pkg = getSelectedPackage();
    const total = state.serverTotalPrice;
    const root = document.getElementById("bookingApp");

    root.innerHTML = `
    <div class="confirmation">
      <div class="confirmation-card">
        <div class="confirmation-icon">✓</div>
        <h2>Booking Confirmed!</h2>
        <p style="margin-bottom:8px;font-size:14px;color:var(--muted-foreground);">
          Thank you, <strong>${escapeHtml(state.form.firstName)}</strong>! Your celebration setup is booked.
        </p>

        <div class="summary-mini">
          <div class="summary-mini-row"><span class="summary-mini-label">Package</span><strong>${pkg?.name || ""}</strong></div>
          <div class="summary-mini-row"><span class="summary-mini-label">Date</span><strong>${escapeHtml(state.form.date)}</strong></div>
          <div class="summary-mini-row"><span class="summary-mini-label">Time</span><strong>${escapeHtml(state.form.time)}</strong></div>
          <div class="summary-mini-row"><span class="summary-mini-label">Total</span><strong style="color:var(--primary);">R${formatMoney(total)}</strong></div>
        </div>

        <p class="confirmation-note">We'll be in touch via WhatsApp to confirm. Transport fee quoted separately.</p>

        <div class="confirmation-actions">
          <a href="/Cust/ViewBooking" class="btn btn-outline">View Bookings</a>
          <a href="/Cust/Index" class="btn btn-primary">Back Home</a>
        </div>
      </div>
    </div>
  `;
}

document.addEventListener("DOMContentLoaded", () => {
    loadQueryPackage();

    const app = document.getElementById("bookingApp");
    if (!app) return;

    renderBookingStep();
});