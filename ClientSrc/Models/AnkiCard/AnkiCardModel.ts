namespace App {
    /**
     * 0 = Basic ("Basic")
     * 1 = BasicReversed ("Basic (and reversed card)")
     * 2 = BasicOptionalReversed ("Basic (optional reversed card)")
     * 3 = BasicTypeAnswer ("Basic (type in the answer)")
     * 4 = Cloze ("Cloze")
     */
    export const enum AnkiCardType {
        Basic = 0,
        BasicReversed = 1,
        BasicOptionalReversed = 2,
        BasicTypeAnswer = 3,
        Cloze = 4
    }

    export const AnkiCardTypeLabel: Record<number, string> = {
        0: 'Hỏi-Đáp',
        1: 'Hỏi-Đáp 2 chiều',
        2: 'Hỏi-Đáp 2 chiều tuỳ chọn',
        3: 'Gõ đáp án',
        4: 'Điền khuyết'
    };

    export interface AnkiCardModel {
        id: string;
        cardType: AnkiCardType;
        fieldFront: string;
        fieldBack: string;
        fieldExtra?: string | null;
        sourceNote?: string | null;
        recordedDate: string;
    }

    export interface CreateAnkiCardRequest {
        cardType: AnkiCardType;
        fieldFront: string;
        fieldBack: string;
        fieldExtra?: string | null;
        sourceNote?: string | null;
    }

    export interface UpdateAnkiCardRequest {
        id: string;
        cardType: AnkiCardType;
        fieldFront: string;
        fieldBack: string;
        fieldExtra?: string | null;
        sourceNote?: string | null;
    }
}
