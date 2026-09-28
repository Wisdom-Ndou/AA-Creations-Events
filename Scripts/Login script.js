// AA Creations & Events — Sign In

document.addEventListener('DOMContentLoaded', function () {

    var role = 'customer';

    var roleCustomerBtn = document.getElementById('roleCustomerBtn');
    var roleAdminBtn = document.getElementById('roleAdminBtn');

    var accessCodeField = document.getElementById('accessCodeField');
    var accessCodeInput = document.getElementById('adminAccessCode');

    var emailInput = document.getElementById('email');
    var passwordInput = document.getElementById('password');

    var togglePassBtn = document.getElementById('togglePass');
    var submitBtn = document.getElementById('submitBtn');

    var indicatorDot = document.getElementById('indicatorDot');
    var indicatorText = document.getElementById('indicatorText');

    var loginForm = document.getElementById('loginForm');

    var customerRegisterRow =
        document.getElementById('customerRegisterRow');

    var adminRegisterRow =
        document.getElementById('adminRegisterRow');

    // ==========================================
    // CHANGE ROLE
    // ==========================================

    function applyRole(newRole) {

        role = newRole;

        // Update active tab
        roleCustomerBtn.classList.toggle(
            'active',
            role === 'customer'
        );

        roleAdminBtn.classList.toggle(
            'active',
            role === 'admin'
        );

        roleCustomerBtn.setAttribute(
            'aria-selected',
            role === 'customer'
        );

        roleAdminBtn.setAttribute(
            'aria-selected',
            role === 'admin'
        );

        // ==========================================
        // ADMIN ACCESS CODE
        // ==========================================

        if (role === 'admin') {

            accessCodeField.hidden = false;
            accessCodeInput.required = true;

            emailInput.placeholder =
                'admin@aacreations.co.za';

            submitBtn.textContent =
                'Sign In as Admin';

        } else {

            accessCodeField.hidden = true;
            accessCodeInput.required = false;

            // Clear access code when switching back
            accessCodeInput.value = '';

            emailInput.placeholder =
                'you@example.com';

            submitBtn.textContent =
                'Sign In';
        }

        // ==========================================
        // INDICATOR
        // ==========================================

        indicatorDot.style.background =
            role === 'admin'
                ? '#d4006a'
                : '#5c1040';

        indicatorText.textContent =
            'Signing in as ' + role;

        // ==========================================
        // REGISTRATION LINKS
        // ==========================================

        if (customerRegisterRow) {

            customerRegisterRow.style.display =
                role === 'customer'
                    ? 'block'
                    : 'none';
        }

        if (adminRegisterRow) {

            adminRegisterRow.style.display =
                role === 'admin'
                    ? 'block'
                    : 'none';
        }
    }

    // ==========================================
    // CUSTOMER BUTTON
    // ==========================================

    roleCustomerBtn.addEventListener('click', function () {

        applyRole('customer');

    });

    // ==========================================
    // ADMIN BUTTON
    // ==========================================

    roleAdminBtn.addEventListener('click', function () {

        applyRole('admin');

    });

    // ==========================================
    // SHOW / HIDE PASSWORD
    // ==========================================

    togglePassBtn.addEventListener('click', function () {

        var isHidden =
            passwordInput.type === 'password';

        passwordInput.type =
            isHidden ? 'text' : 'password';

        togglePassBtn.textContent =
            isHidden ? 'Hide' : 'Show';
    });

    // ==========================================
    // FORM SUBMISSION
    // ==========================================

    loginForm.addEventListener('submit', function (e) {

        // Check email
        if (!emailInput.value.trim()) {

            e.preventDefault();

            emailInput.focus();

            return;
        }

        // Check password
        if (!passwordInput.value.trim()) {

            e.preventDefault();

            passwordInput.focus();

            return;
        }

        // ==========================================
        // ADMIN VALIDATION
        // ==========================================

        if (role === 'admin') {

            if (!accessCodeInput.value.trim()) {

                e.preventDefault();

                alert('Please enter the admin access code.');

                accessCodeInput.focus();

                return;
            }
        }

        // ==========================================
        // SEND ROLE TO MVC
        // ==========================================

        var roleInput =
            document.getElementById('role');

        if (roleInput) {

            roleInput.value = role;
        }

        /*
         * IMPORTANT:
         *
         * We do NOT call e.preventDefault()
         * here.
         *
         * The form will now POST to:
         *
         * CustController -> Login()
         */
    });

    // ==========================================
    // INITIAL STATE
    // ==========================================

    applyRole('customer');

});