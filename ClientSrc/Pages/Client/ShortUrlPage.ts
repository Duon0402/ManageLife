namespace App {
    interface ShortUrlModel {
        id: string;
        code: string;
        originalUrl: string;
        title: string | null;
        clickCount: number;
        expireAt: string | null;
        createdTime: string;
    }

    export class ShortUrlPage extends BasePage {
        private modal!: bootstrap.Modal;
        private expirePicker!: DatePickerBuilder;
        private links: ShortUrlModel[] = [];

        protected initialize(): void {
            this.modal = new bootstrap.Modal(document.getElementById('modal-create-link')!);
            this.expirePicker = new DatePickerBuilder('#su-expire-container')
                .withId('su-expire-at')
                .setMinDate(new Date())
                .setFormat('d/m/Y')
                .enableTyping()
                .build();
            this.loadList();
        }

        protected bindEvents(): void {
            this.root.find('#btn-create-link').on('click', () => this.openModal());
            $('#btn-save-link').on('click', () => this.saveLink());

            this.root.on('click', '.btn-copy', (e) => {
                const id = $(e.currentTarget).data('id') as string;
                const link = this.links.find(l => l.id === id);
                if (link) this.copyLink(link);
            });

            this.root.on('click', '.btn-delete', (e) => {
                const id = $(e.currentTarget).data('id') as string;
                const link = this.links.find(l => l.id === id);
                if (link) this.deleteLink(link);
            });
        }

        private shortLink(code: string): string {
            return `${window.location.origin}/r/${code}`;
        }

        private async loadList(): Promise<void> {
            LoadingService.show();
            try {
                const res = await ApiService.get('/ShortUrl/GetList');
                if (!res.isOk()) {
                    this.renderEmpty('Không thể tải danh sách');
                    return;
                }
                this.links = res.data || [];
                this.renderTable();
            } catch {
                this.renderEmpty('Không thể tải danh sách');
                ToastService.error('Lỗi hệ thống');
            } finally {
                LoadingService.hide();
            }
        }

        private renderTable(): void {
            const $tbody = this.root.find('#su-tbody');
            $tbody.empty();

            if (!this.links.length) {
                this.renderEmpty('Chưa có link nào. Hãy tạo link đầu tiên!');
                return;
            }

            this.links.forEach(link => {
                const expire = link.expireAt
                    ? `<span class="su-badge-expire"><i class="fa-regular fa-clock me-1"></i>${new Date(link.expireAt).toLocaleDateString('vi-VN')}</span>`
                    : `<span class="su-badge-no-expire">Không giới hạn</span>`;

                $tbody.append(`
                    <tr>
                        <td>
                            <a href="${this.shortLink(link.code)}" target="_blank" class="su-code">${this.shortLink(link.code)}</a>
                        </td>
                        <td>
                            <a href="${link.originalUrl}" target="_blank" class="su-original-url" title="${link.originalUrl}">${link.originalUrl}</a>
                        </td>
                        <td>${link.title || '<span class="text-muted">-</span>'}</td>
                        <td class="text-center"><span class="su-click-badge">${link.clickCount}</span></td>
                        <td>${expire}</td>
                        <td>${new Date(link.createdTime).toLocaleDateString('vi-VN')}</td>
                        <td>
                            <div class="su-actions">
                                <button class="su-btn-icon copy btn-copy" data-id="${link.id}" title="Copy link">
                                    <i class="fa-solid fa-copy"></i>
                                </button>
                                <button class="su-btn-icon delete btn-delete" data-id="${link.id}" title="Xóa">
                                    <i class="fa-solid fa-trash"></i>
                                </button>
                            </div>
                        </td>
                    </tr>
                `);
            });
        }

        private renderEmpty(msg: string): void {
            this.root.find('#su-tbody').html(`
                <tr>
                    <td colspan="7" class="text-center py-5 text-muted">
                        <i class="fa-solid fa-link fa-2x mb-2 d-block" style="opacity:.3"></i>
                        ${msg}
                    </td>
                </tr>
            `);
        }

        private openModal(): void {
            $('#su-original-url').val('');
            $('#su-title').val('');
            this.expirePicker.clear();
            this.modal.show();
        }

        private async saveLink(): Promise<void> {
            const originalUrl = (($('#su-original-url').val() as string) || '').trim();
            if (!originalUrl) {
                ToastService.warning('Vui lòng nhập URL gốc');
                return;
            }

            const expireDate = this.expirePicker.getValue();
            const expireAt = expireDate ? expireDate.toISOString() : null;

            LoadingService.show();
            try {
                const res = await ApiService.post('/ShortUrl/Create', {
                    originalUrl,
                    title: (($('#su-title').val() as string) || '').trim() || null,
                    expireAt
                });

                if (res.isOk()) {
                    this.modal.hide();
                    ToastService.success('Tạo link thành công');
                    await this.loadList();
                } else {
                    ToastService.error(res.message || 'Tạo link thất bại');
                }
            } catch {
                ToastService.error('Lỗi hệ thống');
            } finally {
                LoadingService.hide();
            }
        }

        private async copyLink(link: ShortUrlModel): Promise<void> {
            try {
                await navigator.clipboard.writeText(this.shortLink(link.code));
                ToastService.success('Đã copy link');
            } catch {
                ToastService.error('Không thể copy');
            }
        }

        private async deleteLink(link: ShortUrlModel): Promise<void> {
            await MessageService.confirm(
                `Xóa link <strong>${this.shortLink(link.code)}</strong>?`,
                'Xác nhận xóa',
                async () => {
                    LoadingService.show();
                    try {
                        const res = await ApiService.post('/ShortUrl/Delete', { id: link.id });
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
