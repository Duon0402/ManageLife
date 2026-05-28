namespace App {
    export interface StudyCardModel {
        wordId: string;
        word: string;
        phonetic?: string;
        partOfSpeech?: string;
        definition?: string;
        exampleSentence?: string;
        translation?: string;
        audioUrl?: string;
        repetitions: number;
        intervalDays: number;
        masteryLevel: number;
        isNew: boolean;
    }
}
