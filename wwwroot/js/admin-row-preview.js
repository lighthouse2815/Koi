(() => {
    const dialog = document.getElementById('row-preview-dialog');
    if (!dialog || typeof dialog.showModal !== 'function') return;

    const title = document.getElementById('row-preview-title');
    const body = dialog.querySelector('[data-row-preview-body]');
    const status = dialog.querySelector('[role="status"]');
    const close = dialog.querySelector('[data-row-preview-close]');
    let activeTrigger;
    let request;

    document.querySelectorAll('[data-row-preview-trigger]').forEach(trigger => {
        trigger.hidden = false;
        trigger.setAttribute('aria-haspopup', 'dialog');
    });
    document.body.classList.add('row-preview-ready');

    document.addEventListener('click', async event => {
        if (event.defaultPrevented || event.button !== 0 || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey || dialog.open) return;

        const directTrigger = event.target.closest('[data-row-preview-trigger]');
        const row = event.target.closest('[data-row-preview]');
        if (!directTrigger) {
            if (!row || event.target.closest('a, button, input, select, textarea, label')) return;
            const selection = window.getSelection();
            if (selection && !selection.isCollapsed && row.contains(selection.anchorNode)) return;
        }

        const trigger = directTrigger ?? row.querySelector('[data-row-preview-trigger]');
        const template = row?.querySelector('template[data-row-preview-content]');
        const url = trigger?.dataset.rowPreviewUrl;
        if (!template && !url) return;

        event.preventDefault();
        request?.abort();
        activeTrigger = trigger;
        title.textContent = template?.dataset.title ?? trigger.dataset.rowPreviewTitle;
        status.hidden = !url;
        status.textContent = url ? 'Đang tải thông tin…' : '';
        body.replaceChildren();
        if (template) body.append(template.content.cloneNode(true));

        dialog.showModal();
        dialog.scrollTop = 0;
        if (!url) return;

        request = new AbortController();
        const { signal } = request;
        try {
            const response = await fetch(url, { signal, redirect: 'error', cache: 'no-store' });
            if (!response.ok) throw new Error();
            const html = await response.text();
            if (signal.aborted || !dialog.open) return;
            body.innerHTML = html;
            status.hidden = true;
        } catch {
            if (!signal.aborted && dialog.open) status.textContent = 'Không tải được thông tin. Vui lòng đóng và mở lại.';
        }
    });

    close.addEventListener('click', () => dialog.close());
    dialog.addEventListener('close', () => {
        if (dialog.open) return;
        request?.abort();
        body.replaceChildren();
        activeTrigger?.focus({ preventScroll: true });
    });
})();
