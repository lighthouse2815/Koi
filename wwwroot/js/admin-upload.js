(() => {
    document.querySelectorAll('form[data-upload-folder]').forEach(form => {
        const inputs = [...form.querySelectorAll('input[type="file"]')];
        const ajaxSave = form.hasAttribute('data-ajax-save');
        const summary = form.querySelector('[data-valmsg-summary]');
        let submitting = false;
        let processing = false;

        const showError = message => {
            if (!ajaxSave || !summary) {
                alert(message);
                return;
            }
            const list = document.createElement('ul');
            const item = document.createElement('li');
            item.textContent = message;
            list.appendChild(item);
            summary.replaceChildren(list);
            summary.classList.remove('validation-summary-valid');
            summary.classList.add('validation-summary-errors');
        };

        for (const input of inputs) {
            const uploaded = form.elements[`${input.id}Id`];
            const required = input.required;
            if (uploaded.value) input.required = false;
            input.addEventListener('change', () => {
                uploaded.value = '';
                input.required = required;
            });
        }

        form.querySelector('fieldset.upload-fields').disabled = false;

        form.addEventListener('submit', async event => {
            if (submitting) return;
            event.preventDefault();
            if (processing) return;
            processing = true;
            const buttons = [...form.querySelectorAll('button[type="submit"]')];
            buttons.forEach(button => { button.disabled = true; });
            if (ajaxSave && summary) {
                summary.replaceChildren();
                summary.classList.remove('validation-summary-errors');
                summary.classList.add('validation-summary-valid');
            }
            try {
                for (const input of inputs) {
                    const file = input.files[0];
                    const uploaded = form.elements[`${input.id}Id`];
                    if (!file || (ajaxSave && uploaded.value)) continue;
                    if (file.size > 5 * 1024 * 1024 || !/\.(jpe?g|png|webp)$/i.test(file.name))
                        throw new Error('Chọn ảnh JPG, PNG hoặc WebP tối đa 5 MB.');

                    const params = new URLSearchParams({
                        folder: form.dataset.uploadFolder,
                        __RequestVerificationToken: form.elements.__RequestVerificationToken.value
                    });
                    const signed = await fetch('/Admin/Uploads/Sign', {
                        method: 'POST',
                        body: params
                    });
                    if (!signed.ok) {
                        const details = await signed.json().catch(() => ({}));
                        throw new Error(details.message || 'Không thể chuẩn bị tải ảnh. Vui lòng thử lại.');
                    }
                    if (signed.redirected) throw new Error('Phiên đăng nhập đã hết. Vui lòng đăng nhập lại.');
                    const config = await signed.json();
                    const data = new FormData();
                    data.append('file', file);
                    data.append('api_key', config.apiKey);
                    data.append('timestamp', config.timestamp);
                    data.append('public_id', config.publicId);
                    data.append('allowed_formats', config.allowedFormats);
                    data.append('overwrite', config.overwrite);
                    data.append('signature', config.signature);
                    const response = await fetch(`https://api.cloudinary.com/v1_1/${encodeURIComponent(config.cloudName)}/image/upload`, { method: 'POST', body: data });
                    if (!response.ok) throw new Error('Không thể tải ảnh lên Cloudinary. Vui lòng thử lại.');
                    const result = await response.json();
                    if (result.public_id !== config.publicId) throw new Error('Ảnh tải lên không hợp lệ.');
                    uploaded.value = result.public_id;
                    if (!ajaxSave) input.value = '';
                    input.required = false;
                }
                if (ajaxSave) {
                    const response = await fetch(form.action, {
                        method: 'POST',
                        headers: { 'X-Requested-With': 'XMLHttpRequest' },
                        body: new FormData(form)
                    });
                    if (response.redirected) throw new Error('Phiên đăng nhập đã hết. Vui lòng đăng nhập lại.');
                    const result = await response.json().catch(() => ({}));
                    if (!response.ok) throw new Error(result.message || 'Không thể lưu danh mục. Vui lòng thử lại.');
                    if (!result.redirectUrl) throw new Error('Không nhận được kết quả lưu. Vui lòng tải lại trang.');
                    const destination = new URL(result.redirectUrl, window.location.href);
                    if (destination.origin !== window.location.origin) throw new Error('Không thể chuyển về danh sách.');
                    window.location.assign(destination.href);
                    submitting = true;
                    return;
                }
                submitting = true;
                form.requestSubmit();
            } catch (error) {
                showError(error.message || 'Không thể lưu. Vui lòng thử lại.');
            } finally {
                processing = false;
                if (!submitting) buttons.forEach(button => { button.disabled = false; });
            }
        });
    });
})();
