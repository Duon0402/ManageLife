namespace App {
    interface TelegramBotCommandModel {
        id: string;
        command: string;
        description: string;
        sortOrder: number;
    }

    export class AdminTelegramBotCommandPage extends BasePage {
        private gridBuilder!: GridBuilder<TelegramBotCommandModel>;

        protected initialize(): void {
            this.initGrid();
        }

        private initGrid(): void {
            this.gridBuilder = new GridBuilder<TelegramBotCommandModel>('#tblBotCommands')
                .setDataSource({ url: '/Admin/TelegramBotCommand/GetList' })
                .addColumn({ field: 'id', title: 'ID', visible: false })
                .addColumn({
                    field: 'command',
                    title: 'Command',
                    render: (data) => `<code>/${data}</code>`
                })
                .addColumn({ field: 'description', title: 'Mô tả' })
                .addColumn({ field: 'sortOrder', title: 'Thứ tự' })
                .addToolbarButton({
                    icon: 'fa-plus',
                    className: 'btn btn-sm btn-outline-secondary',
                    title: 'Thêm mới',
                    onClick: () => this.gridBuilder.getFormBuilder()?.showCreate()
                })
                .addToolbarButton({
                    icon: 'fa-rotate',
                    className: 'btn btn-sm btn-outline-primary',
                    title: 'Đồng bộ lên Telegram',
                    onClick: () => this.syncToTelegram()
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
                    onClick: (data) => this.deleteCommand(data)
                })
                .setOptions({ ordering: true })
                .setForm({
                    createTitle: 'Thêm Bot Command',
                    editTitle: 'Cập nhật Bot Command',
                    saveButtonText: 'Lưu',
                    cancelButtonText: 'Hủy',
                    showDeleteButton: false,
                    fields: [
                        { name: 'id', label: 'ID', type: 'hidden' },
                        {
                            name: 'command',
                            label: 'Command',
                            type: 'text',
                            required: true,
                            placeholder: 'Ví dụ: start hoặc /start'
                        },
                        {
                            name: 'description',
                            label: 'Mô tả',
                            type: 'text',
                            required: true,
                            placeholder: 'Mô tả ngắn về command'
                        },
                        {
                            name: 'sortOrder',
                            label: 'Thứ tự',
                            type: 'number',
                            placeholder: '0'
                        }
                    ]
                });

            this.gridBuilder.build();

            const formBuilder = this.gridBuilder.getFormBuilder();
            if (formBuilder) {
                formBuilder.onSave((submission) => this.saveCommand(submission));
            }
        }

        private async saveCommand(submission: IFormSubmission<TelegramBotCommandModel>): Promise<void> {
            const url = submission.mode === 'create'
                ? '/Admin/TelegramBotCommand/Create'
                : '/Admin/TelegramBotCommand/Update';

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

        private async deleteCommand(data: TelegramBotCommandModel): Promise<void> {
            await MessageService.confirm(
                `Bạn có chắc chắn muốn xóa command /${data.command}?`,
                'Xác nhận',
                async () => {
                    LoadingService.show();
                    try {
                        const response = await ApiService.post('/Admin/TelegramBotCommand/Delete', { id: data.id });
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

        private async syncToTelegram(): Promise<void> {
            await MessageService.confirm(
                'Đồng bộ danh sách commands lên Telegram Bot?',
                'Đồng bộ',
                async () => {
                    LoadingService.show();
                    try {
                        const response = await ApiService.post('/Admin/TelegramBotCommand/SyncToTelegram', {});
                        if (response.isOk()) {
                            ToastService.success('Đồng bộ thành công');
                        } else {
                            ToastService.error(response.message || 'Đồng bộ thất bại');
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
