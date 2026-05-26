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
        private currentPicker!: DatePickerBuilder;
        private nextPicker!: DatePickerBuilder;

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

            this.currentPicker = new DatePickerBuilder('#currentBusinessDayContainer')
                .withId('currentBusinessDay')
                .setDefaultDate(currentDate)
                .enableTyping()
                .build();

            this.nextPicker = new DatePickerBuilder('#nextBusinessDayContainer')
                .withId('nextBusinessDay')
                .setDefaultDate(nextDate)
                .setMinDate(currentDate)
                .enableTyping()
                .build();
        }

        private async genEmail(): Promise<void> {
            const currentBusinessDay = this.currentPicker.getValue();
            const nextBusinessDay = this.nextPicker.getValue();
            const todayWorkResults = (this.root.find('#todayWorkResults').val() as string).trim();
            const plannedWorkTomorrow = (this.root.find('#plannedWorkTomorrow').val() as string).trim();
            const suggestions = (this.root.find('#suggestions').val() as string).trim();

            if (!currentBusinessDay || !nextBusinessDay || !todayWorkResults || !plannedWorkTomorrow) {
                ToastService.warning("Vui lòng nhập đầy đủ thông tin bắt buộc");
                return;
            }

            const request: EmailReportRequest = {
                currentBusinessDay: currentBusinessDay!,
                nextBusinessDay: nextBusinessDay!,
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
