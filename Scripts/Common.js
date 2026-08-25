document.addEventListener("DOMContentLoaded", () => {
    const toggle = document.querySelector("[data-mobile-toggle]");
    const menu = document.querySelector("[data-mobile-menu]");

    if (toggle && menu) {
        toggle.addEventListener("click", () => {
            const open = menu.classList.toggle("open");
            toggle.setAttribute("aria-expanded", String(open));
        });

        menu.querySelectorAll("a").forEach(link => {
            link.addEventListener("click", () => {
                menu.classList.remove("open");
                toggle.setAttribute("aria-expanded", "false");
            });
        });
    }

    const currentPage = document.body.dataset.page;
    document.querySelectorAll("[data-nav-link]").forEach(link => {
        if (link.dataset.navLink === currentPage) {
            link.classList.add("active");
        }
    });
});
