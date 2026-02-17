namespace App {
    /**
     * Form field types
     */
    export type FormFieldType = 'text' | 'number' | 'email' | 'password' | 'textarea' |
        'select' | 'checkbox' | 'radio' | 'date' | 'datetime' |
        'file' | 'hidden' | 'color';

    /**
     * Form field configuration
     */
    export interface IFormField<T = any> {
        /** Field name (from data model) */
        name: keyof T | string;
        /** Field label */
        label: string;
        /** Field type */
        type?: FormFieldType;
        /** Placeholder text */
        placeholder?: string;
        /** Whether field is required */
        required?: boolean;
        /** Whether field is readonly */
        readonly?: boolean;
        /** Whether field is disabled */
        disabled?: boolean;
        /** Default value */
        defaultValue?: any;
        /** CSS class for field container */
        className?: string;
        /** Field validation rules */
        validation?: IFieldValidation;
        /** Options for select/radio fields */
        options?: ISelectOption[];
        /** Custom render function */
        render?: (value: any, mode: 'create' | 'edit') => string;
        /** Whether to show this field */
        visible?: boolean | ((mode: 'create' | 'edit') => boolean);
        /** Field help text */
        helpText?: string;
        /** Column span (for grid layout) */
        colSpan?: number;
    }

    /**
     * Select field option
     */
    export interface ISelectOption {
        /** Option value */
        value: any;
        /** Option display text */
        text: string;
        /** Whether option is selected */
        selected?: boolean;
        /** Whether option is disabled */
        disabled?: boolean;
    }

    /**
     * Field validation rules
     */
    export interface IFieldValidation {
        /** Minimum length for text */
        minLength?: number;
        /** Maximum length for text */
        maxLength?: number;
        /** Minimum value for numbers */
        min?: number;
        /** Maximum value for numbers */
        max?: number;
        /** Pattern for regex validation */
        pattern?: RegExp | string;
        /** Custom validation function */
        custom?: (value: any, formData: any) => boolean | string;
        /** Custom error message */
        message?: string;
    }

    /**
     * Modal configuration
     */
    export interface IGridModal<T = any> {
        /** Modal ID (auto-generated if not provided) */
        id?: string;
        /** Modal title for create mode */
        createTitle?: string;
        /** Modal title for edit mode */
        editTitle?: string;
        /** Modal size */
        size?: 'sm' | 'md' | 'lg' | 'xl';
        /** Form fields */
        fields: IFormField<T>[];
        /** Save button text */
        saveButtonText?: string;
        /** Cancel button text */
        cancelButtonText?: string;
        /** Show delete button in edit mode */
        showDeleteButton?: boolean;
        /** Custom footer buttons */
        footerButtons?: IModalButton[];
        /** Before save callback */
        beforeSave?: (data: Partial<T>, mode: 'create' | 'edit') => boolean | Promise<boolean>;
        /** After save callback */
        afterSave?: (data: Partial<T>, mode: 'create' | 'edit') => void | Promise<void>;
        /** Form layout columns */
        columns?: number;
    }

    /**
     * Modal button configuration
     */
    export interface IModalButton {
        /** Button text */
        text: string;
        /** Button CSS class */
        className?: string;
        /** Click handler */
        onClick: (formData: any) => void | Promise<void>;
        /** Whether button should close modal after click */
        closeAfter?: boolean;
    }

    /**
     * Form submission data
     */
    export interface IFormSubmission<T = any> {
        /** Form data */
        data: Partial<T>;
        /** Submission mode */
        mode: 'create' | 'edit';
        /** Original data (for edit mode) */
        originalData?: T;
    }
}
