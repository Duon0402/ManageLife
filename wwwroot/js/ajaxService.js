const ajaxService = {
    request: async function (url, method, data = null, options = {}) {
        const defaultOptions = {
            contentType: 'application/json',
            dataType: 'json',
            processData: true,
            showLoading: true,
            hideLoading: true,
            headers: {},
            beforeSend: null,
            onProgress: null,
            onSuccess: null,
            onError: null,
            onComplete: null
        };

        const settings = { ...defaultOptions, ...options };
        const isFormData = (data instanceof FormData);

        if (isFormData) {
            settings.contentType = false;
            settings.processData = false;
        }

        if (settings.showLoading) showLoading();

        return new Promise((resolve, reject) => {
            $.ajax({
                url: url,
                method: method,
                data: isFormData ? data : (data ? JSON.stringify(data) : null),
                contentType: settings.contentType,
                processData: settings.processData,
                dataType: settings.dataType,
                headers: settings.headers,
                beforeSend: function () {
                    if (typeof settings.beforeSend === 'function') {
                        settings.beforeSend();
                    }
                },
                xhr: function () {
                    const xhr = new window.XMLHttpRequest();
                    if (xhr.upload && typeof settings.onProgress === 'function') {
                        xhr.upload.addEventListener('progress', function (e) {
                            if (e.lengthComputable) {
                                const percent = Math.round((e.loaded / e.total) * 100);
                                settings.onProgress(percent, e);
                            }
                        });
                    }
                    return xhr;
                },
                success: function (response) {
                    if (settings.hideLoading) hideLoading();

                    response.isOk = () => response.code === '00';
                    response.isException = () => response.code === '99';
                    response.isError = () => !response.isOk() && !response.isException();

                    if (typeof settings.onSuccess === 'function') {
                        settings.onSuccess(response);
                    } else if (response.message) {
                        showToast(response.message, 'Thông báo', response.code === '00' ? 'success' : 'error');
                    }

                    resolve(response);
                },
                error: function (jqXHR) {
                    if (settings.hideLoading) hideLoading();
                    if (typeof settings.onError === 'function') {
                        settings.onError(jqXHR);
                    } else {
                        const errorMessage = jqXHR.responseJSON?.message || 'Đã có lỗi xảy ra.';
                        showToast(errorMessage, 'Lỗi');
                    }
                    reject(jqXHR);
                },
                complete: function () {
                    if (typeof settings.onComplete === 'function') {
                        settings.onComplete();
                    }
                }
            });
        });
    },

    get: async function (url, params = {}, options = {}) {
        const queryString = $.param(params);
        return await this.request(`${url}?${queryString}`, 'GET', null, options);
    },

    post: async function (url, data, options = {}) {
        return await this.request(url, 'POST', data, options);
    },

    put: async function (url, data, options = {}) {
        return await this.request(url, 'PUT', data, options);
    },

    delete: async function (url, options = {}) {
        return await this.request(url, 'DELETE', null, options);
    },

    upload: async function (url, formData, options = {}) {
        return await this.request(url, 'POST', formData, options);
    }
};
