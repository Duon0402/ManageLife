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
            this.loadData();
        }

        protected bindEvents(): void {
            this.root.find('#btnAssign').on('click', () => this.assignPermissions());
            this.root.find('#btnUnassign').on('click', () => this.unassignPermissions());
        }

        private initGrids(): void {
            const commonOptions = {
                paging: false,
                info: false,
                ordering: false,
                searching: true,
                autoWidth: true,
                rowId: 'id',
                dom: 't', // Simple table only
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
                { title: '', data: null, defaultContent: '', width: '30px' } as IGridColumn<PermissionModel>, // Checkbox column
                { field: 'code', title: 'Code' } as IGridColumn<PermissionModel>,
                { field: 'name', title: 'Name' } as IGridColumn<PermissionModel>,
                { field: 'description', title: 'Description', defaultContent: '' } as IGridColumn<PermissionModel>
            ];

            // Unassigned Grid
            this.gridUnassigned = new GridBuilder<PermissionModel>('#tblUnassigned')
                .setOptions(commonOptions as any);

            commonColumns.forEach(col => this.gridUnassigned.addColumn(col));
            this.gridUnassigned.build();

            // Assigned Grid
            this.gridAssigned = new GridBuilder<PermissionModel>('#tblAssigned')
                .setOptions(commonOptions as any);

            commonColumns.forEach(col => this.gridAssigned.addColumn(col));
            this.gridAssigned.build();
        }

        private async loadData(): Promise<void> {
            LoadingService.show();

            const targetType = this.model.targetType;
            const objectId = targetType === PermissionTargetType.User ? this.model.userId : this.model.roleId;

            try {
                const [unassigned, assigned] = await Promise.all([
                    this.getUnassignedPermissions(objectId, targetType),
                    this.getAssignedPermissions(objectId, targetType)
                ]);

                LoadingService.hide();

                if (unassigned?.isOk() && assigned?.isOk()) {
                    this.gridUnassigned.reload(unassigned.data || []);
                    this.gridAssigned.reload(assigned.data || []);
                } else {
                    ToastService.error('Load permission failed');
                }

            } catch (error) {
                LoadingService.hide();
                ToastService.error('Lỗi hệ thống');
            }
        }

        private getUnassignedPermissions(objectId: string | null, targetType: number) {
            const url = targetType === PermissionTargetType.User
                ? '/Admin/Permission/GetUnassignedPermissionsByUserId'
                : '/Admin/Permission/GetUnassignedPermissionsByRoleId';
            const payload = targetType === PermissionTargetType.User ? { userId: objectId } : { roleId: objectId };

            return ApiService.post(url, payload, { showLoading: false });
        }

        private getAssignedPermissions(objectId: string | null, targetType: number) {
            const url = targetType === PermissionTargetType.User
                ? '/Admin/Permission/GetAssignedPermissionsByUserId'
                : '/Admin/Permission/GetAssignedPermissionsByRoleId';
            const payload = targetType === PermissionTargetType.User ? { userId: objectId } : { roleId: objectId };

            return ApiService.post(url, payload, { showLoading: false });
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
                    this.loadData();
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
