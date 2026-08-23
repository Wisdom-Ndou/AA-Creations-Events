// AA Creations & Events — Sign In
// Vanilla JS replacement for the React useState logic in the original .tsx

document.addEventListener('DOMContentLoaded', function () {
    var role = 'customer'; // mirrors useState<Role>('customer')

    var roleCustomerBtn = document.getElementById('roleCustomerBtn');
    var roleAdminBtn = document.getElementById('roleAdminBtn');
    var accessCodeField = document.getElementById('accessCodeField');
    var accessCodeInput = document.getElementById('accessCode');
    var emailInput = document.getElementById('email');
    var passwordInput = document.getElementById('password');
    var togglePassBtn = document.getElementById('togglePass');
    var submitBtn = document.getElementById('submitBtn');
    var registerLink = document.getElementById('registerLink');
    var indicatorDot = document.getElementById('indicatorDot');
    var indicatorText = document.getElementById('indicatorText');
    var loginForm = document.getElementById('loginForm');

    function applyRole(newRole) {
        role = newRole;

        // Segmented control active state
        roleCustomerBtn.classList.toggle('active', role === 'customer');
        roleAdminBtn.classList.toggle('active', role === 'admin');
        roleCustomerBtn.setAttribute('aria-selected', role === 'customer');
        roleAdminBtn.setAttribute('aria-selected', role === 'admin');

        // Show/hide the admin access code field
        accessCodeField.hidden = role !== 'admin';
        accessCodeInput.required = role === 'admin';

        // Email placeholder swap
        emailInput.placeholder = role === 'admin' ? 'admin@aacreations.co.za' : 'you@example.com';

        // Submit button label
        submitBtn.textContent = role === 'admin' ? 'Sign In as Admin' : 'Sign In';

        // Register link — points at admin vs customer registration page
        registerLink.href = role === 'admin' ? 'AdminRegister.html' : 'CustomerRegister.html';
        registerLink.textContent = role === 'admin' ? 'Request Admin Access' : 'Create Account';

        // Role indicator dot + text
        indicatorDot.style.background = role === 'admin' ? '#d4006a' : '#5c1040';
        indicatorText.textContent = 'Signing in as ' + role;
    }

    roleCustomerBtn.addEventListener('click', function () {
        applyRole('customer');
    });
    roleAdminBtn.addEventListener('click', function () {
        applyRole('admin');
    });

    // --- Show / hide password ---
    togglePassBtn.addEventListener('click', function () {
        var isHidden = passwordInput.type === 'password';
        passwordInput.type = isHidden ? 'text' : 'password';
        togglePassBtn.textContent = isHidden ? 'Hide' : 'Show';
    });

    // --- Submit handler ---
    // The original TSX only called e.preventDefault() and did nothing else
    // (no API call existed yet). Kept that behaviour, plus a console log
    // and a TODO for wiring this to the MVC controller later.
    loginForm.addEventListener('submit', function (e) {
        e.preventDefault();

        // TODO: replace this with an actual POST to your MVC controller action,
        // e.g. fetch('/Account/Login', { method: 'POST', body: new FormData(loginForm) })
        console.log('Login attempt:', {
            role: role,
            email: emailInput.value,
            password: passwordInput.value,
            accessCode: role === 'admin' ? accessCodeInput.value : undefined,
        });
    });

    // Initialise UI state on load
    applyRole(role);
});