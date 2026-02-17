namespace App {
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
    }
}