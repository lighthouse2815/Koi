(() => {
    'use strict';
    const language = document.documentElement.lang === 'vi' ? 'vi' : 'en';
    const path = action => '/' + language + '/Home/' + action + '/';
    const query = new URLSearchParams(location.search);
    const get = name => [...query].find(([key]) => key.toLowerCase() === name)?.[1];
    const integer = (value, fallback = 0) => value && /^[+-]?\d+$/.test(value) && Number.isSafeInteger(Number(value)) && Number(value) >= -2147483648 && Number(value) <= 2147483647 ? Number(value) : fallback;
    const guid = value => /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(value || '') ? value.toLowerCase() : '';
    const format = (value, ...args) => value.replace(/\{(\d+)\}/g, (_, index) => args[Number(index)]);
    const create = (tag, className, text) => {
        const element = document.createElement(tag);
        if (className) element.className = className;
        if (text != null) element.textContent = text;
        return element;
    };
    const url = (action, values, fragment = '') => {
        const search = new URLSearchParams(Object.entries(values).filter(([, value]) => value));
        return path(action) + (search.size ? '?' + search : '') + fragment;
    };
    const anchor = (href, text, current = false) => {
        const link = create('a', '', text); link.href = href;
        if (current) link.setAttribute('aria-current', 'page');
        return link;
    };
    const normalize = values => history.replaceState(null, '', url(document.querySelector('.storeWrap') ? 'Store' : 'News', values, location.hash));
    const read = async name => {
        const response = await fetch('/data/' + name + '-' + language + '.json');
        if (!response.ok) throw new Error('Data HTTP ' + response.status);
        return response.json();
    };

    function paginate(container, action, state, page, pages, labels, store) {
        container.replaceChildren();
        const previous = create('div', 'prev');
        const next = create('div', 'next');
        const list = create('ul', (store ? 'store-page' : 'news-page') + ' flex-container align-center-middle');
        const pageUrl = number => url(action, { ...state, page: number === 1 ? 0 : number }, store ? '' : '#articles');
        if (page > 1) { const link = anchor(pageUrl(page - 1), labels.prev); link.rel = 'prev'; previous.append(link); }
        if (page < pages) { const link = anchor(pageUrl(page + 1), labels.next); link.rel = 'next'; next.append(link); }
        const visible = Array.from({ length: pages }, (_, index) => index + 1).filter(number =>
            !store || pages <= 9 || number === 1 || number === pages || Math.abs(number - page) <= 1 || (page <= 2 && number <= 3) || (page >= pages - 1 && number >= pages - 2));
        visible.forEach((number, index) => {
            if (index && number > visible[index - 1] + 1) { const gap = create('li', 'ellipsis', '…'); gap.setAttribute('aria-hidden', 'true'); list.append(gap); }
            const item = create('li', number === page ? 'current' : '');
            const link = anchor(pageUrl(number), number, number === page);
            link.setAttribute('aria-label', store ? format(labels['Page {0} of {1}'], number, pages) : format(labels['Page {0}'], number));
            item.append(link); list.append(item);
        });
        container.append(previous, list, next);
    }

    async function stores() {
        const data = await read('stores');
        const city = data.locations.find(value => value.id === guid(get('s')));
        const state = { s: city?.id || '' };
        const results = data.stores.filter(value => !state.s || value.locationId === state.s)
            .sort((a, b) => (state.s ? a.locationOrder - b.locationOrder : a.countryOrder - b.countryOrder) || a.id.localeCompare(b.id));
        const pages = Math.max(1, Math.ceil(results.length / 9));
        const page = Math.min(pages, Math.max(1, integer(get('page'), 1)));
        const filters = [...document.querySelectorAll('.storeCatList')];
        const filter = (container, items, selected, base, field) => {
            container.replaceChildren();
            [{ id: '', name: data.labels.All__text }, ...items].forEach(value => {
                const item = create('li', value.id === selected ? 'current' : '');
                const link = anchor(url('Store', { ...base, [field]: value.id }), null, value.id === selected);
                link.className = 'flex-container align-middle';
                link.append(create('span', 'dot'), create('span', 'title', value.name)); item.append(link); container.append(item);
            });
        };
        filter(filters[0], data.locations, state.s, {}, 's');
        const list = document.querySelector('.storeList'); list.innerHTML = results.slice((page - 1) * 9, page * 9).map(value => value.html).join('');
        document.querySelector('.store-empty')?.remove();
        if (!results.length) { const empty = create('p', 'store-empty', data.labels['No stores match these filters. Choose another location.']); empty.setAttribute('role', 'status'); list.after(empty); }
        let pagination = document.querySelector('.store-pageWrap');
        if (pages === 1) pagination?.remove();
        else {
            if (!pagination) { pagination = create('nav', 'store-pageWrap flex-container align-center-middle'); document.querySelector('.storeList-area').append(pagination); }
            pagination.setAttribute('aria-label', data.labels['Store pages']);
            paginate(pagination, 'Store', state, page, pages, data.labels, true);
        }
        normalize({ ...state, page: page === 1 ? 0 : page });
    }

    async function news() {
        const data = await read('news');
        const requested = get('category')?.toUpperCase();
        const category = ['NEWS', 'MEDIA'].includes(requested) ? requested : null;
        const results = data.articles.filter(value => !category || value.category === category);
        const pages = Math.max(1, Math.ceil(results.length / 5));
        const page = Math.min(pages, Math.max(1, integer(get('page'), 1)));
        document.querySelectorAll('.newsCatList a').forEach(link => {
            const current = (new URL(link.href).searchParams.get('category') || null) === category;
            link.parentElement.classList.toggle('current', current);
            if (current) link.setAttribute('aria-current', 'page'); else link.removeAttribute('aria-current');
        });
        document.querySelector('.newsList').innerHTML = results.slice((page - 1) * 5, page * 5).map(value => value.html).join('');
        const pagination = document.querySelector('.news-pageWrap');
        paginate(pagination, 'News', { category }, page, pages, data.labels, false);
        normalize({ category, page: page === 1 ? 0 : page });
    }

    function languages() {
        document.querySelectorAll('[data-static-language]').forEach(link => {
            const target = new URL(link.href);
            target.search = location.search; target.hash = location.hash;
            link.href = target.pathname + target.search + target.hash;
            link.addEventListener('click', () => { try { localStorage.setItem('koi.language', link.dataset.staticLanguage); } catch { /* Navigation also works with storage disabled. */ } });
        });
    }
    async function scripts() {
        for (const src of JSON.parse(document.getElementById('static-scripts')?.textContent || '[]')) {
            await new Promise((resolve, reject) => {
                const script = create('script'); script.src = src; script.async = false;
                script.onload = resolve; script.onerror = () => reject(new Error('Script could not load: ' + src)); document.body.append(script);
            });
        }
    }
    async function start() {
        document.addEventListener('submit', event => { if (event.target.closest('.contactWrap')) event.preventDefault(); });
        try {
            if (location.search && document.querySelector('.storeWrap')) await stores();
            if (location.search && document.querySelector('.newsWrap')) await news();
        } catch (error) {
            const box = create('p', 'static-error', language === 'vi' ? 'Không tải được dữ liệu. Vui lòng tải lại trang.' : 'Data could not load. Please reload this page.');
            box.setAttribute('role', 'alert'); document.querySelector('main')?.prepend(box);
            document.querySelector('.storeList, .newsList')?.replaceChildren(); console.error(error);
        }
        document.documentElement.classList.remove('static-loading');
        languages(); await scripts();
        window.dispatchEvent(new Event('resize'));
        document.dispatchEvent(new Event('koi:static-ready'));
    }
    start().catch(error => console.error(error));
})();
