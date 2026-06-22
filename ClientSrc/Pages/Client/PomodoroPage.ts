namespace App {
    interface PomodoroSettings {
        id: string | null;
        focusMinutes: number;
        shortBreakMinutes: number;
        longBreakMinutes: number;
        sessionLoops: number | null;
        youtubeUrl: string | null;
        backgroundFileId: string | null;
    }

    interface PomodoroSessionRecord {
        id: string;
        startedAt: string;
        durationMinutes: number;
        type: number;
        isCompleted: boolean;
    }

    interface PomodoroHistory {
        sessions: PomodoroSessionRecord[];
        totalFocusMinutes: number;
        completedFocusSessions: number;
    }

    interface SessionPayload {
        type: number;
        durationMinutes: number;
        startedAt: string;
        isCompleted: boolean;
    }

    interface SavedTimerState {
        currentType: number;
        remainingSeconds: number;
        totalSeconds: number;
        startedAt: string | null;
        completedFocusInCycle: number;
    }

    const TYPE_NAMES = ['Focus', 'Short Break', 'Long Break'];
    const TYPE_COLORS = ['#4b49ac', '#27ae60', '#e67e22'];
    const TYPE_ICONS = ['fa-brain', 'fa-coffee', 'fa-bed'];
    const RING_CIRCUMFERENCE = 2 * Math.PI * 85;
    const LS_KEY = 'pomo_timer_state';
    const DEFAULT_SESSION_LOOPS = 4;

    export class PomodoroPage extends BasePage {
        private settings: PomodoroSettings = {
            id: null,
            focusMinutes: 25,
            shortBreakMinutes: 5,
            longBreakMinutes: 15,
            sessionLoops: null,
            youtubeUrl: null,
            backgroundFileId: null
        };

        private currentType = 0;
        private totalSeconds = 25 * 60;
        private remainingSeconds = 25 * 60;
        private isRunning = false;
        private timerId: number | null = null;
        private startedAt: Date | null = null;
        private completedFocusInCycle = 0;

        private sessionQueue: SessionPayload[] = [];
        private isFlushing = false;
        private audioCtx: AudioContext | null = null;

        protected initialize(): void {
            this.loadSettings();
            this.loadHistory();
            this.requestNotificationPermission();

            window.setInterval(() => {
                if (this.sessionQueue.length) this.flushSessionQueue();
            }, 60_000);

            // Hiện dialog xác nhận khi timer đang chạy
            window.addEventListener('beforeunload', (e) => {
                if (this.isRunning) {
                    e.preventDefault();
                    e.returnValue = '';
                }
            });

            // pagehide chỉ fire khi page thực sự đóng (user đã confirm rời hoặc đóng tab)
            window.addEventListener('pagehide', (e) => {
                if ((e as any).persisted) return;
                if (this.isRunning && this.startedAt) this.queueSession(false);
                if (this.sessionQueue.length) this.flushSessionQueue(true);
            });
        }

        protected bindEvents(): void {
            this.root.on('click', '.pomo-tab', (e) => {
                const type = parseInt($(e.currentTarget).data('type') as string);
                this.switchType(type);
            });

            this.root.find('#btn-start').on('click', () => this.toggleTimer());
            this.root.find('#btn-reset').on('click', () => this.resetTimer(true));
            this.root.find('#btn-save-settings').on('click', () => this.saveSettings());
            this.root.find('#input-youtube').on('blur', () => this.previewYoutube());

            this.root.find('#btn-toggle-settings').on('click', () => this.togglePanel('settings'));
            this.root.find('#btn-toggle-stats').on('click', () => this.togglePanel('stats'));
            this.root.find('#close-settings').on('click', () => this.closePanel('settings'));
            this.root.find('#close-stats').on('click', () => this.closePanel('stats'));
            this.root.find('#pomo-overlay').on('click', () => this.closeAllPanels());

            // Keyboard shortcuts: Space = start/pause, R = reset
            $(document).on('keydown.pomodoro', (e) => {
                if ($(e.target).is('input, textarea, select')) return;
                if (e.code === 'Space') { e.preventDefault(); this.toggleTimer(); }
                else if (e.code === 'KeyR') { this.resetTimer(true); }
            });
        }

        // ── Settings load / apply ──────────────────────────────

        private async loadSettings(): Promise<void> {
            const preloaded = (window as any).__pomodoroSettings as PomodoroSettings | undefined;
            if (preloaded) {
                this.settings = preloaded;
            } else {
                const res = await ApiService.get('/Pomodoro/GetSettings');
                if (!res.isOk() || !res.data) return;
                this.settings = res.data as PomodoroSettings;
            }
            const savedRaw = localStorage.getItem(LS_KEY);
            this.applySettings();
            if (savedRaw) this.restoreTimerState(savedRaw);
        }

        private applySettings(): void {
            this.root.find('#input-focus').val(this.settings.focusMinutes);
            this.root.find('#input-short').val(this.settings.shortBreakMinutes);
            this.root.find('#input-long').val(this.settings.longBreakMinutes);
            this.root.find('#input-loops').val(this.settings.sessionLoops ?? '');
            this.root.find('#input-youtube').val(this.settings.youtubeUrl ?? '');
            this.previewYoutube();
            this.resetTimer();
            this.updateLoopStatus();
        }

        private durationForType(type: number): number {
            if (type === 0) return this.settings.focusMinutes;
            if (type === 1) return this.settings.shortBreakMinutes;
            return this.settings.longBreakMinutes;
        }

        private switchType(type: number): void {
            if (this.isRunning) return;
            this.currentType = type;
            this.root.find('.pomo-tab').removeClass('active');
            this.root.find(`.pomo-tab[data-type="${type}"]`).addClass('active');
            this.updateRingColor();
            this.resetTimer();
        }

        private updateRingColor(): void {
            this.root.find('#timer-ring').css('stroke', TYPE_COLORS[this.currentType]);
            this.root.find('#type-label').text(TYPE_NAMES[this.currentType]);
        }

        // ── Timer controls ─────────────────────────────────────

        private toggleTimer(): void {
            if (this.isRunning) this.pauseTimer();
            else this.startTimer();
        }

        private startTimer(): void {
            if (this.remainingSeconds <= 0) {
                this.resetTimer();
                return;
            }
            if (!this.startedAt) this.startedAt = new Date();

            this.isRunning = true;
            this.root.find('#btn-start')
                .addClass('paused')
                .html('<i class="fa-solid fa-pause me-1"></i>Tạm dừng');

            this.controlYoutube('playVideo');
            this.setFocusMode(true);
            this.updateFavicon('⏱️');
            this.updateSettingsLock(true);

            this.timerId = window.setInterval(() => {
                this.remainingSeconds--;
                this.updateDisplay();
                if (this.remainingSeconds > 0 && this.remainingSeconds <= 5) this.playTick();
                if (this.remainingSeconds <= 0) this.onTimerComplete();
            }, 1000);
        }

        private pauseTimer(): void {
            this.isRunning = false;
            if (this.timerId !== null) { clearInterval(this.timerId); this.timerId = null; }
            this.controlYoutube('pauseVideo');
            this.setFocusMode(false);
            this.updateFavicon('⏸️');
            this.updateSettingsLock(false);
            this.root.find('#btn-start')
                .removeClass('paused')
                .html('<i class="fa-solid fa-play me-1"></i>Tiếp tục');
        }

        private resetTimer(saveIncomplete = false): void {
            if (saveIncomplete && this.isRunning && this.startedAt) {
                this.queueSession(false);
            }
            this.isRunning = false;
            if (this.timerId !== null) { clearInterval(this.timerId); this.timerId = null; }
            this.controlYoutube('pauseVideo');
            this.setFocusMode(false);
            this.updateFavicon('🍅');
            this.updateSettingsLock(false);
            this.startedAt = null;

            this.totalSeconds = this.durationForType(this.currentType) * 60;
            this.remainingSeconds = this.totalSeconds;

            localStorage.removeItem(LS_KEY);
            this.updateDisplay();
            this.root.find('#btn-start')
                .removeClass('paused')
                .html('<i class="fa-solid fa-play me-1"></i>Bắt đầu');
        }

        private updateDisplay(): void {
            const m = Math.floor(this.remainingSeconds / 60);
            const s = this.remainingSeconds % 60;
            const timeStr = `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`;
            this.root.find('#timer-display').text(timeStr);

            const progress = this.totalSeconds > 0 ? this.remainingSeconds / this.totalSeconds : 0;
            this.root.find('#timer-ring').css('stroke-dashoffset', RING_CIRCUMFERENCE * (1 - progress));

            if (this.isRunning) this.saveTimerState();
        }

        private onTimerComplete(): void {
            this.pauseTimer();
            this.root.find('#timer-display').text('00:00');
            localStorage.removeItem(LS_KEY);

            this.playBeep();
            this.showNotification(
                `${TYPE_NAMES[this.currentType]} hoàn thành!`,
                'Đã đến lúc chuyển sang phiên tiếp theo.'
            );
            ToastService.success(`${TYPE_NAMES[this.currentType]} hoàn thành!`);

            this.queueSession(true);
            if (this.root.find('#sidebar-stats').hasClass('open')) this.flushSessionQueue();

            if (this.currentType === 0) {
                this.completedFocusInCycle++;
                const loops = this.settings.sessionLoops ?? DEFAULT_SESSION_LOOPS;
                if (this.completedFocusInCycle >= loops) {
                    this.completedFocusInCycle = 0;
                    this.switchType(2);
                } else {
                    this.switchType(1);
                }
            } else {
                this.switchType(0);
            }

            this.updateLoopStatus();
            this.startTimer();
        }

        // ── Session Queue ──────────────────────────────────────

        private queueSession(isCompleted: boolean): void {
            if (!this.startedAt) return;
            this.sessionQueue.push({
                type: this.currentType,
                durationMinutes: this.durationForType(this.currentType),
                startedAt: this.startedAt.toISOString(),
                isCompleted
            });
        }

        private async flushSessionQueue(beacon = false): Promise<void> {
            if (!this.sessionQueue.length) return;
            if (this.isFlushing) return;

            const toFlush = [...this.sessionQueue];
            this.sessionQueue = [];

            if (beacon) {
                navigator.sendBeacon(
                    '/Pomodoro/SaveSessions',
                    new Blob([JSON.stringify(toFlush)], { type: 'application/json' })
                );
                return;
            }

            this.isFlushing = true;
            try {
                const res = await ApiService.post('/Pomodoro/SaveSessions', toFlush);
                if (res.isOk()) {
                    await this.loadHistory();
                } else {
                    this.sessionQueue = [...toFlush, ...this.sessionQueue];
                }
            } finally {
                this.isFlushing = false;
            }
        }

        // ── Loop status ────────────────────────────────────────

        private updateLoopStatus(): void {
            const loops = this.settings.sessionLoops ?? DEFAULT_SESSION_LOOPS;
            const $s = this.root.find('#loop-status');
            if (this.currentType === 0) {
                $s.text(`Vòng ${this.completedFocusInCycle + 1} / ${loops}`);
            } else {
                $s.text('Nghỉ giải lao...');
            }
        }

        // ── Panels ────────────────────────────────────────────

        private togglePanel(name: string): void {
            const $panel = this.root.find(`#sidebar-${name}`);
            const isOpen = $panel.hasClass('open');
            this.closeAllPanels();
            if (!isOpen) {
                if (name === 'stats') this.flushSessionQueue();
                $panel.addClass('open');
                this.root.find('#pomo-overlay').addClass('show');
                this.root.find(`#btn-toggle-${name}`).addClass('active');
            }
        }

        private closePanel(name: string): void {
            this.root.find(`#sidebar-${name}`).removeClass('open');
            this.root.find(`#btn-toggle-${name}`).removeClass('active');
            if (!this.root.find('.pomo-sidebar.open').length) {
                this.root.find('#pomo-overlay').removeClass('show');
            }
        }

        private closeAllPanels(): void {
            this.root.find('.pomo-sidebar').removeClass('open');
            this.root.find('.pomo-panel-btn').removeClass('active');
            this.root.find('#pomo-overlay').removeClass('show');
        }

        // ── History + Chart ───────────────────────────────────

        private async loadHistory(): Promise<void> {
            const res = await ApiService.get('/Pomodoro/GetHistory?days=7');
            if (res.isOk()) this.renderHistory(res.data as PomodoroHistory);
        }

        private renderHistory(data: PomodoroHistory): void {
            const hours = Math.floor(data.totalFocusMinutes / 60);
            const mins = data.totalFocusMinutes % 60;
            this.root.find('#stat-focus-time').text(hours > 0 ? `${hours}h ${mins}m` : `${data.totalFocusMinutes} phút`);
            this.root.find('#stat-sessions').text(data.completedFocusSessions);

            this.renderChart(data.sessions);

            const $list = this.root.find('#sessions-list');
            $list.empty();

            if (!data.sessions.length) {
                $list.html('<p class="text-center text-muted py-3" style="font-size:.82rem">Chưa có phiên nào trong 7 ngày qua</p>');
                return;
            }

            data.sessions.slice(0, 15).forEach(s => {
                const d = new Date(s.startedAt);
                const dateStr = d.toLocaleDateString('vi-VN');
                const timeStr = d.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
                const statusIcon = s.isCompleted
                    ? '<i class="fa-solid fa-circle-check" style="color:#27ae60"></i>'
                    : '<i class="fa-solid fa-circle-xmark" style="color:#e74c3c"></i>';
                $list.append(`
                    <div class="session-item">
                        <span class="session-type" style="color:${TYPE_COLORS[s.type]}">
                            <i class="fa-solid ${TYPE_ICONS[s.type]} me-1"></i>${TYPE_NAMES[s.type]}
                        </span>
                        <span class="session-duration">${s.durationMinutes}p</span>
                        <span class="session-time">${dateStr} ${timeStr}</span>
                        ${statusIcon}
                    </div>
                `);
            });
        }

        private renderChart(sessions: PomodoroSessionRecord[]): void {
            const $chart = this.root.find('#pomo-chart');
            $chart.empty();

            // Build last 7 days array
            const days: { label: string; count: number }[] = [];
            for (let i = 6; i >= 0; i--) {
                const d = new Date();
                d.setDate(d.getDate() - i);
                days.push({ label: d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' }), count: 0 });
            }

            // Count completed focus sessions per day
            sessions.filter(s => s.type === 0 && s.isCompleted).forEach(s => {
                const key = new Date(s.startedAt).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' });
                const day = days.find(x => x.label === key);
                if (day) day.count++;
            });

            const max = Math.max(...days.map(d => d.count), 1);

            days.forEach(day => {
                const pct = Math.round((day.count / max) * 100);
                $chart.append(`
                    <div class="pomo-bar-col">
                        <div class="pomo-bar-count">${day.count > 0 ? day.count : ''}</div>
                        <div class="pomo-bar-wrap">
                            <div class="pomo-bar" style="height:${Math.max(pct, day.count > 0 ? 8 : 2)}%"
                                 title="${day.count} phiên focus"></div>
                        </div>
                        <div class="pomo-bar-label">${day.label}</div>
                    </div>
                `);
            });
        }

        // ── Settings save ──────────────────────────────────────

        private async saveSettings(): Promise<void> {
            const focusMinutes = parseInt(this.root.find('#input-focus').val() as string) || 25;
            const shortBreakMinutes = parseInt(this.root.find('#input-short').val() as string) || 5;
            const longBreakMinutes = parseInt(this.root.find('#input-long').val() as string) || 15;
            const loopsRaw = (this.root.find('#input-loops').val() as string).trim();
            const sessionLoops = loopsRaw ? (parseInt(loopsRaw) || null) : null;
            const youtubeUrl = (this.root.find('#input-youtube').val() as string).trim() || null;

            LoadingService.show();
            try {
                const res = await ApiService.post('/Pomodoro/SaveSettings', {
                    focusMinutes, shortBreakMinutes, longBreakMinutes,
                    sessionLoops, youtubeUrl,
                    backgroundFileId: this.settings.backgroundFileId
                });

                if (res.isOk()) {
                    this.settings.focusMinutes = focusMinutes;
                    this.settings.shortBreakMinutes = shortBreakMinutes;
                    this.settings.longBreakMinutes = longBreakMinutes;
                    this.settings.sessionLoops = sessionLoops;
                    this.settings.youtubeUrl = youtubeUrl;

                    ToastService.success('Đã lưu cài đặt');
                    this.completedFocusInCycle = 0;
                    this.resetTimer();
                    this.updateLoopStatus();
                    this.previewYoutube();
                } else {
                    ToastService.error(res.message || 'Không thể lưu cài đặt');
                }
            } finally {
                LoadingService.hide();
            }
        }

        // ── Focus mode ────────────────────────────────────────

        private setFocusMode(active: boolean): void {
            document.body.classList.toggle('pomo-focus-mode', active);
        }

        private updateSettingsLock(locked: boolean): void {
            this.root.find('#btn-save-settings').prop('disabled', locked);
            this.root.find('.pomo-num, #input-youtube').prop('disabled', locked);
            this.root.find('#settings-lock-note').toggle(locked);
        }

        // ── Dynamic favicon ────────────────────────────────────

        private updateFavicon(emoji: string): void {
            try {
                const canvas = document.createElement('canvas');
                canvas.width = 32;
                canvas.height = 32;
                const ctx = canvas.getContext('2d');
                if (!ctx) return;
                ctx.font = '26px serif';
                ctx.textAlign = 'center';
                ctx.textBaseline = 'middle';
                ctx.fillText(emoji, 16, 17);
                let link = document.querySelector<HTMLLinkElement>('link[rel~="icon"]');
                if (!link) {
                    link = document.createElement('link');
                    link.rel = 'icon';
                    document.head.appendChild(link);
                }
                link.href = canvas.toDataURL();
            } catch {}
        }

        // ── Desktop notifications ─────────────────────────────

        private async requestNotificationPermission(): Promise<void> {
            if ('Notification' in window && Notification.permission === 'default') {
                await Notification.requestPermission();
            }
        }

        private showNotification(title: string, body: string): void {
            if (!('Notification' in window) || Notification.permission !== 'granted') return;
            new Notification(title, { body, icon: '/favicon.ico' });
        }

        // ── Sound ─────────────────────────────────────────────

        private playBeep(): void {
            this.playTone(880, 0.3, 0.9);
        }

        private playTick(): void {
            this.playTone(1100, 0.18, 0.12);
        }

        private getAudioContext(): AudioContext | null {
            if (this.audioCtx) return this.audioCtx;
            const AudioCtx = window.AudioContext || (window as any).webkitAudioContext;
            if (!AudioCtx) return null;
            this.audioCtx = new AudioCtx() as AudioContext;
            return this.audioCtx;
        }

        private playTone(frequency: number, volume: number, duration: number): void {
            try {
                const ctx = this.getAudioContext();
                if (!ctx) return;
                const osc = ctx.createOscillator();
                const gain = ctx.createGain();
                osc.connect(gain);
                gain.connect(ctx.destination);
                osc.type = 'sine';
                osc.frequency.value = frequency;
                gain.gain.setValueAtTime(volume, ctx.currentTime);
                gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + duration);
                osc.start(ctx.currentTime);
                osc.stop(ctx.currentTime + duration);
            } catch {}
        }

        // ── LocalStorage state backup ─────────────────────────

        private saveTimerState(): void {
            try {
                const state: SavedTimerState = {
                    currentType: this.currentType,
                    remainingSeconds: this.remainingSeconds,
                    totalSeconds: this.totalSeconds,
                    startedAt: this.startedAt?.toISOString() ?? null,
                    completedFocusInCycle: this.completedFocusInCycle
                };
                localStorage.setItem(LS_KEY, JSON.stringify(state));
            } catch {}
        }

        private restoreTimerState(raw: string): void {
            try {
                const state: SavedTimerState = JSON.parse(raw);
                if (state.currentType !== this.currentType) {
                    this.currentType = state.currentType;
                    this.root.find('.pomo-tab').removeClass('active');
                    this.root.find(`.pomo-tab[data-type="${this.currentType}"]`).addClass('active');
                    this.updateRingColor();
                }
                this.totalSeconds = state.totalSeconds;
                this.remainingSeconds = state.remainingSeconds;
                this.completedFocusInCycle = state.completedFocusInCycle;
                if (state.startedAt) this.startedAt = new Date(state.startedAt);
                this.updateDisplay();
                this.updateLoopStatus();
                this.root.find('#btn-start')
                    .removeClass('paused')
                    .html('<i class="fa-solid fa-play me-1"></i>Tiếp tục');
            } catch {
                localStorage.removeItem(LS_KEY);
            }
        }

        // ── YouTube ───────────────────────────────────────────

        private previewYoutube(): void {
            const url = (this.root.find('#input-youtube').val() as string || '').trim();
            const $wrap = this.root.find('#youtube-embed-wrap');
            const $frame = this.root.find('#youtube-frame');

            if (!url) { $wrap.hide(); return; }

            const videoId = this.extractYoutubeId(url);
            if (videoId) {
                $frame.attr('src', `https://www.youtube.com/embed/${videoId}?enablejsapi=1&loop=1&playlist=${videoId}`);
                $wrap.show();
            } else {
                $wrap.hide();
            }
        }

        private controlYoutube(func: string): void {
            const frame = this.root.find('#youtube-frame')[0] as HTMLIFrameElement | undefined;
            if (!frame?.contentWindow) return;
            frame.contentWindow.postMessage(JSON.stringify({ event: 'command', func, args: '' }), '*');
        }

        private extractYoutubeId(url: string): string | null {
            const match = url.match(/(?:v=|youtu\.be\/|embed\/)([a-zA-Z0-9_-]{11})/);
            return match ? match[1] : null;
        }
    }
}
