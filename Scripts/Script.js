// AA Creations & Events — Admin Registration
// Vanilla JS replacement for the React useState logic in the original .tsx

document.addEventListener('DOMContentLoaded', function () {
    var form = document.getElementById('adminForm');
    var formWrap = document.getElementById('formWrap');
    var successPage = document.getElementById('successPage');

    var passwordInput = document.getElementById('password');
    var confirmInput = document.getElementById('confirm');
    var togglePassBtn = document.getElementById('togglePass');
    var firstName = document.getElementById('firstName');
    var lastName = document.getElementById('lastName');
    var phone = document.getElementById('phone');

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
    if (passwordInput.value !== confirmInput.value) {
        confirmInput.classList.add('mismatch');
        confirmInput.focus();
        return;
    }

    if (!form.checkValidity()) {
        form.reportValidity();
        return;
    }

    // Live validations
    let valid = true;
    if (!validateName(firstName, 'firstNameError')) valid = false;
    if (!validateName(lastName, 'lastNameError')) valid = false;
    if (!validatePhone(phone, 'phoneError')) valid = false;
    if (!valid) return;

    // --- SHOW THE POPUP MESSAGE HERE ---
    alert('Registration successful!\nWelcome, ' + firstName.value.trim() + ' ' + lastName.value.trim() + '!');

    // Continue with your success logic
    formWrap.hidden = true;
    successPage.hidden = false;
});


    // --- Live validations ---
    if (firstName) {
        firstName.addEventListener('input', function () {
            validateName(this, 'firstNameError');
        });
    }
    if (lastName) {
        lastName.addEventListener('input', function () {
            validateName(this, 'lastNameError');
        });
    }
    if (phone) {
        phone.addEventListener('input', function () {
            validatePhone(this, 'phoneError');
        });
    }

    function validateName(input, errorId) {
        if (!input) return false;
        const value = input.value.trim();
        const regex = /^[A-Za-z]+$/;
        if (!value) {
            showError(errorId, 'This field is required.');
            return false;
        } else if (!regex.test(value)) {
            showError(errorId, 'Only letters are allowed.');
            return false;
        }
        showError(errorId, '');
        return true;
    }

    function validatePhone(input, errorId) {
        if (!input) return false;
        let value = input.value.replace(/\D/g, '');
        input.value = value; // Only allow digits in the field
        if (value.length !== 9) {
            showError(errorId, 'Phone number must be exactly 9 digits.');
            return false;
        }
        showError(errorId, '');
        return true;
    }

    function showError(errorId, message) {
        const el = document.getElementById(errorId);
        if (el) el.textContent = message;
    }
});
