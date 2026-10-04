/* Booking.js
   Client-side booking form logic — four-step state machine.
   Step 4 (Banking Details) is a simulated payment step: fields are
   validated client-side but NEVER sent to the server. Only the real
   booking payload goes to /Cust/CreateBooking.
   Enforces phone: exactly 9 digits and must not start with 0.
   Enforces FirstName/LastName: letters only (A–Z / a–z).
*/

const occasionPackages = {
    "Birthday": [
        { id:"basic", name:"Basic Package", price:650, badge:"Starter", description:"A simple birthday setup with the essential decorative touches for a warm, polished celebration.", inclusions:["1 birthday banner","40 ceiling balloons","10 floor balloons","Artificial rose petals"] },
        { id:"standard", name:"Standard Package", price:850, badge:"Popular", description:"A fuller birthday setup with more volume, colour and personalised celebration details.", inclusions:["1 birthday banner","50 ceiling balloons","20 floor balloons","8 feature balloons","Age-number balloons","Artificial rose petals"] },
        { id:"premium", name:"Premium Package", price:1000, badge:"Premium", description:"An elevated birthday setup with richer styling and extra statement details throughout the space.", inclusions:["1 birthday banner","Bed-area ceiling balloon styling","30 floor balloons","10 feature balloons","6 accent balloons","Age-number balloons","Rose-petal styling","Decorative candles"] }
    ],
    "Anniversary": [
        { id:"basic", name:"Basic Package", price:700, badge:"Starter", description:"A romantic anniversary setup with soft decorative details for an intimate celebration.", inclusions:["1 anniversary banner","30 ceiling balloons","10 floor balloons","Heart accents","Artificial rose petals"] },
        { id:"standard", name:"Standard Package", price:950, badge:"Popular", description:"A richer anniversary setup with layered romantic styling and additional feature decorations.", inclusions:["1 anniversary banner","45 ceiling balloons","18 floor balloons","6 heart balloons","Bed styling","Artificial rose petals","Decorative candles"] },
        { id:"premium", name:"Premium Package", price:1500, badge:"Premium", description:"A complete romantic experience with statement styling and premium decorative details.", inclusions:["Premium anniversary banner styling","Full ceiling balloon arrangement","30 floor balloons","10 heart balloons","Feature bed styling","Rose petals on bed and floor","Decorative candle arrangement","Romantic accent décor"] }
    ],
    "Graduation": [
        { id:"basic", name:"Basic Package", price:650, badge:"Starter", description:"A clean graduation setup celebrating the milestone with essential congratulatory décor.", inclusions:["1 congratulations banner","30 ceiling balloons","10 floor balloons","Graduation-themed accents"] },
        { id:"standard", name:"Standard Package", price:900, badge:"Popular", description:"A fuller graduation display with stronger visual impact and more personalised milestone details.", inclusions:["1 congratulations banner","45 ceiling balloons","18 floor balloons","Graduation number/letter balloons","Feature backdrop accents","Celebration confetti details"] },
        { id:"premium", name:"Premium Package", price:1200, badge:"Premium", description:"An elevated graduation setup with statement décor designed for photos and a memorable reveal.", inclusions:["Premium graduation banner styling","Full balloon arrangement","25 floor balloons","Graduation number/letter balloons","Statement backdrop accents","Photo-area styling","Premium celebration details"] }
    ],
    "Valentine's Day": [
        { id:"basic", name:"Basic Package", price:750, badge:"Starter", description:"A sweet Valentine's setup with romantic essentials and a soft, intimate atmosphere.", inclusions:["1 love-themed banner","30 ceiling balloons","10 floor balloons","4 heart balloons","Artificial rose petals"] },
        { id:"standard", name:"Standard Package", price:1050, badge:"Popular", description:"A fuller Valentine's experience with more romantic detail, heart accents and mood-setting décor.", inclusions:["1 love-themed banner","45 ceiling balloons","18 floor balloons","8 heart balloons","Bed styling","Rose petals on bed and floor","Decorative candles"] }
    ],
    "Baby Shower": [
        { id:"basic", name:"Basic Package", price:800, badge:"Starter", description:"A gentle baby-shower setup with coordinated decorations and essential celebration details.", inclusions:["1 baby-shower banner","30 ceiling balloons","12 floor balloons","Baby-themed decorative accents"] },
        { id:"standard", name:"Standard Package", price:1100, badge:"Popular", description:"A fuller baby-shower setup with layered balloon styling and more themed decorative elements.", inclusions:["1 baby-shower banner","45 ceiling balloons","20 floor balloons","Themed feature balloons","Table/feature-area accents","Photo-area details"] },
        { id:"premium", name:"Premium Package", price:1600, badge:"Premium", description:"A complete baby-shower experience with statement styling and premium themed finishing touches.", inclusions:["Premium baby-shower banner styling","Full balloon arrangement","30 floor balloons","Feature balloon cluster","Statement backdrop accents","Photo-area styling","Premium themed décor"] }
    ]
};

function isCustomOccasion() {
    return state.form.occasion === "Other";
}

function getPackagesForOccasion() {
    return occasionPackages[state.form.occasion] || [];
}

