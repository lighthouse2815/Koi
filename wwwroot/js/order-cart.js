(() => {
    'use strict';

    const cartKey = 'koi.order.cart';
    const customerKey = 'koi.order.customer';
    const money = new Intl.NumberFormat('vi-VN');
    const formatPrice = price => money.format(price) + ' đ';

    function customerToken(element) {
        const token = localStorage.getItem(customerKey) || element.dataset.customerToken;
        localStorage.setItem(customerKey, token);
        return token;
    }

    function forgetCustomerToken(token) {
        if (localStorage.getItem(customerKey) === token) localStorage.removeItem(customerKey);
    }

    const historyForm = document.querySelector('[data-order-history]');
    if (historyForm) {
        loadHistory();
        return;
    }

    async function loadHistory() {
        const results = document.querySelector('[data-history-results]');
        try {
            const token = customerToken(historyForm);
            const data = new FormData(historyForm);
            data.set('customerToken', token);
            const response = await fetch(historyForm.action, { method: 'POST', body: data });
            if (response.status === 401) forgetCustomerToken(token);
            if (!response.ok) throw new Error('History request failed');

            results.innerHTML = await response.text();
        } catch {
            results.textContent = 'Chưa tải được đơn đã mua. Bạn hãy tải lại trang để thử lại.';
        }
    }

    const root = document.querySelector('[data-order-cart]');
    if (!root) return;

    const branchPicker = root.querySelector('#branches');
    const branchSelect = root.querySelector('#storeId');
    const branchName = root.querySelector('[data-branch-name]');
    const branchAddress = root.querySelector('[data-branch-address]');
    const branchChange = root.querySelector('[data-branch-change]');
    const notice = root.querySelector('[data-cart-message]');
    const cartLines = root.querySelector('[data-cart-lines]');
    const lineTemplate = root.querySelector('[data-cart-line]');
    const emptyCart = root.querySelector('[data-empty-cart]');
    const priceError = root.querySelector('[data-price-error]');
    const checkout = root.querySelector('#checkout');
    const checkoutForm = checkout.querySelector('[data-checkout-form]');
    const checkoutButton = checkoutForm.querySelector('button[type="submit"]');
    const checkoutErrors = checkout.querySelector('[data-checkout-errors]');
    const checkoutLink = root.querySelector('[data-checkout-link]');
    const chooseBranch = root.querySelector('[data-choose-branch]');
    const prices = new Map();

    let cart = readCart();
    let priceRequest = 0;
    let fetchingPrices = false;
    let priceRequestFailed = false;
    let submitting = false;

    function readCart() {
        try {
            const saved = JSON.parse(localStorage.getItem(cartKey));
            if (typeof saved?.storeId === 'string' && typeof saved.orderId === 'string' && Array.isArray(saved.items)) {
                const items = saved.items.filter(item => typeof item?.drinkId === 'string'
                    && Number.isInteger(item.quantity) && item.quantity >= 1 && item.quantity <= 20);
                return { ...saved, items: items.slice(0, 20) };
            }
        } catch {
            // A missing or unreadable cart starts empty.
        }
        return { storeId: '', orderId: '', items: [] };
    }

    function notify(text, showCart = false) {
        notice.querySelector('span').textContent = text;
        notice.querySelector('a').hidden = !showCart;
        notice.hidden = !text;
    }

    function saveCart(storeId, items) {
        try {
            const bytes = crypto.getRandomValues(new Uint8Array(16));
            const orderId = Array.from(bytes, byte => byte.toString(16).padStart(2, '0')).join('');
            const nextCart = { storeId, items, orderId };
            localStorage.setItem(cartKey, JSON.stringify(nextCart));
            cart = nextCart;
            checkoutErrors.hidden = true;
            renderCart();
            return true;
        } catch {
            notify('Trình duyệt chưa cho phép lưu giỏ hàng.');
            return false;
        }
    }

    function renderBranch() {
        if (branchSelect) branchSelect.value = cart.storeId;
        const branch = branchSelect?.selectedOptions[0];
        const selected = Boolean(branch?.value);
        root.toggleAttribute('data-has-branch', selected);
        branchName.textContent = selected ? branch.dataset.name : 'Chọn chi nhánh nhận đơn';
        branchAddress.textContent = selected ? branch.dataset.address : '';
        branchAddress.hidden = !selected;
        branchChange.textContent = selected ? 'Thay đổi' : 'Danh sách';
        branchPicker.open = !selected;
    }

    function renderCart() {
        const count = cart.items.reduce((sum, item) => sum + item.quantity, 0);
        const total = cart.items.reduce((sum, item) => sum + (prices.get(item.drinkId)?.price ?? 0) * item.quantity, 0);
        document.querySelectorAll('[data-cart-count]').forEach(element => { element.textContent = count; });
        document.querySelectorAll('[data-cart-total]').forEach(element => { element.textContent = formatPrice(total); });
        emptyCart.hidden = count > 0;
        priceError.hidden = count === 0 || !priceRequestFailed;

        const lines = cart.items.map(item => {
            const line = lineTemplate.content.cloneNode(true);
            const drink = prices.get(item.drinkId);
            const name = drink?.name ?? (fetchingPrices ? 'Đang tải món…' : priceRequestFailed ? 'Chưa tải được món' : 'Món không còn được bán');
            line.querySelector('strong').textContent = name;
            line.querySelector('.cart-line-title span').textContent = drink ? formatPrice(drink.price * item.quantity) : '—';
            line.querySelector('[data-update-item]').dataset.drinkId = item.drinkId;
            line.querySelector('input[name="quantity"]').value = item.quantity;
            const remove = line.querySelector('[data-remove-item]');
            remove.dataset.drinkId = item.drinkId;
            remove.setAttribute('aria-label', 'Xóa món ' + name);
            return line;
        });
        cartLines.replaceChildren(...lines);

        const hasBranch = root.hasAttribute('data-has-branch');
        checkout.hidden = !hasBranch || count === 0;
        checkoutLink.hidden = checkout.hidden;
        chooseBranch.hidden = hasBranch;
        updateCheckoutButton();
    }

    function updateCheckoutButton() {
        checkoutButton.disabled = submitting || fetchingPrices || cart.items.some(item => !prices.has(item.drinkId));
    }

    async function refreshPrices() {
        const requestId = ++priceRequest;
        const items = cart.items;
        fetchingPrices = items.length > 0;
        priceRequestFailed = false;
        renderCart();
        if (!fetchingPrices) return;

        try {
            const url = new URL(root.dataset.linesUrl, location.href);
            for (const item of items) url.searchParams.append('ids', item.drinkId);
            const response = await fetch(url);
            if (!response.ok) throw new Error('Cart prices request failed');
            const drinks = await response.json();
            if (requestId !== priceRequest) return;

            for (const item of items) prices.delete(item.drinkId);
            for (const drink of drinks) prices.set(drink.drinkId, drink);
        } catch {
            if (requestId === priceRequest) priceRequestFailed = true;
        } finally {
            if (requestId === priceRequest) {
                fetchingPrices = false;
                renderCart();
            }
        }
    }

    root.querySelector('[data-branch-form]')?.addEventListener('submit', event => {
        event.preventDefault();
        if (branchSelect.value !== cart.storeId && !saveCart(branchSelect.value, cart.items)) return;

        renderBranch();
        renderCart();
        notify('');
        location.hash = 'drinks';
    });

    root.addEventListener('submit', event => {
        const form = event.target;
        if (!form.matches('[data-add-item], [data-update-item]')) return;
        event.preventDefault();

        const drinkId = form.dataset.drinkId;
        const quantity = Number(form.elements.quantity.value);
        if (form.matches('[data-update-item]')) {
            updateQuantity(drinkId, quantity);
            return;
        }

        const existing = cart.items.find(item => item.drinkId === drinkId);
        const total = (existing?.quantity ?? 0) + quantity;
        if (total > 20) return notify('Mỗi món chỉ được đặt tối đa 20 ly.');
        if (!existing && cart.items.length >= 20) return notify('Giỏ hàng chỉ hỗ trợ tối đa 20 món khác nhau.');

        prices.set(drinkId, { name: form.dataset.name, price: Number(form.dataset.price) });
        if (updateQuantity(drinkId, total)) notify('Đã thêm đồ uống vào giỏ hàng.', true);
    });

    function updateQuantity(drinkId, quantity) {
        const index = cart.items.findIndex(item => item.drinkId === drinkId);
        if (index >= 0 && cart.items[index].quantity === quantity) return true;

        const items = cart.items.filter(item => item.drinkId !== drinkId);
        if (quantity > 0) items.splice(index < 0 ? items.length : index, 0, { drinkId, quantity });
        return saveCart(cart.storeId, items);
    }

    cartLines.addEventListener('click', event => {
        const button = event.target.closest('[data-remove-item]');
        if (button && updateQuantity(button.dataset.drinkId, 0)) notify('');
    });
    priceError.querySelector('[data-retry-prices]').addEventListener('click', refreshPrices);

    checkoutForm.addEventListener('submit', async event => {
        event.preventDefault();
        if (checkoutButton.disabled) return;

        const submitted = cart;
        submitting = true;
        updateCheckoutButton();
        checkoutErrors.hidden = true;

        try {
            const token = customerToken(root);
            const data = new FormData(checkoutForm);
            data.set('Checkout.CustomerToken', token);
            data.set('Checkout.OrderId', submitted.orderId);
            data.set('Checkout.StoreId', submitted.storeId);
            submitted.items.forEach((item, index) => {
                const key = 'Checkout.Items[' + index + ']';
                data.set(key + '.DrinkId', item.drinkId);
                data.set(key + '.Quantity', item.quantity);
                data.set(key + '.Price', prices.get(item.drinkId).price);
            });

            const response = await fetch(checkoutForm.action, {
                method: 'POST',
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                body: data
            });
            const result = await response.json().catch(() => null);
            if (response.status === 401) forgetCustomerToken(token);

            if (!response.ok || !result?.confirmationUrl) {
                const errors = result?.errors ? Object.values(result.errors).flat() : [];
                showCheckoutErrors(errors.length ? errors : [result?.message ?? 'Chưa gửi được đơn. Bạn hãy thử lại.']);
                if (response.status === 400) await refreshPrices();
                return;
            }

            if (readCart().orderId === submitted.orderId) localStorage.removeItem(cartKey);
            location.assign(result.confirmationUrl);
        } catch {
            showCheckoutErrors(['Chưa gửi được đơn. Bạn hãy thử lại.']);
        } finally {
            submitting = false;
            updateCheckoutButton();
        }
    });

    function showCheckoutErrors(messages) {
        const list = document.createElement('ul');
        for (const message of messages) {
            const item = document.createElement('li');
            item.textContent = message;
            list.append(item);
        }
        checkoutErrors.replaceChildren(list);
        checkoutErrors.hidden = false;
        checkoutErrors.focus();
    }

    function restoreCart() {
        cart = readCart();
        renderBranch();
        refreshPrices();
    }

    window.addEventListener('storage', event => {
        if (event.key === cartKey || event.key === null) restoreCart();
    });
    window.addEventListener('pageshow', event => {
        if (event.persisted) restoreCart();
    });
    restoreCart();
})();
