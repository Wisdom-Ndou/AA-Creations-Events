
(function () {
    'use strict';

    
    const CONTACT = {
        company: 'AA Creations & Events',
        address: ['292 Anton Lembede', 'Durban CBD, Durban', 'South Africa'],
        contacts: [
            { name: 'Bookings', number: '0731232660' },   
            { name: 'Support', number: '0698432686' }     
        ],
        emails: [
            { name: 'Support', email: 'support@aa-creations.co.za' },
            { name: 'Info', email: 'info@aa-creations.co.za' }
        ],
        socials: [
            { platform: 'WhatsApp', url: 'https://wa.me/27731232660', handle: '+27 73 123 2660', icon: '💬' },
            { platform: 'Tiktok', url: 'https://www.tiktok.com/@aa.creation.event?_r=1&_t=ZS-99DXTwA3ZAK', handle: '/aacreations', icon: ' 🎶' },
            { platform: 'Instagram', url: 'https://www.instagram.com/aacreation.events?igsi=d2d5OGxkZnFxcDly&ut-m_source=qr', handle: '@aacreations', icon: '📸' }
        ],
       
    };

    
    function digitsOnly(v) { return String(v || '').replace(/\D/g, ''); }

    
    function telHref(number) {
        const d = digitsOnly(number);
        if (d.length === 9) return 'tel:+27' + d;          
        if (d.length === 10 && d.startsWith('0')) return 'tel:+' + d; 
    }

    function whatsappHref(number) {
        let d = digitsOnly(number);
        if (d.length === 10 && d.startsWith('0')) d = d.slice(1);
        if (d.length === 9) d = '27' + d;
        return 'https://wa.me/' + d;
    }

    function escapeHtml(s) {
        return String(s || '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
    }

    function formatPhoneDisplay(v) {
        const d = digitsOnly(v);
        if (d.length === 9) return '0' + d.slice(0, 2) + ' ' + d.slice(2, 5) + ' ' + d.slice(5);
        if (d.length === 10 && d.startsWith('0')) return d.slice(0, 3) + ' ' + d.slice(3, 6) + ' ' + d.slice(6);
        return v;
    }

    async function copyToClipboard(text, btn) {
        try {
            await navigator.clipboard.writeText(text);
            showTemp(btn, 'Copied');
        } catch (e) {
            const ta = document.createElement('textarea');
            ta.value = text;
            ta.style.position = 'fixed';
            ta.style.opacity = '0';
            document.body.appendChild(ta);
            ta.select();
            try { document.execCommand('copy'); showTemp(btn, 'Copied'); }
            catch { showTemp(btn, 'Failed'); }
            finally { document.body.removeChild(ta); }
        }
    }

    function showTemp(btn, txt) {
        const orig = btn.textContent;
        btn.textContent = txt;
        btn.disabled = true;
        setTimeout(() => { btn.textContent = orig; btn.disabled = false; }, 1200);
    }

    function buildHtml() {
        const h = [];
        h.push(`<section class="contact-hero card"><h2 id="contactTitle">${escapeHtml(CONTACT.company)} — Contact Us</h2><p class="muted">Reach out for bookings, support or general enquiries.</p></section>`);
        h.push(`<div class="contact-grid">`);

        
        h.push(`<div class="card contact-card">
      <h3>Address</h3>
      <address class="address-block">`);
        CONTACT.address.forEach(line => h.push(`<div>${escapeHtml(line)}</div>`));
        h.push(`<div><a href="${escapeHtml(CONTACT.mapUrl)}" target="_blank" rel="noopener">View on map</a></div>`);
        h.push(`</address></div>`);

        
        h.push(`<div class="card contact-card"><h3>Contact Numbers</h3><ul class="contact-list">`);
        CONTACT.contacts.forEach(c => {
            const tel = telHref(c.number);
            const wa = whatsappHref(c.number);
            h.push(`<li>
        <strong>${escapeHtml(c.name)}</strong>
        <div class="contact-row">
          <a class="contact-link phone-link" href="${escapeHtml(tel)}">${escapeHtml(formatPhoneDisplay(c.number))}</a>
          <button class="btn btn-plain copy-phone" data-value="${escapeHtml(formatPhoneDisplay(c.number))}" aria-label="Copy ${escapeHtml(c.name)} number">Copy</button>
          <a class="btn whatsapp" href="${escapeHtml(wa)}" target="_blank" rel="noopener">WhatsApp</a>
        </div>
      </li>`);
        });
        h.push(`</ul></div>`);

       
        h.push(`<div class="card contact-card"><h3>Email</h3><ul class="contact-list">`);
        CONTACT.emails.forEach(e => {
            h.push(`<li>
        <strong>${escapeHtml(e.name)}</strong>
        <div class="contact-row">
          <a class="contact-link email-link" href="mailto:${escapeHtml(e.email)}">${escapeHtml(e.email)}</a>
          <button class="btn btn-plain copy-email" data-value="${escapeHtml(e.email)}">Copy</button>
        </div>
      </li>`);
        });
        h.push(`</ul></div>`);

       
        h.push(`<div class="card contact-card contact-socials"><h3>Socials</h3><div class="socials-row">`);
        CONTACT.socials.forEach(s => {
            h.push(`<a class="social-link" href="${escapeHtml(s.url)}" target="_blank" rel="noopener">${escapeHtml(s.icon)} <span class="social-handle">${escapeHtml(s.handle)}</span></a>`);
        });
        h.push(`</div></div>`);

        h.push(`</div>`); 
        return h.join('');
    }

    function attach(root) {
        root.querySelectorAll('.copy-phone').forEach(btn => {
            btn.addEventListener('click', function () { copyToClipboard(this.dataset.value, this); });
        });
        root.querySelectorAll('.copy-email').forEach(btn => {
            btn.addEventListener('click', function () { copyToClipboard(this.dataset.value, this); });
        });

       
        root.querySelectorAll('.phone-link').forEach(a => {
            a.addEventListener('click', function () {
                
            });
        });
    }

    function render() {
        const root = document.getElementById('contactApp');
        if (!root) return;
        root.innerHTML = buildHtml();
        attach(root);
        
        const hash = location.hash && location.hash.toLowerCase();
        if (hash === '#contact' || location.pathname.toLowerCase().includes('/home/contactus')) {
            const title = root.querySelector('#contactTitle');
            if (title) title.focus();
        }
    }

    
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', render);
    } else {
        render();
    }

    
    window.ContactUsWidget = { render: render, update: cfg => Object.assign(CONTACT, cfg) && render() };
})();