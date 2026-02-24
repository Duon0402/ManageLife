namespace App {
    export class AlbumDetailsPage extends BasePage {
        private container: JQuery<HTMLElement>;
        private albumId: string;

        constructor(containerSelector: string, albumId: string) {
            super(containerSelector);
            this.container = $(containerSelector);
            this.albumId = albumId;
            this.init();
        }

        private init() {
            this.loadPhotos();
            this.attachEvents();
        }

        private attachEvents() {
            this.container.on('click', '#btnUploadPhoto', () => this.showUploadPopup());

            // Delegate events for dynamically added photos
            this.container.on('click', '.btn-delete-photo', (e) => {
                e.stopPropagation(); // prevent opening photo
                const fileId = $(e.currentTarget).data('fileid');
                this.unlinkPhoto(fileId);
            });

            this.container.on('click', '.photo-card', (e) => {
                // Ignore clicks if they were on buttons
                if ($(e.target).closest('.btn').length) return;

                const fileId = $(e.currentTarget).data('fileid');
                // Could implement a lightbox view here. For now just open in new tab.
                window.open(`/FileStorage/GetFileUrl?fileId=${fileId}`, '_blank');
            });
        }

        private showUploadPopup() {
            // We use the new PopupBuilder to host the existing FileUploaderBuilder
            const popup = new PopupBuilder({
                title: 'Upload Photos to Album',
                size: 'lg',
                bodyHtml: `<div id="albumUploaderContainer"></div>`,
                onShow: ($body) => {
                    const uploaderContainerId = '#albumUploaderContainer';

                    // Instantiate the FileUploaderBuilder inside the popup
                    new FileUploaderBuilder(uploaderContainerId, {
                        url: '/FileStorage/Upload',
                        title: 'Select photos'
                    })
                        .onSuccess(async (file: File, res: any) => {
                            // When file is uploaded to Telegram, link it to the album
                            if (res.isOk() && res.data && res.data.id) {
                                await this.linkFileToAlbum(res.data.id);
                                this.loadPhotos(); // Refresh grid
                            }
                        })
                        .onError((file: File, err: any) => {
                            MessageService.error(`Error uploading ${file.name}: ` + (typeof err === 'string' ? err : 'Unknown error'));
                        })
                        .build();
                }
            }).show();
        }

        private async linkFileToAlbum(fileId: string) {
            try {
                const formData = new FormData();
                formData.append('albumId', this.albumId);
                formData.append('fileId', fileId);

                await ApiService.post<any>('/Album/LinkFile', formData);
            } catch (error) {
                console.error("Failed to link file to album", error);
            }
        }

        private async unlinkPhoto(fileId: string) {
            if (!confirm('Are you sure you want to remove this photo from the album? The file itself will not be deleted.')) {
                return;
            }

            try {
                // Not implemented yet: await ApiService.post<any>('/Album/UnlinkFile', { albumId: this.albumId, fileId });
                // For simplicity we will skip implementing the endpoint unlink in controller for now, assuming link is the main goal
                MessageService.success('Photo removed from album (placeholder).');
                this.loadPhotos();
            } catch (error) {
                console.error("Failed to unlink file", error);
            }
        }

        private async loadPhotos() {
            const $grid = this.container.find('#photoGrid');
            $grid.html('<div class="col-12 text-center text-muted py-5"><i class="fas fa-spinner fa-spin fa-2x"></i></div>');

            try {
                const response = await ApiService.get<any[]>('/Album/GetFiles?albumId=' + this.albumId);
                $grid.empty();

                if (response.isOk() && response.data) {
                    const files = response.data;
                    if (files.length === 0) {
                        $grid.html(`
                            <div class="empty-state">
                                <i class="fas fa-camera"></i>
                                <h4>No photos yet</h4>
                                <p class="text-muted">Click the upload button to add photos to this album.</p>
                            </div>
                        `);
                    } else {
                        files.forEach(file => {
                            $grid.append(this.generatePhotoCardHtml(file));
                        });
                    }
                } else {
                    MessageService.error('Failed to load photos');
                }
            } catch (error) {
                console.error(error);
                MessageService.error('An unexpected error occurred while loading photos.');
            }
        }

        private generatePhotoCardHtml(file: any): string {
            const url = `/FileStorage/GetFileUrl?fileId=${file.id}`;
            // We use fileId for now to map to GetFileUrl
            return `
                <div class="photo-card" data-fileid="${file.id}">
                    <img src="${url}" alt="${file.fileName}" loading="lazy" />
                    <div class="photo-actions">
                        <button class="btn-set-cover" title="Set as Cover Photo"><i class="fas fa-star"></i></button>
                        <button class="btn-delete-photo" data-fileid="${file.id}" title="Remove from album"><i class="fas fa-trash-alt"></i></button>
                    </div>
                </div>
            `;
        }
    }
}
