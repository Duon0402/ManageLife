namespace App {

    // ─── SHARED HELPERS ──────────────────────────────────────────────────────

    const STATUS_LABEL: Record<TodoStatus, string> = {
        [TodoStatus.Pending]:    '⏳ Chờ làm',
        [TodoStatus.InProgress]: '🔄 Đang làm',
        [TodoStatus.Completed]:  '✅ Hoàn thành',
        [TodoStatus.Cancelled]:  '❌ Huỷ bỏ'
    };
    const STATUS_CSS: Record<TodoStatus, string> = {
        [TodoStatus.Pending]:    'status-pending',
        [TodoStatus.InProgress]: 'status-inprogress',
        [TodoStatus.Completed]:  'status-completed',
        [TodoStatus.Cancelled]:  'status-cancelled'
    };
    const PRIORITY_LABEL: Record<TodoPriority, string> = {
        [TodoPriority.Low]:    '🟢 Thấp',
        [TodoPriority.Medium]: '🟡 TB',
        [TodoPriority.High]:   '🔴 Cao'
    };

    function formatDate(dateStr?: string): string {
        if (!dateStr) return '';
        const d = new Date(dateStr);
        return d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
    }

    function formatDateInput(dateStr?: string): string {
        if (!dateStr) return '';
        return new Date(dateStr).toISOString().split('T')[0];
    }

    function formatDateTimeInput(dateStr?: string): string {
        if (!dateStr) return '';
        const d = new Date(dateStr);
        const offset = d.getTimezoneOffset();
        const local = new Date(d.getTime() - offset * 60000);
        return local.toISOString().slice(0, 16);
    }

    function isLate(dueDate?: string): boolean {
        if (!dueDate) return false;
        return new Date(dueDate) < new Date();
    }

    function escHtml(s: string): string {
        const d = document.createElement('div');
        d.textContent = s;
        return d.innerHTML;
    }

    // ─── TODAY PAGE ──────────────────────────────────────────────────────────

    export class TodoTodayPage extends BasePage {
        private lists: TodoListModel[] = [];
        private tasks: TodoTaskModel[] = [];

        protected initialize(): void {
            const now = new Date();
            const days = ['Chủ nhật','Thứ hai','Thứ ba','Thứ tư','Thứ năm','Thứ sáu','Thứ bảy'];
            this.root.find('#today-date-label').text(
                `${days[now.getDay()]}, ${now.toLocaleDateString('vi-VN', { day: '2-digit', month: 'long', year: 'numeric' })}`
            );
            this.loadData();
        }

        protected bindEvents(): void {
            // Quick add — Enter
            this.root.find('#qa-title').on('keydown', (e) => {
                if (e.key === 'Enter') this.quickAdd();
            });
            this.root.find('#btn-qa').on('click', () => this.quickAdd());

            // Group collapse toggle
            this.root.on('click', '.task-group__hd', (e) => {
                if ($(e.target).closest('.task-action-btn').length) return;
                $(e.currentTarget).closest('.task-group').toggleClass('collapsed');
            });

            // Complete toggle
            this.root.on('click', '.task-check', (e) => {
                e.stopPropagation();
                const $card = $(e.currentTarget).closest('.task-card');
                const id = $card.data('id') as string;
                const isDone = $card.hasClass('done');
                this.changeStatus(id, isDone ? TodoStatus.Pending : TodoStatus.Completed, $card);
            });

            // Delete
            this.root.on('click', '.task-action-btn.del', (e) => {
                e.stopPropagation();
                const id = $(e.currentTarget).data('id') as string;
                this.deleteTask(id);
            });

            // Start (InProgress)
            this.root.on('click', '.task-action-btn.start', (e) => {
                e.stopPropagation();
                const $card = $(e.currentTarget).closest('.task-card');
                const id = $card.data('id') as string;
                this.changeStatus(id, 1, $card);
            });
        }

        private async loadData(): Promise<void> {
            LoadingService.show();
            try {
                const [listsRes, tasksRes] = await Promise.all([
                    ApiService.get<TodoListModel[]>('/Todo/GetLists'),
                    ApiService.get<TodoTaskModel[]>('/Todo/GetTodayTasks')
                ]);

                if (listsRes.isOk()) {
                    this.lists = listsRes.data || [];
                    this.renderListDropdown();
                }

                if (tasksRes.isOk()) {
                    this.tasks = tasksRes.data || [];
                    this.render();
                } else {
                    ToastService.error(tasksRes.message || 'Không thể tải công việc');
                }
            } finally {
                LoadingService.hide();
            }
        }

        private render(): void {
            const inProgress = this.tasks.filter(t => t.status === TodoStatus.InProgress);
            const pending    = this.tasks.filter(t => t.status === TodoStatus.Pending);
            const done       = this.tasks.filter(t => t.status === TodoStatus.Completed || t.status === TodoStatus.Cancelled);
            const total      = this.tasks.length;
            const doneCount  = done.length;

            // Stats
            this.root.find('#stat-total').text(total);
            this.root.find('#stat-inprogress').text(inProgress.length);
            this.root.find('#stat-done').text(doneCount);

            // Progress
            const pct = total === 0 ? 0 : Math.round(doneCount / total * 100);
            this.root.find('#progress-fill').css('width', pct + '%');
            this.root.find('#progress-label').text(`${doneCount}/${total} hoàn thành`);

            // Groups
            this.renderGroup('grp-inprogress', 'list-inprogress', 'cnt-inprogress', inProgress);
            this.renderGroup('grp-pending',    'list-pending',    'cnt-pending',    pending);
            this.renderGroup('grp-done',       'list-done',       'cnt-done',       done);

            this.root.find('#today-empty').toggle(total === 0);
        }

        private renderGroup(grpId: string, listId: string, cntId: string, tasks: TodoTaskModel[]): void {
            const $grp  = this.root.find(`#${grpId}`);
            const $list = this.root.find(`#${listId}`);
            this.root.find(`#${cntId}`).text(tasks.length > 0 ? `(${tasks.length})` : '');
            $grp.toggle(tasks.length > 0);
            $list.html(tasks.map(t => this.buildCard(t)).join(''));
        }

        private buildCard(t: TodoTaskModel): string {
            const isDone  = t.status === TodoStatus.Completed || t.status === TodoStatus.Cancelled;
            const checkIcon = isDone ? '<i class="fa-solid fa-check"></i>' : '';
            const dueHtml   = t.dueDate
                ? `<span class="tag tag-time ${isLate(t.dueDate) && !isDone ? 'late' : ''}">
                       <i class="fa-regular fa-clock me-1"></i>${formatDate(t.dueDate)}
                   </span>` : '';
            const listHtml  = t.todoListName
                ? `<span class="tag tag-list">${escHtml(t.todoListName)}</span>` : '';
            const startBtn  = t.status === TodoStatus.Pending
                ? `<button class="task-action-btn start" data-id="${t.id}" title="Bắt đầu">
                       <i class="fa-solid fa-play"></i>
                   </button>` : '';

            return `
                <div class="task-card ${isDone ? 'done' : ''}" data-id="${t.id}" data-priority="${t.priority}">
                    <div class="task-check">${checkIcon}</div>
                    <div class="task-card__body">
                        <div class="task-card__title">${escHtml(t.title)}</div>
                        <div class="task-card__meta">
                            ${listHtml}
                            ${dueHtml}
                        </div>
                    </div>
                    <div class="task-actions">
                        ${startBtn}
                        <button class="task-action-btn del" data-id="${t.id}" title="Xoá">
                            <i class="fa-solid fa-trash-can"></i>
                        </button>
                    </div>
                </div>`;
        }

        private async quickAdd(): Promise<void> {
            const $input = this.root.find('#qa-title');
            const title  = ($input.val() as string)?.trim();
            if (!title) { $input.focus(); return; }

            const listId   = this.root.find('#qa-list').val() as string;
            const priority = parseInt(this.root.find('#qa-priority').val() as string) as TodoPriority;

            if (!listId) {
                ToastService.error('Vui lòng chọn danh sách');
                return;
            }

            const today = new Date().toISOString().split('T')[0];
            const res = await ApiService.post('/Todo/CreateTask', {
                title, priority,
                todoListId: listId,
                dueDate: today
            });

            if (res.isOk()) {
                $input.val('');
                ToastService.success('Đã thêm công việc');
                this.loadData();
            } else {
                ToastService.error(res.message || 'Không thể thêm');
            }
        }

        private async changeStatus(id: string, status: number, $card: JQuery): Promise<void> {
            const res = await ApiService.post('/Todo/ChangeStatus', { id, status });
            if (res.isOk()) {
                // Optimistic: reload
                this.loadData();
            } else {
                ToastService.error(res.message || 'Không thể cập nhật');
            }
        }

        private async deleteTask(id: string): Promise<void> {
            const ok = await MessageService.confirm('Xoá công việc này?');
            if (!ok) return;
            const res = await ApiService.delete(`/Todo/DeleteTask?id=${encodeURIComponent(id)}`);
            if (res.isOk()) {
                ToastService.success('Đã xoá');
                this.loadData();
            } else {
                ToastService.error(res.message || 'Không thể xoá');
            }
        }

        private renderListDropdown(): void {
            const $form = this.root.find('#quick-add-form');
            const $sel  = this.root.find('#qa-list');

            if (this.lists.length === 0) {
                // Chưa có danh sách — hiện thông báo + nút tạo danh sách (không navigate)
                $form.html(`
                    <i class="fa-solid fa-circle-info" style="color:#c5bbff;flex-shrink:0"></i>
                    <span style="flex:1;font-size:.88rem;color:#999">
                        Bạn chưa có danh sách công việc nào.
                    </span>
                    <button type="button" class="quick-add__btn" id="btn-create-list-today">
                        <i class="fa-solid fa-plus me-1"></i>Tạo danh sách
                    </button>
                `);
                this.root.find('#btn-create-list-today').on('click', () => this.showCreateListModal());
                return;
            }

            $sel.find('option:not(:first)').remove();
            this.lists.forEach(l => $sel.append(`<option value="${l.id}">${escHtml(l.name)}</option>`));
            if (this.lists.length === 1) $sel.val(this.lists[0].id);
        }

        private showCreateListModal(): void {
            const popup = new PopupBuilder({
                title: 'Tạo danh sách mới',
                size: 'sm',
                bodyHtml: `
                    <div class="mb-3">
                        <label class="form-label fw-semibold">Tên danh sách <span class="text-danger">*</span></label>
                        <input type="text" class="form-control" id="new-list-name-today" maxlength="100" placeholder="VD: Công việc, Cá nhân..." />
                    </div>`,
                footerHtml: `
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Huỷ</button>
                    <button type="button" class="btn btn-primary" id="btn-submit-list-today">Tạo</button>`,
                onShow: ($body) => {
                    $body.find('#new-list-name-today').trigger('focus');
                    $body.closest('.modal').find('#btn-submit-list-today').on('click', async () => {
                        const name = ($body.find('#new-list-name-today').val() as string)?.trim();
                        if (!name) { ToastService.error('Vui lòng nhập tên danh sách'); return; }
                        const res = await ApiService.post('/Todo/CreateList', { name });
                        if (res.isOk()) {
                            ToastService.success('Đã tạo danh sách');
                            popup.hide();
                            this.loadData();
                        } else {
                            ToastService.error(res.message || 'Không thể tạo');
                        }
                    });
                }
            });
            popup.show();
        }

        private populateListDropdown($sel: JQuery, lists: TodoListModel[]): void {
            $sel.find('option:not(:first)').remove();
            lists.forEach(l => $sel.append(`<option value="${l.id}">${escHtml(l.name)}</option>`));
        }
    }

    // ─── ALL PAGE ─────────────────────────────────────────────────────────────

    export class TodoAllPage extends BasePage {
        private lists: TodoListModel[] = [];
        private tasks: TodoTaskModel[] = [];
        private activeListId: string = '';
        private detailPanel: bootstrap.Offcanvas | null = null;
        private editingTaskId: string | null = null;

        protected initialize(): void {
            const panelEl = document.getElementById('task-detail-panel');
            if (panelEl) this.detailPanel = new bootstrap.Offcanvas(panelEl);
            this.loadData();
        }

        protected bindEvents(): void {
            // Sidebar list click
            this.root.on('click', '.todo-sidebar__item', (e) => {
                const id = $(e.currentTarget).data('list-id') as string ?? '';
                this.activeListId = id;
                this.root.find('.todo-sidebar__item').removeClass('active');
                $(e.currentTarget).addClass('active');
                this.applyFilters();
            });

            // Create list
            this.root.find('#btn-create-list').on('click', () => this.showCreateListModal());

            // Rename list
            this.root.on('click', '.list-action-btn.edit', (e) => {
                e.stopPropagation();
                const id   = $(e.currentTarget).data('id') as string;
                const name = $(e.currentTarget).data('name') as string;
                this.showRenameListModal(id, name);
            });

            // Delete list
            this.root.on('click', '.list-action-btn.del', (e) => {
                e.stopPropagation();
                const id = $(e.currentTarget).data('id') as string;
                this.deleteList(id);
            });

            // Add task button
            this.root.find('#btn-add-task').on('click', () => this.openDetailPanel(null));

            // Filters
            this.root.find('#filter-status, #filter-priority, #filter-date').on('change', () => this.applyFilters());

            // Task row click → open detail
            this.root.on('click', '.task-row', (e) => {
                if ($(e.target).closest('.task-check, .tr-btn').length) return;
                const id = $(e.currentTarget).data('id') as string;
                const task = this.tasks.find(t => t.id === id);
                if (task) this.openDetailPanel(task);
            });

            // Complete toggle
            this.root.on('click', '.task-row .task-check', (e) => {
                e.stopPropagation();
                const $row  = $(e.currentTarget).closest('.task-row');
                const id    = $row.data('id') as string;
                const isDone = $row.hasClass('done');
                this.changeStatus(id, isDone ? TodoStatus.Pending : TodoStatus.Completed);
            });

            // Delete row
            this.root.on('click', '.tr-btn.del', (e) => {
                e.stopPropagation();
                const id = $(e.currentTarget).data('id') as string;
                this.deleteTask(id);
            });

            // Save from detail panel
            $('#btn-save-task').on('click', () => this.saveTask());

            // Delete from detail panel
            $('#btn-delete-task').on('click', () => {
                if (this.editingTaskId) this.deleteTask(this.editingTaskId);
            });
        }

        private async loadData(): Promise<void> {
            LoadingService.show();
            try {
                const [listsRes, tasksRes] = await Promise.all([
                    ApiService.get<TodoListModel[]>('/Todo/GetLists'),
                    ApiService.get<TodoTaskModel[]>('/Todo/GetList')
                ]);

                if (listsRes.isOk()) {
                    this.lists = listsRes.data || [];
                    this.renderSidebar();
                    this.populateDetailListDropdown();
                }
                if (tasksRes.isOk()) {
                    this.tasks = tasksRes.data || [];
                    this.applyFilters();
                }
            } finally {
                LoadingService.hide();
            }
        }

        private renderSidebar(): void {
            const $list = this.root.find('#sidebar-lists');
            $list.find('.todo-sidebar__item:not([data-list-id=""])').remove();
            this.lists.forEach(l => {
                const cnt = this.tasks.filter(t => t.todoListId === l.id && t.status !== TodoStatus.Completed && t.status !== TodoStatus.Cancelled).length;
                const isActive = this.activeListId === l.id;
                $list.append(`
                    <div class="todo-sidebar__item ${isActive ? 'active' : ''}" data-list-id="${l.id}">
                        <i class="fa-regular fa-folder"></i>
                        <span>${escHtml(l.name)}</span>
                        ${cnt > 0 ? `<span class="count">${cnt}</span>` : ''}
                        <span class="list-actions">
                            <button class="list-action-btn edit" data-id="${l.id}" data-name="${escHtml(l.name)}" title="Đổi tên">
                                <i class="fa-solid fa-pen"></i>
                            </button>
                            <button class="list-action-btn del" data-id="${l.id}" title="Xoá">
                                <i class="fa-solid fa-trash-can"></i>
                            </button>
                        </span>
                    </div>`);
            });
        }

        private applyFilters(): void {
            let filtered = [...this.tasks];

            if (this.activeListId)
                filtered = filtered.filter(t => t.todoListId === this.activeListId);

            const statusVal = this.root.find('#filter-status').val() as string;
            if (statusVal !== '')
                filtered = filtered.filter(t => t.status === parseInt(statusVal));

            const priorityVal = this.root.find('#filter-priority').val() as string;
            if (priorityVal !== '')
                filtered = filtered.filter(t => t.priority === parseInt(priorityVal));

            const dateVal = this.root.find('#filter-date').val() as string;
            if (dateVal)
                filtered = filtered.filter(t => t.dueDate && t.dueDate.startsWith(dateVal));

            this.renderTasks(filtered);
        }

        private renderTasks(tasks: TodoTaskModel[]): void {
            const $list = this.root.find('#task-list-all').empty();
            this.root.find('#all-empty').toggle(tasks.length === 0);

            // Sort: incomplete first, then by priority desc, then due
            const isDoneStatus = (s: TodoStatus) => s === TodoStatus.Completed || s === TodoStatus.Cancelled;
            const sorted = [...tasks].sort((a, b) => {
                if (isDoneStatus(a.status) !== isDoneStatus(b.status)) return isDoneStatus(a.status) ? 1 : -1;
                if (a.priority !== b.priority) return b.priority - a.priority;
                if (a.dueDate && b.dueDate) return new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime();
                return 0;
            });

            sorted.forEach(t => $list.append(this.buildRow(t)));
        }

        private buildRow(t: TodoTaskModel): string {
            const isDone = t.status === TodoStatus.Completed || t.status === TodoStatus.Cancelled;
            const checkIcon = isDone ? '<i class="fa-solid fa-check"></i>' : '';
            const dueHtml = t.dueDate
                ? `<span class="badge-due ${isLate(t.dueDate) && !isDone ? 'late' : ''}">
                       <i class="fa-regular fa-clock me-1"></i>${formatDate(t.dueDate)}
                   </span>` : '';
            const listHtml = t.todoListName
                ? `<span class="badge-list">${escHtml(t.todoListName)}</span>` : '';

            return `
                <div class="task-row ${isDone ? 'done' : ''}" data-id="${t.id}" data-priority="${t.priority}">
                    <div class="task-check">${checkIcon}</div>
                    <div class="tr__body">
                        <div class="tr__title">${escHtml(t.title)}</div>
                        <div class="tr__meta">
                            <span class="badge-status ${STATUS_CSS[t.status]}">${STATUS_LABEL[t.status]}</span>
                            ${listHtml}
                            ${dueHtml}
                        </div>
                    </div>
                    <div class="tr__actions">
                        <button class="tr-btn del" data-id="${t.id}" title="Xoá">
                            <i class="fa-solid fa-trash-can"></i>
                        </button>
                    </div>
                </div>`;
        }

        private openDetailPanel(task: TodoTaskModel | null): void {
            this.editingTaskId = task?.id ?? null;
            $('#detail-id').val(task?.id ?? '');
            $('#detail-title').val(task?.title ?? '');
            $('#detail-desc').val(task?.description ?? '');
            $('#detail-status').val(task?.status ?? TodoStatus.Pending);
            $('#detail-priority').val(task?.priority ?? TodoPriority.Medium);
            $('#detail-start').val(formatDateInput(task?.startDate));
            $('#detail-due').val(formatDateInput(task?.dueDate));
            $('#detail-estimate').val(task?.estimatedMinutes ?? '');
            $('#detail-reminder').val(formatDateTimeInput(task?.reminderAt));
            $('#detail-recurrence').val(task?.recurrence ?? RecurrenceType.None);
            $('#detail-list-id').val(task?.todoListId ?? (this.lists[0]?.id ?? ''));
            $('#btn-delete-task').toggle(!!task);
            this.detailPanel?.show();
        }

        private async saveTask(): Promise<void> {
            const id    = ($('#detail-id').val() as string)?.trim();
            const title = ($('#detail-title').val() as string)?.trim();
            if (!title) { ToastService.error('Vui lòng nhập tiêu đề'); return; }

            const payload = {
                id,
                title,
                description: ($('#detail-desc').val() as string)?.trim() || null,
                status:       parseInt($('#detail-status').val() as string) as TodoStatus,
                priority:     parseInt($('#detail-priority').val() as string) as TodoPriority,
                startDate:    $('#detail-start').val() || null,
                dueDate:      $('#detail-due').val() || null,
                estimatedMinutes: parseInt($('#detail-estimate').val() as string) || null,
                reminderAt:   $('#detail-reminder').val() || null,
                recurrence:   parseInt($('#detail-recurrence').val() as string) as RecurrenceType,
                todoListId:   $('#detail-list-id').val() as string
            };

            const isNew = !id;
            const res = isNew
                ? await ApiService.post('/Todo/CreateTask', payload)
                : await ApiService.post('/Todo/UpdateTask', payload);

            if (res.isOk()) {
                ToastService.success(isNew ? 'Đã thêm công việc' : 'Đã cập nhật');
                this.detailPanel?.hide();
                this.loadData();
            } else {
                ToastService.error(res.message || 'Không thể lưu');
            }
        }

        private async changeStatus(id: string, status: number): Promise<void> {
            const res = await ApiService.post('/Todo/ChangeStatus', { id, status });
            if (res.isOk()) {
                const task = this.tasks.find(t => t.id === id);
                if (task) { task.status = status; this.applyFilters(); this.renderSidebar(); }
            } else {
                ToastService.error(res.message || 'Không thể cập nhật');
            }
        }

        private async deleteTask(id: string): Promise<void> {
            const ok = await MessageService.confirm('Xoá công việc này?');
            if (!ok) return;
            const res = await ApiService.delete(`/Todo/DeleteTask?id=${encodeURIComponent(id)}`);
            if (res.isOk()) {
                this.detailPanel?.hide();
                this.tasks = this.tasks.filter(t => t.id !== id);
                this.applyFilters();
                this.renderSidebar();
                ToastService.success('Đã xoá');
            } else {
                ToastService.error(res.message || 'Không thể xoá');
            }
        }

        private async showCreateListModal(): Promise<void> {
            const popup = new PopupBuilder({
                title: 'Tạo danh sách mới',
                size: 'sm',
                bodyHtml: `
                    <div class="mb-3">
                        <label class="form-label fw-semibold">Tên danh sách <span class="text-danger">*</span></label>
                        <input type="text" class="form-control" id="new-list-name" maxlength="100" placeholder="VD: Công việc, Cá nhân..." />
                    </div>
                    <div class="mb-3">
                        <label class="form-label fw-semibold">Mô tả</label>
                        <textarea class="form-control" id="new-list-desc" rows="2" maxlength="500"></textarea>
                    </div>`,
                footerHtml: `
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Huỷ</button>
                    <button type="button" class="btn btn-primary" id="btn-submit-list">Tạo</button>`,
                onShow: ($body) => {
                    $body.find('#new-list-name').trigger('focus');
                    $body.closest('.modal').find('#btn-submit-list').on('click', async () => {
                        const name = ($body.find('#new-list-name').val() as string)?.trim();
                        if (!name) { ToastService.error('Vui lòng nhập tên danh sách'); return; }
                        const res = await ApiService.post('/Todo/CreateList', {
                            name,
                            description: ($body.find('#new-list-desc').val() as string)?.trim() || null
                        });
                        if (res.isOk()) {
                            ToastService.success('Đã tạo danh sách');
                            popup.hide();
                            this.loadData();
                        } else {
                            ToastService.error(res.message || 'Không thể tạo');
                        }
                    });
                }
            });
            popup.show();
        }

        private showRenameListModal(id: string, currentName: string): void {
            const popup = new PopupBuilder({
                title: 'Đổi tên danh sách',
                size: 'sm',
                bodyHtml: `
                    <div class="mb-3">
                        <label class="form-label fw-semibold">Tên mới <span class="text-danger">*</span></label>
                        <input type="text" class="form-control" id="rename-list-input" maxlength="100" value="${escHtml(currentName)}" />
                    </div>`,
                footerHtml: `
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Huỷ</button>
                    <button type="button" class="btn btn-primary" id="btn-submit-rename">Lưu</button>`,
                onShow: ($body) => {
                    const $input = $body.find('#rename-list-input');
                    $input.trigger('focus').trigger('select');
                    $body.closest('.modal').find('#btn-submit-rename').on('click', async () => {
                        const name = ($input.val() as string)?.trim();
                        if (!name) { ToastService.error('Vui lòng nhập tên'); return; }
                        const res = await ApiService.post('/Todo/UpdateList', { id, name });
                        if (res.isOk()) {
                            ToastService.success('Đã đổi tên');
                            popup.hide();
                            this.loadData();
                        } else {
                            ToastService.error(res.message || 'Không thể đổi tên');
                        }
                    });
                }
            });
            popup.show();
        }

        private async deleteList(id: string): Promise<void> {
            const list = this.lists.find(l => l.id === id);
            const taskCount = this.tasks.filter(t => t.todoListId === id).length;
            const msg = taskCount > 0
                ? `Xoá danh sách "${list?.name}"? Danh sách còn ${taskCount} công việc.`
                : `Xoá danh sách "${list?.name}"?`;

            const ok = await MessageService.confirm(msg);
            if (!ok) return;

            const res = await ApiService.delete(`/Todo/DeleteList?id=${encodeURIComponent(id)}`);
            if (res.isOk()) {
                if (this.activeListId === id) this.activeListId = '';
                ToastService.success('Đã xoá danh sách');
                this.loadData();
            } else {
                ToastService.error(res.message || 'Không thể xoá');
            }
        }

        private populateDetailListDropdown(): void {
            const $sel = $('#detail-list-id').empty();
            this.lists.forEach(l => $sel.append(`<option value="${l.id}">${escHtml(l.name)}</option>`));
        }
    }
}
