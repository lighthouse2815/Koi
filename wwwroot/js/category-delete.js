(() => {
    const dialog = document.getElementById('category-delete-dialog');
    if (!dialog || typeof dialog.showModal !== 'function') return;

    const form = dialog.querySelector('form');
    const name = document.getElementById('category-delete-name');
    const error = document.getElementById('category-delete-error');
    const status = document.getElementById('category-delete-status');
    const empty = document.getElementById('category-empty');
    const submit = form.querySelector('[type="submit"]');
    const cancel = form.querySelector('[data-cancel-delete]');
    let selectedLink;

    document.querySelectorAll('.category-delete').forEach(link => {
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
                throw new Error('Không thể xóa danh mục. Vui lòng thử lại.');
            }

            selectedLink.closest('tr').remove();
            empty.hidden = document.querySelector('.category-delete') !== null;
            status.textContent = 'Đã xóa danh mục đồ uống.';
            status.hidden = false;
            dialog.close();
        } catch (exception) {
            error.textContent = exception.message || 'Không thể xóa danh mục. Vui lòng thử lại.';
            error.hidden = false;
        } finally {
            submit.disabled = false;
            cancel.disabled = false;
        }
    });
})();
