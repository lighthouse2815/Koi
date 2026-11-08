(() => {
    const dialog = document.getElementById('drink-editor');
    if (!dialog || typeof dialog.showModal !== 'function') return;

    const form = dialog.querySelector('form');
    const title = dialog.querySelector('#drink-editor-title');
    const summary = form.querySelector('[data-valmsg-summary]');
    const submit = form.querySelector('[type="submit"]');
    const reload = form.querySelector('[data-drink-reload]');
    const image = form.querySelector('.current-image');

    const openEditor = data => {
        const editing = !!data;
        form.reset();
        form.action = editing ? data.url : form.dataset.createUrl;
        title.textContent = editing ? 'Sửa đồ uống' : 'Thêm đồ uống';
        summary.replaceChildren();
        summary.classList.remove('validation-summary-errors');
        summary.classList.add('validation-summary-valid');
        reload.hidden = !editing;

        if (editing) {
            form.elements.Id.value = data.id;
            form.elements.Revision.value = data.revision;
            form.elements.Name.value = data.name;
            form.elements.CategoryId.value = data.categoryId;
            form.elements.Description.value = data.description;
            form.elements.SortOrder.value = data.sortOrder;
            form.querySelector('#IsPublished').checked = data.isPublished === 'true';
        }

        image.hidden = !data?.image;
        if (data?.image) image.src = data.image;
        else image.removeAttribute('src');
        dialog.showModal();
    };

    document.querySelector('[data-drink-create]').addEventListener('click', () => openEditor());
    document.querySelectorAll('[data-drink-edit]').forEach(button => {
        button.addEventListener('click', () => openEditor(button.dataset));
    });
    dialog.querySelector('[data-drink-cancel]').addEventListener('click', () => {
        if (!submit.disabled) dialog.close();
    });
    dialog.addEventListener('cancel', event => {
        if (submit.disabled) event.preventDefault();
    });
})();
