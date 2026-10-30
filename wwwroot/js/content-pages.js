(() => {
    'use strict';
    const page = document.querySelector('.content-page');
    if (!page) return;
    const reducedMotion = matchMedia('(prefers-reduced-motion: reduce)');
    page.querySelector('[data-copy-link]')?.addEventListener('click', async () => {
        const status = page.querySelector('.share-status');
        try {
            await navigator.clipboard.writeText(location.href);
            status.textContent = 'Link copied';
        } catch {
            status.textContent = 'Copy the address from your browser to share this article.';
        }
    });
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

    const tea = page.querySelector('.our-teaWrap');
    let selectCategory;
    if (tea) {
        const newDrinks = [...tea.querySelectorAll('.new-drink > li')];
        const newPictures = [...tea.querySelectorAll('.new-drinkWrap .pic > li')];
        const newBackgrounds = ['maintea_1_7241_527.jpg', 'maintea_1_7015_9849.jpg', 'maintea_879_6786_5695.jpg', 'maintea_1_1747_2497.jpg'];
        let newIndex = 0;
        let paused = false;
        const selectNew = index => {
            newIndex = index;
            newDrinks.forEach((item, i) => { item.classList.toggle('current', i === index); item.setAttribute('aria-pressed', String(i === index)); });
            newPictures.forEach((item, i) => item.hidden = i !== index);
            tea.querySelector('.new-drinkWrap').style.backgroundImage = 'url(/images/pages/upload_image/maintea/' + newBackgrounds[index] + ')';
        };
        newDrinks.forEach((item, index) => activate(item, () => selectNew(index)));
        selectNew(0);
        const pause = document.createElement('button');
        pause.type = 'button';
        pause.className = 'tea-pause';
        pause.textContent = '( Pause motion )';
        pause.setAttribute('aria-pressed', 'false');
        pause.addEventListener('click', () => {
            paused = !paused;
            tea.classList.toggle('tea-paused', paused);
            pause.textContent = paused ? '( Play motion )' : '( Pause motion )';
            pause.setAttribute('aria-pressed', String(paused));
        });
        tea.querySelector('.new-drinkWrap').append(pause);
        setInterval(() => {
            if (!paused && !reducedMotion.matches && !document.hidden) selectNew((newIndex + 1) % newDrinks.length);
        }, 5000);
        const sections = [...tea.querySelectorAll('.drinkWrapList > li')];
        const pictures = [...tea.querySelectorAll('.drinkList-pic > ul')];
        const menuItems = [...tea.querySelectorAll('.drink-fixedList > li')];
        selectCategory = (index, drink = 0) => {
            sections.forEach((item, i) => item.classList.toggle('current', i === index));
            menuItems.forEach((item, i) => { item.classList.toggle('current', i === index); item.style.opacity = String(1 / (1 + Math.abs(i - index) * 2)); });
            pictures.forEach((list, i) => {
                list.classList.toggle('is-show', i === index);
                [...list.children].forEach((item, j) => item.classList.toggle('is-show', j === drink));
            });
        };
        sections.forEach((section, index) => {
            section.querySelectorAll('.drinkList > li').forEach(item => {
                const select = () => selectCategory(index, Number(item.dataset.drink));
                activate(item, select);
                item.addEventListener('pointerenter', select);
                item.addEventListener('focus', select);
            });
        });
        const scrollCategory = index => {
            selectCategory(index);
            sections[index].scrollIntoView({ behavior: reducedMotion.matches ? 'instant' : 'smooth', block: 'start' });
        };
        menuItems.forEach((item, index) => activate(item, () => scrollCategory(index)));
        const select = tea.querySelector('.menu-select');
        activate(select.querySelector('.current'), () => select.classList.toggle('open'));
        select.querySelectorAll('[data-menu]').forEach(item => activate(item, () => {
            select.querySelector('.current').textContent = item.textContent.trim();
            select.classList.remove('open');
            scrollCategory(Number(item.dataset.menu));
        }));
        selectCategory(0);
    }

    const impact = page.querySelector('.crmWrap');
    if (impact) {
        activate(impact.querySelector('.scroll-down'), () => impact.querySelector('.crmListWrap').scrollIntoView({ behavior: reducedMotion.matches ? 'instant' : 'smooth' }));
        impact.querySelectorAll('.pin-pagination .dot').forEach((dot, index) => {
            dot.setAttribute('aria-label', 'Commitment ' + (index + 1));
            activate(dot, () => {
                const trigger = window.ScrollTrigger?.getById('impact-commitments');
                if (trigger) gsap.to(window, { scrollTo: { y: trigger.start + (trigger.end - trigger.start) * index / 3, autoKill: true }, duration: .92, ease: 'power4.inOut' });
            });
        });
    }
    if (!window.gsap || !window.ScrollTrigger) return;
    gsap.registerPlugin(ScrollTrigger);
    const media = gsap.matchMedia();
    media.add({ motion: '(prefers-reduced-motion: no-preference)', desktop: '(min-width: 1025px)' }, context => {
        if (!context.conditions.motion) return;
        let cleanUpPage = () => {};
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
        if (tea) {
            gsap.fromTo('.our-teaWrap > .head-area > .inner .en', { yPercent: 100, rotation: 10 }, { yPercent: 0, rotation: 0, duration: 1, ease: 'power2.out' });
            tea.querySelectorAll('.drinkWrapList > li').forEach((section, index) => {
                ScrollTrigger.create({ trigger: section, start: 'top 70%', end: 'bottom 70%', onEnter: () => selectCategory(index), onEnterBack: () => selectCategory(index) });
            });
            ScrollTrigger.create({ trigger: '.drinkWrap', start: 'top bottom', end: 'bottom top', toggleClass: 'catalog-active' });
            if (context.conditions.desktop) {
                ScrollTrigger.create({ trigger: '.menu-drinkList', endTrigger: '.drinkWrap', start: 'top 10%', end: 'bottom 90%', pin: true, pinSpacing: false });
                ScrollTrigger.create({ trigger: '.drinkList-pic', endTrigger: '.drinkWrap', start: 'top 39%', end: 'bottom 100%', pin: true, pinSpacing: false });
            }
        }
        if (page.querySelector('.recruitWrap')) {
            gsap.fromTo('.recruit-top .head-area .en', { yPercent: 105, rotation: 10 }, { yPercent: 0, rotation: 0, duration: 1, stagger: .18, ease: 'power2.out' });
            gsap.fromTo('.apply-area .head-area .en', { yPercent: 100, rotation: 10 }, {
                yPercent: 0, rotation: 0, duration: 1, stagger: .25, ease: 'power2.out',
                scrollTrigger: { trigger: '.apply-area', start: '5% 25%', toggleActions: 'play none none reverse' }
            });
            const apply = gsap.timeline({ scrollTrigger: { trigger: '.apply-area', start: '15% 25%', end: '40% 25%', scrub: 1 } });
            apply.fromTo('.apply-fancy .fancy-inner', { opacity: 0, y: 30 }, { opacity: 1, y: 0 })
                .fromTo('.apply-fancy .top-area', { opacity: 0, y: 30 }, { opacity: 1, y: 0 }, .2)
                .fromTo('.apply-fancy .applyList > li', { opacity: 0, y: 30 }, { opacity: 1, y: 0, stagger: .25 }, .4);
        }
        if (page.querySelector('.newsWrap')) {
            gsap.fromTo('.newsWrap .head-area .en', { yPercent: 100, rotation: 10 }, { yPercent: 0, rotation: 0, duration: 1, ease: 'power2.out' });
        }
        if (page.querySelector('.crmWrap')) {
            const hero = page.querySelector('#crm-banner');
            const header = document.querySelector('.site-header');
            header.classList.add('header-light');
            gsap.from('.impact-list-hero__title', { y: 40, opacity: 0, duration: 1, ease: 'power2.out' });
            gsap.timeline({ scrollTrigger: { id: 'impact-banner', trigger: hero, start: 'top top', end: () => '+=' + innerHeight * (context.conditions.desktop ? 2.5 : 3.6), pin: true, scrub: 1, invalidateOnRefresh: true, onUpdate: self => header.classList.toggle('header-light', self.progress < 1) } })
                .to('.crmWrap .center-content, .crmWrap .scroll-down, .crmWrap .side-text', { opacity: 0, duration: 1 }, 0)
                .to('.crmWrap .banner-mask', { opacity: .5, duration: 1 }, 0)
                .fromTo('.crmWrap .slide-text', { x: () => innerWidth }, { x: () => -(page.querySelector('.slide-text').scrollWidth + innerWidth), duration: 2.4, ease: 'none' });
            gsap.from('.crmListWrap .section-title .en', { y: 50, opacity: 0, duration: 1, scrollTrigger: { trigger: '.crmListWrap', start: 'top 75%', toggleActions: 'play none none reverse' } });
            if (context.conditions.desktop) {
                gsap.registerPlugin(ScrollToPlugin);
                const section = page.querySelector('.scroll-pin-section');
                const texts = [...section.querySelectorAll('.text-item')];
                const images = [...section.querySelectorAll('.image-item')];
                const dots = [...section.querySelectorAll('.pin-pagination .dot')];
                let current = -1;
                const select = index => {
                    if (current === index) return;
                    const direction = index > current ? 1 : -1;
                    current = index;
                    texts.forEach((item, i) => {
                        item.classList.toggle('active', i === index);
                        item.inert = i !== index;
                        gsap.to(item, { autoAlpha: i === index ? 1 : 0, y: i === index ? 0 : -22 * direction, duration: .6, overwrite: true });
                    });
                    images.forEach((item, i) => item.classList.toggle('is-active', i === index));
                    dots.forEach((item, i) => { item.classList.toggle('active', i === index); item.setAttribute('aria-pressed', String(i === index)); });
                    gsap.to('.crmWrap .image-wrapper', { y: -images[index].offsetTop, duration: .92, ease: 'power4.inOut', overwrite: true });
                };
                gsap.set(texts.slice(1), { autoAlpha: 0 });
                select(0);
                ScrollTrigger.create({ id: 'impact-commitments', trigger: section, start: 'top top', end: () => '+=' + innerHeight * 3, pin: true, invalidateOnRefresh: true,
                    snap: { snapTo: 1 / 3, duration: .92, delay: .15, ease: 'power4.inOut', directional: false, inertia: false }, onUpdate: self => select(Math.round(self.progress * 3)) });
                cleanUpPage = () => {
                    header.classList.remove('header-light');
                    texts.forEach(item => item.inert = false);
                };
            } else {
                cleanUpPage = () => header.classList.remove('header-light');
            }
        }
        document.fonts.ready.then(() => ScrollTrigger.refresh());
        return () => {
            page.classList.remove('content-motion');
            cleanUpPage();
        };
    });
})();