const addOns = [
    { id: "balloons", name: "Extra Balloon Bouquet", price: 50, icon: "🎈" },
    { id: "confetti", name: "Confetti Cannon", price: 80, icon: "🎊" },
    { id: "lights", name: "LED Fairy Lights", price: 120, icon: "✨" },
    { id: "flowerwall", name: "Flower Wall Backdrop", price: 200, icon: "🌸" },
    { id: "letters", name: "Custom Letter/Number Balloons", price: 150, icon: "🔡" },
    { id: "candles", name: "Scented Candle Set", price: 90, icon: "🕯️" }
];

let bookingMap = null;
let bookingMarker = null;
let bookingMapResizeObserver = null;
let addressSearchTimer = null;
let addressSearchController = null;

const state = {
    step: 1,
    submitted: false,
    serverTotalPrice: null,
    paymentAmount: null,
    termsAccepted: false,
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
        addOns: [],
        latitude: null,
        longitude: null,
        locationValidated: false,
        locationConfirmed: false
    },
    // Step 4 card fields remain UI-only. Card data is never sent or stored.
    // Only the selected business payment amount and T&C acknowledgement are sent.
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
    return getPackagesForOccasion().find(pkg => pkg.id === state.form.packageId);
}

function getSelectedAddOns() {
    return addOns.filter(addon => state.form.addOns.includes(addon.id));
}

function getTotal() {
    if (isCustomOccasion()) return 0;
    const packagePrice = getSelectedPackage()?.price || 0;
    return packagePrice + getSelectedAddOns().reduce((sum, addon) => sum + addon.price, 0);
}

function getDaysUntilEvent() {
    if (!state.form.date) return null;
    const eventDate = new Date(state.form.date + "T00:00:00");
    const now = new Date();
    const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    return Math.round((eventDate.getTime() - today.getTime()) / 86400000);
}

function getMinimumPayment() {
    const total = getTotal();
    const daysUntil = getDaysUntilEvent();
    return daysUntil !== null && daysUntil <= 1 ? total : Math.round(total * 50) / 100;
}

function getSelectedPaymentAmount() {
    const amount = Number(state.paymentAmount);
    return Number.isFinite(amount) ? amount : getMinimumPayment();
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
    // Occasion selection must happen before a package can be selected.
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
        state.form.occasion &&
        (isCustomOccasion() || getSelectedPackage())
    );
}

function isStep2Valid() {
    return Boolean(
        state.form.date &&
        state.form.time &&
        state.form.city &&
        state.form.city.trim() &&
        state.form.address &&
        state.form.address.trim() &&
        state.form.locationValidated &&
        state.form.locationConfirmed &&
        state.form.latitude !== null &&
        state.form.longitude !== null
    );
}

function showLocationMessage(message, type = "") {
    const element = document.getElementById("locationMessage");
    if (!element) return;
    element.textContent = message;
    element.className = `location-message ${type}`;
}

function getSelectedCity() {
    return String(state.form.city || "").trim();
}

function getBookingEndpoint(name) {
    const app = document.getElementById("bookingApp");
    if (!app) return "";
    return app.dataset[name] || "";
}

function clearLocationSelection(clearAddress = false) {
    state.form.latitude = null;
    state.form.longitude = null;
    state.form.locationValidated = false;
    state.form.locationConfirmed = false;

    const latitude = document.getElementById("latitude");
    const longitude = document.getElementById("longitude");

    if (latitude) latitude.value = "";
    if (longitude) longitude.value = "";

    if (clearAddress) {
        state.form.address = "";
        const addressInput = document.getElementById("address");
        if (addressInput) addressInput.value = "";
    }

    const nextButton = document.getElementById("nextStep2");
    if (nextButton) nextButton.disabled = !isStep2Valid();
}

function showAddressSuggestions(results) {
    const list = document.getElementById("addressSuggestions");
    if (!list) return;

    if (!results || results.length === 0) {
        list.innerHTML = `
            <div class="address-suggestion-empty">
                No matching addresses were found.
            </div>`;
        list.classList.add("visible");
        return;
    }

    list.innerHTML = results.map((result, index) => {
        return `
            <button type="button"
                    class="address-suggestion"
                    data-result-index="${index}">
                <span class="address-suggestion-icon">📍</span>
                <span class="address-suggestion-text">
                    <strong>${escapeHtml(result.address || "Unnamed location")}</strong>
                    <small>
                        ${result.resolvedCity
                ? escapeHtml(result.resolvedCity)
                : "Select this location"}
                    </small>
                </span>
            </button>
        `;
    }).join("");

    list.classList.add("visible");

    list.querySelectorAll("[data-result-index]").forEach(button => {
        button.addEventListener("click", () => {
            const index = Number(button.dataset.resultIndex);
            const result = results[index];
            selectAddressResult(result);
        });
    });
}

function hideAddressSuggestions() {
    const list = document.getElementById("addressSuggestions");
    if (list) list.classList.remove("visible");
}

function isResultInsideSelectedCity(result) {
    return Boolean(
        result &&
        (result.inSelectedCity === true ||
         String(result.inSelectedCity).toLowerCase() === "true")
    );
}

