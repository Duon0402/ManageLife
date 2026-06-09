namespace App {
    interface HabitModel {
        id: string;
        name: string;
        description: string | null;
        isActive: boolean;
        createdTime: string;
    }

    export class HabitPage extends BasePage {
        private modal!: bootstrap.Modal;
        private habits: HabitModel[] = [];
        private editingId: string | null = null;

        protected initialize(): void {
            this.modal = new bootstrap.Modal(document.getElementById('modal-habit')!);
            this.loadList();
        }

        protected bindEvents(): void {
            this.root.find('#btn-create-habit').on('click', () => this.openCreateModal());
            $('#btn-save-habit').on('click', () => this.saveHabit());

            this.root.on('click', '.btn-edit', (e) => {
                const id = $(e.currentTarget).data('id') as string;
                const habit = this.habits.find(h => h.id === id);
                if (habit) this.openEditModal(habit);
            });

            this.root.on('click', '.btn-delete', (e) => {
                const id = $(e.currentTarget).data('id') as string;
                const habit = this.habits.find(h => h.id === id);
                if (habit) this.deleteHabit(habit);
            });
        }

        private async loadList(): Promise<void> {
            const res = await ApiService.get('/Habit/GetList');
            if (!res.isOk()) {
                this.renderEmpty('Không thể tải danh sách');
                return;
            }
            this.habits = res.data || [];
            this.renderTable();
        }

        private renderTable(): void {
            const $tbody = this.root.find('#habit-tbody');
            $tbody.empty();

            if (!this.habits.length) {
                this.renderEmpty('Chưa có thói quen nào. Hãy thêm thói quen đầu tiên!');
                return;
            }

            this.habits.forEach(habit => {
                const badge = habit.isActive
                    ? `<span class="habit-badge active"><i class="fa-solid fa-circle-check"></i>Đang duy trì</span>`
                    : `<span class="habit-badge inactive"><i class="fa-solid fa-circle-pause"></i>Tạm dừng</span>`;

                $tbody.append(`
                    <tr>
                        <td><span class="habit-name">${this.escape(habit.name)}</span></td>
                        <td><span class="habit-desc">${habit.description ? this.escape(habit.description) : '<span class="text-muted">-</span>'}</span></td>
                        <td class="text-center">${badge}</td>
                        <td>${new Date(habit.createdTime).toLocaleDateString('vi-VN')}</td>
                        <td>
                            <div class="habit-actions">
                                <button class="habit-btn-icon edit btn-edit" data-id="${habit.id}" title="Chỉnh sửa">
                                    <i class="fa-solid fa-pen"></i>
                                </button>
                                <button class="habit-btn-icon delete btn-delete" data-id="${habit.id}" title="Xóa">
                                    <i class="fa-solid fa-trash"></i>
                                </button>
                            </div>
                        </td>
                    </tr>
                `);
            });
        }

        private renderEmpty(msg: string): void {
            this.root.find('#habit-tbody').html(`
                <tr>
                    <td colspan="5" class="text-center py-5 text-muted">
                        <i class="fa-solid fa-repeat fa-2x mb-2 d-block" style="opacity:.3"></i>
                        ${msg}
                    </td>
                </tr>
            `);
        }

        private openCreateModal(): void {
            this.editingId = null;
            $('#modal-habit-title').html('<i class="fa-solid fa-repeat me-2"></i>Thêm thói quen');
            $('#habit-id').val('');
            $('#habit-name').val('');
            $('#habit-description').val('');
            $('#habit-active-wrap').hide();
            this.modal.show();
        }

        private openEditModal(habit: HabitModel): void {
            this.editingId = habit.id;
            $('#modal-habit-title').html('<i class="fa-solid fa-pen me-2"></i>Chỉnh sửa thói quen');
            $('#habit-id').val(habit.id);
            $('#habit-name').val(habit.name);
            $('#habit-description').val(habit.description || '');
            ($('#habit-is-active')[0] as HTMLInputElement).checked = habit.isActive;
            $('#habit-active-wrap').show();
            this.modal.show();
        }

        private async saveHabit(): Promise<void> {
            const name = (($('#habit-name').val() as string) || '').trim();
            if (!name) {
                ToastService.warning('Vui lòng nhập tên thói quen');
                return;
            }

            const description = (($('#habit-description').val() as string) || '').trim() || null;

            LoadingService.show();
            try {
                let res: ApiResponse;

                if (this.editingId) {
                    const isActive = ($('#habit-is-active')[0] as HTMLInputElement).checked;
                    res = await ApiService.post('/Habit/Update', {
                        id: this.editingId,
                        name,
                        description,
                        isActive
                    });
                } else {
                    res = await ApiService.post('/Habit/Create', { name, description });
                }

                if (res.isOk()) {
                    this.modal.hide();
                    ToastService.success(this.editingId ? 'Cập nhật thành công' : 'Thêm thói quen thành công');
                    await this.loadList();
                } else {
                    ToastService.error(res.message || 'Lưu thất bại');
                }
            } catch {
                ToastService.error('Lỗi hệ thống');
            } finally {
                LoadingService.hide();
            }
        }

        private async deleteHabit(habit: HabitModel): Promise<void> {
            await MessageService.confirm(
                `Xóa thói quen <strong>${this.escape(habit.name)}</strong>?`,
                'Xác nhận xóa',
                async () => {
                    LoadingService.show();
                    try {
                        const res = await ApiService.post('/Habit/Delete', { id: habit.id });
                        if (res.isOk()) {
                            ToastService.success('Xóa thành công');
                            await this.loadList();
                        } else {
                            ToastService.error(res.message || 'Xóa thất bại');
                        }
                    } catch {
                        ToastService.error('Lỗi hệ thống');
                    } finally {
                        LoadingService.hide();
                    }
                }
            );
        }

        private escape(str: string): string {
            return str
                .replace(/&/g, '&amp;')
                .replace(/</g, '&lt;')
                .replace(/>/g, '&gt;')
                .replace(/"/g, '&quot;');
        }
    }
}
