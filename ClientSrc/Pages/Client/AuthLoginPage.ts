namespace App {
    export class AuthLoginPage extends BasePage {
        protected initialize(): void {
        }

        protected bindEvents(): void {
            this.root.find("#loginForm").on("submit", (e) => this.handleLogin(e));
        }

        private handleLogin(event: JQuery.SubmitEvent): void {
            event.preventDefault();

            const formData = {
                userName: this.root.find("#username").val(),
                password: this.root.find("#password").val()
            };

            LoadingService.show();
            ApiService.post('/auth/login', formData).then(response => {
                LoadingService.hide();
                if (response.isOk()) {
                    const params = new URLSearchParams(window.location.search);
                    let returnUrl = params.get("returnUrl");

                    if (!returnUrl) {
                        returnUrl = sessionStorage.getItem("returnUrl") || "/";
                    }
                    sessionStorage.removeItem("returnUrl");

                    window.location.href = returnUrl;
                } else {
                    ToastService.error(response.message || 'Sai tài khoản hoặc mật khẩu');
                }
            }).catch(() => {
                LoadingService.hide();
                ToastService.error('Lỗi hệ thống');
            });
        }
    }
}
