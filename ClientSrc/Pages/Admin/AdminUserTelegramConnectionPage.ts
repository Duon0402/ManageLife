namespace App {
    interface UserOptionModel {
        id: string;
        userName: string;
    }

    export class AdminUserTelegramConnectionPage extends BasePage {
        private gridBuilder!: GridBuilder<UserTelegramConnectionModel>;

        protected initialize(): void {
            this.loadUserOptions().then(userOptions => this.initGrid(userOptions));
        }

        private async loadUserOptions(): Promise<ISelectOption[]> {
            const res = await ApiService.get<UserOptionModel[]>('/Admin/User/GetList', {}, { showLoading: false });
            if (res.isOk() && res.data) {
                return res.data.map(u => ({ value: u.id, text: u.userName }));
            }
            return [];
        }

        private initGrid(userOptions: ISelectOption[]): void {
            this.gridBuilder = new GridBuilder<UserTelegramConnectionModel>('#tblConnections')
                .setDataSource({
                    url: '/Admin/UserTelegramConnection/GetList'
                })
                .addColumn({ field: 'id', title: 'ID', visible: false })
                .addColumn({ field: 'userName', title: 'Người dùng' })
                .addColumn({ field: 'chatId', title: 'Chat ID' })
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
                    onClick: (data) => this.deleteConnection(data)
                })
                .setOptions({ ordering: true })
                .setForm({
                    createTitle: 'Thêm kết nối Telegram',
                    editTitle: 'Cập nhật kết nối Telegram',
                    saveButtonText: 'Lưu',
                    cancelButtonText: 'Hủy',
                    showDeleteButton: false,
                    fields: [
                        {
                            name: 'id',
                            label: 'ID',
                            type: 'hidden'
                        },
                        {
                            name: 'userId',
                            label: 'Người dùng',
                            type: 'select',
                            required: true,
                            options: userOptions
                        },
                        {
                            name: 'chatId',
                            label: 'Chat ID',
                            type: 'text',
                            required: true,
                            placeholder: 'Nhập Telegram Chat ID'
                        }
                    ]
                });

            this.gridBuilder.build();

            const formBuilder = this.gridBuilder.getFormBuilder();
            if (formBuilder) {
                formBuilder.onSave((submission) => this.saveConnection(submission));
            }
        }

        private async saveConnection(submission: IFormSubmission<UserTelegramConnectionModel>): Promise<void> {
            const url = submission.mode === 'create'
                ? '/Admin/UserTelegramConnection/Create'
                : '/Admin/UserTelegramConnection/Update';

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

        private async deleteConnection(data: UserTelegramConnectionModel): Promise<void> {
            await MessageService.confirm(
                'Bạn có chắc chắn muốn xóa kết nối Telegram này?',
                'Xác nhận',
                async () => {
                    LoadingService.show();
                    try {
                        const response = await ApiService.post(
                            '/Admin/UserTelegramConnection/Delete',
                            { id: data.id }
                        );
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
