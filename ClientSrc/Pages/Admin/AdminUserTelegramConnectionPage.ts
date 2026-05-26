namespace App {
    export class AdminUserTelegramConnectionPage extends BasePage {
        private gridBuilder: GridBuilder<UserTelegramConnectionModel>;

        protected initialize(): void {
            this.initGrid();
        }

        private initGrid(): void {
            this.gridBuilder = new GridBuilder<UserTelegramConnectionModel>('#tblConnections')
                .setDataSource({
                    url: '/Admin/UserTelegramConnection/GetListUserTelegramConnections'
                })
                .addColumn({ field: 'id', title: 'ID', visible: false })
                .addColumn({ field: 'userId', title: 'User Id' })
                .addColumn({ field: 'chatId', title: 'Chat Id' })
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
                    onClick: () => console.log("Delete")
                })
                .setOptions({
                    paging: false,
                    info: false,
                    ordering: true,
                    searching: true,
                    autoWidth: true
                })
                .setForm({
                    createTitle: 'Thêm mới',
                    editTitle: 'Cập nhật',
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
                            name: 'userId',
                            label: 'User Id',
                            type: 'text',
                            required: true,
                        },
                        {
                            name: 'chatId',
                            label: 'Chat Id',
                            type: 'text',
                            required: true,
                        }
                    ]
                });

            this.gridBuilder.build();

            // Set up form save handler
            const formBuilder = this.gridBuilder.getFormBuilder();
            if (formBuilder) {
                formBuilder.onSave(() => console.log("Save"))
            }
        }
    }
}