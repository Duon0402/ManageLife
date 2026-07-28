namespace App {

    export class NoteEditPage extends BasePage {
        private noteId!: string;
        private note!: NoteDetailModel;
        private allTags: NoteTagModel[] = [];
        private allNotes: NoteModel[] = [];
        private editor!: EditorBuilder;
        private linkModal!: bootstrap.Modal;
        private isDirty = false;

        protected initialize(): void {
            this.noteId = this.root.data('note-id') as string;
            this.linkModal = new bootstrap.Modal(document.getElementById('modal-link-picker')!);

            this.editor = new EditorBuilder('#ne-editor-container')
                .setMinHeight('500px')
                .setPlaceholder('Bắt đầu viết ghi chú của bạn...')
                .onChange(() => { this.isDirty = true; })
                .build();

            this.loadNote();
        }

        protected bindEvents(): void {
            this.root.find('#btn-save-note').on('click', () => this.saveNote());
            this.root.find('#btn-delete-note').on('click', () => this.deleteNote());
            this.root.find('#btn-open-tag-picker').on('click', () => this.toggleTagPicker());
            this.root.find('#btn-open-link-picker').on('click', () => this.openLinkPicker());
            $('#link-picker-search').on('input', () => this.filterLinkPicker());

            this.root.find('#ne-title').on('input', () => { this.isDirty = true; });

            $(window).on('keydown', (e) => {
                if ((e.ctrlKey || e.metaKey) && e.key === 's') {
                    e.preventDefault();
                    this.saveNote();
                }
            });
        }

        private async loadNote(): Promise<void> {
            LoadingService.show();
            try {
                const [noteRes, tagsRes, notesRes] = await Promise.all([
                    ApiService.get('/Note/GetById', { id: this.noteId }),
                    ApiService.get('/Note/GetTags'),
                    ApiService.get('/Note/GetList')
                ]);

                if (!noteRes.isOk()) {
                    ToastService.error('Không tìm thấy ghi chú');
                    setTimeout(() => window.location.href = '/Note', 1500);
                    return;
                }

                this.note = noteRes.data;
                this.allTags = tagsRes.isOk() ? tagsRes.data || [] : [];
                this.allNotes = (notesRes.isOk() ? notesRes.data || [] : [])
                    .filter((n: NoteModel) => n.id !== this.noteId);

                this.root.find('#ne-title').val(this.note.title);
                this.editor.setValue(this.note.content || '');
                this.isDirty = false;

                this.renderAssignedTags();
                this.renderLinkedNotes();
                this.renderBacklinks();
            } catch {
                ToastService.error('Lỗi hệ thống');
            } finally {
                LoadingService.hide();
            }
        }

        private async saveNote(): Promise<void> {
            const title = (this.root.find('#ne-title').val() as string).trim();
            if (!title) { ToastService.warning('Vui lòng nhập tiêu đề'); return; }

            const tagIds = this.note.tags.map(t => t.id);

            LoadingService.show();
            try {
                const res = await ApiService.post('/Note/Update', {
                    id: this.noteId,
                    title,
                    content: this.editor.getValue(),
                    tagIds
                });
                if (res.isOk()) {
                    this.note.title = title;
                    this.note.content = this.editor.getValue();
                    this.isDirty = false;
                    ToastService.success('Đã lưu');
                } else {
                    ToastService.error(res.message || 'Lưu thất bại');
                }
            } finally {
                LoadingService.hide();
            }
        }

        private async deleteNote(): Promise<void> {
            await MessageService.confirm('Xóa ghi chú này?', 'Xác nhận', async () => {
                LoadingService.show();
                try {
                    const res = await ApiService.post('/Note/Delete', JSON.stringify(this.noteId));
                    if (res.isOk()) window.location.href = '/Note';
                    else ToastService.error(res.message || 'Xóa thất bại');
                } finally {
                    LoadingService.hide();
                }
            });
        }

        // ── Tags ──────────────────────────────────────────────────────────────

        private renderAssignedTags(): void {
            const $wrap = this.root.find('#ne-assigned-tags');
            $wrap.empty();
            if (!this.note.tags.length) {
                $wrap.html('<span class="ne-empty-panel">Chưa có tag</span>');
                return;
            }
            this.note.tags.forEach(tag => {
                $wrap.append(`
                    <span class="ne-tag-item" style="background:${tag.color}">
                        ${tag.name}
                        <button class="remove-tag" data-id="${tag.id}" title="Gỡ tag">
                            <i class="fa-solid fa-xmark"></i>
                        </button>
                    </span>
                `);
            });

            $wrap.off('click', '.remove-tag').on('click', '.remove-tag', (e) => {
                const id = $(e.currentTarget).data('id') as string;
                this.removeTag(id);
            });
        }

        private toggleTagPicker(): void {
            const $picker = this.root.find('#ne-tag-picker');
            if ($picker.hasClass('d-none')) {
                $picker.empty().removeClass('d-none');
                const assignedIds = new Set(this.note.tags.map(t => t.id));
                this.allTags.forEach(tag => {
                    const assigned = assignedIds.has(tag.id);
                    $picker.append(`
                        <span class="ne-tag-option ${assigned ? 'assigned' : ''}"
                              data-id="${tag.id}" style="background:${tag.color}" title="${assigned ? 'Gỡ tag' : 'Gán tag'}">
                            ${tag.name}
                        </span>
                    `);
                });
                $picker.off('click', '.ne-tag-option').on('click', '.ne-tag-option', (e) => {
                    const id = $(e.currentTarget).data('id') as string;
                    const assigned = this.note.tags.some(t => t.id === id);
                    if (assigned) this.removeTag(id); else this.assignTag(id);
                });
            } else {
                $picker.addClass('d-none');
            }
        }

        private removeTag(tagId: string): void {
            this.note.tags = this.note.tags.filter(t => t.id !== tagId);
            this.isDirty = true;
            this.renderAssignedTags();
            this.root.find('#ne-tag-picker').addClass('d-none');
        }

        private assignTag(tagId: string): void {
            const tag = this.allTags.find(t => t.id === tagId);
            if (!tag || this.note.tags.some(t => t.id === tagId)) return;
            this.note.tags.push(tag);
            this.isDirty = true;
            this.renderAssignedTags();
            this.root.find('#ne-tag-picker').addClass('d-none');
        }

        // ── Links ─────────────────────────────────────────────────────────────

        private renderLinkedNotes(): void {
            const $list = this.root.find('#ne-linked-notes');
            $list.empty();
            if (!this.note.linkedNotes.length) {
                $list.html('<div class="ne-empty-panel">Chưa có link</div>');
                return;
            }
            this.note.linkedNotes.forEach(n => {
                $list.append(`
                    <div class="ne-link-item">
                        <a href="/Note/Edit?id=${n.id}">${n.title || 'Không có tiêu đề'}</a>
                        <button class="unlink-btn" data-target="${n.id}" title="Gỡ link">
                            <i class="fa-solid fa-xmark"></i>
                        </button>
                    </div>
                `);
            });
            $list.off('click', '.unlink-btn').on('click', '.unlink-btn', async (e) => {
                const targetId = $(e.currentTarget).data('target') as string;
                await this.removeLink(targetId);
            });
        }

        private renderBacklinks(): void {
            const $list = this.root.find('#ne-backlink-notes');
            $list.empty();
            if (!this.note.backlinkNotes.length) {
                $list.html('<div class="ne-empty-panel">Không có backlink</div>');
                return;
            }
            this.note.backlinkNotes.forEach(n => {
                $list.append(`
                    <a href="/Note/Edit?id=${n.id}" class="ne-link-item">
                        ${n.title || 'Không có tiêu đề'}
                    </a>
                `);
            });
        }

        private openLinkPicker(): void {
            this.filterLinkPicker();
            this.linkModal.show();
        }

        private filterLinkPicker(): void {
            const q = ($('#link-picker-search').val() as string).toLowerCase().trim();
            const linkedIds = new Set(this.note.linkedNotes.map(n => n.id));
            const filtered = this.allNotes.filter(n =>
                !linkedIds.has(n.id) && (n.title.toLowerCase().includes(q) || !q));

            const $list = $('#link-picker-list');
            $list.empty();

            if (!filtered.length) {
                $list.html('<div class="text-muted small p-2">Không tìm thấy</div>');
                return;
            }

            filtered.forEach(n => {
                $list.append(`
                    <div class="link-picker-item" data-id="${n.id}">
                        ${n.title || 'Không có tiêu đề'}
                    </div>
                `);
            });

            $list.off('click', '.link-picker-item').on('click', '.link-picker-item', async (e) => {
                const targetId = $(e.currentTarget).data('id') as string;
                this.linkModal.hide();
                await this.addLink(targetId);
            });
        }

        private async addLink(targetId: string): Promise<void> {
            LoadingService.show();
            try {
                const res = await ApiService.post('/Note/AddLink', {
                    sourceNoteId: this.noteId,
                    targetNoteId: targetId
                });
                if (res.isOk()) {
                    const target = this.allNotes.find(n => n.id === targetId);
                    if (target) this.note.linkedNotes.push(target);
                    this.renderLinkedNotes();
                    ToastService.success('Đã thêm link');
                } else {
                    ToastService.error(res.message || 'Thêm link thất bại');
                }
            } finally {
                LoadingService.hide();
            }
        }

        private async removeLink(targetId: string): Promise<void> {
            LoadingService.show();
            try {
                const res = await ApiService.post('/Note/RemoveLink', {
                    sourceNoteId: this.noteId,
                    targetNoteId: targetId
                });
                if (res.isOk()) {
                    this.note.linkedNotes = this.note.linkedNotes.filter(n => n.id !== targetId);
                    this.renderLinkedNotes();
                    ToastService.success('Đã gỡ link');
                } else {
                    ToastService.error(res.message || 'Gỡ link thất bại');
                }
            } finally {
                LoadingService.hide();
            }
        }
    }
}
