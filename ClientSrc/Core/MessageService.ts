namespace App {
    export type MessageType = 'success' | 'error' | 'warning' | 'info' | 'confirm';

    interface MessageConfig {
        color: string;
        icon: string;
        okText?: string;
        cancelText?: string;
    }

    export class MessageService {
        private static readonly MODAL_ID = "app-message-modal";

        private static readonly CONFIG: Record<MessageType, MessageConfig> = {
            success: { color: "#28A745", icon: "bi-check-circle-fill", okText: "OK" },
            error: { color: "#DC3545", icon: "bi-x-circle-fill", okText: "Đóng" },
            warning: { color: "#FFC107", icon: "bi-exclamation-triangle-fill", okText: "OK" },
            info: { color: "#17A2B8", icon: "bi-info-circle-fill", okText: "OK" },
            confirm: { color: "#0d6efd", icon: "bi-question-circle-fill", okText: "OK", cancelText: "Hủy" }
        }

        private static buildHtml(id: string, title: string, message: string, config: MessageConfig, isConfirm = false): string {
            const okText = config.okText ?? "OK";
            const cancelText = config.cancelText ?? "Cancel";
            return `
                    <div class="modal fade" id="${id}" tabindex="-1" aria-hidden="true">
                        <div class="modal-dialog modal-dialog-centered" style="max-width:420px;">
                            <div class="modal-content">
                                <div class="modal-header py-2" style="background-color:${config.color};color:#fff;">
                                    <i class="bi ${config.icon} me-2"></i>
                                    <h6 class="modal-title mb-0">${title}</h6>
                                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                                </div>
                                <div class="modal-body" style="white-space:pre-wrap;padding:.75rem 1rem;font-size:.95rem;">${message}</div>
                                <div class="modal-footer py-2">
                                    ${isConfirm ? `<button type="button" class="btn btn-secondary btn-sm" data-bs-dismiss="modal">${cancelText}</button>` : ""}
                                    <button type="button" class="btn btn-primary btn-sm" data-bs-dismiss="modal">${okText}</button>
                                </div>
                            </div>
                        </div>
                    </div>
            `;
        }

        private static showModal(id: string, isConfirm: boolean): Promise<boolean | void> {
            return new Promise((resolve) => {
                const $modal = $("#" + id);
                const modalEl = $modal[0];
                const bsModal = new bootstrap.Modal(modalEl, { backdrop: 'static' });

                if (isConfirm) {
                    $modal.on('shown.bs.modal', () => {
                        $modal.find('.btn-primary').off('click').on('click', () => {
                            bsModal.hide();
                            resolve(true);
                        });
                        $modal.find('.btn-secondary').off('click').on('click', () => {
                            bsModal.hide();
                            resolve(false);
                        });
                    });
                    $modal.on('hidden.bs.modal', () => {
                        $modal.remove();
                    });
                }
                else {
                    $modal.on('shown.bs.modal', () => {
                        $modal.find('.btn-primary').off('click').on('click', () => {
                            bsModal.hide();
                        });
                    });
                    $modal.on('hidden.bs.modal', () => {
                        $modal.remove();
                        resolve(undefined);
                    });
                }

                bsModal.show();
            });
        }

        public static show(message: string, title = "Thông báo", type: MessageType = 'info') {
            const config = this.CONFIG[type] ?? this.CONFIG.info;
            const id = `${this.MODAL_ID}-${Date.now()}`;
            const html = this.buildHtml(id, title, message, config, false);
            $('body').append(html);
            // show and ignore result
            this.showModal(id, false).catch(() => { /* ignore */ });
        }

        public static async confirm(
            message: string,
            title = "Xác nhận",
            action?: () => Promise<void>,
            okText?: string,
            cancelText?: string
        ): Promise<boolean | void> {
            const config: MessageConfig = { ...this.CONFIG.confirm };

            if (okText) config.okText = okText;

            if (cancelText) config.cancelText = cancelText;

            const id = `${this.MODAL_ID}-${Date.now()}`;

            const html = this.buildHtml(id, title, message, config, true);

            $('body').append(html);

            const confirmed = await this.showModal(id, true);

            if (!action) {
                return !!confirmed;
            }

            if (confirmed) {
                await action();
            }
        }

        public static success(
            message: string,
            title = "Thành công"
        ): void {
            this.show(message, title, "success");
        }

        public static error(
            message: string,
            title = "Lỗi"
        ): void {
            this.show(message, title, "error");
        }

        public static warning(
            message: string,
            title = "Cảnh báo"
        ): void {
            this.show(message, title, "warning");
        }

        public static info(
            message: string,
            title = "Thông báo"
        ): void {
            this.show(message, title, "info");
        }
    }
}