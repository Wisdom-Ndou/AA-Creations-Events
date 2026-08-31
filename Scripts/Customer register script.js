// Customer Registration Page
// Handles password validation, confirmation matching,
// password strength indicator and form submission.

document.addEventListener("DOMContentLoaded", function () {

    function el(id) {
        return document.getElementById(id);
    }

    var form = el("registerForm");

    var passwordInput = el("password");
    var confirmInput = el("confirm");

    var passwordError = el("passwordError");
    var mismatchText = el("mismatchText");

    var togglePassBtn = el("togglePass");

    var strengthSegs = [
        el("seg1"),
        el("seg2"),
        el("seg3"),
        el("seg4")
    ];

    // Password must:
    // - contain at least one letter
    // - contain at least one number
    // - be 6 to 15 characters long
    var passwordRegex = /^(?=.*[A-Za-z])(?=.*\d).{6,15}$/;


    // --------------------------------------------------
    // HELPER: Set invalid state
    // --------------------------------------------------

    function setInvalid(input, invalid) {

        if (!input) return;

        input.setAttribute(
            "aria-invalid",
            invalid ? "true" : "false"
        );

        input.classList.toggle("mismatch", invalid);
    }


    // --------------------------------------------------
    // HELPER: Show / hide error
    // --------------------------------------------------

    function showError(errorElement, show) {

        if (!errorElement) return;

        errorElement.hidden = !show;
    }


    // --------------------------------------------------
    // PASSWORD STRENGTH
    // --------------------------------------------------

    function updateStrengthBar() {

        if (!passwordInput) return;

        var value = passwordInput.value || "";

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


        for (var i = 0; i < strengthSegs.length; i++) {

            var segment = strengthSegs[i];

            if (!segment) continue;

            segment.classList.remove(
                "weak",
                "medium",
                "strong"
            );


            if (i < score) {

                if (score <= 1) {
                    segment.classList.add("weak");
                }
                else if (score <= 2) {
                    segment.classList.add("medium");
                }
                else {
                    segment.classList.add("strong");
                }

            }

        }

    }


    // --------------------------------------------------
    // PASSWORD VALIDATION
    // --------------------------------------------------

    function validatePassword(showMessage) {

        if (!passwordInput) {
            return true;
        }

        var value = passwordInput.value || "";

        var valid = passwordRegex.test(value);


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


    // --------------------------------------------------
    // CONFIRM PASSWORD
    // --------------------------------------------------

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


        // Only show mismatch when the confirmation
        // has actually been entered.
        var mismatch =
            confirmation.length > 0 &&
            password !== confirmation;


        if (showMessage) {

            showError(
                mismatchText,
                mismatch
            );

        }
        else {

            showError(
                mismatchText,
                mismatch
            );

        }


        setInvalid(
            confirmInput,
            mismatch
        );


        return passwordValid && !mismatch;

    }


    // --------------------------------------------------
    // PASSWORD INPUT
    // --------------------------------------------------

    if (passwordInput) {

        passwordInput.addEventListener(
            "input",
            function () {

                validatePassword(true);

                // Only check matching once confirmation
                // has something inside it.
                if (confirmInput.value.length > 0) {
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


    // --------------------------------------------------
    // CONFIRM PASSWORD INPUT
    // --------------------------------------------------

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


    // --------------------------------------------------
    // SHOW / HIDE PASSWORD
    // --------------------------------------------------

    if (togglePassBtn && passwordInput) {

        togglePassBtn.addEventListener(
            "click",
            function () {

                var hidden =
                    passwordInput.type === "password";


                passwordInput.type =
                    hidden ? "text" : "password";


                togglePassBtn.textContent =
                    hidden ? "Hide" : "Show";

            }
        );

    }


    // --------------------------------------------------
    // FORM SUBMISSION
    // --------------------------------------------------

    if (form) {

        form.addEventListener(
            "submit",
            function (event) {

                // Validate password
                var passwordValid =
                    validatePassword(true);


                // Validate confirmation
                var passwordsMatch =
                    checkPasswordsMatch(true);


                if (!passwordValid || !passwordsMatch) {

                    event.preventDefault();

                    if (!passwordValid) {

                        passwordInput.focus();

                    }
                    else {

                        confirmInput.focus();

                    }

                    return false;

                }


                // IMPORTANT:
                // Do NOT call event.preventDefault()
                // here.
                //
                // MVC will receive the POST request.

            }
        );

    }

});