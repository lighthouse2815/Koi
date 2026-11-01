(() => {
    'use strict';
    const page = document.querySelector('.content-page');
    if (!page) return;
    const reducedMotion = matchMedia('(prefers-reduced-motion: reduce)');
    page.querySelector('[data-copy-link]')?.addEventListener('click', async () => {
        const status = page.querySelector('.share-status');
        try {
            await navigator.clipboard.writeText(location.href);
            status.textContent = window.koiText('Link copied');
        } catch {
            status.textContent = window.koiText('Copy the address from your browser to share this article.');
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
            button.setAttribute('aria-label', window.koiText('Explore the KOI story'));
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
            motionButton.textContent = paused ? window.koiText('( Play motion )') : window.koiText('( Pause motion )');
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

        const history = about.querySelector('.about-event');
        const yearSlider = new Swiper(history.querySelector('.history-year'), { effect: 'fade', fadeEffect: { crossFade: true }, speed: 700, autoHeight: true, allowTouchMove: false });
        const descriptionSlider = new Swiper(history.querySelector('.history-description'), { effect: 'fade', fadeEffect: { crossFade: true }, speed: 700, autoHeight: true, allowTouchMove: false });
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
            button.setAttribute('aria-label', next ? window.koiText('Next milestone') : window.koiText('Previous milestone'));
            activate(button, () => next ? historySlider.slideNext(reducedMotion.matches ? 0 : 900) : historySlider.slidePrev(reducedMotion.matches ? 0 : 900));
        });
    }

    const tea = page.querySelector('.our-teaWrap');
    if (tea) {
        if (window.gsap && window.ScrollToPlugin) gsap.registerPlugin(ScrollToPlugin);
        const newDrinks = [...tea.querySelectorAll('.new-drink > li')];
        const mobile = matchMedia('(max-width: 1024px)');
        let paused = false;
        let copySlider;
        const sliders = new Map();
        const syncMotion = () => sliders.forEach((visible, slider) => {
            if (visible && !paused && !reducedMotion.matches && !document.hidden) slider.autoplay.start();
            else slider.autoplay.stop();
        });
        const observer = new IntersectionObserver(entries => {
            entries.forEach(entry => sliders.forEach((visible, slider) => {
                if (slider.el.closest('.drinkListWrap, .new-drinkWrap') === entry.target) {
                    sliders.set(slider, entry.isIntersecting);
                    entry.target.classList.toggle('tea-motion-off', !entry.isIntersecting);
                }
            }));
            syncMotion();
        });
        const watchMotion = slider => {
            sliders.set(slider, false);
            slider.autoplay.stop();
            observer.observe(slider.el.closest('.drinkListWrap, .new-drinkWrap'));
        };
        const heroBackground = new Swiper(tea.querySelector('.tea-hero-background'), {
            effect: 'fade', fadeEffect: { crossFade: true }, speed: 500, allowTouchMove: false, rewind: true,
            autoplay: { delay: 3000, disableOnInteraction: false }
        });
        watchMotion(heroBackground);
        reducedMotion.addEventListener('change', syncMotion);
        document.addEventListener('visibilitychange', syncMotion);
        if (newDrinks.length) {
            const newBackground = new Swiper(tea.querySelector('.tea-new-background'), {
                effect: 'fade', fadeEffect: { crossFade: true }, speed: 1000, allowTouchMove: false
            });
            const showNew = index => {
                newDrinks.forEach((item, i) => {
                    item.classList.toggle('current', i === index);
                    item.setAttribute('aria-pressed', String(i === index));
                    item.setAttribute('aria-hidden', String(mobile.matches && i !== index));
                    item.tabIndex = mobile.matches && i !== index ? -1 : 0;
                });
                newBackground.slideTo(index, reducedMotion.matches ? 0 : 1000);
                copySlider?.slideTo(index, reducedMotion.matches ? 0 : 1500);
            };
            const newPictures = new Swiper(tea.querySelector('.tea-new-pictures'), {
                effect: 'fade', fadeEffect: { crossFade: true }, speed: 1000, allowTouchMove: false, rewind: true,
                autoplay: { delay: 3000, disableOnInteraction: false },
                on: { slideChange: slider => showNew(slider.activeIndex) }
            });
            const mountCopy = () => {
                copySlider?.destroy(true, true);
                copySlider = undefined;
                const wrapper = tea.querySelector('.new-drink');
                wrapper.classList.toggle('swiper-wrapper', mobile.matches);
                newDrinks.forEach(item => item.classList.toggle('swiper-slide', mobile.matches));
                if (mobile.matches) copySlider = new Swiper(tea.querySelector('.tea-new-copy'), {
                    initialSlide: newPictures.activeIndex, speed: 1500, effect: 'fade', fadeEffect: { crossFade: true },
                    pagination: { el: tea.querySelector('.tea-new-pagination'), clickable: true },
                    on: { slideChange: slider => newPictures.slideTo(slider.activeIndex, reducedMotion.matches ? 0 : 1000) }
                });
                showNew(newPictures.activeIndex);
            };
            newDrinks.forEach((item, index) => activate(item, () => newPictures.slideTo(index, reducedMotion.matches ? 0 : 1000)));
            mountCopy();
            mobile.addEventListener('change', mountCopy);
            watchMotion(newPictures);
            const pause = tea.querySelector('.tea-pause');
            pause.addEventListener('click', () => {
                paused = !paused;
                tea.classList.toggle('tea-paused', paused);
                pause.textContent = paused ? window.koiText('( Play motion )') : window.koiText('( Pause motion )');
                pause.setAttribute('aria-pressed', String(paused));
                syncMotion();
            });
        }
        const explore = tea.querySelector('.scroll-down');
        explore.setAttribute('aria-label', window.koiText('Explore new drinks'));
        activate(explore, () => (tea.querySelector('.new-drinkWrap') || tea.querySelector('.tea-catalog')).scrollIntoView({ behavior: reducedMotion.matches ? 'instant' : 'smooth' }));

    }

    const impact = page.querySelector('.crmWrap');
    let navigateImpact;
    let updateImpactHeader;
    if (impact) {
        const header = document.querySelector('.site-header');
        updateImpactHeader = () => header.classList.toggle('header-light', impact.querySelector('.crm-banner').getBoundingClientRect().bottom > header.offsetHeight);
        window.addEventListener('scroll', updateImpactHeader, { passive: true });
        window.addEventListener('resize', updateImpactHeader);
        updateImpactHeader();
        activate(impact.querySelector('.scroll-down'), () => impact.querySelector('.crmListWrap').scrollIntoView({ behavior: reducedMotion.matches ? 'instant' : 'smooth' }));
        impact.querySelectorAll('.pin-pagination .dot').forEach((dot, index) => {
            dot.setAttribute('aria-label', window.koiText('Commitment {0}', index + 1));
            activate(dot, () => navigateImpact?.(index));
        });
    }
    const menus = page.querySelector('.menuListWrap');
    if (menus) {
        const startAtTop = () => {
            if (location.hash) return;
            history.scrollRestoration = 'manual';
            window.ScrollTrigger?.clearScrollMemory('manual');
            scrollTo({ top: 0, behavior: 'instant' });
        };
        startAtTop();
        addEventListener('pageshow', startAtTop);
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
                const exitDistance = (_, element) => Math.max(innerHeight * (innerWidth > 640 ? 1.5 : 1.1), (innerHeight + element.offsetHeight) / 2 + 2);
                drinks.to('.drink-area li:nth-child(odd)', { y: (index, element) => -exitDistance(index, element) }, 0)
                    .to('.drink-area li:nth-child(even)', { y: exitDistance }, 0);
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
            if (context.conditions.desktop) {
                const header = document.querySelector('.site-header');
                ScrollTrigger.create({ trigger: '.about-event', start: 'top 1px', end: 'bottom top', onToggle: self => header.classList.toggle('header-light', self.isActive) });
                cleanUpPage = () => header.classList.remove('header-light');
            }
        }
        if (tea) {
            gsap.fromTo('.our-teaWrap > .head-area > .inner .en', { yPercent: 100, rotation: 10 }, { yPercent: 0, rotation: 0, duration: 1, delay: 1, ease: 'power2.out' });

        }
        if (page.querySelector('.recruitWrap')) {
            const intro = gsap.timeline({ delay: 1, defaults: { duration: 1, ease: 'power2.out' } });
            const introLines = context.conditions.desktop
                ? ['.recruit-top .en.top', '.recruit-top .en.bottom']
                : ['.recruit-top .head-1 .en', '.recruit-top .head-1-2 .en', '.recruit-top .head-2 .en'];
            introLines.forEach((selector, index) => intro.fromTo(selector, { yPercent: context.conditions.desktop ? 100 : 105, y: 0, rotation: 10 }, { yPercent: 0, y: 0, rotation: 0 }, index * .25));
            const applyArea = page.querySelector('.apply-area');
            const fancy = applyArea.querySelector('.apply-fancy');
            const title = gsap.timeline({ paused: true, defaults: { duration: 1, ease: 'power2.out' } })
                .fromTo('.apply-area .en.top', { yPercent: 100, y: 0, rotation: 10 }, { yPercent: 0, y: -25, rotation: 0 })
                .fromTo('.apply-area .en.bottom', { yPercent: 100, y: 0, rotation: 10 }, { yPercent: 0, y: 0, rotation: 0 }, .25);
            ScrollTrigger.create({
                id: 'recruit-apply-active', trigger: applyArea, start: '5% 25%', end: '80% 25%',
                onEnter: () => title.play(),
                onToggle: self => {
                    fancy.classList.toggle('is-show', self.isActive);
                    fancy.inert = !self.isActive;
                }
            });
            fancy.inert = !ScrollTrigger.getById('recruit-apply-active').isActive;
            [
                ['.fancy-inner', 15, 25], ['.top-area', 20, 35],
                ['.applyList > li:first-child', 25, 35], ['.applyList > li:last-child', 30, 40]
            ].forEach(([selector, start, end], index) => {
                gsap.fromTo(fancy.querySelector(selector), { opacity: 0, y: 30 }, {
                    opacity: 1, y: 0,
                    scrollTrigger: { id: 'recruit-apply-stage-' + index, trigger: applyArea, start: start + '% 25%', end: end + '% 25%', scrub: 1 }
                });
            });
            cleanUpPage = () => {
                fancy.classList.remove('is-show');
                fancy.inert = false;
            };
        }
        if (page.querySelector('.newsWrap')) {
            gsap.fromTo('.newsWrap .head-area .en', { yPercent: 100, rotation: 10 }, { yPercent: 0, rotation: 0, duration: 1, delay: 1, ease: 'power2.out' });
        }
        if (menus) {
            const list = menus.querySelector('.menuList');
            list.inert = true;
            gsap.timeline({ delay: 1, onComplete: () => list.inert = false })
                .fromTo(menus.querySelector('.head-area'), { '--menu-rule-width': '0%' },
                    { '--menu-rule-width': '100%', duration: 1, ease: 'power3.out' })
                .fromTo(menus.querySelector('.head-area .en'),
                    { y: 0, yPercent: context.conditions.desktop && innerWidth >= 1200 ? 100 : 120, rotation: 10 },
                    { y: 0, yPercent: 0, rotation: 0, duration: .75, ease: 'power2.out' }, .5)
                .fromTo(list, { y: 20, opacity: 0 }, { y: 0, opacity: 1, duration: 1, ease: 'power2.out' });
            cleanUpPage = () => list.inert = false;
        }
        if (page.querySelector('.crmWrap')) {
            const hero = page.querySelector('#crm-banner');
            const smallScreen = () => innerWidth <= 768;
            gsap.timeline({ delay: 1, defaults: { duration: 1, ease: 'power2.out' } })
                .fromTo('.impact-list-hero__title .en', { yPercent: 100, y: 0, rotation: 10 }, { yPercent: 0, y: 0, rotation: 0 })
                .fromTo('.crmWrap .sub-title', { y: 30, opacity: 0 }, { y: 0, opacity: 1 }, .4)
                .from('.crmWrap .scroll-down, .crmWrap .side-text', { opacity: 0 }, .8);
            gsap.timeline({ scrollTrigger: { id: 'impact-banner', trigger: hero, start: 'top top', end: () => '+=' + innerHeight * (smallScreen() ? 3.6 : 2.5), pin: true, scrub: 1, invalidateOnRefresh: true, onUpdate: updateImpactHeader } })
                .to('.crmWrap .center-content, .crmWrap .scroll-down, .crmWrap .side-text', { opacity: 0, duration: 1 }, 0)
                .to('.crmWrap .banner-mask', { opacity: .5, duration: 1 }, 0)
                .fromTo('.crmWrap .slide-text', { x: () => innerWidth }, { x: () => -(page.querySelector('.slide-text').scrollWidth + innerWidth * (smallScreen() ? 1.8 : 1)), duration: 2.4, ease: 'none' })
                .to({}, { duration: () => smallScreen() ? .45 : 0 });
            gsap.fromTo('.crmListWrap .section-title .en', { y: 50, opacity: 0 }, { y: 0, opacity: 1, duration: 1, ease: 'power2.out', scrollTrigger: { trigger: '.crmListWrap .section-title', start: 'top 80%', toggleActions: 'restart none none none' } });
            if (context.conditions.desktop) {
                gsap.registerPlugin(ScrollToPlugin);
                const section = page.querySelector('.scroll-pin-section');
                const texts = [...section.querySelectorAll('.text-item')];
                const images = [...section.querySelectorAll('.image-item')];
                const dots = [...section.querySelectorAll('.pin-pagination .dot')];
                let current = -1;
                let busy = false;
                let scrollTween;
                let release;
                let pin;
                const imageStep = () => images[1].offsetTop - images[0].offsetTop;
                const wheelDistance = event => event.deltaY * (event.deltaMode === 1 ? 16 : event.deltaMode === 2 ? innerHeight : 1);
                const select = (index, immediate = false) => {
                    if (current === index && !immediate) return;
                    const previous = current;
                    const direction = index > current ? 1 : -1;
                    current = index;
                    gsap.killTweensOf([...texts, section.querySelector('.image-wrapper')]);
                    texts.forEach((item, i) => {
                        item.classList.toggle('active', i === index);
                        item.inert = i !== index;
                        if (immediate || (i !== previous && i !== index)) gsap.set(item, { autoAlpha: 0, y: 22 });
                    });
                    images.forEach((item, i) => item.classList.toggle('is-active', i === index));
                    dots.forEach((item, i) => { item.classList.toggle('active', i === index); item.setAttribute('aria-pressed', String(i === index)); });
                    if (immediate) {
                        gsap.set(texts[index], { autoAlpha: 1, y: 0 });
                        gsap.set('.crmWrap .image-wrapper', { y: -imageStep() * index });
                    } else {
                        gsap.set(texts[index], { autoAlpha: 0, y: 22 * direction });
                        gsap.to(texts[previous], { autoAlpha: 0, y: -22 * direction, duration: .22, ease: 'power2.out' });
                        gsap.to(texts[index], { autoAlpha: 1, y: 0, duration: .72, delay: .2, ease: 'power3.out' });
                        gsap.to('.crmWrap .image-wrapper', { y: -imageStep() * index, duration: .92, ease: 'power4.inOut' });
                    }
                };
                const goTo = (index, immediate = false) => {
                    busy = true;
                    scrollTween?.kill();
                    release?.kill();
                    select(index, immediate);
                    const y = Math.min(pin.end - 1, Math.max(pin.start + 1, pin.start + (pin.end - pin.start) * index / 3));
                    if (immediate) {
                        scrollTo({ top: y, behavior: 'instant' });
                        ScrollTrigger.update();
                        release = gsap.delayedCall(.18, () => busy = false);
                    } else scrollTween = gsap.to(window, { scrollTo: { y, autoKill: false }, duration: .92, ease: 'power4.inOut', onComplete: () => busy = false });
                };
                const step = direction => {
                    if (busy) return;
                    if (current + direction < 0 || current + direction > 3) {
                        observer.disable();
                        busy = true;
                        if (observer.event.type === 'wheel') {
                            scrollTo({ top: scrollY + wheelDistance(observer.event), behavior: 'instant' });
                            ScrollTrigger.update();
                            release = gsap.delayedCall(.18, () => busy = false);
                        } else scrollTween = gsap.to(window, { scrollTo: direction > 0 ? pin.end + 2 : pin.start - 2, duration: .42, ease: 'power2.out', onComplete: () => busy = false });
                    } else goTo(current + direction);
                };
                const observer = ScrollTrigger.observe({ id: 'impact-input', target: window, type: 'wheel,touch', wheelSpeed: -1, tolerance: 14, dragMinimum: 24, preventDefault: true, allowClicks: true, onUp: () => step(1), onDown: () => step(-1) });
                observer.disable();
                gsap.set(texts, { autoAlpha: 0, y: 50 });
                gsap.set('.crmWrap .image-wrapper', { y: 0 });
                select(0, true);
                pin = ScrollTrigger.create({ id: 'impact-commitments', trigger: section, start: 'top top', end: () => '+=' + innerHeight * 3, pin: true, anticipatePin: 1, invalidateOnRefresh: true,
                    onEnter: self => { if (!busy) select(Math.round(self.progress * 3), true); observer.enable(); },
                    onEnterBack: self => { if (!busy) select(Math.round(self.progress * 3), true); observer.enable(); },
                    onLeave: () => observer.disable(), onLeaveBack: () => observer.disable(),
                    onUpdate: self => { if (!busy) select(Math.round(self.progress * 3)); },
                    onRefresh: () => { gsap.killTweensOf(section.querySelector('.image-wrapper')); gsap.set('.crmWrap .image-wrapper', { y: -imageStep() * current }); }
                });
                const enterOnWheel = event => {
                    if (pin.isActive || busy || event.ctrlKey) return;
                    const delta = wheelDistance(event);
                    if (scrollY >= pin.start - 2 && scrollY <= pin.end + 2) {
                        const direction = Math.sign(delta);
                        if (Math.abs(delta) >= 14 && current + direction >= 0 && current + direction <= 3) {
                            event.preventDefault(); observer.enable(); goTo(current + direction);
                        }
                        return;
                    }
                    if (scrollY < pin.start && scrollY + delta >= pin.start) { event.preventDefault(); goTo(0, true); }
                    else if (scrollY > pin.end && scrollY + delta <= pin.end) { event.preventDefault(); goTo(3, true); }
                };
                window.addEventListener('wheel', enterOnWheel, { passive: false, capture: true });
                navigateImpact = index => goTo(index);
                const requested = (new URLSearchParams(location.search).get('impact') || '').toLowerCase();
                const requestedIndex = requested ? texts.findIndex(item => item.querySelector('.more-btn').getAttribute('href').toLowerCase().includes(requested)) : 0;
                if (location.hash === '#scroll-pin-section' && requestedIndex >= 0) release = gsap.delayedCall(.1, () => goTo(requestedIndex, true));
                cleanUpPage = () => {
                    observer.kill();
                    scrollTween?.kill();
                    release?.kill();
                    gsap.killTweensOf([...texts, section.querySelector('.image-wrapper')]);
                    window.removeEventListener('wheel', enterOnWheel, true);
                    navigateImpact = undefined;
                    texts.forEach(item => item.inert = false);
                };
            } else {
                gsap.fromTo('.crmWrap .pin-mobile__heading .en', { yPercent: 100, y: 0, rotation: 10 }, { yPercent: 0, y: 0, rotation: 0, duration: .95, ease: 'power3.out', scrollTrigger: { trigger: '.pin-mobile__heading', start: 'top 80%', once: true } });
                const items = [...impact.querySelectorAll('.pin-mobile__item')];
                const activateItem = item => items.forEach(element => element.classList.toggle('is-active', element === item));
                activateItem(items[0]);
                items.forEach(item => ScrollTrigger.create({ trigger: item, start: 'top 58%', end: 'bottom 42%', onUpdate: self => { if (self.isActive) activateItem(item); } }));
                cleanUpPage = () => items.forEach(item => item.classList.remove('is-active'));
            }
            updateImpactHeader();
        }
        document.fonts.ready.then(() => ScrollTrigger.refresh());
        return () => {
            page.classList.remove('content-motion');
            cleanUpPage();
        };
    });
})();
