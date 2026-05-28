namespace App {
    export interface VocabWordModel {
        id: string;
        word: string;
        phonetic?: string;
        partOfSpeech?: string;
        definition?: string;
        exampleSentence?: string;
        translation?: string;
        audioUrl?: string;
        imageUrl?: string;
        dictionarySource: number;
        masteryLevel: number;
        nextReviewDate?: string;
        createdTime: string;
    }

    export const VocabMasteryLabel: Record<number, string> = {
        0: 'Mới',
        1: 'Đang học',
        2: 'Ôn tập',
        3: 'Thuộc'
    };
}
