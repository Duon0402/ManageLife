namespace App {
    /**
     * Helper class for building complex grid columns with formatters
     */
    export class GridColumnBuilder<T = any> {
        private column: IGridColumn<T>;

        constructor(field: keyof T, title: string) {
            this.column = {
                field: field,
                title: title,
                data: field as string
            };
        }

        /**
         * Set column visibility
         */
        visible(visible: boolean): this {
            this.column.visible = visible;
            return this;
        }

        /**
         * Set column width
         */
        width(width: string): this {
            this.column.width = width;
            return this;
        }

        /**
         * Set column CSS class
         */
        className(className: string): this {
            this.column.className = className;
            return this;
        }

        /**
         * Set whether column is orderable
         */
        orderable(orderable: boolean): this {
            this.column.orderable = orderable;
            return this;
        }

        /**
         * Set whether column is searchable
         */
        searchable(searchable: boolean): this {
            this.column.searchable = searchable;
            return this;
        }

        /**
         * Set custom render function
         */
        render(renderFn: (data: any, type: string, row: T, meta: any) => string | number): this {
            this.column.render = renderFn;
            return this;
        }

        /**
         * Format as date
         */
        asDate(format: string = 'DD/MM/YYYY'): this {
            this.column.render = (data) => {
                if (!data) return '';
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
        asCurrency(symbol: string = '₫', decimalPlaces: number = 0): this {
            this.column.render = (data) => {
                if (data === null || data === undefined) return '';
                const num = Number(data);
                if (isNaN(num)) return data;
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
        asBoolean(trueText: string = 'Yes', falseText: string = 'No'): this {
            this.column.render = (data) => {
                return data ? trueText : falseText;
            };
            return this;
        }

        /**
         * Format as badge
         */
        asBadge(colorMap?: (value: any) => string): this {
            this.column.render = (data) => {
                if (!data) return '';
                const color = colorMap ? colorMap(data) : 'secondary';
                return `<span class="badge bg-${color}">${data}</span>`;
            };
            return this;
        }

        /**
         * Apply HTML template
         */
        template(templateFn: (row: T) => string): this {
            this.column.render = (data, type, row) => {
                return templateFn(row);
            };
            return this;
        }

        /**
         * Set default content for null/undefined values
         */
        defaultContent(content: string): this {
            this.column.defaultContent = content;
            return this;
        }

        /**
         * Build and return the column configuration
         */
        build(): IGridColumn<T> {
            return this.column;
        }
    }
}
