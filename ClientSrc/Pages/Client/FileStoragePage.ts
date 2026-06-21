namespace App {
    export class FileStoragePage extends BasePage {
        protected initialize(): void {
            // Replaced by FileUploaderBuilder in bindEvents
        }

        protected bindEvents(): void {
            new FileUploaderBuilder("#file-storage-page-container", {
                url: '/filestorage/upload',
                title: 'Fast File Uploader',
                instructionText: 'Drop large files here or'
            })
                .onSuccess((_file, _res) => {
                })
                .onError((file, err) => {
                    console.error(`Error uploading: ${file.name}`, err);
                })
                .build();
        }
    }
}
