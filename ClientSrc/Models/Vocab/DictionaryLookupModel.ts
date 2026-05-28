namespace App {
    export interface DictionaryMeaningResult {
        partOfSpeech: string;
        definition: string;
        exampleSentence?: string;
    }

    export interface DictionaryLookupResult {
        word: string;
        phonetic?: string;
        audioUrl?: string;
        rawJson?: string;
        meanings: DictionaryMeaningResult[];
    }
}
