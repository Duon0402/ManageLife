namespace App {
    interface CodeSequenceModel {
        id: string;
        category: string;
        prefix: string;
        suffix: string;
        numberLength: number;
        currentSeq: number;
        createdTime: string;
    }

    export class AdminCodeSequencePage extends BasePage {
        private gridBuilder!: GridBuilder<CodeSequenceModel>;

        protected initialize(): void {
            this.initGrid();
        }

        protected bindEvents(): void { }

        private initGrid(): void {
            this.gridBuilder = new GridBuilder<CodeSequenceModel>('#tblCodeSequence')
                .setDataSource({ url: '/Admin/CodeSequence/GetList' })
                .addColumn({ field: 'id', title: 'ID', visible: false })
                .addColumn({ field: 'category', title: 'Category' })
                .addColumn({ field: 'prefix', title: 'Prefix' })
                .addColumn({ field: 'suffix', title: 'Suffix' })
                .addColumn({ field: 'numberLength', title: 'Độ dài số' })
                .addColumn({ field: 'currentSeq', title: 'Sequence hiện tại' })
                .addColumn({ field: 'createdTime', title: 'Ngày tạo', render: (data) => data ? new Date(data).toLocaleString('vi-VN') : '' })
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
                    icon: 'fa-rotate-left',
                    className: 'btn-outline-warning',
                    title: 'Reset sequence',
                    onClick: (data) => this.resetSequence(data)
                })
                .addActionButton({
                    icon: 'fa-trash',
                    className: 'btn-outline-danger',
                    title: 'Xóa',
                    onClick: (data) => this.deleteSequence(data)
                })
                .setForm({
                    createTitle: 'Thêm code sequence',
                    editTitle: 'Cập nhật code sequence',
                    saveButtonText: 'Lưu',
                    cancelButtonText: 'Hủy',
                    fields: [
                        { name: 'id', label: 'ID', type: 'hidden' },
                        { name: 'category', label: 'Category', type: 'text', required: true, placeholder: 'vd: ShortUrl, Invoice' },
                        { name: 'prefix', label: 'Prefix', type: 'text', placeholder: 'vd: SU, INV' },
                        { name: 'suffix', label: 'Suffix', type: 'text', placeholder: 'Để trống nếu không dùng' },
                        { name: 'numberLength', label: 'Độ dài số', type: 'number', required: true, placeholder: 'vd: 6' }
                    ]
                });

            this.gridBuilder.build();

            const formBuilder = this.gridBuilder.getFormBuilder();
            if (formBuilder) {
                formBuilder.onSave((submission) => this.saveSequence(submission));
            }
        }

        private async saveSequence(submission: IFormSubmission<CodeSequenceModel>): Promise<void> {
            const isCreate = submission.mode === 'create';
            const url = isCreate ? '/Admin/CodeSequence/Create' : '/Admin/CodeSequence/Update';

            LoadingService.show();
            try {
                const response = await ApiService.post(url, submission.data);
                if (response.isOk()) {
                    ToastService.success(isCreate ? 'Thêm thành công' : 'Cập nhật thành công');
                    this.gridBuilder.reload();
                } else {
                    ToastService.error(response.message || 'Lỗi khi lưu');
                }
            } catch {
                ToastService.error('Lỗi hệ thống');
            } finally {
                LoadingService.hide();
            }
        }

        private resetSequence(data: CodeSequenceModel): void {
            const inputId = 'reset-seq-value';
            const popup = new PopupBuilder({
                title: `Reset sequence — ${data.category}`,
                size: 'sm',
                bodyHtml: `
                    <p class="text-muted mb-2">Sequence hiện tại: <strong>${data.currentSeq}</strong></p>
                    <label class="form-label">Giá trị mới</label>
                    <input id="${inputId}" type="number" class="form-control" value="0" min="0" autofocus />
                `,
                footerHtml: `
                    <button type="button" class="btn btn-secondary btn-sm" data-bs-dismiss="modal">Hủy</button>
                    <button type="button" class="btn btn-warning btn-sm" id="btn-confirm-reset">Reset</button>
                `,
                onShow: (body) => {
                    body.closest('.modal').find('#btn-confirm-reset').on('click', async () => {
                        const value = parseInt($(`#${inputId}`).val() as string, 10);
                        if (isNaN(value) || value < 0) {
                            ToastService.error('Giá trị không hợp lệ');
                            return;
                        }
                        popup.hide();
                        LoadingService.show();
                        try {
                            const response = await ApiService.post('/Admin/CodeSequence/Reset', { id: data.id, value });
                            if (response.isOk()) {
                                ToastService.success(`Đã reset sequence về ${value}`);
                                this.gridBuilder.reload();
                            } else {
                                ToastService.error(response.message || 'Reset thất bại');
                            }
                        } catch {
                            ToastService.error('Lỗi hệ thống');
                        } finally {
                            LoadingService.hide();
                        }
                    });
                }
            });
            popup.show();
        }

        private async deleteSequence(data: CodeSequenceModel): Promise<void> {
            await MessageService.confirm(
                `Xóa category "${data.category}"? Hành động này không thể hoàn tác.`,
                'Xác nhận xóa',
                async () => {
                    LoadingService.show();
                    try {
                        const response = await ApiService.post('/Admin/CodeSequence/Delete', { id: data.id });
                        if (response.isOk()) {
                            ToastService.success('Xóa thành công');
                            this.gridBuilder.reload();
                        } else {
                            ToastService.error(response.message || 'Xóa thất bại');
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
