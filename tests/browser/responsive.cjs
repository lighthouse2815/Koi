const assert = require('node:assert/strict');
const { chromium } = require('playwright');

const base = process.env.KOI_TEST_URL || 'http://localhost:5118';
const sizes = [
    [360, 640], [390, 844], [844, 390], [768, 1024], [1024, 600],
    [1093, 550], [1280, 600], [1366, 650], [1536, 720], [1280, 800], [1440, 850], [1440, 900], [1920, 1080]
];

async function box(locator) {
    return locator.evaluate(element => {
        const bounds = element.getBoundingClientRect();
        return { top: bounds.top, bottom: bounds.bottom, left: bounds.left, right: bounds.right, height: bounds.height };
    });
}

async function contained(parent, children, message) {
    const outer = await box(parent);
    for (const child of await children.all()) {
        const inner = await box(child);
        assert(inner.top >= outer.top - 2 && inner.bottom <= outer.bottom + 2, message + ': content exceeds section');
    }
}

(async () => {
    const browser = await chromium.launch({
        executablePath: process.env.KOI_BROWSER_EXECUTABLE || undefined,
        args: ['--no-sandbox']
    });
    const failures = [];
    try {
        for (const [width, height] of sizes) {
            const context = await browser.newContext({ viewport: { width, height } });
            await context.addInitScript(() => localStorage.setItem('koi.intro.lastPlayed', String(Date.now())));
            const page = await context.newPage();
            page.on('pageerror', error => failures.push(error.message));
            page.on('response', response => {
                if (response.url().startsWith(base) && response.status() >= 400) failures.push(response.status() + ' ' + response.url());
            });
            for (const language of ['en', 'vi']) {
                const label = `${width}x${height} ${language}`;
                const visit = async action => {
                    await page.goto(`${base}/Home/${action}?culture=${language}`);
                    await page.evaluate(() => document.fonts.ready);
                    await page.waitForFunction(() => typeof window.koiText === 'function');
                };
                try {
                    await visit('Index');
                    await page.waitForFunction(() => !!window.ScrollTrigger?.getById('drinks-scroll'));
                    assert((await box(page.locator('.hero'))).bottom <= height + 1, label + ': hero exceeds viewport');
                    await contained(page.locator('.hero'), page.locator('.hero-note, .motion-toggle, .intro-replay'), label + ': hero controls');
                    await page.mouse.move(0, height - 1);
                    await page.locator('.menu-toggle').focus();
                    await page.keyboard.press('Enter');
                    await page.waitForTimeout(600);
                    for (const link of await page.locator('.navigation-dialog nav > a').all()) {
                        const bounds = await box(link);
                        assert(bounds.left >= -1 && bounds.right <= width + 1 && bounds.top >= -1 && bounds.bottom <= height + 1, label + ': navigation link is clipped: ' + await link.innerText() + ' ' + JSON.stringify(bounds));
                    }
                    await page.keyboard.press('Escape');
                    await page.waitForTimeout(450);
                    await page.evaluate(() => {
                        const trigger = ScrollTrigger.getById('feature-columns');
                        scrollTo({ top: trigger.start + (trigger.end - trigger.start) * .15, behavior: 'instant' });
                    });
                    await page.waitForTimeout(1300);
                    const feature = await page.locator('.features .feature-main .feature-frame').evaluateAll(frames => {
                        const current = frames.sort((a, b) => Math.abs(a.getBoundingClientRect().top) - Math.abs(b.getBoundingClientRect().top))[0];
                        const bounds = current.querySelector('.feature-caption').getBoundingClientRect();
                        return { top: bounds.top, bottom: bounds.bottom };
                    });
                    assert(feature.top >= 0 && feature.bottom <= height + 1, label + ': feature caption is clipped');
                    await page.evaluate(() => scrollTo({ top: ScrollTrigger.getById('recommendation-burst').start + 50, behavior: 'instant' }));
                    await page.waitForTimeout(1300);
                    for (const part of await page.locator('.recommendation-label, .recommendation h2, .recommendation-captions, .milk-tea > img, .green-tea > img').all()) {
                        const bounds = await box(part);
                        assert(bounds.top >= -1 && bounds.bottom <= height + 1, label + ': recommendation artwork or text is clipped');
                    }
                    await page.evaluate(() => {
                        const trigger = ScrollTrigger.getById('drinks-scroll');
                        scrollTo({ top: trigger.start + (trigger.end - trigger.start) * .35, behavior: 'instant' });
                    });
                    await page.waitForTimeout(1100);
                    const controls = await box(page.locator('.drink-controls'));
                    for (const card of await page.locator('.drink-card').all()) {
                        const bounds = await box(card);
                        assert(bounds.top >= 0 && bounds.bottom < controls.top - 4, label + ': drink name overlaps controls or is clipped');
                    }
                    const before = await page.evaluate(() => ScrollTrigger.getById('drinks-scroll').progress);
                    await page.locator('[data-drink-direction="1"]').click();
                    await page.waitForTimeout(1100);
                    assert(await page.evaluate(() => ScrollTrigger.getById('drinks-scroll').progress) > before, label + ': drink navigation');

                    await visit('OurTea');
                    await page.waitForFunction(() => !!document.querySelector('.tea-new-pictures')?.swiper);
                    await contained(page.locator('.new-drinkWrap'), page.locator('.new-drinkWrap .title, .new-drinkWrap .new-drink, .tea-pause'), label + ': New Drinks');

                    await visit('About');
                    await page.waitForFunction(() => !!document.querySelector('.ingredient-copy')?.swiper);
                    await contained(page.locator('.about-ingredient'), page.locator('.about-ingredient .article-area, .ingredient-copy'), label + ': ingredients');
                    assert((await box(page.locator('.about-ingredient .pic-area'))).height > 100, label + ': ingredient image collapsed');
                    if (width > 1024) {
                        const section = await box(page.locator('.about-ingredient'));
                        const photo = await box(page.locator('.about-ingredient .pic-area'));
                        assert(photo.height >= section.height - 2, label + ': ingredient image does not fill its column');
                        await contained(page.locator('.about-ingredient .pic-area'), page.locator('.about-ingredient .triangle'), label + ': ingredient selector');
                    }
                    const panels = width > 1024 ? '#horizontalWrap > li' : '.about-infoWrap.outter.hide-for-large > li';
                    for (const panel of await page.locator(panels).all()) {
                        await contained(panel, panel.locator('.article-area'), label + ': brand story');
                    }
                    const history = page.locator('.about-event');
                    await history.scrollIntoViewIfNeeded();
                    await page.waitForTimeout(1300);
                    const strayCups = await page.locator('.drink-area li img').evaluateAll(images => images.some(image => {
                        const bounds = image.getBoundingClientRect();
                        return bounds.width > 0 && bounds.bottom > 1 && bounds.top < innerHeight - 1;
                    }));
                    assert.equal(strayCups, false, label + ': animated cups overlap history');
                    for (let index = 0; index < 9; index++) {
                        await page.locator('.about-event .history-description').evaluate((element, index) => element.swiper.slideTo(index, 0), index);
                        await page.waitForTimeout(50);
                        await contained(history, history.locator('.history-year, .history-description'), label + ': history');
                        const text = history.locator('.history-description .swiper-slide-active');
                        assert(await text.evaluate(element => element.scrollHeight <= element.clientHeight + 2), label + ': history text is clipped');
                        if (width <= 1024) {
                            const arrows = history.locator('.arrow-area.hide-for-large');
                            await contained(history, arrows, label + ': history arrows');
                            assert((await box(arrows)).top >= (await box(history.locator('.history-description'))).bottom + 4, label + ': history arrows overlap copy');
                        }
                    }

                    await visit('Precision');
                    await page.waitForFunction(() => !!window.ScrollTrigger?.getById('precision-banner'));
                    await contained(page.locator('.precision-hero'), page.locator('.precision-hero .scroll-down'), label + ': Precision scroll control');
                    assert((await box(page.locator('.precision-hero'))).height <= height + 1, label + ': pinned Precision hero exceeds viewport');
                    console.log('PASS', label, 'hero, navigation, drink controls, New Drinks, ingredients, story panels, 9 history slides, Precision');
                } catch (error) {
                    failures.push(label + ': ' + error.message);
                    console.error('FAIL', label, error.message);
                }
            }
            await context.close();
        }
        const resizePage = await browser.newPage();
        await resizePage.addInitScript(() => localStorage.setItem('koi.intro.lastPlayed', String(Date.now())));
        await resizePage.goto(base + '/Home/Index?culture=vi');
        await resizePage.waitForFunction(() => !!window.ScrollTrigger?.getById('drinks-scroll'));
        for (const [width, height] of [[1366, 650], [844, 390], [390, 844], [1920, 1080]]) {
            await resizePage.setViewportSize({ width, height });
            await resizePage.waitForTimeout(450);
            await resizePage.evaluate(() => {
                const trigger = ScrollTrigger.getById('drinks-scroll');
                scrollTo({ top: trigger.start + (trigger.end - trigger.start) * .35, behavior: 'instant' });
            });
            await resizePage.waitForTimeout(1100);
            const controls = await box(resizePage.locator('.drink-controls'));
            for (const card of await resizePage.locator('.drink-card').all()) {
                assert((await box(card)).bottom < controls.top - 4, 'resizing window clips a drink card');
            }
        }
        await resizePage.emulateMedia({ reducedMotion: 'reduce' });
        await resizePage.waitForTimeout(300);
        assert.equal(await resizePage.locator('body').evaluate(element => element.classList.contains('motion-ready')), false);
        await resizePage.close();
        console.log('PASS resizing across desktop/mobile breakpoints and reduced motion');
        assert.deepEqual(failures, []);
    } finally {
        await browser.close();
    }
})().catch(error => { console.error(error); process.exitCode = 1; });
