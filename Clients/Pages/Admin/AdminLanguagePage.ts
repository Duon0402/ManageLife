namespace App {
    interface LanguageModel {
        id: number;
        code: string;
        name: string;
    }

    export class AdminLanguagePage extends BasePage {
        private table: DataTables.Api;
        private languageModalMode: 'create' | 'edit' = 'create';
        private editingLanguageId: number | null = null;

        protected initialize(): void {
            this.initTable();
            this.loadData();
        }

        protected bindEvents(): void {
            this.root.find('#tblLanguage').on('click', '.btn-edit', (e) => {
                const btn = $(e.currentTarget);
                const rowData = this.table.row(btn.closest('tr')).data() as LanguageModel;
                this.editLanguage(rowData);
            });

            this.root.find('#tblLanguage').on('click', '.btn-delete', (e) => {
                const btn = $(e.currentTarget);
                const rowData = this.table.row(btn.closest('tr')).data() as LanguageModel;
                // Implement delete logic if needed, previously it just logged
                console.log("DELETE language:", rowData);
            });

            // Modal buttons (assuming there is a save button in the modal, but the View logic didn't show it explicitly, maybe it's in the partial)
            // The original View didn't show the save logic! It only showed open/reset.
            // I will implement open/reset logic as per original view.
        }

        private loadData(): void {
            LoadingService.show();
            ApiService.get('/Admin/Language/GetListLanguages').then(response => {
                LoadingService.hide();
                if (response.isOk()) {
                    const data = response.data || [];
                    this.table.clear();
                    this.table.rows.add(data);
                    this.table.draw();
                } else {
                    ToastService.error(response.message || 'Error');
                }
            }).catch(() => {
                LoadingService.hide();
                ToastService.error('Không thể tải danh sách user');
            });
        }

        private initTable(): void {
            this.table = this.root.find("#tblLanguage").DataTable({
                buttons: [
                    {
                        text: '<i class="fa-solid fa-plus"></i>',
                        className: 'btn btn-sm btn-outline-secondary',
                        titleAttr: 'Thêm mới',
                        action: () => {
                            this.createLanguage();
                        }
                    }
                ],
                layout: {
                    topEnd: [
                        'buttons',
                        'search'
                    ],
                },
                scrollY: 'calc(100vh - 395px)',
                scrollCollapse: true,
                destroy: true,
                data: [],
                paging: false,
                info: false,
                ordering: false,
                searching: true,
                autoWidth: true,
                columns: [
                    { title: "ID", data: "id", visible: false },
                    { title: "Code", data: "code" },
                    { title: "Name", data: "name" },
                    {
                        title: "",
                        data: null,
                        orderable: false,
                        searchable: false,
                        className: "text-center",
                        width: "120px",
                        render: (data, type, row) => {
                            return `
                                <button class="btn btn-sm btn-outline-primary btn-edit"
                                        data-id="${row.id}"
                                        title="Sửa">
                                    <i class="fa-solid fa-pen-to-square"></i>
                                </button>
                                <button class="btn btn-sm btn-outline-danger btn-delete"
                                        data-id="${row.id}"
                                        title="Xóa">
                                    <i class="fa-solid fa-trash"></i>
                                </button>
                            `;
                        }
                    }
                ]
            } as any);
        }

        private createLanguage(): void {
            this.openLanguageModal(null);
        }

        private editLanguage(language: LanguageModel): void {
            this.openLanguageModal(language);
        }

        private openLanguageModal(data: LanguageModel | null): void {
            this.resetLanguageForm();
            if (data) {
                this.languageModalMode = 'edit';
                this.editingLanguageId = data.id;

                $('#languageModalLabel').text('Cập nhật ngôn ngữ');
                $('#txtCode').val(data.code);
                $('#txtName').val(data.name);
            } else {
                this.languageModalMode = 'create';
                this.editingLanguageId = null;

                $('#languageModalLabel').text('Thêm ngôn ngữ');
            }

            $('#languageModal').modal('show');
        }

        private resetLanguageForm(): void {
            $('#txtCode').val('');
            $('#txtName').val('');
        }
    }
}