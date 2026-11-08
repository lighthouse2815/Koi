(() => {
    const dialog = document.getElementById('news-editor');
    if (!dialog || typeof dialog.showModal !== 'function') return;

    const form = dialog.querySelector('form');
    const title = dialog.querySelector('#news-editor-title');
    const summary = form.querySelector('[data-valmsg-summary]');
    const submit = form.querySelector('[type="submit"]');
    const reload = form.querySelector('[data-news-reload]');
    const imageInput = form.querySelector('#ImageUpload');
    const image = form.querySelector('.current-image');

    const openEditor = data => {
        const editing = !!data;
        form.reset();
        form.action = editing ? data.url : form.dataset.createUrl;
        title.textContent = editing ? 'Sửa bài viết' : 'Thêm bài viết';
        summary.replaceChildren();
        summary.classList.remove('validation-summary-errors');
        summary.classList.add('validation-summary-valid');
        reload.hidden = !editing;
        imageInput.required = !editing;
        imageInput.dataset.requiredOnChange = String(!editing);

        if (editing) {
            form.elements.Id.value = data.id;
            form.elements.Revision.value = data.revision;
            form.elements.Title.value = data.title;
            form.elements.Category.value = data.category;
            form.elements.Date.value = data.date;
            form.elements.BodyHtml.value = data.bodyHtml;
            form.elements.VideoUrl.value = data.videoUrl;
            form.elements.HomeOrder.value = data.homeOrder;
            form.querySelector('#IsPublished').checked = data.isPublished === 'true';
            form.querySelector('#IsFeatured').checked = data.isFeatured === 'true';
            form.querySelector('#ShowOnHome').checked = data.showOnHome === 'true';
        }

        image.hidden = !data?.image;
        if (data?.image) image.src = data.image;
        else image.removeAttribute('src');
        dialog.showModal();
    };

    document.querySelector('[data-news-create]').addEventListener('click', () => openEditor());
    document.querySelectorAll('[data-news-edit]').forEach(button => {
        button.addEventListener('click', () => openEditor(button.dataset));
    });
    dialog.querySelector('[data-news-cancel]').addEventListener('click', () => {
        if (!submit.disabled) dialog.close();
    });
    dialog.addEventListener('cancel', event => {
        if (submit.disabled) event.preventDefault();
    });
})();
