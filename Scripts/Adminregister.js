document.addEventListener("DOMContentLoaded", function () {

    var form = document.getElementById("adminForm");

    var password = document.getElementById("password");
    var confirm = document.getElementById("confirm");
    var togglePass = document.getElementById("togglePass");

    /*
     * Show / hide password
     */
    if (togglePass && password) {

        togglePass.addEventListener("click", function () {

            if (password.type === "password") {
                password.type = "text";
                togglePass.textContent = "Hide";
                togglePass.setAttribute("aria-pressed", "true");
            }
            else {
                password.type = "password";
                togglePass.textContent = "Show";
                togglePass.setAttribute("aria-pressed", "false");
            }

        });
    }


    /*
     * Client-side password confirmation check.
     * IMPORTANT:
     * We do NOT prevent the form from submitting.
     */
    if (form) {

        form.addEventListener("submit", function () {

            if (password && confirm) {

                if (password.value !== confirm.value) {

                    confirm.setCustomValidity(
                        "Passwords do not match."
                    );

                }
                else {

                    confirm.setCustomValidity("");
                }
            }

            /*
             * DO NOT use:
             *
             * event.preventDefault();
             *
             * The browser must submit the form to
             * CustController.Adminregister().
             */
        });
    }

});