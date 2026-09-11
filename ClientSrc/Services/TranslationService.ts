namespace App {
    export class TranslationService {
        private static _dict: Record<string, string> = {};
        private static _initialized = false;
        private static _loadPromise: Promise<void> | null = null;

        public static async load(): Promise<void> {
            if (this._initialized) return;
            if (this._loadPromise) return this._loadPromise;

            this._loadPromise = ApiService.get<Record<string, string>>('/Language/GetTranslations')
                .then(res => {
                    if (res.isOk() && res.data) {
                        this._dict = res.data;
                    }
                    this._initialized = true;
                })
                .catch(() => {
                    this._initialized = true;
                });

            return this._loadPromise;
        }

        public static t(key: string, ...args: (string | number)[]): string {
            let value = this._dict[key] ?? key;
            args.forEach((arg, i) => {
                value = value.replace(`{${i}}`, String(arg));
            });
            return value;
        }

        public static isLoaded(): boolean {
            return this._initialized;
        }

        public static reset(): void {
            this._dict = {};
            this._initialized = false;
            this._loadPromise = null;
        }
    }
}
