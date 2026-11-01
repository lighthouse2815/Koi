(() => {
    'use strict';
    const catalog = document.querySelector('.tea-catalog');
    const order = document.querySelector('.order-page');
    if (!catalog && !order) return;

    if (catalog) {
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
    gsap.matchMedia().add('(prefers-reduced-motion: no-preference)', () => {
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
        if (order) {
            reveal(order.querySelector('.order-intro'), order);
            order.querySelectorAll('.order-card').forEach(panel => reveal(panel, panel));
        }
        document.fonts.ready.then(() => ScrollTrigger.refresh());
    });
})();
