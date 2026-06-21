namespace App {
    export interface NoteTagModel { id: string; name: string; color: string; }
    export interface NoteModel {
        id: string; title: string; content: string | null;
        tags: NoteTagModel[]; createdTime: string; updatedTime: string | null;
    }
    export interface NoteDetailModel extends NoteModel {
        linkedNotes: NoteModel[];
        backlinkNotes: NoteModel[];
    }
}
