namespace App {
    interface UserModel {
        id: number;
        userName: string;
        fullName: string;
        email: string;
        isActive: boolean;
    }

    export class AdminUserPage extends BasePage {
        private gridBuilder: GridBuilder<UserModel>;

        protected initialize(): void {
            this.initGrid();
        }

        private initGrid(): void {
            this.gridBuilder = new GridBuilder<UserModel>('#tblUser')
                .setDataSource({
                    url: '/Admin/User/GetListUsers'
                })
                .addColumn({ field: 'id', title: 'ID' })
                .addColumn({ field: 'userName', title: 'User Name' })
                .addColumn({ field: 'fullName', title: 'Full Name', defaultContent: '' })
                .addColumn({ field: 'email', title: 'Email', defaultContent: '' })
                .addColumn(new GridColumnBuilder<UserModel>('isActive', 'Active')
                    .render((data) => data
                        ? '<span class="badge bg-success">Active</span>'
                        : '<span class="badge bg-secondary">Inactive</span>')
                )
                .addActionButton({
                    icon: 'fa-key',
                    className: 'btn-outline-primary',
                    title: 'Permission',
                    onClick: (data) => {
                        window.location.href = `/admin/permission/indexbyuser?userId=${data.id}`;
                    }
                })
                .setOptions({
                    paging: false,
                    info: false,
                    ordering: false,
                    searching: true,
                    autoWidth: false
                });

            this.gridBuilder.build();
        }
    }
}
