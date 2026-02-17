namespace App {
    export class ClientHeader extends BasePage {
        private currentLangCode: string;

        protected initialize(): void {
            this.currentLangCode = this.root.find('#currentLangCode').val() as string;
            this.loadLanguages();
        }

        private loadLanguages(): void {
            ApiService.get('/Language/GetListLanguages', {}, { showLoading: false }).then(res => {
                if (res.isOk() && res.data) {
                    const $menu = this.root.find('#languageMenu');
                    $menu.empty();

                    res.data.forEach((lang: any) => {
                        const activeClass = (lang.code === this.currentLangCode) ? "active" : "";
                        const $item = $('<a>', {
                            class: `dropdown-item ${activeClass}`,
                            href: '#',
                            text: lang.name,
                            click: (e: JQuery.ClickEvent) => {
                                e.preventDefault();
                                this.changeLanguage(lang.code);
                            }
                        });
                        $menu.append($('<li>').append($item));
                    });
                }
            });
        }

        private changeLanguage(langCode: string): void {
            const req = {
                LanguageCode: langCode,
                ReturnUrl: window.location.pathname + window.location.search
            };

            ApiService.post('/Language/ChangeLanguage', req, { showLoading: false }).then(res => {
                if (res.isOk()) {
                    if (res.data && res.data.returnUrl) {
                        window.location.href = res.data.returnUrl;
                    } else {
                        location.reload();
                    }
                } else {
                    ToastService.error(res.message || "Không đổi được ngôn ngữ");
                }
            });
        }
    }
}
