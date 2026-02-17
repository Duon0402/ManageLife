var App;
(function (App) {
    class ApiService {
        static get DEFAULT_OPTIONS() {
            return {
                contentType: App.Constants.Defaults.CONTENT_TYPE,
                dataType: App.Constants.Defaults.DATA_TYPE,
                processData: true,
                showLoading: true,
                showToast: true
            };
        }
        static get(url, params = {}, options = {}) {
            const requestUrl = params ? `${url}?${$.param(params)}` : url;
            return this.request(requestUrl, App.Constants.HttpMethod.GET, null, options);
        }
        static post(url, data, options = {}) {
            return this.request(url, App.Constants.HttpMethod.POST, data, options);
        }
        static put(url, data, options = {}) {
            return this.request(url, App.Constants.HttpMethod.PUT, data, options);
        }
        static delete(url, options = {}) {
            return this.request(url, App.Constants.HttpMethod.DELETE, null, options);
        }
        static upload(url, formData, options = {}) {
            if (!(formData instanceof FormData)) {
                throw new Error("upload() expects FormData");
            }
            return this.request(url, App.Constants.HttpMethod.POST, formData, Object.assign(Object.assign({}, options), { contentType: false, processData: false }));
        }
        static request(url, method, data, options) {
            const settings = Object.assign(Object.assign({}, this.DEFAULT_OPTIONS), options);
            if (data instanceof FormData) {
                settings.contentType = false;
                settings.processData = false;
            }
            if (settings.showLoading) {
                App.LoadingService.show();
            }
            return new Promise((resolve, reject) => {
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
                            xhr.upload.addEventListener('progress', (e) => {
                                if (e.lengthComputable && settings.onProgress) {
                                    settings.onProgress(Math.round(e.loaded / e.total * 100), e);
                                }
                            });
                        }
                        return xhr;
                    },
                    success: (res, textStatus, jqXHR) => {
                        const apiResponse = res;
                        apiResponse.isOk = () => apiResponse.code === App.Constants.ApiCode.SUCCESS;
                        apiResponse.isException = () => apiResponse.code === App.Constants.ApiCode.EXCEPTION;
                        apiResponse.isError = () => !apiResponse.isOk() && !apiResponse.isException();
                        if (settings.success) {
                            if (Array.isArray(settings.success)) {
                                settings.success.forEach(fn => fn(res, textStatus, jqXHR));
                            }
                            else {
                                settings.success(res, textStatus, jqXHR);
                            }
                        }
                        else if (settings.showToast && apiResponse.message) {
                            if (!apiResponse.isOk()) {
                                App.ToastService.error(apiResponse.message || App.Constants.Messages.DEFAULT_ERROR, App.Constants.Messages.NOTIFICATION_TITLE);
                            }
                            else {
                                App.ToastService.success(apiResponse.message, App.Constants.Messages.NOTIFICATION_TITLE);
                            }
                        }
                        resolve(apiResponse);
                    },
                    error: (jqXHR) => {
                        var _a, _b;
                        if (jqXHR.status === App.Constants.HttpStatus.UNAUTHORIZED) {
                            window.location.href = App.Constants.Urls.LOGIN + "?returnUrl=" +
                                encodeURIComponent(window.location.pathname + window.location.search);
                            return;
                        }
                        if (settings.error) {
                            if (Array.isArray(settings.error)) {
                                settings.error.forEach((fn) => fn(jqXHR, 'error', jqXHR.statusText));
                            }
                            else {
                                settings.error(jqXHR, 'error', jqXHR.statusText);
                            }
                        }
                        if (((_a = jqXHR.responseJSON) === null || _a === void 0 ? void 0 : _a.code) === App.Constants.ApiCode.FORBIDDEN) {
                            App.ToastService.error(App.Constants.Messages.FORBIDDEN, App.Constants.Messages.NOTIFICATION_TITLE);
                        }
                        else {
                            const msg = ((_b = jqXHR.responseJSON) === null || _b === void 0 ? void 0 : _b.message) || App.Constants.Messages.DEFAULT_ERROR;
                            App.ToastService.error(msg, App.Constants.Messages.NOTIFICATION_TITLE);
                        }
                        reject(jqXHR);
                    },
                    complete: (jqXHR, textStatus) => {
                        if (settings.showLoading) {
                            App.LoadingService.hide();
                        }
                        if (settings.complete) {
                            if (Array.isArray(settings.complete)) {
                                settings.complete.forEach(fn => fn(jqXHR, textStatus));
                            }
                            else {
                                settings.complete(jqXHR, textStatus);
                            }
                        }
                    }
                });
            });
        }
    }
    App.ApiService = ApiService;
})(App || (App = {}));
var App;
(function (App) {
    class BasePage {
        constructor(rootSelector, model) {
            if (!rootSelector) {
                throw new Error("rootSelector is required");
            }
            this.root = $(rootSelector);
            this.model = model || {};
            this.initialize();
            this.bindEvents();
        }
        initialize() {
        }
        bindEvents() {
        }
    }
    App.BasePage = BasePage;
})(App || (App = {}));
var App;
(function (App) {
    var Constants;
    (function (Constants) {
        Constants.HttpMethod = {
            GET: 'GET',
            POST: 'POST',
            PUT: 'PUT',
            DELETE: 'DELETE'
        };
        Constants.ApiCode = {
            SUCCESS: '00',
            EXCEPTION: '99',
            FORBIDDEN: '403'
        };
        Constants.HttpStatus = {
            UNAUTHORIZED: 401
        };
        Constants.Messages = {
            DEFAULT_ERROR: 'Đã có lỗi xảy ra',
            FORBIDDEN: 'Bạn không có quyền thực hiện chức năng này',
            NOTIFICATION_TITLE: 'Thông báo'
        };
        Constants.Urls = {
            LOGIN: '/Auth/Login'
        };
        Constants.Defaults = {
            CONTENT_TYPE: 'application/json',
            DATA_TYPE: 'json'
        };
    })(Constants = App.Constants || (App.Constants = {}));
})(App || (App = {}));
var App;
(function (App) {
    class LoadingService {
        static show() {
            if (++this.count > 1)
                return;
            $("body").append(`
                <div id="${this.ID}" style="position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(0,0,0,.5);display:flex;justify-content:center;align-items:center;z-index:10000">
                    <div style="border:4px solid #f3f3f3;border-top:4px solid #3498db;border-radius:50%;width:50px;height:50px;animation:app-spin 1s linear infinite">
                    </div>
                </div>
            `);
            if (!$("#app-loading-style").length)
                $("head").append(`<style id="app-loading-style">@keyframes app-spin{0%{transform:rotate(0)}100%{transform:rotate(360deg)}}</style>`);
        }
        static hide() {
            if (this.count === 0 || --this.count > 0)
                return;
            $("#" + this.ID).remove();
        }
    }
    LoadingService.ID = "loadingOverlay";
    LoadingService.count = 0;
    App.LoadingService = LoadingService;
})(App || (App = {}));
var App;
(function (App) {
    class ToastService {
        static show(options) {
            var _a, _b, _c;
            const title = (_a = options.title) !== null && _a !== void 0 ? _a : this.DEFAULT_TITLE;
            const type = (_b = options.type) !== null && _b !== void 0 ? _b : 'info';
            const duration = (_c = options.duration) !== null && _c !== void 0 ? _c : this.DEFAULT_DURATION;
            const config = this.CONFIG[type];
            const toastId = this.generateId();
            this.ensureContainer();
            const html = this.buildHtml(toastId, options.message, title, config);
            const $container = $("#" + this.CONTAINER_ID);
            $container.prepend(html);
            const $toast = $("#" + toastId);
            new bootstrap.Toast($toast[0], { delay: duration }).show();
            $toast.on("hidden.bs.toast", () => $toast.remove());
        }
        static buildHtml(toastId, message, title, config) {
            return `
                <div id="${toastId}" class="toast border-0 mb-2" role="alert" aria-live="assertive" aria-atomic="true" style="min-width:300px;">
                    <div class="toast-header text-white" style="background-color:${config.color};">
                        <i class="bi ${config.icon} me-2"></i>
                        <strong class="me-auto">${title}</strong>
                        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast" aria-label="Close"></button>
                    </div>
                    <div class="toast-body" style="white-space: pre-wrap;">${message}</div>
                </div>
            `;
        }
        static generateId() {
            return "toast-" + Date.now();
        }
        static ensureContainer() {
            if ($("#" + this.CONTAINER_ID).length)
                return;
            $("body").append(`<div id="${this.CONTAINER_ID}" class="position-fixed top-0 end-0 p-3" style="z-index:9999;"></div>`);
        }
        static success(message, title) {
            this.show({ message, title, type: "success" });
        }
        static error(message, title) {
            this.show({ message, title, type: "error" });
        }
        static warning(message, title) {
            this.show({ message, title, type: "warning" });
        }
        static info(message, title) {
            this.show({ message, title, type: "info" });
        }
    }
    ToastService.DEFAULT_TITLE = "Thông báo";
    ToastService.DEFAULT_DURATION = 3000;
    ToastService.CONTAINER_ID = "toast-container";
    ToastService.CONFIG = {
        success: { color: "#28A745", icon: "bi-check-circle-fill" },
        error: { color: "#DC3545", icon: "bi-x-circle-fill" },
        warning: { color: "#FFC107", icon: "bi-exclamation-triangle-fill" },
        info: { color: "#17A2B8", icon: "bi-info-circle-fill" },
    };
    App.ToastService = ToastService;
})(App || (App = {}));
var App;
(function (App) {
    class AdminLanguagePage extends App.BasePage {
        constructor() {
            super(...arguments);
            this.languageModalMode = 'create';
            this.editingLanguageId = null;
        }
        initialize() {
            this.initTable();
            this.loadData();
        }
        bindEvents() {
            this.root.find('#tblLanguage').on('click', '.btn-edit', (e) => {
                const btn = $(e.currentTarget);
                const rowData = this.table.row(btn.closest('tr')).data();
                this.editLanguage(rowData);
            });
            this.root.find('#tblLanguage').on('click', '.btn-delete', (e) => {
                const btn = $(e.currentTarget);
                const rowData = this.table.row(btn.closest('tr')).data();
                // Implement delete logic if needed, previously it just logged
                console.log("DELETE language:", rowData);
            });
            // Modal buttons (assuming there is a save button in the modal, but the View logic didn't show it explicitly, maybe it's in the partial)
            // The original View didn't show the save logic! It only showed open/reset.
            // I will implement open/reset logic as per original view.
        }
        loadData() {
            App.LoadingService.show();
            App.ApiService.get('/Admin/Language/GetListLanguages').then(response => {
                App.LoadingService.hide();
                if (response.isOk()) {
                    const data = response.data || [];
                    this.table.clear();
                    this.table.rows.add(data);
                    this.table.draw();
                }
                else {
                    App.ToastService.error(response.message || 'Error');
                }
            }).catch(() => {
                App.LoadingService.hide();
                App.ToastService.error('Không thể tải danh sách user');
            });
        }
        initTable() {
            this.table = this.root.find("#tblLanguage").DataTable({
                buttons: [
                    {
                        text: '<i class="fa-solid fa-plus"></i>',
                        className: 'btn btn-sm btn-outline-secondary',
                        titleAttr: 'Thêm mới',
                        action: () => {
                            this.createLanguage();
                        }
                    }
                ],
                layout: {
                    topEnd: [
                        'buttons',
                        'search'
                    ],
                },
                scrollY: 'calc(100vh - 395px)',
                scrollCollapse: true,
                destroy: true,
                data: [],
                paging: false,
                info: false,
                ordering: false,
                searching: true,
                autoWidth: true,
                columns: [
                    { title: "ID", data: "id", visible: false },
                    { title: "Code", data: "code" },
                    { title: "Name", data: "name" },
                    {
                        title: "",
                        data: null,
                        orderable: false,
                        searchable: false,
                        className: "text-center",
                        width: "120px",
                        render: (data, type, row) => {
                            return `
                                <button class="btn btn-sm btn-outline-primary btn-edit"
                                        data-id="${row.id}"
                                        title="Sửa">
                                    <i class="fa-solid fa-pen-to-square"></i>
                                </button>
                                <button class="btn btn-sm btn-outline-danger btn-delete"
                                        data-id="${row.id}"
                                        title="Xóa">
                                    <i class="fa-solid fa-trash"></i>
                                </button>
                            `;
                        }
                    }
                ]
            });
        }
        createLanguage() {
            this.openLanguageModal(null);
        }
        editLanguage(language) {
            this.openLanguageModal(language);
        }
        openLanguageModal(data) {
            this.resetLanguageForm();
            if (data) {
                this.languageModalMode = 'edit';
                this.editingLanguageId = data.id;
                $('#languageModalLabel').text('Cập nhật ngôn ngữ');
                $('#txtCode').val(data.code);
                $('#txtName').val(data.name);
            }
            else {
                this.languageModalMode = 'create';
                this.editingLanguageId = null;
                $('#languageModalLabel').text('Thêm ngôn ngữ');
            }
            $('#languageModal').modal('show');
        }
        resetLanguageForm() {
            $('#txtCode').val('');
            $('#txtName').val('');
        }
    }
    App.AdminLanguagePage = AdminLanguagePage;
})(App || (App = {}));
var App;
(function (App) {
    class AdminTranslationPage extends App.BasePage {
        initialize() {
            this.loadTranslations();
        }
        bindEvents() {
            this.root.find('#btnLoad').on('click', () => this.loadTranslations());
            this.root.find('#btnCreate').on('click', () => this.createTranslation());
        }
        loadTranslations() {
            const languageId = this.root.find("#ddlLanguageId").val();
            const req = {
                languageId: languageId
            };
            App.LoadingService.show();
            App.ApiService.post('/Admin/Translation/GetListTranslations', req, {
                showLoading: false, // handled manually
                success: (response) => {
                    App.LoadingService.hide();
                    if (response.isOk()) {
                        const tbody = this.root.find('#tblTranslations tbody');
                        tbody.empty();
                        (response.data || []).forEach((t) => {
                            const row = `
                                <tr>
                                    <td>${t.key}</td>
                                    <td>${t.value}</td>
                                    <td>${t.languageName}</td>
                                </tr>
                            `;
                            tbody.append(row);
                        });
                    }
                    else {
                        App.ToastService.error(response.message || 'Lấy danh sách thất bại');
                    }
                },
                error: () => {
                    App.LoadingService.hide();
                    App.ToastService.error('Lỗi hệ thống');
                }
            });
        }
        createTranslation() {
            const key = this.root.find("#txtKey").val();
            const value = this.root.find("#txtValue").val();
            const languageId = this.root.find("#ddlLanguageId").val();
            if (!key || !value || !languageId) {
                App.ToastService.warning("Vui lòng nhập đầy đủ thông tin");
                return;
            }
            const req = {
                key: key,
                value: value,
                languageId: languageId
            };
            App.LoadingService.show();
            App.ApiService.post('/Admin/Translation/CreateTranslation', req, {
                showLoading: false,
                success: (response) => {
                    App.LoadingService.hide();
                    if (response.isOk()) {
                        App.ToastService.success("Thêm thành công");
                        this.loadTranslations();
                        this.root.find("#txtKey").val('');
                        this.root.find("#txtValue").val('');
                        // keep language selection
                    }
                    else {
                        App.ToastService.error(response.message || 'Thêm thất bại');
                    }
                },
                error: () => {
                    App.LoadingService.hide();
                    App.ToastService.error('Lỗi hệ thống');
                }
            });
        }
    }
    App.AdminTranslationPage = AdminTranslationPage;
})(App || (App = {}));
//# sourceMappingURL=app.js.map