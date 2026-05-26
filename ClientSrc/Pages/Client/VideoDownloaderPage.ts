namespace App {
    interface VideoInfo {
        originalUrl: string;
        title: string;
        authorNickname: string;
        thumbnailUrl: string;
        videoUrl: string;
        musicUrl: string;
        musicTitle: string;
        duration: number;
    }

    export class VideoDownloaderPage extends BasePage {
        private currentInfo: VideoInfo | null = null;

        protected initialize(): void { }

        protected bindEvents(): void {
            this.root.find('.btn-get-info').on('click', () => this.fetchVideoInfo());
            this.root.find('#video-url-input').on('keydown', (e) => {
                if (e.key === 'Enter') this.fetchVideoInfo();
            });
            this.root.find('.btn-download-video').on('click', () => this.downloadVideo());
            this.root.find('.btn-download-audio').on('click', () => this.downloadAudio());
        }

        private async fetchVideoInfo(): Promise<void> {
            const url = (this.root.find('#video-url-input').val() as string).trim();
            if (!url) {
                ToastService.warning('Vui lòng nhập link video');
                return;
            }

            this.hideResult();
            this.hideError();

            LoadingService.show();
            try {
                const res = await ApiService.post<VideoInfo>('/VideoDownloader/GetVideoInfo', { url });
                if (res.isOk()) {
                    this.currentInfo = res.data;
                    this.renderResult(res.data);
                } else {
                    this.showError(res.message || 'Không lấy được thông tin video');
                }
            } catch {
                this.showError('Lỗi hệ thống, vui lòng thử lại');
            } finally {
                LoadingService.hide();
            }
        }

        private renderResult(info: VideoInfo): void {
            this.root.find('#video-thumbnail').attr('src', info.thumbnailUrl);
            this.root.find('#video-title').text(info.title || '(Không có tiêu đề)');
            this.root.find('#video-author').text(info.authorNickname || '');
            this.root.find('#video-duration').text(this.formatDuration(info.duration));

            const $musicRow = this.root.find('#video-music-title');
            if (info.musicTitle) {
                $musicRow.find('span').text(info.musicTitle);
                $musicRow.removeClass('d-none');
            } else {
                $musicRow.addClass('d-none');
            }

            this.root.find('.btn-download-audio').toggleClass('d-none', !info.musicUrl);
            this.root.find('#video-result').removeClass('d-none');
        }

        private downloadVideo(): void {
            if (!this.currentInfo?.originalUrl) {
                ToastService.error('Không có thông tin video');
                return;
            }
            LoadingService.show();
            ApiService.postBlob('/VideoDownloader/Download', {
                originalUrl: this.currentInfo.originalUrl,
                fileName: this.currentInfo.title || 'video'
            })
                .then(blob => {
                    const url = URL.createObjectURL(blob);
                    const a = document.createElement('a');
                    a.href = url;
                    a.download = `${this.currentInfo!.title || 'video'}.mp4`;
                    a.click();
                    URL.revokeObjectURL(url);
                })
                .catch(() => ToastService.error('Tải video thất bại'))
                .finally(() => LoadingService.hide());
        }

        private downloadAudio(): void {
            if (!this.currentInfo?.musicUrl) {
                ToastService.error('Không có URL âm thanh');
                return;
            }
            const a = document.createElement('a');
            a.href = this.currentInfo.musicUrl;
            a.download = `${this.currentInfo.musicTitle || 'audio'}.mp3`;
            a.target = '_blank';
            a.click();
        }

        private formatDuration(seconds: number): string {
            if (!seconds) return '';
            const m = Math.floor(seconds / 60);
            const s = seconds % 60;
            return `${m}:${s.toString().padStart(2, '0')}`;
        }

        private hideResult(): void {
            this.root.find('#video-result').addClass('d-none');
        }

        private showError(msg: string): void {
            this.root.find('#video-error span').text(msg);
            this.root.find('#video-error').removeClass('d-none');
        }

        private hideError(): void {
            this.root.find('#video-error').addClass('d-none');
        }
    }
}
