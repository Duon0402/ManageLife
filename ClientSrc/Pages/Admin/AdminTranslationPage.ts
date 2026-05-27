namespace App {
    interface TranslationModel {
        id: number;
        key: string;
        value: string;
        languageId: number;
        languageName: string;
    }

    interface TranslationPageModel {
        languages: { key: string; value: string }[];
    }

    export class AdminTranslationPage extends BasePage<TranslationPageModel> {
        private gridBuilder!: GridBuilder<TranslationModel>;

        protected initialize(): void {
            this.addPageAction({ icon: 'fa-download', label: 'Tải template', onClick: () => this.downloadTemplate() });
            this.addPageAction({ icon: 'fa-upload', label: 'Import Excel', onClick: () => this.triggerUpload() });
            this.initGrid();
        }

        private initGrid(): void {
            const languageOptions: ISelectOption[] = (this.model.languages || []).map(l => ({
                value: l.key,
                text: l.value
            }));

            this.gridBuilder = new GridBuilder<TranslationModel>('#tblTranslations')
                .setDataSource({ url: '/Admin/Translation/GetList' })
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
                .setOptions({ ordering: true })
                .setForm({
                    createTitle: 'Thêm bản dịch',
                    editTitle: 'Cập nhật bản dịch',
                    saveButtonText: 'Lưu',
                    cancelButtonText: 'Hủy',
                    showDeleteButton: false,
                    fields: [
                        { name: 'id', label: 'ID', type: 'hidden' },
                        { name: 'key', label: 'Key', type: 'text', required: true, placeholder: 'Nhập key (vd: common.save)' },
                        { name: 'value', label: 'Value', type: 'textarea', required: true, placeholder: 'Nhập giá trị dịch' },
                        { name: 'languageId', label: 'Language', type: 'select', required: true, options: languageOptions }
                    ]
                });

            this.gridBuilder.build();

            const formBuilder = this.gridBuilder.getFormBuilder();
            if (formBuilder) {
                formBuilder.onSave((submission) => this.saveTranslation(submission));
            }
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
                    this.gridBuilder.reload();
                } else {
                    ToastService.error(response.message || 'Lỗi khi lưu');
                }
            } catch {
                LoadingService.hide();
                ToastService.error('Lỗi hệ thống');
            }
        }

        private async downloadTemplate(): Promise<void> {
            LoadingService.show();
            try {
                const blob = await ApiService.getBlob('/Admin/Translation/DownloadTemplate');
                const url = URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = 'translation_template.xlsx';
                a.click();
                URL.revokeObjectURL(url);
            } catch {
                ToastService.error('Lỗi khi tải template');
            } finally {
                LoadingService.hide();
            }
        }

        private triggerUpload(): void {
            const input = document.createElement('input');
            input.type = 'file';
            input.accept = '.xlsx';
            input.onchange = async () => {
                const file = input.files?.[0];
                if (!file) return;
                const formData = new FormData();
                formData.append('file', file);
                LoadingService.show();
                try {
                    const response = await ApiService.upload('/Admin/Translation/ImportTemplate', formData);
                    LoadingService.hide();
                    if (response.isOk()) {
                        ToastService.success('Import thành công');
                        this.gridBuilder.reload();
                    } else {
                        ToastService.error(response.message || 'Import thất bại');
                    }
                } catch {
                    LoadingService.hide();
                    ToastService.error('Lỗi hệ thống');
                }
            };
            input.click();
        }

        private async deleteTranslation(translation: TranslationModel): Promise<void> {
            const ok = await MessageService.confirm('Bạn có chắc chắn muốn xóa bản dịch này?', 'Xác nhận');
            if (!ok) return;

            LoadingService.show();
            ApiService.post('/Admin/Translation/Delete', { id: translation.id })
                .then(response => {
                    LoadingService.hide();
                    if (response.isOk()) {
                        ToastService.success('Xóa thành công');
                        this.gridBuilder.reload();
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
