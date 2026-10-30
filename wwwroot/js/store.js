(() => {
    const root = document.querySelector('.storeWrap');
    if (!root) return;
    root.classList.add('content-motion');
    const startAtTop = () => {
        if (!location.hash) {
            history.scrollRestoration = 'manual';
            window.ScrollTrigger?.clearScrollMemory('manual');
            scrollTo({ top: 0, behavior: 'instant' });
        }
    };
    startAtTop();
    addEventListener('pageshow', startAtTop);
    const title = root.querySelector('.head-area .en');
    const reducedMotion = matchMedia('(prefers-reduced-motion: reduce)');
    let animation;
    const revealTitle = () => {
        animation?.kill();
        if (reducedMotion.matches || !window.gsap) {
            title.style.transform = 'none';
            return;
        }
        animation = gsap.fromTo(title, { y: 0, yPercent: 100, rotation: 10 }, {
            y: 0, yPercent: 0, rotation: 0, duration: 1, delay: 1, ease: 'power2.out'
        });
    };
    revealTitle();
    reducedMotion.addEventListener('change', revealTitle);
    const dialog = root.querySelector('#store-photo-dialog');
    const photo = dialog.querySelector('.pic img');
    root.querySelectorAll('[data-store-photo]').forEach(link => {
        link.addEventListener('click', event => {
            if (event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;
            event.preventDefault();
            photo.src = link.href;
            photo.alt = link.dataset.storeName;
            dialog.setAttribute('aria-label', `Photo of ${link.dataset.storeName}`);
            dialog.showModal();
            dialog.classList.add('is-show');
        });
    });
    dialog.querySelector('.back').addEventListener('click', () => dialog.close());
    dialog.addEventListener('click', event => {
        if (!event.target.closest('.pic, .back')) dialog.close();
    });
    dialog.addEventListener('close', () => dialog.classList.remove('is-show'));
})();
