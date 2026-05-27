namespace App {
    interface PageActionConfig {
        label?: string;
        icon?: string;
        className?: string;
        href?: string;
        onClick?: () => void;
    }

    export abstract class BasePage<TModel = any> {
        protected root: JQuery;
        protected model: TModel;

        constructor(rootSelector: string, model?: TModel) {
            if (!rootSelector) {
                throw new Error("rootSelector is required");
            }
            this.root = $(rootSelector);
            this.model = model || {} as TModel;
            this.initialize();
            this.bindEvents();
        }

        protected initialize(): void {

        }

        protected bindEvents(): void {

        }

        protected addPageAction(config: PageActionConfig): void {
            const $container = $('#page-actions-container');
            if ($container.length === 0) return;

            const iconHtml = config.icon ? `<i class="fa-solid ${config.icon} me-1"></i>` : '';
            const cls = `btn btn-sm ${config.className || 'btn-outline-secondary'}`;

            if (config.href) {
                $container.append(`<a class="${cls}" href="${config.href}">${iconHtml}${config.label || ''}</a>`);
            } else {
                const $btn = $(`<button class="${cls}">${iconHtml}${config.label || ''}</button>`);
                if (config.onClick) $btn.on('click', config.onClick);
                $container.append($btn);
            }
        }
    }
}