namespace App {
    export type ToastType = 'success' | 'error' | 'info' | 'warning';

    interface ToastConfig {
        color: string;
        icon: string;
    }

    interface ShowToastOptions {
        message: string;
        title?: string;
        type?: ToastType;
        duration?: number; // in milliseconds
    }

    export class ToastService {
        private static readonly DEFAULT_TITLE = "Thông báo";
        private static readonly DEFAULT_DURATION = 3000;
        private static readonly CONTAINER_ID = "toast-container";

        private static readonly CONFIG: Record<ToastType, ToastConfig> = {
            success: { color: "#28A745", icon: "bi-check-circle-fill" },
            error: { color: "#DC3545", icon: "bi-x-circle-fill" },
            warning: { color: "#FFC107", icon: "bi-exclamation-triangle-fill" },
            info: { color: "#17A2B8", icon: "bi-info-circle-fill" },
        }

        public static show(options: ShowToastOptions): void {
            const title = options.title ?? this.DEFAULT_TITLE;
            const type = options.type ?? 'info';
            const duration = options.duration ?? this.DEFAULT_DURATION;
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

        private static buildHtml(toastId: string, message: string, title: string, config: ToastConfig): string {
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

        private static generateId(): string {
            return "toast-" + Date.now();
        }

        private static ensureContainer(): void {
            if ($("#" + this.CONTAINER_ID).length) return;
            $("body").append(`<div id="${this.CONTAINER_ID}" class="position-fixed top-0 end-0 p-3" style="z-index:9999;"></div>`);
        }

        public static success(message: string, title?: string) {
            this.show({ message, title, type: "success" });
        }

        public static error(message: string, title?: string) {
            this.show({ message, title, type: "error" });
        }

        public static warning(message: string, title?: string) {
            this.show({ message, title, type: "warning" });
        }

        public static info(message: string, title?: string) {
            this.show({ message, title, type: "info" });
        }
    }
}