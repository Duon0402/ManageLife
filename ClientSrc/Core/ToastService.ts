namespace App {
    export type ToastType = 'success' | 'error' | 'info' | 'warning';

    interface ShowToastOptions {
        message: string;
        title?: string;
        type?: ToastType;
        duration?: number;
    }

    export class ToastService {
        private static readonly DEFAULT_TITLE = "Thông báo";
        private static readonly DEFAULT_DURATION = 3000;
        private static readonly CONTAINER_ID = "ml-toast-container";

        private static readonly ICONS: Record<ToastType, string> = {
            success: "bi-check-circle-fill",
            error: "bi-x-circle-fill",
            warning: "bi-exclamation-triangle-fill",
            info: "bi-info-circle-fill",
        };

        public static show(options: ShowToastOptions): void {
            const title = options.title ?? this.DEFAULT_TITLE;
            const type = options.type ?? 'info';
            const duration = options.duration ?? this.DEFAULT_DURATION;
            const icon = this.ICONS[type];
            const toastId = "ml-toast-" + Date.now();

            this.ensureContainer();

            const html = `
                <div id="${toastId}" class="toast ml-toast ml-toast-${type} border-0"
                     role="alert" aria-live="assertive" aria-atomic="true">
                    <div class="toast-header">
                        <i class="bi ${icon} ml-toast-icon"></i>
                        <strong class="me-auto">${title}</strong>
                        <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
                    </div>
                    <div class="toast-body">${options.message}</div>
                </div>
            `;

            const $container = $("#" + this.CONTAINER_ID);
            $container.prepend(html);

            const $toast = $("#" + toastId);
            new bootstrap.Toast($toast[0], { delay: duration }).show();
            $toast.on("hidden.bs.toast", () => $toast.remove());
        }

        private static ensureContainer(): void {
            if ($("#" + this.CONTAINER_ID).length) return;
            $("body").append(`<div id="${this.CONTAINER_ID}"></div>`);
        }

        public static success(message: string, title?: string): void {
            this.show({ message, title, type: "success" });
        }

        public static error(message: string, title?: string): void {
            this.show({ message, title, type: "error" });
        }

        public static warning(message: string, title?: string): void {
            this.show({ message, title, type: "warning" });
        }

        public static info(message: string, title?: string): void {
            this.show({ message, title, type: "info" });
        }
    }
}
