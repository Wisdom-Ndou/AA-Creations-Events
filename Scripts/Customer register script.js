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

    var cookiePreferenceInput = el("cookiePreference");


    // ==================================================
    // PREVENT FORM SUBMISSION BEFORE AGREEMENT
    // ==================================================

    if (form) {
        form.addEventListener("submit", function (event) {
            // Check if terms have been accepted via the form's hidden input
            if (termsAcceptedInput && termsAcceptedInput.value !== "true") {
                event.preventDefault();
                event.stopPropagation();

                // Show the agreement modal if it exists and is hidden
                if (agreementModal && agreementModal.style.display === "none") {
                    agreementModal.style.display = "block";
                    agreementModal.setAttribute("aria-hidden", "false");

                    // Focus on the agreement modal for accessibility
                    var firstButton = agreementModal.querySelector("button");
                    if (firstButton) {
                        firstButton.focus();
                    }
                }

                return false;
            }

            return true;
        });
    }


    // ==================================================
    // CLOSE AGREEMENT AND LEAVE
    // ==================================================

    function closeAgreementAndLeave() {

        if (agreementModal) {

            agreementModal.style.display = "none";

            agreementModal.setAttribute(
                "aria-hidden",
                "true"
            );
        }

        window.location.href = "/Cust/Index";
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

        // Prevent the button from performing any default action.
        if (event) {

            event.preventDefault();

            event.stopPropagation();
        }


        // --------------------------------------------------
        // REQUIRE TERMS CHECKBOX
        // --------------------------------------------------

        if (!termsCheckbox || !termsCheckbox.checked) {

            showAgreementError(true);

            if (termsCheckbox) {
                termsCheckbox.focus();
            }

            return false;
        }


        // --------------------------------------------------
        // HIDE AGREEMENT ERROR
        // --------------------------------------------------

        showAgreementError(false);


        // --------------------------------------------------
        // MARK TERMS AS ACCEPTED
        // --------------------------------------------------

        if (termsAcceptedInput) {

            termsAcceptedInput.value = "true";
        }


        // --------------------------------------------------
        // GET COOKIE PREFERENCE
        // --------------------------------------------------

        var selectedCookie = document.querySelector(
            'input[name="agreementCookiePreference"]:checked'
        );


        var selectedPreference =
            selectedCookie
                ? selectedCookie.value
                : "necessary";


        // --------------------------------------------------
        // SAVE COOKIE PREFERENCE TO FORM
        // --------------------------------------------------

        if (cookiePreferenceInput) {

            cookiePreferenceInput.value =
                selectedPreference;
        }


        // --------------------------------------------------
        // CLOSE TERMS POPUP
        // --------------------------------------------------

        if (agreementModal) {

            agreementModal.style.display = "none";

            agreementModal.setAttribute(
                "aria-hidden",
                "true"
            );
        }


        // --------------------------------------------------
        // REMEMBER TERMS ACCEPTANCE
        // --------------------------------------------------

        try {

            sessionStorage.setItem(
                "aa_registration_terms_accepted",
                "true"
            );

        }
        catch (error) {

            console.warn(
                "Unable to save Terms acceptance.",
                error
            );

        }


        // --------------------------------------------------
        // REMEMBER COOKIE PREFERENCE
        // --------------------------------------------------

        try {

            localStorage.setItem(
                "aa_cookie_preference",
                selectedPreference
            );

        }
        catch (error) {

            console.warn(
                "Unable to save cookie preference.",
                error
            );

        }

        // Focus on the first form field to guide user
        var firstField = form.querySelector("input[type='text'], input[type='email'], input[type='tel'], input[type='password']");
        if (firstField) {
            firstField.focus();
        }

        return true;
    }


    // ==================================================
    // AGREE & CONTINUE BUTTON
    // ==================================================

    if (agreeButton) {

        agreeButton.addEventListener(
            "click",
            function (event) {

                acceptAgreement(event);

            }
        );

    }


    // ==================================================
    // CANCEL BUTTONS
    // ==================================================

    if (cancelButton) {

        cancelButton.addEventListener(
            "click",
            function () {

                closeAgreementAndLeave();

            }
        );

    }


    if (cancelTopButton) {

        cancelTopButton.addEventListener(
            "click",
            function () {

                closeAgreementAndLeave();

            }
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
                // REQUIRE TERMS ACCEPTANCE
                // ------------------------------------------

                if (
                    !termsAcceptedInput ||
                    termsAcceptedInput.value !== "true"
                ) {

                    event.preventDefault();

                    showAgreementError(true);

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


                // ------------------------------------------
                // EVERYTHING IS VALID
                //
                // DO NOT CALL preventDefault()
                //
                // MVC WILL RECEIVE THE POST REQUEST.
                // ------------------------------------------

            }
        );

    }


    // ==================================================
    // INITIAL TERMS POPUP
    // ==================================================

    if (agreementModal) {

        var alreadyAccepted = false;


        try {

            alreadyAccepted =
                sessionStorage.getItem(
                    "aa_registration_terms_accepted"
                ) === "true";

        }
        catch (error) {

            console.warn(
                "Unable to read Terms acceptance.",
                error
            );

        }


        if (!alreadyAccepted) {

            agreementModal.style.display =
                "flex";


            agreementModal.setAttribute(
                "aria-hidden",
                "false"
            );

        }
        else {

            agreementModal.style.display =
                "none";


            agreementModal.setAttribute(
                "aria-hidden",
                "true"
            );

        }

    }

});
});