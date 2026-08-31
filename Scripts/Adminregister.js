// Client-side validation for Admin registration form with stronger email & password rules
document.addEventListener('DOMContentLoaded', function () {
    var form = document.getElementById('adminForm');
    var firstName = document.getElementById('firstName');
    var lastName = document.getElementById('lastName');
    var firstNameError = document.getElementById('firstNameError');
    var lastNameError = document.getElementById('lastNameError');

    var email = document.getElementById('email');
    var emailError = document.getElementById('emailError');

    var phone = document.getElementById('phone');
    var phoneError = document.getElementById('phoneError');

    var password = document.getElementById('password');
    var confirm = document.getElementById('confirm');
    var passwordError = document.getElementById('passwordError');
    var mismatchText = document.getElementById('mismatchText');
    var togglePass = document.getElementById('togglePass');

    var terms = document.getElementById('termsAccepted');
    var successPage = document.getElementById('successPage');
    var formWrap = document.getElementById('formWrap');

    // helpers
    function setInvalid(el, invalid) {
        if (!el) return;
        el.setAttribute('aria-invalid', invalid ? 'true' : 'false');
        el.classList.toggle('invalid', invalid);
    }
    function show(el, visible) { if (!el) return; el.hidden = !visible; }

    // Names: letters only
    var lettersOnly = /[^A-Za-z]/g;
    function validateNameField(el, errEl) {
        if (!el) return false;
        var v = (el.value || '').trim();
        var cleaned = v.replace(lettersOnly, '');
        if (v !== cleaned) el.value = cleaned;
        var ok = cleaned.length > 0;
        show(errEl, !ok);
        setInvalid(el, !ok);
        return ok;
    }
    firstName?.addEventListener('input', function () { validateNameField(firstName, firstNameError); });
    firstName?.addEventListener('blur', function () { validateNameField(firstName, firstNameError); });
    lastName?.addEventListener('input', function () { validateNameField(lastName, lastNameError); });
    lastName?.addEventListener('blur', function () { validateNameField(lastName, lastNameError); });

    // Email: robust simple regex (additional to type="email")
    var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    function validateEmail() {
        if (!email) return false;
        var v = (email.value || '').trim();
        var ok = emailRegex.test(v);
        show(emailError, !ok);
        setInvalid(email, !ok);
        return ok;
    }
    email?.addEventListener('input', validateEmail);
    email?.addEventListener('blur', validateEmail);

    // Phone: digits only, exactly 9 digits, not start with 0
    var digitsOnly = /[^0-9]/g;
    function validatePhone() {
        if (!phone) return false;
        var v = (phone.value || '').replace(digitsOnly, '');
        if (v.length > 9) v = v.slice(0, 9);
        if (phone.value !== v) phone.value = v;
        var ok = v.length === 9 && !/^0/.test(v);
        show(phoneError, !ok);
        setInvalid(phone, !ok);
        return ok;
    }
    phone?.addEventListener('input', validatePhone);
    phone?.addEventListener('blur', validatePhone);

    // Password rule:
    // - at least 8 characters
    // - at least 2 uppercase letters
    // - at least 1 special character (non-alphanumeric)
    var pwdRegex = /^(?=.{8,}$)(?=(.*[A-Z]){2,})(?=.*[^A-Za-z0-9]).*$/;
    function validatePassword(lively) {
        if (!password) return false;
        var v = password.value || '';
        var ok = pwdRegex.test(v);
        // be less noisy while typing: only show if user has typed something when lively=true
        var showMsg = !ok && (lively ? v.length > 0 : true);
        show(passwordError, showMsg);
        setInvalid(password, !ok);
        return ok;
    }

    // Confirm match
    function checkPasswordsMatch(lively) {
        if (!password || !confirm) return false;
        var p = password.value || '';
        var c = confirm.value || '';
        var mismatch = c.length > 0 && p !== c;
        if (lively === false) mismatch = p !== c;
        show(mismatchText, mismatch);
        setInvalid(confirm, mismatch);
        return !mismatch;
    }

    password?.addEventListener('input', function () {
        validatePassword(true);
        checkPasswordsMatch(true);
    });
    password?.addEventListener('blur', function () { validatePassword(false); });
    confirm?.addEventListener('input', function () { checkPasswordsMatch(true); });
    confirm?.addEventListener('blur', function () { checkPasswordsMatch(false); });

    // Toggle show/hide password
    if (togglePass && password) {
        togglePass.addEventListener('click', function () {
            var hidden = password.type === 'password';
            password.type = hidden ? 'text' : 'password';
            togglePass.textContent = hidden ? 'Hide' : 'Show';
            togglePass.setAttribute('aria-pressed', hidden ? 'true' : 'false');
        });
    }

    // Form submit
    form?.addEventListener('submit', function (e) {
        e.preventDefault();

        // browser required checks
        var requiredInputs = Array.from(form.querySelectorAll('input[required]')).filter(function (i) { return i.offsetParent !== null; });
        for (var i = 0; i < requiredInputs.length; i++) {
            if (!requiredInputs[i].checkValidity()) {
                requiredInputs[i].reportValidity();
                return;
            }
        }

        // custom checks
        var fnOk = validateNameField(firstName, firstNameError);
        var lnOk = validateNameField(lastName, lastNameError);
        var emOk = validateEmail();
        var phOk = validatePhone();
        var pwdOk = validatePassword(false);
        var matchOk = checkPasswordsMatch(false);
        var termsOk = terms ? terms.checked : false;
        if (!termsOk) {
            terms.focus();
            alert('You must accept the terms to continue.');
            return;
        }

        if (!fnOk || !lnOk || !emOk || !phOk || !pwdOk || !matchOk) {
            if (!fnOk) firstName.focus();
            else if (!lnOk) lastName.focus();
            else if (!emOk) email.focus();
            else if (!phOk) phone.focus();
            else if (!pwdOk) password.focus();
            else if (!matchOk) confirm.focus();
            return;
        }

        // All client-side validation passed.
        // Uncomment form.submit() to perform the post to server.
        // form.submit();

        // For immediate UX show success panel (demo)
        var name = firstName.value.trim() + ' ' + lastName.value.trim();
        document.getElementById('successMessage').textContent =
            'Welcome, ' + name + '. Your registration was successful. You can now sign in and start booking.';
        if (formWrap) formWrap.hidden = true;
        if (successPage) successPage.hidden = false;
    });
});