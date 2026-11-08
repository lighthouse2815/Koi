(() => {
    const dialog = document.getElementById('menu-editor');
    if (!dialog || typeof dialog.showModal !== 'function') return;

    const form = dialog.querySelector('form');
    const title = dialog.querySelector('#menu-editor-title');
    const summary = form.querySelector('[data-valmsg-summary]');
    const submit = form.querySelector('[type="submit"]');
    const reload = form.querySelector('[data-menu-reload]');
    const imageInput = form.querySelector('#ImageUpload');
    const imageLink = form.querySelector('[data-menu-current]');
    const image = imageLink.querySelector('img');

    const openEditor = data => {
        const editing = !!data;
        form.reset();
        form.action = editing ? data.url : form.dataset.createUrl;
        title.textContent = editing ? 'Sửa thực đơn' : 'Thêm thực đơn';
        summary.replaceChildren();
        summary.classList.remove('validation-summary-errors');
        summary.classList.add('validation-summary-valid');
        reload.hidden = !editing;
        imageInput.required = !editing;
        imageInput.dataset.requiredOnChange = String(!editing);

        if (editing) {
            form.elements.Id.value = data.id;
            form.elements.Revision.value = data.revision;
            form.elements.Label.value = data.label;
            form.elements.SortOrder.value = data.sortOrder;
            form.querySelector('#IsPublished').checked = data.isPublished === 'true';
        }

        imageLink.hidden = !data?.image;
        if (data?.image) {
            image.src = data.image;
            imageLink.href = data.image;
        } else {
            image.removeAttribute('src');
            imageLink.removeAttribute('href');
        }
        dialog.showModal();
    };

    document.querySelector('[data-menu-create]').addEventListener('click', () => openEditor());
    document.querySelectorAll('[data-menu-edit]').forEach(button => {
        button.addEventListener('click', () => openEditor(button.dataset));
    });
    dialog.querySelector('[data-menu-cancel]').addEventListener('click', () => {
        if (!submit.disabled) dialog.close();
    });
    dialog.addEventListener('cancel', event => {
        if (submit.disabled) event.preventDefault();
    });
})();
