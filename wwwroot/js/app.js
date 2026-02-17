var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var App;
(function (App) {
    class ApiService {
        static get DEFAULT_OPTIONS() {
            return {
                contentType: App.Constants.Defaults.CONTENT_TYPE,
                dataType: App.Constants.Defaults.DATA_TYPE,
                processData: true,
                showLoading: true,
                showToast: true
            };
        }
        static get(url, params = {}, options = {}) {
            const requestUrl = params ? `${url}?${$.param(params)}` : url;
            return this.request(requestUrl, App.Constants.HttpMethod.GET, null, options);
        }
        static post(url, data, options = {}) {
            return this.request(url, App.Constants.HttpMethod.POST, data, options);
        }
        static put(url, data, options = {}) {
            return this.request(url, App.Constants.HttpMethod.PUT, data, options);
        }
        static delete(url, options = {}) {
            return this.request(url, App.Constants.HttpMethod.DELETE, null, options);
        }
        static upload(url, formData, options = {}) {
            if (!(formData instanceof FormData)) {
                throw new Error("upload() expects FormData");
            }
            return this.request(url, App.Constants.HttpMethod.POST, formData, Object.assign(Object.assign({}, options), { contentType: false, processData: false }));
        }
        static request(url, method, data, options) {
            const settings = Object.assign(Object.assign({}, this.DEFAULT_OPTIONS), options);
            if (data instanceof FormData) {
                settings.contentType = false;
                settings.processData = false;
            }
            if (settings.showLoading) {
                App.LoadingService.show();
            }
            return new Promise((resolve, reject) => {
                $.ajax({
                    url: url,
                    method: method,
                    data: (settings.contentType === false || !settings.contentType) ? data : (data ? JSON.stringify(data) : null),
                    contentType: settings.contentType,
                    processData: settings.processData,
                    dataType: settings.dataType,
                    xhrFields: { withCredentials: true },
                    beforeSend: settings.beforeSend,
                    xhr: function () {
                        const xhr = new window.XMLHttpRequest();
                        if (xhr.upload && settings.onProgress) {
                            xhr.upload.addEventListener('progress', (e) => {
                                if (e.lengthComputable && settings.onProgress) {
                                    settings.onProgress(Math.round(e.loaded / e.total * 100), e);
                                }
                            });
                        }
                        return xhr;
                    },
                    success: (res, textStatus, jqXHR) => {
                        const apiResponse = res;
                        apiResponse.isOk = () => apiResponse.code === App.Constants.ApiCode.SUCCESS;
                        apiResponse.isException = () => apiResponse.code === App.Constants.ApiCode.EXCEPTION;
                        apiResponse.isError = () => !apiResponse.isOk() && !apiResponse.isException();
                        if (settings.success) {
                            if (Array.isArray(settings.success)) {
                                settings.success.forEach(fn => fn(res, textStatus, jqXHR));
                            }
                            else {
                                settings.success(res, textStatus, jqXHR);
                            }
                        }
                        else if (settings.showToast && apiResponse.message) {
                            if (!apiResponse.isOk()) {
                                App.ToastService.error(apiResponse.message || App.Constants.Messages.DEFAULT_ERROR, App.Constants.Messages.NOTIFICATION_TITLE);
                            }
                            else {
                                App.ToastService.success(apiResponse.message, App.Constants.Messages.NOTIFICATION_TITLE);
                            }
                        }
                        resolve(apiResponse);
                    },
                    error: (jqXHR) => {
                        var _a, _b;
                        if (jqXHR.status === App.Constants.HttpStatus.UNAUTHORIZED) {
                            window.location.href = App.Constants.Urls.LOGIN + "?returnUrl=" +
                                encodeURIComponent(window.location.pathname + window.location.search);
                            return;
                        }
                        if (settings.error) {
                            if (Array.isArray(settings.error)) {
                                settings.error.forEach((fn) => fn(jqXHR, 'error', jqXHR.statusText));
                            }
                            else {
                                settings.error(jqXHR, 'error', jqXHR.statusText);
                            }
                        }
                        if (((_a = jqXHR.responseJSON) === null || _a === void 0 ? void 0 : _a.code) === App.Constants.ApiCode.FORBIDDEN) {
                            App.ToastService.error(App.Constants.Messages.FORBIDDEN, App.Constants.Messages.NOTIFICATION_TITLE);
                        }
                        else {
                            const msg = ((_b = jqXHR.responseJSON) === null || _b === void 0 ? void 0 : _b.message) || App.Constants.Messages.DEFAULT_ERROR;
                            App.ToastService.error(msg, App.Constants.Messages.NOTIFICATION_TITLE);
                        }
                        reject(jqXHR);
                    },
                    complete: (jqXHR, textStatus) => {
                        if (settings.showLoading) {
                            App.LoadingService.hide();
                        }
                        if (settings.complete) {
                            if (Array.isArray(settings.complete)) {
                                settings.complete.forEach(fn => fn(jqXHR, textStatus));
                            }
                            else {
                                settings.complete(jqXHR, textStatus);
                            }
                        }
                    }
                });
            });
        }
    }
    App.ApiService = ApiService;
})(App || (App = {}));
var App;
(function (App) {
    class BasePage {
        constructor(rootSelector, model) {
            if (!rootSelector) {
                throw new Error("rootSelector is required");
            }
            this.root = $(rootSelector);
            this.model = model || {};
            this.initialize();
            this.bindEvents();
        }
        initialize() {
        }
        bindEvents() {
        }
    }
    App.BasePage = BasePage;
})(App || (App = {}));
var App;
(function (App) {
    var Constants;
    (function (Constants) {
        Constants.HttpMethod = {
            GET: 'GET',
            POST: 'POST',
            PUT: 'PUT',
            DELETE: 'DELETE'
        };
        Constants.ApiCode = {
            SUCCESS: '00',
            EXCEPTION: '99',
            FORBIDDEN: '403'
        };
        Constants.HttpStatus = {
            UNAUTHORIZED: 401
        };
        Constants.Messages = {
            DEFAULT_ERROR: 'Đã có lỗi xảy ra',
            FORBIDDEN: 'Bạn không có quyền thực hiện chức năng này',
            NOTIFICATION_TITLE: 'Thông báo'
        };
        Constants.Urls = {
            LOGIN: '/Auth/Login'
        };
        Constants.Defaults = {
            CONTENT_TYPE: 'application/json',
            DATA_TYPE: 'json'
        };
    })(Constants = App.Constants || (App.Constants = {}));
})(App || (App = {}));
var App;
(function (App) {
    /**
     * Fluent builder for creating DataTables grids with type safety
     * Usage similar to DevExtreme grid configuration
     */
    class GridBuilder {
        constructor(selector) {
            this.columns = [];
            this.toolbarButtons = [];
            this.actionButtons = [];
            this.options = {
                paging: false,
                info: false,
                ordering: false,
                searching: true,
                autoWidth: true,
                destroy: true,
                scrollCollapse: true
            };
            this.dataSource = null;
            this.callbacks = {};
            this.actionColumnConfig = {
                title: '',
                width: '120px',
                className: 'text-center',
                position: -1 // Last column by default
            };
            this.layout = null;
            this.tableInstance = null;
            this.formBuilder = null;
            this.formConfig = null;
            this.selector = selector;
        }
        /**
         * Add a column to the grid
         */
        addColumn(config) {
            if (config instanceof App.GridColumnBuilder) {
                this.columns.push(config.build());
            }
            else {
                // Ensure data property is set if field is provided
                if (config.field && !config.data) {
                    config.data = config.field;
                }
                this.columns.push(config);
            }
            return this;
        }
        /**
         * Add multiple columns at once
         */
        addColumns(configs) {
            configs.forEach(config => this.addColumn(config));
            return this;
        }
        /**
         * Add a toolbar button
         */
        addToolbarButton(config) {
            this.toolbarButtons.push(config);
            return this;
        }
        /**
         * Add a row action button (edit, delete, etc.)
         */
        addActionButton(config) {
            this.actionButtons.push(config);
            return this;
        }
        /**
         * Configure action column appearance
         */
        configureActionColumn(config) {
            this.actionColumnConfig = Object.assign(Object.assign({}, this.actionColumnConfig), config);
            return this;
        }
        /**
         * Set grid options
         */
        setOptions(options) {
            this.options = Object.assign(Object.assign({}, this.options), options);
            return this;
        }
        /**
         * Set data source
         */
        setDataSource(dataSource) {
            if (Array.isArray(dataSource)) {
                this.dataSource = { data: dataSource };
            }
            else {
                this.dataSource = dataSource;
            }
            return this;
        }
        /**
         * Set layout configuration
         */
        setLayout(layout) {
            this.layout = layout;
            return this;
        }
        /**
         * Set create callback
         */
        onCreate(callback) {
            this.callbacks.onCreate = callback;
            return this;
        }
        /**
         * Set edit callback
         */
        onEdit(callback) {
            this.callbacks.onEdit = callback;
            return this;
        }
        /**
         * Set delete callback
         */
        onDelete(callback) {
            this.callbacks.onDelete = callback;
            return this;
        }
        /**
         * Set row click callback
         */
        onRowClick(callback) {
            this.callbacks.onRowClick = callback;
            return this;
        }
        /**
         * Configure automatic modal form for create/edit operations
         */
        setForm(config) {
            this.formConfig = config;
            return this;
        }
        /**
         * Build and initialize the form modal
         */
        buildFormModal() {
            if (!this.formConfig) {
                return;
            }
            this.formBuilder = new App.GridFormBuilder(this.formConfig);
            this.formBuilder.build();
            // Set up save callback to handle form submission
            this.formBuilder.onSave((submission) => {
                return this.handleFormSave(submission);
            });
            // Set up delete callback if configured
            if (this.formConfig.showDeleteButton) {
                this.formBuilder.onDelete((data) => {
                    if (this.callbacks.onDelete) {
                        return Promise.resolve(this.callbacks.onDelete(data));
                    }
                });
            }
            // Override onCreate callback to use form modal
            if (!this.callbacks.onCreate) {
                this.callbacks.onCreate = () => {
                    var _a;
                    (_a = this.formBuilder) === null || _a === void 0 ? void 0 : _a.showCreate();
                };
            }
            // Override onEdit callback to use form modal
            if (!this.callbacks.onEdit) {
                this.callbacks.onEdit = (data) => {
                    var _a;
                    (_a = this.formBuilder) === null || _a === void 0 ? void 0 : _a.showEdit(data);
                };
            }
        }
        /**
         * Handle form save submission
         */
        handleFormSave(submission) {
            return __awaiter(this, void 0, void 0, function* () {
                // This method should be overridden or handled by callbacks
                // You can emit events or call API services here
                console.log('Form submitted:', submission);
            });
        }
        /**
         * Build the action column if action buttons are defined
         */
        buildActionColumn() {
            if (this.actionButtons.length === 0) {
                return null;
            }
            return {
                title: this.actionColumnConfig.title,
                data: null,
                orderable: false,
                searchable: false,
                className: this.actionColumnConfig.className,
                width: this.actionColumnConfig.width,
                render: (data, type, row) => {
                    return this.actionButtons
                        .filter(btn => {
                        if (typeof btn.visible === 'function') {
                            return btn.visible(row);
                        }
                        return btn.visible !== false;
                    })
                        .map(btn => {
                        if (btn.render) {
                            return btn.render(row);
                        }
                        const className = btn.className || 'btn-outline-secondary';
                        return `
                                <button class="btn btn-sm ${className} grid-action-btn"
                                        data-action="${btn.icon}"
                                        title="${btn.title || ''}">
                                    <i class="fa-solid ${btn.icon}"></i>
                                </button>
                            `;
                    })
                        .join(' ');
                }
            };
        }
        /**
         * Build DataTables buttons configuration
         */
        buildButtons() {
            return this.toolbarButtons.map(btn => {
                const text = btn.icon
                    ? `<i class="fa-solid ${btn.icon}"></i>${btn.text ? ' ' + btn.text : ''}`
                    : btn.text || '';
                return {
                    text: text,
                    className: btn.className || 'btn btn-sm btn-outline-secondary',
                    titleAttr: btn.title || '',
                    action: () => {
                        if (btn.onClick) {
                            btn.onClick();
                        }
                    }
                };
            });
        }
        /**
         * Build layout configuration
         */
        buildLayout() {
            if (this.layout) {
                return this.layout;
            }
            // Default layout with buttons and search if toolbar buttons exist
            if (this.toolbarButtons.length > 0) {
                return {
                    topEnd: ['buttons', 'search']
                };
            }
            return {
                topEnd: ['search']
            };
        }
        /**
         * Bind action button events
         */
        bindActionEvents($table) {
            if (this.actionButtons.length === 0) {
                return;
            }
            // Delegate click events for action buttons
            $table.off('click', '.grid-action-btn').on('click', '.grid-action-btn', (e) => {
                var _a;
                e.preventDefault();
                e.stopPropagation();
                const $btn = $(e.currentTarget);
                const actionIcon = $btn.data('action');
                const $row = $btn.closest('tr');
                const rowData = (_a = this.tableInstance) === null || _a === void 0 ? void 0 : _a.row($row).data();
                if (!rowData) {
                    return;
                }
                // Find matching action button
                const actionBtn = this.actionButtons.find(btn => btn.icon === actionIcon);
                if (actionBtn && actionBtn.onClick) {
                    actionBtn.onClick(rowData, e);
                }
            });
        }
        /**
         * Bind row click events
         */
        bindRowEvents($table) {
            if (!this.callbacks.onRowClick) {
                return;
            }
            $table.off('click', 'tbody tr').on('click', 'tbody tr', (e) => {
                var _a;
                // Don't trigger if clicking on action buttons
                if ($(e.target).closest('.grid-action-btn').length > 0) {
                    return;
                }
                const rowData = (_a = this.tableInstance) === null || _a === void 0 ? void 0 : _a.row(e.currentTarget).data();
                if (rowData && this.callbacks.onRowClick) {
                    this.callbacks.onRowClick(rowData, e);
                }
            });
        }
        /**
         * Build and initialize the DataTable
         */
        build() {
            var _a, _b;
            const $table = $(this.selector);
            if ($table.length === 0) {
                throw new Error(`Grid selector "${this.selector}" not found`);
            }
            // Add action column if needed
            const actionColumn = this.buildActionColumn();
            const allColumns = [...this.columns];
            if (actionColumn) {
                if (this.actionColumnConfig.position === -1 || this.actionColumnConfig.position >= allColumns.length) {
                    allColumns.push(actionColumn);
                }
                else {
                    allColumns.splice(this.actionColumnConfig.position, 0, actionColumn);
                }
            }
            // Build DataTables configuration
            const config = Object.assign(Object.assign({}, this.options), { columns: allColumns, data: ((_a = this.dataSource) === null || _a === void 0 ? void 0 : _a.data) || [] });
            // Add buttons if configured
            if (this.toolbarButtons.length > 0) {
                config.buttons = this.buildButtons();
                config.layout = this.buildLayout();
            }
            // Add AJAX config if URL is provided
            if ((_b = this.dataSource) === null || _b === void 0 ? void 0 : _b.url) {
                config.ajax = {
                    url: this.dataSource.url,
                    type: this.dataSource.method || 'GET',
                    dataSrc: this.dataSource.dataSrc || ''
                };
                delete config.data;
            }
            // Initialize DataTable
            this.tableInstance = $table.DataTable(config);
            // Bind events
            this.bindActionEvents($table);
            this.bindRowEvents($table);
            // Build form modal if configured
            this.buildFormModal();
            return this.tableInstance;
        }
        /**
         * Get the built DataTable instance
         */
        getTable() {
            return this.tableInstance;
        }
        /**
         * Get the form builder instance
         */
        getFormBuilder() {
            return this.formBuilder;
        }
        /**
         * Reload grid data
         */
        reload(data) {
            var _a;
            if (!this.tableInstance) {
                return;
            }
            if (data) {
                this.tableInstance.clear();
                this.tableInstance.rows.add(data);
                this.tableInstance.draw();
            }
            else if ((_a = this.dataSource) === null || _a === void 0 ? void 0 : _a.url) {
                this.tableInstance.ajax.reload();
            }
        }
        /**
         * Clear grid data
         */
        clear() {
            if (this.tableInstance) {
                this.tableInstance.clear().draw();
            }
        }
        /**
         * Destroy the grid
         */
        destroy() {
            if (this.tableInstance) {
                this.tableInstance.destroy();
                this.tableInstance = null;
            }
        }
    }
    App.GridBuilder = GridBuilder;
})(App || (App = {}));
var App;
(function (App) {
    /**
     * Helper class for building complex grid columns with formatters
     */
    class GridColumnBuilder {
        constructor(field, title) {
            this.column = {
                field: field,
                title: title,
                data: field
            };
        }
        /**
         * Set column visibility
         */
        visible(visible) {
            this.column.visible = visible;
            return this;
        }
        /**
         * Set column width
         */
        width(width) {
            this.column.width = width;
            return this;
        }
        /**
         * Set column CSS class
         */
        className(className) {
            this.column.className = className;
            return this;
        }
        /**
         * Set whether column is orderable
         */
        orderable(orderable) {
            this.column.orderable = orderable;
            return this;
        }
        /**
         * Set whether column is searchable
         */
        searchable(searchable) {
            this.column.searchable = searchable;
            return this;
        }
        /**
         * Set custom render function
         */
        render(renderFn) {
            this.column.render = renderFn;
            return this;
        }
        /**
         * Format as date
         */
        asDate(format = 'DD/MM/YYYY') {
            this.column.render = (data) => {
                if (!data)
                    return '';
                const date = new Date(data);
                // Simple date formatting (you can integrate moment.js or date-fns)
                const day = ('0' + date.getDate()).slice(-2);
                const month = ('0' + (date.getMonth() + 1)).slice(-2);
                const year = date.getFullYear();
                return format
                    .replace('DD', day)
                    .replace('MM', month)
                    .replace('YYYY', String(year));
            };
            return this;
        }
        /**
         * Format as currency
         */
        asCurrency(symbol = '₫', decimalPlaces = 0) {
            this.column.render = (data) => {
                if (data === null || data === undefined)
                    return '';
                const num = Number(data);
                if (isNaN(num))
                    return data;
                return `${num.toLocaleString('vi-VN', {
                    minimumFractionDigits: decimalPlaces,
                    maximumFractionDigits: decimalPlaces
                })} ${symbol}`;
            };
            return this;
        }
        /**
         * Format as boolean with custom display
         */
        asBoolean(trueText = 'Yes', falseText = 'No') {
            this.column.render = (data) => {
                return data ? trueText : falseText;
            };
            return this;
        }
        /**
         * Format as badge
         */
        asBadge(colorMap) {
            this.column.render = (data) => {
                if (!data)
                    return '';
                const color = colorMap ? colorMap(data) : 'secondary';
                return `<span class="badge bg-${color}">${data}</span>`;
            };
            return this;
        }
        /**
         * Apply HTML template
         */
        template(templateFn) {
            this.column.render = (data, type, row) => {
                return templateFn(row);
            };
            return this;
        }
        /**
         * Set default content for null/undefined values
         */
        defaultContent(content) {
            this.column.defaultContent = content;
            return this;
        }
        /**
         * Build and return the column configuration
         */
        build() {
            return this.column;
        }
    }
    App.GridColumnBuilder = GridColumnBuilder;
})(App || (App = {}));
var App;
(function (App) {
    /**
     * Builder for creating automatic CRUD forms/modals
     */
    class GridFormBuilder {
        constructor(config) {
            this.$modal = null;
            this.currentMode = 'create';
            this.currentData = null;
            this.onSaveCallback = null;
            this.onDeleteCallback = null;
            this.config = Object.assign({ size: 'md', createTitle: 'Create New', editTitle: 'Edit', saveButtonText: 'Save', cancelButtonText: 'Cancel', showDeleteButton: false, columns: 1 }, config);
            this.modalId = this.config.id || 'gridModal_' + Math.random().toString(36).substr(2, 9);
        }
        /**
         * Set the save callback
         */
        onSave(callback) {
            this.onSaveCallback = callback;
            return this;
        }
        /**
         * Set the delete callback
         */
        onDelete(callback) {
            this.onDeleteCallback = callback;
            return this;
        }
        /**
         * Build and inject the modal into the page
         */
        build() {
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
        generateModalHtml() {
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
        generateFormHtml() {
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
        generateFieldHtml(field) {
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
        generateFooterHtml() {
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
        bindEvents() {
            if (!this.$modal)
                return;
            // Save button
            this.$modal.find(`#${this.modalId}_saveBtn`).on('click', () => this.handleSave());
            // Delete button
            this.$modal.find(`#${this.modalId}_deleteBtn`).on('click', () => this.handleDelete());
            // Custom buttons
            this.$modal.find('[data-custom-btn]').on('click', (e) => {
                var _a;
                const idx = parseInt($(e.currentTarget).data('custom-btn'));
                const btn = (_a = this.config.footerButtons) === null || _a === void 0 ? void 0 : _a[idx];
                if (btn === null || btn === void 0 ? void 0 : btn.onClick) {
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
        showCreate() {
            this.currentMode = 'create';
            this.currentData = null;
            this.updateModalTitle();
            this.resetForm();
            this.show();
        }
        /**
         * Show modal in edit mode
         */
        showEdit(data) {
            this.currentMode = 'edit';
            this.currentData = data;
            this.updateModalTitle();
            this.populateForm(data);
            this.show();
        }
        /**
         * Show the modal
         */
        show() {
            if (!this.$modal)
                return;
            // Show/hide delete button based on mode
            if (this.config.showDeleteButton) {
                const $deleteBtn = this.$modal.find(`#${this.modalId}_deleteBtn`);
                if (this.currentMode === 'edit') {
                    $deleteBtn.show();
                }
                else {
                    $deleteBtn.hide();
                }
            }
            this.$modal.modal('show');
        }
        /**
         * Hide the modal
         */
        hide() {
            if (!this.$modal)
                return;
            this.$modal.modal('hide');
        }
        /**
         * Update modal title based on mode
         */
        updateModalTitle() {
            if (!this.$modal)
                return;
            const title = this.currentMode === 'create'
                ? this.config.createTitle
                : this.config.editTitle;
            this.$modal.find('.modal-title').text(title || '');
        }
        /**
         * Populate form with data
         */
        populateForm(data) {
            if (!this.$modal)
                return;
            this.config.fields.forEach(field => {
                const fieldId = `${this.modalId}_${String(field.name)}`;
                const $field = this.$modal.find(`#${fieldId}`);
                const value = data[field.name];
                if (field.type === 'checkbox') {
                    $field.prop('checked', !!value);
                }
                else {
                    $field.val(value || '');
                }
            });
        }
        /**
         * Reset form
         */
        resetForm() {
            if (!this.$modal)
                return;
            const $form = this.$modal.find('form');
            $form[0].reset();
            // Set default values
            this.config.fields.forEach(field => {
                if (field.defaultValue !== undefined) {
                    const fieldId = `${this.modalId}_${String(field.name)}`;
                    const $field = this.$modal.find(`#${fieldId}`);
                    if (field.type === 'checkbox') {
                        $field.prop('checked', !!field.defaultValue);
                    }
                    else {
                        $field.val(field.defaultValue);
                    }
                }
            });
        }
        /**
         * Get form data
         */
        getFormData() {
            const formData = {};
            this.config.fields.forEach(field => {
                const fieldId = `${this.modalId}_${String(field.name)}`;
                const $field = this.$modal.find(`#${fieldId}`);
                if (field.type === 'checkbox') {
                    formData[field.name] = $field.prop('checked');
                }
                else if (field.type === 'number') {
                    const val = $field.val();
                    formData[field.name] = val ? Number(val) : null;
                }
                else {
                    formData[field.name] = $field.val();
                }
            });
            return formData;
        }
        /**
         * Validate form
         */
        validateForm() {
            // Simple validation for now
            let isValid = true;
            this.config.fields.forEach(field => {
                if (field.required) {
                    const fieldId = `${this.modalId}_${String(field.name)}`;
                    const $field = this.$modal.find(`#${fieldId}`);
                    const value = field.type === 'checkbox'
                        ? $field.prop('checked')
                        : $field.val();
                    if (!value) {
                        $field.addClass('is-invalid');
                        isValid = false;
                    }
                    else {
                        $field.removeClass('is-invalid');
                    }
                }
            });
            return isValid;
        }
        /**
         * Handle save
         */
        handleSave() {
            return __awaiter(this, void 0, void 0, function* () {
                if (!this.validateForm()) {
                    App.ToastService.warning('Please fill in all required fields');
                    return;
                }
                const formData = this.getFormData();
                // Call beforeSave if provided
                if (this.config.beforeSave) {
                    const canProceed = yield Promise.resolve(this.config.beforeSave(formData, this.currentMode));
                    if (!canProceed) {
                        return;
                    }
                }
                // Call save callback
                if (this.onSaveCallback) {
                    const submission = {
                        data: formData,
                        mode: this.currentMode,
                        originalData: this.currentData || undefined
                    };
                    yield Promise.resolve(this.onSaveCallback(submission));
                }
                // Call afterSave if provided
                if (this.config.afterSave) {
                    yield Promise.resolve(this.config.afterSave(formData, this.currentMode));
                }
                this.hide();
            });
        }
        /**
         * Handle delete
         */
        handleDelete() {
            return __awaiter(this, void 0, void 0, function* () {
                if (!this.currentData || !this.onDeleteCallback) {
                    return;
                }
                // Confirm deletion
                if (!confirm('Are you sure you want to delete this item?')) {
                    return;
                }
                yield Promise.resolve(this.onDeleteCallback(this.currentData));
                this.hide();
            });
        }
        /**
         * Get modal jQuery element
         */
        getModal() {
            return this.$modal;
        }
        /**
         * Get modal ID
         */
        getModalId() {
            return this.modalId;
        }
    }
    App.GridFormBuilder = GridFormBuilder;
})(App || (App = {}));
var App;
(function (App) {
    class LoadingService {
        static show() {
            if (++this.count > 1)
                return;
            $("body").append(`
                <div id="${this.ID}" style="position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(0,0,0,.5);display:flex;justify-content:center;align-items:center;z-index:10000">
                    <div style="border:4px solid #f3f3f3;border-top:4px solid #3498db;border-radius:50%;width:50px;height:50px;animation:app-spin 1s linear infinite">
                    </div>
                </div>
            `);
            if (!$("#app-loading-style").length)
                $("head").append(`<style id="app-loading-style">@keyframes app-spin{0%{transform:rotate(0)}100%{transform:rotate(360deg)}}</style>`);
        }
        static hide() {
            if (this.count === 0 || --this.count > 0)
                return;
            $("#" + this.ID).remove();
        }
    }
    LoadingService.ID = "loadingOverlay";
    LoadingService.count = 0;
    App.LoadingService = LoadingService;
})(App || (App = {}));
var App;
(function (App) {
    class ToastService {
        static show(options) {
            var _a, _b, _c;
            const title = (_a = options.title) !== null && _a !== void 0 ? _a : this.DEFAULT_TITLE;
            const type = (_b = options.type) !== null && _b !== void 0 ? _b : 'info';
            const duration = (_c = options.duration) !== null && _c !== void 0 ? _c : this.DEFAULT_DURATION;
            const config = this.CONFIG[type];
            const toastId = this.generateId();
            this.ensureContainer();
            const html = this.buildHtml(toastId, options.message, title, config);
            const $container = $("#" + this.CONTAINER_ID);
            $container.prepend(html);
            const $toast = $("#" + toastId);
            new bootstrap.Toast($toast[0], { delay: duration }).show();
            $toast.on("hidden.bs.toast", () => $toast.remove());
        }
        static buildHtml(toastId, message, title, config) {
            return `
                <div id="${toastId}" class="toast border-0 mb-2" role="alert" aria-live="assertive" aria-atomic="true" style="min-width:300px;">
                    <div class="toast-header text-white" style="background-color:${config.color};">
                        <i class="bi ${config.icon} me-2"></i>
                        <strong class="me-auto">${title}</strong>
                        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast" aria-label="Close"></button>
                    </div>
                    <div class="toast-body" style="white-space: pre-wrap;">${message}</div>
                </div>
            `;
        }
        static generateId() {
            return "toast-" + Date.now();
        }
        static ensureContainer() {
            if ($("#" + this.CONTAINER_ID).length)
                return;
            $("body").append(`<div id="${this.CONTAINER_ID}" class="position-fixed top-0 end-0 p-3" style="z-index:9999;"></div>`);
        }
        static success(message, title) {
            this.show({ message, title, type: "success" });
        }
        static error(message, title) {
            this.show({ message, title, type: "error" });
        }
        static warning(message, title) {
            this.show({ message, title, type: "warning" });
        }
        static info(message, title) {
            this.show({ message, title, type: "info" });
        }
    }
    ToastService.DEFAULT_TITLE = "Thông báo";
    ToastService.DEFAULT_DURATION = 3000;
    ToastService.CONTAINER_ID = "toast-container";
    ToastService.CONFIG = {
        success: { color: "#28A745", icon: "bi-check-circle-fill" },
        error: { color: "#DC3545", icon: "bi-x-circle-fill" },
        warning: { color: "#FFC107", icon: "bi-exclamation-triangle-fill" },
        info: { color: "#17A2B8", icon: "bi-info-circle-fill" },
    };
    App.ToastService = ToastService;
})(App || (App = {}));
var App;
(function (App) {
    class AdminLanguagePage extends App.BasePage {
        initialize() {
            this.initGrid();
            this.loadData();
        }
        bindEvents() {
            // Event binding is now handled by GridBuilder and GridFormBuilder
        }
        loadData() {
            App.LoadingService.show();
            App.ApiService.get('/Admin/Language/GetListLanguages').then(response => {
                App.LoadingService.hide();
                if (response.isOk()) {
                    const data = response.data || [];
                    this.table.clear();
                    this.table.rows.add(data);
                    this.table.draw();
                }
                else {
                    App.ToastService.error(response.message || 'Error');
                }
            }).catch(() => {
                App.LoadingService.hide();
                App.ToastService.error('Không thể tải danh sách user');
            });
        }
        initGrid() {
            this.gridBuilder = new App.GridBuilder('#tblLanguage')
                .addColumn({ field: 'id', title: 'ID', visible: false })
                .addColumn({ field: 'code', title: 'Code' })
                .addColumn({ field: 'name', title: 'Name' })
                .addToolbarButton({
                icon: 'fa-plus',
                className: 'btn btn-sm btn-outline-secondary',
                title: 'Thêm mới',
                onClick: () => { var _a; return (_a = this.gridBuilder.getFormBuilder()) === null || _a === void 0 ? void 0 : _a.showCreate(); }
            })
                .addActionButton({
                icon: 'fa-pen-to-square',
                className: 'btn-outline-primary',
                title: 'Sửa',
                onClick: (data) => { var _a; return (_a = this.gridBuilder.getFormBuilder()) === null || _a === void 0 ? void 0 : _a.showEdit(data); }
            })
                .addActionButton({
                icon: 'fa-trash',
                className: 'btn-outline-danger',
                title: 'Xóa',
                onClick: (data) => this.deleteLanguage(data)
            })
                .setOptions({
                scrollY: 'calc(100vh - 395px)',
                scrollCollapse: true,
                paging: false,
                info: false,
                ordering: false,
                searching: true,
                autoWidth: true
            })
                .setForm({
                createTitle: 'Thêm ngôn ngữ',
                editTitle: 'Cập nhật ngôn ngữ',
                saveButtonText: 'Lưu',
                cancelButtonText: 'Hủy',
                showDeleteButton: true,
                fields: [
                    {
                        name: 'id',
                        label: 'ID',
                        type: 'hidden'
                    },
                    {
                        name: 'code',
                        label: 'Code',
                        type: 'text',
                        required: true,
                        placeholder: 'Nhập mã ngôn ngữ (vd: en, vi)'
                    },
                    {
                        name: 'name',
                        label: 'Name',
                        type: 'text',
                        required: true,
                        placeholder: 'Nhập tên ngôn ngữ'
                    }
                ]
            });
            this.table = this.gridBuilder.build();
            // Set up form save handler
            const formBuilder = this.gridBuilder.getFormBuilder();
            if (formBuilder) {
                formBuilder.onSave((submission) => this.saveLanguage(submission));
                formBuilder.onDelete((data) => this.deleteLanguage(data));
            }
        }
        saveLanguage(submission) {
            return __awaiter(this, void 0, void 0, function* () {
                const url = submission.mode === 'create'
                    ? '/Admin/Language/Create'
                    : '/Admin/Language/Update';
                App.LoadingService.show();
                try {
                    const response = yield App.ApiService.post(url, submission.data);
                    App.LoadingService.hide();
                    if (response.isOk()) {
                        App.ToastService.success(submission.mode === 'create' ? 'Thêm thành công' : 'Cập nhật thành công');
                        this.loadData();
                    }
                    else {
                        App.ToastService.error(response.message || 'Lỗi khi lưu');
                    }
                }
                catch (error) {
                    App.LoadingService.hide();
                    App.ToastService.error('Lỗi hệ thống');
                }
            });
        }
        deleteLanguage(language) {
            if (!confirm('Bạn có chắc chắn muốn xóa ngôn ngữ này?')) {
                return;
            }
            App.LoadingService.show();
            App.ApiService.post('/Admin/Language/Delete', { id: language.id })
                .then(response => {
                App.LoadingService.hide();
                if (response.isOk()) {
                    App.ToastService.success('Xóa thành công');
                    this.loadData();
                }
                else {
                    App.ToastService.error(response.message || 'Xóa thất bại');
                }
            })
                .catch(() => {
                App.LoadingService.hide();
                App.ToastService.error('Lỗi hệ thống');
            });
        }
    }
    App.AdminLanguagePage = AdminLanguagePage;
})(App || (App = {}));
var App;
(function (App) {
    class AdminTranslationPage extends App.BasePage {
        initialize() {
            this.initGrid();
            this.loadTranslations();
        }
        bindEvents() {
            this.root.find('#btnLoad').on('click', () => this.loadTranslations());
        }
        initGrid() {
            // Convert model languages to select options
            const languageOptions = (this.model.Languages || []).map(lang => ({
                value: lang.Key,
                text: lang.Value
            }));
            this.gridBuilder = new App.GridBuilder('#tblTranslations')
                .addColumn({ field: 'key', title: 'Key' })
                .addColumn({ field: 'value', title: 'Value' })
                .addColumn({ field: 'languageName', title: 'Language' })
                .addToolbarButton({
                icon: 'fa-plus',
                className: 'btn btn-sm btn-outline-secondary',
                title: 'Thêm mới',
                onClick: () => { var _a; return (_a = this.gridBuilder.getFormBuilder()) === null || _a === void 0 ? void 0 : _a.showCreate(); }
            })
                .addActionButton({
                icon: 'fa-pen-to-square',
                className: 'btn-outline-primary',
                title: 'Sửa',
                onClick: (data) => { var _a; return (_a = this.gridBuilder.getFormBuilder()) === null || _a === void 0 ? void 0 : _a.showEdit(data); }
            })
                .addActionButton({
                icon: 'fa-trash',
                className: 'btn-outline-danger',
                title: 'Xóa',
                onClick: (data) => this.deleteTranslation(data)
            })
                .setOptions({
                scrollY: 'calc(100vh - 395px)',
                scrollCollapse: true,
                paging: false,
                info: false,
                ordering: true,
                searching: true,
                autoWidth: true
            })
                .setForm({
                createTitle: 'Thêm bản dịch',
                editTitle: 'Cập nhật bản dịch',
                saveButtonText: 'Lưu',
                cancelButtonText: 'Hủy',
                showDeleteButton: true,
                fields: [
                    {
                        name: 'id',
                        label: 'ID',
                        type: 'hidden'
                    },
                    {
                        name: 'key',
                        label: 'Key',
                        type: 'text',
                        required: true,
                        placeholder: 'Nhập key (vd: common.save)'
                    },
                    {
                        name: 'value',
                        label: 'Value',
                        type: 'textarea',
                        required: true,
                        placeholder: 'Nhập giá trị dịch'
                    },
                    {
                        name: 'languageId',
                        label: 'Language',
                        type: 'select',
                        required: true,
                        options: languageOptions
                    }
                ]
            });
            this.table = this.gridBuilder.build();
            // Set up form save handler
            const formBuilder = this.gridBuilder.getFormBuilder();
            if (formBuilder) {
                formBuilder.onSave((submission) => this.saveTranslation(submission));
                formBuilder.onDelete((data) => this.deleteTranslation(data));
            }
        }
        loadTranslations() {
            const languageId = this.root.find("#ddlLanguageId").val();
            const req = {
                languageId: languageId
            };
            App.LoadingService.show();
            App.ApiService.post('/Admin/Translation/GetListTranslations', req, {
                showLoading: false,
                success: (response) => {
                    App.LoadingService.hide();
                    if (response.isOk()) {
                        const data = response.data || [];
                        this.table.clear();
                        this.table.rows.add(data);
                        this.table.draw();
                    }
                    else {
                        App.ToastService.error(response.message || 'Lấy danh sách thất bại');
                    }
                },
                error: () => {
                    App.LoadingService.hide();
                    App.ToastService.error('Lỗi hệ thống');
                }
            });
        }
        saveTranslation(submission) {
            return __awaiter(this, void 0, void 0, function* () {
                const url = submission.mode === 'create'
                    ? '/Admin/Translation/Create'
                    : '/Admin/Translation/Update';
                App.LoadingService.show();
                try {
                    const response = yield App.ApiService.post(url, submission.data);
                    App.LoadingService.hide();
                    if (response.isOk()) {
                        App.ToastService.success(submission.mode === 'create' ? 'Thêm thành công' : 'Cập nhật thành công');
                        this.loadTranslations();
                    }
                    else {
                        App.ToastService.error(response.message || 'Lỗi khi lưu');
                    }
                }
                catch (error) {
                    App.LoadingService.hide();
                    App.ToastService.error('Lỗi hệ thống');
                }
            });
        }
        deleteTranslation(translation) {
            if (!confirm('Bạn có chắc chắn muốn xóa bản dịch này?')) {
                return;
            }
            App.LoadingService.show();
            App.ApiService.post('/Admin/Translation/Delete', { id: translation.id })
                .then(response => {
                App.LoadingService.hide();
                if (response.isOk()) {
                    App.ToastService.success('Xóa thành công');
                    this.loadTranslations();
                }
                else {
                    App.ToastService.error(response.message || 'Xóa thất bại');
                }
            })
                .catch(() => {
                App.LoadingService.hide();
                App.ToastService.error('Lỗi hệ thống');
            });
        }
    }
    App.AdminTranslationPage = AdminTranslationPage;
})(App || (App = {}));
//# sourceMappingURL=app.js.map