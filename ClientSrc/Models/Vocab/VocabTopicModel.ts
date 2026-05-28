namespace App {
    export interface VocabTopicModel {
        id: string;
        name: string;
        description?: string;
        color?: string;
        icon?: string;
        isPublic: boolean;
        deckCount: number;
    }
}
