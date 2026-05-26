namespace App {
    export class LoadingService {
        private static readonly OVERLAY_ID = "ml-loading-overlay";
        private static count = 0;

        public static show(): void {
            if (++this.count > 1) return;
            if (!$("#" + this.OVERLAY_ID).length) {
                $("body").append(`
                    <div id="${this.OVERLAY_ID}" class="ml-loading-overlay">
                        <div class="ml-loading-spinner"></div>
                    </div>
                `);
            }
        }

        public static hide(): void {
            if (this.count === 0 || --this.count > 0) return;
            $("#" + this.OVERLAY_ID).remove();
        }
    }
}
