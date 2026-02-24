namespace App {
    export interface IFileUploaderOptions {
        url: string;
        maxFiles?: number;
        allowedExtensions?: string[];
        maxFileSizeMb?: number;
        title?: string;
        instructionText?: string;
    }

    export class FileUploaderBuilder {
        private selector: string;
        private options: IFileUploaderOptions;
        private $container: JQuery;
        private $uploaderHtml: JQuery;
        private $fileList: JQuery;
        private $fileBrowseInput: JQuery;
        private $fileUploadBox: JQuery;
        private uploadedCount: number = 0;
        private totalCount: number = 0;

        private callbacks = {
            onUploadSuccess: (file: File, response: any) => { },
            onUploadError: (file: File, error: any) => { }
        };

        constructor(selector: string, options: IFileUploaderOptions) {
            this.selector = selector;
            this.options = {
                title: 'File Uploader',
                instructionText: 'Drag files here or',
                ...options
            };
        }

        public onSuccess(callback: (file: File, res: any) => void): this {
            this.callbacks.onUploadSuccess = callback;
            return this;
        }

        public onError(callback: (file: File, err: any) => void): this {
            this.callbacks.onUploadError = callback;
            return this;
        }

        public build(): void {
            this.$container = $(this.selector);
            if (this.$container.length === 0) {
                console.error(`FileUploaderBuilder: Selector '${this.selector}' not found.`);
                return;
            }

            this.renderHtml();
            this.bindEvents();
        }

        private renderHtml(): void {
            this.$container.empty();

            const html = `
                <div class="file-uploader">
                    <div class="uploader-header">
                        <h2 class="uploader-title">${this.options.title}</h2>
                        <h4 class="file-completed-status"> <span class="uploaded-count">0</span> / <span class="total-count">0</span> Files Completed </h4>
                    </div>
                    <ul class="file-list"></ul>
                    <div class="file-upload-box">
                        <h2 class="box-title">
                            <span class="file-instruction">${this.options.instructionText}</span>
                            <span class="file-browse-button">browse</span>
                        </h2>
                        <input class="file-browse-input" type="file" multiple hidden>
                    </div>
                </div>`;

            this.$uploaderHtml = $(html);
            this.$fileList = this.$uploaderHtml.find('.file-list');
            this.$fileBrowseInput = this.$uploaderHtml.find('.file-browse-input');
            this.$fileUploadBox = this.$uploaderHtml.find('.file-upload-box');

            this.$container.append(this.$uploaderHtml);
        }

        private bindEvents(): void {
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
                this.$fileUploadBox.find('.file-instruction').text(this.options.instructionText!);
            });

            this.$fileBrowseInput.on('change', (e) => {
                const files = (e.target as HTMLInputElement).files;
                if (files) this.handleSelectedFiles(Array.from(files));
                // reset input to allow picking same file again
                (e.target as HTMLInputElement).value = '';
            });

            this.$uploaderHtml.find('.file-browse-button').on('click', () => this.$fileBrowseInput.click());
        }

        private updateStatusCount(): void {
            this.$uploaderHtml.find('.uploaded-count').text(this.uploadedCount);
            this.$uploaderHtml.find('.total-count').text(this.totalCount);
        }

        private handleSelectedFiles(files: File[]): void {
            if (files.length === 0) return;

            const validFiles = this.validateFiles(files);
            if (validFiles.length === 0) return;

            this.totalCount += validFiles.length;
            this.updateStatusCount();

            validFiles.forEach((file) => {
                const $itemHtml = $(this.createFileItemHtml(file));
                this.$fileList.append($itemHtml);

                // Find element refs inside item
                const $progress = $itemHtml.find('.file-progress');
                const $status = $itemHtml.find('.file-status');
                const $cancelBtn = $itemHtml.find('.cancel-button');

                // Cancel handling
                $cancelBtn.on('click', () => {
                    // Logic to abort request could go here if ApiService supported AbortController 
                    // for now just remove visually.
                    if ($status.text() !== 'Completed') {
                        this.totalCount--;
                        this.updateStatusCount();
                    }
                    $itemHtml.remove();
                });

                const formData = new FormData();
                formData.append('file', file);

                ApiService.upload(this.options.url, formData, {
                    showLoading: false,
                    showToast: false,
                    onProgress: (percent) => {
                        $progress.css('width', percent + '%');
                        $status.text(`Uploading... ${percent}%`);
                    }
                }).then(res => {
                    if (res.isOk()) {
                        $status.text('Completed');
                        $progress.css('width', '100%');
                        this.uploadedCount++;
                        this.updateStatusCount();
                        this.callbacks.onUploadSuccess(file, res);
                    } else {
                        $status.text('Failed');
                        $progress.css('background', 'red');
                        this.callbacks.onUploadError(file, res.message);
                        ToastService.error(res.message);
                    }
                }).catch((err) => {
                    $status.text('Failed');
                    $progress.css('background', 'red');
                    this.callbacks.onUploadError(file, err);
                });
            });
        }

        private validateFiles(files: File[]): File[] {
            let validFiles: File[] = [];

            // Example validation stub
            for (const file of files) {
                if (this.options.maxFileSizeMb && (file.size / (1024 * 1024)) > this.options.maxFileSizeMb) {
                    ToastService.error(`File ${file.name} is larger than allowed limit of ${this.options.maxFileSizeMb}MB.`);
                    continue;
                }

                if (this.options.allowedExtensions && this.options.allowedExtensions.length > 0) {
                    const ext = file.name.split('.').pop()?.toLowerCase() || '';
                    if (this.options.allowedExtensions.indexOf(ext) === -1 && this.options.allowedExtensions.indexOf('.' + ext) === -1) {
                        ToastService.error(`File extension ${ext} is not allowed.`);
                        continue;
                    }
                }

                validFiles.push(file);
            }

            return validFiles;
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
                      <button class="cancel-button" title="Remove">
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
