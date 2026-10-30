(() => {
    'use strict';
    const page = document.querySelector('.impact-detail');
    if (!page) return;
    const reducedMotion = matchMedia('(prefers-reduced-motion: reduce)');
    const responsibility = page.querySelector('.responsibilityWrap');
    if (responsibility && !location.hash) {
        history.scrollRestoration = 'manual';
        window.ScrollTrigger?.clearScrollMemory('manual');
        const startAtTop = () => {
            if (!location.hash) scrollTo({ top: 0, behavior: 'instant' });
        };
        startAtTop();
        window.addEventListener('pageshow', startAtTop);
    }
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
        const back = section.querySelector('[class$="-video__back"]');
        let updateVideoLayout;
        let unlockVideoScroll;
        if (responsibility) {
            const mobile = matchMedia('(max-width: 1024px)');
            let lockedScroll;
            unlockVideoScroll = () => {
                if (lockedScroll === undefined) return;
                document.body.classList.remove('is-responsibility-video-playing');
                document.body.style.removeProperty('--responsibility-scroll-y');
                scrollTo({ top: lockedScroll, behavior: 'instant' });
                lockedScroll = undefined;
            };
            updateVideoLayout = () => {
                if (!mobile.matches) return unlockVideoScroll();
                if (!section.classList.contains('is-playing') || lockedScroll !== undefined) return;
                const rect = frame.getBoundingClientRect();
                scrollTo({ top: scrollY + rect.top - (innerHeight - rect.height) / 2, behavior: 'instant' });
                lockedScroll = scrollY;
                document.body.style.setProperty('--responsibility-scroll-y', -lockedScroll + 'px');
                document.body.classList.add('is-responsibility-video-playing');
            };
            mobile.addEventListener('change', updateVideoLayout);
        }
        button.addEventListener('click', () => {
            const url = new URL(iframe.dataset.src || iframe.src);
            url.searchParams.set('autoplay', '1');
            url.searchParams.set('mute', '0');
            iframe.src = url.href;
            frame.classList.add('is-playing');
            section.classList.add('is-playing');
            button.hidden = true;
            updateVideoLayout?.();
            if (document.body.classList.contains('is-responsibility-video-playing')) back.focus({ preventScroll: true });
        });
        back?.addEventListener('click', () => {
            iframe.src = iframe.dataset.src || iframe.src.replace('autoplay=1', 'autoplay=0');
            frame.classList.remove('is-playing');
            section.classList.remove('is-playing');
            button.hidden = false;
            unlockVideoScroll?.();
            if (responsibility) button.focus({ preventScroll: true });
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
        if (responsibility) {
            page.classList.add('content-motion');
            gsap.set('.responsibility-hero__content', { opacity: 1 });
            gsap.timeline({ delay: .65, defaults: { ease: 'power2.out' } })
                .fromTo('.responsibility-hero__title .en', { yPercent: 100, y: 0, rotation: 10 }, { yPercent: 0, y: 0, rotation: 0, duration: 1 }, 1.85)
                .fromTo('.responsibility-hero .sub-title', { y: 30, opacity: 0 }, { y: 0, opacity: 1, duration: 1 }, 2.25)
                .fromTo('.responsibility-hero .scroll-down', { y: 20, opacity: 0 }, { y: 0, opacity: 1, duration: 1.05 }, 2.7);
            for (const name of ['belief', 'video', 'numbers']) {
                const title = responsibility.querySelector('.responsibility-' + name + '__title');
                gsap.fromTo(title.querySelectorAll('.en'), { yPercent: 100, y: 0, rotation: 10 }, {
                    yPercent: 0, y: 0, rotation: 0, duration: 1, stagger: name === 'belief' ? .12 : 0, ease: 'power2.out',
                    scrollTrigger: { trigger: title, start: 'top 80%', end: 'bottom 80%', toggleActions: 'play resume resume resume' }
                });
            }
            const count = gsap.timeline({ scrollTrigger: { trigger: '.responsibility-numbers__cards', start: 'top 80%', once: true } });
            numbers.forEach(item => {
                const counter = { value: 0 };
                item.textContent = '0' + (item.dataset.suffix || '');
                count.to(counter, { value: Number(item.dataset.value), duration: 1.8, ease: 'power2.out', onUpdate: () => item.textContent = Math.round(counter.value) + (item.dataset.suffix || '') }, 0);
            });
            responsibility.querySelectorAll('.responsibility-source__image').forEach(image => {
                gsap.fromTo(image, { y: 46, opacity: 0 }, { y: 0, opacity: 1, duration: 1.05, ease: 'power2.out', scrollTrigger: { trigger: image, start: 'top 82%', once: true } });
            });
            return () => {
                page.classList.remove('content-motion');
                numbers.forEach(item => item.textContent = item.dataset.value + (item.dataset.suffix || ''));
            };
        }
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
