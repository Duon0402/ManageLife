namespace App {
    interface PermissionModel {
        id: number;
        code: string;
        name: string;
        description: string;
    }

    interface AdminPermissionViewModel {
        targetType: number; // 1=User, 2=Role
        userId: string | null;
        roleId: string | null;
        userName: string;
        roleName: string;
    }

    enum PermissionTargetType {
        User = 1,
        Role = 2
    }

    export class AdminPermissionPage extends BasePage<AdminPermissionViewModel> {
        private gridUnassigned: GridBuilder<PermissionModel>;
        private gridAssigned: GridBuilder<PermissionModel>;

        protected initialize(): void {
            this.initGrids();
        }

        protected bindEvents(): void {
            this.root.find('#btnAssign').on('click', () => this.assignPermissions());
            this.root.find('#btnUnassign').on('click', () => this.unassignPermissions());
        }

        private buildUrl(action: string): string {
            const isUser = this.model.targetType === PermissionTargetType.User;
            return isUser
                ? `/Admin/Permission/${action}ByUserId?userId=${this.model.userId}`
                : `/Admin/Permission/${action}ByRoleId?roleId=${this.model.roleId}`;
        }

        private initGrids(): void {
            const commonOptions = {
                rowId: 'id',
                dom: 't',
                scrollX: true,
                columnDefs: [
                    {
                        orderable: false,
                        render: (window as any).DataTable?.render?.select(),
                        className: 'select-checkbox',
                        defaultContent: '',
                        targets: 0
                    }
                ],
                select: {
                    style: 'multi',
                    selector: 'td:first-child'
                }
            };

            const commonColumns = [
                { title: '', data: null, defaultContent: '', width: '30px' } as IGridColumn<PermissionModel>,
                { field: 'code', title: 'Code' } as IGridColumn<PermissionModel>,
                { field: 'name', title: 'Name' } as IGridColumn<PermissionModel>,
                { field: 'description', title: 'Description', defaultContent: '' } as IGridColumn<PermissionModel>
            ];

            // Unassigned Grid
            this.gridUnassigned = new GridBuilder<PermissionModel>('#tblUnassigned')
                .setDataSource({ url: this.buildUrl('GetUnassignedPermissions') })
                .setOptions(commonOptions as any);
            commonColumns.forEach(col => this.gridUnassigned.addColumn(col));
            this.gridUnassigned.build();

            // Assigned Grid
            this.gridAssigned = new GridBuilder<PermissionModel>('#tblAssigned')
                .setDataSource({ url: this.buildUrl('GetAssignedPermissions') })
                .setOptions(commonOptions as any);
            commonColumns.forEach(col => this.gridAssigned.addColumn(col));
            this.gridAssigned.build();
        }

        private assignPermissions(): void {
            this.movePermissions(true);
        }

        private unassignPermissions(): void {
            this.movePermissions(false);
        }

        private movePermissions(isAssign: boolean): void {
            const sourceGrid = isAssign ? this.gridUnassigned : this.gridAssigned;
            const sourceTable = sourceGrid.getTable();

            if (!sourceTable) return;

            const selectedData = sourceTable.rows({ selected: true }).data().toArray() as PermissionModel[];
            const permissionIds = selectedData.map(r => r.id);

            if (permissionIds.length === 0) {
                ToastService.warning("Chưa chọn permission nào");
                return;
            }

            const targetType = this.model.targetType;
            const objectId = targetType === PermissionTargetType.User ? this.model.userId : this.model.roleId;
            const url = isAssign
                ? '/Admin/Permission/AssignPermissions'
                : '/Admin/Permission/UnassignPermissions';

            const payload = {
                ObjectId: objectId,
                TargetType: targetType,
                PermissionIds: permissionIds
            };

            LoadingService.show();
            ApiService.post(url, payload).then(response => {
                LoadingService.hide();
                if (response.isOk()) {
                    ToastService.success('Success');
                    this.gridUnassigned.reload();
                    this.gridAssigned.reload();
                } else {
                    ToastService.error(response.message || 'Error');
                }
            }).catch(() => {
                LoadingService.hide();
                ToastService.error('Lỗi hệ thống');
            });
        }
    }
}
