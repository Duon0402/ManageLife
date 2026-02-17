namespace App {
    /**
     * Builder for creating automatic CRUD forms/modals
     */
    export class GridFormBuilder<T = any> {
        private config: App.IGridModal<T>;
        private modalId: string;
        private $modal: JQuery | null = null;
        private currentMode: 'create' | 'edit' = 'create';
        private currentData: T | null = null;
        private onSaveCallback: ((data: App.IFormSubmission<T>) => void | Promise<void>) | null = null;
        private onDeleteCallback: ((data: T) => void | Promise<void>) | null = null;

        constructor(config: App.IGridModal<T>) {
            this.config = {
                size: 'md',
                createTitle: 'Create New',
                editTitle: 'Edit',
                saveButtonText: 'Save',
                cancelButtonText: 'Cancel',
                showDeleteButton: false,
                columns: 1,
                ...config
            };
            this.modalId = this.config.id || 'gridModal_' + Math.random().toString(36).substr(2, 9);
        }

        /**
         * Set the save callback
         */
        onSave(callback: (data: App.IFormSubmission<T>) => void | Promise<void>): this {
            this.onSaveCallback = callback;
            return this;
        }

        /**
         * Set the delete callback
         */
        onDelete(callback: (data: T) => void | Promise<void>): this {
            this.onDeleteCallback = callback;
            return this;
        }

        /**
         * Build and inject the modal into the page
         */
        build(): this {
            const modalHtml = this.generateModalHtml();

            // Remove existing modal if any
            $('#' + this.modalId).remove();

            // Append to body
            $('body').append(modalHtml);

            this.$modal = $('#' + this.modalId);

            // Bind events
            this.bindEvents();

            return this;
        }

        /**
         * Generate modal HTML
         */
        private generateModalHtml(): string {
            const sizeClass = this.config.size !== 'md' ? ` modal-${this.config.size}` : '';

            return `
                <div class="modal fade" id="${this.modalId}" tabindex="-1" aria-hidden="true">
                    <div class="modal-dialog${sizeClass}">
                        <div class="modal-content">
                            <div class="modal-header">
                                <h5 class="modal-title">${this.config.createTitle}</h5>
                                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                            </div>
                            <div class="modal-body">
                                <form id="${this.modalId}_form">
                                    ${this.generateFormHtml()}
                                </form>
                            </div>
                            <div class="modal-footer">
                                ${this.generateFooterHtml()}
                            </div>
                        </div>
                    </div>
                </div>
            `;
        }

        /**
         * Generate form fields HTML
         */
        private generateFormHtml(): string {
            const columns = this.config.columns || 1;
            const rowClass = columns > 1 ? 'row' : '';

            let html = `<div class="${rowClass}">`;

            this.config.fields.forEach(field => {
                const colClass = columns > 1
                    ? `col-md-${field.colSpan || (12 / columns)}`
                    : '';

                html += `<div class="mb-3 ${colClass}">`;
                html += this.generateFieldHtml(field);
                html += '</div>';
            });

            html += '</div>';
            return html;
        }

        /**
         * Generate single field HTML
         */
        private generateFieldHtml(field: App.IFormField<T>): string {
            const fieldId = `${this.modalId}_${String(field.name)}`;
            const required = field.required ? 'required' : '';
            const readonly = field.readonly ? 'readonly' : '';
            const disabled = field.disabled ? 'disabled' : '';

            let html = '';

            // Label
            if (field.type !== 'hidden' && field.type !== 'checkbox') {
                html += `<label for="${fieldId}" class="form-label">
                    ${field.label}
                    ${field.required ? '<span class="text-danger">*</span>' : ''}
                </label>`;
            }

            // Field input
            switch (field.type) {
                case 'textarea':
                    html += `<textarea 
                        id="${fieldId}" 
                        name="${String(field.name)}"
                        class="form-control ${field.className || ''}"
                        placeholder="${field.placeholder || ''}"
                        ${required} ${readonly} ${disabled}
                    ></textarea>`;
                    break;

                case 'select':
                    html += `<select 
                        id="${fieldId}" 
                        name="${String(field.name)}"
                        class="form-select ${field.className || ''}"
                        ${required} ${disabled}
                    >`;
                    if (!field.required) {
                        html += '<option value="">-- Select --</option>';
                    }
                    (field.options || []).forEach(opt => {
                        html += `<option value="${opt.value}" ${opt.selected ? 'selected' : ''} ${opt.disabled ? 'disabled' : ''}>
                            ${opt.text}
                        </option>`;
                    });
                    html += '</select>';
                    break;

                case 'checkbox':
                    html += `<div class="form-check">
                        <input 
                            type="checkbox" 
                            id="${fieldId}" 
                            name="${String(field.name)}"
                            class="form-check-input ${field.className || ''}"
                            ${disabled}
                        >
                        <label class="form-check-label" for="${fieldId}">
                            ${field.label}
                        </label>
                    </div>`;
                    break;

                case 'hidden':
                    html += `<input 
                        type="hidden" 
                        id="${fieldId}" 
                        name="${String(field.name)}"
                    >`;
                    break;

                default:
                    html += `<input 
                        type="${field.type || 'text'}" 
                        id="${fieldId}" 
                        name="${String(field.name)}"
                        class="form-control ${field.className || ''}"
                        placeholder="${field.placeholder || ''}"
                        ${required} ${readonly} ${disabled}
                    >`;
            }

            // Help text
            if (field.helpText) {
                html += `<div class="form-text">${field.helpText}</div>`;
            }

            return html;
        }

        /**
         * Generate footer buttons HTML
         */
        private generateFooterHtml(): string {
            let html = '';

            // Delete button (only in edit mode, controlled by JS)
            if (this.config.showDeleteButton) {
                html += `<button type="button" class="btn btn-danger me-auto" id="${this.modalId}_deleteBtn" style="display:none;">
                    <i class="fa-solid fa-trash"></i> Delete
                </button>`;
            }

            // Custom footer buttons
            (this.config.footerButtons || []).forEach((btn, idx) => {
                html += `<button type="button" class="btn ${btn.className || 'btn-secondary'}" 
                    data-custom-btn="${idx}">
                    ${btn.text}
                </button>`;
            });

            // Cancel button
            html += `<button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                ${this.config.cancelButtonText}
            </button>`;

            // Save button
            html += `<button type="button" class="btn btn-primary" id="${this.modalId}_saveBtn">
                ${this.config.saveButtonText}
            </button>`;

            return html;
        }

        /**
         * Bind modal events
         */
        private bindEvents(): void {
            if (!this.$modal) return;

            // Save button
            this.$modal.find(`#${this.modalId}_saveBtn`).on('click', () => this.handleSave());

            // Delete button
            this.$modal.find(`#${this.modalId}_deleteBtn`).on('click', () => this.handleDelete());

            // Custom buttons
            this.$modal.find('[data-custom-btn]').on('click', (e) => {
                const idx = parseInt($(e.currentTarget).data('custom-btn'));
                const btn = this.config.footerButtons?.[idx];
                if (btn?.onClick) {
                    const formData = this.getFormData();
                    Promise.resolve(btn.onClick(formData)).then(() => {
                        if (btn.closeAfter) {
                            this.hide();
                        }
                    });
                }
            });

            // Reset form on hide
            this.$modal.on('hidden.bs.modal', () => {
                this.resetForm();
            });
        }

        /**
         * Show modal in create mode
         */
        showCreate(): void {
            this.currentMode = 'create';
            this.currentData = null;
            this.updateModalTitle();
            this.resetForm();
            this.show();
        }

        /**
         * Show modal in edit mode
         */
        showEdit(data: T): void {
            this.currentMode = 'edit';
            this.currentData = data;
            this.updateModalTitle();
            this.populateForm(data);
            this.show();
        }

        /**
         * Show the modal
         */
        private show(): void {
            if (!this.$modal) return;

            // Show/hide delete button based on mode
            if (this.config.showDeleteButton) {
                const $deleteBtn = this.$modal.find(`#${this.modalId}_deleteBtn`);
                if (this.currentMode === 'edit') {
                    $deleteBtn.show();
                } else {
                    $deleteBtn.hide();
                }
            }

            (this.$modal as any).modal('show');
        }

        /**
         * Hide the modal
         */
        hide(): void {
            if (!this.$modal) return;
            (this.$modal as any).modal('hide');
        }

        /**
         * Update modal title based on mode
         */
        private updateModalTitle(): void {
            if (!this.$modal) return;
            const title = this.currentMode === 'create'
                ? this.config.createTitle
                : this.config.editTitle;
            this.$modal.find('.modal-title').text(title || '');
        }

        /**
         * Populate form with data
         */
        private populateForm(data: T): void {
            if (!this.$modal) return;

            this.config.fields.forEach(field => {
                const fieldId = `${this.modalId}_${String(field.name)}`;
                const $field = this.$modal!.find(`#${fieldId}`);
                const value = (data as any)[field.name];

                if (field.type === 'checkbox') {
                    $field.prop('checked', !!value);
                } else {
                    $field.val(value || '');
                }
            });
        }

        /**
         * Reset form
         */
        private resetForm(): void {
            if (!this.$modal) return;

            const $form = this.$modal.find('form');
            $form[0].reset();

            // Set default values
            this.config.fields.forEach(field => {
                if (field.defaultValue !== undefined) {
                    const fieldId = `${this.modalId}_${String(field.name)}`;
                    const $field = this.$modal!.find(`#${fieldId}`);

                    if (field.type === 'checkbox') {
                        $field.prop('checked', !!field.defaultValue);
                    } else {
                        $field.val(field.defaultValue);
                    }
                }
            });
        }

        /**
         * Get form data
         */
        private getFormData(): Partial<T> {
            const formData: any = {};

            this.config.fields.forEach(field => {
                const fieldId = `${this.modalId}_${String(field.name)}`;
                const $field = this.$modal!.find(`#${fieldId}`);

                if (field.type === 'checkbox') {
                    formData[field.name] = $field.prop('checked');
                } else if (field.type === 'number') {
                    const val = $field.val();
                    formData[field.name] = val ? Number(val) : null;
                } else {
                    formData[field.name] = $field.val();
                }
            });

            return formData;
        }

        /**
         * Validate form
         */
        private validateForm(): boolean {
            // Simple validation for now
            let isValid = true;

            this.config.fields.forEach(field => {
                if (field.required) {
                    const fieldId = `${this.modalId}_${String(field.name)}`;
                    const $field = this.$modal!.find(`#${fieldId}`);
                    const value = field.type === 'checkbox'
                        ? $field.prop('checked')
                        : $field.val();

                    if (!value) {
                        $field.addClass('is-invalid');
                        isValid = false;
                    } else {
                        $field.removeClass('is-invalid');
                    }
                }
            });

            return isValid;
        }

        /**
         * Handle save
         */
        private async handleSave(): Promise<void> {
            if (!this.validateForm()) {
                App.ToastService.warning('Please fill in all required fields');
                return;
            }

            const formData = this.getFormData();

            // Call beforeSave if provided
            if (this.config.beforeSave) {
                const canProceed = await Promise.resolve(
                    this.config.beforeSave(formData, this.currentMode)
                );
                if (!canProceed) {
                    return;
                }
            }

            // Call save callback
            if (this.onSaveCallback) {
                const submission: App.IFormSubmission<T> = {
                    data: formData,
                    mode: this.currentMode,
                    originalData: this.currentData || undefined
                };

                await Promise.resolve(this.onSaveCallback(submission));
            }

            // Call afterSave if provided
            if (this.config.afterSave) {
                await Promise.resolve(this.config.afterSave(formData, this.currentMode));
            }

            this.hide();
        }

        /**
         * Handle delete
         */
        private async handleDelete(): Promise<void> {
            if (!this.currentData || !this.onDeleteCallback) {
                return;
            }

            // Confirm deletion
            if (!confirm('Are you sure you want to delete this item?')) {
                return;
            }

            await Promise.resolve(this.onDeleteCallback(this.currentData));
            this.hide();
        }

        /**
         * Get modal jQuery element
         */
        getModal(): JQuery | null {
            return this.$modal;
        }

        /**
         * Get modal ID
         */
        getModalId(): string {
            return this.modalId;
        }
    }
}
