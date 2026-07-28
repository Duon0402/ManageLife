namespace App {
    export class AnkiCardPage extends BasePage {
        private modal!: bootstrap.Modal;
        private cards: AnkiCardModel[] = [];
        private editingId: string | null = null;

        protected initialize(): void {
            this.modal = new bootstrap.Modal(document.getElementById('modal-anki-card')!);
            this.loadList();
        }

        protected bindEvents(): void {
            this.root.find('#btn-create-anki-card').on('click', () => this.openCreateModal());
            $('#btn-save-anki-card').on('click', () => this.saveCard());
            this.root.find('#btn-export-anki').on('click', () => AnkiCardService.exportAnki());
            this.root.find('#btn-export-anki-text').on('click', () => AnkiCardService.exportAnkiText());

            $('#anki-card-type').on('change', (e) => {
                const cardType = Number($(e.currentTarget).val()) as AnkiCardType;
                this.applyCardTypeLayout(cardType);
            });

            this.root.on('click', '.btn-edit', (e) => {
                const id = $(e.currentTarget).data('id') as string;
                const card = this.cards.find(c => c.id === id);
                if (card) this.openEditModal(card);
            });

            this.root.on('click', '.btn-delete', (e) => {
                const id = $(e.currentTarget).data('id') as string;
                const card = this.cards.find(c => c.id === id);
                if (card) this.deleteCard(card);
            });
        }

        private async loadList(): Promise<void> {
            LoadingService.show();
            try {
                const res = await AnkiCardService.getList();
                if (!res.isOk()) {
                    this.renderEmpty('Không thể tải danh sách thẻ');
                    return;
                }
                this.cards = res.data || [];
                this.renderTable();
            } catch {
                this.renderEmpty('Không thể tải danh sách thẻ');
                ToastService.error('Lỗi hệ thống');
            } finally {
                LoadingService.hide();
            }
        }

        private renderTable(): void {
            const $tbody = this.root.find('#anki-card-tbody');
            $tbody.empty();

            if (!this.cards.length) {
                this.renderEmpty('Chưa có thẻ nào. Hãy thêm thẻ đầu tiên!');
                return;
            }

            this.cards.forEach(card => {
                const badgeClass = `anki-badge-${card.cardType}`;
                const label = AnkiCardTypeLabel[card.cardType] || 'Không xác định';

                $tbody.append(`
                    <tr>
                        <td data-label="Loại thẻ"><span class="anki-badge ${badgeClass}">${Utils.escapeHtml(label)}</span></td>
                        <td data-label="Mặt trước"><span class="anki-cell-text">${Utils.escapeHtml(card.fieldFront)}</span></td>
                        <td data-label="Mặt sau"><span class="anki-cell-text">${Utils.escapeHtml(card.fieldBack)}</span></td>
                        <td data-label="Ngày tạo">${new Date(card.recordedDate).toLocaleDateString('vi-VN')}</td>
                        <td class="anki-actions-cell" data-label="">
                            <div class="anki-actions">
                                <button class="anki-btn-icon edit btn-edit" data-id="${card.id}" title="Chỉnh sửa">
                                    <i class="fa-solid fa-pen"></i>
                                </button>
                                <button class="anki-btn-icon delete btn-delete" data-id="${card.id}" title="Xóa">
                                    <i class="fa-solid fa-trash"></i>
                                </button>
                            </div>
                        </td>
                    </tr>
                `);
            });
        }

        private renderEmpty(msg: string): void {
            this.root.find('#anki-card-tbody').html(`
                <tr>
                    <td colspan="5" class="text-center py-5 text-muted">
                        <i class="fa-solid fa-layer-group fa-2x mb-2 d-block" style="opacity:.3"></i>
                        ${msg}
                    </td>
                </tr>
            `);
        }

        private applyCardTypeLayout(cardType: AnkiCardType): void {
            const $frontLabel = $('#anki-field-front-label');
            const $backLabel = $('#anki-field-back-label');
            const $extraCheckboxWrap = $('#anki-extra-checkbox-wrap');
            const $extraTextareaWrap = $('#anki-extra-textarea-wrap');

            if (cardType === AnkiCardType.Cloze) {
                // Cloze
                $frontLabel.text('Câu có chỗ trống (dùng ___)');
                $backLabel.text('Đáp án');
                $extraCheckboxWrap.hide();
                $extraTextareaWrap.show();
            } else if (cardType === AnkiCardType.BasicOptionalReversed) {
                // Basic (optional reversed card)
                $frontLabel.text('Mặt trước');
                $backLabel.text('Mặt sau');
                $extraCheckboxWrap.show();
                $extraTextareaWrap.hide();
            } else {
                // Basic / BasicReversed / BasicTypeAnswer
                $frontLabel.text('Mặt trước');
                $backLabel.text('Mặt sau');
                $extraCheckboxWrap.hide();
                $extraTextareaWrap.hide();
            }
        }

        private resetForm(): void {
            $('#anki-id').val('');
            $('#anki-card-type').val(String(AnkiCardType.Basic));
            $('#anki-field-front').val('');
            $('#anki-field-back').val('');
            $('#anki-field-extra').val('');
            $('#anki-source-note').val('');
            ($('#anki-extra-checkbox')[0] as HTMLInputElement).checked = false;
            this.applyCardTypeLayout(AnkiCardType.Basic);
        }

        private openCreateModal(): void {
            this.editingId = null;
            $('#modal-anki-card-title').html('<i class="fa-solid fa-layer-group me-2"></i>Thêm thẻ Anki');
            this.resetForm();
            this.modal.show();
        }

        private openEditModal(card: AnkiCardModel): void {
            this.editingId = card.id;
            $('#modal-anki-card-title').html('<i class="fa-solid fa-pen me-2"></i>Chỉnh sửa thẻ Anki');
            $('#anki-id').val(card.id);
            $('#anki-card-type').val(String(card.cardType));
            $('#anki-field-front').val(card.fieldFront);
            $('#anki-field-back').val(card.fieldBack);
            $('#anki-source-note').val(card.sourceNote || '');

            if (card.cardType === AnkiCardType.BasicOptionalReversed) {
                ($('#anki-extra-checkbox')[0] as HTMLInputElement).checked = card.fieldExtra === 'y';
                $('#anki-field-extra').val('');
            } else if (card.cardType === AnkiCardType.Cloze) {
                $('#anki-field-extra').val(card.fieldExtra || '');
                ($('#anki-extra-checkbox')[0] as HTMLInputElement).checked = false;
            } else {
                $('#anki-field-extra').val('');
                ($('#anki-extra-checkbox')[0] as HTMLInputElement).checked = false;
            }

            this.applyCardTypeLayout(card.cardType);
            this.modal.show();
        }

        private async saveCard(): Promise<void> {
            const cardType = Number($('#anki-card-type').val()) as AnkiCardType;
            const fieldFront = (($('#anki-field-front').val() as string) || '').trim();
            const fieldBack = (($('#anki-field-back').val() as string) || '').trim();
            const sourceNote = (($('#anki-source-note').val() as string) || '').trim() || null;

            if (!fieldFront) {
                ToastService.warning(cardType === AnkiCardType.Cloze
                    ? 'Vui lòng nhập nội dung có chỗ trống'
                    : 'Vui lòng nhập mặt trước');
                return;
            }
            if (!fieldBack) {
                ToastService.warning('Vui lòng nhập mặt sau / đáp án');
                return;
            }
            if (cardType === AnkiCardType.Cloze && !fieldFront.includes('___')) {
                ToastService.warning('Nội dung phải chứa dấu chỗ trống "___"');
                return;
            }

            let fieldExtra: string | null = null;
            if (cardType === AnkiCardType.BasicOptionalReversed) {
                const checked = ($('#anki-extra-checkbox')[0] as HTMLInputElement).checked;
                fieldExtra = checked ? 'y' : null;
            } else if (cardType === AnkiCardType.Cloze) {
                fieldExtra = (($('#anki-field-extra').val() as string) || '').trim() || null;
            }

            LoadingService.show();
            try {
                let res: ApiResponse;

                if (this.editingId) {
                    res = await AnkiCardService.update({
                        id: this.editingId,
                        cardType,
                        fieldFront,
                        fieldBack,
                        fieldExtra,
                        sourceNote
                    });
                } else {
                    res = await AnkiCardService.create({
                        cardType,
                        fieldFront,
                        fieldBack,
                        fieldExtra,
                        sourceNote
                    });
                }

                if (res.isOk()) {
                    this.modal.hide();
                    ToastService.success(this.editingId ? 'Cập nhật thành công' : 'Thêm thẻ thành công');
                    await this.loadList();
                } else {
                    ToastService.error(res.message || 'Lưu thất bại');
                }
            } catch {
                ToastService.error('Lỗi hệ thống');
            } finally {
                LoadingService.hide();
            }
        }

        private async deleteCard(card: AnkiCardModel): Promise<void> {
            await MessageService.confirm(
                `Xóa thẻ <strong>${Utils.escapeHtml(card.fieldFront)}</strong>?`,
                'Xác nhận xóa',
                async () => {
                    LoadingService.show();
                    try {
                        const res = await AnkiCardService.delete(card.id);
                        if (res.isOk()) {
                            ToastService.success('Xóa thành công');
                            await this.loadList();
                        } else {
                            ToastService.error(res.message || 'Xóa thất bại');
                        }
                    } catch {
                        ToastService.error('Lỗi hệ thống');
                    } finally {
                        LoadingService.hide();
                    }
                }
            );
        }
    }
}
