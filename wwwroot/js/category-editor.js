(() => {
    const dialog = document.getElementById('category-editor');
    if (!dialog || typeof dialog.showModal !== 'function') return;

    const form = dialog.querySelector('form');
    const title = dialog.querySelector('#category-editor-title');
    const summary = form.querySelector('[data-valmsg-summary]');
    const submit = form.querySelector('[type="submit"]');
    const imageInput = form.querySelector('#ImageUpload');
    const reload = form.querySelector('[data-category-reload]');

    const openEditor = data => {
        const editing = !!data;
        form.reset();
        form.action = editing ? form.dataset.editUrl : form.dataset.createUrl;
        title.textContent = editing ? 'Sửa danh mục' : 'Thêm danh mục';
        summary.replaceChildren();
        summary.classList.remove('validation-summary-errors');
        summary.classList.add('validation-summary-valid');
        imageInput.required = !editing;
        imageInput.dataset.requiredOnChange = String(!editing);
        reload.hidden = !editing;

        if (editing) {
            form.elements.Id.value = data.id;
            form.elements.Revision.value = data.revision;
            form.elements.Name.value = data.name;
            form.elements.SortOrder.value = data.sortOrder;
            form.querySelector('#IsPublished').checked = data.isPublished === 'true';
            form.querySelector('#ShowOnHome').checked = data.showOnHome === 'true';
        }

        for (const [selector, url] of [['.current-image', data?.image], ['.current-icon', data?.icon]]) {
            const image = form.querySelector(selector);
            image.hidden = !url;
            if (url) image.src = url;
            else image.removeAttribute('src');
        }
        dialog.showModal();
    };

    document.querySelector('[data-category-create]').addEventListener('click', () => openEditor());
    document.querySelectorAll('[data-category-edit]').forEach(button => {
        button.addEventListener('click', () => openEditor(button.dataset));
    });
    dialog.querySelector('[data-category-cancel]').addEventListener('click', () => {
        if (!submit.disabled) dialog.close();
    });
    dialog.addEventListener('cancel', event => {
        if (submit.disabled) event.preventDefault();
    });
})();
