const ajaxService = {
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
                    else if (settings.showToast && res.message)
                        showToast(res.message, 'Thông báo', res.isOk() ? 'success' : 'error');
                    resolve(res);
                },
                error: function (jqXHR) {
                    if (jqXHR.status === 401) {
                        window.location.href = "/Auth/Login?returnUrl=" +
                            encodeURIComponent(window.location.pathname + window.location.search);
                        return;
                    }

                    if (settings.onError) settings.onError(jqXHR);
                    reject(jqXHR);
                },
                complete: settings.onComplete
            });
        });

        try {
            return await doRequest();
        }
        catch (error) {
            if (error.responseJSON?.code === "403") {
                showToast("Bạn không có quyền thực hiện chức năng này", 'Thông báo', 'error');
                return;
            }
            showToast(error.responseJSON?.message || 'Đã có lỗi xảy ra', 'Thông báo', 'error');
        }
        finally {
            if (settings.showLoading) hideLoading();
        }
    },

    get: (url, params = {}, options = {}) => ajaxService.request(params ? `${url}?${$.param(params)}` : url, 'GET', null, options),
    post: (url, data, options = {}) => ajaxService.request(url, 'POST', data, options),
    put: (url, data, options = {}) => ajaxService.request(url, 'PUT', data, options),
    delete: (url, options = {}) => ajaxService.request(url, 'DELETE', null, options),
    upload: (url, formData, options = {}) => {
        if (!(formData instanceof FormData)) throw new Error("upload() expects FormData");
        return ajaxService.request(url, 'POST', formData, options);
    }
};
