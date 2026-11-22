const adminMenu = document.querySelector('#admin-menu');
const adminMenuToggle = document.querySelector('.admin-menu-toggle');

if (adminMenu && adminMenuToggle) {
    const setMenuOpen = open => {
        adminMenuToggle.setAttribute('aria-expanded', String(open));
        adminMenu.dataset.open = String(open);
    };

    setMenuOpen(false);
    adminMenuToggle.hidden = false;

    adminMenuToggle.addEventListener('click', () => {
        setMenuOpen(adminMenuToggle.getAttribute('aria-expanded') !== 'true');
    });

    document.addEventListener('keydown', event => {
        if (event.key === 'Escape' && adminMenu.dataset.open === 'true' && adminMenuToggle.offsetWidth > 0) {
            setMenuOpen(false);
            adminMenuToggle.focus();
        }
    });
}
