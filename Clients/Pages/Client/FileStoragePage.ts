namespace App {
    export class FileStoragePage extends BasePage {
        private $fileList: JQuery;
        private $fileBrowseInput: JQuery;
        private $fileUploadBox: JQuery;

        protected initialize(): void {
            this.$fileList = this.root.find('.file-list');
            this.$fileBrowseInput = this.root.find('.file-browse-input');
            this.$fileUploadBox = this.root.find('.file-upload-box');
        }

        protected bindEvents(): void {
            this.$fileUploadBox.on("drop", (e) => {
                e.preventDefault();
                const files = (e.originalEvent as DragEvent).dataTransfer?.files;
                if (files) this.handleSelectedFiles(Array.from(files));
                this.$fileUploadBox.addClass('active');
                this.$fileUploadBox.find('.file-instruction').text('Release to upload or');
            });

            this.$fileUploadBox.on("dragover", (e) => {
                e.preventDefault();
                this.$fileUploadBox.addClass('active');
                this.$fileUploadBox.find('.file-instruction').text('Release to upload or');
            });

            this.$fileUploadBox.on("dragleave", (e) => {
                e.preventDefault();
                this.$fileUploadBox.removeClass('active');
                this.$fileUploadBox.find('.file-instruction').text('Drag files here or');
            });

            this.$fileBrowseInput.on('change', (e) => {
                const files = (e.target as HTMLInputElement).files;
                if (files) this.handleSelectedFiles(Array.from(files));
            });

            this.root.find('.file-browse-button').on('click', () => this.$fileBrowseInput.click());
        }

        private handleSelectedFiles(files: File[]): void {
            if (files.length === 0) return;

            files.forEach((file) => {
                const $item = $(this.createFileItemHtml(file));
                this.$fileList.append($item);

                const formData = new FormData();
                formData.append('file', file);

                ApiService.upload('/filestorage/upload', formData, {
                    showLoading: false,
                    showToast: false,
                    onProgress: (percent) => {
                        $item.find('.file-progress').css('width', percent + '%');
                        $item.find('.file-status').text(`Uploading... ${percent}%`);
                    }
                }).then(res => {
                    if (res.isOk()) {
                        $item.find('.file-status').text('Completed');
                        $item.find('.file-progress').css('width', '100%');
                    } else {
                        $item.find('.file-status').text('Failed');
                        $item.find('.file-progress').css('background', 'red');
                    }
                }).catch(() => {
                    $item.find('.file-status').text('Failed');
                    $item.find('.file-progress').css('background', 'red');
                });
            });
        }

        private createFileItemHtml(file: File): string {
            const { name, size } = file;
            const extension = name.split(".").pop();
            const sizeInMb = (size / (1024 * 1024)).toFixed(2);

            return `
                <li class="file-item">
                  <div class="file-extension">${extension}</div>
                  <div class="file-content-wrapper">
                    <div class="file-content">
                      <div class="file-details">
                        <h5 class="file-name">${name}</h5>
                        <div class="file-info">
                          <small class="file-size">${sizeInMb} MB</small>
                          <small class="file-divider">-</small>
                          <small class="file-status">Uploading...</small>
                        </div>
                      </div>
                      <button class="cancel-button">
                        <i class="fa-solid fa-xmark"></i>
                      </button>
                    </div>
                    <div class="file-progress-bar">
                      <div class="file-progress">
                      </div>
                    </div>
                  </div>
                </li>
            `;
        }
    }
}
