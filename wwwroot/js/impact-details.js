(() => {
    'use strict';
    const page = document.querySelector('.impact-detail');
    if (!page) return;
    const reducedMotion = matchMedia('(prefers-reduced-motion: reduce)');
    const responsibility = page.querySelector('.responsibilityWrap');
    const precision = page.querySelector('.precisionWrap');
    const people = page.querySelector('.peopleWrap');
    if ((responsibility || precision || people) && !location.hash) {
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
        const videoName = responsibility ? 'responsibility' : precision ? 'precision' : null;
        const playingClass = 'is-' + videoName + '-video-playing';
        const scrollProperty = '--' + videoName + '-scroll-y';
        let updateVideoLayout;
        let unlockVideoScroll;
        if (videoName) {
            const mobile = matchMedia('(max-width: 1024px)');
            let lockedScroll;
            let lockedPin;
            unlockVideoScroll = () => {
                if (lockedScroll === undefined) return;
                document.body.classList.remove(playingClass);
                document.body.style.removeProperty(scrollProperty);
                scrollTo({ top: lockedScroll, behavior: 'instant' });
                lockedPin?.enable(false);
                lockedPin = undefined;
                lockedScroll = undefined;
            };
            updateVideoLayout = () => {
                if (!mobile.matches) return unlockVideoScroll();
                if (!section.classList.contains('is-playing') || lockedScroll !== undefined) return;
                lockedPin = precision ? window.ScrollTrigger?.getById('precision-banner') : undefined;
                lockedPin?.disable(false);
                const rect = frame.getBoundingClientRect();
                scrollTo({ top: scrollY + rect.top - (innerHeight - rect.height) / 2, behavior: 'instant' });
                lockedScroll = scrollY;
                document.body.style.setProperty(scrollProperty, -lockedScroll + 'px');
                document.body.classList.add(playingClass);
            };
            mobile.addEventListener('change', updateVideoLayout);
            reducedMotion.addEventListener('change', () => {
                if (document.body.classList.contains(playingClass)) back.click();
            });
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
            if (videoName && document.body.classList.contains(playingClass)) back.focus({ preventScroll: true });
        });
        back?.addEventListener('click', () => {
            iframe.src = iframe.dataset.src || iframe.src.replace('autoplay=1', 'autoplay=0');
            frame.classList.remove('is-playing');
            section.classList.remove('is-playing');
            button.hidden = false;
            unlockVideoScroll?.();
            if (videoName) button.focus({ preventScroll: true });
        });
    });

    for (const prefix of ['people-features', 'connections-share']) {
        const section = page.querySelector('.' + prefix);
        if (!section) continue;
        if (prefix === 'people-features' && window.Swiper) continue;
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

    if (people && window.Swiper) {
        const features = people.querySelector('.people-features');
        const viewport = features.querySelector('.people-features__viewport');
        viewport.classList.add('swiper');
        features.querySelector('.people-features__track').classList.add('swiper-wrapper');
        features.querySelectorAll('.people-features__card').forEach(card => card.classList.add('swiper-slide'));
        const dots = [...features.querySelectorAll('.dot')];
        const featureSlider = new Swiper(viewport, {
            slidesPerView: 'auto', loop: true, speed: reducedMotion.matches ? 0 : 720, grabCursor: true,
            breakpoints: { 0: { spaceBetween: 22 }, 1025: { spaceBetween: 48 } },
            navigation: { prevEl: features.querySelector('.people-features__arrow--prev'), nextEl: features.querySelector('.people-features__arrow--next') },
            on: { slideChange: slider => dots.forEach((dot, index) => {
                dot.classList.toggle('active', index === slider.realIndex);
                dot.setAttribute('aria-pressed', String(index === slider.realIndex));
            }) }
        });
        dots.forEach((dot, index) => dot.addEventListener('click', () => featureSlider.slideToLoop(index)));

        const marquee = people.querySelector('.people-share__marquee');
        const motionButton = people.querySelector('.people-share__motion');
        marquee.classList.add('swiper');
        people.querySelector('.people-share__track').classList.add('swiper-wrapper');
        people.querySelectorAll('.people-share__item').forEach(item => item.classList.add('swiper-slide'));
        let visible = false;
        let paused = false;
        const shareSlider = new Swiper(marquee, {
            slidesPerView: 'auto', loop: true, grabCursor: true, freeMode: { enabled: true, momentum: !reducedMotion.matches },
            breakpoints: { 0: { spaceBetween: 18 }, 1025: { spaceBetween: 25 } },
            autoplay: { delay: 0, disableOnInteraction: false },
            on: { init: slider => slider.params.speed = Math.round((slider.slides[0].offsetWidth + Number(slider.params.spaceBetween)) / .027) }
        });
        const freezeShare = () => {
            const position = shareSlider.getTranslate();
            shareSlider.autoplay.stop();
            shareSlider.setTransition(0);
            shareSlider.setTranslate(position);
            shareSlider.updateActiveIndex();
            shareSlider.updateSlidesClasses();
            shareSlider.transitionEnd();
        };
        const syncShare = () => {
            featureSlider.params.speed = reducedMotion.matches ? 0 : 720;
            if (reducedMotion.matches && featureSlider.animating) {
                featureSlider.setTransition(0);
                featureSlider.transitionEnd();
            }
            shareSlider.params.freeMode.momentum = !reducedMotion.matches;
            if (visible && !paused && !reducedMotion.matches && !document.hidden) {
                if (!shareSlider.autoplay.running) shareSlider.autoplay.start();
            } else freezeShare();
            motionButton.textContent = paused ? '( Play motion )' : '( Pause motion )';
            motionButton.disabled = reducedMotion.matches;
        };
        motionButton.addEventListener('click', () => {
            paused = !paused;
            motionButton.setAttribute('aria-pressed', String(paused));
            syncShare();
        });
        new IntersectionObserver(entries => { visible = entries[0].isIntersecting; syncShare(); }).observe(marquee);
        shareSlider.on('resize', () => {
            freezeShare();
            shareSlider.params.speed = Math.round((shareSlider.slides[0].offsetWidth + Number(shareSlider.params.spaceBetween)) / .027);
            syncShare();
        });
        document.addEventListener('visibilitychange', syncShare);
        reducedMotion.addEventListener('change', syncShare);
        syncShare();
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
    const revealSourceImages = selector => page.querySelectorAll(selector).forEach(image => {
        gsap.fromTo(image, { y: 46, opacity: 0 }, { y: 0, opacity: 1, duration: 1.05, ease: 'power2.out', scrollTrigger: { trigger: image, start: 'top 82%', once: true } });
    });
    if (precision) gsap.matchMedia().add('(prefers-reduced-motion: no-preference)', () => {
        page.classList.add('content-motion');
        gsap.fromTo('.precision-hero__label', { y: 42, opacity: 0 }, { y: 0, opacity: 1, duration: 1, delay: .35, ease: 'power3.out' });
        gsap.fromTo('.precision-hero .head-area .en', { yPercent: 100, y: 0, rotation: 10 }, { yPercent: 0, y: 0, rotation: 0, duration: 1, delay: 1, stagger: .12, ease: 'power2.out' });
        gsap.fromTo('.precision-hero .scroll-down', { y: 20, opacity: 0 }, { y: 0, opacity: 1, duration: 1, delay: 1.65, ease: 'power3.out' });
        gsap.timeline({ scrollTrigger: { id: 'precision-banner', trigger: '.precision-hero', start: 'top top', end: '+=2600', pin: true, scrub: 1, anticipatePin: 1, invalidateOnRefresh: true } })
            .to('.precision-hero .scroll-down', { opacity: 0, y: 24, duration: .15, ease: 'none' }, 0)
            .to('.precision-hero__shade', { opacity: 1, duration: .28, ease: 'none' }, .02)
            .fromTo('.precision-hero__scroll-text', { y: () => innerHeight * .68 }, { y: () => innerHeight * (innerWidth <= 1024 ? -.3 : -1.15), duration: .75, ease: 'none' }, .25)
            .to('.precision-hero__content', { opacity: () => innerWidth <= 1024 ? 0 : 1, y: () => innerWidth <= 1024 ? -40 : 0, duration: .22, ease: 'none' }, .04);
        gsap.fromTo('.precision-video__title .en', { yPercent: 100, y: 0, rotation: 10 }, { yPercent: 0, y: 0, rotation: 0, duration: 1, ease: 'power2.out', scrollTrigger: { trigger: '.precision-video__title', start: 'top 80%', end: 'bottom 80%', toggleActions: 'play resume resume resume' } });
        revealSourceImages('.precision-source__image');
        return () => page.classList.remove('content-motion');
    });
    else if (people) gsap.matchMedia().add('(prefers-reduced-motion: no-preference)', () => {
        page.classList.add('content-motion');
        gsap.fromTo('.people-hero .head-area .en', { yPercent: 100, y: 0, rotation: 10 }, { yPercent: 0, y: 0, rotation: 0, duration: 1, delay: 1, stagger: .12, ease: 'power2.out' });
        gsap.fromTo('.people-hero .scroll-down', { y: 20, opacity: 0 }, { y: 0, opacity: 1, duration: 1, delay: 1.45, ease: 'power3.out' });
        gsap.fromTo('.people-belief__title .en', { yPercent: 100, y: 0, rotation: 10 }, { yPercent: 0, y: 0, rotation: 0, duration: 1, stagger: .1, ease: 'power2.out', scrollTrigger: { trigger: '.people-belief', start: 'top 70%', once: true } });
        for (const name of ['features', 'share']) {
            gsap.fromTo('.people-' + name + '__title .en', { yPercent: 100, y: 0, rotation: 10 }, { yPercent: 0, y: 0, rotation: 0, duration: 1, ease: 'power2.out', scrollTrigger: { trigger: '.people-' + name + '__title', start: 'top 72%', once: true } });
        }
        gsap.fromTo('.people-share__cta-title .en', { yPercent: 100, y: 0, rotation: 10 }, { yPercent: 0, y: 0, rotation: 0, duration: 1, stagger: .12, ease: 'power2.out', scrollTrigger: { trigger: '.people-share__cta', start: 'top 76%', once: true } });
        revealSourceImages('.people-source__image');
        return () => page.classList.remove('content-motion');
    });
    else gsap.matchMedia().add('(prefers-reduced-motion: no-preference)', () => {
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
            revealSourceImages('.responsibility-source__image');
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
