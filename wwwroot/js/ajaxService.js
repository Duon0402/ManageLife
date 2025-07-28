const ajaxService = {
    request: async function (url, method, data = null, options = {}) {
        const defaultOptions = {
            contentType: 'application/json',
            dataType: 'json',
            processData: true,
            showLoading: true,
            headers: {},
            beforeSend: null,
            onProgress: null,
            onSuccess: null,
            onError: null,
            onComplete: null
        };

        const settings = { ...defaultOptions, ...options };
        const isFormData = (data instanceof FormData);

        // Tự động gắn token vào request nếu chưa có
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
                    } else if (response.message) {
                        showToast(response.message, 'Thông báo', response.code === '00' ? 'success' : 'error');
                    }

                    resolve(response);
                },
                error: async function (jqXHR) {
                    const errorMessage = jqXHR.responseJSON?.message || 'Đã có lỗi xảy ra.';

                    // Nếu token hết hạn (401 Unauthorized)
                    if (jqXHR.status === 401) {
                        try {
                            const refreshed = await ajaxService.refreshToken();
                            if (refreshed) {
                                // Gọi lại request gốc với token mới
                                const retryResult = await ajaxService.request(url, method, data, options);
                                return resolve(retryResult);
                            }
                            // Nếu refresh thất bại, về trang login
                            ajaxService.redirectToLogin();
                            return;
                        } catch (err) {
                            ajaxService.redirectToLogin();
                            return;
                        }
                    }

                    if (typeof settings.onError === 'function') {
                        settings.onError(jqXHR);
                    } else {
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

        return doRequest().finally(() => {
            if (settings.showLoading) hideLoading();
        });
    },

    // Các method tiện dụng
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
    },

    // Refresh token API
    refreshToken: async function () {
        const refreshToken = localStorage.getItem('refreshToken');
        if (!refreshToken) return false;

        try {
            const res = await this.post('/Auth/RefreshToken', { refreshToken }, { showLoading: false });
            if (res.isOk()) {
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
