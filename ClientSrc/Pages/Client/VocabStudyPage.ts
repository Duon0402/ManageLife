namespace App {

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
            this.root.find('#btn-show-answer').on('click', () => this.showAnswer());

            this.root.on('click', '.btn-rating', async (e) => {
                if (!this.isAnswerShown) return;
                const quality = parseInt($(e.currentTarget).data('quality') as string);
                await this.submitAndNext(quality);
            });

            this.root.find('#card-audio-btn').on('click', () => {
                if (this.currentAudioUrl) new Audio(this.currentAudioUrl).play();
            });
        }

        // ── Load ──────────────────────────────────────────────────────────────

        private async loadCards(): Promise<void> {
            const res = await ApiService.get('/Vocab/GetDueCards', { deckId: this.deckId });

            this.root.find('#study-loading').hide();

            if (!res.isOk()) {
                ToastService.error(res.message || 'Không thể tải thẻ học');
                this.root.find('#study-empty').show();
                return;
            }

            this.cards = res.data || [];
            this.currentIndex = 0;
            this.correctCount = 0;
            this.againCount = 0;

            if (!this.cards.length) {
                this.root.find('#study-empty').show();
                return;
            }

            this.root.find('#study-area').show();
            this.showCard(0);
        }

        // ── Card display ──────────────────────────────────────────────────────

        private showCard(index: number): void {
            const card = this.cards[index];
            this.isAnswerShown = false;
            this.currentAudioUrl = card.audioUrl || null;

            // Front
            this.root.find('#card-word').text(card.word);
            this.root.find('#card-phonetic').text(card.phonetic || '');
            if (card.audioUrl) {
                this.root.find('#card-audio-btn').show();
            } else {
                this.root.find('#card-audio-btn').hide();
            }
            this.root.find('#card-badge-new').toggle(card.isNew);

            // Back
            this.root.find('#card-back-pos').html(card.partOfSpeech ? `<span class="card-back-pos">${card.partOfSpeech}</span>` : '');
            this.root.find('#card-back-definition').text(card.definition || '');
            this.root.find('#card-back-translation').text(card.translation ? `🇻🇳 ${card.translation}` : '');
            if (card.exampleSentence) {
                this.root.find('#card-back-example').text(card.exampleSentence).show();
            } else {
                this.root.find('#card-back-example').hide();
            }

            // Reset flip
            this.root.find('#flashcard').removeClass('flipped');
            this.root.find('#btn-show-answer').show();
            this.root.find('#rating-buttons').hide();

            // Progress
            const done = index;
            const total = this.cards.length;
            const pct = total > 0 ? Math.round((done / total) * 100) : 0;
            this.root.find('#progress-text').text(`${done} / ${total}`);
            this.root.find('#progress-remain').text(`Còn lại: ${total - done}`);
            this.root.find('#progress-bar').css('width', pct + '%');
        }

        private showAnswer(): void {
            this.isAnswerShown = true;
            this.root.find('#flashcard').addClass('flipped');
            this.root.find('#btn-show-answer').hide();
            this.root.find('#rating-buttons').show();
        }

        // ── Submit & advance ──────────────────────────────────────────────────

        private async submitAndNext(quality: number): Promise<void> {
            const card = this.cards[this.currentIndex];

            // Disable buttons during submit
            this.root.find('#rating-buttons .btn-rating').prop('disabled', true);

            const res = await ApiService.post('/Vocab/SubmitReview', {
                wordId: card.wordId,
                deckId: this.deckId,
                quality
            });

            if (!res.isOk()) {
                ToastService.error(res.message || 'Lưu kết quả thất bại');
                this.root.find('#rating-buttons .btn-rating').prop('disabled', false);
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
            this.root.find('#study-area').hide();
            this.root.find('#stat-total').text(total);
            this.root.find('#stat-correct').text(this.correctCount);
            this.root.find('#stat-again').text(this.againCount);

            // Final progress = 100%
            this.root.find('#progress-bar').css('width', '100%');
            this.root.find('#progress-text').text(`${total} / ${total}`);
            this.root.find('#progress-remain').text('Hoàn thành!');

            this.root.find('#study-summary').show();
        }
    }
}
