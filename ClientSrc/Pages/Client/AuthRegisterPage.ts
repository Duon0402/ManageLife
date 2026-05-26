namespace App {
    export class AuthRegisterPage extends BasePage {
        protected initialize(): void {
        }

        protected bindEvents(): void {
            this.root.find("#registerForm").on("submit", (e) => this.handleRegister(e));
            this.root.find("#togglePassword").on("click", () => this.toggleInput("#password", "#togglePasswordIcon"));
            this.root.find("#toggleConfirm").on("click", () => this.toggleInput("#confirmPassword", "#toggleConfirmIcon"));
        }

        private toggleInput(inputSel: string, iconSel: string): void {
            const input = this.root.find(inputSel);
            const icon = this.root.find(iconSel);
            if (input.attr("type") === "password") {
                input.attr("type", "text");
                icon.removeClass("fa-eye").addClass("fa-eye-slash");
            } else {
                input.attr("type", "password");
                icon.removeClass("fa-eye-slash").addClass("fa-eye");
            }
        }

        private handleRegister(event: JQuery.SubmitEvent): void {
            event.preventDefault();

            const formData = {
                userName: this.root.find("#username").val(),
                password: this.root.find("#password").val(),
                confirmPassword: this.root.find("#confirmPassword").val(),
            };

            if (formData.password !== formData.confirmPassword) {
                ToastService.warning("Mật khẩu xác nhận không khớp");
                return;
            }

            LoadingService.show();
            ApiService.post('/Auth/Register', formData).then(response => {
                LoadingService.hide();
                if (response.isOk()) {
                    ToastService.success("Đăng ký thành công");
                    window.location.href = '/';
                } else {
                    ToastService.error(response.message || 'Đăng ký thất bại');
                }
            }).catch(() => {
                LoadingService.hide();
                ToastService.error('Lỗi hệ thống');
            });
        }
    }
}
