(() => {
    const dialog = document.getElementById('geography-editor');
    if (!dialog || typeof dialog.showModal !== 'function') return;

    const form = dialog.querySelector('form');
    const title = dialog.querySelector('#geography-editor-title');
    const summary = form.querySelector('[data-valmsg-summary]');
    const submit = form.querySelector('[type="submit"]');
    const reload = form.querySelector('[data-geography-reload]');

    const openEditor = data => {
        const editing = !!data;
        form.reset();
        form.action = editing ? data.url : form.dataset.createUrl;
        title.textContent = editing ? 'Sửa khu vực' : 'Thêm khu vực';
        summary.replaceChildren();
        summary.classList.remove('validation-summary-errors');
        summary.classList.add('validation-summary-valid');
        reload.hidden = !editing;

        if (editing) {
            form.elements.Id.value = data.id;
            form.elements.Revision.value = data.revision;
            form.elements.Name.value = data.name;
            form.elements.SortOrder.value = data.sortOrder;
        }
        dialog.showModal();
    };

    document.querySelector('[data-geography-create]').addEventListener('click', () => openEditor());
    document.querySelectorAll('[data-geography-edit]').forEach(button => {
        button.addEventListener('click', () => openEditor(button.dataset));
    });
    dialog.querySelector('[data-geography-cancel]').addEventListener('click', () => {
        if (!submit.disabled) dialog.close();
    });
    dialog.addEventListener('cancel', event => {
        if (submit.disabled) event.preventDefault();
    });
})();
