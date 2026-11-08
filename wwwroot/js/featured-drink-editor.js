(() => {
    const dialog = document.getElementById('featured-editor');
    if (!dialog || typeof dialog.showModal !== 'function') return;

    const form = dialog.querySelector('form');
    const title = dialog.querySelector('#featured-editor-title');
    const summary = form.querySelector('[data-valmsg-summary]');
    const submit = form.querySelector('[type="submit"]');
    const reload = form.querySelector('[data-featured-reload]');

    const openEditor = data => {
        const editing = !!data;
        form.reset();
        form.action = editing ? data.url : form.dataset.createUrl;
        title.textContent = editing ? 'Sửa New Drinks' : 'Thêm New Drinks';
        summary.replaceChildren();
        summary.classList.remove('validation-summary-errors');
        summary.classList.add('validation-summary-valid');
        reload.hidden = !editing;

        for (const input of form.querySelectorAll('input[type="file"]')) {
            input.required = !editing;
            input.dataset.requiredOnChange = String(!editing);
        }

        if (editing) {
            form.elements.Id.value = data.id;
            form.elements.Revision.value = data.revision;
            form.elements.Name.value = data.name;
            form.elements.SortOrder.value = data.sortOrder;
            form.querySelector('#IsPublished').checked = data.isPublished === 'true';
        }

        for (const [selector, url] of [['[data-featured-image]', data?.image], ['[data-featured-background]', data?.background]]) {
            const image = form.querySelector(selector);
            image.hidden = !url;
            if (url) image.src = url;
            else image.removeAttribute('src');
        }
        dialog.showModal();
    };

    document.querySelector('[data-featured-create]').addEventListener('click', () => openEditor());
    document.querySelectorAll('[data-featured-edit]').forEach(button => {
        button.addEventListener('click', () => openEditor(button.dataset));
    });
    dialog.querySelector('[data-featured-cancel]').addEventListener('click', () => {
        if (!submit.disabled) dialog.close();
    });
    dialog.addEventListener('cancel', event => {
        if (submit.disabled) event.preventDefault();
    });
})();
