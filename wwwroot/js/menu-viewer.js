(() => {
    const viewer = document.getElementById('menu-viewer');
    if (typeof viewer?.showModal !== 'function') return;

    const viewport = viewer.querySelector('.menu-viewer__viewport');
    const image = viewport.querySelector('img');
    const minZoom = 1;
    const maxZoom = 6;
    let zoom = minZoom;
    let drag;
    let touchStart;

    function setZoom(value) {
        zoom = value;
        image.style.setProperty('--menu-zoom', zoom);
        viewport.toggleAttribute('data-zoomed', zoom > minZoom);
    }

    function zoomAt(value, clientX, clientY) {
        const nextZoom = Math.min(maxZoom, Math.max(minZoom, value));
        if (nextZoom === zoom || !image.naturalWidth) return;

        const before = image.getBoundingClientRect();
        const x = (clientX - before.left) / before.width;
        const y = (clientY - before.top) / before.height;
        setZoom(nextZoom);
        const after = image.getBoundingClientRect();
        viewport.scrollBy(
            after.left + after.width * x - clientX,
            after.top + after.height * y - clientY
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
            viewer.setAttribute('aria-label', photo.alt);
            image.alt = photo.alt;
            image.style.setProperty('--menu-image-ratio', photo.naturalWidth / photo.naturalHeight || 1);
            image.src = link.href;
            setZoom(minZoom);
            viewer.showModal();
            viewport.scrollTo(0, 0);
        });
    });

    viewer.addEventListener('wheel', event => {
        event.preventDefault();
        const deltaMode = event.deltaMode;
        const unit = deltaMode === 1 ? 16 : deltaMode === 2 ? viewport.clientHeight : 1;
        zoomAt(zoom * Math.exp(-event.deltaY * unit / 450), event.clientX, event.clientY);
    }, { passive: false });

    viewer.addEventListener('keydown', event => {
        if (event.ctrlKey || event.metaKey || event.altKey || !['+', '=', '-', '−'].includes(event.key)) return;
        event.preventDefault();
        const bounds = viewport.getBoundingClientRect();
        const factor = event.key === '-' || event.key === '−' ? 1 / 1.25 : 1.25;
        zoomAt(zoom * factor, bounds.left + bounds.width / 2, bounds.top + bounds.height / 2);
    });

    viewer.addEventListener('pointerdown', event => {
        touchStart = event.pointerType === 'touch' ? event : undefined;
        if (event.target !== image || event.pointerType !== 'mouse' || event.button !== 0 || zoom === minZoom) return;
        image.setPointerCapture(event.pointerId);
        drag = {
            id: event.pointerId,
            x: event.clientX + viewport.scrollLeft,
            y: event.clientY + viewport.scrollTop
        };
        viewport.classList.add('is-dragging');
    });

    image.addEventListener('pointermove', event => {
        if (drag?.id !== event.pointerId) return;
        viewport.scrollTo(drag.x - event.clientX, drag.y - event.clientY);
    });

    image.addEventListener('lostpointercapture', () => {
        drag = undefined;
        viewport.classList.remove('is-dragging');
    });

    viewer.addEventListener('click', event => {
        if (touchStart) {
            // The browser can retarget a tap just outside the image to the image itself.
            const { clientX, clientY } = touchStart;
            touchStart = undefined;
            if (document.elementFromPoint(clientX, clientY) === image) {
                zoomAt(zoom === minZoom ? 2 : minZoom, clientX, clientY);
            } else {
                viewer.close();
            }
            return;
        }
        if (event.target === viewer || event.target === viewport) viewer.close();
    });
})();
