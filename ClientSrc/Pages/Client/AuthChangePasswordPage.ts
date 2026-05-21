namespace App {
    export class AuthChangePasswordPage extends BasePage {
        protected initialize(): void {
        }

        protected bindEvents(): void {
            this.root.find("#changePasswordForm").on("submit", (e) => this.handleSubmit(e));
        }

        private handleSubmit(event: JQuery.SubmitEvent): void {
            event.preventDefault();

            const newPassword = this.root.find("#newPassword").val() as string;
            const confirmNewPassword = this.root.find("#confirmNewPassword").val() as string;

            if (newPassword !== confirmNewPassword) {
                ToastService.warning("Mật khẩu xác nhận không khớp");
                return;
            }

            if (newPassword.length < 6) {
                ToastService.warning("Mật khẩu mới phải có ít nhất 6 ký tự");
                return;
            }

            const formData = {
                oldPassword: this.root.find("#oldPassword").val(),
                newPassword: newPassword,
                confirmPassword: confirmNewPassword,
            };

            LoadingService.show();
            ApiService.post('/Auth/ChangePassword', formData).then(response => {
                LoadingService.hide();
                if (response.isOk()) {
                    ToastService.success("Đổi mật khẩu thành công");
                    setTimeout(() => {
                        window.location.href = '/Auth/Login';
                    }, 1200);
                } else {
                    ToastService.error(response.message || 'Đổi mật khẩu thất bại');
                }
            }).catch(() => {
                LoadingService.hide();
                ToastService.error('Lỗi hệ thống');
            });
        }
    }
}
