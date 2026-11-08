(() => {
    const dialog = document.getElementById('store-editor');
    if (!dialog || typeof dialog.showModal !== 'function') return;

    const form = dialog.querySelector('form');
    const title = dialog.querySelector('#store-editor-title');
    const summary = form.querySelector('[data-valmsg-summary]');
    const submit = form.querySelector('[type="submit"]');
    const reload = form.querySelector('[data-store-reload]');
    const image = form.querySelector('.current-image');

    const openEditor = data => {
        const editing = !!data;
        form.reset();
        form.action = editing ? data.url : form.dataset.createUrl;
        title.textContent = editing ? 'Sửa chi nhánh' : 'Thêm chi nhánh';
        summary.replaceChildren();
        summary.classList.remove('validation-summary-errors');
        summary.classList.add('validation-summary-valid');
        reload.hidden = !editing;

        if (editing) {
            form.elements.Id.value = data.id;
            form.elements.Revision.value = data.revision;
            form.elements.Name.value = data.name;
            form.elements.CityId.value = data.cityId;
            form.elements.Address.value = data.address;
            form.elements.Phone.value = data.phone;
            form.elements.Hours.value = data.hours;
            form.elements.NavigationUrl.value = data.navigationUrl;
            form.elements.SortOrder.value = data.sortOrder;
            form.querySelector('#IsPublished').checked = data.isPublished === 'true';
        }

        image.hidden = !data?.image;
        if (data?.image) image.src = data.image;
        else image.removeAttribute('src');
        dialog.showModal();
    };

    document.querySelector('[data-store-create]').addEventListener('click', () => openEditor());
    document.querySelectorAll('[data-store-edit]').forEach(button => {
        button.addEventListener('click', () => openEditor(button.dataset));
    });
    dialog.querySelector('[data-store-cancel]').addEventListener('click', () => {
        if (!submit.disabled) dialog.close();
    });
    dialog.addEventListener('cancel', event => {
        if (submit.disabled) event.preventDefault();
    });
})();
