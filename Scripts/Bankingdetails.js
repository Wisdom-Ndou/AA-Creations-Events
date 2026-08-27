(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {

        var form = document.getElementById("bankingDetailsForm");
        if (!form) {
            return;
        }

        var cardholderName = document.getElementById("CardholderName");
        var cardNumber = document.getElementById("CardNumber");
        var expiryDate = document.getElementById("ExpiryDate");
        var cvv = document.getElementById("Cvv");
        var streetAddress = document.getElementById("StreetAddress");
        var city = document.getElementById("City");
        var postalCode = document.getElementById("PostalCode");
        var confirmButton = document.getElementById("confirmButton");

        var requiredFields = [
            cardholderName, cardNumber, expiryDate, cvv,
            streetAddress, city, postalCode
        ];

        // ---- Formatting helpers ----

        function digitsOnly(value) {
            return value.replace(/\D/g, "");
        }

        function formatCardNumber(value) {
            var digits = digitsOnly(value).substring(0, 16);
            var groups = digits.match(/.{1,4}/g);
            return groups ? groups.join(" ") : "";
        }

        function formatExpiry(value) {
            var digits = digitsOnly(value).substring(0, 4);
            if (digits.length >= 3) {
                return digits.substring(0, 2) + "/" + digits.substring(2);
            }
            return digits;
        }

        // ---- Live formatting listeners ----

        cardNumber.addEventListener("input", function () {
            var cursorAtEnd = cardNumber.selectionEnd === cardNumber.value.length;
            cardNumber.value = formatCardNumber(cardNumber.value);
            if (cursorAtEnd) {
                cardNumber.selectionStart = cardNumber.selectionEnd = cardNumber.value.length;
            }
            validateForm();
        });

        expiryDate.addEventListener("input", function () {
            expiryDate.value = formatExpiry(expiryDate.value);
            validateForm();
        });

        cvv.addEventListener("input", function () {
            cvv.value = digitsOnly(cvv.value).substring(0, 4);
            validateForm();
        });

        postalCode.addEventListener("input", function () {
            postalCode.value = digitsOnly(postalCode.value).substring(0, 4);
            validateForm();
        });

        // ---- Validation ----

        function isExpiryValid(value) {
            var match = /^(\d{2})\/(\d{2})$/.exec(value);
            if (!match) {
                return false;
            }
            var month = parseInt(match[1], 10);
            if (month < 1 || month > 12) {
                return false;
            }
            return true;
        }

        function isFieldFilled(field) {
            return field.value && field.value.trim().length > 0;
        }

        function validateForm() {
            var allFilled = requiredFields.every(isFieldFilled);
            var cardNumberValid = digitsOnly(cardNumber.value).length >= 13;
            var expiryValid = isExpiryValid(expiryDate.value);
            var cvvValid = cvv.value.length === 3 || cvv.value.length === 4;
            var postalValid = postalCode.value.length === 4;

            var isValid = allFilled && cardNumberValid && expiryValid && cvvValid && postalValid;

            confirmButton.disabled = !isValid;
            confirmButton.textContent = isValid ? "Confirm & Pay" : "Fill all fields to confirm";

            return isValid;
        }

        requiredFields.forEach(function (field) {
            field.addEventListener("input", validateForm);
            field.addEventListener("blur", function () {
                markFieldValidity(field);
            });
        });

        function markFieldValidity(field) {
            var value = field.value.trim();
            var valid = value.length > 0;

            if (field === cardNumber) {
                valid = digitsOnly(value).length >= 13;
            } else if (field === expiryDate) {
                valid = isExpiryValid(value);
            } else if (field === cvv) {
                valid = value.length === 3 || value.length === 4;
            } else if (field === postalCode) {
                valid = value.length === 4;
            }

            field.classList.toggle("field-invalid", !valid);
        }

        form.addEventListener("submit", function (event) {
            if (!validateForm()) {
                event.preventDefault();
                requiredFields.forEach(markFieldValidity);
            }
        });

        // Initial state on load (e.g. browser autofill)
        validateForm();
    });
})();