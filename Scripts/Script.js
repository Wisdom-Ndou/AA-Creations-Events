// AA Creations & Events — Admin Registration
// Vanilla JS replacement for the React useState logic in the original .tsx

document.addEventListener('DOMContentLoaded', function () {
    var form = document.getElementById('adminForm');
    var formWrap = document.getElementById('formWrap');
    var successPage = document.getElementById('successPage');

    var passwordInput = document.getElementById('password');
    var confirmInput = document.getElementById('confirm');
    var togglePassBtn = document.getElementById('togglePass');

    // --- Show / hide password (mirrors the showPass state) ---
    togglePassBtn.addEventListener('click', function () {
        var isHidden = passwordInput.type === 'password';
        passwordInput.type = isHidden ? 'text' : 'password';
        togglePassBtn.textContent = isHidden ? 'Hide' : 'Show';
    });

    // --- Live confirm-password match styling ---
    function checkConfirmMatch() {
        confirmInput.classList.remove('match', 'mismatch');
        if (!confirmInput.value) return;
        if (passwordInput.value === confirmInput.value) {
            confirmInput.classList.add('match');
        } else {
            confirmInput.classList.add('mismatch');
        }
    }
    confirmInput.addEventListener('input', checkConfirmMatch);
    passwordInput.addEventListener('input', checkConfirmMatch);

    // --- Submit handler (mirrors setSubmitted(true)) ---
    form.addEventListener('submit', function (e) {
        e.preventDefault();

        // Basic client-side sanity check before "submitting".
        // NOTE: this is NOT real validation/security — see the notes
        // handed back with these files for what still needs to be added
        // server-side once this is wired into the MVC project.
        if (passwordInput.value !== confirmInput.value) {
            confirmInput.classList.add('mismatch');
            confirmInput.focus();
            return;
        }

        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }

        // TODO: replace this with an actual POST to your MVC controller action,
        // e.g. fetch('/Admin/Register', { method: 'POST', body: new FormData(form) })
        console.log('Admin registration form data:', Object.fromEntries(new FormData(form)));

        formWrap.hidden = true;
        successPage.hidden = false;
    });
});