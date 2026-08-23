// AA Creations & Events — Customer Registration
// Vanilla JS replacement for the React useState logic in the original .tsx

document.addEventListener('DOMContentLoaded', function () {
    var steps = ['Personal', 'Contact', 'Security'];
    var step = 0; // mirrors useState(0)

    var panels = [
        document.getElementById('panel0'),
        document.getElementById('panel1'),
        document.getElementById('panel2'),
    ];
    var badges = [
        document.getElementById('badge0'),
        document.getElementById('badge1'),
        document.getElementById('badge2'),
    ];
    var labels = [
        document.getElementById('label0'),
        document.getElementById('label1'),
        document.getElementById('label2'),
    ];
    var connectors = document.querySelectorAll('.step-connector');

    var backBtn = document.getElementById('backBtn');
    var continueBtn = document.getElementById('continueBtn');
    var submitBtn = document.getElementById('submitBtn');
    var form = document.getElementById('registerForm');
    var formWrap = document.querySelector('.form-wrap');
    var successPage = document.getElementById('successPage');

    var passwordInput = document.getElementById('password');
    var confirmInput = document.getElementById('confirm');
    var togglePassBtn = document.getElementById('togglePass');
    var mismatchText = document.getElementById('mismatchText');
    var strengthSegs = [
        document.getElementById('seg1'),
        document.getElementById('seg2'),
        document.getElementById('seg3'),
        document.getElementById('seg4'),
    ];

    // --- Per-step required fields, so hidden panels don't block validation ---
    var stepFieldIds = [
        ['firstName', 'lastName', 'city'],
        ['email', 'phone'],
        ['password', 'confirm'],
    ];

    function renderStepIndicator() {
        for (var i = 0; i < steps.length; i++) {
            badges[i].classList.remove('current', 'done');
            labels[i].classList.remove('current');

            if (i < step) {
                badges[i].classList.add('done');
                badges[i].textContent = '✓';
            } else if (i === step) {
                badges[i].classList.add('current');
                labels[i].classList.add('current');
                badges[i].textContent = String(i + 1);
            } else {
                badges[i].textContent = String(i + 1);
            }
        }
        connectors.forEach(function (connector, i) {
            connector.classList.toggle('done', i < step);
        });
    }

    function renderPanels() {
        panels.forEach(function (panel, i) {
            panel.hidden = i !== step;
        });

        backBtn.hidden = step === 0;
        continueBtn.hidden = step === steps.length - 1;
        submitBtn.hidden = step !== steps.length - 1;

        // Only the visible step's fields are required, so browser validation
        // doesn't complain about fields the user can't currently see.
        stepFieldIds.forEach(function (ids, i) {
            ids.forEach(function (id) {
                var el = document.getElementById(id);
                if (el) el.required = i === step && id !== 'newsletter';
            });
        });
    }

    function goToStep(newStep) {
        step = newStep;
        renderStepIndicator();
        renderPanels();
    }

    function currentStepIsValid() {
        var panel = panels[step];
        var inputs = panel.querySelectorAll('input[required]');
        for (var i = 0; i < inputs.length; i++) {
            if (!inputs[i].checkValidity()) {
                inputs[i].reportValidity();
                return false;
            }
        }
        if (step === 2 && passwordInput.value !== confirmInput.value) {
            confirmInput.classList.add('mismatch');
            mismatchText.hidden = false;
            confirmInput.focus();
            return false;
        }
        return true;
    }

    continueBtn.addEventListener('click', function () {
        if (!currentStepIsValid()) return;
        goToStep(step + 1);
    });

    backBtn.addEventListener('click', function () {
        goToStep(step - 1);
    });

    // --- Show / hide password ---
    togglePassBtn.addEventListener('click', function () {
        var isHidden = passwordInput.type === 'password';
        passwordInput.type = isHidden ? 'text' : 'password';
        togglePassBtn.textContent = isHidden ? 'Hide' : 'Show';
    });

    // --- Password strength bar (mirrors the length-based bar in the TSX) ---
    function updateStrengthBar() {
        var len = passwordInput.value.length;
        strengthSegs.forEach(function (seg, idx) {
            var i = idx + 1; // 1..4
            seg.classList.remove('weak', 'medium', 'strong');
            if (len >= i * 3) {
                if (i <= 2) seg.classList.add('weak');
                else if (i === 3) seg.classList.add('medium');
                else seg.classList.add('strong');
            }
        });
    }

    // --- Live confirm-password match styling ---
    function checkConfirmMatch() {
        confirmInput.classList.remove('match', 'mismatch');
        mismatchText.hidden = true;
        if (!confirmInput.value) return;
        if (passwordInput.value === confirmInput.value) {
            confirmInput.classList.add('match');
        } else {
            confirmInput.classList.add('mismatch');
            mismatchText.hidden = false;
        }
    }

    passwordInput.addEventListener('input', function () {
        updateStrengthBar();
        checkConfirmMatch();
    });
    confirmInput.addEventListener('input', checkConfirmMatch);

    // --- Final submit ---
    form.addEventListener('submit', function (e) {
        e.preventDefault();

        if (!currentStepIsValid()) return;

        // TODO: replace this with an actual POST to your MVC controller action,
        // e.g. fetch('/Customer/Register', { method: 'POST', body: new FormData(form) })
        console.log('Customer registration data:', Object.fromEntries(new FormData(form)));

        formWrap.hidden = true;
        successPage.hidden = false;
    });

    // Initialise UI state
    renderStepIndicator();
    renderPanels();
});