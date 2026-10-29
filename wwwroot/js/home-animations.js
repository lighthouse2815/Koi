(() => {
    'use strict';
    if (!window.gsap || !window.ScrollTrigger || !window.ScrollToPlugin) return;
    gsap.registerPlugin(ScrollTrigger, ScrollToPlugin);
    const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
    const heroTrack = document.querySelector('.hero-track');
    const hero = document.querySelector('.hero');
    const lettering = document.querySelector('.hero-lettering');
    const story = document.querySelector('.story');
    const features = document.querySelector('.features');
    const featureIntro = document.querySelector('.feature-intro');
    const featureStage = features.querySelector('.feature-stage');
    const recommendation = document.querySelector('.recommendation');
    const drinkTrack = document.querySelector('.drinks-track');
    const drinkStage = document.querySelector('.drinks-section');
    const carousel = document.querySelector('.drink-carousel');
    const drinkButtons = [...document.querySelectorAll('[data-drink-direction]')];
    const media = gsap.matchMedia();

    const setupScenes = () => media.add({
        desktop: '(min-width: 1025px)',
        mobile: '(max-width: 1024px)',
        reduce: '(prefers-reduced-motion: reduce)'
    }, (context) => {
        if (context.conditions.reduce) return;
        document.body.classList.add('motion-ready');
        const heroTravel = () => Math.max(0, lettering.offsetWidth - innerWidth);
        const drinkTravel = () => Math.max(0, carousel.offsetWidth - innerWidth);
        const marquees = [...document.querySelectorAll('[data-marquee-speed]')];
        const sizeTracks = () => {
            heroTrack.style.height = `${hero.offsetHeight + heroTravel()}px`;
            drinkTrack.style.height = `${drinkStage.offsetHeight + drinkTravel()}px`;
            marquees.forEach(track => track.style.setProperty('--marquee-duration', `${track.scrollWidth / 2 / Number(track.dataset.marqueeSpeed)}s`));
        };
        sizeTracks();
        ScrollTrigger.addEventListener('refreshInit', sizeTracks);
        const marqueeObserver = new IntersectionObserver(entries => {
            entries.forEach(entry => entry.target.classList.toggle('is-marquee-visible', entry.isIntersecting));
        });
        marquees.forEach(track => marqueeObserver.observe(track));

        gsap.to(lettering, {
            x: () => -heroTravel(), ease: 'none',
            scrollTrigger: { id: 'hero-scroll', trigger: heroTrack, start: 'top top', end: 'bottom bottom', scrub: 0.65, invalidateOnRefresh: true }
        });
        gsap.to('.hero-o', {
            rotation: 100, ease: 'none',
            scrollTrigger: { trigger: heroTrack, start: 'top top', end: 'bottom bottom', scrub: 1 }
        });

        // The reference uses 23 photographs in one sprite, followed by a circular wipe.
        gsap.to('.story-sprite', { rotation: 360, duration: 15, repeat: -1, ease: 'none' });
        gsap.to('.story-sprite', {
            backgroundPosition: '0% 100%', ease: 'steps(22)',
            scrollTrigger: { id: 'tea-frames', trigger: story, start: 'top top', end: '50% top', scrub: 1 }
        });
        gsap.to('.story-wipe', {
            scale: context.conditions.desktop ? 6.5 : 5, ease: 'power1.inOut',
            scrollTrigger: { id: 'tea-wipe', trigger: story, start: context.conditions.desktop ? '60% 80%' : '60% center', end: context.conditions.desktop ? '80% 80%' : '80% center', scrub: 1 }
        });

        const up = features.querySelectorAll('.feature-up');
        const down = features.querySelectorAll('.feature-down');
        const featureParts = features.querySelectorAll('.feature-dot, .feature-caption');
        const featureTimeline = gsap.timeline({
            defaults: { ease: 'none' },
            scrollTrigger: {
                id: 'feature-columns', trigger: features, start: 'top top', end: () => `+=${features.offsetHeight}`,
                scrub: context.conditions.desktop ? 0.8 : true, invalidateOnRefresh: true,
                onUpdate: self => {
                    const active = self.scroll() >= self.start;
                    features.classList.toggle('is-active', active);
                    featureIntro.classList.toggle('is-covered', active);
                }
            }
        });
        featureTimeline.set(up, { y: 0 }, 0)
            .set(down, { y: () => -3 * featureStage.clientHeight }, 0);
        if (context.conditions.desktop) {
            featureTimeline.fromTo(featureParts, { scale: 0 }, { scale: 1, duration: 0.05 }, 0.05);
        } else {
            featureTimeline.set(featureParts, { scale: 1 }, 0);
            gsap.fromTo(featureIntro.querySelectorAll('.feature-dot, .feature-caption'), { scale: 0 }, {
                scale: 1, ease: 'power1.out',
                scrollTrigger: { id: 'feature-intro', trigger: featureIntro, start: '10% top', end: '25% top', scrub: 1 }
            });
        }
        for (let step = 1; step <= 3; step++) {
            const at = (context.conditions.desktop ? 0.19 : 0) + (step - 1) * 0.2;
            const duration = context.conditions.desktop ? 0.01 : 0.1;
            featureTimeline.to(up, { y: () => -step * featureStage.clientHeight, duration, ease: 'power1.out' }, at)
                .to(down, { y: () => -(3 - step) * featureStage.clientHeight, duration, ease: 'power1.out' }, at);
        }
        featureTimeline.to(featureParts, { scale: 0, duration: 0.05 }, 0.7)
            .to({}, { duration: 0.25 }, 0.75);

        const art = document.querySelector('.recommendation-art');
        const layers = [...art.querySelectorAll('.recommendation-layer')];
        const burst = gsap.timeline({
            scrollTrigger: { id: 'recommendation-burst', trigger: recommendation, start: '10% center', end: 'bottom center', toggleActions: 'play reverse play reverse' }
        });
        burst.from('.recommendation-label', { scale: 0, opacity: 0, duration: 0.5, ease: 'power2.out' }, 0)
            .from('.recommendation h2', { scale: 0, opacity: 0, duration: 0.6, ease: 'power2.out' }, 0.05)
            .from('.recommendation-captions', { scale: 0, opacity: 0, duration: 0.6 }, 0.15)
            .from(layers.map(layer => layer.firstElementChild), {
                x: (index) => art.clientWidth / 2 - layers[index].offsetLeft - layers[index].offsetWidth / 2,
                y: (index) => art.clientHeight / 2 - layers[index].offsetTop - (layers[index].offsetHeight || layers[index].offsetWidth) / 2,
                scale: 0.15, opacity: 0, stagger: 0.05, duration: 0.75, ease: 'power2.out'
            }, 0.05);
        const parallax = layers.map(layer => ({
            depth: Number(layer.dataset.depth),
            x: gsap.quickTo(layer, 'x', { duration: 0.8, ease: 'power2.out' }),
            y: gsap.quickTo(layer, 'y', { duration: 0.8, ease: 'power2.out' })
        }));
        const moveArt = (event) => {
            if (event.pointerType === 'touch' || !burst.scrollTrigger.isActive) return;
            const bounds = recommendation.getBoundingClientRect();
            const x = (event.clientX / innerWidth - 0.5) * 12;
            const y = ((event.clientY - bounds.top) / innerHeight - 0.5) * 12;
            parallax.forEach(layer => { layer.x(x * layer.depth); layer.y(y * layer.depth); });
        };
        const resetArt = () => parallax.forEach(layer => { layer.x(0); layer.y(0); });
        recommendation.addEventListener('pointermove', moveArt);
        recommendation.addEventListener('pointerleave', resetArt);

        const drinkTween = gsap.to(carousel, {
            x: () => -drinkTravel(), ease: 'none',
            scrollTrigger: {
                id: 'drinks-scroll', trigger: drinkTrack, start: 'top top', end: 'bottom bottom', scrub: 0.65, invalidateOnRefresh: true,
                onUpdate: (self) => {
                    drinkButtons.forEach(button => {
                        button.disabled = Number(button.dataset.drinkDirection) < 0 ? self.progress < 0.002 : self.progress > 0.998;
                    });
                    if (!context.conditions.mobile) return;
                    carousel.querySelectorAll('.drink-photo-circle').forEach(circle => {
                        const left = circle.parentElement.getBoundingClientRect().left;
                        gsap.set(circle, { rotation: left < 150 ? left / 10 - 15 : 0 });
                    });
                }
            }
        });
        const stepDrinks = (event) => {
            const trigger = drinkTween.scrollTrigger;
            const count = carousel.querySelectorAll('.drink-card').length - 1;
            const index = Math.round(trigger.progress * count);
            const target = gsap.utils.clamp(0, count, index + event.detail.direction);
            gsap.to(window, { scrollTo: { y: trigger.start + target / count * (trigger.end - trigger.start), autoKill: true }, duration: 0.9, ease: 'power2.inOut' });
        };
        const keyDrinks = (event) => {
            if (!['ArrowLeft', 'ArrowRight'].includes(event.key)) return;
            event.preventDefault();
            stepDrinks({ detail: { direction: event.key === 'ArrowLeft' ? -1 : 1 } });
        };
        document.addEventListener('koi:drink-step', stepDrinks);
        carousel.addEventListener('keydown', keyDrinks);
        drinkButtons.forEach(button => { button.disabled = Number(button.dataset.drinkDirection) < 0; });

        const hoverCleanups = [];
        document.querySelectorAll('.drink-photo').forEach(photo => {
            if (context.conditions.mobile) return;
            const rotation = gsap.to(photo.querySelector('.drink-photo-circle'), { rotation: 360, duration: 10, repeat: -1, ease: 'none', paused: true });
            const play = (event) => { if (event.pointerType !== 'touch') rotation.play(); };
            const reverse = () => rotation.reverse();
            photo.addEventListener('pointerenter', play);
            photo.addEventListener('pointerleave', reverse);
            hoverCleanups.push(() => { photo.removeEventListener('pointerenter', play); photo.removeEventListener('pointerleave', reverse); });
        });
        gsap.utils.toArray('.heading-word').forEach(word => gsap.from(word, {
            yPercent: 120, rotation: 10, duration: 1, ease: 'power2.out',
            scrollTrigger: { trigger: word.closest('section'), start: 'top 80%', toggleActions: 'play none none reverse' }
        }));
        ScrollTrigger.refresh();
        return () => {
            ScrollTrigger.removeEventListener('refreshInit', sizeTracks);
            marqueeObserver.disconnect();
            marquees.forEach(track => {
                track.style.removeProperty('--marquee-duration');
                track.classList.remove('is-marquee-visible');
            });
            features.classList.remove('is-active');
            featureIntro.classList.remove('is-covered');
            recommendation.removeEventListener('pointermove', moveArt);
            recommendation.removeEventListener('pointerleave', resetArt);
            document.removeEventListener('koi:drink-step', stepDrinks);
            carousel.removeEventListener('keydown', keyDrinks);
            hoverCleanups.forEach(cleanup => cleanup());
            heroTrack.style.removeProperty('height');
            drinkTrack.style.removeProperty('height');
            carousel.scrollLeft = 0;
            document.body.classList.remove('motion-ready');
        };
    });

    const intro = document.querySelector('.intro');
    const introVideo = document.querySelector('.intro-video');
    const introSkip = document.querySelector('.intro-skip');
    let introTimer;
    let introSequence;
    let introFinishing = false;
    const closeIntro = () => {
        clearTimeout(introTimer);
        introSequence?.kill();
        introVideo.pause();
        intro.close();
        intro.hidden = true;
        document.body.classList.remove('is-intro');
        gsap.set([hero, lettering, intro, introSkip, ...document.querySelectorAll('.site-header, .floating-bear')], { clearProps: 'transform,opacity,visibility,pointerEvents' });
        introFinishing = false;
        ScrollTrigger.refresh();
    };
    const finishIntro = (skip = true) => {
        if (intro.hidden || introFinishing) return;
        if (reducedMotion.matches) { closeIntro(); return; }
        introFinishing = true;
        clearTimeout(introTimer);
        introSequence?.kill();
        const start = skip ? 0.75 : 0;
        introSequence = gsap.timeline({ onComplete: closeIntro });
        if (skip) introSequence.to(intro, { autoAlpha: 0, duration: 0.5 }, 0);
        introSequence.to(hero, { yPercent: 0, duration: 1.5, ease: 'power2.out' }, start)
            .to(lettering, { xPercent: 0, duration: 1.5, ease: 'power3.out' }, start + 0.5)
            .to('.site-header', { autoAlpha: 1, duration: 0.5 }, start)
            .to('.floating-bear', { autoAlpha: 1, duration: 0.5 }, start + 2)
            .call(() => { introVideo.pause(); intro.close(); intro.hidden = true; }, [], start + 2.5)
            .to({}, { duration: 1 }, start + 2.5);
    };
    const playIntro = () => {
        if (reducedMotion.matches || !intro.hidden || introFinishing) return;
        window.scrollTo({ top: 0, behavior: 'instant' });
        document.body.classList.add('is-intro');
        gsap.set(intro, { clearProps: 'opacity,visibility' });
        gsap.set(hero, { yPercent: 100 });
        gsap.set(lettering, { xPercent: 60 });
        gsap.set('.site-header, .floating-bear', { autoAlpha: 0 });
        gsap.set(introSkip, { autoAlpha: 0, pointerEvents: 'none' });
        intro.hidden = false;
        intro.show();
        introSequence = gsap.timeline()
            .to('.site-header', { autoAlpha: 1, duration: 0.5 }, 1)
            .to(introSkip, { autoAlpha: 1, pointerEvents: 'auto', duration: 0.3 }, 2);
        introVideo.currentTime = 0;
        introVideo.play().catch(() => finishIntro());
        introTimer = setTimeout(() => finishIntro(false), 5500);
        try { localStorage.setItem('koi.intro.lastPlayed', String(Date.now())); } catch { /* The page also works with storage disabled. */ }
    };
    intro.addEventListener('cancel', (event) => { event.preventDefault(); finishIntro(); });
    introVideo.addEventListener('error', () => finishIntro());
    introSkip.addEventListener('click', () => finishIntro());
    document.querySelector('.intro-replay').addEventListener('click', playIntro);
    reducedMotion.addEventListener('change', () => { if (reducedMotion.matches && document.body.classList.contains('is-intro')) closeIntro(); });
    document.addEventListener('click', (event) => {
        const link = event.target.closest('a[href^="#"]');
        if (!link || link.classList.contains('skip-link') || reducedMotion.matches) return;
        const target = document.getElementById(link.hash.slice(1));
        if (!target) return;
        event.preventDefault();
        const drinkTrigger = ScrollTrigger.getById('drinks-scroll');
        const y = link.hash === '#drinks' && drinkTrigger ? drinkTrigger.start : target.getBoundingClientRect().top + scrollY;
        gsap.to(window, { scrollTo: { y, autoKill: true }, duration: link.hash === '#home' ? 2 : 1.2, ease: 'power2.inOut' });
    });
    document.fonts.ready.then(() => {
        setupScenes();
        let lastPlayed = 0;
        try { lastPlayed = Number(localStorage.getItem('koi.intro.lastPlayed')); } catch { /* Storage is optional. */ }
        if (!reducedMotion.matches && Date.now() - lastPlayed > 86400000) playIntro();
        else if (!reducedMotion.matches) gsap.fromTo('.floating-bear', { autoAlpha: 0 }, { autoAlpha: 1, duration: 0.5, clearProps: 'opacity,visibility' });
    });
    window.addEventListener('load', () => ScrollTrigger.refresh(), { once: true });
})();
