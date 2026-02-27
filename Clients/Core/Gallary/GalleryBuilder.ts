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
        /** Extra PhotoSwipe options forwarded to PhotoSwipeLightbox */
        photoswipeOptions?: Record<string, any>;
    }

    // ─── GalleryBuilder ─────────────────────────────────────────────────────────

    export class GalleryBuilder {

        private options: Required<Omit<IGalleryOptions, 'items' | 'photoswipeOptions'>> & Pick<IGalleryOptions, 'photoswipeOptions'>;
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
                        overflow:hidden;
                        border-radius:${this.options.borderRadius}px;
                        background:#1a1a2e;
                        position:relative;
                        cursor:pointer;
                        text-decoration:none;
                    ">
                    <img 
                        src="${thumbSrc}" 
                        alt="${alt}"
                        style="
                            width:100%;
                            height:100%;
                            object-fit:cover;
                            display:block;
                            transition:transform .3s ease, opacity .3s ease;
                        " />
                    ${zoomIcon}
                </a>`);

            // Hover micro-animation
            $tile.on('mouseenter', function () {
                $(this).find('img').css('transform', 'scale(1.07)').css('opacity', '0.85');
                $(this).find('.pswp-gallery__zoom-icon').css('opacity', '1');
            }).on('mouseleave', function () {
                $(this).find('img').css('transform', 'scale(1)').css('opacity', '1');
                $(this).find('.pswp-gallery__zoom-icon').css('opacity', '0');
            });

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
                }
            `;

            const $style = $(`<style id="${styleId}">${css}</style>`);
            $('head').append($style);
        }
    }
}
