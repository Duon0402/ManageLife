namespace App {
    interface LanguageModel {
        id: number;
        code: string;
        name: string;
    }

    export class AdminLanguagePage extends BasePage {
        private gridBuilder: GridBuilder<LanguageModel>;

        protected initialize(): void {
            this.initGrid();
        }

        protected bindEvents(): void {
            // Event binding is now handled by GridBuilder and GridFormBuilder
        }

        private initGrid(): void {
            this.gridBuilder = new GridBuilder<LanguageModel>('#tblLanguage')
                .setDataSource({
                    url: '/Admin/Language/GetListLanguages'
                })
                .addColumn({ field: 'id', title: 'ID', visible: false })
                .addColumn({ field: 'code', title: 'Code' })
                .addColumn({ field: 'name', title: 'Name' })
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
                    onClick: (data) => this.deleteLanguage(data)
                })
                .setForm({
                    createTitle: 'Thêm ngôn ngữ',
                    editTitle: 'Cập nhật ngôn ngữ',
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
                            name: 'code',
                            label: 'Code',
                            type: 'text',
                            required: true,
                            placeholder: 'Nhập mã ngôn ngữ (vd: en, vi)'
                        },
                        {
                            name: 'name',
                            label: 'Name',
                            type: 'text',
                            required: true,
                            placeholder: 'Nhập tên ngôn ngữ'
                        }
                    ]
                });

            this.gridBuilder.build();

            // Set up form save handler
            const formBuilder = this.gridBuilder.getFormBuilder();
            if (formBuilder) {
                formBuilder.onSave((submission) => this.saveLanguage(submission));
                formBuilder.onDelete((data) => this.deleteLanguage(data));
            }
        }

        private async saveLanguage(submission: IFormSubmission<LanguageModel>): Promise<void> {
            const url = submission.mode === 'create'
                ? '/Admin/Language/Create'
                : '/Admin/Language/Update';

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
            } catch (error) {
                LoadingService.hide();
                ToastService.error('Lỗi hệ thống');
            }
        }

        private async deleteLanguage(language: LanguageModel): Promise<void> {
            await MessageService.confirm(
                'Bạn có chắc chắn muốn xóa ngôn ngữ này?',
                'Xác nhận',
                async () => {
                    LoadingService.show();
                    try {
                        const response = await ApiService.post(
                            '/Admin/Language/Delete',
                            { id: language.id }
                        );
                        if (response.isOk()) {
                            ToastService.success('Xóa thành công');
                            this.gridBuilder.reload();
                        }
                        else {
                            ToastService.error(
                                response.message || 'Xóa thất bại'
                            );
                        }
                    }
                    catch {
                        ToastService.error('Lỗi hệ thống');
                    }
                    finally {
                        LoadingService.hide();
                    }
                }
            );
        }
    }
}