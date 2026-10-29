'use strict';

const menu = document.querySelector('#navigation-dialog');
document.querySelector('.menu-toggle')?.addEventListener('click', () => menu.showModal());
document.querySelector('.menu-close')?.addEventListener('click', () => menu.close());
menu?.addEventListener('click', (event) => {
    if (event.target === menu) {
        const bounds = menu.getBoundingClientRect();
        if (event.clientX < bounds.left || event.clientY > bounds.bottom) menu.close();
    }
});
menu?.querySelectorAll('a').forEach((link) => link.addEventListener('click', () => menu.close()));

const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
const video = document.querySelector('.hero-video');
const motionToggle = document.querySelector('.motion-toggle');
let userPausedVideo = false;
const syncVideo = () => {
    if (!video) return;
    if (userPausedVideo || reducedMotion.matches || document.hidden) video.pause();
    else video.play().catch(() => { /* Browsers can require a gesture before playback. */ });
};
motionToggle?.addEventListener('click', () => {
    userPausedVideo = !userPausedVideo;
    motionToggle.setAttribute('aria-pressed', String(userPausedVideo));
    motionToggle.setAttribute('aria-label', userPausedVideo ? 'Play background video' : 'Pause background video');
    motionToggle.innerHTML = userPausedVideo ? '▷ <span>PLAY</span>' : 'Ⅱ <span>PAUSE</span>';
    syncVideo();
});
reducedMotion.addEventListener('change', syncVideo);
document.addEventListener('visibilitychange', syncVideo);
syncVideo();

// Keep the oversized wordmark in view while scrolling, as on the reference site.
const heroTrack = document.querySelector('.hero-track');
const heroLettering = document.querySelector('.hero-lettering');
if (heroTrack && heroLettering) {
    let travel = 0;
    let framePending = false;
    const updateHero = () => {
        const progress = Math.min(travel, Math.max(0, -heroTrack.getBoundingClientRect().top));
        heroLettering.style.setProperty('--hero-x', `${-progress}px`);
        framePending = false;
    };
    const sizeHero = () => {
        travel = reducedMotion.matches ? 0 : Math.max(0, heroLettering.offsetWidth - window.innerWidth + 40);
        heroTrack.style.height = `${Math.max(680, window.innerHeight) + travel}px`;
        updateHero();
    };
    window.addEventListener('scroll', () => {
        if (!framePending) {
            framePending = true;
            requestAnimationFrame(updateHero);
        }
    }, { passive: true });
    window.addEventListener('resize', sizeHero);
    reducedMotion.addEventListener('change', sizeHero);
    window.addEventListener('load', sizeHero, { once: true });
    sizeHero();
}

const drinkCarousel = document.querySelector('#drink-carousel');
const drinkButtons = document.querySelectorAll('[data-drink-direction]');
if (drinkCarousel) {
    const updateDrinkButtons = () => {
        drinkButtons.forEach((button) => {
            const atStart = drinkCarousel.scrollLeft < 5;
            const atEnd = drinkCarousel.scrollLeft + drinkCarousel.clientWidth >= drinkCarousel.scrollWidth - 5;
            button.disabled = Number(button.dataset.drinkDirection) < 0 ? atStart : atEnd;
        });
    };
    drinkButtons.forEach((button) => button.addEventListener('click', () => {
        const card = drinkCarousel.querySelector('.drink-card');
        const gap = parseFloat(getComputedStyle(drinkCarousel).columnGap) || 0;
        drinkCarousel.scrollBy({ left: (card.offsetWidth + gap) * Number(button.dataset.drinkDirection), behavior: reducedMotion.matches ? 'instant' : 'smooth' });
    }));
    drinkCarousel.addEventListener('scroll', updateDrinkButtons, { passive: true });
    window.addEventListener('resize', updateDrinkButtons);
    updateDrinkButtons();
}

const newsSlides = document.querySelectorAll('.news-slide');
const newsButtons = document.querySelectorAll('[data-news-slide]');
newsButtons.forEach((button) => button.addEventListener('click', () => {
    const selected = Number(button.dataset.newsSlide);
    newsSlides.forEach((slide, index) => { slide.hidden = index !== selected; });
    newsButtons.forEach((item, index) => item.setAttribute('aria-pressed', String(index === selected)));
}));
