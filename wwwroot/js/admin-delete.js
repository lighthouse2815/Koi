(() => {
    const dialog = document.getElementById('delete-dialog');
    if (!dialog || typeof dialog.showModal !== 'function') return;

    const form = dialog.querySelector('form');
    const name = document.getElementById('delete-name');
    const error = document.getElementById('delete-error');
    const status = document.getElementById('delete-status');
    const empty = document.getElementById('delete-empty');
    const submit = form.querySelector('[type="submit"]');
    const cancel = form.querySelector('[data-cancel-delete]');
    let selectedLink;

    document.querySelectorAll('.delete-link').forEach(link => {
        link.addEventListener('click', event => {
            event.preventDefault();
            selectedLink = link;
            name.textContent = link.dataset.name;
            form.action = link.href;
            error.hidden = true;
            dialog.showModal();
        });
    });

    cancel.addEventListener('click', () => dialog.close());
    dialog.addEventListener('cancel', event => {
        if (submit.disabled) event.preventDefault();
    });
    form.addEventListener('submit', async event => {
        event.preventDefault();
        submit.disabled = true;
        cancel.disabled = true;
        error.hidden = true;
        try {
            const response = await fetch(form.action, { method: 'POST', body: new FormData(form) });
            const destination = new URL(form.dataset.listUrl, window.location.href);
            const result = new URL(response.url);
            if (!response.ok || !response.redirected || result.pathname.replace(/\/$/, '') !== destination.pathname.replace(/\/$/, '')) {
                if (response.status === 409) {
                    const body = await response.text();
                    const message = response.headers.get('content-type')?.includes('text/plain')
                        ? body.trim()
                        : new DOMParser().parseFromString(body, 'text/html').querySelector('.validation li')?.textContent.trim();
                    if (message) throw new Error(message);
                }
                throw new Error('Không thể xóa mục này. Vui lòng thử lại.');
            }

            selectedLink.closest('tr').remove();
            empty.hidden = document.querySelector('.delete-link') !== null;
            status.textContent = document.querySelector('.pagination')
                ? 'Đã xóa. Tải lại trang để cập nhật phân trang.'
                : 'Đã xóa thành công.';
            status.hidden = false;
            dialog.close();
        } catch (exception) {
            error.textContent = exception.message || 'Không thể xóa mục này. Vui lòng thử lại.';
            error.hidden = false;
        } finally {
            submit.disabled = false;
            cancel.disabled = false;
        }
    });
})();
