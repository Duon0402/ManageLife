const ajaxService = {
    _refreshingTokenPromise: null,

    request: async function (url, method, data = null, options = {}) {
        const settings = {
            contentType: 'application/json',
            dataType: 'json',
            processData: true,
            showLoading: true,
            showToast: true,
            beforeSend: null,
            onProgress: null,
            onSuccess: null,
            onError: null,
            onComplete: null,
            ...options
        };

        const isFormData = data instanceof FormData;
        if (isFormData) { settings.contentType = false; settings.processData = false; }
        if (settings.showLoading) showLoading();

        const doRequest = () => new Promise((resolve, reject) => {
            $.ajax({
                url,
                method,
                data: isFormData ? data : (data ? JSON.stringify(data) : null),
                contentType: settings.contentType,
                processData: settings.processData,
                dataType: settings.dataType,
                xhrFields: { withCredentials: true },
                beforeSend: settings.beforeSend,
                xhr: function () {
                    const xhr = new window.XMLHttpRequest();
                    if (xhr.upload && settings.onProgress)
                        xhr.upload.addEventListener('progress', e => {
                            if (e.lengthComputable)
                                settings.onProgress(Math.round(e.loaded / e.total * 100), e);
                        });
                    return xhr;
                },
                success: function (res) {
                    res.isOk = () => res.code === '00';
                    res.isException = () => res.code === '99';
                    res.isError = () => !res.isOk() && !res.isException();
                    if (settings.onSuccess) settings.onSuccess(res);
                    else if (settings.showToast && res.message) showToast(res.message, 'Thông báo', res.isOk() ? 'success' : 'error');
                    resolve(res);
                },
                error: async function (jqXHR) {
                    if (jqXHR.status === 401) {
                        const refreshed = await ajaxService._handle401();
                        if (refreshed)
                            return resolve(await ajaxService.request(url, method, data, options));
                        ajaxService.redirectToLogin();
                        return reject(new Error("Unauthorized"));
                    }
                    if (settings.onError) settings.onError(jqXHR);
                    else if (settings.showToast) showToast(jqXHR.responseJSON?.message || 'Đã có lỗi xảy ra', 'Thông báo', 'error');
                    reject(jqXHR);
                },
                complete: settings.onComplete
            });
        });

        try { return await doRequest(); }
        finally { if (settings.showLoading) hideLoading(); }
    },

    get: (url, params = {}, options = {}) => ajaxService.request(params ? `${url}?${$.param(params)}` : url, 'GET', null, options),
    post: (url, data, options = {}) => ajaxService.request(url, 'POST', data, options),
    put: (url, data, options = {}) => ajaxService.request(url, 'PUT', data, options),
    delete: (url, options = {}) => ajaxService.request(url, 'DELETE', null, options),
    upload: (url, formData, options = {}) => {
        if (!(formData instanceof FormData)) throw new Error("upload() expects FormData");
        return ajaxService.request(url, 'POST', formData, options);
    },

    _handle401: async function () {
        if (this._refreshingTokenPromise) return await this._refreshingTokenPromise;
        this._refreshingTokenPromise = (async () => {
            try {
                const res = await fetch('/Auth/RefreshToken', { method: 'POST', credentials: 'include' });
                if (!res.ok) return false;
                const data = await res.json();
                return data.code === '00';
            } catch { return false; }
            finally { this._refreshingTokenPromise = null; }
        })();
        return await this._refreshingTokenPromise;
    },

    redirectToLogin: function () {
        sessionStorage.setItem('returnUrl', window.location.href);
        window.location.href = '/Auth/Login?returnUrl=' + encodeURIComponent(window.location.href);
    }
};
