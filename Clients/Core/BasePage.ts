namespace App {
    export abstract class BasePage {
        protected root: JQuery;

        constructor(rootSelector?: string) {
            this.root = rootSelector ? $(rootSelector) : $("body");
            this.initialize();
            this.bindEvents();
        }

        protected initialize(): void {

        }

        protected bindEvents(): void {

        }
    }
}