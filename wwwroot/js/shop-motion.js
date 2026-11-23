(() => {
    'use strict';
    const catalog = document.querySelector('.tea-catalog');
    const order = document.querySelector('.order-page');
    if (!catalog && !order) return;

    if (catalog && 'IntersectionObserver' in window) {
        const links = [...catalog.querySelectorAll('.tea-category-nav a')];
        const observer = new IntersectionObserver(entries => {
            const entry = entries.find(value => value.isIntersecting);
            if (!entry) return;
            links.forEach(link => {
                if (link.hash === '#' + entry.target.id) link.setAttribute('aria-current', 'true');
                else link.removeAttribute('aria-current');
            });
        }, { rootMargin: '-12% 0px -65% 0px' });
        catalog.querySelectorAll('.tea-category').forEach(section => observer.observe(section));
    }

    if (!window.gsap || !window.ScrollTrigger) return;
    gsap.registerPlugin(ScrollTrigger);
    gsap.matchMedia().add('(prefers-reduced-motion: no-preference)', context => {
        const reveal = (elements, trigger) => gsap.fromTo(elements,
            { autoAlpha: 0, y: 45 },
            { autoAlpha: 1, y: 0, duration: .8, stagger: .09, ease: 'power3.out', clearProps: 'opacity,visibility,transform',
                scrollTrigger: { trigger, start: 'top 92%', once: true } });
        if (catalog) {
            reveal(catalog.querySelectorAll('.tea-catalog-heading > *, .tea-category-nav'), catalog);
            catalog.querySelectorAll('.tea-category').forEach(section => {
                gsap.fromTo(section.querySelector('.tea-category-rule'), { scaleX: 0 }, {
                    scaleX: 1, duration: 1.1, ease: 'power2.inOut', scrollTrigger: { trigger: section, start: 'top 92%', once: true }
                });
                reveal(section.querySelector('.tea-category-heading'), section);
                section.querySelectorAll('.tea-product').forEach((card, index) => {
                    gsap.fromTo(card, { autoAlpha: 0, y: 65, rotation: index % 2 ? 2 : -2 }, {
                        autoAlpha: 1, y: 0, rotation: 0, duration: .9, delay: index % 2 * .08, ease: 'power3.out', clearProps: 'opacity,visibility,transform',
                        scrollTrigger: { trigger: card, start: 'top 94%', once: true }
                    });
                });
            });
        }
        document.fonts.ready.then(() => ScrollTrigger.refresh());
        if (!order) return;

        const intro = gsap.timeline({ defaults: { duration: .7, ease: 'power3.out' } });
        intro.from(order.querySelectorAll('.order-title-line > *'), { yPercent: 108, stagger: .11, clearProps: 'transform' }, 0);

        const introCopy = order.querySelectorAll('.order-hero-copy > p, .order-hero-copy > a, .order-confirmation > p, .order-confirmation-actions, .order-history-heading p, .order-history-heading > a');
        intro.from(introCopy, { opacity: .25, y: 14, stagger: .08, clearProps: 'opacity,transform' }, .18);

        const heroArt = order.querySelector('.order-hero-art');
        if (heroArt) {
            intro
                .from(heroArt, { opacity: 0, scale: .91, clearProps: 'opacity,transform' }, .06)
                .from(heroArt.querySelector('.order-hero-drink'), { opacity: 0, y: 32, rotation: 12, duration: .9, clearProps: 'opacity,transform' }, .15)
                .from(heroArt.querySelector('.order-hero-orbit'), { scale: .68, rotation: -38, clearProps: 'transform' }, .24)
                .from(heroArt.querySelector('.order-hero-spark'), { scale: .35, rotation: -75, ease: 'back.out(1.8)', clearProps: 'transform' }, .4);
        }

        const confirmation = order.querySelector('.order-confirmation-mark');
        if (confirmation) {
            intro.from(confirmation, { scale: .35, rotation: -18, ease: 'back.out(1.8)', clearProps: 'transform' }, 0);
        }

        const revealOrderCards = cards => cards.forEach((card, index) => {
            gsap.from(card, {
                opacity: .25, y: 22, duration: .65, delay: index % 3 * .065, ease: 'power3.out',
                clearProps: 'opacity,transform',
                scrollTrigger: { trigger: card, start: 'top 97%', once: true }
            });
        });

        revealOrderCards(order.querySelectorAll('[data-drink-id], .order-history-card, .order-history-empty'));

        const revealMore = event => context.add(() => {
            revealOrderCards(event.detail.cards);
            ScrollTrigger.refresh();
        });
        const refreshOnToggle = event => {
            if (event.target.matches('.branch-picker, .drink-customize')) ScrollTrigger.refresh(true);
        };
        order.addEventListener('menu:loaded', revealMore);
        order.addEventListener('toggle', refreshOnToggle, true);
        return () => {
            order.removeEventListener('menu:loaded', revealMore);
            order.removeEventListener('toggle', refreshOnToggle, true);
        };
    });
})();
