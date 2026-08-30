// Customer Registration Form Validation
// Shows success message after account creation

document.addEventListener('DOMContentLoaded', function () {
    function el(id) { return document.getElementById(id); }

    var form = el('registerForm');
    var formWrap = document.querySelector('.form-wrap');
    var successPage = el('successPage');
    var successMessage = el('successMessage');

    var firstNameInput = el('firstName');
    var lastNameInput = el('lastName');
    var emailInput = el('email');
    var phoneInput = el('phone');
    var firstNameError = el('firstNameError');
    var lastNameError = el('lastNameError');
    var phoneError = el('phoneError');

    var passwordInput = el('password');
    var confirmInput = el('confirm');
    var mismatchText = el('mismatchText');
    var togglePassBtn = el('togglePass');

    var passwordError = el('passwordError');
    var strengthSegs = [el('seg1'), el('seg2'), el('seg3'), el('seg4')];

    // Regex patterns
    var nameRegex = /^[A-Za-z\s]*$/;  // Only letters and spaces
    var phoneRegex = /^[1-9]\d{8}$/;  // 9 digits, doesn't start with 0
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

    // ===== NAME VALIDATION =====
    function sanitizeNameInput(inputEl) {
        if (!inputEl) return;
        var currentValue = inputEl.value;
        var sanitizedValue = currentValue.replace(/[^A-Za-z\s]/g, '');  // Keep letters and spaces only
        if (currentValue !== sanitizedValue) {
            inputEl.value = sanitizedValue;
        }
    }

    function validateName(inputEl, errorEl) {
        if (!inputEl) return true;
        var val = inputEl.value || '';
        var isValid = nameRegex.test(val) && val.trim().length > 0;
        var hasContent = val.trim().length > 0;
        var hasInvalidChars = !nameRegex.test(val);
        var shouldShowError = hasContent && hasInvalidChars;
        showError(errorEl, shouldShowError);
        setInvalid(inputEl, !isValid && hasContent);
        return isValid || val.trim().length === 0;
    }

    function validateFirstName() {
        if (!firstNameInput) return true;
        sanitizeNameInput(firstNameInput);
        return validateName(firstNameInput, firstNameError);
    }

    function validateLastName() {
        if (!lastNameInput) return true;
        sanitizeNameInput(lastNameInput);
        return validateName(lastNameInput, lastNameError);
    }

    // ===== PREVENT NUMBER INPUT IN REAL-TIME =====
    if (firstNameInput) {
        firstNameInput.addEventListener('input', function () {
            sanitizeNameInput(this);
            validateFirstName();
        });

        firstNameInput.addEventListener('keypress', function (e) {
            var char = String.fromCharCode(e.which);
            if (!/[A-Za-z\s]/.test(char)) {
                e.preventDefault();
                return false;
            }
        });

        firstNameInput.addEventListener('paste', function (e) {
            e.preventDefault();
            var pastedText = (e.clipboardData || window.clipboardData).getData('text');
            var sanitizedText = pastedText.replace(/[^A-Za-z\s]/g, '');
            this.value = sanitizedText;
            validateFirstName();
        });

        firstNameInput.addEventListener('blur', function () { validateFirstName(); });
    }

    if (lastNameInput) {
        lastNameInput.addEventListener('input', function () {
            sanitizeNameInput(this);
            validateLastName();
        });

        lastNameInput.addEventListener('keypress', function (e) {
            var char = String.fromCharCode(e.which);
            if (!/[A-Za-z\s]/.test(char)) {
                e.preventDefault();
                return false;
            }
        });

        lastNameInput.addEventListener('paste', function (e) {
            e.preventDefault();
            var pastedText = (e.clipboardData || window.clipboardData).getData('text');
            var sanitizedText = pastedText.replace(/[^A-Za-z\s]/g, '');
            this.value = sanitizedText;
            validateLastName();
        });

        lastNameInput.addEventListener('blur', function () { validateLastName(); });
    }

    // ===== PHONE VALIDATION =====
    function validatePhone() {
        if (!phoneInput) return true;
        var val = phoneInput.value || '';
        var isValid = phoneRegex.test(val);
        var shouldShowError = val.length > 0 && !isValid;
        showError(phoneError, shouldShowError);
        setInvalid(phoneInput, !isValid && val.length > 0);
        return isValid || val.length === 0;
    }

    if (phoneInput) {
        phoneInput.addEventListener('input', function () {
            validatePhone();
        });
        phoneInput.addEventListener('blur', function () { validatePhone(); });
    }

    // ===== PASSWORD VALIDATION =====
    function updateStrengthBar() {
        if (!passwordInput) return;
        var val = passwordInput.value || '';
        var score = 0;
        if (val.length >= 6) score++;
        if (/[A-Za-z]/.test(val)) score++;
        if (/\d/.test(val)) score++;

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

    function validatePassword(lively) {
        if (!passwordInput) return true;
        var val = passwordInput.value || '';
        var ok = passwordRegex.test(val);
        var show = !ok && (lively ? val.length > 0 : true);
        showError(passwordError, show);
        setInvalid(passwordInput, !ok);
        updateStrengthBar();
        return ok;
    }

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

    // Submit handler
    if (form) {
        form.addEventListener('submit', function (e) {
            e.preventDefault();

            // Validate all fields
            var firstNameOk = validateFirstName();
            var lastNameOk = validateLastName();
            var phoneOk = validatePhone();

            if (!firstNameOk || !lastNameOk || !phoneOk) {
                if (!firstNameOk && firstNameInput) firstNameInput.focus();
                else if (!lastNameOk && lastNameInput) lastNameInput.focus();
                else if (!phoneOk && phoneInput) phoneInput.focus();
                return;
            }

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

            // All validation passed — send to server
            var fn = (firstNameInput && firstNameInput.value) ? firstNameInput.value.trim() : '';
            var ln = (lastNameInput && lastNameInput.value) ? lastNameInput.value.trim() : '';
            var email = (emailInput && emailInput.value) ? emailInput.value.trim() : '';
            var phone = (phoneInput && phoneInput.value) ? phoneInput.value.trim() : '';
            var password = passwordInput.value || '';

            var fullName = (fn + ' ' + ln).trim() || 'Customer';

            function escapeHtml(s) {
                return String(s).replace(/[&<>"'\/]/g, function (c) {
                    return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;', '/': '&#x2F;' }[c];
                });
            }

            // Disable submit button
            var submitBtn = el('submitBtn');
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.textContent = 'Creating Account...';
            }

            // Send to server
            fetch('/Cust/Customerregister', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    firstName: fn,
                    lastName: ln,
                    email: email,
                    phone: phone,
                    password: password
                })
            })
            .then(function(response) {
                return response.json();
            })
            .then(function(data) {
                if (data.success) {
                    // Show success message
                    if (successMessage) {
                        successMessage.innerHTML = 'Welcome, <strong>' + escapeHtml(fullName) + '</strong>! Your registration was successful. You can now sign in and start booking your celebrations.';
                    }
                    
                    // Hide form and show success page
                    if (formWrap) formWrap.hidden = true;
                    if (successPage) successPage.hidden = false;

                    // Redirect to login after 3 seconds
                    setTimeout(function() {
                        window.location.href = '/Cust/Login';
                    }, 3000);
                } else {
                    alert('Registration failed: ' + (data.message || 'Please try again'));
                    
                    // Re-enable submit button
                    if (submitBtn) {
                        submitBtn.disabled = false;
                        submitBtn.textContent = 'Create Account';
                    }
                }
            })
            .catch(function(error) {
                console.error('Error:', error);
                alert('An error occurred. Please try again.');
                
                // Re-enable submit button
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.textContent = 'Create Account';
                }
            });

        }, false);
    }
});