function selectAddressResult(result) {
    if (!result) return;

    const city = getSelectedCity();
    const latitude = Number(result.latitude);
    const longitude = Number(result.longitude);

    if (!isResultInsideSelectedCity(result) ||
        !Number.isFinite(latitude) ||
        !Number.isFinite(longitude)) {
        clearLocationSelection(false);
        hideAddressSuggestions();

        showLocationMessage(
            `That address is outside the ${city} service area. Please choose another address or pin a location inside ${city}.`,
            "error"
        );

        updateStep2Button();
        return;
    }

    state.form.address = result.address || "";
    state.form.latitude = latitude;
    state.form.longitude = longitude;
    state.form.locationValidated = true;
    state.form.locationConfirmed = true;

    const addressInput = document.getElementById("address");
    const latitudeInput = document.getElementById("latitude");
    const longitudeInput = document.getElementById("longitude");

    if (addressInput) addressInput.value = state.form.address;
    if (latitudeInput) latitudeInput.value = state.form.latitude;
    if (longitudeInput) longitudeInput.value = state.form.longitude;

    hideAddressSuggestions();

    if (bookingMap && bookingMarker) {
        bookingMarker.setLatLng([latitude, longitude]);
        bookingMap.setView([latitude, longitude], 16);
        bookingMap.invalidateSize({ pan: false });
    }

    showLocationMessage(
        "✓ Address found and location confirmed.",
        "success"
    );

    updateStep2Button();
}

function updateStep2Button() {
    const nextButton = document.getElementById("nextStep2");
    if (nextButton) nextButton.disabled = !isStep2Valid();
}

async function searchEventAddresses(showMessage = true) {
    const addressInput = document.getElementById("address");
    const city = getSelectedCity();

    if (!addressInput) return [];

    const address = addressInput.value.trim();

    if (!city) {
        showLocationMessage(
            "Please select a valid map location first.",
            "error"
        );
        hideAddressSuggestions();
        return [];
    }

    if (address.length < 3) {
        hideAddressSuggestions();
        return [];
    }

    if (showMessage) {
        showLocationMessage("Searching for matching addresses...", "info");
    }

    if (addressSearchController) {
        addressSearchController.abort();
    }

    addressSearchController = new AbortController();

    const endpoint = getBookingEndpoint("addressSearchUrl");

    if (!endpoint) {
        showLocationMessage("The address search service is not configured.", "error");
        return [];
    }

    const url =
        `${endpoint}?address=${encodeURIComponent(address)}&city=${encodeURIComponent(city)}`;

    try {
        const response = await fetch(url, {
            method: "GET",
            headers: { "Accept": "application/json" },
            signal: addressSearchController.signal
        });

        const result = await response.json();

        if (!response.ok || !result.success) {
            throw new Error(result.message || "Address search failed.");
        }

        const results = result.results || [];
        showAddressSuggestions(results);

        if (results.length === 0) {
            showLocationMessage(
                "No matching address was found. Try a fuller street address or use Pin Location on Map.",
                "warning"
            );
        } else if (showMessage) {
            showLocationMessage(
                "Select an address from the list below.",
                "info"
            );
        }

        return results;
    } catch (error) {
        if (error.name === "AbortError") {
            return [];
        }

        console.error("Address search failed:", error);

        showLocationMessage(
            "We couldn't search for that address right now. Please try again or pin the location on the map.",
            "error"
        );

        hideAddressSuggestions();
        return [];
    }
}

function handleAddressTyping() {
    const addressInput = document.getElementById("address");
    if (!addressInput) return;

    // Any manual edit invalidates the previous selected coordinates.
    state.form.address = addressInput.value;
    state.form.latitude = null;
    state.form.longitude = null;
    state.form.locationValidated = false;
    state.form.locationConfirmed = false;

    const latitude = document.getElementById("latitude");
    const longitude = document.getElementById("longitude");

    if (latitude) latitude.value = "";
    if (longitude) longitude.value = "";

    updateStep2Button();

    clearTimeout(addressSearchTimer);

    if (addressInput.value.trim().length < 3) {
        hideAddressSuggestions();
        return;
    }

    addressSearchTimer = setTimeout(() => {
        searchEventAddresses(false);
    }, 650);
}

function getCityMapSettings(city) {
    const normalized = String(city || "").trim().toLowerCase();

    switch (normalized) {
        case "pietermaritzburg":
            return {
                lat: -29.6006,
                lng: 30.3794,
                zoom: 13,
                radiusKm: 25
            };

        case "mandeni":
        case "emandeni":
            return {
                lat: -29.1460,
                lng: 31.4070,
                zoom: 13,
                radiusKm: 20
            };

        case "durban":
        default:
            return {
                lat: -29.8587,
                lng: 31.0218,
                zoom: 12,
                radiusKm: 35
            };
    }
}

function getCityMapCenter(city) {
    const settings = getCityMapSettings(city);

    return {
        lat: settings.lat,
        lng: settings.lng
    };
}
 
