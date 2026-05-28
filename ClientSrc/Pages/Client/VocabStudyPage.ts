namespace App {

    interface VocabStudyPageModel {
        deckId: string;
    }

    export class VocabStudyPage extends BasePage<VocabStudyPageModel> {

        private cards: StudyCardModel[] = [];
        private currentIndex: number = 0;
        private correctCount: number = 0;
        private againCount: number = 0;
        private isAnswerShown: boolean = false;
        private currentAudioUrl: string | null = null;

        private get deckId(): string { return this.model.deckId; }

        protected initialize(): void {
            this.loadCards();
        }

        protected bindEvents(): void {
            $('#btn-show-answer').on('click', () => this.showAnswer());

            this.root.on('click', '.btn-rating', async (e) => {
                if (!this.isAnswerShown) return;
                const quality = parseInt($(e.currentTarget).data('quality') as string);
                await this.submitAndNext(quality);
            });

            $('#card-audio-btn').on('click', () => {
                if (this.currentAudioUrl) new Audio(this.currentAudioUrl).play();
            });
        }

        // ── Load ──────────────────────────────────────────────────────────────

        private async loadCards(): Promise<void> {
            const res = await ApiService.get('/Vocab/GetDueCards', { deckId: this.deckId });

            $('#study-loading').hide();

            if (!res.isOk()) {
                ToastService.error(res.message || 'Không thể tải thẻ học');
                $('#study-empty').show();
                return;
            }

            this.cards = res.data || [];
            this.currentIndex = 0;
            this.correctCount = 0;
            this.againCount = 0;

            if (!this.cards.length) {
                $('#study-empty').show();
                return;
            }

            $('#study-area').show();
            this.showCard(0);
        }

        // ── Card display ──────────────────────────────────────────────────────

        private showCard(index: number): void {
            const card = this.cards[index];
            this.isAnswerShown = false;
            this.currentAudioUrl = card.audioUrl || null;

            // Front
            $('#card-word').text(card.word);
            $('#card-phonetic').text(card.phonetic || '');
            if (card.audioUrl) {
                $('#card-audio-btn').show();
            } else {
                $('#card-audio-btn').hide();
            }
            $('#card-badge-new').toggle(card.isNew);

            // Back
            $('#card-back-pos').html(card.partOfSpeech ? `<span class="card-back-pos">${card.partOfSpeech}</span>` : '');
            $('#card-back-definition').text(card.definition || '');
            $('#card-back-translation').text(card.translation ? `🇻🇳 ${card.translation}` : '');
            if (card.exampleSentence) {
                $('#card-back-example').text(card.exampleSentence).show();
            } else {
                $('#card-back-example').hide();
            }

            // Reset flip
            $('#flashcard').removeClass('flipped');
            $('#btn-show-answer').show();
            $('#rating-buttons').hide();

            // Progress
            const done = index;
            const total = this.cards.length;
            const pct = total > 0 ? Math.round((done / total) * 100) : 0;
            $('#progress-text').text(`${done} / ${total}`);
            $('#progress-remain').text(`Còn lại: ${total - done}`);
            $('#progress-bar').css('width', pct + '%');
        }

        private showAnswer(): void {
            this.isAnswerShown = true;
            $('#flashcard').addClass('flipped');
            $('#btn-show-answer').hide();
            $('#rating-buttons').show();
        }

        // ── Submit & advance ──────────────────────────────────────────────────

        private async submitAndNext(quality: number): Promise<void> {
            const card = this.cards[this.currentIndex];

            // Disable buttons during submit
            $('#rating-buttons .btn-rating').prop('disabled', true);

            const res = await ApiService.post('/Vocab/SubmitReview', {
                wordId: card.wordId,
                deckId: this.deckId,
                quality
            });

            if (!res.isOk()) {
                ToastService.error(res.message || 'Lưu kết quả thất bại');
                $('#rating-buttons .btn-rating').prop('disabled', false);
                return;
            }

            if (quality >= 3) {
                this.correctCount++;
            } else {
                this.againCount++;
                // Push card to end of queue for re-review this session
                this.cards.push({ ...card, isNew: false });
            }

            this.currentIndex++;

            if (this.currentIndex >= this.cards.length) {
                this.showSummary();
            } else {
                this.showCard(this.currentIndex);
            }
        }

        // ── Summary ───────────────────────────────────────────────────────────

        private showSummary(): void {
            const total = this.currentIndex;
            $('#study-area').hide();
            $('#stat-total').text(total);
            $('#stat-correct').text(this.correctCount);
            $('#stat-again').text(this.againCount);

            // Final progress = 100%
            $('#progress-bar').css('width', '100%');
            $('#progress-text').text(`${total} / ${total}`);
            $('#progress-remain').text('Hoàn thành!');

            $('#study-summary').show();
        }
    }
}
