// Customer register page script
// - Relaxed password rule: minimum 6 chars, must include at least one letter and one number
// - Live strength bar, inline error, aria updates
// - Password/confirm must match before submit

document.addEventListener('DOMContentLoaded', function () {
    function el(id) { return document.getElementById(id); }

    var form = el('registerForm');
    var formWrap = document.querySelector('.form-wrap');
    var successPage = el('successPage');
    var successMessage = el('successMessage');

    var passwordInput = el('password');
    var confirmInput = el('confirm');
    var mismatchText = el('mismatchText');
    var togglePassBtn = el('togglePass');

    var passwordError = el('passwordError');
    var strengthSegs = [el('seg1'), el('seg2'), el('seg3'), el('seg4')];

    // Relaxed rule: at least one letter and one digit, minimum 6 chars
    var passwordRegex = /^(?=.*[A-Za-z])(?=.*\d).{6,}$/;

    // Helper: set aria-invalid and class for an input
    function setInvalid(inputEl, invalid) {
        if (!inputEl) return;
        inputEl.setAttribute('aria-invalid', invalid ? 'true' : 'false');
        inputEl.classList.toggle('mismatch', invalid);
    }

    function showError(elError, show) {
        if (!elError) return;
        elError.hidden = !show;
    }

        // Update strength bar: simple scoring based on length, letters present, digits present
    function updateStrengthBar() {
        if (!passwordInput) return;
        var val = passwordInput.value || '';
        var score = 0;
        if (val.length >= 6) score++;
        if (/[A-Za-z]/.test(val)) score++;
        if (/\d/.test(val)) score++;

        // Fill up to 3 segments; use fourth as bonus for longer (>10)
        for (var i = 0; i < strengthSegs.length; i++) {
            var seg = strengthSegs[i];
            if (!seg) continue;
            seg.classList.remove('weak', 'medium', 'strong');
            if (i < score) {
                if (score === 1) seg.classList.add('weak');
                else if (score === 2) seg.classList.add('medium');
                else seg.classList.add('strong');
            } else if (i === 3 && val.length >= 10) {
                seg.classList.add('strong');
            }
        }
    }

    // Validate password according to the relaxed rule
    function validatePassword(lively) {
        if (!passwordInput) return true;
        var val = passwordInput.value || '';
        var ok = passwordRegex.test(val);

        // Be less noisy while typing: show only when user typed something if lively=true
        var show = !ok && (lively ? val.length > 0 : true);

        showError(passwordError, show);
        setInvalid(passwordInput, !ok);
        updateStrengthBar();
        return ok;
    }

    // Password/confirm match check
    function checkPasswordsMatch(lively) {
        if (!passwordInput || !confirmInput) return true;
        var p = passwordInput.value || '';
        var c = confirmInput.value || '';

        var strengthOk = validatePassword(true);

        var showMismatch = c.length > 0 && p !== c;
        if (lively === false) showMismatch = p !== c;

        if (mismatchText) mismatchText.hidden = !showMismatch;
        setInvalid(confirmInput, showMismatch);

        return !showMismatch && strengthOk;
    }

    // Wire events
    if (passwordInput) {
        passwordInput.addEventListener('input', function () {
            validatePassword(true);
            checkPasswordsMatch(true);
        });
        passwordInput.addEventListener('blur', function () { validatePassword(false); });
    }

    if (confirmInput) {
        confirmInput.addEventListener('input', function () { checkPasswordsMatch(true); });
        confirmInput.addEventListener('blur', function () { checkPasswordsMatch(false); });
    }

    if (togglePassBtn && passwordInput) {
        togglePassBtn.addEventListener('click', function () {
            var isHidden = passwordInput.type === 'password';
            passwordInput.type = isHidden ? 'text' : 'password';
            togglePassBtn.textContent = isHidden ? 'Hide' : 'Show';
        });
    }

    // Utility: visible required inputs
    function visibleRequiredInputs() {
        if (!form) return [];
        return Array.from(form.querySelectorAll('input[required], select[required]')).filter(function (i) {
            return i.offsetParent !== null;
        });
    }

    // Submit handler: validate visible inputs, password rule and match
    if (form) {
        form.addEventListener('submit', function (e) {
            e.preventDefault();

            // Browser-level checks for visible required fields
            var visible = visibleRequiredInputs();
            for (var i = 0; i < visible.length; i++) {
                if (!visible[i].checkValidity()) {
                    visible[i].reportValidity();
                    return;
                }
            }

            // Validate password rule and match
            var passwordsOk = checkPasswordsMatch(false);
            if (!passwordsOk) {
                if (!validatePassword(false)) {
                    if (passwordInput) passwordInput.focus();
                } else if (confirmInput) {
                    confirmInput.focus();
                }
                return;
            }

            // All client-side validation passed — show success UI
            var fn = (el('firstName') && el('firstName').value) ? el('firstName').value.trim() : '';
            var ln = (el('lastName') && el('lastName').value) ? el('lastName').value.trim() : '';
            var fullName = (fn + ' ' + ln).trim() || 'Customer';

            function escapeHtml(s) {
                return String(s).replace(/[&<>"'\/]/g, function (c) {
                    return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;', '/': '&#x2F;' }[c];
                });
            }

            if (successMessage) {
                successMessage.innerHTML = 'Welcome, <strong>' + escapeHtml(fullName) + '</strong>. Registration successful.';
            }
            if (formWrap) formWrap.hidden = true;
            if (successPage) successPage.hidden = false;

            // TODO: send to server here
        }, false);
    }
});