function getCityMapBounds(city) {
    const settings = getCityMapSettings(city);
    const radiusKm = settings.radiusKm || 25;
    const latitudeDelta = radiusKm / 111;
    const longitudeScale =
        111 * Math.cos(settings.lat * Math.PI / 180);
    const longitudeDelta =
        longitudeScale > 0 ? radiusKm / longitudeScale : latitudeDelta;

    return [
        [settings.lat - latitudeDelta, settings.lng - longitudeDelta],
        [settings.lat + latitudeDelta, settings.lng + longitudeDelta]
    ];
}

function openLocationMap() {
    const mapContainer = document.getElementById("mapContainer");
    const mapElement = document.getElementById("bookingMap");

    if (!mapContainer || !mapElement) {
        console.error("Map container or bookingMap element not found.");
        return;
    }

    if (typeof L === "undefined") {
        console.error("Leaflet is not loaded.");
        showLocationMessage(
            "The map service could not load. Please refresh the page.",
            "error"
        );
        return;
    }

    const city = getSelectedCity();

    if (!city) {
        showLocationMessage(
            "Please select a city/town before opening the map.",
            "error"
        );
        return;
    }

    const settings = getCityMapSettings(city);

    // Make the map visible first. Leaflet must measure a real rendered size.
    mapContainer.style.display = "block";

    if (bookingMapResizeObserver) {
        bookingMapResizeObserver.disconnect();
        bookingMapResizeObserver = null;
    }

    if (bookingMap) {
        bookingMap.off();
        bookingMap.remove();
        bookingMap = null;
        bookingMarker = null;
    }

    if (mapElement._leaflet_id) {
        mapElement._leaflet_id = null;
    }

    let startLat = settings.lat;
    let startLng = settings.lng;
    let startZoom = settings.zoom;

    if (
        state.form.latitude !== null &&
        state.form.longitude !== null &&
        Number.isFinite(Number(state.form.latitude)) &&
        Number.isFinite(Number(state.form.longitude))
    ) {
        startLat = Number(state.form.latitude);
        startLng = Number(state.form.longitude);
        startZoom = 16;
    }

    // Wait until the browser has laid out the newly-visible container.
    requestAnimationFrame(() => {
        requestAnimationFrame(() => {
            bookingMap = L.map("bookingMap", {
                zoomControl: true,
                attributionControl: true,
                maxBounds: getCityMapBounds(city),
                maxBoundsViscosity: 0.85
            });

            bookingMap.setView([startLat, startLng], startZoom);

            L.tileLayer(
                "https://tile.openstreetmap.org/{z}/{x}/{y}.png",
                {
                    minZoom: 3,
                    maxZoom: 19,
                    updateWhenIdle: false,
                    keepBuffer: 3,
                    attribution: "&copy; OpenStreetMap contributors"
                }
            ).addTo(bookingMap);

            bookingMarker = L.marker(
                [startLat, startLng],
                { draggable: true }
            ).addTo(bookingMap);

            async function setSelectedMapLocation(latlng) {
                const lat = Number(latlng.lat);
                const lng = Number(latlng.lng);

                state.form.latitude = lat;
                state.form.longitude = lng;
                state.form.locationValidated = false;
                state.form.locationConfirmed = false;

                const latitudeInput = document.getElementById("latitude");
                const longitudeInput = document.getElementById("longitude");

                if (latitudeInput) latitudeInput.value = lat;
                if (longitudeInput) longitudeInput.value = lng;

                showLocationMessage(
                    "Location selected. Finding the address...",
                    "info"
                );

                await reverseGeocodeMapLocation(lat, lng);
            }

            bookingMap.on("click", function (event) {
                bookingMarker.setLatLng(event.latlng);
                setSelectedMapLocation(event.latlng);
            });

            bookingMarker.on("dragend", function () {
                setSelectedMapLocation(bookingMarker.getLatLng());
            });

            // Recalculate whenever the responsive booking card changes width.
            if (typeof ResizeObserver !== "undefined") {
                bookingMapResizeObserver = new ResizeObserver(() => {
                    if (bookingMap) {
                        bookingMap.invalidateSize({ pan: false, debounceMoveend: true });
                    }
                });
                bookingMapResizeObserver.observe(mapContainer);
            }

            // One immediate resize plus a delayed one handles fonts/nav/layout shifts.
            bookingMap.invalidateSize({ pan: false });
            setTimeout(() => {
                if (bookingMap) bookingMap.invalidateSize({ pan: false });
            }, 250);
        });
    });
}
async function reverseGeocodeMapLocation(lat, lng) {
    const city = getSelectedCity();
    const endpoint = getBookingEndpoint("addressReverseUrl");

    if (!city) {
        clearLocationSelection(false);
        showLocationMessage("Please select a city/town before choosing a map location.", "error");
        return;
    }

    if (!endpoint) {
        clearLocationSelection(false);
        showLocationMessage("The map address service is not configured.", "error");
        return;
    }

    const url =
        `${endpoint}?latitude=${encodeURIComponent(lat)}&longitude=${encodeURIComponent(lng)}&city=${encodeURIComponent(city)}`;

    try {
        const response = await fetch(url, {
            method: "GET",
            headers: { "Accept": "application/json" }
        });

        const result = await response.json();

        if (!response.ok || !result.success) {
            throw new Error(result.message || "Unable to verify the selected map location.");
        }

        const addressInput = document.getElementById("address");
        state.form.address = result.address || "";
        if (addressInput) addressInput.value = state.form.address;

        if (result.inSelectedCity !== true) {
            state.form.locationValidated = false;
            state.form.locationConfirmed = false;

            showLocationMessage(
                `That map location is outside the ${city} service area. Please move the pin to a location within ${city}.`,
                "error"
            );

            updateStep2Button();
            return;
        }

        state.form.latitude = Number(result.latitude);
        state.form.longitude = Number(result.longitude);
        state.form.locationValidated = true;
        state.form.locationConfirmed = false;

        showLocationMessage(
            `✓ Location found inside the ${city} service area. Click Confirm Location to continue.`,
            "success"
        );

        updateStep2Button();
    } catch (error) {
        console.error("Reverse geocoding failed:", error);
        state.form.locationValidated = false;
        state.form.locationConfirmed = false;

        showLocationMessage(
            error.message || "We could not verify that map location. Please try again.",
            "error"
        );

        updateStep2Button();
    }
}

