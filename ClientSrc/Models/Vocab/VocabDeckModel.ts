namespace App {
    export interface VocabDeckModel {
        id: string;
        name: string;
        description?: string;
        topicId?: string;
        topicName?: string;
        topicColor?: string;
        totalCards: number;
        createdTime: string;
    }
}
