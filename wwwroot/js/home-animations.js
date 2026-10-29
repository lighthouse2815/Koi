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
    const featureStage = document.querySelector('.feature-stage');
    const recommendation = document.querySelector('.recommendation');
    const drinkTrack = document.querySelector('.drinks-track');
    const drinkStage = document.querySelector('.drinks-section');
    const carousel = document.querySelector('.drink-carousel');
    const drinkButtons = [...document.querySelectorAll('[data-drink-direction]')];
    const media = gsap.matchMedia();

    const setupScenes = () => media.add({
        desktop: '(min-width: 1024px)',
        mobile: '(max-width: 1023px)',
        reduce: '(prefers-reduced-motion: reduce)'
    }, (context) => {
        if (context.conditions.reduce) return;
        document.body.classList.add('motion-ready');
        const heroTravel = () => Math.max(0, lettering.offsetWidth - innerWidth + 40);
        const drinkTravel = () => Math.max(0, carousel.offsetWidth - innerWidth);
        const sizeTracks = () => {
            heroTrack.style.height = `${hero.offsetHeight + heroTravel()}px`;
            drinkTrack.style.height = `${drinkStage.offsetHeight + drinkTravel()}px`;
        };
        sizeTracks();
        ScrollTrigger.addEventListener('refreshInit', sizeTracks);

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
            scrollTrigger: { id: 'feature-columns', trigger: features, start: 'top top', end: () => `+=${features.offsetHeight}`, scrub: 0.8, invalidateOnRefresh: true }
        });
        featureTimeline.set(up, { y: 0 }, 0)
            .set(down, { y: () => -3 * featureStage.clientHeight }, 0)
            .fromTo(featureParts, { scale: 0 }, { scale: 1, duration: 0.05 }, 0.05);
        for (let step = 1; step <= 3; step++) {
            const at = 0.19 + (step - 1) * 0.2;
            featureTimeline.to(up, { y: () => -step * featureStage.clientHeight, duration: 0.01 }, at)
                .to(down, { y: () => -(3 - step) * featureStage.clientHeight, duration: 0.01 }, at);
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
                onUpdate: (self) => drinkButtons.forEach(button => {
                    button.disabled = Number(button.dataset.drinkDirection) < 0 ? self.progress < 0.002 : self.progress > 0.998;
                })
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
    let introTimer;
    let introFinishing = false;
    const entrance = (fromIntro) => {
        const timeline = gsap.timeline();
        if (fromIntro) timeline.fromTo(hero, { yPercent: 100 }, { yPercent: 0, duration: 1.5, ease: 'power2.out', clearProps: 'transform' }, 0);
        timeline.fromTo(lettering, { xPercent: fromIntro ? 60 : 25 }, { xPercent: 0, duration: 1.5, ease: 'power3.out' }, fromIntro ? 0.5 : 0)
            .fromTo('.site-header, .floating-bear', { autoAlpha: 0 }, { autoAlpha: 1, duration: 0.6, clearProps: 'opacity,visibility' }, 0.5);
        return timeline;
    };
    const finishIntro = () => {
        if (intro.hidden || introFinishing) return;
        introFinishing = true;
        clearTimeout(introTimer);
        introVideo.pause();
        if (reducedMotion.matches) {
            intro.close();
            intro.hidden = true;
            document.body.classList.remove('is-intro');
            introFinishing = false;
            return;
        }
        entrance(true);
        gsap.to(intro, { autoAlpha: 0, duration: 0.8, delay: 0.5, onComplete: () => {
            intro.close();
            intro.hidden = true;
            document.body.classList.remove('is-intro');
            introFinishing = false;
            ScrollTrigger.refresh();
        } });
    };
    const playIntro = () => {
        if (reducedMotion.matches || !intro.hidden) return;
        window.scrollTo({ top: 0, behavior: 'instant' });
        document.body.classList.add('is-intro');
        gsap.set(intro, { clearProps: 'opacity,visibility' });
        intro.hidden = false;
        intro.showModal();
        introVideo.currentTime = 0;
        introVideo.play().catch(finishIntro);
        introTimer = setTimeout(finishIntro, 8000);
        try { localStorage.setItem('koi.intro.lastPlayed', String(Date.now())); } catch { /* The page also works with storage disabled. */ }
    };
    intro.addEventListener('cancel', (event) => { event.preventDefault(); finishIntro(); });
    introVideo.addEventListener('ended', finishIntro);
    introVideo.addEventListener('error', finishIntro);
    document.querySelector('.intro-skip').addEventListener('click', finishIntro);
    document.querySelector('.intro-replay').addEventListener('click', playIntro);
    reducedMotion.addEventListener('change', () => { if (reducedMotion.matches) finishIntro(); });
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
        else if (!reducedMotion.matches && scrollY < innerHeight) entrance(false);
    });
    window.addEventListener('load', () => ScrollTrigger.refresh(), { once: true });
})();
