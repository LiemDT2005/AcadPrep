/**
 * AcadPrep exam review — sidebar palette + full-screen question view with explanation.
 */
(function () {
    const cfg = window.examReviewConfig || {};
    const allQuestions = Array.isArray(cfg.questions) ? cfg.questions : [];
    let filter = (cfg.initialFilter || 'all').toLowerCase();
    let visibleIndexes = [];
    let cursor = 0;

    const paletteEl = document.getElementById('question-palette');
    const sidebarEl = document.getElementById('question-sidebar');
    const sidebarBackdrop = document.getElementById('sidebar-backdrop');
    const workspaceEl = document.getElementById('exam-workspace');
    const emptyFilterEl = document.getElementById('empty-filter');
    const visibleCountEl = document.getElementById('visible-count');
    const questionLabelEl = document.getElementById('question-label');
    const statusBadgeEl = document.getElementById('status-badge');
    const currentQNumEl = document.getElementById('current-q-num');
    const totalQNumEl = document.getElementById('total-q-num');
    const currentPartEl = document.getElementById('current-part-label');
    const passageWrap = document.getElementById('passage-wrap');
    const passageImage = document.getElementById('passage-image');
    const passageContent = document.getElementById('passage-content');
    const questionImageWrap = document.getElementById('question-image-wrap');
    const questionImage = document.getElementById('question-image');
    const questionText = document.getElementById('question-text');
    const optionsContainer = document.getElementById('options-container');
    const explanationPanel = document.getElementById('explanation-panel');
    const explanationEmpty = document.getElementById('explanation-empty');
    const explanationText = document.getElementById('explanation-text');
    const audioSection = document.getElementById('audio-section');
    const audioEl = document.getElementById('review-audio');
    const btnPrev = document.getElementById('btn-prev');
    const btnNext = document.getElementById('btn-next');

    function matchesFilter(q) {
        if (filter === 'incorrect') return q.isAnswered && !q.isCorrect;
        if (filter === 'correct') return q.isCorrect;
        if (filter === 'unanswered') return !q.isAnswered;
        return true;
    }

    function rebuildVisible() {
        visibleIndexes = allQuestions
            .map((q, idx) => ({ q, idx }))
            .filter(({ q }) => matchesFilter(q))
            .map(({ idx }) => idx);

        if (visibleCountEl) visibleCountEl.textContent = String(visibleIndexes.length);
        if (totalQNumEl) totalQNumEl.textContent = String(visibleIndexes.length);

        if (visibleIndexes.length === 0) {
            workspaceEl?.classList.add('hidden');
            emptyFilterEl?.classList.remove('hidden');
            buildPalette();
            return;
        }

        workspaceEl?.classList.remove('hidden');
        emptyFilterEl?.classList.add('hidden');

        if (cursor >= visibleIndexes.length) cursor = 0;
        renderCurrent();
        buildPalette();
    }

    function statusOf(q) {
        if (!q.isAnswered) return 'unanswered';
        return q.isCorrect ? 'correct' : 'incorrect';
    }

    function buildPalette() {
        if (!paletteEl) return;
        paletteEl.innerHTML = '';

        const byPart = new Map();
        visibleIndexes.forEach((absIdx, visIdx) => {
            const q = allQuestions[absIdx];
            if (!byPart.has(q.part)) byPart.set(q.part, []);
            byPart.get(q.part).push({ q, absIdx, visIdx });
        });

        [...byPart.keys()].sort((a, b) => a - b).forEach((part) => {
            const group = document.createElement('div');
            group.className = 'space-y-2';

            const heading = document.createElement('div');
            heading.className = 'text-label-sm font-bold text-on-surface-variant uppercase tracking-wide px-1';
            heading.textContent = `Part ${part}`;
            group.appendChild(heading);

            const grid = document.createElement('div');
            grid.className = 'flex flex-wrap gap-1.5';

            byPart.get(part).forEach(({ q, visIdx }) => {
                const cell = document.createElement('button');
                cell.type = 'button';
                cell.textContent = String(q.questionNumber);
                const st = statusOf(q);
                let cls = 'q-cell rounded-full border text-[11px] font-bold flex items-center justify-center transition-all hover:scale-105 ';
                if (st === 'correct') cls += 'q-cell-correct ';
                else if (st === 'incorrect') cls += 'q-cell-incorrect ';
                else cls += 'q-cell-unanswered ';
                if (visIdx === cursor) cls += 'q-cell-active ';
                cell.className = cls;
                cell.title = `Question ${q.questionNumber} — ${st}`;
                cell.addEventListener('click', () => {
                    cursor = visIdx;
                    renderCurrent();
                    buildPalette();
                    closeSidebar();
                });
                grid.appendChild(cell);
            });

            group.appendChild(grid);
            paletteEl.appendChild(group);
        });
    }

    function renderCurrent() {
        if (visibleIndexes.length === 0) return;
        const absIdx = visibleIndexes[cursor];
        const q = allQuestions[absIdx];
        if (!q) return;

        if (currentQNumEl) currentQNumEl.textContent = String(cursor + 1);
        if (currentPartEl) currentPartEl.textContent = String(q.part);
        if (questionLabelEl) questionLabelEl.textContent = `Question ${q.questionNumber}`;

        const st = statusOf(q);
        if (statusBadgeEl) {
            if (st === 'correct') {
                statusBadgeEl.className = 'px-2.5 py-1 rounded-md text-label-sm font-bold bg-green-100 text-green-800';
                statusBadgeEl.textContent = 'Correct';
            } else if (st === 'incorrect') {
                statusBadgeEl.className = 'px-2.5 py-1 rounded-md text-label-sm font-bold bg-red-100 text-red-800';
                statusBadgeEl.textContent = 'Incorrect';
            } else {
                statusBadgeEl.className = 'px-2.5 py-1 rounded-md text-label-sm font-bold bg-neutral-200 text-neutral-700';
                statusBadgeEl.textContent = 'Unanswered';
            }
        }

        // Passage
        const hasPassage = !!(q.passageContent || q.passageImageUrl);
        if (passageWrap) {
            passageWrap.classList.toggle('hidden', !hasPassage);
            if (hasPassage) {
                if (passageImage) {
                    if (q.passageImageUrl) {
                        passageImage.src = q.passageImageUrl;
                        passageImage.classList.remove('hidden');
                    } else {
                        passageImage.removeAttribute('src');
                        passageImage.classList.add('hidden');
                    }
                }
                if (passageContent) passageContent.textContent = q.passageContent || '';
            }
        }

        // Question image / text
        if (questionImageWrap && questionImage) {
            if (q.imageUrl) {
                questionImage.src = q.imageUrl;
                questionImageWrap.classList.remove('hidden');
            } else {
                questionImage.removeAttribute('src');
                questionImageWrap.classList.add('hidden');
            }
        }
        if (questionText) questionText.textContent = q.questionText || '';

        // Audio
        if (audioSection && audioEl) {
            if (q.audioUrl) {
                audioSection.classList.remove('hidden');
                if (audioEl.getAttribute('src') !== q.audioUrl) {
                    audioEl.pause();
                    audioEl.src = q.audioUrl;
                }
            } else {
                audioSection.classList.add('hidden');
                audioEl.pause();
                audioEl.removeAttribute('src');
            }
        }

        // Options
        if (optionsContainer) {
            optionsContainer.innerHTML = '';
            (q.options || []).forEach((opt) => {
                const isSelected = q.selectedOption === opt.letter;
                const isCorrect = q.correctOption === opt.letter;
                let cls = 'flex items-start gap-3 p-4 rounded-xl border border-outline-variant/40 ';
                if (isCorrect) cls += 'opt-correct ';
                else if (isSelected && !q.isCorrect) cls += 'opt-wrong ';

                const row = document.createElement('div');
                row.className = cls;
                row.innerHTML = `
                    <span class="w-8 h-8 rounded-full border border-current flex items-center justify-center text-label-sm font-bold shrink-0">${escapeHtml(opt.letter)}</span>
                    <div class="flex-1 min-w-0">
                        <p class="font-body-sm text-on-surface">${escapeHtml(opt.text || '')}</p>
                        <div class="flex flex-wrap gap-2 mt-1"></div>
                    </div>
                `;
                const tags = row.querySelector('div.flex');
                if (isSelected) {
                    const t = document.createElement('span');
                    t.className = 'text-[11px] font-bold ' + (q.isCorrect ? 'text-green-700' : 'text-red-700');
                    t.textContent = 'Your answer';
                    tags.appendChild(t);
                }
                if (isCorrect) {
                    const t = document.createElement('span');
                    t.className = 'text-[11px] font-bold text-green-700';
                    t.textContent = 'Correct answer';
                    tags.appendChild(t);
                }
                optionsContainer.appendChild(row);
            });
        }

        // Explanation
        const hasExplanation = !!(q.explanation && String(q.explanation).trim());
        if (explanationPanel && explanationEmpty && explanationText) {
            if (hasExplanation) {
                explanationPanel.classList.remove('hidden');
                explanationEmpty.classList.add('hidden');
                explanationText.textContent = q.explanation;
            } else {
                explanationPanel.classList.add('hidden');
                explanationEmpty.classList.remove('hidden');
            }
        }

        if (btnPrev) btnPrev.disabled = cursor <= 0;
        if (btnNext) btnNext.disabled = cursor >= visibleIndexes.length - 1;
    }

    function escapeHtml(str) {
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    function openSidebar() {
        if (!sidebarEl) return;
        sidebarEl.classList.add('is-open');
        sidebarEl.classList.remove('-translate-x-full');
        sidebarBackdrop?.classList.remove('hidden');
    }

    function closeSidebar() {
        if (!sidebarEl) return;
        sidebarEl.classList.remove('is-open');
        sidebarEl.classList.add('-translate-x-full');
        sidebarBackdrop?.classList.add('hidden');
    }

    function toggleSidebar() {
        if (!sidebarEl) return;
        if (sidebarEl.classList.contains('is-open')) closeSidebar();
        else openSidebar();
    }

    function setFilter(next) {
        filter = next;
            document.querySelectorAll('.review-filter').forEach((btn) => {
            const active = btn.getAttribute('data-filter') === filter;
            btn.className = active
                ? 'review-filter px-3 py-1.5 rounded-lg bg-primary text-on-primary text-label-md font-bold'
                : 'review-filter px-3 py-1.5 rounded-lg bg-surface-container-high text-on-surface text-label-md font-bold hover:bg-surface-container';
        });
        cursor = 0;
        rebuildVisible();

        const url = new URL(window.location.href);
        url.searchParams.set('filter', filter);
        window.history.replaceState({}, '', url.toString());
    }

    document.getElementById('btn-sidebar-toggle')?.addEventListener('click', toggleSidebar);
    document.getElementById('btn-sidebar-close')?.addEventListener('click', closeSidebar);
    sidebarBackdrop?.addEventListener('click', closeSidebar);

    document.querySelectorAll('.review-filter').forEach((btn) => {
        btn.addEventListener('click', () => setFilter(btn.getAttribute('data-filter') || 'all'));
    });

    btnPrev?.addEventListener('click', () => {
        if (cursor <= 0) return;
        cursor -= 1;
        renderCurrent();
        buildPalette();
    });

    btnNext?.addEventListener('click', () => {
        if (cursor >= visibleIndexes.length - 1) return;
        cursor += 1;
        renderCurrent();
        buildPalette();
    });

    document.addEventListener('keydown', (e) => {
        if (e.key === 'ArrowLeft') btnPrev?.click();
        if (e.key === 'ArrowRight') btnNext?.click();
    });

    // Prefer incorrect filter when linked from incorrect-answers page
    setFilter(filter);
})();
