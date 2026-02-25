namespace App {
    export class TranslationService {
        private static dictionary: { [key: string]: string } = {};
        private static isInitialized: boolean = false;

        public static async initialize(): Promise<void> {
            if (this.isInitialized) return;

            try {
                const rs = await ApiService.get('/Language/GetTranslations');
                if (rs.isOk() && rs.data) {
                    this.dictionary = rs.data;
                    this.isInitialized = true;
                }
            } catch (error) {
                console.error('Failed to initialize translations', error);
            }
        }

        public static t(key: string, ...args: any[]): string {
            let value = this.dictionary[key] || key;
            if (args && args.length > 0) {
                for (let i = 0; i < args.length; i++) {
                    value = value.replace(`{${i}}`, args[i]);
                }
            }
            return value;
        }
    }

    export const t = (key: string, ...args: any[]) => TranslationService.t(key, ...args);
}
