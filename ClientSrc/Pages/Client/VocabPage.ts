namespace App {

    // ─── PAGE ─────────────────────────────────────────────────────────────────

    export class VocabPage extends BasePage {

        private words: VocabWordModel[] = [];
        private modal!: bootstrap.Modal;
        private isEdit = false;
        private selectedMeaning: DictionaryMeaningResult | null = null;

        protected initialize(): void {
            this.modal = new bootstrap.Modal(document.getElementById('modal-word')!);
            this.loadWords();
        }

        protected bindEvents(): void {
            // Tìm kiếm
            this.root.find('#vocab-search').on('input', () => this.filterWords());

            // Mở modal thêm mới
            this.root.find('#btn-add-word').on('click', () => this.openCreateModal());

            // Tra từ điển
            $('#btn-lookup').on('click', () => this.lookupWord());
            $('#lookup-input').on('keydown', (e) => { if (e.key === 'Enter') this.lookupWord(); });

            // Lưu từ
            $('#btn-save-word').on('click', () => this.saveWord());

            // Event delegation cho các nút trong bảng
            this.root.on('click', '.btn-action.edit', (e) => {
                const id = $(e.currentTarget).data('id') as string;
                const word = this.words.find(w => w.id === id);
                if (word) this.openEditModal(word);
            });

            this.root.on('click', '.btn-action.delete', (e) => {
                const id = $(e.currentTarget).data('id') as string;
                const word = this.words.find(w => w.id === id);
                if (word) this.deleteWord(id, word.word);
            });
        }

        // ── Load ──────────────────────────────────────────────────────────────

        private async loadWords(): Promise<void> {
            const res = await ApiService.get('/Vocab/GetWords');
            if (!res.isOk()) {
                ToastService.error(res.message || 'Không thể tải danh sách từ vựng');
                this.renderEmpty();
                return;
            }
            this.words = res.data || [];
            this.renderTable(this.words);
        }

        private filterWords(): void {
            const kw = (this.root.find('#vocab-search').val() as string).toLowerCase().trim();
            if (!kw) { this.renderTable(this.words); return; }
            const filtered = this.words.filter(w =>
                w.word.toLowerCase().includes(kw) ||
                (w.definition || '').toLowerCase().includes(kw) ||
                (w.translation || '').toLowerCase().includes(kw)
            );
            this.renderTable(filtered);
        }

        // ── Render ─────────────────────────────────────────────────────────────

        private renderTable(words: VocabWordModel[]): void {
            const $tbody = this.root.find('#vocab-tbody');
            $tbody.empty();

            if (!words.length) {
                $tbody.html(`
                    <tr><td colspan="6">
                        <div class="vocab-empty">
                            <i class="fa-solid fa-book d-block"></i>
                            <p>Chưa có từ nào. Hãy thêm từ đầu tiên!</p>
                        </div>
                    </td></tr>`);
                return;
            }

            words.forEach(w => $tbody.append(this.renderRow(w)));
        }

        private renderEmpty(): void {
            this.root.find('#vocab-tbody').html(`
                <tr><td colspan="6">
                    <div class="vocab-empty">
                        <i class="fa-solid fa-triangle-exclamation d-block"></i>
                        <p>Không thể tải dữ liệu.</p>
                    </div>
                </td></tr>`);
        }

        private renderRow(w: VocabWordModel): string {
            const audioBtn = w.audioUrl
                ? `<button class="btn-action audio" onclick="new Audio('${w.audioUrl}').play()" title="Phát âm"><i class="fa-solid fa-volume-high"></i></button>`
                : '';
            return `
                <tr data-id="${w.id}">
                    <td class="word-cell">
                        <strong>${w.word}</strong>
                        ${w.phonetic ? `<div class="word-phonetic">${w.phonetic}</div>` : ''}
                    </td>
                    <td>${w.partOfSpeech ? `<span class="word-pos">${w.partOfSpeech}</span>` : '-'}</td>
                    <td style="max-width:260px">${w.definition || '-'}</td>
                    <td>${w.translation || '-'}</td>
                    <td><span class="mastery-badge mastery-${w.masteryLevel}">${VocabMasteryLabel[w.masteryLevel] ?? '-'}</span></td>
                    <td style="white-space:nowrap">
                        ${audioBtn}
                        <button class="btn-action edit" data-id="${w.id}" title="Sửa"><i class="fa-solid fa-pen"></i></button>
                        <button class="btn-action delete" data-id="${w.id}" title="Xóa"><i class="fa-solid fa-trash"></i></button>
                    </td>
                </tr>`;
        }

        // ── Modal ─────────────────────────────────────────────────────────────

        private openCreateModal(): void {
            this.isEdit = false;
            this.selectedMeaning = null;
            this.resetForm();
            $('#modal-word-title').text('Thêm từ mới');
            $('#lookup-section').show();
            this.modal.show();
        }

        private openEditModal(word: VocabWordModel): void {
            this.isEdit = true;
            this.resetForm();
            $('#modal-word-title').text('Sửa từ vựng');
            $('#lookup-section').hide();
            $('#word-id').val(word.id);
            $('#word-word').val(word.word);
            $('#word-phonetic').val(word.phonetic || '');
            $('#word-pos').val(word.partOfSpeech || '');
            $('#word-definition').val(word.definition || '');
            $('#word-example').val(word.exampleSentence || '');
            $('#word-translation').val(word.translation || '');
            $('#word-audio').val(word.audioUrl || '');
            this.modal.show();
        }

        private resetForm(): void {
            $('#word-id').val('');
            $('#word-word').val('');
            $('#word-phonetic').val('');
            $('#word-pos').val('');
            $('#word-definition').val('');
            $('#word-example').val('');
            $('#word-translation').val('');
            $('#word-audio').val('');
            $('#word-raw-json').val('');
            $('#word-dict-source').val('0');
            $('#lookup-input').val('');
            $('#lookup-result').hide().empty();
            this.selectedMeaning = null;
        }

        // ── Dictionary Lookup ─────────────────────────────────────────────────

        private async lookupWord(): Promise<void> {
            const word = ($('#lookup-input').val() as string).trim();
            if (!word) return;

            const $btn = $('#btn-lookup');
            $btn.prop('disabled', true).html('<i class="fa-solid fa-spinner fa-spin"></i>');

            try {
                const res = await ApiService.get(`/Vocab/Lookup?word=${encodeURIComponent(word)}`);
                if (!res.isOk()) {
                    ToastService.warning(res.message || 'Không tìm thấy từ này');
                    return;
                }

                const result: DictionaryLookupResult = res.data;

                // Điền sẵn word + phonetic + audio
                $('#word-word').val(result.word);
                $('#word-phonetic').val(result.phonetic || '');
                $('#word-audio').val(result.audioUrl || '');
                $('#word-raw-json').val(result.rawJson || '');
                $('#word-dict-source').val('1');

                this.renderLookupMeanings(result.meanings);
            } finally {
                $btn.prop('disabled', false).html('<i class="fa-solid fa-magnifying-glass me-1"></i> Tra cứu');
            }
        }

        private renderLookupMeanings(meanings: DictionaryMeaningResult[]): void {
            const $container = $('#lookup-result');
            $container.empty().show();

            if (!meanings.length) {
                $container.html('<div class="lookup-result text-muted small">Không có định nghĩa nào.</div>');
                return;
            }

            const items = meanings.map((m, i) => `
                <div class="lookup-meaning-item" data-index="${i}">
                    <span class="word-pos me-2">${m.partOfSpeech}</span>
                    <span>${m.definition}</span>
                    ${m.exampleSentence ? `<div class="text-muted small mt-1">"${m.exampleSentence}"</div>` : ''}
                </div>`).join('');

            $container.html(`<div class="lookup-result"><div class="small text-muted mb-2">Chọn nghĩa muốn lưu:</div>${items}</div>`);

            // Click chọn nghĩa
            $container.find('.lookup-meaning-item').on('click', (e) => {
                const idx = parseInt($(e.currentTarget).data('index'));
                $container.find('.lookup-meaning-item').removeClass('selected');
                $(e.currentTarget).addClass('selected');
                this.selectedMeaning = meanings[idx];
                $('#word-pos').val(meanings[idx].partOfSpeech);
                $('#word-definition').val(meanings[idx].definition);
                $('#word-example').val(meanings[idx].exampleSentence || '');
            });
        }

        // ── CRUD ──────────────────────────────────────────────────────────────

        private async saveWord(): Promise<void> {
            const word = ($('#word-word').val() as string).trim();
            const definition = ($('#word-definition').val() as string).trim();
            if (!word || !definition) {
                ToastService.warning('Vui lòng nhập Từ và Định nghĩa');
                return;
            }

            const $btn = $('#btn-save-word').prop('disabled', true);

            try {
                if (this.isEdit) {
                    const id = $('#word-id').val() as string;
                    const payload: any = {
                        id,
                        word,
                        definition,
                        phonetic: $('#word-phonetic').val() || null,
                        partOfSpeech: $('#word-pos').val() || null,
                        exampleSentence: $('#word-example').val() || null,
                        translation: $('#word-translation').val() || null,
                        audioUrl: $('#word-audio').val() || null,
                    };
                    const res = await ApiService.put('/Vocab/UpdateWord', payload);
                    if (!res.isOk()) { ToastService.error(res.message || 'Cập nhật thất bại'); return; }
                    ToastService.success('Đã cập nhật từ vựng');
                } else {
                    const payload: any = {
                        word,
                        definition,
                        phonetic: $('#word-phonetic').val() || null,
                        partOfSpeech: $('#word-pos').val() || null,
                        exampleSentence: $('#word-example').val() || null,
                        translation: $('#word-translation').val() || null,
                        audioUrl: $('#word-audio').val() || null,
                        rawDictionaryData: $('#word-raw-json').val() || null,
                        dictionarySource: parseInt($('#word-dict-source').val() as string) || 0,
                    };
                    const res = await ApiService.post('/Vocab/CreateWord', payload);
                    if (!res.isOk()) { ToastService.error(res.message || 'Tạo thất bại'); return; }
                    ToastService.success('Đã thêm từ vựng');
                }

                this.modal.hide();
                await this.loadWords();
            } finally {
                $btn.prop('disabled', false);
            }
        }

        private async deleteWord(id: string, word: string): Promise<void> {
            const confirmed = await MessageService.confirm(`Xóa từ "<strong>${word}</strong>"?`);
            if (!confirmed) return;

            const res = await ApiService.delete(`/Vocab/DeleteWord?id=${id}`);
            if (!res.isOk()) { ToastService.error(res.message || 'Xóa thất bại'); return; }
            ToastService.success('Đã xóa từ vựng');
            await this.loadWords();
        }

    }
}
