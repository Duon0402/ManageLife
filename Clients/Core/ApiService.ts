namespace App {
    export interface ApiResponse<T = any> {
        code: string;
        message: string;
        data: T;
        isOk: () => boolean;
        isException: () => boolean;
        isError: () => boolean;
    }

    export interface ApiRequestOptions extends JQuery.AjaxSettings {
        showLoading?: boolean;
        showToast?: boolean;
        onProgress?: (percent: number, event: ProgressEvent) => void;
    }

    export class ApiService {
        private static get DEFAULT_OPTIONS(): ApiRequestOptions {
            return {
                contentType: Constants.Defaults.CONTENT_TYPE,
                dataType: Constants.Defaults.DATA_TYPE,
                processData: true,
                showLoading: true,
                showToast: true
            };
        }

        public static get<T = any>(url: string, params: any = {}, options: ApiRequestOptions = {}): Promise<ApiResponse<T>> {
            const requestUrl = params ? `${url}?${$.param(params)}` : url;
            return this.request<T>(requestUrl, Constants.HttpMethod.GET, null, options);
        }

        public static post<T = any>(url: string, data: any, options: ApiRequestOptions = {}): Promise<ApiResponse<T>> {
            return this.request<T>(url, Constants.HttpMethod.POST, data, options);
        }

        public static put<T = any>(url: string, data: any, options: ApiRequestOptions = {}): Promise<ApiResponse<T>> {
            return this.request<T>(url, Constants.HttpMethod.PUT, data, options);
        }

        public static delete<T = any>(url: string, options: ApiRequestOptions = {}): Promise<ApiResponse<T>> {
            return this.request<T>(url, Constants.HttpMethod.DELETE, null, options);
        }

        public static upload<T = any>(url: string, formData: FormData, options: ApiRequestOptions = {}): Promise<ApiResponse<T>> {
            if (!(formData instanceof FormData)) {
                throw new Error("upload() expects FormData");
            }
            return this.request<T>(url, Constants.HttpMethod.POST, formData, {
                ...options,
                contentType: false,
                processData: false
            });
        }

        private static request<T>(url: string, method: string, data: any, options: ApiRequestOptions): Promise<ApiResponse<T>> {
            const settings = { ...this.DEFAULT_OPTIONS, ...options };

            if (data instanceof FormData) {
                settings.contentType = false;
                settings.processData = false;
            }

            if (settings.showLoading) {
                LoadingService.show();
            }

            return new Promise<ApiResponse<T>>((resolve, reject) => {
                $.ajax({
                    url: url,
                    method: method,
                    data: (settings.contentType === false || !settings.contentType) ? data : (data ? JSON.stringify(data) : null),
                    contentType: settings.contentType,
                    processData: settings.processData,
                    dataType: settings.dataType,
                    xhrFields: { withCredentials: true },
                    beforeSend: settings.beforeSend,
                    xhr: function () {
                        const xhr = new window.XMLHttpRequest();
                        if (xhr.upload && settings.onProgress) {
                            xhr.upload.addEventListener('progress', (e: ProgressEvent) => {
                                if (e.lengthComputable && settings.onProgress) {
                                    settings.onProgress(Math.round(e.loaded / e.total * 100), e);
                                }
                            });
                        }
                        return xhr;
                    },
                    success: (res: any, textStatus: any, jqXHR: JQuery.jqXHR) => {
                        const apiResponse = res as ApiResponse<T>;
                        apiResponse.isOk = () => apiResponse.code === Constants.ApiCode.SUCCESS;
                        apiResponse.isException = () => apiResponse.code === Constants.ApiCode.EXCEPTION;
                        apiResponse.isError = () => !apiResponse.isOk() && !apiResponse.isException();

                        if (settings.success) {
                            if (Array.isArray(settings.success)) {
                                settings.success.forEach(fn => fn(res, textStatus, jqXHR));
                            } else {
                                settings.success(res, textStatus, jqXHR);
                            }
                        } else if (settings.showToast && apiResponse.message) {

                            if (!apiResponse.isOk()) {
                                ToastService.error(apiResponse.message || Constants.Messages.DEFAULT_ERROR, Constants.Messages.NOTIFICATION_TITLE);
                            } else {
                                ToastService.success(apiResponse.message, Constants.Messages.NOTIFICATION_TITLE);
                            }
                        }
                        resolve(apiResponse);
                    },
                    error: (jqXHR: JQuery.jqXHR) => {
                        if (jqXHR.status === Constants.HttpStatus.UNAUTHORIZED) {
                            window.location.href = Constants.Urls.LOGIN + "?returnUrl=" +
                                encodeURIComponent(window.location.pathname + window.location.search);
                            return;
                        }

                        if (settings.error) {
                            if (Array.isArray(settings.error)) {
                                settings.error.forEach((fn: any) => fn(jqXHR, 'error', jqXHR.statusText));
                            } else {
                                settings.error(jqXHR, 'error', jqXHR.statusText);
                            }
                        }

                        if (jqXHR.responseJSON?.code === Constants.ApiCode.FORBIDDEN) {
                            ToastService.error(Constants.Messages.FORBIDDEN, Constants.Messages.NOTIFICATION_TITLE);
                        } else {
                            const msg = jqXHR.responseJSON?.message || Constants.Messages.DEFAULT_ERROR;
                            ToastService.error(msg, Constants.Messages.NOTIFICATION_TITLE);
                        }

                        reject(jqXHR);
                    },
                    complete: (jqXHR, textStatus) => {
                        if (settings.showLoading) {
                            LoadingService.hide();
                        }
                        if (settings.complete) {
                            if (Array.isArray(settings.complete)) {
                                settings.complete.forEach(fn => fn(jqXHR, textStatus));
                            } else {
                                settings.complete(jqXHR, textStatus);
                            }
                        }
                    }
                });
            });
        }
    }
}
