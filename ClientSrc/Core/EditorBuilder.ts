namespace App {
    export interface IEditorOptions {
        placeholder?: string;
        toolbar?: boolean;
        minHeight?: string;
        maxHeight?: string;
        autoFocus?: boolean;
    }

    export class EditorBuilder {
        private selector: string;
        private options: IEditorOptions;
        private editorInstance: any;
        private $container!: JQuery;
        private changeCallback: ((value: string) => void) | null = null;

        constructor(selector: string, options?: IEditorOptions) {
            this.selector = selector;
            this.options = {
                toolbar: true,
                minHeight: '300px',
                autoFocus: false,
                ...options
            };
        }

        public setPlaceholder(placeholder: string): this {
            this.options.placeholder = placeholder;
            return this;
        }

        public setMinHeight(height: string): this {
            this.options.minHeight = height;
            return this;
        }

        public setMaxHeight(height: string): this {
            this.options.maxHeight = height;
            return this;
        }

        public hideToolbar(): this {
            this.options.toolbar = false;
            return this;
        }

        public autoFocus(): this {
            this.options.autoFocus = true;
            return this;
        }

        public onChange(callback: (value: string) => void): this {
            this.changeCallback = callback;
            return this;
        }

        public build(): this {
            this.$container = $(this.selector);
            if (this.$container.length === 0) {
                console.error(`EditorBuilder: Selector '${this.selector}' not found.`);
                return this;
            }

            const easyMDE = (window as any).EasyMDE;
            if (!easyMDE) {
                console.error('EditorBuilder: EasyMDE is not loaded');
                return this;
            }

            const textareaId = `editor-${Math.random().toString(36).substr(2, 9)}`;
            this.$container.html(`<textarea id="${textareaId}"></textarea>`);

            this.editorInstance = new easyMDE({
                element: document.getElementById(textareaId),
                placeholder: this.options.placeholder ?? '',
                toolbar: this.options.toolbar ? undefined : false,
                minHeight: this.options.minHeight,
                maxHeight: this.options.maxHeight,
                spellChecker: false,
                autofocus: this.options.autoFocus ?? false,
                status: false,
                renderingConfig: { singleLineBreaks: false }
            });

            if (this.changeCallback) {
                this.editorInstance.codemirror.on('change', () => {
                    this.changeCallback!(this.editorInstance.value());
                });
            }

            return this;
        }

        public getValue(): string {
            return this.editorInstance?.value() ?? '';
        }

        public setValue(content: string): this {
            if (this.editorInstance) {
                this.editorInstance.value(content);
            }
            return this;
        }

        public clear(): this {
            this.editorInstance?.value('');
            return this;
        }

        public focus(): this {
            this.editorInstance?.codemirror.focus();
            return this;
        }

        public destroy(): void {
            this.editorInstance?.toTextArea();
            this.$container.empty();
        }

        public getElement(): JQuery {
            return this.$container;
        }
    }
}
