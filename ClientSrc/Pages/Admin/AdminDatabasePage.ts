namespace App {

    export class AdminDatabasePage extends BasePage {
        private pendingCount: number = 0;
        private pending: string[] = [];
        private applied: string[] = [];

        protected initialize(): void {
            const $el = this.root;
            this.pendingCount = parseInt($el.data('pending-count') ?? '0');
            try { this.pending = JSON.parse($el.attr('data-pending') || '[]'); } catch { this.pending = []; }
            try { this.applied = JSON.parse($el.attr('data-applied') || '[]'); } catch { this.applied = []; }

            this.render();

            if (this.pendingCount > 0) {
                this.addPageAction({
                    label: `Apply ${this.pendingCount} migration(s)`,
                    icon: 'fa-play',
                    className: 'btn-warning',
                    onClick: () => this.applyMigrations()
                });
            }
        }

        private render(): void {
            const statusHtml = this.pendingCount === 0
                ? `<div class="db-status ok">
                       <i class="fa-solid fa-circle-check"></i>
                       <span>Database đã được cập nhật đầy đủ</span>
                   </div>`
                : `<div class="db-status warn">
                       <i class="fa-solid fa-triangle-exclamation"></i>
                       <span>Có <strong>${this.pendingCount}</strong> migration chưa được apply</span>
                   </div>`;

            const pendingRows = this.pending.length === 0
                ? `<div class="db-empty">Không có migration nào đang chờ</div>`
                : this.pending.map(m => `
                    <div class="db-migration-row pending">
                        <i class="fa-solid fa-circle-dot text-warning"></i>
                        <span>${this.formatMigrationName(m)}</span>
                        <small class="text-muted ms-auto">${m}</small>
                    </div>`).join('');

            const appliedRows = this.applied.map((m, i) => `
                <div class="db-migration-row applied ${i === 0 ? 'latest' : ''}">
                    <i class="fa-solid fa-circle-check text-success"></i>
                    <span>${this.formatMigrationName(m)}</span>
                    <small class="text-muted ms-auto">${m}</small>
                </div>`).join('');

            this.root.html(`
                <div class="db-page p-3 h-100 overflow-auto">
                    ${statusHtml}

                    <div class="db-section">
                        <div class="db-section-title">
                            <i class="fa-solid fa-clock-rotate-left text-warning me-2"></i>
                            Pending <span class="db-badge warn">${this.pending.length}</span>
                        </div>
                        <div class="db-list">${pendingRows}</div>
                    </div>

                    <div class="db-section">
                        <div class="db-section-title">
                            <i class="fa-solid fa-circle-check text-success me-2"></i>
                            Applied <span class="db-badge ok">${this.applied.length}</span>
                        </div>
                        <div class="db-list">${appliedRows}</div>
                    </div>
                </div>

                <style>
                    .db-page { font-size: .88rem; }

                    .db-status {
                        display: flex; align-items: center; gap: 10px;
                        padding: 12px 16px;
                        border-radius: 8px;
                        font-weight: 600;
                        margin-bottom: 20px;
                    }
                    .db-status.ok { background: #eafaf1; color: #1e8449; }
                    .db-status.warn { background: #fef9e7; color: #b7950b; }
                    .db-status i { font-size: 1.1rem; }

                    .db-section { margin-bottom: 20px; }
                    .db-section-title {
                        font-size: .72rem;
                        font-weight: 700;
                        text-transform: uppercase;
                        letter-spacing: .08em;
                        color: #b0aac8;
                        margin-bottom: 8px;
                        display: flex;
                        align-items: center;
                    }
                    .db-badge {
                        font-size: .7rem;
                        padding: 1px 7px;
                        border-radius: 99px;
                        font-weight: 700;
                        margin-left: 6px;
                    }
                    .db-badge.ok { background: #eafaf1; color: #1e8449; }
                    .db-badge.warn { background: #fef9e7; color: #b7950b; }

                    .db-migration-row {
                        display: flex; align-items: center; gap: 10px;
                        padding: 8px 12px;
                        border-radius: 6px;
                        border: 1px solid #f0edf8;
                        margin-bottom: 4px;
                        background: #fff;
                        transition: background .15s;
                    }
                    .db-migration-row:hover { background: #faf8ff; }
                    .db-migration-row.latest { border-color: #c5bbff; }
                    .db-migration-row span { font-weight: 500; color: #2c2c54; }
                    .db-migration-row small { font-size: .72rem; white-space: nowrap; }

                    .db-empty {
                        padding: 16px 12px;
                        color: #aaa;
                        font-size: .85rem;
                        text-align: center;
                        border: 1px dashed #e0ddf5;
                        border-radius: 6px;
                    }
                </style>
            `);
        }

        private formatMigrationName(raw: string): string {
            // "20260531113958_AddTodoTaskReminderFields" → "Add Todo Task Reminder Fields"
            const parts = raw.split('_');
            if (parts.length < 2) return raw;
            const name = parts.slice(1).join('_');
            return name.replace(/([A-Z])/g, ' $1').trim();
        }

        private async applyMigrations(): Promise<void> {
            const confirmed = await MessageService.confirm(
                `Apply ${this.pendingCount} migration(s) vào database?`
            );
            if (!confirmed) return;

            LoadingService.show();
            try {
                const res = await ApiService.post('/Admin/Database/Migrate', {});
                if (res.isOk()) {
                    ToastService.success(res.message || 'Migration thành công');
                    setTimeout(() => location.reload(), 1200);
                } else {
                    ToastService.error(res.message || 'Migration thất bại');
                }
            } catch {
                ToastService.error('Không thể kết nối server');
            } finally {
                LoadingService.hide();
            }
        }
    }
}
