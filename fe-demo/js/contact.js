(() => {
    'use strict';
    const root = document.querySelector('.contactWrap');
    if (!root) return;
    root.classList.add('content-motion');
    const reducedMotion = matchMedia('(prefers-reduced-motion: reduce)');
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
    let titleAnimation;
    const revealTitle = () => {
        titleAnimation?.kill();
        if (reducedMotion.matches || !window.gsap) {
            title.style.transform = 'none';
            return;
        }
        titleAnimation = gsap.fromTo(title, { y: 0, yPercent: 100, rotation: 10 }, {
            y: 0, yPercent: 0, rotation: 0, duration: 1, delay: 1, ease: 'power2.out'
        });
    };
    revealTitle();
    reducedMotion.addEventListener('change', revealTitle);
})();
