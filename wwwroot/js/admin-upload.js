(() => {
    document.querySelectorAll('form[data-upload-folder]').forEach(form => {
        const inputs = [...form.querySelectorAll('input[type="file"]')];
        let submitting = false;

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
            const buttons = [...form.querySelectorAll('button[type="submit"]')];
            buttons.forEach(button => { button.disabled = true; });
            try {
                for (const input of inputs) {
                    const file = input.files[0];
                    if (!file) continue;
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
                    if (!signed.ok) throw new Error('Không thể chuẩn bị tải ảnh. Vui lòng thử lại.');
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
                    form.elements[`${input.id}Id`].value = result.public_id;
                    input.value = '';
                    input.required = false;
                }
                submitting = true;
                form.requestSubmit();
            } catch (error) {
                alert(error.message || 'Không thể tải ảnh. Vui lòng thử lại.');
                buttons.forEach(button => { button.disabled = false; });
            }
        });
    });
})();
