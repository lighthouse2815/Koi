(() => {
    'use strict';
    const page = document.querySelector('.content-page');
    if (!page) return;
    const reducedMotion = matchMedia('(prefers-reduced-motion: reduce)');
    const activate = (element, callback) => {
        element.setAttribute('role', 'button');
        element.tabIndex = 0;
        element.addEventListener('click', callback);
        element.addEventListener('keydown', event => {
            if (event.key === 'Enter' || event.key === ' ') {
                event.preventDefault();
                element.click();
            }
        });
    };

    const about = page.querySelector('.aboutWrap');
    if (about) {
        about.querySelectorAll('.text-area-outter .scroll').forEach(button => {
            button.setAttribute('aria-label', 'Explore the KOI story');
            activate(button, () => about.querySelector('.number-area').scrollIntoView({ behavior: reducedMotion.matches ? 'instant' : 'smooth' }));
        });
        const ingredients = [...about.querySelectorAll('[data-ing]')];
        const triangles = [...about.querySelectorAll('[data-tri]')];
        const backgrounds = [...about.querySelectorAll('.about-ingredient .bg > li')];
        const selectIngredient = index => {
            ingredients.forEach((item, i) => item.classList.toggle('slick-current', i === index));
            triangles.forEach((item, i) => {
                item.classList.toggle('is-show', i === index);
                item.setAttribute('aria-pressed', String(i === index));
            });
            backgrounds.forEach((item, i) => item.hidden = i !== index);
        };
        ingredients.forEach((item, index) => { item.classList.add('slick-slide'); activate(item, () => selectIngredient(index)); });
        triangles.forEach((item, index) => activate(item, () => selectIngredient(index)));
        selectIngredient(0);

        const brands = [...about.querySelectorAll('.brandmap > li')];
        const maps = [...about.querySelectorAll('.brandmapList > li')];
        let brandIndex = 0;
        const selectBrand = index => {
            brandIndex = (index + brands.length) % brands.length;
            brands.forEach((item, i) => {
                item.classList.toggle('current', i === brandIndex);
                item.setAttribute('aria-pressed', String(i === brandIndex));
            });
            maps.forEach((item, i) => item.hidden = i !== brandIndex);
        };
        brands.forEach((item, index) => {
            item.setAttribute('aria-label', ['KOI Thé', 'KOI Thé Express', 'FIFTYLAN', 'KOI Café', 'Signature KOI', 'KOI PLUS'][index]);
            activate(item, () => selectBrand(index));
        });
        ['prev', 'next'].forEach((direction, index) => {
            const button = about.querySelector('.brandmapWrap .' + direction);
            button.setAttribute('aria-label', index ? 'Next brand' : 'Previous brand');
            activate(button, () => selectBrand(brandIndex + (index ? 1 : -1)));
        });
        selectBrand(0);
        const countrySelect = about.querySelector('.country-select');
        activate(countrySelect.querySelector('.current'), () => countrySelect.classList.toggle('open'));

        const history = about.querySelector('.about-event');
        const years = [...history.querySelectorAll('.number > li')];
        let yearIndex = 0;
        const selectYear = index => {
            yearIndex = (index + years.length) % years.length;
            ['.number', '.en', '.pic-area'].forEach(selector => {
                history.querySelectorAll(selector).forEach(list => {
                    [...list.children].forEach((item, i) => item.hidden = i !== yearIndex);
                });
            });
        };
        history.querySelectorAll('.prev, .next').forEach(button => {
            const next = button.classList.contains('next');
            button.setAttribute('aria-label', next ? 'Next milestone' : 'Previous milestone');
            activate(button, () => selectYear(yearIndex + (next ? 1 : -1)));
        });
        selectYear(0);
        const cards = about.querySelector('.about-infoWrap.hide-for-large');
        const pagination = document.createElement('div');
        pagination.className = 'content-pager hide-for-large';
        pagination.setAttribute('aria-label', 'Brand values');
        [...cards.children].forEach((card, index) => {
            const button = document.createElement('button');
            button.type = 'button';
            button.setAttribute('aria-label', 'Brand value ' + (index + 1));
            button.addEventListener('click', () => cards.scrollTo({ left: cards.clientWidth * index, behavior: reducedMotion.matches ? 'instant' : 'smooth' }));
            pagination.append(button);
        });
        cards.after(pagination);
        const updatePagination = () => [...pagination.children].forEach((button, index) => button.setAttribute('aria-pressed', String(index === Math.round(cards.scrollLeft / cards.clientWidth))));
        cards.addEventListener('scroll', updatePagination, { passive: true });
        updatePagination();
    }

    if (!window.gsap || !window.ScrollTrigger) return;
    gsap.registerPlugin(ScrollTrigger);
    const media = gsap.matchMedia();
    media.add({ motion: '(prefers-reduced-motion: no-preference)', desktop: '(min-width: 1025px)' }, context => {
        if (!context.conditions.motion) return;
        page.classList.add('content-motion');
        if (about) {
            gsap.fromTo('.about-animation .text-area', { scale: 10 }, { scale: 1, duration: 1.75, ease: 'power4.out' });
            const number = gsap.timeline({ scrollTrigger: { trigger: '.number-area', start: 'top top', end: '35% top', scrub: 1 } });
            about.querySelectorAll('.number-area .inner-sticky').forEach(group => {
                const items = [...group.querySelectorAll('[class*="hollow"]')];
                items.forEach((item, index) => number.to(item, group.classList.contains('show-for-large')
                    ? { y: [324, 162, -162, -324][index] }
                    : { yPercent: (items.length / 2 - index - (index >= items.length / 2 ? 1 : 0)) * 100 }, 0));
            });
            const drinks = gsap.timeline({ scrollTrigger: { trigger: '.drink-area', start: context.conditions.desktop ? 'top top' : 'top 60%', end: '90% top', scrub: 1 } });
            drinks.to('.drink-area li:nth-child(odd)', { y: 0, yPercent: -110 }, 0)
                .to('.drink-area li:nth-child(even)', { y: 0, yPercent: 110 }, 0);
            gsap.to('.drink-area .bg-change', { opacity: 1, scrollTrigger: { trigger: '.drink-area', start: 'top top', end: '55% top', scrub: true } });
            if (context.conditions.desktop) {
                gsap.to('.about-info-preview .about-infoWrap', { opacity: 1, scrollTrigger: { trigger: '.about-info-preview', start: '10% top', end: '50% top', scrub: 1 } });
                const track = about.querySelector('#horizontalWrap');
                gsap.to(track, {
                    x: () => -(track.scrollWidth - innerWidth), ease: 'none',
                    scrollTrigger: { trigger: track, start: 'top top', end: () => '+=' + (track.scrollWidth - innerWidth), pin: true, scrub: true, invalidateOnRefresh: true,
                        onToggle: trigger => about.querySelector('.about-info-preview').classList.toggle('is-not-show', trigger.isActive || trigger.progress === 1) }
                });
            }
        }
        document.fonts.ready.then(() => ScrollTrigger.refresh());
        return () => page.classList.remove('content-motion');
    });
})();
