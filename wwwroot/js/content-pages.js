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
        const motionButton = about.querySelector('.about-motion-pause');
        const autoplay = new Map();
        let paused = false;
        const syncAutoplay = () => autoplay.forEach((visible, slider) => {
            if (visible && !paused && !reducedMotion.matches && !document.hidden) slider.autoplay?.start();
            else slider.autoplay?.stop();
        });
        const observer = new IntersectionObserver(entries => {
            entries.forEach(entry => autoplay.forEach((visible, slider) => {
                if (slider.el.closest('section, .about-ingredient, .about-event') === entry.target) autoplay.set(slider, entry.isIntersecting);
            }));
            syncAutoplay();
        });
        const watchAutoplay = slider => {
            if (!slider.params.autoplay?.enabled) return;
            autoplay.set(slider, false);
            slider.autoplay?.stop();
            observer.observe(slider.el.closest('.about-ingredient, .about-event'));
        };
        motionButton.addEventListener('click', () => {
            paused = !paused;
            motionButton.setAttribute('aria-pressed', String(paused));
            motionButton.textContent = paused ? '( Play motion )' : '( Pause motion )';
            syncAutoplay();
        });
        reducedMotion.addEventListener('change', syncAutoplay);
        document.addEventListener('visibilitychange', syncAutoplay);
        let copySlider;
        let syncingCopy = false;
        const showIngredient = index => {
            ingredients.forEach((item, i) => item.classList.toggle('slick-current', i === index));
            triangles.forEach((item, i) => {
                item.classList.toggle('is-show', i === index);
                item.setAttribute('aria-pressed', String(i === index));
            });
            backgrounds.forEach((item, i) => item.setAttribute('aria-hidden', String(i !== index)));
            syncingCopy = true;
            copySlider?.slideTo(index, reducedMotion.matches ? 0 : 1000);
            syncingCopy = false;
        };
        const backgroundSlider = new Swiper(about.querySelector('.ingredient-background'), {
            speed: 1000, effect: 'fade', fadeEffect: { crossFade: true }, allowTouchMove: false,
            autoplay: { delay: 5000, disableOnInteraction: false },
            pagination: { el: about.querySelector('.ingredient-pagination'), clickable: true },
            on: {
                slideChange: slider => showIngredient(slider.activeIndex),
                reachEnd: slider => { slider.params.autoplay.reverseDirection = true; },
                reachBeginning: slider => { slider.params.autoplay.reverseDirection = false; }
            }
        });
        copySlider = new Swiper(about.querySelector('.ingredient-copy'), {
            direction: 'vertical', speed: 1000, slidesPerView: 1, breakpoints: { 640: { slidesPerView: 2.5 } },
            on: { slideChange: slider => { if (!syncingCopy) backgroundSlider.slideTo(slider.activeIndex); } }
        });
        const selectIngredient = index => {
            backgroundSlider.slideTo(index, reducedMotion.matches ? 0 : 1000);
            showIngredient(index);
        };
        ingredients.forEach((item, index) => activate(item, () => selectIngredient(index)));
        triangles.forEach((item, index) => activate(item, () => selectIngredient(index)));
        showIngredient(0);
        watchAutoplay(backgroundSlider);

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
        const yearSlider = new Swiper(history.querySelector('.history-year'), { effect: 'fade', fadeEffect: { crossFade: true }, speed: 700, autoHeight: true, allowTouchMove: false });
        const descriptionSlider = new Swiper(history.querySelector('.history-description'), { effect: 'fade', fadeEffect: { crossFade: true }, speed: 700, allowTouchMove: false });
        let historySlider;
        const desktop = matchMedia('(min-width: 1025px)');
        const smallScreen = matchMedia('(max-width: 639px)');
        const mountHistory = () => {
            if (historySlider) {
                autoplay.delete(historySlider);
                historySlider.destroy(true, true);
            }
            const element = history.querySelector(desktop.matches ? '.pic-area.show-for-large' : '.pic-area.hide-for-large');
            element.dir = desktop.matches ? 'rtl' : 'ltr';
            const update = slider => {
                const index = slider.realIndex;
                const count = slider.slides.length;
                slider.slides.forEach(slide => {
                    const number = Number(slide.dataset.swiperSlideIndex);
                    const distance = (number - index + count) % count;
                    slide.classList.toggle('is-selected', distance === 0);
                    slide.classList.toggle('is-show', distance < 4);
                    slide.style.zIndex = String(count - distance);
                });
                yearSlider.slideTo(index % 9, reducedMotion.matches ? 0 : 700);
                descriptionSlider.slideTo(index % 9, reducedMotion.matches ? 0 : 700);
            };
            historySlider = new Swiper(element, {
                slidesPerView: 'auto', centeredSlides: true, loop: true,
                spaceBetween: desktop.matches ? -225 : innerWidth < 640 ? -200 : -300,
                speed: 900, allowTouchMove: !desktop.matches,
                autoplay: desktop.matches ? false : { delay: 3000, disableOnInteraction: false },
                on: { init: update, realIndexChange: update }
            });
            watchAutoplay(historySlider);
        };
        mountHistory();
        desktop.addEventListener('change', mountHistory);
        smallScreen.addEventListener('change', mountHistory);
        history.querySelectorAll('.prev, .next').forEach(button => {
            const next = button.classList.contains('next');
            button.setAttribute('aria-label', next ? 'Next milestone' : 'Previous milestone');
            activate(button, () => next ? historySlider.slideNext(reducedMotion.matches ? 0 : 900) : historySlider.slidePrev(reducedMotion.matches ? 0 : 900));
        });
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
            const number = gsap.timeline({ scrollTrigger: { trigger: '.number-area', start: 'top top', end: context.conditions.desktop ? '20% top' : '35% top', scrub: 1 } });
            about.querySelectorAll('.number-area .inner-sticky').forEach(group => {
                const items = [...group.querySelectorAll('[class*="hollow"]')];
                items.forEach((item, index) => {
                    const line = Number(item.className.match(/hollow-(\d+)/)?.[1]);
                    number.to(item, group.classList.contains('show-for-large')
                        ? { y: [324, 162, -162, -324][index] }
                        : { yPercent: (line <= 5 ? 6 - line : 5 - line) * 100 }, 0);
                });
            });
            if (!context.conditions.desktop) number.to('.number-area .hollow', { opacity: 0 });
            const drinks = gsap.timeline({ scrollTrigger: { trigger: '.drink-area', start: context.conditions.desktop ? 'top top' : 'top 60%', end: context.conditions.desktop ? '90% top' : '90% 60%', scrub: 1, invalidateOnRefresh: true } });
            if (context.conditions.desktop) {
                drinks.to('.drink-area li:nth-child(odd)', { y: 0, yPercent: -110 }, 0)
                    .to('.drink-area li:nth-child(even)', { y: 0, yPercent: 110 }, 0);
            } else {
                drinks.to('.drink-area li:nth-child(odd)', { y: () => -innerHeight * (innerWidth > 640 ? 1.5 : 1.1) }, 0)
                    .to('.drink-area li:nth-child(even)', { y: () => innerHeight * (innerWidth > 640 ? 1.5 : 1.1) }, 0);
            }
            if (context.conditions.desktop) gsap.to('.drink-area .bg-change', { opacity: 1, scrollTrigger: { trigger: '.drink-area', start: 'top top', end: '55% top', scrub: true } });
            if (context.conditions.desktop) {
                gsap.to('.about-info-preview .about-infoWrap', { opacity: 1, scrollTrigger: { trigger: '.about-info-preview', start: '10% top', end: '50% top', scrub: 1 } });
                const track = about.querySelector('#horizontalWrap');
                const pictureElements = [...track.querySelectorAll('.pic-area')];
                gsap.set(pictureElements, { rotation: 0 });
                const pictures = pictureElements.map(picture => gsap.quickSetter(picture, 'rotation', 'deg'));
                const rotatePictures = () => {
                    const x = Number(gsap.getProperty(track, 'x'));
                    [...track.children].forEach((card, index) => {
                        const left = card.offsetLeft + x;
                        if (left < innerWidth / 2) pictures[index](left / 20);
                    });
                };
                gsap.to(track, {
                    x: () => -(track.scrollWidth - innerWidth), ease: 'none',
                    scrollTrigger: { trigger: track, start: 'top top', end: () => '+=' + (track.scrollWidth - innerWidth), pin: true, scrub: true, invalidateOnRefresh: true,
                        onUpdate: rotatePictures,
                        onToggle: trigger => about.querySelector('.about-info-preview').classList.toggle('is-not-show', trigger.isActive || trigger.progress === 1) }
                });
            }
            gsap.fromTo('.about-brandmap .title .overflow .en', { yPercent: 100, rotation: 10 }, {
                yPercent: 0, rotation: 0, duration: 1, ease: 'power2.out',
                scrollTrigger: { trigger: '.about-brandmap', start: 'top 80%', toggleActions: 'play none play reverse' }
            });
            if (context.conditions.desktop) {
                const header = document.querySelector('.site-header');
                ScrollTrigger.create({ trigger: '.about-event', start: 'top 1px', end: 'bottom top', onToggle: self => header.classList.toggle('header-light', self.isActive) });
                cleanUpPage = () => header.classList.remove('header-light');
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
