namespace App {

    // ─── MODEL ───────────────────────────────────────────────────────────────

    interface FolderModel {
        id: string;
        name: string;
        description?: string;
        createdTime: string;
        photoCount: number;
    }

    interface FolderFileItem {
        fileId: string;
        fileName: string;
        fileUrl: string;
    }

    // ─── FOLDER LIST PAGE ────────────────────────────────────────────────────

    export class FolderPage extends BasePage {

        protected initialize(): void {
            this.loadFolders();
        }

        protected bindEvents(): void {
            this.root.find('#btn-create-folder').on('click', () => this.showCreateModal());
        }

        private async loadFolders(): Promise<void> {
            LoadingService.show();
            try {
                const res = await ApiService.get('/Folder/GetAll');
                if (!res.isOk()) {
                    ToastService.error(res.message || 'Không thể tải danh sách');
                    return;
                }

                const folders: FolderModel[] = res.data;
                const $grid = this.root.find('#folder-grid');
                const $empty = this.root.find('#folder-empty');
                $grid.empty();

                if (!folders || folders.length === 0) {
                    $empty.show();
                    return;
                }

                $empty.hide();
                folders.forEach(f => $grid.append(this.createFolderCard(f)));
            } catch {
                ToastService.error('Lỗi hệ thống');
            } finally {
                LoadingService.hide();
            }
        }

        private createFolderCard(folder: FolderModel): string {
            return `
                <div class="folder-card" data-id="${folder.id}" onclick="window.location.href='/Folder/Detail/${folder.id}'">
                    <div class="folder-card__actions">
                        <button class="folder-card__delete" data-id="${folder.id}" title="Xoá album"
                                onclick="event.stopPropagation(); App.FolderPage.deleteFolder('${folder.id}')">
                            <i class="fa-solid fa-trash-can"></i>
                        </button>
                    </div>
                    <div class="folder-card__icon">
                        <i class="fa-solid fa-folder"></i>
                    </div>
                    <div class="folder-card__name">${this.escapeHtml(folder.name)}</div>
                    <div class="folder-card__desc">${this.escapeHtml(folder.description || '')}</div>
                    <div class="folder-card__meta">
                        <span>${this.formatDate(folder.createdTime)}</span>
                        <span class="folder-card__count">${folder.photoCount} ảnh</span>
                    </div>
                </div>`;
        }

        private showCreateModal(): void {
            const popup = new PopupBuilder({
                title: 'Tạo Album Mới',
                size: 'sm',
                bodyHtml: `
                    <div class="create-folder-form">
                        <div class="mb-3">
                            <label class="form-label">Tên album <span class="text-danger">*</span></label>
                            <input type="text" class="form-control" id="folder-name" placeholder="VD: Kỷ niệm 2024" maxlength="200" />
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Mô tả</label>
                            <textarea class="form-control" id="folder-desc" rows="2" placeholder="Mô tả ngắn..." maxlength="500"></textarea>
                        </div>
                    </div>`,
                footerHtml: `
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Huỷ</button>
                    <button type="button" class="btn btn-primary" id="btn-submit-folder">Tạo</button>`,
                onShow: ($body) => {
                    const $modal = $body.closest('.modal');
                    $modal.find('#btn-submit-folder').on('click', async () => {
                        const name = ($body.find('#folder-name').val() as string)?.trim();
                        const desc = ($body.find('#folder-desc').val() as string)?.trim();

                        if (!name) {
                            ToastService.error('Vui lòng nhập tên album');
                            return;
                        }

                        const res = await ApiService.post('/Folder/Create', { name, description: desc || null });
                        if (res.isOk()) {
                            ToastService.success('Tạo album thành công!');
                            popup.hide();
                            this.loadFolders();
                        } else {
                            ToastService.error(res.message || 'Không thể tạo album');
                        }
                    });
                }
            });
            popup.show();
        }

