namespace App {
    export class LoadingService {
        private static readonly ID = "loadingOverlay";
        private static count = 0;
        public static show(): void {
            if (++this.count > 1) return;
            $("body").append(`
                <div id="${this.ID}" style="position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(0,0,0,.5);display:flex;justify-content:center;align-items:center;z-index:10000">
                    <div style="border:4px solid #f3f3f3;border-top:4px solid #3498db;border-radius:50%;width:50px;height:50px;animation:app-spin 1s linear infinite">
                    </div>
                </div>
            `);
            if (!$("#app-loading-style").length)
                $("head").append(`<style id="app-loading-style">@keyframes app-spin{0%{transform:rotate(0)}100%{transform:rotate(360deg)}}</style>`);
        }
        public static hide(): void {
            if (this.count === 0 || --this.count > 0) return;
            $("#" + this.ID).remove();
        }
    }
}
