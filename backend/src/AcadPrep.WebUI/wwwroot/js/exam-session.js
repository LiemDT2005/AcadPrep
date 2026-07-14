/**
 * AcadPrep exam session — question navigation, timer, save/submit APIs.
 */
(function () {
    const config = window.examSessionConfig || {};
    const mode = config.mode || 'full';
    const questions = config.questions || [];
    const answers = { ...(config.savedAnswers || {}) };
    let currentIndex = config.currentIndex || 0;
    let remaining = config.remainingSeconds || 0;
    let timerInterval = null;
    let isSubmitting = false;

    const timerEl = document.getElementById('exam-timer');
    const paletteEl = document.getElementById('question-palette');
    const optionsEl = document.getElementById('options-container');
    const questionTextEl = document.getElementById('question-text');
    const questionLabelEl = document.getElementById('question-label');
    const questionImageWrap = document.getElementById('question-image-wrap');
    const questionImageEl = document.getElementById('question-image');
    const partLabelEl = document.getElementById('current-part-label');
    const audioSection = document.getElementById('audio-section');
    const audioLockedRow = document.getElementById('audio-locked-row');
    const audioEl = document.getElementById('exam-audio');
    const audioTimeEl = document.getElementById('audio-time');
    const answeredCountEl = document.getElementById('answered-count');
    const currentQNumEl = document.getElementById('current-q-num');
    const totalQNumEl = document.getElementById('total-q-num');

    let segmentStopHandler = null;
    let activeAudioSrc = null;
    let activeSegmentKey = null;

    if (!questions.length) return;

    if (totalQNumEl) totalQNumEl.textContent = String(questions.length);

    function formatTime(seconds) {
        const h = Math.floor(seconds / 3600);
        const m = Math.floor((seconds % 3600) / 60);
        const s = seconds % 60;
        if (h > 0) return [h, m, s].map(n => String(n).padStart(2, '0')).join(':');
        return [m, s].map(n => String(n).padStart(2, '0')).join(':');
    }

    function updateTimer() {
        if (!timerEl || !remaining) return;
        timerEl.textContent = formatTime(remaining);
        if (remaining <= 300) timerEl.classList.add('exam-timer-warning');
        if (remaining <= 0) {
            clearInterval(timerInterval);
            autoSubmit();
        }
    }

    function startTimer() {
        if (!remaining) return;
        updateTimer();
        timerInterval = setInterval(() => {
            remaining -= 1;
            updateTimer();
        }, 1000);
    }

    function getCurrentQuestion() {
        return questions[currentIndex];
    }

    function isListeningPart(part) {
        return part <= 4;
    }

    function resolveQuestionAudio(q) {
        if (q.audioUrl) {
            return { src: q.audioUrl, start: null, end: null };
        }

        if (config.examAudioUrl && q.audioStartSecond != null) {
            return {
                src: config.examAudioUrl,
                start: q.audioStartSecond,
                end: q.audioEndSecond ?? null
            };
        }

        return null;
    }

    function clearSegmentHandler() {
        if (segmentStopHandler && audioEl) {
            audioEl.removeEventListener('timeupdate', segmentStopHandler);
            segmentStopHandler = null;
        }
    }

    function formatAudioClock(seconds) {
        const m = Math.floor(seconds / 60);
        const s = Math.floor(seconds % 60);
        return `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`;
    }

    function updateAudioTimeLabel(start, end) {
        if (!audioTimeEl) return;
        if (start == null || end == null) {
            audioTimeEl.textContent = '';
            return;
        }
        audioTimeEl.textContent = `${formatAudioClock(start)} – ${formatAudioClock(end)}`;
    }

    function playQuestionAudio(audioInfo) {
        if (!audioEl || !audioInfo?.src) return;

        clearSegmentHandler();

        const segmentKey = `${audioInfo.src}|${audioInfo.start ?? ''}|${audioInfo.end ?? ''}`;
        const needsSegment = audioInfo.start != null;

        const beginPlayback = () => {
            if (needsSegment) {
                audioEl.currentTime = audioInfo.start;
                updateAudioTimeLabel(audioInfo.start, audioInfo.end);

                segmentStopHandler = () => {
                    const stopAt = audioInfo.end ?? audioEl.duration;
                    if (stopAt && audioEl.currentTime >= stopAt - 0.05) {
                        audioEl.pause();
                        clearSegmentHandler();
                    }
                };
                audioEl.addEventListener('timeupdate', segmentStopHandler);
            } else {
                updateAudioTimeLabel(null, null);
            }

            if (mode === 'full') {
                audioEl.controls = false;
                audioEl.play().catch(() => {});
            } else {
                audioEl.controls = true;
            }
        };

        if (activeAudioSrc !== audioInfo.src) {
            activeAudioSrc = audioInfo.src;
            activeSegmentKey = null;
            audioEl.src = audioInfo.src;
            audioEl.load();
            audioEl.onloadedmetadata = () => {
                audioEl.onloadedmetadata = null;
                beginPlayback();
            };
            return;
        }

        if (activeSegmentKey !== segmentKey) {
            activeSegmentKey = segmentKey;
            beginPlayback();
        }
    }

    function hideAudioPlayer() {
        clearSegmentHandler();
        if (audioSection) audioSection.classList.add('hidden');
        if (audioLockedRow) audioLockedRow.classList.add('hidden');
        if (audioEl) {
            audioEl.pause();
        }
        updateAudioTimeLabel(null, null);
    }

    function renderQuestion() {
        const q = getCurrentQuestion();
        if (!q) return;

        if (questionLabelEl) questionLabelEl.textContent = `Question ${q.questionNumber}`;
        if (currentQNumEl) currentQNumEl.textContent = String(currentIndex + 1);
        if (partLabelEl) partLabelEl.textContent = `Part ${q.part}`;

        if (questionTextEl) {
            questionTextEl.textContent = q.questionText || '(No question text)';
        }

        if (questionImageWrap && questionImageEl) {
            if (q.imageUrl) {
                questionImageWrap.classList.remove('hidden');
                questionImageEl.src = q.imageUrl;
            } else {
                questionImageWrap.classList.add('hidden');
            }
        }

        if (audioEl) {
            const audioInfo = resolveQuestionAudio(q);
            if (audioInfo && isListeningPart(q.part)) {
                if (audioSection) audioSection.classList.remove('hidden');
                if (audioLockedRow) audioLockedRow.classList.toggle('hidden', mode !== 'full');
                playQuestionAudio(audioInfo);
            } else {
                hideAudioPlayer();
            }
        }

        if (optionsEl) {
            optionsEl.innerHTML = '';
            (q.options || []).forEach(opt => {
                const btn = document.createElement('button');
                btn.type = 'button';
                btn.dataset.answer = opt.letter;
                btn.className = 'w-full text-left p-4 rounded-xl border-2 transition-all flex items-start gap-3 ' +
                    (answers[q.id] === opt.letter
                        ? 'border-primary bg-primary/5 ring-2 ring-primary/20'
                        : 'border-outline-variant/40 hover:border-primary/50');
                btn.innerHTML = `<span class="shrink-0 w-8 h-8 rounded-lg bg-surface-container flex items-center justify-center font-bold">${opt.letter}</span><span class="pt-1">${opt.text}</span>`;
                btn.addEventListener('click', () => selectAnswer(q.id, opt.letter));
                optionsEl.appendChild(btn);
            });
        }

        if (paletteEl) {
            paletteEl.querySelectorAll('[data-q-idx]').forEach(cell => {
                const idx = parseInt(cell.dataset.qIdx, 10);
                cell.classList.remove('ring-2', 'ring-primary');
                if (idx === currentIndex) cell.classList.add('ring-2', 'ring-primary');
            });
        }

        const prevBtn = document.getElementById('btn-prev');
        if (prevBtn) {
            const lockPrev = mode === 'full' && isListeningPart(q.part);
            prevBtn.disabled = lockPrev || currentIndex === 0;
            prevBtn.classList.toggle('opacity-40', prevBtn.disabled);
        }
    }

    function buildPalette() {
        if (!paletteEl) return;
        paletteEl.innerHTML = '';
        const cols = questions.length <= 30 ? 10 : 20;
        paletteEl.style.gridTemplateColumns = `repeat(${cols}, minmax(28px, 1fr))`;

        questions.forEach((q, idx) => {
            const cell = document.createElement('button');
            cell.type = 'button';
            cell.dataset.qIdx = String(idx);
            cell.textContent = String(q.questionNumber);
            cell.className = 'q-cell rounded-md border border-outline-variant/30 text-[10px] flex items-center justify-center ' +
                (answers[q.id] ? 'bg-tertiary/20 text-tertiary font-bold' : 'bg-surface-container text-on-surface-variant');
            cell.addEventListener('click', () => {
                if (mode === 'full' && isListeningPart(getCurrentQuestion().part)) return;
                currentIndex = idx;
                renderQuestion();
            });
            paletteEl.appendChild(cell);
        });
    }

    function updateAnsweredCount() {
        if (answeredCountEl) answeredCountEl.textContent = String(Object.keys(answers).length);
        buildPalette();
    }

    async function selectAnswer(questionId, letter) {
        answers[questionId] = letter;
        updateAnsweredCount();
        renderQuestion();

        if (mode === 'full' && config.attemptId) {
            try {
                await fetch('?handler=SaveAnswer', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ attemptId: config.attemptId, questionId, selectedOption: letter })
                });
            } catch (e) { console.error(e); }
        }
    }

    async function saveProgress() {
        if (mode !== 'full' || !config.attemptId) return;
        try {
            await fetch('?handler=SaveProgress', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ attemptId: config.attemptId, remainingSeconds: remaining })
            });
        } catch (e) { console.error(e); }
    }

    async function autoSubmit() {
        const modal = document.getElementById('modal-timeout');
        if (modal) modal.classList.remove('hidden');
        await submitTest();
    }

    async function submitTest() {
        if (mode === 'full' && config.attemptId) {
            try {
                const res = await fetch('?handler=Submit', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ attemptId: config.attemptId, remainingSeconds: remaining })
                });
                const result = await res.json();
                if (result.success) {
                    isSubmitting = true;
                    window.location.href = `/Exams/Results?attemptId=${result.attemptId}`;
                }
            } catch (e) { console.error(e); }
        } else if (mode === 'practice' && config.sessionId) {
            try {
                const res = await fetch('?handler=Submit', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ sessionId: config.sessionId, answers })
                });
                const result = await res.json();
                if (result.success) {
                    isSubmitting = true;
                    window.location.href = `/Exams/Results?sessionId=${result.sessionId}`;
                }
            } catch (e) { console.error(e); }
        }
    }

    function showModal(id) {
        const el = document.getElementById(id);
        if (el) el.classList.remove('hidden');
    }
    function hideModal(id) {
        const el = document.getElementById(id);
        if (el) el.classList.add('hidden');
    }

    document.getElementById('btn-next')?.addEventListener('click', () => {
        if (currentIndex < questions.length - 1) {
            currentIndex += 1;
            renderQuestion();
        }
    });

    document.getElementById('btn-prev')?.addEventListener('click', () => {
        if (currentIndex > 0) {
            currentIndex -= 1;
            renderQuestion();
        }
    });

    document.getElementById('btn-submit')?.addEventListener('click', () => showModal('modal-submit'));
    document.querySelectorAll('[data-close-submit]').forEach(b => b.addEventListener('click', () => hideModal('modal-submit')));
    document.querySelectorAll('[data-confirm-submit]').forEach(b => b.addEventListener('click', () => submitTest()));

    document.getElementById('btn-exit')?.addEventListener('click', (e) => {
        e.preventDefault();
        if (mode === 'full') {
            showModal('modal-leave');
        } else if (mode === 'practice') {
            showModal('modal-leave');
        }
    });
    document.querySelectorAll('[data-close-leave]').forEach(b => b.addEventListener('click', () => hideModal('modal-leave')));
    document.querySelector('[data-confirm-leave]')?.addEventListener('click', async () => {
        isSubmitting = true;
        if (mode === 'full') {
            await saveProgress();
            window.location.href = `/Exams/Detail/${config.examId}`;
        } else {
            const exitBtn = document.getElementById('btn-exit');
            const exitUrl = exitBtn?.dataset?.exitUrl || `/Exams/Detail/${config.examId}`;
            window.location.href = exitUrl;
        }
    });

    // Practice mode: warn user when leaving/closing tab that progress will NOT be saved
    window.addEventListener('beforeunload', (e) => {
        if (isSubmitting) return;
        if (mode === 'full') {
            saveProgress();
        }
        if (mode === 'practice' && Object.keys(answers).length > 0) {
            e.preventDefault();
            e.returnValue = 'Your practice progress will not be saved. Are you sure you want to leave?';
            return e.returnValue;
        }
    });

    buildPalette();
    updateAnsweredCount();
    renderQuestion();
    startTimer();
})();
