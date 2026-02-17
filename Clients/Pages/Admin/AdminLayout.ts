namespace App {
    export class AdminLayout extends BasePage {
        protected initialize(): void {
            this.initSidebar();
        }

        protected bindEvents(): void {
            this.root.find('#back_btn').on('click', () => this.onBackClick());
        }

        private initSidebar(): void {
            let currentUrl = window.location.pathname.toLowerCase();
            const defaultMap: Record<string, string> = {
                "/admin": "/admin/dashboard",
                "/admin/": "/admin/dashboard"
            };

            if (defaultMap[currentUrl]) {
                currentUrl = defaultMap[currentUrl];
            }

            this.root.find(".sub-menu").each((_, el) => {
                const $submenu = $(el);
                $submenu.find(".sub-link").each((_, linkEl) => {
                    const $link = $(linkEl);
                    if ($link.attr("href")?.toLowerCase() === currentUrl) {
                        $link.addClass("active");
                        $link.closest(".sub-item").addClass("active");
                        $submenu.addClass("show");
                        $submenu.prev(".menu-link").removeClass("collapsed").addClass("active");
                        $submenu.closest(".menu-item").addClass("active");
                    }
                });
            });

            this.root.find(".menu-item > .menu-link").each((_, el) => {
                const $link = $(el);
                if ($link.attr("href") && $link.attr("href").toLowerCase() === currentUrl) {
                    $link.addClass("active");
                    $link.closest(".menu-item").addClass("active");
                }
            });
        }

        private onBackClick(): void {
            const backUrl = (window as any).options?.backUrl;
            if (backUrl) {
                window.location.href = backUrl;
            }
        }
    }
}
