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
    const options = [...root.querySelectorAll('[data-contact-type]')];
    const groups = [
        [root.querySelector('.content-1'), root.querySelector('.content-2')],
        [root.querySelector('#contactForm1'), root.querySelector('#contactForm2')]
    ];
    const overseas = root.querySelector('.overseas');
    const animated = [...groups.flat(), overseas];
    let current = 1;
    let transition;
    const showPanels = () => {
        groups.forEach(group => group.forEach((panel, index) => {
            panel.hidden = index + 1 !== current;
            panel.inert = panel.hidden;
            panel.style.opacity = '';
        }));
        overseas.hidden = current !== 2;
        overseas.style.opacity = '';
        options.forEach(option => option.checked = Number(option.value) === current);
    };
    const selectType = value => {
        if (current === value) return;
        transition?.kill();
        current = value;
        options.forEach(option => option.checked = Number(option.value) === current);
        if (reducedMotion.matches || !window.gsap) {
            showPanels();
            return;
        }
        const visible = animated.filter(panel => !panel.hidden);
        // Vue's out-in transition in the reference fades the old panels before mounting the new ones.
        transition = gsap.timeline()
            .to(visible, { opacity: 0, duration: .5, ease: 'none' })
            .call(() => {
                showPanels();
                gsap.set(animated.filter(panel => !panel.hidden), { opacity: 0 });
            })
            .to(animated, { opacity: 1, duration: .5, ease: 'none', clearProps: 'opacity' });
    };
    options.forEach(option => option.addEventListener('change', () => selectType(Number(option.value))));
    reducedMotion.addEventListener('change', () => {
        transition?.kill();
        showPanels();
        revealTitle();
    });
    showPanels();
})();
