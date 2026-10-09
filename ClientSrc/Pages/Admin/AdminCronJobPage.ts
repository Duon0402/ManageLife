namespace App {
    interface CronJobModel {
        jobId: number;
        title: string;
        url: string;
        enabled: boolean;
        requestMethod: number;
        hours: string;
        minutes: string;
        mdays: string;
        months: string;
        wdays: string;
        scheduleText: string;
        timezone: string;
        body?: string;
        attachCronSecret: boolean;
        isSystem: boolean;
        lastStatus: number;
        lastStatusText: string;
        lastExecution?: string;
        nextExecution?: string;
    }

    interface CronJobHistoryModel {
        date: string;
        duration: number;
        status: number;
        statusText: string;
        httpStatus: number;
    }

    /** Mẫu lịch: chọn trong form sẽ điền sẵn các ô Giờ/Phút/Ngày/Tháng/Thứ. */
    const SCHEDULE_PRESETS: Record<string, Partial<CronJobModel>> = {
        daily7: { hours: '7', minutes: '0', mdays: '*', months: '*', wdays: '*' },
        daily3: { hours: '3', minutes: '0', mdays: '*', months: '*', wdays: '*' },
        hourly: { hours: '*', minutes: '0', mdays: '*', months: '*', wdays: '*' },
        every15: { hours: '*', minutes: '0,15,30,45', mdays: '*', months: '*', wdays: '*' },
        weekdays8: { hours: '8', minutes: '0', mdays: '*', months: '*', wdays: '1,2,3,4,5' },
        monthly1: { hours: '7', minutes: '0', mdays: '1', months: '*', wdays: '*' },
    };

    const METHODS = ['GET', 'POST', 'OPTIONS', 'HEAD', 'PUT', 'DELETE', 'TRACE', 'CONNECT', 'PATCH'];
    const FORM_ID = 'cronJobForm';

    export class AdminCronJobPage extends BasePage {
        private gridBuilder!: GridBuilder<CronJobModel>;

        protected initialize(): void {
            this.initGrid();
            this.bindSchedulePreset();
        }

        private initGrid(): void {
            const scheduleHelp = 'Số cách nhau dấu phẩy, * = mọi';
            this.gridBuilder = new GridBuilder<CronJobModel>('#cronJobsTable')
                .setDataSource({ url: '/Admin/CronJob/GetList' })
                .addColumn({ field: 'jobId', title: 'ID' })
                .addColumn({
                    field: 'title',
                    title: 'Tiêu đề',
                    render: (data, _type, row) =>
                        Utils.escapeHtml(data) + (row.isSystem ? ' <span class="badge bg-info ms-1">Hệ thống</span>' : '')
                })
                .addColumn({
                    field: 'url',
                    title: 'URL',
                    render: (data, _type, row) =>
                        `<code>${METHODS[row.requestMethod] ?? '?'}</code> <span class="text-break">${Utils.escapeHtml(data)}</span>`
                })
                .addColumn({
                    field: 'scheduleText',
                    title: 'Lịch',
                    render: (data, _type, row) => Utils.escapeHtml(data)
                        + (row.timezone !== 'Asia/Ho_Chi_Minh' ? ` <small class="text-muted">(${Utils.escapeHtml(row.timezone)})</small>` : '')
                })
                .addColumn({
                    field: 'enabled',
                    title: 'Trạng thái',
                    render: (data) => data
                        ? '<span class="badge bg-success">Hoạt động</span>'
                        : '<span class="badge bg-secondary">Tạm dừng</span>'
                })
                .addColumn({
                    field: 'lastStatusText',
                    title: 'Lần chạy gần nhất',
                    render: (data, _type, row) => {
                        if (!row.lastExecution) return '<span class="text-muted">Chưa chạy</span>';
                        const color = row.lastStatus === 1 ? 'text-success' : 'text-danger';
                        return `<span class="${color}">${Utils.escapeHtml(data)}</span><br><small class="text-muted">${row.lastExecution}</small>`;
                    }
                })
                .addColumn({
                    field: 'nextExecution',
                    title: 'Lần chạy tới',
                    render: (data) => data ?? '<span class="text-muted">—</span>'
                })
                .addToolbarButton({
                    icon: 'fa-plus',
                    className: 'btn btn-sm btn-outline-secondary',
                    title: 'Thêm mới',
                    onClick: () => this.gridBuilder.getFormBuilder()?.showCreate()
                })
                .addToolbarButton({
                    icon: 'fa-rotate',
                    className: 'btn btn-sm btn-outline-primary',
                    title: 'Đồng bộ job hệ thống',
                    onClick: () => this.syncSystemJobs()
                })
                .addActionButton({
                    icon: 'fa-pen-to-square',
                    className: 'btn-outline-primary',
                    title: 'Sửa',
                    onClick: (data) => this.editJob(data)
                })
                .addActionButton({
                    icon: 'fa-clock-rotate-left',
                    className: 'btn-outline-secondary',
                    title: 'Lịch sử chạy',
                    onClick: (data) => this.showHistory(data)
                })
                .addActionButton({
                    icon: 'fa-trash',
                    className: 'btn-outline-danger',
                    title: 'Xóa',
                    onClick: (data) => this.deleteJob(data)
                })
                .setOptions({ autoWidth: false, ordering: true })
                .setForm({
                    id: FORM_ID,
                    size: 'lg',
                    columns: 2,
                    createTitle: 'Thêm cron job',
                    editTitle: 'Cập nhật cron job',
                    saveButtonText: 'Lưu',
                    cancelButtonText: 'Hủy',
                    showDeleteButton: false,
                    fields: [
                        { name: 'jobId', label: 'ID', type: 'hidden' },
                        { name: 'title', label: 'Tiêu đề', type: 'text', required: true, colSpan: 12 },
                        { name: 'url', label: 'URL', type: 'text', required: true, colSpan: 12, placeholder: 'https://...' },
                        {
                            name: 'requestMethod',
                            label: 'Phương thức',
                            type: 'select',
                            defaultValue: 1,
                            options: METHODS.map((text, value) => ({ value, text }))
                        },
                        {
                            name: 'schedulePreset',
                            label: 'Mẫu lịch',
                            type: 'select',
                            helpText: 'Chọn để điền nhanh các ô lịch bên dưới',
                            options: [
                                { value: '', text: '— Tuỳ chỉnh —' },
                                { value: 'daily7', text: 'Hằng ngày 07:00' },
                                { value: 'daily3', text: 'Hằng ngày 03:00' },
                                { value: 'hourly', text: 'Mỗi giờ (phút 0)' },
                                { value: 'every15', text: 'Mỗi 15 phút' },
                                { value: 'weekdays8', text: 'Thứ 2–6 lúc 08:00' },
                                { value: 'monthly1', text: 'Ngày 1 hằng tháng 07:00' }
                            ]
                        },
                        { name: 'hours', label: 'Giờ (0–23)', type: 'text', defaultValue: '7', helpText: scheduleHelp },
                        { name: 'minutes', label: 'Phút (0–59)', type: 'text', defaultValue: '0', helpText: scheduleHelp },
                        { name: 'mdays', label: 'Ngày trong tháng (1–31)', type: 'text', defaultValue: '*', helpText: scheduleHelp },
                        { name: 'months', label: 'Tháng (1–12)', type: 'text', defaultValue: '*', helpText: scheduleHelp },
                        { name: 'wdays', label: 'Thứ (0=CN … 6=T7)', type: 'text', defaultValue: '*', helpText: scheduleHelp },
                        { name: 'enabled', label: 'Hoạt động', type: 'checkbox', defaultValue: true },
                        {
                            name: 'attachCronSecret',
                            label: 'Gắn X-Cron-Secret',
                            type: 'checkbox',
                            colSpan: 12,
                            defaultValue: true,
                            helpText: 'Bắt buộc với endpoint /api/cron và /api/token của app; chỉ gắn được cho URL của app, server tự lấy secret từ cấu hình'
                        },
                        {
                            name: 'body',
                            label: 'Body (tuỳ chọn)',
                            type: 'textarea',
                            colSpan: 12,
                            placeholder: '{ "message": "..." }'
                        }
                    ]
                });

            this.gridBuilder.build();
            this.gridBuilder.getFormBuilder()?.onSave((submission) => this.saveJob(submission));
        }

        /** Form không có sự kiện đổi giá trị: bắt change của ô "Mẫu lịch" để điền các ô lịch. */
        private bindSchedulePreset(): void {
            $(document).on('change', `#${FORM_ID}_schedulePreset`, (event) => {
                const preset = SCHEDULE_PRESETS[String($(event.currentTarget).val())];
                if (!preset) return;
                (['hours', 'minutes', 'mdays', 'months', 'wdays'] as const).forEach((field) =>
                    $(`#${FORM_ID}_${field}`).val(preset[field] ?? '*')
                );
            });
        }

        /** Danh sách không có header/body: lấy chi tiết trước khi mở form sửa. */
        private async editJob(row: CronJobModel): Promise<void> {
            LoadingService.show();
            try {
                const response = await ApiService.get<CronJobModel>('/Admin/CronJob/GetById', { jobId: row.jobId });
                if (response.isOk()) {
                    // Form điền `value || ''` nên số 0 (GET) thành rỗng: truyền dạng chuỗi để chọn đúng
                    const job = response.data;
                    this.gridBuilder.getFormBuilder()?.showEdit({
                        ...job,
                        jobId: String(job.jobId),
                        requestMethod: String(job.requestMethod),
                        schedulePreset: ''
                    } as unknown as CronJobModel);
                } else {
                    ToastService.error(response.message || 'Không lấy được cron job');
                }
            } catch {
                ToastService.error('Lỗi hệ thống');
            } finally {
                LoadingService.hide();
            }
        }

        private async saveJob(submission: IFormSubmission<CronJobModel>): Promise<void> {
            const url = submission.mode === 'create' ? '/Admin/CronJob/Create' : '/Admin/CronJob/Update';
            // Ô hidden/select trả về chuỗi: ép về số cho khớp kiểu int ở server
            const data = {
                ...submission.data,
                jobId: submission.mode === 'create' ? 0 : Number(submission.data.jobId),
                requestMethod: Number(submission.data.requestMethod)
            };
            if (Number.isNaN(data.requestMethod) || data.jobId !== 0 && !data.jobId) {
                ToastService.error('Dữ liệu form không hợp lệ, hãy mở lại form');
                return;
            }

            LoadingService.show();
            try {
                const response = await ApiService.post(url, data);
                if (response.isOk()) {
                    ToastService.success(submission.mode === 'create' ? 'Thêm thành công' : 'Cập nhật thành công');
                    this.gridBuilder.reload();
                } else {
                    ToastService.error(response.message || 'Lỗi khi lưu');
                }
            } catch {
                ToastService.error('Lỗi hệ thống');
            } finally {
                LoadingService.hide();
            }
        }

        private async deleteJob(data: CronJobModel): Promise<void> {
            const warning = data.isSystem ? ' Đây là job hệ thống, xoá thì tính năng liên quan sẽ ngừng chạy.' : '';
            await MessageService.confirm(
                `Xóa cron job "${Utils.escapeHtml(data.title)}"?${warning}`,
                'Xác nhận',
                async () => {
                    LoadingService.show();
                    try {
                        const response = await ApiService.post('/Admin/CronJob/Delete', { jobId: data.jobId });
                        if (response.isOk()) {
                            ToastService.success('Xóa thành công');
                            this.gridBuilder.reload();
                        } else {
                            ToastService.error(response.message || 'Xóa thất bại');
                        }
                    } catch {
                        ToastService.error('Lỗi hệ thống');
                    } finally {
                        LoadingService.hide();
                    }
                }
            );
        }

        private async showHistory(data: CronJobModel): Promise<void> {
            LoadingService.show();
            let items: CronJobHistoryModel[] = [];
            try {
                const response = await ApiService.get<CronJobHistoryModel[]>('/Admin/CronJob/GetHistory', { jobId: data.jobId });
                if (!response.isOk()) {
                    ToastService.error(response.message || 'Không lấy được lịch sử chạy');
                    return;
                }
                items = response.data ?? [];
            } catch {
                ToastService.error('Lỗi hệ thống');
                return;
            } finally {
                LoadingService.hide();
            }

            const rows = items.length === 0
                ? '<tr><td colspan="4" class="text-center text-muted">Chưa có lần chạy nào</td></tr>'
                : items.map((item) => `
                    <tr>
                        <td>${item.date}</td>
                        <td class="${item.status === 1 ? 'text-success' : 'text-danger'}">${Utils.escapeHtml(item.statusText)}</td>
                        <td>${item.httpStatus || '—'}</td>
                        <td>${item.duration} ms</td>
                    </tr>`).join('');

            const popup = new PopupBuilder({
                title: `Lịch sử chạy — ${Utils.escapeHtml(data.title)}`,
                size: 'lg',
                scrollable: true,
                bodyHtml: `
                    <table class="table table-sm mb-0">
                        <thead><tr><th>Thời gian (giờ VN)</th><th>Kết quả</th><th>HTTP</th><th>Thời lượng</th></tr></thead>
                        <tbody>${rows}</tbody>
                    </table>`,
                onHidden: () => popup.destroy()
            });
            popup.show();
        }

        private async syncSystemJobs(): Promise<void> {
            await MessageService.confirm(
                'Tạo/cập nhật các job hệ thống (tóm tắt công việc 07:00, dọn refresh token 03:00) trên cron-job.org? '
                + 'Tiêu đề, lịch và header của các job này sẽ được đặt lại theo mặc định (job đang tạm dừng vẫn giữ tạm dừng).',
                'Đồng bộ',
                async () => {
                    LoadingService.show();
                    try {
                        const response = await ApiService.post<string>('/Admin/CronJob/SyncSystemJobs', {});
                        if (response.isOk()) {
                            ToastService.success(response.data || 'Đồng bộ thành công');
                            this.gridBuilder.reload();
                        } else {
                            ToastService.error(response.message || 'Đồng bộ thất bại');
                        }
                    } catch {
                        ToastService.error('Lỗi hệ thống');
                    } finally {
                        LoadingService.hide();
                    }
                }
            );
        }
    }
}
