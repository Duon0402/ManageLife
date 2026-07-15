namespace App {
    const enum SettingType {
        Text = 0,
        Boolean = 1,
        File = 2,
        Number = 3,
        Json = 4,
        Color = 5,
        Url = 6,
        Password = 7
    }

    interface SettingModel {
        id: string;
        key: string;
        value: string;
        type: SettingType;
        group: string | null;
        description: string | null;
    }

    export class AdminSettingPage extends BasePage {

        protected initialize(): void {
            this.addPageAction({
                icon: 'fa-paper-plane',
                label: 'Gửi email thử',
                onClick: () => this.sendTestEmail()
            });
            this.initGrid();
        }

        protected bindEvents(): void {
            this.root.on('change', '.setting-toggle-input', (e) => {
                const $input = $(e.currentTarget);
                const id = $input.data('id') as string;
                const value = $input.is(':checked') ? 'true' : 'false';
                this.saveSetting(id, value, null);
            });

            this.root.on('keydown', '.setting-value-input:not(textarea)', (e) => {
                if (e.key === 'Enter') {
                    $(e.currentTarget).closest('tr').find('.grid-action-btn').trigger('click');
                }
            });
        }

        private initGrid(): void {
            new GridBuilder<SettingModel>('#tblSetting')
                .setDataSource({ url: '/Admin/Setting/GetList' })
                .setOptions({ searching: false, ordering: false })
                .addColumn({
                    field: 'group',
                    title: 'Nhóm',
                    width: '130px',
                    defaultContent: '<span class="text-muted fst-italic">Khác</span>'
                })
                .addColumn(new GridColumnBuilder<SettingModel>('key', 'Key / Mô tả')
                    .width('280px')
                    .render((_data, _type, row) => {
                        const desc = row.description
                            ? `<div class="text-muted small mt-1">${this.esc(row.description)}</div>`
                            : '';
                        return `<code class="text-primary">${this.esc(row.key)}</code>${desc}`;
                    })
                )
                .addColumn(new GridColumnBuilder<SettingModel>('value', 'Giá trị')
                    .render((_data, _type, row) => this.renderInput(row))
                )
                .addActionButton({
                    icon: 'fa-floppy-disk',
                    title: 'Lưu',
                    className: 'btn-outline-primary',
                    visible: (row) => row.type !== SettingType.Boolean,
                    onClick: (data, e) => {
                        const $btn = $(e.currentTarget);
                        const val = $btn.closest('tr').find('.setting-value-input').val() as string;
                        this.saveSetting(data.id, val, $btn);
                    }
                })
                .build();
        }

        private renderInput(s: SettingModel): string {
            if (s.type === SettingType.Boolean) {
                const checked = s.value === 'true' || s.value === '1' ? 'checked' : '';
                return `
                    <div class="form-check form-switch mb-0">
                        <input class="form-check-input setting-toggle-input" type="checkbox" data-id="${s.id}" ${checked}>
                    </div>`;
            }
            const val = this.esc(s.value);
            switch (s.type) {
                case SettingType.Number:
                    return `<input type="number" class="form-control form-control-sm setting-value-input" value="${val}">`;
                case SettingType.Color:
                    return `<input type="color" class="form-control form-control-color form-control-sm setting-value-input" value="${s.value || '#000000'}">`;
                case SettingType.Url:
                    return `<input type="url" class="form-control form-control-sm setting-value-input" value="${val}" placeholder="https://...">`;
                case SettingType.Json:
                    return `<textarea class="form-control form-control-sm setting-value-input" rows="3">${val}</textarea>`;
                case SettingType.Password:
                    return `<input type="password" class="form-control form-control-sm setting-value-input" value="${val}" autocomplete="new-password">`;
                default:
                    return `<input type="text" class="form-control form-control-sm setting-value-input" value="${val}">`;
            }
        }

        private async saveSetting(id: string, value: string, $btn: JQuery | null): Promise<void> {
            $btn?.prop('disabled', true);
            try {
                const res = await ApiService.post('/Admin/Setting/Update', { id, value });
                if (res.isOk()) {
                    ToastService.success('Đã lưu cấu hình');
                } else {
                    ToastService.error(res.message || 'Lưu thất bại');
                }
            } catch {
                ToastService.error('Lỗi hệ thống');
            } finally {
                $btn?.prop('disabled', false);
            }
        }

        private async sendTestEmail(): Promise<void> {
            const email = window.prompt('Nhập địa chỉ email nhận thử:');
            if (!email) return;
            try {
                const res = await ApiService.post('/Admin/Setting/SendTestEmail', { to: email });
                if (res.isOk()) {
                    ToastService.success('Đã gửi email thử');
                } else {
                    ToastService.error(res.message || 'Gửi email thất bại');
                }
            } catch {
                ToastService.error('Lỗi hệ thống');
            }
        }

        private esc(str: string): string {
            return (str || '')
                .replace(/&/g, '&amp;')
                .replace(/</g, '&lt;')
                .replace(/>/g, '&gt;')
                .replace(/"/g, '&quot;');
        }
    }
}
