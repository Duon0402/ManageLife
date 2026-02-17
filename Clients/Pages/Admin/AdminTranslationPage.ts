namespace App {
    interface TranslationModel {
        id: number;
        key: string;
        value: string;
        languageId: number;
        languageName: string;
    }

    interface TranslationViewModel {
        Languages: { Key: string; Value: string; }[];
    }

    export class AdminTranslationPage extends BasePage<TranslationViewModel> {
        private gridBuilder: GridBuilder<TranslationModel>;
        private table: DataTables.Api;

        protected initialize(): void {
            this.initGrid();
            this.loadTranslations();
        }

        protected bindEvents(): void {
            this.root.find('#btnLoad').on('click', () => this.loadTranslations());
        }

        private initGrid(): void {
            // Convert model languages to select options
            const languageOptions: ISelectOption[] = (this.model.Languages || []).map(lang => ({
                value: lang.Key,
                text: lang.Value
            }));

            this.gridBuilder = new GridBuilder<TranslationModel>('#tblTranslations')
                .addColumn({ field: 'key', title: 'Key' })
                .addColumn({ field: 'value', title: 'Value' })
                .addColumn({ field: 'languageName', title: 'Language' })
                .addToolbarButton({
                    icon: 'fa-plus',
                    className: 'btn btn-sm btn-outline-secondary',
                    title: 'Thêm mới',
                    onClick: () => this.gridBuilder.getFormBuilder()?.showCreate()
                })
                .addActionButton({
                    icon: 'fa-pen-to-square',
                    className: 'btn-outline-primary',
                    title: 'Sửa',
                    onClick: (data) => this.gridBuilder.getFormBuilder()?.showEdit(data)
                })
                .addActionButton({
                    icon: 'fa-trash',
                    className: 'btn-outline-danger',
                    title: 'Xóa',
                    onClick: (data) => this.deleteTranslation(data)
                })
                .setOptions({
                    scrollY: 'calc(100vh - 395px)',
                    scrollCollapse: true,
                    paging: false,
                    info: false,
                    ordering: true,
                    searching: true,
                    autoWidth: true
                })
                .setForm({
                    createTitle: 'Thêm bản dịch',
                    editTitle: 'Cập nhật bản dịch',
                    saveButtonText: 'Lưu',
                    cancelButtonText: 'Hủy',
                    showDeleteButton: true,
                    fields: [
                        {
                            name: 'id',
                            label: 'ID',
                            type: 'hidden'
                        },
                        {
                            name: 'key',
                            label: 'Key',
                            type: 'text',
                            required: true,
                            placeholder: 'Nhập key (vd: common.save)'
                        },
                        {
                            name: 'value',
                            label: 'Value',
                            type: 'textarea',
                            required: true,
                            placeholder: 'Nhập giá trị dịch'
                        },
                        {
                            name: 'languageId',
                            label: 'Language',
                            type: 'select',
                            required: true,
                            options: languageOptions
                        }
                    ]
                });

            this.table = this.gridBuilder.build();

            // Set up form save handler
            const formBuilder = this.gridBuilder.getFormBuilder();
            if (formBuilder) {
                formBuilder.onSave((submission) => this.saveTranslation(submission));
                formBuilder.onDelete((data) => this.deleteTranslation(data));
            }
        }

        private loadTranslations(): void {
            const languageId = this.root.find("#ddlLanguageId").val();

            const req = {
                languageId: languageId
            };

            LoadingService.show();
            ApiService.post('/Admin/Translation/GetListTranslations', req, {
                showLoading: false,
                success: (response: any) => {
                    LoadingService.hide();
                    if (response.isOk()) {
                        const data = response.data || [];
                        this.table.clear();
                        this.table.rows.add(data);
                        this.table.draw();
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

        private async saveTranslation(submission: IFormSubmission<TranslationModel>): Promise<void> {
            const url = submission.mode === 'create'
                ? '/Admin/Translation/Create'
                : '/Admin/Translation/Update';

            LoadingService.show();

            try {
                const response = await ApiService.post(url, submission.data);
                LoadingService.hide();

                if (response.isOk()) {
                    ToastService.success(submission.mode === 'create' ? 'Thêm thành công' : 'Cập nhật thành công');
                    this.loadTranslations();
                } else {
                    ToastService.error(response.message || 'Lỗi khi lưu');
                }
            } catch (error) {
                LoadingService.hide();
                ToastService.error('Lỗi hệ thống');
            }
        }

        private deleteTranslation(translation: TranslationModel): void {
            if (!confirm('Bạn có chắc chắn muốn xóa bản dịch này?')) {
                return;
            }

            LoadingService.show();
            ApiService.post('/Admin/Translation/Delete', { id: translation.id })
                .then(response => {
                    LoadingService.hide();
                    if (response.isOk()) {
                        ToastService.success('Xóa thành công');
                        this.loadTranslations();
                    } else {
                        ToastService.error(response.message || 'Xóa thất bại');
                    }
                })
                .catch(() => {
                    LoadingService.hide();
                    ToastService.error('Lỗi hệ thống');
                });
        }
    }
}