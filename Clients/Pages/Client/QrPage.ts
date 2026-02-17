namespace App {
    export class QrPage extends BasePage {
        protected initialize(): void {
        }

        protected bindEvents(): void {
            this.root.find("#btnGenerate").on("click", () => this.generateQr());
        }

        private generateQr(): void {
            const text = this.root.find("#textInput").val() as string;
            if (!text) {
                ToastService.warning("Vui lòng nhập nội dung QR!");
                return;
            }

            LoadingService.show();
            ApiService.get("/qr/generate", { text: text }).then(res => {
                LoadingService.hide();
                if (res.isOk()) {
                    const base64 = "data:image/png;base64," + res.data;

                    this.root.find("#qrImage").attr("src", base64);
                    this.root.find("#qrContainer").show();

                    const link = document.createElement("a");
                    link.href = base64;
                    link.download = "qr.png";
                    document.body.appendChild(link);
                    link.click();
                    document.body.removeChild(link);
                } else {
                    ToastService.error(res.message || "Lỗi khi tạo QR");
                }
            }).catch(() => {
                LoadingService.hide();
                ToastService.error("Lỗi hệ thống");
            });
        }
    }
}
