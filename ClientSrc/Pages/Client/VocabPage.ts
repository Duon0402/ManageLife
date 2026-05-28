namespace App {

    // ─── PAGE ─────────────────────────────────────────────────────────────────

    export class VocabPage extends BasePage {

        private words: VocabWordModel[] = [];
        private topics: VocabTopicModel[] = [];
        private decks: VocabDeckModel[] = [];
        private modal!: bootstrap.Modal;
        private modalTopic!: bootstrap.Modal;
        private modalDeck!: bootstrap.Modal;
        private isEdit = false;
        private isEditTopic = false;
        private isEditDeck = false;

        protected initialize(): void {
            this.modal = new bootstrap.Modal(document.getElementById('modal-word')!);
            this.modalTopic = new bootstrap.Modal(document.getElementById('modal-topic')!);
            this.modalDeck = new bootstrap.Modal(document.getElementById('modal-deck')!);
            this.loadAll();
        }

        protected bindEvents(): void {
            // Tìm kiếm
            this.root.find('#vocab-search').on('input', () => this.filterWords());

            // Mở modal thêm từ
            this.root.find('#btn-add-word').on('click', () => this.openCreateModal());

            // Tra từ điển
            $('#btn-lookup').on('click', () => this.lookupWord());
            $('#lookup-input').on('keydown', (e) => { if (e.key === 'Enter') this.lookupWord(); });

            // Lưu từ
            $('#btn-save-word').on('click', () => this.saveWord());

            // Nghe thử audio
            $('#btn-play-audio').on('click', () => {
                const url = ($('#word-audio').val() as string).trim();
                if (!url) { ToastService.warning('Chưa có link audio'); return; }
                new Audio(url).play();
            });

            // Topic
            $('#btn-add-topic').on('click', () => this.openCreateTopicModal());
            $('#btn-save-topic').on('click', () => this.saveTopic());

            // Deck
            $('#btn-add-deck').on('click', () => this.openCreateDeckModal());
            $('#btn-save-deck').on('click', () => this.saveDeck());

            // Event delegation bảng từ
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

            // Event delegation sidebar
            this.root.on('click', '.sidebar-item[data-topic]', (e) => {
                this.root.find('.sidebar-item[data-topic]').removeClass('active');
                $(e.currentTarget).addClass('active');
                this.filterWords();
            });

            this.root.on('click', '.topic-edit-btn', (e) => {
                e.stopPropagation();
                const id = $(e.currentTarget).data('id') as string;
                const topic = this.topics.find(t => t.id === id);
                if (topic) this.openEditTopicModal(topic);
            });

            this.root.on('click', '.topic-delete-btn', (e) => {
                e.stopPropagation();
                const id = $(e.currentTarget).data('id') as string;
                const topic = this.topics.find(t => t.id === id);
                if (topic) this.deleteTopic(id, topic.name);
            });

            this.root.on('click', '.deck-delete-btn', (e) => {
                e.stopPropagation();
                const id = $(e.currentTarget).data('id') as string;
                const deck = this.decks.find(d => d.id === id);
                if (deck) this.deleteDeck(id, deck.name);
            });
        }

        // ── Load ──────────────────────────────────────────────────────────────

        private async loadAll(): Promise<void> {
            LoadingService.show();
            try {
                await Promise.all([this.loadTopicsAndDecks(), this.loadWords()]);
            } finally {
                LoadingService.hide();
            }
        }

        private async loadTopicsAndDecks(): Promise<void> {
            const [topicsRes, decksRes] = await Promise.all([
                ApiService.get('/Vocab/GetTopics'),
                ApiService.get('/Vocab/GetDecks')
            ]);

            this.topics = topicsRes.isOk() ? topicsRes.data || [] : [];
            this.decks = decksRes.isOk() ? decksRes.data || [] : [];

            this.renderTopicSidebar();
            this.renderDeckSidebar();
            this.populateTopicSelect();
        }

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

        private renderTopicSidebar(): void {
            const $list = this.root.find('#topic-list');
            $list.html(`
                <div class="sidebar-item active" data-topic="">
                    <i class="fa-solid fa-layer-group"></i> Tất cả
                </div>`);

            this.topics.forEach(t => {
                const dot = t.color ? `<span class="topic-dot" style="background:${t.color}"></span>` : '<i class="fa-solid fa-tag"></i>';
                $list.append(`
                    <div class="sidebar-item" data-topic="${t.id}" style="justify-content:space-between">
                        <span style="display:flex;align-items:center;gap:8px">${dot} ${t.name}</span>
                        <span style="display:flex;gap:2px">
                            <button class="btn-action edit topic-edit-btn" data-id="${t.id}" style="font-size:0.75rem;padding:2px 5px"><i class="fa-solid fa-pen"></i></button>
                            <button class="btn-action delete topic-delete-btn" data-id="${t.id}" style="font-size:0.75rem;padding:2px 5px"><i class="fa-solid fa-trash"></i></button>
                        </span>
                    </div>`);
            });
        }

        private renderDeckSidebar(): void {
            const $list = this.root.find('#deck-list');
            if (!this.decks.length) {
                $list.html('<div class="text-muted" style="font-size:0.82rem;padding:4px 10px">Chưa có deck nào</div>');
                return;
            }
            $list.empty();
            this.decks.forEach(d => {
                const dot = d.topicColor ? `<span class="topic-dot" style="background:${d.topicColor}"></span>` : '<i class="fa-solid fa-cards-blank"></i>';
                $list.append(`
                    <div class="sidebar-item" style="justify-content:space-between">
                        <a href="/Vocab/Deck/${d.id}" style="display:flex;align-items:center;gap:8px;flex:1;text-decoration:none;color:inherit">
                            ${dot} <span>${d.name}</span>
                            <span class="badge-count">${d.totalCards}</span>
                        </a>
                        <button class="btn-action delete deck-delete-btn" data-id="${d.id}" style="font-size:0.75rem;padding:2px 5px"><i class="fa-solid fa-trash"></i></button>
                    </div>`);
            });
        }

        private populateTopicSelect(): void {
            const $sel = $('#deck-topic');
            $sel.find('option:not(:first)').remove();
            this.topics.forEach(t => $sel.append(`<option value="${t.id}">${t.name}</option>`));
        }

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

        // ── Word Modal ────────────────────────────────────────────────────────

        private openCreateModal(): void {
            this.isEdit = false;
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
            $('#word-id,#word-word,#word-phonetic,#word-pos,#word-definition,#word-example,#word-translation,#word-audio,#word-raw-json').val('');
            $('#word-dict-source').val('0');
            $('#lookup-input').val('');
            $('#lookup-result').hide().empty();
        }

        // ── Dictionary Lookup ─────────────────────────────────────────────────

        private async lookupWord(): Promise<void> {
            const word = ($('#lookup-input').val() as string).trim();
            if (!word) return;

            const $btn = $('#btn-lookup').prop('disabled', true).html('<i class="fa-solid fa-spinner fa-spin"></i>');

            try {
                const res = await ApiService.get('/Vocab/Lookup', { word });
                if (!res.isOk()) { ToastService.warning(res.message || 'Không tìm thấy từ này'); return; }

                const result: DictionaryLookupResult = res.data;
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

            $container.find('.lookup-meaning-item').on('click', (e) => {
                const idx = parseInt($(e.currentTarget).data('index'));
                $container.find('.lookup-meaning-item').removeClass('selected');
                $(e.currentTarget).addClass('selected');
                $('#word-pos').val(meanings[idx].partOfSpeech);
                $('#word-definition').val(meanings[idx].definition);
                $('#word-example').val(meanings[idx].exampleSentence || '');
            });
        }

        // ── Word CRUD ─────────────────────────────────────────────────────────

        private async saveWord(): Promise<void> {
            const word = ($('#word-word').val() as string).trim();
            const definition = ($('#word-definition').val() as string).trim();
            if (!word || !definition) { ToastService.warning('Vui lòng nhập Từ và Định nghĩa'); return; }

            const $btn = $('#btn-save-word').prop('disabled', true);
            LoadingService.show();

            try {
                if (this.isEdit) {
                    const res = await ApiService.put('/Vocab/UpdateWord', {
                        id: $('#word-id').val(),
                        word, definition,
                        phonetic: $('#word-phonetic').val() || null,
                        partOfSpeech: $('#word-pos').val() || null,
                        exampleSentence: $('#word-example').val() || null,
                        translation: $('#word-translation').val() || null,
                        audioUrl: $('#word-audio').val() || null,
                    });
                    if (!res.isOk()) { ToastService.error(res.message || 'Cập nhật thất bại'); return; }
                    ToastService.success('Đã cập nhật từ vựng');
                } else {
                    const res = await ApiService.post('/Vocab/CreateWord', {
                        word, definition,
                        phonetic: $('#word-phonetic').val() || null,
                        partOfSpeech: $('#word-pos').val() || null,
                        exampleSentence: $('#word-example').val() || null,
                        translation: $('#word-translation').val() || null,
                        audioUrl: $('#word-audio').val() || null,
                        rawDictionaryData: $('#word-raw-json').val() || null,
                        dictionarySource: parseInt($('#word-dict-source').val() as string) || 0,
                    });
                    if (!res.isOk()) { ToastService.error(res.message || 'Tạo thất bại'); return; }
                    ToastService.success('Đã thêm từ vựng');
                }
                this.modal.hide();
                await this.loadWords();
            } finally {
                $btn.prop('disabled', false);
                LoadingService.hide();
            }
        }

        private async deleteWord(id: string, word: string): Promise<void> {
            const confirmed = await MessageService.confirm(`Xóa từ "<strong>${word}</strong>"?`);
            if (!confirmed) return;

            LoadingService.show();
            try {
                const res = await ApiService.delete(`/Vocab/DeleteWord?id=${encodeURIComponent(id)}`);
                if (!res.isOk()) { ToastService.error(res.message || 'Xóa thất bại'); return; }
                ToastService.success('Đã xóa từ vựng');
                await this.loadWords();
            } finally {
                LoadingService.hide();
            }
        }

        // ── Topic CRUD ────────────────────────────────────────────────────────

        private openCreateTopicModal(): void {
            this.isEditTopic = false;
            $('#topic-id').val('');
            $('#topic-name').val('');
            $('#topic-desc').val('');
            $('#topic-color').val('#667eea');
            $('#modal-topic-title').text('Thêm chủ đề');
            this.modalTopic.show();
        }

        private openEditTopicModal(topic: VocabTopicModel): void {
            this.isEditTopic = true;
            $('#topic-id').val(topic.id);
            $('#topic-name').val(topic.name);
            $('#topic-desc').val(topic.description || '');
            $('#topic-color').val(topic.color || '#667eea');
            $('#modal-topic-title').text('Sửa chủ đề');
            this.modalTopic.show();
        }

        private async saveTopic(): Promise<void> {
            const name = ($('#topic-name').val() as string).trim();
            if (!name) { ToastService.warning('Vui lòng nhập tên chủ đề'); return; }

            const $btn = $('#btn-save-topic').prop('disabled', true);
            LoadingService.show();
            try {
                const payload = { name, description: $('#topic-desc').val() || null, color: $('#topic-color').val() };
                let res: ApiResponse;
                if (this.isEditTopic) {
                    res = await ApiService.put('/Vocab/UpdateTopic', { id: $('#topic-id').val(), ...payload });
                } else {
                    res = await ApiService.post('/Vocab/CreateTopic', payload);
                }
                if (!res.isOk()) { ToastService.error(res.message || 'Lưu thất bại'); return; }
                ToastService.success(this.isEditTopic ? 'Đã cập nhật chủ đề' : 'Đã thêm chủ đề');
                this.modalTopic.hide();
                await this.loadTopicsAndDecks();
            } finally {
                $btn.prop('disabled', false);
                LoadingService.hide();
            }
        }

        private async deleteTopic(id: string, name: string): Promise<void> {
            const confirmed = await MessageService.confirm(`Xóa chủ đề "<strong>${name}</strong>"?`);
            if (!confirmed) return;

            LoadingService.show();
            try {
                const res = await ApiService.delete(`/Vocab/DeleteTopic?id=${encodeURIComponent(id)}`);
                if (!res.isOk()) { ToastService.error(res.message || 'Xóa thất bại'); return; }
                ToastService.success('Đã xóa chủ đề');
                await this.loadTopicsAndDecks();
            } finally {
                LoadingService.hide();
            }
        }

        // ── Deck CRUD ─────────────────────────────────────────────────────────

        private openCreateDeckModal(): void {
            this.isEditDeck = false;
            $('#deck-id').val('');
            $('#deck-name').val('');
            $('#deck-desc').val('');
            $('#deck-topic').val('');
            $('#modal-deck-title').text('Thêm bộ thẻ');
            this.modalDeck.show();
        }

        private async saveDeck(): Promise<void> {
            const name = ($('#deck-name').val() as string).trim();
            if (!name) { ToastService.warning('Vui lòng nhập tên bộ thẻ'); return; }

            const $btn = $('#btn-save-deck').prop('disabled', true);
            LoadingService.show();
            try {
                const payload = {
                    name,
                    description: $('#deck-desc').val() || null,
                    topicId: $('#deck-topic').val() || null
                };
                let res: ApiResponse;
                if (this.isEditDeck) {
                    res = await ApiService.put('/Vocab/UpdateDeck', { id: $('#deck-id').val(), ...payload });
                } else {
                    res = await ApiService.post('/Vocab/CreateDeck', payload);
                }
                if (!res.isOk()) { ToastService.error(res.message || 'Lưu thất bại'); return; }
                ToastService.success(this.isEditDeck ? 'Đã cập nhật bộ thẻ' : 'Đã thêm bộ thẻ');
                this.modalDeck.hide();
                await this.loadTopicsAndDecks();
            } finally {
                $btn.prop('disabled', false);
                LoadingService.hide();
            }
        }

        private async deleteDeck(id: string, name: string): Promise<void> {
            const confirmed = await MessageService.confirm(`Xóa bộ thẻ "<strong>${name}</strong>"?`);
            if (!confirmed) return;

            LoadingService.show();
            try {
                const res = await ApiService.delete(`/Vocab/DeleteDeck?id=${encodeURIComponent(id)}`);
                if (!res.isOk()) { ToastService.error(res.message || 'Xóa thất bại'); return; }
                ToastService.success('Đã xóa bộ thẻ');
                await this.loadTopicsAndDecks();
            } finally {
                LoadingService.hide();
            }
        }
    }
}
