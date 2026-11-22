(() => {
    const viewer = document.getElementById('menu-viewer');
    if (typeof viewer?.showModal !== 'function') return;

    const title = viewer.querySelector('h2');
    const viewport = viewer.querySelector('.menu-viewer__viewport');
    const image = viewport.querySelector('img');
    const zoomOut = viewer.querySelector('[data-zoom-out]');
    const zoomIn = viewer.querySelector('[data-zoom-in]');
    const zoomLabel = viewer.querySelector('output');
    const minZoom = 100;
    const maxZoom = 400;
    const zoomStep = 50;
    let zoom = minZoom;

    function setZoom(value) {
        zoom = value;
        image.style.setProperty('--menu-zoom', zoom / 100);
        zoomOut.disabled = zoom <= minZoom;
        zoomIn.disabled = zoom >= maxZoom;
        zoomLabel.value = `${zoom}%`;
    }

    function changeZoom(amount) {
        const centerX = (viewport.scrollLeft + viewport.clientWidth / 2) / viewport.scrollWidth;
        const centerY = (viewport.scrollTop + viewport.clientHeight / 2) / viewport.scrollHeight;
        setZoom(zoom + amount);
        viewport.scrollTo(
            centerX * viewport.scrollWidth - viewport.clientWidth / 2,
            centerY * viewport.scrollHeight - viewport.clientHeight / 2
        );
    }

    image.addEventListener('load', () => {
        image.style.setProperty('--menu-image-ratio', image.naturalWidth / image.naturalHeight);
    });

    document.querySelectorAll('[data-menu-photo]').forEach(link => {
        link.setAttribute('aria-haspopup', 'dialog');
        link.addEventListener('click', event => {
            if (event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;
            event.preventDefault();
            const photo = link.querySelector('img');
            title.textContent = photo.alt;
            image.alt = photo.alt;
            image.src = link.href;
            setZoom(minZoom);
            viewer.showModal();
            viewport.scrollTo(0, 0);
        });
    });

    zoomOut.addEventListener('click', () => changeZoom(-zoomStep));
    zoomIn.addEventListener('click', () => changeZoom(zoomStep));
    viewer.addEventListener('click', event => {
        if (event.target === viewer) viewer.close();
    });
})();
