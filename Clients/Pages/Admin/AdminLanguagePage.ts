namespace App {
    interface LanguageModel {
        id: number;
        code: string;
        name: string;
    }

    export class AdminLanguagePage extends App.BasePage {
        private gridBuilder: App.GridBuilder<LanguageModel>;
        private table: DataTables.Api;

        protected initialize(): void {
            this.initGrid();
            this.loadData();
        }

        protected bindEvents(): void {
            // Event binding is now handled by GridBuilder and GridFormBuilder
        }

        private loadData(): void {
            App.LoadingService.show();
            App.ApiService.get('/Admin/Language/GetListLanguages').then(response => {
                App.LoadingService.hide();
                if (response.isOk()) {
                    const data = response.data || [];
                    this.table.clear();
                    this.table.rows.add(data);
                    this.table.draw();
                } else {
                    App.ToastService.error(response.message || 'Error');
                }
            }).catch(() => {
                App.LoadingService.hide();
                App.ToastService.error('Không thể tải danh sách user');
            });
        }

        private initGrid(): void {
            this.gridBuilder = new App.GridBuilder<LanguageModel>('#tblLanguage')
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
                .setOptions({
                    scrollY: 'calc(100vh - 395px)',
                    scrollCollapse: true,
                    paging: false,
                    info: false,
                    ordering: false,
                    searching: true,
                    autoWidth: true
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

            this.table = this.gridBuilder.build();

            // Set up form save handler
            const formBuilder = this.gridBuilder.getFormBuilder();
            if (formBuilder) {
                formBuilder.onSave((submission) => this.saveLanguage(submission));
                formBuilder.onDelete((data) => this.deleteLanguage(data));
            }
        }

        private async saveLanguage(submission: App.IFormSubmission<LanguageModel>): Promise<void> {
            const url = submission.mode === 'create'
                ? '/Admin/Language/Create'
                : '/Admin/Language/Update';

            App.LoadingService.show();

            try {
                const response = await App.ApiService.post(url, submission.data);
                App.LoadingService.hide();

                if (response.isOk()) {
                    App.ToastService.success(submission.mode === 'create' ? 'Thêm thành công' : 'Cập nhật thành công');
                    this.loadData();
                } else {
                    App.ToastService.error(response.message || 'Lỗi khi lưu');
                }
            } catch (error) {
                App.LoadingService.hide();
                App.ToastService.error('Lỗi hệ thống');
            }
        }

        private deleteLanguage(language: LanguageModel): void {
            if (!confirm('Bạn có chắc chắn muốn xóa ngôn ngữ này?')) {
                return;
            }

            App.LoadingService.show();
            App.ApiService.post('/Admin/Language/Delete', { id: language.id })
                .then(response => {
                    App.LoadingService.hide();
                    if (response.isOk()) {
                        App.ToastService.success('Xóa thành công');
                        this.loadData();
                    } else {
                        App.ToastService.error(response.message || 'Xóa thất bại');
                    }
                })
                .catch(() => {
                    App.LoadingService.hide();
                    App.ToastService.error('Lỗi hệ thống');
                });
        }
    }
}