function confirmEventLocation() {
    if (!state.form.locationValidated ||
        state.form.latitude === null ||
        state.form.longitude === null) {
        showLocationMessage(
            `Please select a valid location inside ${getSelectedCity()} first.`,
            "error"
        );
        return;
    }

    state.form.locationConfirmed = true;

    const latitude = document.getElementById("latitude");
    const longitude = document.getElementById("longitude");

    if (latitude) latitude.value = state.form.latitude;
    if (longitude) longitude.value = state.form.longitude;

    showLocationMessage(
        "✓ Event location confirmed successfully.",
        "success"
    );

    updateStep2Button();
}

async function findEventAddress() {
    const addressInput = document.getElementById("address");
    const city = getSelectedCity();

    if (!addressInput) return;

    if (!city) {
        showLocationMessage(
            "Please select a city/town before searching for an address.",
            "error"
        );
        return;
    }

    const address = addressInput.value.trim();

    if (!address) {
        showLocationMessage("Please enter an event address first.", "error");
        return;
    }

    clearTimeout(addressSearchTimer);

    const results = await searchEventAddresses(true);

    if (!results || results.length === 0) {
        state.form.locationValidated = false;
        state.form.locationConfirmed = false;
        updateStep2Button();

        showLocationMessage(
            "We couldn't find this address. Please check the spelling or pin the exact location on the map.",
            "warning"
        );

        openLocationMap();
        return;
    }

    // Only accept a result that falls within the selected city's service area.
    const validResult = results.find(isResultInsideSelectedCity);

    if (validResult) {
        selectAddressResult(validResult);
        return;
    }

    clearLocationSelection(false);

    showLocationMessage(
        `We found matching addresses, but none are inside the ${city} service area. Please refine the address or pin a location within ${city}.`,
        "error"
    );

    updateStep2Button();
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
    if (isCustomOccasion()) {
        return Boolean(state.termsAccepted);
    }

    const b = state.banking;
    const total = getTotal();
    const minimum = getMinimumPayment();
    const paymentAmount = getSelectedPaymentAmount();

    return Boolean(
        paymentAmount >= minimum &&
        paymentAmount <= total &&
        state.termsAccepted &&
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
        const availablePackages = getPackagesForOccasion();

        root.innerHTML = `
      <h2>Choose Your Occasion</h2>
      <p class="muted" style="margin-bottom:18px;">Tell us what you're celebrating. We'll then show only the packages available for that occasion.</p>

      <div class="form-group full">
        <label class="form-label" for="occasion">Occasion Type</label>
        <select class="form-control" id="occasion" name="occasion" required>
          <option value="">Select an occasion…</option>
          ${["Birthday", "Anniversary", "Graduation", "Valentine's Day", "Baby Shower", "Other"]
              .map(o => `<option value="${escapeHtml(o)}" ${state.form.occasion === o ? "selected" : ""}>${escapeHtml(o)}</option>`).join("")}
        </select>
      </div>

      ${state.form.occasion && !isCustomOccasion() ? `
        <section class="package-reveal" aria-live="polite">
          <div class="booking-package-heading">
            <p class="eyebrow">Choose Your Package</p>
            <h3>Packages Available for Your ${escapeHtml(state.form.occasion)}</h3>
            <p>Prices and inclusions below apply specifically to this occasion.</p>
          </div>

          <div class="package-select-grid booking-package-grid">
            ${availablePackages.map(pkg => {
                const selected = state.form.packageId === pkg.id;
                return `
                <button type="button"
                        class="package-choice booking-package-choice ${selected ? "selected" : ""}"
                        data-package="${pkg.id}"
                        aria-pressed="${selected ? "true" : "false"}">
                  <div class="booking-package-topline">
                    <span class="choice-badge">${pkg.badge}</span>
                    ${selected ? '<span class="selected-mark" aria-label="Selected">✓ Selected</span>' : ""}
                  </div>
                  <p class="choice-title">${pkg.name}</p>
                  <div class="choice-price">R${formatMoney(pkg.price)}</div>
                  <p class="package-choice-description">${escapeHtml(pkg.description)}</p>
                  <ul class="package-choice-inclusions">
                    ${pkg.inclusions.map(item => `<li><span class="check">✓</span><span>${escapeHtml(item)}</span></li>`).join("")}
                  </ul>
                  <span class="package-select-cta">${selected ? "Package Selected" : "Select Package"}</span>
                </button>
              `;
            }).join("")}
          </div>
        </section>
      ` : ""}

      ${isCustomOccasion() ? `
        <section class="custom-occasion-card package-reveal" aria-live="polite">
          <p class="eyebrow">Custom Celebration</p>
          <h3>Planning something unique?</h3>
          <p>We don't currently have a preset package for this type of event, but we'd be happy to create a setup suited to your occasion.</p>
          <p>Continue to Event Info and use the Special Instructions section to tell us about your event, theme, decorations or anything else you'd like us to know.</p>
        </section>
      ` : ""}

      <div class="form-actions" style="justify-content:flex-end;">
        <button type="button" class="btn btn-primary" id="nextStep1" ${isStep1Valid() ? "" : "disabled"}>
          ${isCustomOccasion() ? "Continue to Event Info →" : "Next: Event Info →"}
        </button>
      </div>
    `;
    }

    if (state.step === 2) {
        const today = new Date().toISOString().split("T")[0];

        root.innerHTML = `
      <h2>Event Details</h2>

      ${isCustomOccasion() ? `
        <div class="selected-package-summary custom">
          <div>
            <span class="summary-kicker">${escapeHtml(state.form.occasion)}</span>
            <strong>Custom setup request</strong>
            <small>No preset package — we'll use your Special Instructions to prepare a quote.</small>
          </div>
          <span class="summary-price">Quote required</span>
        </div>
      ` : `
        <div class="selected-package-summary">
          <div>
            <span class="summary-kicker">${escapeHtml(state.form.occasion)}</span>
            <strong>${escapeHtml(getSelectedPackage()?.name || "")}</strong>
            <small>Your Step 1 package selection</small>
          </div>
          <span class="summary-price">R${formatMoney(getSelectedPackage()?.price || 0)}</span>
        </div>
      `}

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
          <label class="form-label" for="city">City / Town</label>
          <select class="form-control" id="city" name="city" required>
            <option value="">Select a city/town…</option>
            <option value="Durban" ${state.form.city === "Durban" ? "selected" : ""}>Durban</option>
            <option value="Pietermaritzburg" ${state.form.city === "Pietermaritzburg" ? "selected" : ""}>Pietermaritzburg</option>
            <option value="Mandeni" ${state.form.city === "Mandeni" ? "selected" : ""}>Mandeni</option>
          </select>
          <small class="muted">Choose the city first. Address search and map pins are then restricted to that service area.</small>
        </div>

        <div class="form-group full location-section">
          <label class="form-label" for="address">Event Address</label>
          <div class="address-autocomplete">
            <input class="form-control" id="address" name="address"
                   value="${escapeHtml(state.form.address)}"
                   placeholder="Start typing an address…"
                   autocomplete="off"
                   ${state.form.city ? "" : "disabled"}
                   required>
            <div id="addressSuggestions" class="address-suggestions" role="listbox"></div>
          </div>
          <small class="muted">${state.form.city ? "Search for an address in " + escapeHtml(state.form.city) + ", or pin the location on the map." : "Select a city/town before entering an address."}</small>

          <div class="location-actions">
            <button type="button" class="btn btn-outline" id="findLocationButton" ${state.form.city ? "" : "disabled"}>🔎 Find Address</button>
            <button type="button" class="btn btn-primary" id="pinLocationButton" ${state.form.city ? "" : "disabled"}>📍 Pin Location on Map</button>
          </div>

          <div id="locationMessage" class="location-message"></div>

          <div id="mapContainer" class="booking-map-container" style="display:none;">
            <div id="bookingMap"></div>
            <p class="map-instruction">Click the map or drag the marker. The selected point must be inside the chosen city service area.</p>
            <button type="button" class="btn btn-primary" id="confirmLocationButton">Confirm Location</button>
          </div>

          <input type="hidden" id="latitude" name="latitude" value="${state.form.latitude ?? ""}">
          <input type="hidden" id="longitude" name="longitude" value="${state.form.longitude ?? ""}">
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
            <span class="muted">${isCustomOccasion() ? "Custom Setup — quote required" : (selectedPackage?.name || "No package selected")}</span>
            <span>${isCustomOccasion() ? "Pending quote" : "R" + formatMoney(selectedPackage?.price)}</span>
          </div>
          ${selectedAddOns.map(a => `
            <div class="review-row">
              <span class="muted">${a.icon} ${a.name}</span>
              <span>R${a.price}</span>
            </div>
          `).join("")}
          <div class="review-total">
            <span>${isCustomOccasion() ? "Estimated Total" : "Total"}</span>
            <span class="total-value">${isCustomOccasion() ? "Quote pending" : "R" + formatMoney(total)}</span>
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
        if (isCustomOccasion()) {
            state.paymentAmount = 0;

            root.innerHTML = `
      <div class="payment-heading">Custom Event Request</div>
      <p class="payment-subtext">Because this occasion does not use a preset package, no payment is due yet. AA Creations & Events will review your event details and prepare a custom quote.</p>

      <div class="review-box" style="margin-bottom:18px;">
        <div class="review-title">What happens next</div>
        <p class="muted" style="line-height:1.6;margin:.4rem 0;">
          Submit your booking request now. Your Special Instructions will be used to understand the setup you want, and payment will only be requested after a quote is prepared.
        </p>
        <label style="display:flex;gap:10px;align-items:flex-start;margin-top:12px;">
          <input type="checkbox" id="termsAccepted" name="termsAccepted" ${state.termsAccepted ? "checked" : ""} style="margin-top:4px;">
          <span>I understand and accept the <a href="/Cust/TermsAndConditions" target="_blank" rel="noreferrer">Terms & Conditions</a> for this booking request.</span>
        </label>
      </div>

      <div class="payment-actions">
        <button type="button" class="ghost-btn" id="backStep4">← Back to Review</button>
        <div class="confirm-wrap">
          <p class="fill-hint" id="fillHint" ${isStep4Valid() ? "hidden" : ""}>Accept the Terms & Conditions to continue</p>
          <button type="button" class="pink-btn" id="confirmBookingFinal" ${isStep4Valid() ? "" : "disabled"}>Submit Custom Request ✓</button>
        </div>
      </div>
    `;

            attachStepHandlers();
            return;
        }

        const total = getTotal();
        const minimumPayment = getMinimumPayment();
        if (state.paymentAmount === null || Number(state.paymentAmount) < minimumPayment || Number(state.paymentAmount) > total) {
            state.paymentAmount = minimumPayment;
        }
        const paymentAmount = getSelectedPaymentAmount();
        const remainingBalance = Math.max(0, total - paymentAmount);
        const cardDigits = digitsOnly(state.banking.cardNumber);
        const requiresFullPayment = getDaysUntilEvent() !== null && getDaysUntilEvent() <= 1;

        root.innerHTML = `
      <div class="payment-heading">Payment & Banking Details</div>
      <p class="payment-subtext">This project uses a simulated payment step. Card details are never sent or stored.</p>

      <div class="order-summary-strip">
        <span class="summary-label">Booking Total</span>
        <span class="summary-total">R${formatMoney(total)}</span>
      </div>

      <div class="review-box" style="margin-bottom:18px;">
        <div class="review-title">Payment Today</div>
        <div class="review-grid">
          <span class="label">Minimum required</span><strong>R${formatMoney(minimumPayment)}</strong>
          <span class="label">Rule</span><span>${requiresFullPayment ? "100% required for same-day/day-before bookings" : "Minimum 50% deposit"}</span>
        </div>
        <div class="field-group" style="margin-top:14px;">
          <label class="field-label" for="paymentAmount">Amount you want to pay now</label>
          <input class="field-input" id="paymentAmount" name="paymentAmount" type="number"
                 min="${minimumPayment}" max="${total}" step="0.01"
                 value="${paymentAmount.toFixed(2)}" ${requiresFullPayment ? "readonly" : ""} required>
          <small class="muted">Remaining balance after this payment: <strong id="remainingBalanceText">R${formatMoney(remainingBalance)}</strong></small>
        </div>
      </div>

      <div class="review-box" style="margin-bottom:18px;">
        <div class="review-title">Cancellation & Refund Summary</div>
        <p class="muted" style="line-height:1.6;margin:.4rem 0;">
          More than 7 days before the event: 10% of the amount paid is retained and 90% is refundable.
          Seven days or less: 20% is retained and 80% is refundable.
          If AA Creations & Events cancels, the amount paid is fully refundable, subject to the Terms & Conditions and applicable law.
        </p>
        <label style="display:flex;gap:10px;align-items:flex-start;margin-top:12px;">
          <input type="checkbox" id="termsAccepted" name="termsAccepted" ${state.termsAccepted ? "checked" : ""} style="margin-top:4px;">
          <span>I understand and accept the cancellation/refund policy and the <a href="/Cust/TermsAndConditions" target="_blank" rel="noreferrer">Terms & Conditions</a> for this booking.</span>
        </label>
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

    document.getElementById("findLocationButton")?.addEventListener("click", findEventAddress);
    document.getElementById("pinLocationButton")?.addEventListener("click", openLocationMap);
    document.getElementById("confirmLocationButton")?.addEventListener("click", confirmEventLocation);

    const addressInput = document.getElementById("address");
    if (addressInput) {
        addressInput.addEventListener("input", handleAddressTyping);
        addressInput.addEventListener("focus", () => {
            if (addressInput.value.trim().length >= 3 && !state.form.locationConfirmed) {
                searchEventAddresses(false);
            }
        });
    }

    document.removeEventListener("click", handleAddressOutsideClick);
    document.addEventListener("click", handleAddressOutsideClick);
}

function handleAddressOutsideClick(event) {
    const input = document.getElementById("address");
    const list = document.getElementById("addressSuggestions");
    if (!input || !list) return;
    if (event.target !== input && !list.contains(event.target)) {
        hideAddressSuggestions();
    }
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

    // ---- Step 4 payment controls ----

    if (control.name === "paymentAmount") {
        let amount = Number(control.value);
        if (!Number.isFinite(amount)) amount = getMinimumPayment();
        state.paymentAmount = amount;
        const balance = Math.max(0, getTotal() - amount);
        const balanceText = document.getElementById("remainingBalanceText");
        if (balanceText) balanceText.textContent = "R" + formatMoney(balance);
        updateStep4Button();
        return;
    }

    if (control.name === "termsAccepted") {
        state.termsAccepted = control.checked;
        updateStep4Button();
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

    if (control.name === "city" && state.step === 2) {
        state.form.address = "";
        state.form.latitude = null;
        state.form.longitude = null;
        state.form.locationValidated = false;
        state.form.locationConfirmed = false;
        if (bookingMapResizeObserver) {
            bookingMapResizeObserver.disconnect();
            bookingMapResizeObserver = null;
        }
        if (bookingMap) {
            bookingMap.remove();
            bookingMap = null;
            bookingMarker = null;
        }
        renderBookingStep();
        return;
    }

    if (control.name === "occasion" && state.step === 1) {
        state.form.packageId = "";
        renderBookingStep();
        return;
    }

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
        latitude: state.form.latitude,
        longitude: state.form.longitude,
        notes: state.form.notes,

        packageId: isCustomOccasion() ? "" : state.form.packageId,
        addOns: state.form.addOns,
        paymentAmount: getSelectedPaymentAmount(),
        termsAccepted: state.termsAccepted
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
        state.paymentAmount = Number(result.amountPaid);

        renderConfirmation(result);

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

function renderConfirmation(result = {}) {
    const pkg = getSelectedPackage();
    const total = state.serverTotalPrice;
    const root = document.getElementById("bookingApp");

    root.innerHTML = `
    <div class="confirmation">
      <div class="confirmation-card">
        <div class="confirmation-icon">✓</div>
        <h2>${isCustomOccasion() ? "Request Submitted!" : "Booking Confirmed!"}</h2>
        <p style="margin-bottom:8px;font-size:14px;color:var(--muted-foreground);">
          Thank you, <strong>${escapeHtml(state.form.firstName)}</strong>! ${isCustomOccasion() ? "Your custom celebration request has been received." : "Your celebration setup is booked."}
        </p>

        <div class="summary-mini">
          <div class="summary-mini-row"><span class="summary-mini-label">Package</span><strong>${isCustomOccasion() ? "Custom Setup" : (pkg?.name || "")}</strong></div>
          <div class="summary-mini-row"><span class="summary-mini-label">Date</span><strong>${escapeHtml(state.form.date)}</strong></div>
          <div class="summary-mini-row"><span class="summary-mini-label">Time</span><strong>${escapeHtml(state.form.time)}</strong></div>
          <div class="summary-mini-row"><span class="summary-mini-label">Total</span><strong style="color:var(--primary);">${isCustomOccasion() ? "Quote pending" : "R" + formatMoney(total)}</strong></div>
          <div class="summary-mini-row"><span class="summary-mini-label">Paid now</span><strong>${isCustomOccasion() ? "No payment due yet" : "R" + formatMoney(result.amountPaid ?? state.paymentAmount)}</strong></div>
          <div class="summary-mini-row"><span class="summary-mini-label">Balance</span><strong>${isCustomOccasion() ? "Set after quote" : "R" + formatMoney(result.balanceOutstanding ?? Math.max(0,total-state.paymentAmount))}</strong></div>
          <div class="summary-mini-row"><span class="summary-mini-label">Payment status</span><strong>${escapeHtml(result.paymentStatus || "")}</strong></div>
        </div>

        <p class="confirmation-note">${isCustomOccasion() ? "We'll be in touch via WhatsApp with your custom quote and next steps." : "We'll be in touch via WhatsApp to confirm. Transport fee quoted separately."}</p>

        <div class="confirmation-actions">
          <a href="/Cust/ViewBooking" class="btn btn-outline">View Bookings</a>
          <a href="/Cust/Index" class="btn btn-primary">Back Home</a>
        </div>
      </div>
    </div>
  `;    
}

document.addEventListener("DOMContentLoaded", () => {
    const app = document.getElementById("bookingApp");
    if (!app) return;

    const isLoggedIn = app.dataset.loggedIn === "true";
    const loginOverlay = document.getElementById("bookingLoginOverlay");

    if (!isLoggedIn) {
        if (loginOverlay) {
            loginOverlay.classList.add("visible");
        }

        return;
    }

    // Load the signed-in customer's details from the server-rendered page.
    // These values came from the Customers table using Session["CustomerId"].
    state.form.firstName = app.dataset.customerFirstName || "";
    state.form.lastName = app.dataset.customerLastName || "";
    state.form.email = app.dataset.customerEmail || "";
    state.form.phone = app.dataset.customerPhone || "";

    loadQueryPackage();
    renderBookingStep();
});