        public static async deleteFolder(folderId: string): Promise<void> {
            const confirmed = await MessageService.confirm('Xác nhận xoá album này? Thao tác này không thể hoàn tác.');
            if (!confirmed) return;

            LoadingService.show();
            try {
                const res = await ApiService.delete('/Folder/Delete/' + folderId);
                if (res.isOk()) {
                    ToastService.success('Đã xoá album');
                    // Reload folder grid
                    $(`.folder-card[data-id="${folderId}"]`).fadeOut(300, function () {
                        $(this).remove();
                        if ($('#folder-grid').children().length === 0) {
                            $('#folder-empty').show();
                        }
                    });
                } else {
                    ToastService.error(res.message || 'Không thể xoá album');
                }
            } finally {
                LoadingService.hide();
            }
        }

        private escapeHtml(text: string): string {
            const div = document.createElement('div');
            div.textContent = text;
            return div.innerHTML;
        }

        private formatDate(dateStr: string): string {
            if (!dateStr) return '';
            const d = new Date(dateStr);
            return d.toLocaleDateString('vi-VN');
        }
    }

    // ─── FOLDER DETAIL PAGE ──────────────────────────────────────────────────

    export class FolderDetailPage extends BasePage<{ folderId: string }> {

        private gallery: GalleryBuilder;
        private isLoadingGallery = false;
        private fileItems: FolderFileItem[] = [];

        protected initialize(): void {
            // Init uploader
            new FileUploaderBuilder('#folder-uploader-container', {
                url: `/Folder/Upload/${this.model.folderId}`,
                title: 'Upload Ảnh',
                instructionText: 'Kéo ảnh vào đây hoặc',
                allowedExtensions: ['jpg', 'jpeg', 'png', 'gif', 'webp', 'bmp'],
                maxFileSizeMb: 50
            })
                .onSuccess((_file, _res) => {
                    // Reload gallery khi upload thành công
                    this.loadGallery();
                })
                .build();

            // Init gallery with delete enabled
            this.gallery = new GalleryBuilder({
                container: '#folder-gallery-container',
                columns: 4,
                gap: 10,
                borderRadius: 8,
                items: [],
                enableDelete: true,
                onDelete: (fileId, index) => this.deleteImage(fileId, index)
            });
            this.gallery.build();

            this.loadGallery();
        }

        private async loadGallery(): Promise<void> {
            if (this.isLoadingGallery) return;
            this.isLoadingGallery = true;
            try {
                const res = await ApiService.get('/Folder/Files', { id: this.model.folderId });
                if (!res.isOk()) {
                    ToastService.error(res.message || 'Không thể tải ảnh');
                    return;
                }

                this.fileItems = (res.data as FolderFileItem[]) || [];

                // Map file list directly to gallery items.
                // GalleryBuilder's IntersectionObserver lazy-loads each image and
                // automatically patches data-pswp-width/height with real naturalWidth/Height
                // once the thumbnail actually loads – no upfront dimension probing needed.
                const items: IGalleryItem[] = this.fileItems.map(f => ({
                    fileId: f.fileId,
                    src: f.fileUrl,
                    width: 800,
                    height: 600,
                    alt: f.fileName,
                    title: f.fileName
                }));

                this.gallery.setItems(items);
            } catch {
                ToastService.error('Không thể tải ảnh');
            } finally {
                this.isLoadingGallery = false;
            }
        }

        private async deleteImage(fileId: string, index: number): Promise<void> {
            const confirmed = await MessageService.confirm('Xoá ảnh này khỏi album?');
            if (!confirmed) return;

            LoadingService.show();
            try {
                const res = await ApiService.delete(
                    `/Folder/RemoveFile?folderId=${encodeURIComponent(this.model.folderId)}&fileId=${encodeURIComponent(fileId)}`
                );

                if (res.isOk()) {
                    ToastService.success('Đã xoá ảnh');
                    this.fileItems = this.fileItems.filter(f => f.fileId !== fileId);
                    this.gallery.removeItem(index);
                } else {
                    ToastService.error(res.message || 'Không thể xoá ảnh');
                }
            } finally {
                LoadingService.hide();
            }
        }
    }
}
