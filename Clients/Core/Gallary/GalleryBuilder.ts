namespace App {

    // ─── Interfaces ────────────────────────────────────────────────────────────

    export interface IGalleryItem {
        /** URL to the full-size image (shown in lightbox) */
        src: string;
        /** URL to the thumbnail (shown in grid). Falls back to `src` if omitted. */
        thumbnail?: string;
        /** Image width in pixels (required by PhotoSwipe for best layout) */
        width: number;
        /** Image height in pixels (required by PhotoSwipe for best layout) */
        height: number;
        /** Optional caption / alt text */
        alt?: string;
        /** Optional title shown below image in lightbox */
        title?: string;
        /** Optional file ID used for delete operations */
        fileId?: string;
    }

    export interface IGalleryOptions {
        /** CSS selector or HTMLElement of the container to render the gallery into */
        container: string | HTMLElement;
        /** Initial set of images. More can be added later with addItems() */
        items?: IGalleryItem[];
        /** Number of columns in the grid (default: 3) */
        columns?: number;
        /** Gap between thumbnails in px (default: 8) */
        gap?: number;
        /** Border-radius applied to thumbnails in px (default: 6) */
        borderRadius?: number;
        /** Show a small overlay icon on hover (default: true) */
        showZoomIcon?: boolean;
        /** Show a delete button on hover (default: false) */
        enableDelete?: boolean;
        /** Callback when user clicks delete on a tile */
        onDelete?: (fileId: string, index: number) => void;
        /** Extra PhotoSwipe options forwarded to PhotoSwipeLightbox */
        photoswipeOptions?: Record<string, any>;
    }

    // ─── GalleryBuilder ─────────────────────────────────────────────────────────

    export class GalleryBuilder {

        private options: Required<Omit<IGalleryOptions, 'items' | 'photoswipeOptions' | 'onDelete'>> & Pick<IGalleryOptions, 'photoswipeOptions' | 'onDelete'>;
        private items: IGalleryItem[];
        private $container: JQuery<HTMLElement>;
        private $grid: JQuery<HTMLElement>;
        private lightbox: any; // PhotoSwipeLightbox (loaded from UMD)

        constructor(options: IGalleryOptions) {
            this.options = {
                container: options.container,
                columns: options.columns ?? 3,
                gap: options.gap ?? 8,
                borderRadius: options.borderRadius ?? 6,
                showZoomIcon: options.showZoomIcon ?? true,
                enableDelete: options.enableDelete ?? false,
                onDelete: options.onDelete,
                photoswipeOptions: options.photoswipeOptions
            };
            this.items = [...(options.items ?? [])];
        }

        // ─── Public API ──────────────────────────────────────────────────────────

        /**
         * Append one or more items to the gallery.
         * Automatically re-renders if already built.
         */
        public addItems(items: IGalleryItem | IGalleryItem[]): this {
            const toAdd = Array.isArray(items) ? items : [items];
            this.items.push(...toAdd);
            if (this.$grid) {
                toAdd.forEach((item, idx) => {
                    const globalIdx = this.items.length - toAdd.length + idx;
                    const $tile = this.createTile(item, globalIdx);
                    this.$grid.append($tile);
                });
            }
            return this;
        }

        /**
         * Replace the entire item list and re-render the gallery.
         */
        public setItems(items: IGalleryItem[]): this {
            this.items = [...items];
            if (this.$grid) {
                this.$grid.empty();
                this.items.forEach((item, idx) => {
                    this.$grid.append(this.createTile(item, idx));
                });
                // Re-init lightbox after DOM is rebuilt
                this.initPhotoSwipe();
            }
            return this;
        }

        /**
         * Remove a single item by index without reloading all.
         */
        public removeItem(index: number): this {
            this.items.splice(index, 1);
            if (this.$grid) {
                this.$grid.find(`[data-pswp-index="${index}"]`).remove();
                // Re-index remaining tiles
                this.$grid.find('.pswp-gallery__item').each((i, el) => {
                    $(el).attr('data-pswp-index', i);
                });
                // Re-init lightbox so index mapping is correct
                this.initPhotoSwipe();
            }
            return this;
        }

        /**
         * Remove all items and clear the grid.
         */
        public clear(): this {
            this.items = [];
            if (this.$grid) this.$grid.empty();
            return this;
        }

        /**
         * Programmatically open the lightbox at a given index.
         */
        public open(index: number = 0): this {
            if (!this.lightbox) {
                console.warn('GalleryBuilder: Call build() before open().');
                return this;
            }
            const dataSource = this.buildDataSource();
            this.lightbox.loadAndOpen(index, dataSource);
            return this;
        }

        /**
         * Initialize the gallery into the DOM and bind PhotoSwipe.
         * Must be called once after setting items.
         */
        public build(): this {
            this.$container = typeof this.options.container === 'string'
                ? $(this.options.container)
                : $(this.options.container);

            if (this.$container.length === 0) {
                console.error(`GalleryBuilder: container '${this.options.container}' not found.`);
                return this;
            }

            this.injectStyles();
            this.renderGrid();
            this.initPhotoSwipe();
            return this;
        }

        /**
         * Destroy the gallery: removes the grid, unbinds PhotoSwipe.
         */
        public destroy(): void {
            if (this.lightbox) {
                this.lightbox.destroy();
                this.lightbox = null;
            }
            if (this.$grid) {
                this.$grid.remove();
            }
        }

        // ─── Private Helpers ─────────────────────────────────────────────────────

        private renderGrid(): void {
            this.$container.empty();

            const gridId = `pswp-gallery-${Math.random().toString(36).substr(2, 7)}`;

            this.$grid = $(`<div 
                id="${gridId}" 
                class="pswp-gallery-grid"
                style="
                    display: grid;
                    grid-template-columns: repeat(${this.options.columns}, 1fr);
                    gap: ${this.options.gap}px;
                ">
            </div>`);

            this.items.forEach((item, idx) => {
                this.$grid.append(this.createTile(item, idx));
            });

            this.$container.append(this.$grid);
        }

        private createTile(item: IGalleryItem, index: number): JQuery<HTMLElement> {
            const thumbSrc = item.thumbnail ?? item.src;
            const alt = item.alt ?? item.title ?? `Image ${index + 1}`;
            const zoomIcon = this.options.showZoomIcon
                ? `<span class="pswp-gallery__zoom-icon">&#9906;</span>`
                : '';

            const deleteBtn = (this.options.enableDelete && item.fileId)
                ? `<button class="pswp-gallery__delete-btn" title="Xóa ảnh" data-file-id="${item.fileId}" data-index="${index}">
                       <i class="fa-solid fa-trash-can"></i>
                   </button>`
                : '';

            const downloadBtn = `<button class="pswp-gallery__download-btn" title="Tải xuống" data-src="${item.src}" data-filename="${item.title ?? alt}">
                   <i class="fa-solid fa-download"></i>
               </button>`;

            const $tile = $(`
                <a 
                    class="pswp-gallery__item"
                    data-pswp-index="${index}"
                    href="${item.src}"
                    data-pswp-width="${item.width}"
                    data-pswp-height="${item.height}"
                    target="_blank"
                    title="${item.title ?? alt}"
                    style="
                        display:block;
                        border-radius:${this.options.borderRadius}px;
                        background:#1a1a2e;
                        position:relative;
                        cursor:pointer;
                        text-decoration:none;
                    ">
                    <div style="
                        overflow:hidden;
                        border-radius:${this.options.borderRadius}px;
                        width:100%;
                        height:100%;
                        position:absolute;
                        top:0; left:0;
                    ">
                        <img 
                            alt="${alt}"
                            style="
                                width:100%;
                                height:100%;
                                object-fit:cover;
                                display:block;
                                transition:transform .3s ease, opacity .3s ease;
                                opacity:0;
                                will-change:opacity;
                            " />
                    </div>
                    ${zoomIcon}
                    ${downloadBtn}
                    ${deleteBtn}
                </a>`);

            // Lazy load via IntersectionObserver
            const $img = $tile.find('img');
            const imgEl = $img[0] as HTMLImageElement;

            // Apply real dimensions to PhotoSwipe when image finishes loading.
            // (This is called by whichever path actually loads the image below.)
            const applyRealDimensions = () => {
                const w = imgEl.naturalWidth;
                const h = imgEl.naturalHeight;
                if (w && h) {
                    $tile.attr('data-pswp-width', w).attr('data-pswp-height', h);
                }
                $img.css('opacity', '1');
            };

            // Native browser lazy loading – zero JS on the scroll path, no jank.
            imgEl.loading = 'lazy';
            imgEl.decoding = 'async';
            imgEl.src = thumbSrc;
            imgEl.onload = () => applyRealDimensions();
            imgEl.onerror = () => $img.css('opacity', '0.3');

            // Hover micro-animation
            $tile.on('mouseenter', function () {
                $(this).find('img').css('transform', 'scale(1.07)').css('opacity', '0.85');
                $(this).find('.pswp-gallery__zoom-icon').css('opacity', '1');
                $(this).find('.pswp-gallery__delete-btn, .pswp-gallery__download-btn').css('opacity', '1');
            }).on('mouseleave', function () {
                $(this).find('img').css('transform', 'scale(1)').css('opacity', '1');
                $(this).find('.pswp-gallery__zoom-icon').css('opacity', '0');
                $(this).find('.pswp-gallery__delete-btn, .pswp-gallery__download-btn').css('opacity', '0');
            });

            // Download button handler
            $tile.find('.pswp-gallery__download-btn').on('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                const src = $(this).attr('data-src')!;
                const filename = $(this).attr('data-filename') || 'download';
                const link = document.createElement('a');
                link.href = src;
                link.download = filename;
                link.click();
            });

            // Delete button handler
            if (this.options.enableDelete && this.options.onDelete) {
                const onDelete = this.options.onDelete;
                $tile.find('.pswp-gallery__delete-btn').on('click', function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    const fileId = $(this).attr('data-file-id')!;
                    const idx = parseInt($(this).attr('data-index')!, 10);
                    onDelete(fileId, idx);
                });
            }

            return $tile;
        }

        private buildDataSource(): { items: { src: string; width: number; height: number; alt?: string }[] } {
            return {
                items: this.items.map(item => ({
                    src: item.src,
                    width: item.width,
                    height: item.height,
                    alt: item.alt ?? item.title ?? ''
                }))
            };
        }

        private initPhotoSwipe(): void {
            const PSWPLightbox = (window as any).PhotoSwipeLightbox;
            if (typeof PSWPLightbox === 'undefined') {
                console.error('GalleryBuilder: PhotoSwipeLightbox is not loaded. ' +
                    'Add the PhotoSwipe UMD script before this script:\n' +
                    '<script src="/lib/photoswipe/photoswipe.umd.min.js"></script>\n' +
                    '<script src="/lib/photoswipe/photoswipe-lightbox.umd.min.js"></script>');
                return;
            }

            if (this.lightbox) {
                this.lightbox.destroy();
            }

            this.lightbox = new PSWPLightbox({
                gallery: this.$grid[0],
                children: 'a.pswp-gallery__item',
                pswpModule: (window as any).PhotoSwipe,
                bgOpacity: 0.9,
                showHideAnimationType: 'zoom',
                ...(this.options.photoswipeOptions ?? {})
            });

            this.lightbox.init();
        }

        private injectStyles(): void {
            const styleId = 'pswp-gallery-builder-styles';
            if (document.getElementById(styleId)) return;

            const css = `
                .pswp-gallery__item {
                    aspect-ratio: 1 / 1;
                    overflow: visible;
                }
                .pswp-gallery__zoom-icon {
                    position: absolute;
                    top: 50%;
                    left: 50%;
                    transform: translate(-50%, -50%);
                    font-size: 2rem;
                    color: #fff;
                    opacity: 0;
                    transition: opacity .25s ease;
                    pointer-events: none;
                    text-shadow: 0 2px 8px rgba(0,0,0,.6);
                    z-index: 5;
                }
                .pswp-gallery__delete-btn {
                    position: absolute;
                    top: 6px;
                    right: 6px;
                    width: 28px;
                    height: 28px;
                    border: none;
                    border-radius: 50%;
                    background: rgba(220, 38, 38, 0.85);
                    color: #fff;
                    font-size: 0.75rem;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    cursor: pointer;
                    opacity: 0;
                    transition: opacity .2s ease, background .2s ease;
                    z-index: 10;
                    backdrop-filter: blur(4px);
                }
                .pswp-gallery__delete-btn:hover {
                    background: rgba(185, 28, 28, 1);
                }
                .pswp-gallery__download-btn {
                    position: absolute;
                    top: 6px;
                    right: 40px;
                    width: 28px;
                    height: 28px;
                    border-radius: 50%;
                    background: rgba(30, 30, 60, 0.75);
                    color: #fff;
                    font-size: 0.75rem;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    cursor: pointer;
                    opacity: 0;
                    transition: opacity .2s ease, background .2s ease;
                    z-index: 10;
                    backdrop-filter: blur(4px);
                    text-decoration: none;
                }
                .pswp-gallery__download-btn:hover {
                    background: rgba(102, 126, 234, 0.9);
                }
            `;

            const $style = $(`<style id="${styleId}">${css}</style>`);
            $('head').append($style);
        }
    }
}
