namespace App {
    export class AdminTranslationPage extends BasePage {

        protected initialize(): void {
            LoadingService.show();
            ToastService.success("Page loaded");
            LoadingService.hide();
        }
    }
}