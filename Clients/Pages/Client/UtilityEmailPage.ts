namespace App {
    interface EmailReportRequest {
        currentBusinessDay: Date;
        nextBusinessDay: Date;
        todayWorkResults: string;
        plannedWorkTomorrow: string;
        suggestions: string;
    }

    interface EmailReportResponse {
        emailTo: string[];
        emailCc: string[];
        subject: string;
        body: string;
    }

    export class UtilityEmailPage extends BasePage {
        protected initialize(): void {
            this.initDatePickers();
        }

        protected bindEvents(): void {
            this.root.find(".btn-gen-email").on("click", () => this.genEmail());
            this.root.find(".copy-btn").on("click", (e) => {
                const target = $(e.currentTarget).data("target");
                this.copyField(target);
            });
        }

        private initDatePickers(): void {
            const currentDate = new Date();
            const nextDate = new Date(currentDate);

            if (currentDate.getDay() === 6) {
                nextDate.setDate(currentDate.getDate() + 2);
            } else {
                nextDate.setDate(currentDate.getDate() + 1);
            }

            const $current = this.root.find('#currentBusinessDay');
            const $next = this.root.find('#nextBusinessDay');

            ($current as any).datepicker({
                format: 'dd/mm/yyyy',
                autoclose: true,
                todayHighlight: true,
                clearBtn: true
            }).datepicker('setDate', currentDate);

            ($next as any).datepicker({
                format: 'dd/mm/yyyy',
                autoclose: true,
                todayHighlight: true,
                clearBtn: true,
                startDate: currentDate
            }).datepicker('setDate', nextDate);
        }

        private async genEmail(): Promise<void> {
            const currentBusinessDay = (this.root.find('#currentBusinessDay') as any).datepicker('getUTCDate');
            const nextBusinessDay = (this.root.find('#nextBusinessDay') as any).datepicker('getUTCDate');
            const todayWorkResults = (this.root.find('#todayWorkResults').val() as string).trim();
            const plannedWorkTomorrow = (this.root.find('#plannedWorkTomorrow').val() as string).trim();
            const suggestions = (this.root.find('#suggestions').val() as string).trim();

            if (!todayWorkResults || !plannedWorkTomorrow) {
                ToastService.warning("Vui lòng nhập đầy đủ thông tin bắt buộc");
                return;
            }

            const request: EmailReportRequest = {
                currentBusinessDay,
                nextBusinessDay,
                todayWorkResults,
                plannedWorkTomorrow,
                suggestions
            };

            LoadingService.show();
            try {
                const res = await ApiService.post<EmailReportResponse>('/Utility/GenerateEmailDailyReport', request);
                LoadingService.hide();

                if (res.isOk()) {
                    this.root.find('#emailTo').val(res.data.emailTo?.join(', ') || '');
                    this.root.find('#EmailCc').val(res.data.emailCc?.join(', ') || '');
                    this.root.find('#subject').val(res.data.subject);
                    this.root.find('#body').val(res.data.body);
                } else {
                    ToastService.error(res.message || 'Lỗi khi tạo email');
                }
            } catch (error) {
                LoadingService.hide();
                ToastService.error('Lỗi hệ thống');
            }
        }

        private copyField(selector: string): void {
            if (!selector) return;
            const text = (this.root.find(selector).val() as string)?.trim();

            if (!text) {
                ToastService.warning('Không có nội dung để copy!');
                return;
            }

            navigator.clipboard.writeText(text)
                .then(() => ToastService.success('Copy thành công!'))
                .catch(() => ToastService.error('Copy thất bại!'));
        }
    }
}
