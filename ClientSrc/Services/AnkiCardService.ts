namespace App {
    /**
     * Wraps all HTTP calls to the /AnkiCard/... endpoints so that pages never talk to
     * ApiService or raw endpoint strings directly.
     */
    export class AnkiCardService {
        private static readonly BASE_URL = '/AnkiCard';

        public static readonly EXPORT_ANKI_URL = `${AnkiCardService.BASE_URL}/ExportAnki`;
        public static readonly EXPORT_ANKI_TEXT_URL = `${AnkiCardService.BASE_URL}/ExportAnkiText`;

        public static getList(): Promise<ApiResponse<AnkiCardModel[]>> {
            return ApiService.get<AnkiCardModel[]>(`${this.BASE_URL}/GetList`);
        }

        public static create(request: CreateAnkiCardRequest): Promise<ApiResponse> {
            return ApiService.post(`${this.BASE_URL}/Create`, request);
        }

        public static update(request: UpdateAnkiCardRequest): Promise<ApiResponse> {
            return ApiService.put(`${this.BASE_URL}/Update`, request);
        }

        public static delete(id: string): Promise<ApiResponse> {
            return ApiService.delete(`${this.BASE_URL}/Delete?id=${encodeURIComponent(id)}`);
        }

        public static exportAnki(): void {
            window.location.href = this.EXPORT_ANKI_URL;
        }

        public static exportAnkiText(): void {
            window.location.href = this.EXPORT_ANKI_TEXT_URL;
        }
    }
}
