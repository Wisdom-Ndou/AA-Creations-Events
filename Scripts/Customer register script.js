// Customer Registration Page
// Handles password validation, confirmation matching,
// password strength indicator, Terms & Conditions agreement,
// cookie preference and form submission.

document.addEventListener("DOMContentLoaded", function () {

    // ==================================================
    // HELPER
    // ==================================================

    function el(id) {
        return document.getElementById(id);
    }


    // ==================================================
    // FORM
    // ==================================================

    var form = el("registerForm");


    // ==================================================
    // ACCOUNT AGREEMENT
    // ==================================================

    var agreementModal = el("registrationAgreement");
    var agreeButton = el("agreeAndContinue");
    var cancelButton = el("cancelAgreement");
    var cancelTopButton = el("cancelAgreementTop");
    var termsCheckbox = el("termsAgreementCheckbox");
    var agreementError = el("agreementError");

    var termsAcceptedInput = el("termsAccepted");
    var termsVersionInput = el("termsVersion");
    var cookiePreferenceInput = el("cookiePreference");


    // ==================================================
    // CLOSE AGREEMENT AND LEAVE
    // ==================================================

    function closeAgreementAndLeave(event) {

        if (event) {
            event.preventDefault();
            event.stopPropagation();
        }

        if (termsAcceptedInput) {
            termsAcceptedInput.value = "false";
        }

        // Customer chose not to accept the Terms,
        // so leave the registration page.
        window.location.href = "/Cust/Login";
    }


    // ==================================================
    // SHOW AGREEMENT ERROR
    // ==================================================

    function showAgreementError(show) {

        if (!agreementError) {
            return;
        }

        agreementError.hidden = !show;
    }


    // ==================================================
    // ACCEPT AGREEMENT
    // ==================================================

    function acceptAgreement(event) {

        if (event) {
            event.preventDefault();
            event.stopPropagation();
        }


        // Require Terms checkbox.
        if (!termsCheckbox || !termsCheckbox.checked) {

            showAgreementError(true);

            if (termsCheckbox) {
                termsCheckbox.focus();
            }

            return false;
        }


        // Make sure there is a current Terms version.
        var acceptedVersion =
            termsVersionInput
                ? termsVersionInput.value
                : "";

        if (!acceptedVersion) {

            showAgreementError(true);

            return false;
        }


        showAgreementError(false);


        // Get selected cookie preference.
        var selectedCookie = document.querySelector(
            'input[name="agreementCookiePreference"]:checked'
        );

        var selectedPreference =
            selectedCookie
                ? selectedCookie.value
                : "necessary";


        // Update hidden form values.
        if (termsAcceptedInput) {
            termsAcceptedInput.value = "true";
        }

        if (cookiePreferenceInput) {
            cookiePreferenceInput.value =
                selectedPreference;
        }

        // Acceptance is deliberately not persisted in browser storage.
        // The server records it against the newly-created customer account.

        // Close the Terms modal.
        if (agreementModal) {

            agreementModal.style.display = "none";

            agreementModal.setAttribute(
                "aria-hidden",
                "true"
            );

            document.body.style.overflow = "";
        }


        // Move customer to first registration field.
        if (form) {

            var firstField = form.querySelector(
                "input[type='text'], input[type='email'], input[type='tel'], input[type='password']"
            );

            if (firstField) {
                firstField.focus();
            }
        }

        return true;
    }


    // ==================================================
    // AGREE & CONTINUE BUTTON
    // ==================================================

    if (agreeButton) {

        agreeButton.addEventListener(
            "click",
            acceptAgreement
        );
    }


    // ==================================================
    // CANCEL BUTTON
    // ==================================================

    if (cancelButton) {

        cancelButton.addEventListener(
            "click",
            closeAgreementAndLeave
        );
    }


    // ==================================================
    // TOP X BUTTON
    // ==================================================

    if (cancelTopButton) {

        cancelTopButton.addEventListener(
            "click",
            closeAgreementAndLeave
        );
    }


    // ==================================================
    // TERMS CHECKBOX
    // ==================================================

    if (termsCheckbox) {

        termsCheckbox.addEventListener(
            "change",
            function () {

                if (termsCheckbox.checked) {
                    showAgreementError(false);
                }

            }
        );
    }



    // ==================================================
    // PASSWORD ELEMENTS
    // ==================================================

    var passwordInput = el("password");

    var confirmInput = el("confirm");

    var passwordError = el("passwordError");

    var mismatchText = el("mismatchText");

    var togglePassBtn = el("togglePass");


    // ==================================================
    // PASSWORD STRENGTH ELEMENTS
    // ==================================================

    var strengthSegs = [

        el("seg1"),

        el("seg2"),

        el("seg3"),

        el("seg4")

    ];


    // ==================================================
    // PASSWORD RULE
    // ==================================================

    var passwordRegex =
        /^(?=.*[A-Za-z])(?=.*\d).{6,15}$/;


    // ==================================================
    // SET INVALID STATE
    // ==================================================

    function setInvalid(input, invalid) {

        if (!input) {
            return;
        }

        input.setAttribute(
            "aria-invalid",
            invalid ? "true" : "false"
        );

        input.classList.toggle(
            "mismatch",
            invalid
        );
    }


    // ==================================================
    // SHOW / HIDE ERROR
    // ==================================================

    function showError(errorElement, show) {

        if (!errorElement) {
            return;
        }

        errorElement.hidden = !show;
    }


    // ==================================================
    // PASSWORD STRENGTH
    // ==================================================

    function updateStrengthBar() {

        if (!passwordInput) {
            return;
        }


        var value =
            passwordInput.value || "";


        var score = 0;


        if (value.length >= 6) {
            score++;
        }


        if (/[A-Za-z]/.test(value)) {
            score++;
        }


        if (/\d/.test(value)) {
            score++;
        }


        if (value.length >= 10) {
            score++;
        }


        for (
            var i = 0;
            i < strengthSegs.length;
            i++
        ) {

            var segment = strengthSegs[i];


            if (!segment) {
                continue;
            }


            segment.classList.remove(
                "weak",
                "medium",
                "strong"
            );


            if (i < score) {

                if (score <= 1) {

                    segment.classList.add(
                        "weak"
                    );

                }
                else if (score <= 2) {

                    segment.classList.add(
                        "medium"
                    );

                }
                else {

                    segment.classList.add(
                        "strong"
                    );

                }

            }

        }

    }


    // ==================================================
    // PASSWORD VALIDATION
    // ==================================================

    function validatePassword(showMessage) {

        if (!passwordInput) {
            return true;
        }


        var value =
            passwordInput.value || "";


        var valid =
            passwordRegex.test(value);


        if (showMessage) {

            showError(
                passwordError,
                !valid && value.length > 0
            );

        }
        else {

            showError(
                passwordError,
                !valid
            );

        }


        setInvalid(
            passwordInput,
            !valid
        );


        updateStrengthBar();


        return valid;
    }


    // ==================================================
    // CONFIRM PASSWORD
    // ==================================================

    function checkPasswordsMatch(showMessage) {

        if (!passwordInput || !confirmInput) {
            return true;
        }


        var password =
            passwordInput.value || "";


        var confirmation =
            confirmInput.value || "";


        var passwordValid =
            validatePassword(showMessage);


        var mismatch =
            confirmation.length > 0 &&
            password !== confirmation;


        showError(
            mismatchText,
            mismatch
        );


        setInvalid(
            confirmInput,
            mismatch
        );


        return passwordValid && !mismatch;
    }


    // ==================================================
    // PASSWORD INPUT
    // ==================================================

    if (passwordInput) {

        passwordInput.addEventListener(
            "input",
            function () {

                validatePassword(true);


                if (
                    confirmInput &&
                    confirmInput.value.length > 0
                ) {

                    checkPasswordsMatch(true);

                }

            }
        );


        passwordInput.addEventListener(
            "blur",
            function () {

                validatePassword(true);

            }
        );

    }


    // ==================================================
    // CONFIRM PASSWORD INPUT
    // ==================================================

    if (confirmInput) {

        confirmInput.addEventListener(
            "input",
            function () {

                checkPasswordsMatch(true);

            }
        );


        confirmInput.addEventListener(
            "blur",
            function () {

                checkPasswordsMatch(true);

            }
        );

    }


    // ==================================================
    // SHOW / HIDE PASSWORD
    // ==================================================

    if (togglePassBtn && passwordInput) {

        togglePassBtn.addEventListener(
            "click",
            function () {

                var hidden =
                    passwordInput.type === "password";


                passwordInput.type =
                    hidden
                        ? "text"
                        : "password";


                togglePassBtn.textContent =
                    hidden
                        ? "Hide"
                        : "Show";

            }
        );

    }


    // ==================================================
    // FORM SUBMISSION
    // ==================================================

    if (form) {

        form.addEventListener(
            "submit",
            function (event) {

                // ------------------------------------------
                // REQUIRE CURRENT TERMS ACCEPTANCE
                // ------------------------------------------

                if (
                    !termsAcceptedInput ||
                    termsAcceptedInput.value !== "true"
                ) {

                    event.preventDefault();

                    showAgreementError(true);

                    if (agreementModal) {

                        agreementModal.style.display = "flex";

                        agreementModal.setAttribute(
                            "aria-hidden",
                            "false"
                        );
                    }

                    if (termsCheckbox) {
                        termsCheckbox.focus();
                    }

                    return false;
                }


                // ------------------------------------------
                // VALIDATE PASSWORD
                // ------------------------------------------

                var passwordValid =
                    validatePassword(true);


                // ------------------------------------------
                // VALIDATE CONFIRM PASSWORD
                // ------------------------------------------

                var passwordsMatch =
                    checkPasswordsMatch(true);


                // ------------------------------------------
                // STOP IF VALIDATION FAILS
                // ------------------------------------------

                if (
                    !passwordValid ||
                    !passwordsMatch
                ) {

                    event.preventDefault();

                    if (
                        !passwordValid &&
                        passwordInput
                    ) {

                        passwordInput.focus();

                    }
                    else if (confirmInput) {

                        confirmInput.focus();

                    }

                    return false;
                }

                // Otherwise allow MVC to submit.

            }
        );

    }

    // ==================================================
    // INITIAL TERMS POPUP
    // ==================================================
    // Every registration attempt represents a new account.
    // Browser storage must never satisfy another customer's acceptance.
    if (agreementModal) {
        if (termsAcceptedInput) {
            termsAcceptedInput.value = "false";
        }

        agreementModal.style.display = "flex";
        agreementModal.setAttribute("aria-hidden", "false");

        if (agreeButton) {
            agreeButton.focus();
        }
    }

});
