namespace App {

    export class NoteIndexPage extends BasePage {
        private notes: NoteModel[] = [];
        private tags: NoteTagModel[] = [];
        private tagModal!: bootstrap.Modal;

        protected initialize(): void {
            this.tagModal = new bootstrap.Modal(document.getElementById('modal-tags')!);
            this.loadAll();
        }

        protected bindEvents(): void {
            this.root.find('#btn-new-note').on('click', () => this.createNote());
            this.root.find('#btn-manage-tags').on('click', () => this.tagModal.show());
            this.root.find('#ni-search').on('input', () => this.renderNotes());

            this.root.on('click', '.ni-card-del', (e) => {
                e.stopPropagation();
                const id = $(e.currentTarget).data('id') as string;
                this.deleteNote(id);
            });

            this.root.on('click', '.ni-card', (e) => {
                if ($(e.target).closest('.ni-card-del').length) return;
                const id = $(e.currentTarget).data('id') as string;
                window.location.href = `/Note/Edit?id=${id}`;
            });

            $('#btn-add-tag').on('click', () => this.addTag());
            $('#tag-name-input').on('keydown', (e) => { if (e.key === 'Enter') this.addTag(); });
        }

        private async loadAll(): Promise<void> {
            LoadingService.show();
            try {
                const [notesRes, tagsRes] = await Promise.all([
                    ApiService.get('/Note/GetList'),
                    ApiService.get('/Note/GetTags')
                ]);
                this.notes = notesRes.isOk() ? notesRes.data || [] : [];
                this.tags = tagsRes.isOk() ? tagsRes.data || [] : [];
                this.renderNotes();
                this.renderTagList();
            } catch {
                ToastService.error('Lỗi hệ thống');
            } finally {
                LoadingService.hide();
            }
        }

        private renderNotes(): void {
            const q = (this.root.find('#ni-search').val() as string).toLowerCase().trim();
            const filtered = q
                ? this.notes.filter(n => n.title.toLowerCase().includes(q) || (n.content || '').toLowerCase().includes(q))
                : this.notes;

            const $grid = this.root.find('#ni-notes-grid');
            const $empty = this.root.find('#ni-empty');
            $grid.empty();

            if (!filtered.length) {
                $grid.addClass('d-none');
                $empty.removeClass('d-none');
                return;
            }

            $grid.removeClass('d-none');
            $empty.addClass('d-none');

            filtered.forEach(note => {
                const tagHtml = note.tags.slice(0, 3)
                    .map(t => `<span class="ni-tag" style="background:${t.color}">${t.name}</span>`)
                    .join('');
                const preview = (note.content || '').replace(/[#*`>\-\[\]]/g, '').trim().slice(0, 80);
                const date = new Date(note.updatedTime || note.createdTime).toLocaleDateString('vi-VN');

                $grid.append(`
                    <div class="ni-card" data-id="${note.id}">
                        <div class="ni-card-title">${note.title || 'Không có tiêu đề'}</div>
                        <div class="ni-card-preview">${preview || '<em style="opacity:.5">Chưa có nội dung</em>'}</div>
                        <div class="ni-card-footer">
                            <div class="ni-card-tags">${tagHtml}</div>
                            <div style="display:flex;align-items:center;gap:.5rem">
                                <span class="ni-card-date">${date}</span>
                                <button class="ni-card-del" data-id="${note.id}" title="Xóa"><i class="fa-solid fa-trash"></i></button>
                            </div>
                        </div>
                    </div>
                `);
            });
        }

        private async createNote(): Promise<void> {
            LoadingService.show();
            try {
                const res = await ApiService.post('/Note/Create', { title: 'Ghi chú mới', content: '', tagIds: [] });
                if (res.isOk()) {
                    await this.loadAll();
                    ToastService.success('Đã tạo ghi chú');
                } else {
                    ToastService.error(res.message || 'Tạo thất bại');
                }
            } finally {
                LoadingService.hide();
            }
        }

        private async deleteNote(id: string): Promise<void> {
            await MessageService.confirm('Xóa ghi chú này?', 'Xác nhận', async () => {
                LoadingService.show();
                try {
                    const res = await ApiService.post('/Note/Delete', JSON.stringify(id));
                    if (res.isOk()) {
                        this.notes = this.notes.filter(n => n.id !== id);
                        this.renderNotes();
                        ToastService.success('Đã xóa');
                    } else {
                        ToastService.error(res.message || 'Xóa thất bại');
                    }
                } finally {
                    LoadingService.hide();
                }
            });
        }

        private renderTagList(): void {
            const $list = $('#tag-list');
            $list.empty();
            if (!this.tags.length) {
                $list.html('<span class="text-muted small">Chưa có tag nào</span>');
                return;
            }
            this.tags.forEach(tag => {
                $list.append(`
                    <span class="tag-chip" style="background:${tag.color}">
                        ${tag.name}
                        <button class="tag-del" data-id="${tag.id}" title="Xóa tag">
                            <i class="fa-solid fa-xmark"></i>
                        </button>
                    </span>
                `);
            });

            $list.off('click', '.tag-del').on('click', '.tag-del', async (e) => {
                const id = $(e.currentTarget).data('id') as string;
                await this.deleteTag(id);
            });
        }

        private async addTag(): Promise<void> {
            const name = ($('#tag-name-input').val() as string).trim();
            const color = $('#tag-color-input').val() as string;
            if (!name) return;

            LoadingService.show();
            try {
                const res = await ApiService.post('/NoteTag/Create', { name, color });
                if (res.isOk()) {
                    $('#tag-name-input').val('');
                    const tagsRes = await ApiService.get('/Note/GetTags');
                    this.tags = tagsRes.isOk() ? tagsRes.data || [] : [];
                    this.renderTagList();
                    ToastService.success('Đã thêm tag');
                } else {
                    ToastService.error(res.message || 'Thêm thất bại');
                }
            } finally {
                LoadingService.hide();
            }
        }

        private async deleteTag(id: string): Promise<void> {
            await MessageService.confirm('Xóa tag này?', 'Xác nhận', async () => {
                LoadingService.show();
                try {
                    const res = await ApiService.post('/NoteTag/Delete', JSON.stringify(id));
                    if (res.isOk()) {
                        this.tags = this.tags.filter(t => t.id !== id);
                        this.renderTagList();
                        ToastService.success('Đã xóa tag');
                    } else {
                        ToastService.error(res.message || 'Xóa thất bại');
                    }
                } finally {
                    LoadingService.hide();
                }
            });
        }
    }
}
