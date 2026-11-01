(() => {
    'use strict';
    const results = document.querySelector('[data-menu-results]');
    if (!results || !('IntersectionObserver' in window)) return;
    const grid = results.querySelector('.drink-grid');
    let marker = results.querySelector('[data-menu-scroll]');
    if (!marker) return;
    let loading = false;

    const observer = new IntersectionObserver(entries => {
        if (entries.some(entry => entry.isIntersecting)) loadMore();
    }, { rootMargin: '0px 0px 400px 0px' });

    const watchNextBatch = () => {
        const link = marker.querySelector('a[rel="next"]');
        if (!link) { observer.disconnect(); return; }
        link.hidden = true;
        observer.observe(marker);
    };

    const loadMore = async () => {
        const link = marker.querySelector('a[rel="next"]');
        if (loading || !link) return;
        loading = true;
        observer.unobserve(marker);
        results.setAttribute('aria-busy', 'true');
        link.hidden = true;
        marker.querySelector('[role="status"]').textContent = 'Đang chuẩn bị thêm món cho bạn…';
        try {
            const response = await fetch(link.getAttribute('href'), { credentials: 'same-origin' });
            if (!response.ok) throw new Error('Menu request failed');
            const nextPage = new DOMParser().parseFromString(await response.text(), 'text/html');
            const cards = [...nextPage.querySelectorAll('[data-menu-results] .drink-card')];
            const nextMarker = nextPage.querySelector('[data-menu-scroll]');
            if (!cards.length || !nextMarker) throw new Error('Menu response is incomplete');
            grid.append(...cards);
            marker.replaceWith(nextMarker);
            marker = nextMarker;
            results.dispatchEvent(new CustomEvent('menu:loaded', { bubbles: true, detail: { cards } }));
            watchNextBatch();
        } catch {
            marker.querySelector('[role="status"]').textContent = 'Chưa tải được món tiếp theo. Bạn thử lại nhé.';
            link.textContent = 'Thử tải lại ↻';
            link.hidden = false;
        } finally {
            loading = false;
            results.setAttribute('aria-busy', 'false');
        }
    };

    results.addEventListener('click', event => {
        if (!event.target.closest('.menu-load-more') || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;
        event.preventDefault();
        loadMore();
    });
    watchNextBatch();
})();
