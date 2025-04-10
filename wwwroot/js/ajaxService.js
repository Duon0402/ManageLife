const ajaxService = {
    request: async function (url, method, data = null, options = {}) {
        const defaultOptions = {
            contentType: 'application/json',
            dataType: 'json',
            showLoading: true,
            hideLoading: true,
            onSuccess: null,
            onError: null,
        };
        const settings = { ...defaultOptions, ...options };

        if (settings.showLoading) {
            showLoading();
        }

        return new Promise((resolve, reject) => {
            $.ajax({
                url: url,
                method: method,
                contentType: settings.contentType,
                dataType: settings.dataType,
                data: data ? JSON.stringify(data) : null,
                headers: settings.headers || {},
                success: function (response) {
                    if (settings.hideLoading) {
                        hideLoading();
                    }

                    response.isOk = function () {
                        return response.code === '00';
                    };

                    response.isException = function () {
                        return response.code === '99';
                    };

                    response.isError = function () {
                        return !this.isOk() && !this.isException();
                    };

                    if (settings.onSuccess) {
                        settings.onSuccess(response);
                    }
                    else {
                        if (response.message) {
                            showToast(response.message, 'Thông báo', response.code == '00' ? 'error' : 'success');
                        }
                    }

                    resolve(response);
                },
                error: function (jqXHR) {
                    if (settings.hideLoading) {
                        hideLoading();
                    }
                    if (settings.onError) {
                        settings.onError(jqXHR);
                    }
                    else {
                        const errorMessage = jqXHR.responseJSON?.message || 'Đã có lỗi xảy ra.';
                        showToast(errorMessage, 'error');
                    }

                    reject(jqXHR);
                },
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
};
