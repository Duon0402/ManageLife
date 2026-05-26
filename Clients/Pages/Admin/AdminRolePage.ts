namespace App {
    interface RoleModel {
        id: number;
        code: string;
        name: string;
        description: string;
    }

    export class AdminRolePage extends BasePage {
        private gridBuilder: GridBuilder<RoleModel>;

        protected initialize(): void {
            this.initGrid();
        }

        private initGrid(): void {
            this.gridBuilder = new GridBuilder<RoleModel>('#tblRole')
                .setDataSource({
                    url: '/Admin/Role/GetListRoles'
                })
                .addColumn({ field: 'id', title: 'ID', visible: false })
                .addColumn({ field: 'code', title: 'Code' })
                .addColumn({ field: 'name', title: 'Name' })
                .addColumn({ field: 'description', title: 'Description' })
                .addToolbarButton({
                    icon: 'fa-plus',
                    className: 'btn btn-sm btn-outline-secondary',
                    title: 'Thêm mới',
                    onClick: () => this.gridBuilder.getFormBuilder()?.showCreate()
                })
                .addActionButton({
                    icon: 'fa-key',
                    className: 'btn-outline-primary',
                    title: 'Permission',
                    onClick: (data) => {
                        window.location.href = `/admin/permission/indexbyrole?roleId=${data.id}`;
                    }
                })
                .addActionButton({
                    icon: 'fa-trash',
                    className: 'btn-outline-danger',
                    title: 'Xóa',
                    onClick: (data) => this.deleteRole(data)
                })
                .setOptions({
                    paging: false,
                    info: false,
                    ordering: false,
                    searching: true,
                    autoWidth: false
                })
                .setForm({
                    createTitle: 'Thêm mới Role',
                    editTitle: 'Cập nhật Role',
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
                            placeholder: 'Nhập mã Role'
                        },
                        {
                            name: 'name',
                            label: 'Name',
                            type: 'text',
                            required: true,
                            placeholder: 'Nhập tên Role'
                        },
                        {
                            name: 'description',
                            label: 'Description',
                            type: 'textarea',
                            placeholder: 'Nhập mô tả'
                        }
                    ]
                });

            this.gridBuilder.build();

            const formBuilder = this.gridBuilder.getFormBuilder();
            if (formBuilder) {
                formBuilder.onSave((submission) => this.saveRole(submission));
                formBuilder.onDelete((data) => this.deleteRole(data));
            }
        }

        private async saveRole(submission: IFormSubmission<RoleModel>): Promise<void> {
            const url = submission.mode === 'create'
                ? '/Admin/Role/CreateRole'
                : '/Admin/Role/UpdateRole';

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

        private async deleteRole(role: RoleModel): Promise<void> {
            const ok = await MessageService.confirm('Bạn có chắc chắn muốn xoá role này?', 'Xác nhận');
            if (!ok) return;

            LoadingService.show();
            ApiService.post('/Admin/Role/DeleteRole', { roleId: role.id })
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
