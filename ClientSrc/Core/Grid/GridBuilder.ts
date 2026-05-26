namespace App {
    /**
     * Fluent builder for creating DataTables grids with type safety
     * Usage similar to DevExtreme grid configuration
     */
    export class GridBuilder<T = any> {
        private selector: string;
        private columns: IGridColumn<T>[] = [];
        private toolbarButtons: IGridButton[] = [];
        private actionButtons: IGridActionButton<T>[] = [];
        private options: IGridOptions = {
            paging: false,
            info: false,
            ordering: false,
            searching: true,
            autoWidth: true,
            destroy: true
        };
        private dataSource: IGridDataSource<T> | null = null;
        private callbacks: IGridCallbacks<T> = {};
        private actionColumnConfig: IGridActionColumn = {
            title: '',
            width: '120px',
            className: 'text-center',
            position: -1 // Last column by default
        };
        private layout: IGridLayout | null = null;
        private tableInstance: DataTables.Api | null = null;
        private formBuilder: GridFormBuilder<T> | null = null;
        private formConfig: IGridModal<T> | null = null;

        constructor(selector: string) {
            this.selector = selector;
        }

        /**
         * Add a column to the grid
         */
        addColumn(config: IGridColumn<T> | GridColumnBuilder<T>): this {
            if (config instanceof GridColumnBuilder) {
                this.columns.push(config.build());
            } else {
                // Ensure data property is set if field is provided
                if (config.field && !config.data) {
                    config.data = config.field as string;
                }
                this.columns.push(config);
            }
            return this;
        }

        /**
         * Add multiple columns at once
         */
        addColumns(configs: Array<IGridColumn<T> | GridColumnBuilder<T>>): this {
            configs.forEach(config => this.addColumn(config));
            return this;
        }

        /**
         * Add a toolbar button
         */
        addToolbarButton(config: IGridButton): this {
            this.toolbarButtons.push(config);
            return this;
        }

        /**
         * Add a row action button (edit, delete, etc.)
         */
        addActionButton(config: IGridActionButton<T>): this {
            this.actionButtons.push(config);
            return this;
        }

        /**
         * Configure action column appearance
         */
        configureActionColumn(config: Partial<IGridActionColumn>): this {
            this.actionColumnConfig = { ...this.actionColumnConfig, ...config };
            return this;
        }

        /**
         * Set grid options
         */
        setOptions(options: IGridOptions): this {
            this.options = { ...this.options, ...options };
            return this;
        }

        /**
         * Set data source
         */
        setDataSource(dataSource: IGridDataSource<T> | T[]): this {
            if (Array.isArray(dataSource)) {
                this.dataSource = { data: dataSource };
            } else {
                this.dataSource = dataSource;
            }
            return this;
        }

        /**
         * Set layout configuration
         */
        setLayout(layout: IGridLayout): this {
            this.layout = layout;
            return this;
        }

        /**
         * Set create callback
         */
        onCreate(callback: () => void): this {
            this.callbacks.onCreate = callback;
            return this;
        }

        /**
         * Set edit callback
         */
        onEdit(callback: (data: T) => void): this {
            this.callbacks.onEdit = callback;
            return this;
        }

        /**
         * Set delete callback
         */
        onDelete(callback: (data: T) => void): this {
            this.callbacks.onDelete = callback;
            return this;
        }

        /**
         * Set row click callback
         */
        onRowClick(callback: (data: T, event: JQuery.ClickEvent) => void): this {
            this.callbacks.onRowClick = callback;
            return this;
        }

        /**
         * Configure automatic modal form for create/edit operations
         */
        setForm(config: IGridModal<T>): this {
            this.formConfig = config;
            return this;
        }

        /**
         * Build and initialize the form modal
         */
        private buildFormModal(): void {
            if (!this.formConfig) {
                return;
            }

            this.formBuilder = new GridFormBuilder<T>(this.formConfig);
            this.formBuilder.build();

            // Set up save callback to handle form submission
            this.formBuilder.onSave((submission: IFormSubmission<T>) => {
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
                    this.formBuilder?.showCreate();
                };
            }

            // Override onEdit callback to use form modal
            if (!this.callbacks.onEdit) {
                this.callbacks.onEdit = (data: T) => {
                    this.formBuilder?.showEdit(data);
                };
            }
        }

        /**
         * Handle form save submission
         */
        private async handleFormSave(submission: IFormSubmission<T>): Promise<void> {
            // This method should be overridden or handled by callbacks
            // You can emit events or call API services here
            console.log('Form submitted:', submission);
        }

        /**
         * Build the action column if action buttons are defined
         */
        private buildActionColumn(): IGridColumn<T> | null {
            if (this.actionButtons.length === 0) {
                return null;
            }

            return {
                title: this.actionColumnConfig.title ?? '',
                data: undefined,
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
        private buildButtons(): any[] {
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
        private buildLayout(): any {
            if (this.layout) {
                return this.layout;
            }

            if (this.toolbarButtons.length > 0) {
                return {
                    topStart: null,
                    topEnd: ['buttons', 'search']
                };
            }

            return {
                topStart: null,
                topEnd: 'search'
            };
        }

        /**
         * Bind action button events
         */
        private bindActionEvents($table: JQuery): void {
            if (this.actionButtons.length === 0) {
                return;
            }

            // Delegate click events for action buttons
            $table.off('click', '.grid-action-btn').on('click', '.grid-action-btn', (e) => {
                e.preventDefault();
                e.stopPropagation();

                const $btn = $(e.currentTarget);
                const actionIcon = $btn.data('action');
                const $row = $btn.closest('tr');
                const rowData = this.tableInstance?.row($row).data() as T;

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
        private bindRowEvents($table: JQuery): void {
            if (!this.callbacks.onRowClick) {
                return;
            }

            $table.off('click', 'tbody tr').on('click', 'tbody tr', (e) => {
                // Don't trigger if clicking on action buttons
                if ($(e.target).closest('.grid-action-btn').length > 0) {
                    return;
                }

                const rowData = this.tableInstance?.row(e.currentTarget).data() as T;
                if (rowData && this.callbacks.onRowClick) {
                    this.callbacks.onRowClick(rowData, e);
                }
            });
        }

        /**
         * Build and initialize the DataTable
         */
        build(): DataTables.Api {
            const $table = $(this.selector);

            if ($table.length === 0) {
                throw new Error(`Grid selector "${this.selector}" not found`);
            }

            // Add action column if needed
            const actionColumn = this.buildActionColumn();
            const allColumns = [...this.columns];

            if (actionColumn) {
                const pos = this.actionColumnConfig.position ?? -1;
                if (pos === -1 || pos >= allColumns.length) {
                    allColumns.push(actionColumn);
                } else {
                    allColumns.splice(pos, 0, actionColumn);
                }
            }

            // Build DataTables configuration
            const config: any = {
                ...this.options,
                columns: allColumns,
                data: this.dataSource?.data || []
            };

            // Layout always set for consistent positioning; buttons added when configured
            config.layout = this.buildLayout();
            if (this.toolbarButtons.length > 0) {
                config.buttons = this.buildButtons();
            }

            // Add AJAX config if URL is provided
            if (this.dataSource?.url) {
                config.ajax = {
                    url: this.dataSource.url,
                    method: this.dataSource.method || 'GET',
                    dataSrc: (res: any) => {
                        if (res && res.code === Constants.ApiCode.SUCCESS) {
                            const ds = this.dataSource?.dataSrc || 'data';
                            if (typeof ds === 'function') {
                                return ds(res);
                            }
                            return ds === '' ? res : res[ds as string];
                        }
                        console.error('Grid AJAX error:', res);
                        return [];
                    }
                };
                delete config.data;
            }

            // Initialize DataTable
            this.tableInstance = $table.DataTable(config as any);

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
        getTable(): DataTables.Api | null {
            return this.tableInstance;
        }

        /**
         * Get the form builder instance
         */
        getFormBuilder(): GridFormBuilder<T> | null {
            return this.formBuilder;
        }

        /**
         * Reload grid data
         */
        reload(data?: T[]): void {
            if (!this.tableInstance) {
                return;
            }

            if (data) {
                this.tableInstance.clear();
                this.tableInstance.rows.add(data);
                this.tableInstance.draw();
            } else if (this.dataSource?.url) {
                this.tableInstance.ajax.reload();
            }
        }

        /**
         * Clear grid data
         */
        clear(): void {
            if (this.tableInstance) {
                this.tableInstance.clear().draw();
            }
        }

        /**
         * Destroy the grid
         */
        destroy(): void {
            if (this.tableInstance) {
                this.tableInstance.destroy();
                this.tableInstance = null;
            }
        }
    }
}