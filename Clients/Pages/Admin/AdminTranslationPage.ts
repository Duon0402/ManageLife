namespace App {
    interface TranslationViewModel {
        Languages: { Key: string; Value: string; }[];
    }

    export class AdminTranslationPage extends BasePage<TranslationViewModel> {

        protected initialize(): void {
            this.loadTranslations();
        }

        protected bindEvents(): void {
            this.root.find('#btnLoad').on('click', () => this.loadTranslations());
            this.root.find('#btnCreate').on('click', () => this.createTranslation());
        }

        private loadTranslations(): void {
            const languageId = this.root.find("#ddlLanguageId").val();

            const req = {
                languageId: languageId
            };

            LoadingService.show();
            ApiService.post('/Admin/Translation/GetListTranslations', req, {
                showLoading: false, // handled manually
                success: (response: any) => {
                    LoadingService.hide();
                    if (response.isOk()) {
                        const tbody = this.root.find('#tblTranslations tbody');
                        tbody.empty();

                        (response.data || []).forEach((t: any) => {
                            const row = `
                                <tr>
                                    <td>${t.key}</td>
                                    <td>${t.value}</td>
                                    <td>${t.languageName}</td>
                                </tr>
                            `;
                            tbody.append(row);
                        });
                    } else {
                        ToastService.error(response.message || 'Lấy danh sách thất bại');
                    }
                },
                error: () => {
                    LoadingService.hide();
                    ToastService.error('Lỗi hệ thống');
                }
            });
        }

        private createTranslation(): void {
            const key = this.root.find("#txtKey").val();
            const value = this.root.find("#txtValue").val();
            const languageId = this.root.find("#ddlLanguageId").val();

            if (!key || !value || !languageId) {
                ToastService.warning("Vui lòng nhập đầy đủ thông tin");
                return;
            }

            const req = {
                key: key,
                value: value,
                languageId: languageId
            };

            LoadingService.show();
            ApiService.post('/Admin/Translation/CreateTranslation', req, {
                showLoading: false,
                success: (response: any) => {
                    LoadingService.hide();
                    if (response.isOk()) {
                        ToastService.success("Thêm thành công");
                        this.loadTranslations();
                        this.root.find("#txtKey").val('');
                        this.root.find("#txtValue").val('');
                        // keep language selection
                    } else {
                        ToastService.error(response.message || 'Thêm thất bại');
                    }
                },
                error: () => {
                    LoadingService.hide();
                    ToastService.error('Lỗi hệ thống');
                }
            });
        }
    }
}