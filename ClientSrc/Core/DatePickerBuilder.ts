namespace App {
    export interface IDatePickerOptions {
        inputId?: string;
        format?: string;
        defaultDate?: Date;
        minDate?: Date;
        maxDate?: Date;
        allowInput?: boolean;
        disableMobile?: boolean;
    }

    export class DatePickerBuilder {
        private selector: string;
        private options: IDatePickerOptions;
        private $container: JQuery;
        private pickerInstance: any;
        private resolvedInputId: string;

        private changeCallback: ((date: Date | null) => void) | null = null;

        constructor(selector: string, options?: IDatePickerOptions) {
            this.selector = selector;
            this.options = {
                format: 'd/m/Y',
                disableMobile: true,
                ...options
            };
            this.resolvedInputId = options?.inputId || `dp-${Math.random().toString(36).substr(2, 9)}`;
        }

        public withId(inputId: string): this {
            this.resolvedInputId = inputId;
            return this;
        }

        public setFormat(format: string): this {
            this.options.format = format;
            return this;
        }

        public setDefaultDate(date: Date): this {
            this.options.defaultDate = date;
            return this;
        }

        public setMinDate(date: Date): this {
            this.options.minDate = date;
            return this;
        }

        public setMaxDate(date: Date): this {
            this.options.maxDate = date;
            return this;
        }

        public enableTyping(): this {
            this.options.allowInput = true;
            return this;
        }

        public onChange(callback: (date: Date | null) => void): this {
            this.changeCallback = callback;
            return this;
        }

        public build(): this {
            this.$container = $(this.selector);
            if (this.$container.length === 0) {
                console.error(`DatePickerBuilder: Selector '${this.selector}' not found.`);
                return this;
            }
            this.renderHtml();
            this.initPicker();
            return this;
        }

        private renderHtml(): void {
            const readonlyAttr = this.options.allowInput ? '' : 'readonly';
            const html = `
                <div class="input-group date-picker-wrap">
                    <input type="text" class="form-control" id="${this.resolvedInputId}" data-input ${readonlyAttr}>
                    <span class="input-group-text" data-toggle style="cursor:pointer">
                        <i class="fa-regular fa-calendar"></i>
                    </span>
                </div>
                <div class="invalid-feedback date-picker-error"></div>`;
            this.$container.html(html);
        }

        private initPicker(): void {
            const fp = (window as any).flatpickr;
            const wrapEl = this.$container.find('.date-picker-wrap')[0];

            const fpOptions: any = {
                wrap: true,
                dateFormat: this.options.format,
                allowInput: this.options.allowInput ?? false,
                disableMobile: this.options.disableMobile,
                onChange: (selectedDates: Date[]) => {
                    if (this.options.allowInput) {
                        this.handleValidation(selectedDates);
                    }
                    if (this.changeCallback) {
                        this.changeCallback(selectedDates[0] ?? null);
                    }
                },
            };

            if (this.options.defaultDate !== undefined) fpOptions.defaultDate = this.options.defaultDate;
            if (this.options.minDate !== undefined) fpOptions.minDate = this.options.minDate;
            if (this.options.maxDate !== undefined) fpOptions.maxDate = this.options.maxDate;

            this.pickerInstance = fp(wrapEl, fpOptions);

            if (this.options.allowInput) {
                this.$container.find(`#${this.resolvedInputId}`).on('blur', () => this.validateOnBlur());
            }
        }

        private handleValidation(selectedDates: Date[]): void {
            const $input = this.$container.find(`#${this.resolvedInputId}`);
            const rawValue = ($input.val() as string || '').trim();
            if (rawValue && selectedDates.length === 0) {
                this.showError(`Định dạng không hợp lệ. Vui lòng nhập: ${this.getFormatHint()}`);
            } else {
                this.clearError();
            }
        }

        private validateOnBlur(): void {
            const $input = this.$container.find(`#${this.resolvedInputId}`);
            const rawValue = ($input.val() as string || '').trim();
            const hasDate = (this.pickerInstance?.selectedDates?.length ?? 0) > 0;
            if (rawValue && !hasDate) {
                this.showError(`Định dạng không hợp lệ. Vui lòng nhập: ${this.getFormatHint()}`);
            } else if (!rawValue) {
                this.clearError();
            }
        }

        private getFormatHint(): string {
            return (this.options.format || 'd/m/Y')
                .replace('d', 'dd').replace('m', 'mm').replace('Y', 'yyyy');
        }

        private showError(msg: string): void {
            this.$container.find(`#${this.resolvedInputId}`).addClass('is-invalid');
            this.$container.find('.date-picker-error').text(msg).css('display', 'block');
        }

        private clearError(): void {
            this.$container.find(`#${this.resolvedInputId}`).removeClass('is-invalid');
            this.$container.find('.date-picker-error').css('display', 'none');
        }

        public getValue(): Date | null {
            return this.pickerInstance?.selectedDates[0] ?? null;
        }

        public isValid(): boolean {
            if (!this.options.allowInput) return this.getValue() !== null;
            const $input = this.$container.find(`#${this.resolvedInputId}`);
            const rawValue = ($input.val() as string || '').trim();
            return rawValue === '' || (this.pickerInstance?.selectedDates?.length ?? 0) > 0;
        }

        public setValue(date: Date): this {
            this.pickerInstance?.setDate(date, true);
            return this;
        }

        public updateMinDate(date: Date): this {
            this.pickerInstance?.set('minDate', date);
            return this;
        }

        public updateMaxDate(date: Date): this {
            this.pickerInstance?.set('maxDate', date);
            return this;
        }

        public clear(): this {
            this.pickerInstance?.clear();
            this.clearError();
            return this;
        }

        public destroy(): void {
            this.pickerInstance?.destroy();
            this.$container.empty();
        }

        public getInputElement(): JQuery {
            return this.$container.find(`#${this.resolvedInputId}`);
        }
    }
}
