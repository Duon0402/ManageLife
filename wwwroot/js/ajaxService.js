const ajaxService = {
    _refreshingTokenPromise: null, // Lock refresh

    request: async function (url, method, data = null, options = {}) {
        const defaultOptions = {
            contentType: 'application/json',
            dataType: 'json',
            processData: true,
            showLoading: true,
            showToast: true,
            isRetry: false,
            headers: {},
            beforeSend: null,
            onProgress: null,
            onSuccess: null,
            onError: null,
            onComplete: null
        };

        const settings = { ...defaultOptions, ...options };
        const isFormData = (data instanceof FormData);

        // Gắn token nếu chưa có
        const token = localStorage.getItem('accessToken');
        if (token && !settings.headers['Authorization']) {
            settings.headers['Authorization'] = 'Bearer ' + token;
        }

        if (isFormData) {
            settings.contentType = false;
            settings.processData = false;
        }

        if (settings.showLoading) showLoading();

        const doRequest = () => new Promise((resolve, reject) => {
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
                    response.isOk = () => response.code === '00';
                    response.isException = () => response.code === '99';
                    response.isError = () => !response.isOk() && !response.isException();

                    if (typeof settings.onSuccess === 'function') {
                        settings.onSuccess(response);
                    } else if (!settings.isRetry && settings.showToast && response.message) {
                        showToast(response.message, 'Thông báo', response.isOk() ? 'success' : 'error');
                    }

                    resolve(response);
                },
                error: async (jqXHR) => {
                    const errorMessage = jqXHR.responseJSON?.message || 'Đã có lỗi xảy ra.';

                    // Nếu lỗi 401 thì xử lý refresh token
                    if (jqXHR.status === 401) {
                        try {
                            const refreshed = await ajaxService._handle401();
                            if (refreshed) {
                                // Retry nhưng không show loading/toast lại
                                const newToken = localStorage.getItem('accessToken');
                                settings.headers['Authorization'] = 'Bearer ' + newToken;
                                const retryResult = await ajaxService.request(url, method, data, {
                                    ...options,
                                    isRetry: true,
                                    showLoading: false,
                                    showToast: false
                                });
                                return resolve(retryResult);
                            }
                            ajaxService.redirectToLogin();
                            return;
                        } catch {
                            ajaxService.redirectToLogin();
                            return;
                        }
                    }

                    if (typeof settings.onError === 'function') {
                        settings.onError(jqXHR);
                    } else if (!settings.isRetry) {
                        showToast(errorMessage, 'Thông báo', 'error');
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

        try {
            return await doRequest();
        } finally {
            if (settings.showLoading) hideLoading();
        }
    },

    // Các method tiện dụng
    get: async function (url, params = {}, options = {}) {
        const queryString = $.param(params);
        const finalUrl = queryString ? `${url}?${queryString}` : url;
        return await this.request(finalUrl, 'GET', null, options);
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
        if (!(formData instanceof FormData)) {
            throw new Error("upload() expects FormData");
        }
        return await this.request(url, 'POST', formData, options);
    },

    // Hàm xử lý refresh token có lock
    _handle401: async function () {
        // Nếu đang refresh, chờ kết quả
        if (this._refreshingTokenPromise) {
            return await this._refreshingTokenPromise;
        }

        // Tạo promise refresh mới
        this._refreshingTokenPromise = new Promise(async (resolve) => {
            const success = await this._refreshToken();
            this._refreshingTokenPromise = null; // Reset lock
            resolve(success);
        });

        return await this._refreshingTokenPromise;
    },

    // Refresh token bằng fetch để tránh vòng lặp
    _refreshToken: async function () {
        const refreshToken = localStorage.getItem('refreshToken');
        if (!refreshToken) return false;

        try {
            const response = await fetch('/Auth/RefreshToken', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ refreshToken })
            });

            if (!response.ok) return false;

            const res = await response.json();
            if (res.code === '00') {
                localStorage.setItem('accessToken', res.data.accessToken);
                localStorage.setItem('refreshToken', res.data.refreshToken);
                return true;
            }
            return false;
        } catch {
            return false;
        }
    },

    // Chuyển sang login khi không thể refresh token
    redirectToLogin: function () {
        const currentUrl = window.location.href;
        sessionStorage.setItem('returnUrl', currentUrl);
        window.location.href = '/Auth/Login?returnUrl=' + encodeURIComponent(currentUrl);
    }
};
