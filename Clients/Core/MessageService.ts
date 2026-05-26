namespace App {
    export type MessageType = 'success' | 'error' | 'warning' | 'info' | 'confirm';

    interface MessageConfig {
        icon: string;
        okText: string;
        cancelText: string;
    }

    export class MessageService {
        private static readonly MODAL_ID = "app-message-modal";

        private static readonly CONFIG: Record<MessageType, MessageConfig> = {
            success: { icon: "bi-check-circle-fill", okText: "OK", cancelText: "Hủy" },
            error: { icon: "bi-x-circle-fill", okText: "Đóng", cancelText: "Hủy" },
            warning: { icon: "bi-exclamation-triangle-fill", okText: "OK", cancelText: "Hủy" },
            info: { icon: "bi-info-circle-fill", okText: "OK", cancelText: "Hủy" },
            confirm: { icon: "bi-question-circle-fill", okText: "Xác nhận", cancelText: "Hủy" },
        };

        private static buildHtml(
            id: string,
            title: string,
            message: string,
            type: MessageType,
            isConfirm: boolean,
            okText?: string,
            cancelText?: string,
        ): string {
            const cfg = this.CONFIG[type];
            const ok = okText ?? cfg.okText;
            const cancel = cancelText ?? cfg.cancelText;

            return `
                <div class="modal fade ml-message-modal" id="${id}" tabindex="-1" aria-hidden="true">
                    <div class="modal-dialog modal-dialog-centered">
                        <div class="modal-content">
                            <div class="modal-body">
                                <div class="ml-msg-icon ml-icon-${type}">
                                    <i class="bi ${cfg.icon}"></i>
                                </div>
                                <div class="ml-msg-title">${title}</div>
                                <div class="ml-msg-text">${message}</div>
                            </div>
                            <div class="modal-footer">
                                ${isConfirm
                    ? `<button type="button" class="btn btn-secondary btn-sm" data-bs-dismiss="modal">${cancel}</button>`
                    : ""}
                                <button type="button" class="btn btn-primary btn-sm msg-ok-btn">${ok}</button>
                            </div>
                        </div>
                    </div>
                </div>
            `;
        }

        private static showModal(id: string, isConfirm: boolean): Promise<boolean | void> {
            return new Promise((resolve) => {
                const $modal = $("#" + id);
                const bsModal = new bootstrap.Modal($modal[0], { backdrop: 'static' });

                $modal.on('shown.bs.modal', () => {
                    $modal.find('.msg-ok-btn').off('click').on('click', () => {
                        bsModal.hide();
                        if (isConfirm) resolve(true);
                    });

                    if (isConfirm) {
                        $modal.find('.btn-secondary').off('click').on('click', () => {
                            bsModal.hide();
                            resolve(false);
                        });
                    }
                });

                $modal.on('hidden.bs.modal', () => {
                    $modal.remove();
                    if (!isConfirm) resolve(undefined);
                });

                bsModal.show();
            });
        }

        public static show(message: string, title = "Thông báo", type: MessageType = 'info'): void {
            const id = `${this.MODAL_ID}-${Date.now()}`;
            const html = this.buildHtml(id, title, message, type, false);
            $('body').append(html);
            this.showModal(id, false).catch(() => { /* ignore */ });
        }

        public static async confirm(
            message: string,
            title = "Xác nhận",
            action?: () => Promise<void>,
            okText?: string,
            cancelText?: string,
        ): Promise<boolean | void> {
            const id = `${this.MODAL_ID}-${Date.now()}`;
            const html = this.buildHtml(id, title, message, 'confirm', true, okText, cancelText);
            $('body').append(html);

            const confirmed = await this.showModal(id, true);
            if (!action) return !!confirmed;
            if (confirmed) await action();
        }

        public static success(message: string, title = "Thành công"): void {
            this.show(message, title, "success");
        }

        public static error(message: string, title = "Lỗi"): void {
            this.show(message, title, "error");
        }

        public static warning(message: string, title = "Cảnh báo"): void {
            this.show(message, title, "warning");
        }

        public static info(message: string, title = "Thông báo"): void {
            this.show(message, title, "info");
        }
    }
}
