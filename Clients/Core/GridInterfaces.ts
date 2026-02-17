namespace App {
    /**
     * Column configuration for the grid
     */
    export interface IGridColumn<T = any> {
        /** Field name from the data model */
        field?: keyof T;
        /** Column header title */
        title: string;
        /** Column data property (for nested or custom data access) */
        data?: string | ((row: T, type: string, set: any, meta: any) => any);
        /** Whether the column is visible */
        visible?: boolean;
        /** Column width */
        width?: string;
        /** CSS class for the column */
        className?: string;
        /** Whether the column is orderable */
        orderable?: boolean;
        /** Whether the column is searchable */
        searchable?: boolean;
        /** Custom render function */
        render?: (data: any, type: string, row: T, meta: any) => string | number;
        /** Default content when data is null/undefined */
        defaultContent?: string;
    }

    /**
     * Toolbar button configuration
     */
    export interface IGridButton {
        /** Button text (can be HTML) */
        text?: string;
        /** Icon class (e.g., 'fa-plus') */
        icon?: string;
        /** Button CSS classes */
        className?: string;
        /** Button title/tooltip */
        title?: string;
        /** Click handler */
        onClick: () => void;
        /** Whether the button is visible */
        visible?: boolean | (() => boolean);
    }

    /**
     * Row action button configuration
     */
    export interface IGridActionButton<T = any> {
        /** Icon class (e.g., 'fa-edit') */
        icon: string;
        /** Button CSS classes */
        className?: string;
        /** Button title/tooltip */
        title?: string;
        /** Click handler with row data */
        onClick: (data: T, event?: JQuery.ClickEvent) => void;
        /** Conditional visibility based on row data */
        visible?: boolean | ((data: T) => boolean);
        /** Custom HTML render function */
        render?: (data: T) => string;
    }

    /**
     * Grid display and behavior options
     */
    export interface IGridOptions {
        /** Enable/disable paging */
        paging?: boolean;
        /** Page length */
        pageLength?: number;
        /** Enable/disable info display */
        info?: boolean;
        /** Enable/disable ordering */
        ordering?: boolean;
        /** Enable/disable searching */
        searching?: boolean;
        /** Auto width calculation */
        autoWidth?: boolean;
        /** Scroll Y height */
        scrollY?: string;
        /** Scroll collapse */
        scrollCollapse?: boolean;
        /** Responsive mode */
        responsive?: boolean;
        /** Destroy existing table before reinit */
        destroy?: boolean;
        /** Language settings */
        language?: any;
        /** DOM layout */
        dom?: string;
        /** Custom initialization */
        initComplete?: (settings: any, json: any) => void;
        /** Row callback */
        rowCallback?: (row: Node, data: any, index: number) => void;
    }

    /**
     * Data source configuration
     */
    export interface IGridDataSource<T = any> {
        /** Static data array */
        data?: T[];
        /** AJAX URL for server-side data */
        url?: string;
        /** HTTP method */
        method?: 'GET' | 'POST';
        /** Request data transformation */
        dataSrc?: string | ((json: any) => T[]);
        /** Auto-load data on initialization */
        autoLoad?: boolean;
    }

    /**
     * CRUD callback configuration
     */
    export interface IGridCallbacks<T = any> {
        /** Create callback */
        onCreate?: () => void;
        /** Edit callback */
        onEdit?: (data: T) => void;
        /** Delete callback */
        onDelete?: (data: T) => void;
        /** Row click callback */
        onRowClick?: (data: T, event: JQuery.ClickEvent) => void;
        /** Selection change callback */
        onSelectionChanged?: (selectedRows: T[]) => void;
    }

    /**
     * Action column configuration
     */
    export interface IGridActionColumn {
        /** Column title */
        title?: string;
        /** Column width */
        width?: string;
        /** CSS class */
        className?: string;
        /** Column position (0 = first, -1 = last) */
        position?: number;
    }

    /**
     * Grid layout configuration
     */
    export interface IGridLayout {
        /** Top start elements */
        topStart?: string[];
        /** Top end elements */
        topEnd?: string[];
        /** Bottom start elements */
        bottomStart?: string[];
        /** Bottom end elements */
        bottomEnd?: string[];
    }

    /**
     * Column formatter options
     */
    export interface IColumnFormatter<T = any> {
        /** Date format (e.g., 'DD/MM/YYYY') */
        dateFormat?: string;
        /** Currency symbol */
        currencySymbol?: string;
        /** Number of decimal places */
        decimalPlaces?: number;
        /** Boolean display values */
        booleanDisplay?: { true: string; false: string };
        /** Custom formatter function */
        format?: (value: any, row: T) => string;
    }
}
