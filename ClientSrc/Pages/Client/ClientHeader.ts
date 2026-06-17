namespace App {
    export class ClientHeader extends BasePage {
        private currentLangCode: string;

        protected initialize(): void {
            this.currentLangCode = this.root.find('#currentLangCode').val() as string;
            this.loadLanguages();
            this.bindToggle();
            this.bindLogout();
        }

        private bindToggle(): void {
            const $toggle = this.root.find('#header-toggle');
            const $nav = this.root.find('#header-nav');
            const $right = this.root.find('#header-right');

            $toggle.on('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                $toggle.toggleClass('open');
                $nav.toggleClass('open');
                $right.toggleClass('open');
            });

            // Close menu when clicking a nav link (mobile)
            $nav.find('.nav-link:not(.dropdown-toggle)').on('click', () => {
                $toggle.removeClass('open');
                $nav.removeClass('open');
                $right.removeClass('open');
            });

            // Close menu when clicking outside
            $(document).on('click', (e) => {
                if ($toggle.hasClass('open') && !$(e.target).closest('#client-header').length) {
                    $toggle.removeClass('open');
                    $nav.removeClass('open');
                    $right.removeClass('open');
                }
            });
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
            }).catch((err: unknown) => console.error('Không thể tải danh sách ngôn ngữ', err));
        }

        private bindLogout(): void {
            this.root.find('#client-logout-btn').on('click', (e) => {
                e.preventDefault();
                ApiService.post('/Auth/Logout', {}, { showLoading: true }).then(res => {
                    if (res.isOk()) {
                        window.location.href = '/Auth/Login';
                    } else {
                        ToastService.error(res.message || 'Đăng xuất thất bại');
                    }
                }).catch(() => {
                    ToastService.error('Lỗi hệ thống');
                });
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
            }).catch((err: unknown) => {
                console.error('Không thể đổi ngôn ngữ', err);
                ToastService.error('Lỗi hệ thống');
            });
        }
    }
}
