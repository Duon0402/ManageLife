namespace App {
    export interface IPopupOptions {
        id?: string;
        title: string;
        bodyHtml: string;
        footerHtml?: string;
        size?: 'sm' | 'md' | 'lg' | 'xl';
        onShow?: (popupBody: JQuery<HTMLElement>) => void;
        onHidden?: () => void;
    }

    export class PopupBuilder {
        private options: IPopupOptions;
        private popupId: string;
        private $popupElement: JQuery<HTMLElement>;
        private bootstrapModal: any; // Using any for bootstrap global

        constructor(options: IPopupOptions) {
            this.options = options;
            this.popupId = options.id || `popup-${Math.random().toString(36).substr(2, 9)}`;
            this.$popupElement = this.generateHtml();
        }

        private generateHtml(): JQuery<HTMLElement> {
            const sizeClass = this.options.size ? `modal-${this.options.size}` : '';
            const html = `
                <div class="modal fade" id="${this.popupId}" tabindex="-1" aria-hidden="true">
                    <div class="modal-dialog modal-dialog-centered ${sizeClass}">
                        <div class="modal-content">
                            <div class="modal-header">
                                <h5 class="modal-title">${this.options.title}</h5>
                                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                            </div>
                            <div class="modal-body">
                                ${this.options.bodyHtml}
                            </div>
                            ${this.options.footerHtml ? `
                            <div class="modal-footer">
                                ${this.options.footerHtml}
                            </div>
                            ` : ''}
                        </div>
                    </div>
                </div>
            `;

            const $element = $(html);
            $('body').append($element);
            return $element;
        }

        public show(): this {
            if (typeof (window as any).bootstrap !== 'undefined') {
                this.bootstrapModal = new (window as any).bootstrap.Modal(this.$popupElement[0]);
                this.bootstrapModal.show();
            } else {
                console.error("Bootstrap is not loaded!");
            }

            if (this.options.onShow) {
                this.$popupElement.on('shown.bs.modal', () => {
                    this.options.onShow!(this.$popupElement.find('.modal-body'));
                });
            }

            this.$popupElement.on('hidden.bs.modal', () => {
                if (this.options.onHidden) {
                    this.options.onHidden();
                }
                this.destroy();
            });

            return this;
        }

        public hide(): this {
            if (this.bootstrapModal) {
                this.bootstrapModal.hide();
            }
            return this;
        }

        public destroy(): void {
            if (this.bootstrapModal) {
                this.bootstrapModal.dispose();
            }
            this.$popupElement.remove();
        }

        public getPopupBody(): JQuery<HTMLElement> {
            return this.$popupElement.find('.modal-body');
        }
    }
}
