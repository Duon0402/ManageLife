var App;
(function (App) {
    class BasePage {
        constructor(rootSelector) {
            this.root = rootSelector ? $(rootSelector) : $("body");
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
        initialize() {
        }
        bindEvents() {
        }
    }
    App.AdminLanguagePage = AdminLanguagePage;
})(App || (App = {}));
var App;
(function (App) {
    class AdminTranslationPage extends App.BasePage {
        initialize() {
            App.LoadingService.show();
            App.ToastService.success("Page loaded");
            App.LoadingService.hide();
        }
    }
    App.AdminTranslationPage = AdminTranslationPage;
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
    class AjaxService {
    }
    App.AjaxService = AjaxService;
})(App || (App = {}));
//# sourceMappingURL=app.js.map