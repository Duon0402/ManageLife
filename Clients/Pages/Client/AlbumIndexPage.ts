namespace App {
    export class AlbumIndexPage extends BasePage {
        private albumContainer: JQuery<HTMLElement>;

        constructor(containerSelector: string) {
            super(containerSelector);
            this.albumContainer = $(containerSelector);
            this.init();
        }

        private init() {
            this.renderLayout();
            this.loadAlbums();
            this.attachEvents();
        }

        private renderLayout() {
            const html = `
                <div class="album-header">
                    <h2>My Photo Albums</h2>
                    <button class="btn-create-album" id="btnCreateAlbum">
                        <i class="fas fa-plus mr-2"></i> Create New Album
                    </button>
                </div>
                <div class="album-grid" id="albumGrid">
                    <!-- Loading state or albums will be appended here -->
                </div>
            `;
            this.albumContainer.html(html);
        }

        private attachEvents() {
            this.albumContainer.on('click', '#btnCreateAlbum', () => this.showCreateAlbumPopup());
        }

        private showCreateAlbumPopup() {
            const formHtml = `
                <form id="createAlbumForm">
                    <div class="mb-3">
                        <label for="albumTitle" class="form-label">Album Title <span class="text-danger">*</span></label>
                        <input type="text" class="form-control" id="albumTitle" name="title" required placeholder="e.g. Summer Vacation 2024">
                    </div>
                    <div class="mb-3">
                        <label for="albumDesc" class="form-label">Description (Optional)</label>
                        <textarea class="form-control" id="albumDesc" name="description" rows="3" placeholder="A few words about this album..."></textarea>
                    </div>
                </form>
            `;

            const footerHtml = `
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                <button type="button" class="btn btn-primary" id="btnSaveAlbum">Create</button>
            `;

            const popup = new PopupBuilder({
                title: 'Create New Album',
                bodyHtml: formHtml,
                footerHtml: footerHtml,
                onShow: ($body) => {
                    const $form = $body.find('#createAlbumForm');
                    const $btnSave = $body.closest('.modal-content').find('#btnSaveAlbum');

                    $btnSave.on('click', async () => {
                        if (($form[0] as HTMLFormElement).checkValidity()) {
                            const title = $form.find('#albumTitle').val() as string;
                            const desc = $form.find('#albumDesc').val() as string;

                            $btnSave.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Creating...');

                            await this.createAlbum(title, desc);
                            popup.hide();
                            this.loadAlbums(); // refresh list
                        } else {
                            ($form[0] as HTMLFormElement).reportValidity();
                        }
                    });
                }
            }).show();
        }

        private async createAlbum(title: string, description: string) {
            try {
                const formData = new FormData();
                formData.append('title', title);
                if (description) formData.append('description', description);

                const response = await ApiService.post<any>('/Album/Create', formData, {
                    headers: {} // Form data doesn't need Content-Type, browser sets it
                });

                if (response.isOk()) {
                    MessageService.success('Album created successfully!');
                } else {
                    MessageService.error(response.message || 'Failed to create album');
                }
            } catch (error) {
                console.error(error);
                MessageService.error('An unexpected error occurred.');
            }
        }

        private async loadAlbums() {
            const $grid = this.albumContainer.find('#albumGrid');
            $grid.html('<div class="col-12 text-center text-muted py-5"><i class="fas fa-spinner fa-spin fa-2x"></i><p class="mt-2">Loading albums...</p></div>');

            try {
                const response = await ApiService.get<any[]>('/Album/GetAll');
                $grid.empty();

                if (response.isOk() && response.data) {
                    const albums = response.data;
                    if (albums.length === 0) {
                        $grid.html(`
                            <div class="col-12" style="grid-column: 1 / -1;">
                                <div class="empty-state">
                                    <i class="fas fa-images"></i>
                                    <h4>No albums yet</h4>
                                    <p class="text-muted">Create your first photo album to get started.</p>
                                </div>
                            </div>
                        `);
                    } else {
                        albums.forEach(album => {
                            $grid.append(this.generateAlbumCardHtml(album));
                        });
                    }
                } else {
                    MessageService.error('Failed to load albums');
                }
            } catch (error) {
                console.error(error);
                MessageService.error('An unexpected error occurred while loading albums.');
            }
        }

        private generateAlbumCardHtml(album: any): string {
            const date = new Date(album.createdTime).toLocaleDateString();

            // Note: cover photo implementation will depend on how we serve photos
            // For now, if no cover, show empty state
            let coverHtml = '';
            if (album.coverPhotoId) {
                // Assuming we have an endpoint like /FileStorage/GetFileUrl?fileId=...
                // Or maybe we directly load the image by file ID.
                // We'll use a placeholder URL for now and fix when uploading is done
                coverHtml = `<div class="album-cover" style="background-image: url('/FileStorage/GetFileUrl?fileId=${album.coverPhotoId}')"></div>`;
            } else {
                coverHtml = `<div class="album-cover empty"><i class="fas fa-photo-video"></i></div>`;
            }

            return `
                <a href="/Album/Details/${album.id}" class="album-card">
                    ${coverHtml}
                    <div class="album-info">
                        <div class="album-title" title="${album.title}">${album.title}</div>
                        <div class="album-meta">
                            <i class="far fa-calendar-alt"></i> ${date}
                        </div>
                    </div>
                </a>
            `;
        }
    }
}
