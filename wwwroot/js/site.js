{
    const translations = JSON.parse(document.getElementById('site-translations')?.textContent || '{}');
    window.koiText = (key, ...args) => (translations[key] || key).replace(/\{(\d+)\}/g, (_, index) => args[Number(index)] ?? '');
}
(() => {
    'use strict';
    const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
    const menu = document.querySelector('#navigation-dialog');
    const menuToggle = document.querySelector('.menu-toggle');
    let hoverMenu = false;
    let menuClosing = false;
    let hoverAfter = 0;
    const openMenu = (hover = false) => {
        if (!menu || menu.open || menuClosing) return;
        hoverMenu = hover;
        menu.showModal();
        menuToggle.setAttribute('aria-expanded', 'true');
        if (window.gsap && !reducedMotion.matches) {
            gsap.fromTo(menu, { rotation: 90 }, { rotation: 0, duration: 0.5, ease: 'power2.out', overwrite: true });
        }
    };
    const closeMenu = () => {
        if (!menu?.open || menuClosing) return;
        menuClosing = true;
        hoverMenu = false;
        const finish = () => {
            menu.close();
            menuClosing = false;
            hoverAfter = performance.now() + 300;
            menuToggle.setAttribute('aria-expanded', 'false');
        };
        if (window.gsap && !reducedMotion.matches) {
            gsap.to(menu, { rotation: 90, duration: 0.4, ease: 'power2.in', overwrite: true, onComplete: finish });
        } else finish();
    };
    menuToggle?.setAttribute('aria-expanded', 'false');
    menuToggle?.addEventListener('click', () => openMenu());
    document.querySelector('.menu-close')?.addEventListener('click', closeMenu);
    document.querySelector('.fan-navigation')?.addEventListener('pointerenter', () => {
        if (performance.now() > hoverAfter && window.matchMedia('(min-width: 1024px) and (hover: hover)').matches) openMenu(true);
    });
    menu?.addEventListener('pointerleave', () => { if (hoverMenu) closeMenu(); });
    // A modal dialog's backdrop also targets the dialog, so leaving the fan
    // does not always emit pointerleave. Check its bounds while hovering.
    menu?.addEventListener('pointermove', (event) => {
        if (!hoverMenu) return;
        const bounds = menu.getBoundingClientRect();
        if (event.clientX < bounds.left || event.clientX > bounds.right || event.clientY < bounds.top || event.clientY > bounds.bottom) closeMenu();
    });
    menu?.addEventListener('keydown', () => { hoverMenu = false; });
    menu?.addEventListener('cancel', (event) => { event.preventDefault(); closeMenu(); });
    menu?.addEventListener('click', (event) => {
        if (event.target !== menu) return;
        const bounds = menu.getBoundingClientRect();
        if (event.clientX < bounds.left || event.clientY > bounds.bottom) closeMenu();
    });
    menu?.querySelectorAll('a').forEach((link) => link.addEventListener('click', () => menu.close()));
    menu?.addEventListener('close', () => menuToggle.setAttribute('aria-expanded', 'false'));

    const video = document.querySelector('.hero-video');
    const motionToggle = document.querySelector('.motion-toggle');
    let userPausedVideo = false;
    const syncVideo = () => {
        if (!video) return;
        if (userPausedVideo || reducedMotion.matches || document.hidden) video.pause();
        else video.play().catch(() => { /* A browser may require a gesture before playback. */ });
    };
    motionToggle?.addEventListener('click', () => {
        userPausedVideo = !userPausedVideo;
        motionToggle.setAttribute('aria-pressed', String(userPausedVideo));
        motionToggle.setAttribute('aria-label', window.koiText(userPausedVideo ? 'Play background video' : 'Pause background video'));
        motionToggle.innerHTML = (userPausedVideo ? '▷ <span>' + window.koiText('PLAY') : 'Ⅱ <span>' + window.koiText('PAUSE')) + '</span>';
        syncVideo();
    });
    reducedMotion.addEventListener('change', syncVideo);
    document.addEventListener('visibilitychange', syncVideo);
    syncVideo();

    const drinkCarousel = document.querySelector('#drink-carousel');
    const drinkButtons = document.querySelectorAll('[data-drink-direction]');
    if (drinkCarousel) {
        const updateDrinkButtons = () => {
            if (document.body.classList.contains('motion-ready')) return;
            drinkButtons.forEach((button) => {
                const atStart = drinkCarousel.scrollLeft < 5;
                const atEnd = drinkCarousel.scrollLeft + drinkCarousel.clientWidth >= drinkCarousel.scrollWidth - 5;
                button.disabled = Number(button.dataset.drinkDirection) < 0 ? atStart : atEnd;
            });
        };
        drinkButtons.forEach((button) => button.addEventListener('click', () => {
            const direction = Number(button.dataset.drinkDirection);
            if (document.body.classList.contains('motion-ready')) {
                document.dispatchEvent(new CustomEvent('koi:drink-step', { detail: { direction } }));
                return;
            }
            const card = drinkCarousel.querySelector('.drink-card');
            if (!card) return;
            const gap = parseFloat(getComputedStyle(drinkCarousel).columnGap) || 0;
            drinkCarousel.scrollBy({ left: (card.offsetWidth + gap) * direction, behavior: reducedMotion.matches ? 'instant' : 'smooth' });
        }));
        drinkCarousel.addEventListener('scroll', updateDrinkButtons, { passive: true });
        window.addEventListener('resize', updateDrinkButtons);
        reducedMotion.addEventListener('change', () => requestAnimationFrame(updateDrinkButtons));
        updateDrinkButtons();
    }

    const newsSlides = [...document.querySelectorAll('.news-slide')];
    const newsButtons = [...document.querySelectorAll('[data-news-slide]')];
    let currentNews = 0;
    let switchingNews = false;
    newsButtons.forEach((button) => button.addEventListener('click', () => {
        const selected = Number(button.dataset.newsSlide);
        if (selected === currentNews || switchingNews) return;
        const outgoing = newsSlides[currentNews];
        const incoming = newsSlides[selected];
        const direction = selected > currentNews ? 1 : -1;
        currentNews = selected;
        newsButtons.forEach((item, index) => item.setAttribute('aria-pressed', String(index === selected)));
        incoming.hidden = false;
        if (!window.gsap || reducedMotion.matches) {
            outgoing.hidden = true;
            return;
        }
        switchingNews = true;
        outgoing.inert = true;
        gsap.set(outgoing, { position: 'absolute', inset: 0, width: '100%' });
        gsap.timeline({ onComplete: () => {
            outgoing.hidden = true;
            outgoing.inert = false;
            gsap.set([outgoing, incoming], { clearProps: 'transform,position,inset,width' });
            switchingNews = false;
        } })
            .to(outgoing, { xPercent: -100 * direction, duration: 1, ease: 'power2.inOut' }, 0)
            .fromTo(incoming, { xPercent: 100 * direction }, { xPercent: 0, duration: 1, ease: 'power2.inOut' }, 0);
    }));
})();
