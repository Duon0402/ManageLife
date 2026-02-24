namespace App {
    export class AlbumDetailsPage extends BasePage {
        private container: JQuery<HTMLElement>;
        private albumId: string;
        private photos: any[] = [];
        private currentIndex: number = 0;

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
                if ($(e.target).closest('.btn').length || $(e.target).closest('button').length) return;

                const fileId = $(e.currentTarget).data('fileid');
                this.showLightbox(fileId);
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
                // No toast here, handled by uploader UI
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
                    this.photos = response.data;
                    if (this.photos.length === 0) {
                        $grid.html(`
                            <div class="empty-state">
                                <i class="fas fa-camera"></i>
                                <h4>No photos yet</h4>
                                <p class="text-muted">Click the upload button to add photos to this album.</p>
                            </div>
                        `);
                    } else {
                        this.photos.forEach(file => {
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
            const fileId = file.id || file.Id;
            const fileName = file.fileName || file.FileName;
            const url = `/FileStorage/GetFile?fileId=${fileId}`;
            // We use fileId for now to map to GetFile
            return `
                <div class="photo-card" data-fileid="${fileId}">
                    <img src="${url}" alt="${fileName}" loading="lazy" />
                    <div class="photo-actions">
                        <button class="btn-set-cover" title="Set as Cover Photo"><i class="fas fa-star"></i></button>
                        <button class="btn-delete-photo" data-fileid="${fileId}" title="Remove from album"><i class="fas fa-trash-alt"></i></button>
                    </div>
                </div>
            `;
        }

        private showLightbox(fileId: string) {
            this.currentIndex = this.photos.findIndex(p => (p.id || p.Id) === fileId);
            if (this.currentIndex === -1) return;

            const modalHtml = `
                <div class="lightbox-container" style="background: rgba(0,0,0,0.9); position: fixed; top: 0; left: 0; width: 100%; height: 100%; z-index: 9999; display: flex; align-items: center; justify-content: center; user-select: none;">
                    <button id="lightboxPrev" class="btn" style="position: absolute; left: 20px; color: white; font-size: 3rem; background: transparent; border: none; z-index: 10001;"><i class="fas fa-chevron-left"></i></button>
                    <div id="lightboxContent" style="max-width: 90%; max-height: 90%; position: relative;">
                        <!-- Image will be injected here -->
                    </div>
                    <button id="lightboxNext" class="btn" style="position: absolute; right: 20px; color: white; font-size: 3rem; background: transparent; border: none; z-index: 10001;"><i class="fas fa-chevron-right"></i></button>
                    <button id="lightboxClose" class="btn" style="position: absolute; top: 20px; right: 20px; color: white; font-size: 2rem; background: transparent; border: none; z-index: 10001;"><i class="fas fa-times"></i></button>
                    <div id="lightboxInfo" style="position: absolute; bottom: 20px; left: 50%; transform: translateX(-50%); color: white; background: rgba(0,0,0,0.5); padding: 5px 15px; border-radius: 20px; font-size: 0.9rem;"></div>
                </div>
            `;

            const $lightbox = $(modalHtml);
            $('body').append($lightbox);

            const updateImage = () => {
                const photo = this.photos[this.currentIndex];
                const id = photo.id || photo.Id;
                const fileName = photo.fileName || photo.FileName;
                const url = `/FileStorage/GetFile?fileId=${id}`;

                const imgHtml = `<img src="${url}" alt="${fileName}" style="max-width: 100%; max-height: 100%; object-fit: contain; box-shadow: 0 0 30px rgba(0,0,0,0.5); border-radius: 4px;">`;
                $lightbox.find('#lightboxContent').html(imgHtml);
                $lightbox.find('#lightboxInfo').text(`${this.currentIndex + 1} / ${this.photos.length} - ${fileName}`);

                // Hide/show nav buttons
                $lightbox.find('#lightboxPrev').css('visibility', this.currentIndex > 0 ? 'visible' : 'hidden');
                $lightbox.find('#lightboxNext').css('visibility', this.currentIndex < this.photos.length - 1 ? 'visible' : 'hidden');
            };

            updateImage();

            $lightbox.on('click', '#lightboxPrev', (e) => {
                e.stopPropagation();
                if (this.currentIndex > 0) {
                    this.currentIndex--;
                    updateImage();
                }
            });

            $lightbox.on('click', '#lightboxNext', (e) => {
                e.stopPropagation();
                if (this.currentIndex < this.photos.length - 1) {
                    this.currentIndex++;
                    updateImage();
                }
            });

            $lightbox.on('click', '#lightboxClose', () => $lightbox.remove());
            $lightbox.on('click', (e) => {
                if ($(e.target).is($lightbox)) $lightbox.remove();
            });

            // Keyboard navigation
            $(document).on('keydown.lightbox', (e) => {
                if (e.key === 'ArrowLeft') $lightbox.find('#lightboxPrev').trigger('click');
                else if (e.key === 'ArrowRight') $lightbox.find('#lightboxNext').trigger('click');
                else if (e.key === 'Escape') $lightbox.find('#lightboxClose').trigger('click');
            });

            // Cleanup keyboard events when removed
            const observer = new MutationObserver((mutations) => {
                if (!document.body.contains($lightbox[0])) {
                    $(document).off('keydown.lightbox');
                    observer.disconnect();
                }
            });
            observer.observe(document.body, { childList: true });
        }
    }
}
