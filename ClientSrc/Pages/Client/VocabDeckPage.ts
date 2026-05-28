namespace App {

    export class VocabDeckPage extends BasePage {

        private deckId: string = '';
        private deckWords: VocabWordModel[] = [];
        private allWords: VocabWordModel[] = [];
        private modal!: bootstrap.Modal;

        protected initialize(): void {
            this.deckId = this.root.data('deck-id') as string;
            this.modal = new bootstrap.Modal(document.getElementById('modal-add-word')!);
            this.loadDeckWords();
        }

        protected bindEvents(): void {
            $('#btn-add-to-deck').on('click', () => this.openAddWordModal());
            $('#add-word-search').on('input', () => this.filterPickList());

            this.root.on('click', '.btn-remove', async (e) => {
                const wordId = $(e.currentTarget).data('word-id') as string;
                const word = this.deckWords.find(w => w.id === wordId);
                if (!word) return;
                const confirmed = await MessageService.confirm(`Xóa từ "<strong>${word.word}</strong>" khỏi deck?`);
                if (!confirmed) return;
                LoadingService.show();
                try {
                    const res = await ApiService.delete(`/Vocab/RemoveWordFromDeck?deckId=${encodeURIComponent(this.deckId)}&wordId=${encodeURIComponent(wordId)}`);
                    if (!res.isOk()) { ToastService.error(res.message || 'Xóa thất bại'); return; }
                    ToastService.success('Đã xóa từ khỏi deck');
                    await this.loadDeckWords();
                } finally {
                    LoadingService.hide();
                }
            });
        }

        // ── Load ──────────────────────────────────────────────────────────────

        private async loadDeckWords(): Promise<void> {
            const res = await ApiService.get('/Vocab/GetDeckWords', { deckId: this.deckId });
            if (!res.isOk()) { this.renderEmpty(); return; }
            this.deckWords = res.data || [];
            this.renderTable(this.deckWords);
        }

        private async loadAllWords(): Promise<void> {
            const res = await ApiService.get('/Vocab/GetWords');
            this.allWords = res.isOk() ? res.data || [] : [];
        }

        // ── Render ─────────────────────────────────────────────────────────────

        private renderTable(words: VocabWordModel[]): void {
            const $tbody = this.root.find('#deck-tbody');
            $tbody.empty();

            if (!words.length) {
                $tbody.html(`
                    <tr><td colspan="5">
                        <div class="deck-empty">
                            <i class="fa-solid fa-book d-block"></i>
                            <p>Deck chưa có từ nào. Hãy thêm từ!</p>
                        </div>
                    </td></tr>`);
                return;
            }

            words.forEach(w => {
                const audio = w.audioUrl
                    ? `<button class="btn-action audio" onclick="new Audio('${w.audioUrl}').play()"><i class="fa-solid fa-volume-high"></i></button>`
                    : '';
                $tbody.append(`
                    <tr>
                        <td>
                            <strong>${w.word}</strong>
                            ${w.phonetic ? `<div class="word-phonetic">${w.phonetic}</div>` : ''}
                        </td>
                        <td>${w.partOfSpeech ? `<span class="word-pos">${w.partOfSpeech}</span>` : '-'}</td>
                        <td style="max-width:260px">${w.definition || '-'}</td>
                        <td>${w.translation || '-'}</td>
                        <td style="white-space:nowrap">
                            ${audio}
                            <button class="btn-remove" data-word-id="${w.id}" title="Xóa khỏi deck">
                                <i class="fa-solid fa-minus"></i>
                            </button>
                        </td>
                    </tr>`);
            });
        }

        private renderEmpty(): void {
            this.root.find('#deck-tbody').html(`
                <tr><td colspan="5">
                    <div class="deck-empty">
                        <i class="fa-solid fa-triangle-exclamation d-block"></i>
                        <p>Không thể tải dữ liệu.</p>
                    </div>
                </td></tr>`);
        }

        // ── Add word modal ────────────────────────────────────────────────────

        private async openAddWordModal(): Promise<void> {
            $('#add-word-search').val('');
            $('#word-pick-list').html('<div class="text-muted text-center py-3"><i class="fa-solid fa-spinner fa-spin"></i></div>');
            this.modal.show();
            await this.loadAllWords();
            this.renderPickList(this.allWords);
        }

        private filterPickList(): void {
            const kw = ($('#add-word-search').val() as string).toLowerCase().trim();
            const filtered = kw
                ? this.allWords.filter(w => w.word.toLowerCase().includes(kw) || (w.definition || '').toLowerCase().includes(kw))
                : this.allWords;
            this.renderPickList(filtered);
        }

        private renderPickList(words: VocabWordModel[]): void {
            const $list = $('#word-pick-list');
            const inDeckIds = new Set(this.deckWords.map(w => w.id));
            $list.empty();

            if (!words.length) {
                $list.html('<div class="text-muted text-center py-3">Không có từ nào</div>');
                return;
            }

            words.forEach(w => {
                const inDeck = inDeckIds.has(w.id);
                $list.append(`
                    <div class="d-flex align-items-center justify-content-between py-2 border-bottom">
                        <div>
                            <strong>${w.word}</strong>
                            ${w.phonetic ? `<span class="text-muted ms-2 small">${w.phonetic}</span>` : ''}
                            <div class="text-muted small">${w.definition || ''}</div>
                        </div>
                        <button class="btn btn-sm ${inDeck ? 'btn-secondary' : 'btn-outline-primary'} ms-3 btn-add-pick"
                            data-word-id="${w.id}" ${inDeck ? 'disabled' : ''}>
                            ${inDeck ? '<i class="fa-solid fa-check"></i>' : '<i class="fa-solid fa-plus"></i>'}
                        </button>
                    </div>`);
            });

            $list.find('.btn-add-pick:not(:disabled)').on('click', async (e) => {
                const wordId = $(e.currentTarget).data('word-id') as string;
                $(e.currentTarget).prop('disabled', true).html('<i class="fa-solid fa-spinner fa-spin"></i>');

                const res = await ApiService.post('/Vocab/AddWordToDeck', { deckId: this.deckId, wordId });
                if (!res.isOk()) {
                    ToastService.error(res.message || 'Thêm thất bại');
                    $(e.currentTarget).prop('disabled', false).html('<i class="fa-solid fa-plus"></i>');
                    return;
                }

                $(e.currentTarget).removeClass('btn-outline-primary').addClass('btn-secondary')
                    .html('<i class="fa-solid fa-check"></i>');
                ToastService.success('Đã thêm vào deck');
                await this.loadDeckWords();
            });
        }
    }
}
