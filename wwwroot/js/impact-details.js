(() => {
    'use strict';
    const page = document.querySelector('.impact-detail');
    if (!page) return;
    const reducedMotion = matchMedia('(prefers-reduced-motion: reduce)');
    page.querySelectorAll('.scroll-down').forEach(button => {
        button.setAttribute('role', 'button');
        button.setAttribute('aria-label', 'Explore more');
        button.tabIndex = 0;
        const scroll = () => button.closest('section').nextElementSibling?.scrollIntoView({ behavior: reducedMotion.matches ? 'instant' : 'smooth' });
        button.addEventListener('click', scroll);
        button.addEventListener('keydown', event => {
            if (event.key === 'Enter' || event.key === ' ') {
                event.preventDefault();
                scroll();
            }
        });
    });

    page.querySelectorAll('[class$="-video__play"]').forEach(button => {
        const section = button.closest('section');
        const frame = button.parentElement;
        const iframe = frame.querySelector('iframe');
        button.addEventListener('click', () => {
            const url = new URL(iframe.dataset.src || iframe.src);
            url.searchParams.set('autoplay', '1');
            url.searchParams.set('mute', '0');
            iframe.src = url.href;
            frame.classList.add('is-playing');
            section.classList.add('is-playing');
            button.hidden = true;
        });
        section.querySelector('[class$="-video__back"]')?.addEventListener('click', () => {
            iframe.src = iframe.dataset.src || iframe.src.replace('autoplay=1', 'autoplay=0');
            frame.classList.remove('is-playing');
            section.classList.remove('is-playing');
            button.hidden = false;
        });
    });

    for (const prefix of ['people-features', 'connections-share']) {
        const section = page.querySelector('.' + prefix);
        if (!section) continue;
        const track = section.querySelector('.' + prefix + '__track');
        let viewport = track.parentElement;
        if (prefix === 'connections-share') {
            viewport = document.createElement('div');
            viewport.className = 'details-carousel-viewport';
            track.before(viewport);
            viewport.append(track);
        }
        const items = [...track.children];
        const dots = [...section.querySelectorAll('.dot')];
        let index = 0;
        const select = next => {
            index = (next + items.length) % items.length;
            viewport.scrollTo({ left: items[index].offsetLeft - items[0].offsetLeft, behavior: reducedMotion.matches ? 'instant' : 'smooth' });
            dots.forEach((dot, i) => { dot.classList.toggle('active', i === index); dot.setAttribute('aria-pressed', String(i === index)); });
        };
        section.querySelector('.' + prefix + '__arrow--prev')?.addEventListener('click', () => select(index - 1));
        section.querySelector('.' + prefix + '__arrow--next')?.addEventListener('click', () => select(index + 1));
        dots.forEach((dot, i) => dot.addEventListener('click', () => select(i)));
        select(0);
    }

    const feature = page.querySelector('.connections-feature');
    if (feature) {
        const slides = [...feature.querySelectorAll('.connections-feature__slide')];
        const panels = [...feature.querySelectorAll('.connections-feature__panel')];
        const dots = [...feature.querySelectorAll('.dot')];
        let index = 0;
        const select = next => {
            index = (next + slides.length) % slides.length;
            [slides, panels, dots].forEach(group => group.forEach((item, i) => item.classList.toggle('active', i === index)));
            dots.forEach((dot, i) => dot.setAttribute('aria-pressed', String(i === index)));
            feature.querySelector('.connections-feature__progress span').style.transform = 'translateX(' + index * 100 + '%)';
        };
        feature.querySelector('.connections-feature__arrow--prev').addEventListener('click', () => select(index - 1));
        feature.querySelector('.connections-feature__arrow--next').addEventListener('click', () => select(index + 1));
        dots.forEach((dot, i) => dot.addEventListener('click', () => select(i)));
        select(0);
    }

    const numbers = [...page.querySelectorAll('[data-value]')];
    numbers.forEach(item => item.textContent = item.dataset.value + (item.dataset.suffix || ''));
    if (!window.gsap || !window.ScrollTrigger) return;
    gsap.registerPlugin(ScrollTrigger);
    gsap.matchMedia().add('(prefers-reduced-motion: no-preference)', () => {
        page.querySelectorAll('.head-area .en').forEach(title => {
            gsap.fromTo(title, { yPercent: 100, rotation: 10 }, { yPercent: 0, rotation: 0, duration: 1, ease: 'power2.out', scrollTrigger: { trigger: title.parentElement, start: 'top 90%', toggleActions: 'play none none reverse' } });
        });
        numbers.forEach(item => {
            const value = { number: 0 };
            gsap.to(value, { number: Number(item.dataset.value), duration: 1.8, ease: 'power2.out', onUpdate: () => item.textContent = Math.round(value.number) + (item.dataset.suffix || ''), scrollTrigger: { trigger: item, start: 'top 90%', once: true } });
        });
        return () => numbers.forEach(item => item.textContent = item.dataset.value + (item.dataset.suffix || ''));
    });
    document.fonts.ready.then(() => ScrollTrigger.refresh());
})();
