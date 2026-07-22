/**
 * AcadPrep exam session — Zenlish sidebar, section timers, TOEIC IIG listening units.
 *
 * Listening units:
 *   Part 1–2: one question = one unit (audio → next unit)
 *   Part 3–4: QuestionGroup of 3 = one unit (show all 3, play audio once → next group)
 * Reading units:
 *   Part 6: Passage set (4 blanks) with shared text/image
 *   Part 7: QuestionGroup reading set with 1–3 passages + questions
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
    let timerRunning = false;

    const LISTENING_SECONDS = config.listeningSeconds || 45 * 60;
    const READING_SECONDS = config.readingSeconds || 75 * 60;

    /** @type {'intro-listening'|'listening'|'intro-reading'|'reading'|'active'} */
    let sectionPhase = mode === 'full' ? 'intro-listening' : 'active';
    let maxReachedIndex = currentIndex;
    let audioCompleteForCurrent = false;
    let advancingFromAudio = false;
    let playedUnitKeys = new Set();
    let segmentPollTimer = null;

    const progressKey = mode === 'full' && config.attemptId
        ? `acadprep-exam-${config.attemptId}`
        : null;

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
    const audioPlayerUi = document.getElementById('audio-player-ui');
    const audioPlayBtn = document.getElementById('audio-play-btn');
    const audioPlayIcon = document.getElementById('audio-play-icon');
    const audioProgressTrack = document.getElementById('audio-progress-track');
    const audioProgressFill = document.getElementById('audio-progress-fill');
    const audioTimeCurrentEl = document.getElementById('audio-time-current');
    const audioTimeDurationEl = document.getElementById('audio-time-duration');
    const audioTimeRangeEl = document.getElementById('audio-time-range');
    const answeredCountEl = document.getElementById('answered-count');
    const answeredCountBadge = document.getElementById('answered-count-badge');
    const currentQNumEl = document.getElementById('current-q-num');
    const totalQNumEl = document.getElementById('total-q-num');
    const totalQNumNav = document.getElementById('total-q-num-nav');
    const sectionIntroEl = document.getElementById('section-intro');
    const examWorkspace = document.getElementById('exam-workspace');
    const sidebarEl = document.getElementById('question-sidebar');
    const sidebarBackdrop = document.getElementById('sidebar-backdrop');
    const navHintEl = document.getElementById('nav-hint');
    const selectionVocabButton = document.getElementById('selection-vocab-button');
    const vocabToast = document.getElementById('vocab-toast');

    let segmentStopHandler = null;
    let endedHandler = null;
    let segmentTimeout = null;
    let seekedHandler = null;
    let segmentProgressHandler = null;
    let activeAudioSrc = null;
    let activePlaybackToken = 0;
    /** @type {{ start: number, end: number|null, duration: number|null }|null} */
    let activeSegment = null;
    let practiceSegmentReady = false;
    let selectedVocabWord = '';
    let selectionHideTimer = null;
    let toastHideTimer = null;

    if (!questions.length) return;

    if (totalQNumEl) totalQNumEl.textContent = String(questions.length);
    if (totalQNumNav) totalQNumNav.textContent = String(questions.length);

    const firstReadingIndex = questions.findIndex(q => q.part >= 5);
    const hasListening = questions.some(q => q.part <= 4);
    const hasReading = firstReadingIndex >= 0;

    // ── Persistence ─────────────────────────────────────────
    function loadProgress() {
        if (!progressKey) return null;
        try {
            return JSON.parse(localStorage.getItem(progressKey) || 'null');
        } catch {
            return null;
        }
    }

    function saveLocalProgress() {
        if (!progressKey) return;
        localStorage.setItem(progressKey, JSON.stringify({
            sectionPhase,
            maxReachedIndex,
            playedUnits: [...playedUnitKeys],
            remaining,
            currentIndex
        }));
    }

    function clearLocalProgress() {
        if (progressKey) localStorage.removeItem(progressKey);
    }

    // ── Timer ───────────────────────────────────────────────
    function formatTime(seconds) {
        const h = Math.floor(seconds / 3600);
        const m = Math.floor((seconds % 3600) / 60);
        const s = seconds % 60;
        if (h > 0) return [h, m, s].map(n => String(n).padStart(2, '0')).join(':');
        return [m, s].map(n => String(n).padStart(2, '0')).join(':');
    }

    function updateTimer() {
        if (!timerEl) return;
        if (!timerRunning) {
            timerEl.textContent = remaining > 0 ? formatTime(remaining) : '--:--:--';
            return;
        }
        timerEl.textContent = formatTime(Math.max(0, remaining));
        if (remaining <= 300) timerEl.classList.add('exam-timer-warning');
        else timerEl.classList.remove('exam-timer-warning');

        if (remaining <= 0) {
            clearInterval(timerInterval);
            timerInterval = null;
            timerRunning = false;
            onSectionTimeUp();
        }
    }

    function startTimer() {
        if (!remaining || timerRunning) return;
        timerRunning = true;
        updateTimer();
        timerInterval = setInterval(() => {
            remaining -= 1;
            updateTimer();
            if (remaining % 15 === 0) saveLocalProgress();
        }, 1000);
    }

    function stopTimer() {
        timerRunning = false;
        if (timerInterval) {
            clearInterval(timerInterval);
            timerInterval = null;
        }
    }

    // ── Question units (listening groups + reading sets) ────
    function isListeningPart(part) {
        return part <= 4;
    }

    function isReadingGroupPart(part) {
        return part === 6 || part === 7;
    }

    /** Stable unit identity for grouping contiguous questions, or null for singles. */
    function getGroupIdentity(q) {
        if (!q) return null;
        if ((q.part === 3 || q.part === 4 || q.part === 7) && q.questionGroupId != null) {
            return { key: `g-${q.questionGroupId}`, part: q.part };
        }
        if (q.part === 6 && q.passageId != null) {
            return { key: `p-${q.passageId}`, part: q.part };
        }
        return null;
    }

    function sameGroup(a, b) {
        const ia = getGroupIdentity(a);
        const ib = getGroupIdentity(b);
        return !!(ia && ib && ia.key === ib.key && ia.part === ib.part);
    }

    /** Bounds of the unit containing `idx` (inclusive). */
    function getUnitRange(idx) {
        const q = questions[idx];
        if (!q) return { start: idx, end: idx, isGroup: false, key: `q-${idx}` };

        const identity = getGroupIdentity(q);
        if (identity) {
            let start = idx;
            while (start > 0 && sameGroup(questions[start - 1], q)) start -= 1;
            let end = idx;
            while (end < questions.length - 1 && sameGroup(questions[end + 1], q)) end += 1;
            return { start, end, isGroup: true, key: identity.key };
        }

        return { start: idx, end: idx, isGroup: false, key: `q-${q.id}` };
    }

    function escapeHtml(value) {
        return String(value ?? '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function normalizeSelectedWord(value) {
        const parts = String(value || '')
            .match(/[A-Za-z]+(?:['’-][A-Za-z]+)?/g);

        return (parts || []).slice(0, 3).join(' ').trim();
    }

    function clearTextSelection() {
        const selection = window.getSelection?.();
        if (selection && selection.rangeCount > 0) {
            selection.removeAllRanges();
        }
    }

    function hideSelectionButton() {
        if (selectionHideTimer) {
            clearTimeout(selectionHideTimer);
            selectionHideTimer = null;
        }
        selectionVocabButton?.classList.add('hidden');
        selectionVocabButton?.classList.remove('flex');
        selectedVocabWord = '';
    }

    function scheduleSelectionButtonHide() {
        if (selectionHideTimer) {
            clearTimeout(selectionHideTimer);
        }
        selectionHideTimer = setTimeout(() => {
            hideSelectionButton();
        }, 120);
    }

    function showToast(message, isError = false) {
        if (!vocabToast) return;
        if (toastHideTimer) {
            clearTimeout(toastHideTimer);
        }
        vocabToast.textContent = message;
        vocabToast.classList.remove('hidden', 'bg-surface-container-high', 'text-on-surface', 'bg-error', 'text-on-primary');
        vocabToast.classList.add(isError ? 'bg-error' : 'bg-surface-container-high');
        vocabToast.classList.add(isError ? 'text-on-primary' : 'text-on-surface');
        toastHideTimer = setTimeout(() => {
            vocabToast.classList.add('hidden');
        }, 2800);
    }

    function getSelectionAnchorRect() {
        const selection = window.getSelection?.();
        if (!selection || selection.rangeCount === 0 || selection.isCollapsed) {
            return null;
        }

        const range = selection.getRangeAt(0);
        const rect = range.getBoundingClientRect();
        if (rect.width > 0 || rect.height > 0) {
            return rect;
        }

        const rects = range.getClientRects();
        return rects.length ? rects[0] : null;
    }

    function isSelectionInsideAllowedArea(selection) {
        if (!selection || selection.rangeCount === 0) return false;
        const range = selection.getRangeAt(0);
        const anchor = range.commonAncestorContainer;
        const target = anchor.nodeType === Node.ELEMENT_NODE ? anchor : anchor.parentElement;
        if (!target) return false;

        return !!target.closest('#question-text');
    }

    function updateSelectionButton() {
        if (mode !== 'practice' || !selectionVocabButton) return;

        const selection = window.getSelection?.();
        if (!selection || selection.isCollapsed || !isSelectionInsideAllowedArea(selection)) {
            hideSelectionButton();
            return;
        }

        const word = normalizeSelectedWord(selection.toString());
        if (!word) {
            hideSelectionButton();
            return;
        }

        const rect = getSelectionAnchorRect();
        if (!rect) {
            hideSelectionButton();
            return;
        }

        selectedVocabWord = word;
        selectionVocabButton.classList.remove('hidden');
        selectionVocabButton.classList.add('flex');
        selectionVocabButton.style.left = `${Math.min(window.innerWidth - 220, Math.max(16, rect.left + window.scrollX))}px`;
        selectionVocabButton.style.top = `${Math.max(16, rect.top + window.scrollY - 52)}px`;
    }

    async function addSelectedVocabulary() {
        const word = normalizeSelectedWord(selectedVocabWord || window.getSelection?.().toString());
        if (!word) {
            showToast('Please select a valid word first.', true);
            return;
        }

        hideSelectionButton();
        showToast(`Saving "${word}"...`);

        try {
            const res = await fetch('?handler=AddVocabulary', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ keyword: word })
            });
            const result = await res.json();

            if (!result.success) {
                showToast(result.error || 'Unable to save this word.', true);
                return;
            }

            if (result.status === 'already_saved') {
                showToast(`"${result.word || word}" is already in your notebook.`);
            } else {
                const meaning = result.meaning ? ` — ${result.meaning}` : '';
                showToast(`Saved "${result.word || word}"${meaning}`);
            }
            clearTextSelection();
        } catch (error) {
            console.error(error);
            showToast('Unable to save this word right now.', true);
        }
    }

    function renderPassagesHtml(passages) {
        return (passages || []).map((p, i) => {
            const blocks = [];
            if (p.imageUrl) {
                blocks.push(`<img src="${escapeHtml(p.imageUrl)}" alt="Passage ${i + 1}" class="max-h-96 rounded-lg mx-auto mb-3" />`);
            }
            if (p.content) {
                blocks.push(`<div class="text-body-md whitespace-pre-wrap">${escapeHtml(p.content)}</div>`);
            }
            if (!blocks.length) return '';
            const sep = i > 0 ? 'pt-4 mt-4 border-t border-outline-variant/20' : '';
            return `<div class="passage-block ${sep}">${blocks.join('')}</div>`;
        }).join('');
    }

    function snapToUnitStart(idx) {
        return getUnitRange(idx).start;
    }

    function getCurrentUnit() {
        return getUnitRange(currentIndex);
    }

    function getCurrentQuestion() {
        return questions[currentIndex];
    }

    function resolveUnitAudio(unit) {
        const head = questions[unit.start];
        if (!head) return null;

        // Part 3–4: prefer group-level audio
        if (unit.isGroup && isListeningPart(head.part)) {
            if (head.groupAudioUrl) {
                return { src: head.groupAudioUrl, start: null, end: null };
            }
            if (config.examAudioUrl && head.groupAudioStartSecond != null) {
                return {
                    src: config.examAudioUrl,
                    start: head.groupAudioStartSecond,
                    end: head.groupAudioEndSecond ?? null
                };
            }
        }

        // Part 1–2 (or fallback): question-level audio
        if (head.audioUrl) {
            return { src: head.audioUrl, start: null, end: null };
        }
        if (config.examAudioUrl && head.audioStartSecond != null) {
            return {
                src: config.examAudioUrl,
                start: head.audioStartSecond,
                end: head.audioEndSecond ?? null
            };
        }

        // Last resort: any question in unit with per-question audio
        for (let i = unit.start; i <= unit.end; i++) {
            const q = questions[i];
            if (q.audioUrl) return { src: q.audioUrl, start: null, end: null };
            if (config.examAudioUrl && q.audioStartSecond != null) {
                return {
                    src: config.examAudioUrl,
                    start: q.audioStartSecond,
                    end: q.audioEndSecond ?? null
                };
            }
        }

        return null;
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

    function setPracticePlayIcon(playing) {
        if (!audioPlayIcon) return;
        audioPlayIcon.textContent = playing ? 'pause' : 'play_arrow';
        if (audioPlayBtn) {
            audioPlayBtn.setAttribute('aria-label', playing ? 'Pause audio segment' : 'Play audio segment');
        }
    }

    function getSegmentBounds() {
        if (!activeSegment) return null;
        const start = Number(activeSegment.start) || 0;
        let end = activeSegment.end != null ? Number(activeSegment.end) : null;
        if (end == null && audioEl && Number.isFinite(audioEl.duration) && audioEl.duration > 0) {
            end = audioEl.duration;
        }
        if (end == null || !(end > start)) return { start, end: null, duration: null };
        return { start, end, duration: end - start };
    }

    function updatePracticeProgressUi() {
        if (!audioPlayerUi || !audioEl || !activeSegment) return;
        const bounds = getSegmentBounds();
        if (!bounds || bounds.duration == null) {
            if (audioTimeCurrentEl) audioTimeCurrentEl.textContent = '00:00';
            if (audioTimeDurationEl) audioTimeDurationEl.textContent = '--:--';
            if (audioProgressFill) audioProgressFill.style.width = '0%';
            return;
        }

        const rel = Math.min(bounds.duration, Math.max(0, audioEl.currentTime - bounds.start));
        const pct = (rel / bounds.duration) * 100;
        if (audioProgressFill) audioProgressFill.style.width = `${pct}%`;
        if (audioTimeCurrentEl) audioTimeCurrentEl.textContent = formatAudioClock(rel);
        if (audioTimeDurationEl) audioTimeDurationEl.textContent = formatAudioClock(bounds.duration);
        if (audioProgressTrack) {
            audioProgressTrack.setAttribute('aria-valuenow', String(Math.round(pct)));
            audioProgressTrack.setAttribute('aria-valuemax', '100');
        }
    }

    function setupPracticeSegmentUi(audioInfo) {
        if (!audioPlayerUi) return;

        const hasClip = audioInfo.start != null && audioInfo.start !== undefined;
        const start = hasClip ? Number(audioInfo.start) : 0;
        const end = audioInfo.end != null ? Number(audioInfo.end) : null;

        activeSegment = { start, end, duration: end != null && end > start ? end - start : null };
        practiceSegmentReady = false;
        setPracticePlayIcon(false);

        if (audioTimeRangeEl) {
            audioTimeRangeEl.textContent = 'Audio clip';
        }

        if (audioTimeCurrentEl) audioTimeCurrentEl.textContent = '00:00';
        if (audioTimeDurationEl) {
            audioTimeDurationEl.textContent = activeSegment.duration != null
                ? formatAudioClock(activeSegment.duration)
                : '--:--';
        }
        if (audioProgressFill) audioProgressFill.style.width = '0%';
    }

    function clearAudioWatchers() {
        if (segmentStopHandler && audioEl) {
            audioEl.removeEventListener('timeupdate', segmentStopHandler);
            segmentStopHandler = null;
        }
        if (segmentProgressHandler && audioEl) {
            audioEl.removeEventListener('timeupdate', segmentProgressHandler);
            segmentProgressHandler = null;
        }
        if (endedHandler && audioEl) {
            audioEl.removeEventListener('ended', endedHandler);
            endedHandler = null;
        }
        if (seekedHandler && audioEl) {
            audioEl.removeEventListener('seeked', seekedHandler);
            seekedHandler = null;
        }
        if (segmentPollTimer) {
            clearInterval(segmentPollTimer);
            segmentPollTimer = null;
        }
        if (segmentTimeout) {
            clearTimeout(segmentTimeout);
            segmentTimeout = null;
        }
    }

    function markUnitPlayed(unitKey) {
        playedUnitKeys.add(unitKey);
        audioCompleteForCurrent = true;
        saveLocalProgress();
    }

    function onUnitAudioFinished(unitKey) {
        if (mode === 'full') {
            if (playedUnitKeys.has(unitKey)) return;
            markUnitPlayed(unitKey);
            clearAudioWatchers();
            activePlaybackToken += 1; // invalidate any in-flight playback callbacks
            if (audioEl) {
                try { audioEl.pause(); } catch { /* ignore */ }
            }

            if (sectionPhase === 'listening') {
                // Defer so pause settles before seek/render of next unit
                setTimeout(() => {
                    try {
                        advanceListening();
                    } catch (err) {
                        console.error('advanceListening failed', err);
                        advancingFromAudio = false;
                    }
                }, 40);
            }
            return;
        }

        // Practice: stop at segment end and allow replay of the same clip
        audioCompleteForCurrent = true;
        if (audioEl) {
            try { audioEl.pause(); } catch { /* ignore */ }
            const bounds = getSegmentBounds();
            if (bounds?.end != null) {
                try { audioEl.currentTime = bounds.end; } catch { /* ignore */ }
            }
        }
        setPracticePlayIcon(false);
        updatePracticeProgressUi();
        if (segmentTimeout) {
            clearTimeout(segmentTimeout);
            segmentTimeout = null;
        }
    }

    function advanceListening() {
        if (advancingFromAudio) return;
        advancingFromAudio = true;

        try {
            const unit = getCurrentUnit();
            const nextIdx = unit.end + 1;

            if (nextIdx >= questions.length || (hasReading && nextIdx >= firstReadingIndex)) {
                enterReadingIntro();
                return;
            }

            currentIndex = snapToUnitStart(nextIdx);
            maxReachedIndex = Math.max(maxReachedIndex, getUnitRange(currentIndex).end);
            saveLocalProgress();
            renderQuestion();
        } finally {
            advancingFromAudio = false;
        }
    }

    function playUnitAudio(unit, audioInfo) {
        if (!audioEl || !audioInfo?.src) {
            audioCompleteForCurrent = true;
            return;
        }

        if (mode === 'full' && playedUnitKeys.has(unit.key)) {
            audioCompleteForCurrent = true;
            if (audioSection) audioSection.classList.remove('hidden');
            updateAudioTimeLabel(audioInfo.start, audioInfo.end);
            return;
        }

        clearAudioWatchers();
        audioCompleteForCurrent = false;
        const token = ++activePlaybackToken;
        const needsSegment = audioInfo.start != null && audioInfo.start !== undefined;
        const stopAt = audioInfo.end != null ? Number(audioInfo.end) : null;
        const startAt = needsSegment ? Number(audioInfo.start) : 0;

        if (mode === 'practice') {
            audioEl.controls = false;
            setupPracticeSegmentUi(audioInfo);
        }

        const finish = () => {
            if (token !== activePlaybackToken) return;
            onUnitAudioFinished(unit.key);
        };

        const armFinishWatchers = () => {
            if (token !== activePlaybackToken) return;

            segmentStopHandler = () => {
                if (token !== activePlaybackToken || !audioEl) return;
                if (stopAt != null && audioEl.currentTime >= stopAt - 0.05) {
                    finish();
                }
            };
            audioEl.addEventListener('timeupdate', segmentStopHandler);

            if (mode === 'practice') {
                segmentProgressHandler = () => {
                    if (token !== activePlaybackToken) return;
                    updatePracticeProgressUi();
                };
                audioEl.addEventListener('timeupdate', segmentProgressHandler);
            }

            segmentPollTimer = setInterval(() => {
                if (token !== activePlaybackToken || !audioEl) return;
                if (stopAt != null && !audioEl.paused && audioEl.currentTime >= stopAt - 0.05) {
                    finish();
                }
            }, 100);

            // Full test only: hard timeout advance (practice allows pause/replay)
            if (mode === 'full' && needsSegment && stopAt != null && stopAt > startAt) {
                const ms = Math.max(400, (stopAt - startAt) * 1000 + 250);
                segmentTimeout = setTimeout(finish, ms);
            }

            endedHandler = () => {
                if (token !== activePlaybackToken) return;
                finish();
            };
            audioEl.addEventListener('ended', endedHandler);
        };

        const startPlayback = () => {
            if (token !== activePlaybackToken) return;
            armFinishWatchers();

            if (mode === 'full') {
                audioEl.controls = false;
                const playPromise = audioEl.play();
                if (playPromise && typeof playPromise.catch === 'function') {
                    playPromise.catch(() => {
                        audioEl.controls = true;
                    });
                }
            } else {
                // Practice: clip is armed at start; user presses play on the segment UI
                audioEl.controls = false;
                practiceSegmentReady = true;
                try { audioEl.pause(); } catch { /* ignore */ }
                updatePracticeProgressUi();
            }
        };

        const beginPlayback = () => {
            if (token !== activePlaybackToken) return;

            // Resolve duration for short clip files (no start/end markers)
            if (mode === 'practice' && !needsSegment && audioEl.duration && Number.isFinite(audioEl.duration)) {
                activeSegment = { start: 0, end: audioEl.duration, duration: audioEl.duration };
                if (audioTimeDurationEl) audioTimeDurationEl.textContent = formatAudioClock(audioEl.duration);
                if (audioTimeRangeEl) audioTimeRangeEl.textContent = 'Clip';
            }

            if (needsSegment) {
                updateAudioTimeLabel(startAt, stopAt);

                seekedHandler = () => {
                    if (token !== activePlaybackToken) return;
                    audioEl.removeEventListener('seeked', seekedHandler);
                    seekedHandler = null;
                    startPlayback();
                };
                audioEl.addEventListener('seeked', seekedHandler);

                try {
                    // Pause before seek so continuous full-file playback cannot run past the segment
                    audioEl.pause();
                    audioEl.currentTime = startAt;
                } catch { /* ignore seek race */ }

                // If already at target (or seek completed sync), don't wait forever
                setTimeout(() => {
                    if (token !== activePlaybackToken || !seekedHandler) return;
                    if (Math.abs(audioEl.currentTime - startAt) < 0.2) {
                        audioEl.removeEventListener('seeked', seekedHandler);
                        seekedHandler = null;
                        startPlayback();
                    }
                }, 120);
            } else {
                updateAudioTimeLabel(null, null);
                startPlayback();
            }
        };

        if (activeAudioSrc !== audioInfo.src) {
            activeAudioSrc = audioInfo.src;
            audioEl.src = audioInfo.src;
            audioEl.load();
            const onMeta = () => {
                audioEl.removeEventListener('loadedmetadata', onMeta);
                beginPlayback();
            };
            audioEl.addEventListener('loadedmetadata', onMeta);
            if (audioEl.readyState >= 1) {
                audioEl.removeEventListener('loadedmetadata', onMeta);
                beginPlayback();
            }
        } else {
            beginPlayback();
        }
    }

    function hideAudioPlayer() {
        clearAudioWatchers();
        activePlaybackToken += 1;
        activeSegment = null;
        practiceSegmentReady = false;
        if (audioSection) audioSection.classList.add('hidden');
        if (audioLockedRow) audioLockedRow.classList.add('hidden');
        if (audioEl) audioEl.pause();
        setPracticePlayIcon(false);
        updateAudioTimeLabel(null, null);
    }

    function togglePracticeAudio() {
        if (mode !== 'practice' || !audioEl || !practiceSegmentReady || !activeSegment) return;
        const bounds = getSegmentBounds();
        if (!bounds) return;

        if (!audioEl.paused) {
            audioEl.pause();
            setPracticePlayIcon(false);
            return;
        }

        // Replay from start when at (or past) the clip end
        const atEnd = bounds.end != null && audioEl.currentTime >= bounds.end - 0.08;
        const beforeStart = audioEl.currentTime < bounds.start - 0.05;
        if (atEnd || beforeStart) {
            try { audioEl.currentTime = bounds.start; } catch { /* ignore */ }
        }

        const playPromise = audioEl.play();
        setPracticePlayIcon(true);
        if (playPromise && typeof playPromise.catch === 'function') {
            playPromise.catch(() => setPracticePlayIcon(false));
        }
    }

    function seekPracticeAudio(clientX) {
        if (mode !== 'practice' || !audioEl || !audioProgressTrack || !practiceSegmentReady) return;
        const bounds = getSegmentBounds();
        if (!bounds || bounds.duration == null) return;

        const rect = audioProgressTrack.getBoundingClientRect();
        if (rect.width <= 0) return;
        const ratio = Math.min(1, Math.max(0, (clientX - rect.left) / rect.width));
        const target = bounds.start + ratio * bounds.duration;
        try {
            audioEl.currentTime = Math.min(bounds.end - 0.01, Math.max(bounds.start, target));
        } catch { /* ignore */ }
        updatePracticeProgressUi();
    }

    // ── Navigation rules ────────────────────────────────────
    function canNavigateTo(idx) {
        if (idx < 0 || idx >= questions.length) return false;
        if (mode === 'practice') return true;

        const target = questions[idx];
        const unit = getCurrentUnit();
        const targetUnit = getUnitRange(idx);

        if (sectionPhase === 'listening') {
            // Within current unit: ok
            if (idx >= unit.start && idx <= unit.end) return true;
            // Past units: locked
            if (targetUnit.end < unit.start) return false;
            // Next unit only after current audio done
            if (targetUnit.start === unit.end + 1 && audioCompleteForCurrent) return true;
            return false;
        }

        if (sectionPhase === 'reading') {
            return !isListeningPart(target.part);
        }

        return false;
    }

    function canChangeAnswer(q) {
        if (mode === 'practice') return true;
        if (sectionPhase === 'reading') return true;
        if (sectionPhase === 'listening') {
            const unit = getCurrentUnit();
            const idx = questions.findIndex(x => x.id === q.id);
            return idx >= unit.start && idx <= unit.end;
        }
        return false;
    }

    function goToIndex(idx) {
        if (!canNavigateTo(idx)) return;

        const targetUnit = getUnitRange(idx);
        currentIndex = targetUnit.start;
        maxReachedIndex = Math.max(maxReachedIndex, targetUnit.end);

        saveLocalProgress();
        renderQuestion();
        closeSidebar();
    }

    function updateNavButtons() {
        const prevBtn = document.getElementById('btn-prev');
        const nextBtn = document.getElementById('btn-next');
        const readingNav = document.getElementById('reading-nav');
        const unit = getCurrentUnit();

        // Full test: Prev/Next only during Reading (Listening auto-advances)
        if (readingNav) {
            const showReadingNav = mode === 'full' && sectionPhase === 'reading';
            readingNav.classList.toggle('hidden', !showReadingNav);
            if (navHintEl) navHintEl.classList.toggle('hidden', showReadingNav);
        }

        if (!prevBtn && !nextBtn) return;

        const firstNavIndex = (mode === 'full' && sectionPhase === 'reading' && hasReading)
            ? firstReadingIndex
            : 0;
        const atFirst = unit.start <= firstNavIndex;
        const atLast = unit.end >= questions.length - 1;

        if (prevBtn) {
            prevBtn.disabled = atFirst;
            prevBtn.classList.toggle('opacity-40', atFirst);
        }
        if (nextBtn) {
            nextBtn.disabled = atLast;
            nextBtn.classList.toggle('opacity-40', atLast);
        }
    }

    // ── Render ──────────────────────────────────────────────
    function renderOptionButtons(q, container, answerLocked) {
        (q.options || []).forEach(opt => {
            const btn = document.createElement('button');
            btn.type = 'button';
            btn.dataset.answer = opt.letter;
            const selected = answers[q.id] === opt.letter;
            btn.disabled = answerLocked;
            btn.className = 'w-full text-left p-3 rounded-xl border-2 transition-all flex items-start gap-3 ' +
                (selected
                    ? 'border-primary bg-primary/5 ring-2 ring-primary/20'
                    : 'border-outline-variant/40 hover:border-primary/50') +
                (answerLocked ? ' opacity-60 cursor-not-allowed' : '');
            btn.innerHTML = `<span class="shrink-0 w-8 h-8 rounded-full bg-surface-container flex items-center justify-center font-bold">${opt.letter}</span><span class="pt-1">${opt.text || ''}</span>`;
            if (!answerLocked) {
                btn.addEventListener('click', () => selectAnswer(q.id, opt.letter));
            }
            container.appendChild(btn);
        });
    }

    function renderQuestion() {
        hideSelectionButton();
        clearTextSelection();
        const unit = getCurrentUnit();
        currentIndex = unit.start;
        const head = questions[unit.start];
        if (!head) return;

        const nums = [];
        for (let i = unit.start; i <= unit.end; i++) nums.push(questions[i].questionNumber);

        // 1) Update ALL visible UI before touching audio
        if (questionLabelEl) {
            questionLabelEl.textContent = unit.isGroup
                ? `Questions ${nums[0]}–${nums[nums.length - 1]}`
                : `Question ${head.questionNumber}`;
        }
        if (currentQNumEl) currentQNumEl.textContent = String(head.questionNumber);
        if (partLabelEl) partLabelEl.textContent = `Part ${head.part}`;

        if (navHintEl && mode === 'full') {
            navHintEl.textContent = sectionPhase === 'listening'
                ? (unit.isGroup
                    ? 'Part 3/4 — answer all 3 questions while audio plays. Advances to the next set when audio ends.'
                    : 'Listening — answer while audio plays. Advances automatically when audio ends.')
                : (unit.isGroup && isReadingGroupPart(head.part)
                    ? 'Part 6/7 — read the passage(s), then answer all questions in this set. Use Prev/Next or the question list.'
                    : 'Use Prev/Next or the question list to move between reading questions.');
        }

        const passages = head.passages || [];
        const isReadingSet = unit.isGroup && isReadingGroupPart(head.part) && passages.length > 0;
        const stimulusImage = !isReadingSet ? (head.groupImageUrl || head.imageUrl) : null;

        if (questionImageWrap && questionImageEl) {
            if (stimulusImage) {
                questionImageWrap.classList.remove('hidden');
                // Force reload so consecutive Part 1 images always refresh
                questionImageEl.src = '';
                questionImageEl.src = stimulusImage;
            } else {
                questionImageWrap.classList.add('hidden');
                questionImageEl.removeAttribute('src');
            }
        }

        if (questionTextEl) {
            if (isReadingSet) {
                questionTextEl.innerHTML = renderPassagesHtml(passages);
            } else if (unit.isGroup) {
                // Part 3/4: stimulus is audio + optional group image only
                questionTextEl.textContent = '';
            } else {
                questionTextEl.textContent = head.questionText || '';
            }
        }

        if (optionsEl) {
            optionsEl.innerHTML = '';
            for (let i = unit.start; i <= unit.end; i++) {
                const q = questions[i];
                const answerLocked = mode === 'full' && sectionPhase === 'listening' && !canChangeAnswer(q);

                const block = document.createElement('div');
                block.className = 'space-y-2' + (i > unit.start ? ' pt-4 border-t border-outline-variant/20' : '');
                block.dataset.questionId = String(q.id);

                if (unit.isGroup) {
                    const qHead = document.createElement('div');
                    qHead.className = 'text-label-sm font-bold text-on-surface mb-2';
                    qHead.textContent = `${q.questionNumber}. ${q.questionText || ''}`.trim();
                    block.appendChild(qHead);
                }

                const optsWrap = document.createElement('div');
                optsWrap.className = 'space-y-2';
                renderOptionButtons(q, optsWrap, answerLocked);
                block.appendChild(optsWrap);
                optionsEl.appendChild(block);
            }
        }

        updatePaletteHighlight();
        updateNavButtons();

        // 2) Start audio after UI is committed
        if (audioEl && isListeningPart(head.part) && (mode !== 'full' || sectionPhase === 'listening' || mode === 'practice')) {
            const audioInfo = resolveUnitAudio(unit);
            if (audioInfo) {
                if (audioSection) audioSection.classList.remove('hidden');
                if (audioLockedRow) audioLockedRow.classList.toggle('hidden', mode !== 'full');
                playUnitAudio(unit, audioInfo);
            } else {
                hideAudioPlayer();
                audioCompleteForCurrent = true;
            }
        } else if (!isListeningPart(head.part)) {
            hideAudioPlayer();
            audioCompleteForCurrent = true;
        }
    }

    function buildPalette() {
        if (!paletteEl) return;
        paletteEl.innerHTML = '';

        const byPart = new Map();
        questions.forEach((q, idx) => {
            if (!byPart.has(q.part)) byPart.set(q.part, []);
            byPart.get(q.part).push({ q, idx });
        });

        const displayUnit = getCurrentUnit();
        const listeningActiveUnit = (mode === 'full' && sectionPhase === 'listening') ? displayUnit : null;

        [...byPart.keys()].sort((a, b) => a - b).forEach(part => {
            const group = document.createElement('div');
            group.className = 'space-y-2';

            const heading = document.createElement('div');
            heading.className = 'text-label-sm font-bold text-on-surface-variant uppercase tracking-wide px-1';
            heading.textContent = `Part ${part}`;
            group.appendChild(heading);

            const grid = document.createElement('div');
            grid.className = 'flex flex-wrap gap-1.5';

            byPart.get(part).forEach(({ q, idx }) => {
                const cell = document.createElement('button');
                cell.type = 'button';
                cell.dataset.qIdx = String(idx);
                cell.textContent = String(q.questionNumber);

                const answered = !!answers[q.id];
                const inDisplayUnit = idx >= displayUnit.start && idx <= displayUnit.end;
                const targetUnit = getUnitRange(idx);
                const isHeardLocked = mode === 'full' && sectionPhase === 'listening' && targetUnit.end < (listeningActiveUnit?.start ?? 0);
                const navigable = canNavigateTo(idx);

                let cls = 'q-cell rounded-full border text-[11px] font-medium flex items-center justify-center transition-all ';
                if (inDisplayUnit) {
                    cls += 'bg-primary text-on-primary border-primary ring-2 ring-primary/30 ';
                } else if (isHeardLocked) {
                    cls += 'bg-outline-variant/40 text-on-surface-variant/50 border-transparent cursor-not-allowed ';
                } else if (answered) {
                    cls += 'bg-tertiary/20 text-tertiary border-tertiary/30 font-bold ';
                } else {
                    cls += 'bg-[#9e9e9e] text-white border-transparent ';
                }

                if (!navigable && !inDisplayUnit) cls += 'opacity-50 cursor-not-allowed ';
                else if (navigable) cls += 'hover:scale-105 ';

                cell.className = cls;
                cell.disabled = !navigable && !inDisplayUnit;
                cell.title = isHeardLocked ? 'Already played — locked' : `Question ${q.questionNumber}`;
                cell.addEventListener('click', () => goToIndex(snapToUnitStart(idx)));
                grid.appendChild(cell);
            });

            group.appendChild(grid);
            paletteEl.appendChild(group);
        });
    }

    function updatePaletteHighlight() {
        buildPalette();
    }

    function updateAnsweredCount() {
        const count = Object.keys(answers).length;
        if (answeredCountEl) answeredCountEl.textContent = String(count);
        if (answeredCountBadge) answeredCountBadge.textContent = String(count);
        buildPalette();
    }

    async function selectAnswer(questionId, letter) {
        const q = questions.find(x => x.id === questionId);
        if (!q || !canChangeAnswer(q)) return;

        answers[questionId] = letter;
        updateAnsweredCount();

        // Re-highlight options without restarting audio
        if (optionsEl) {
            const unit = getCurrentUnit();
            optionsEl.innerHTML = '';
            for (let i = unit.start; i <= unit.end; i++) {
                const qq = questions[i];
                const answerLocked = mode === 'full' && sectionPhase === 'listening' && !canChangeAnswer(qq);
                const block = document.createElement('div');
                block.className = 'space-y-2' + (i > unit.start ? ' pt-4 border-t border-outline-variant/20' : '');
                if (unit.isGroup) {
                    const qHead = document.createElement('div');
                    qHead.className = 'text-label-sm font-bold text-on-surface mb-2';
                    qHead.textContent = `${qq.questionNumber}. ${qq.questionText || ''}`.trim();
                    block.appendChild(qHead);
                }
                const optsWrap = document.createElement('div');
                optsWrap.className = 'space-y-2';
                renderOptionButtons(qq, optsWrap, answerLocked);
                block.appendChild(optsWrap);
                optionsEl.appendChild(block);
            }
        }

        if (mode === 'full' && config.attemptId) {
            try {
                await fetch('?handler=SaveAnswer', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ attemptId: config.attemptId, questionId, selectedOption: letter })
                });
            } catch (e) { console.error(e); }
        }
        // Do NOT auto-advance on answer — IIG advances only when audio ends
    }

    async function saveProgress() {
        if (mode !== 'full' || !config.attemptId) return;
        saveLocalProgress();
        try {
            await fetch('?handler=SaveProgress', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ attemptId: config.attemptId, remainingSeconds: remaining })
            });
        } catch (e) { console.error(e); }
    }

    // ── Section intros ──────────────────────────────────────
    function showSectionIntro(kind) {
        if (!sectionIntroEl) return;
        stopTimer();
        hideAudioPlayer();
        closeSidebar();

        const icon = document.getElementById('section-intro-icon');
        const eyebrow = document.getElementById('section-intro-eyebrow');
        const title = document.getElementById('section-intro-title');
        const desc = document.getElementById('section-intro-desc');
        const rules = document.getElementById('section-intro-rules');
        const startBtn = document.getElementById('btn-section-start');

        if (kind === 'listening') {
            sectionPhase = 'intro-listening';
            if (icon) icon.textContent = 'headphones';
            if (eyebrow) eyebrow.textContent = 'Section 1';
            if (title) title.textContent = 'Listening Test';
            if (desc) desc.textContent = 'Parts 1–4 · 45 minutes';
            if (rules) {
                rules.innerHTML = `
                    <li>• Audio plays once (TOEIC IIG style).</li>
                    <li>• Parts 3–4 show 3 questions per conversation/talk.</li>
                    <li>• When audio ends, you move to the next question or set.</li>
                    <li>• You cannot revisit questions you have already heard.</li>`;
            }
            if (startBtn) startBtn.textContent = 'Start Listening';
            remaining = Math.min(remaining > 0 && remaining <= LISTENING_SECONDS ? remaining : LISTENING_SECONDS, LISTENING_SECONDS);
        } else {
            sectionPhase = 'intro-reading';
            if (icon) icon.textContent = 'menu_book';
            if (eyebrow) eyebrow.textContent = 'Section 2';
            if (title) title.textContent = 'Reading Test';
            if (desc) desc.textContent = 'Parts 5–7 · 75 minutes';
            if (rules) {
                rules.innerHTML = `
                    <li>• You may freely move between reading questions with Prev/Next.</li>
                    <li>• Parts 6–7 show the passage with all questions in the set.</li>
                    <li>• Use the sidebar to jump to any Part 5–7 question.</li>
                    <li>• Timer starts when you press Start.</li>`;
            }
            if (startBtn) startBtn.textContent = 'Start Reading';
            remaining = READING_SECONDS;
        }

        updateTimer();
        sectionIntroEl.classList.remove('hidden');
        if (examWorkspace) examWorkspace.classList.add('invisible', 'pointer-events-none');
        document.getElementById('btn-sidebar-toggle')?.classList.add('opacity-40', 'pointer-events-none');
        saveLocalProgress();
    }

    function hideSectionIntro() {
        if (sectionIntroEl) sectionIntroEl.classList.add('hidden');
        if (examWorkspace) examWorkspace.classList.remove('invisible', 'pointer-events-none');
        document.getElementById('btn-sidebar-toggle')?.classList.remove('opacity-40', 'pointer-events-none');
    }

    function enterReadingIntro() {
        hideAudioPlayer();
        if (!hasReading) {
            autoSubmit();
            return;
        }
        currentIndex = firstReadingIndex;
        maxReachedIndex = Math.max(maxReachedIndex, firstReadingIndex);
        showSectionIntro('reading');
    }

    function startListeningSection() {
        sectionPhase = 'listening';
        hideSectionIntro();
        if (hasListening) {
            const firstListen = questions.findIndex(q => q.part <= 4);
            if (firstReadingIndex >= 0) {
                currentIndex = Math.min(currentIndex, firstReadingIndex - 1);
            }
            if (!isListeningPart(questions[currentIndex]?.part)) {
                currentIndex = firstListen >= 0 ? firstListen : 0;
            }
            currentIndex = snapToUnitStart(currentIndex);
            maxReachedIndex = Math.max(maxReachedIndex, getUnitRange(currentIndex).end);
        }
        remaining = remaining > 0 && remaining <= LISTENING_SECONDS ? remaining : LISTENING_SECONDS;
        saveLocalProgress();
        renderQuestion();
        startTimer();
    }

    function startReadingSection() {
        sectionPhase = 'reading';
        hideSectionIntro();
        if (hasReading) {
            currentIndex = Math.max(currentIndex, firstReadingIndex);
            if (isListeningPart(questions[currentIndex]?.part)) {
                currentIndex = firstReadingIndex;
            }
        }
        remaining = READING_SECONDS;
        saveLocalProgress();
        renderQuestion();
        startTimer();
        saveProgress();
    }

    function onSectionTimeUp() {
        if (sectionPhase === 'listening') {
            const timeoutMsg = document.getElementById('timeout-message');
            if (timeoutMsg) timeoutMsg.textContent = 'Listening time is over. Moving to Reading…';
            const modal = document.getElementById('modal-timeout');
            if (modal) {
                modal.classList.remove('hidden');
                setTimeout(() => {
                    modal.classList.add('hidden');
                    enterReadingIntro();
                }, 1800);
            } else {
                enterReadingIntro();
            }
            return;
        }
        autoSubmit();
    }

    async function autoSubmit() {
        const modal = document.getElementById('modal-timeout');
        const timeoutMsg = document.getElementById('timeout-message');
        if (timeoutMsg) timeoutMsg.textContent = "Time's up! Grading your test…";
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
                    clearLocalProgress();
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
        document.getElementById(id)?.classList.remove('hidden');
    }
    function hideModal(id) {
        document.getElementById(id)?.classList.add('hidden');
    }

    function openSidebar() {
        sidebarEl?.classList.add('is-open');
        sidebarEl?.classList.remove('-translate-x-full');
        sidebarBackdrop?.classList.remove('hidden');
    }
    function closeSidebar() {
        sidebarEl?.classList.remove('is-open');
        sidebarEl?.classList.add('-translate-x-full');
        sidebarBackdrop?.classList.add('hidden');
    }
    function toggleSidebar() {
        if (!sidebarEl) return;
        if (sidebarEl.classList.contains('is-open')) closeSidebar();
        else openSidebar();
    }

    // ── Events ──────────────────────────────────────────────
    document.getElementById('btn-sidebar-toggle')?.addEventListener('click', toggleSidebar);
    document.getElementById('btn-sidebar-close')?.addEventListener('click', closeSidebar);
    sidebarBackdrop?.addEventListener('click', closeSidebar);

    audioPlayBtn?.addEventListener('click', togglePracticeAudio);
    audioProgressTrack?.addEventListener('click', (e) => seekPracticeAudio(e.clientX));
    audioEl?.addEventListener('pause', () => {
        if (mode === 'practice') setPracticePlayIcon(false);
    });
    audioEl?.addEventListener('play', () => {
        if (mode === 'practice') setPracticePlayIcon(true);
    });

    document.getElementById('btn-section-start')?.addEventListener('click', () => {
        if (sectionPhase === 'intro-listening') startListeningSection();
        else if (sectionPhase === 'intro-reading') startReadingSection();
    });

    document.getElementById('btn-next')?.addEventListener('click', () => {
        const unit = getCurrentUnit();
        if (unit.end < questions.length - 1) goToIndex(unit.end + 1);
    });
    document.getElementById('btn-prev')?.addEventListener('click', () => {
        const unit = getCurrentUnit();
        if (unit.start > 0) goToIndex(snapToUnitStart(unit.start - 1));
    });

    document.getElementById('btn-submit')?.addEventListener('click', () => showModal('modal-submit'));
    document.querySelectorAll('[data-close-submit]').forEach(b => b.addEventListener('click', () => hideModal('modal-submit')));
    document.querySelectorAll('[data-confirm-submit]').forEach(b => b.addEventListener('click', () => submitTest()));

    document.getElementById('btn-exit')?.addEventListener('click', (e) => {
        e.preventDefault();
        showModal('modal-leave');
    });
    document.querySelectorAll('[data-close-leave]').forEach(b => b.addEventListener('click', () => hideModal('modal-leave')));
    document.querySelector('[data-confirm-leave]')?.addEventListener('click', async () => {
        isSubmitting = true;
        if (mode === 'full') {
            await saveProgress();
            window.location.href = `/Exams/Detail/${config.examId}`;
        } else {
            const exitBtn = document.getElementById('btn-exit');
            window.location.href = exitBtn?.dataset?.exitUrl || `/Exams/Detail/${config.examId}`;
        }
    });

    selectionVocabButton?.addEventListener('mousedown', (e) => e.preventDefault());
    selectionVocabButton?.addEventListener('click', addSelectedVocabulary);
    questionTextEl?.addEventListener('mouseup', () => setTimeout(updateSelectionButton, 0));
    questionTextEl?.addEventListener('touchend', () => setTimeout(updateSelectionButton, 0));
    questionTextEl?.addEventListener('keyup', () => setTimeout(updateSelectionButton, 0));
    selectionVocabButton?.addEventListener('blur', scheduleSelectionButtonHide);
    selectionVocabButton?.addEventListener('mouseenter', () => {
        if (selectionHideTimer) {
            clearTimeout(selectionHideTimer);
            selectionHideTimer = null;
        }
    });
    selectionVocabButton?.addEventListener('mouseleave', scheduleSelectionButtonHide);

    document.addEventListener('selectionchange', () => {
        if (mode !== 'practice') return;
        setTimeout(updateSelectionButton, 0);
    });
    document.addEventListener('click', (e) => {
        if (!(e.target instanceof Element)) {
            hideSelectionButton();
            return;
        }

        if (e.target.closest('#selection-vocab-button') || e.target.closest('#question-text')) {
            return;
        }

        hideSelectionButton();
    });

    window.addEventListener('beforeunload', (e) => {
        if (isSubmitting) return;
        if (mode === 'full') saveProgress();
        if (mode === 'practice' && Object.keys(answers).length > 0) {
            e.preventDefault();
            e.returnValue = 'Your practice progress will not be saved. Are you sure you want to leave?';
            return e.returnValue;
        }
    });

    // ── Bootstrap ───────────────────────────────────────────
    function bootstrapFullTest() {
        currentIndex = snapToUnitStart(currentIndex);
        const saved = loadProgress();
        if (saved) {
            if (typeof saved.maxReachedIndex === 'number') {
                maxReachedIndex = Math.max(maxReachedIndex, saved.maxReachedIndex);
            }
            if (Array.isArray(saved.playedUnits)) {
                playedUnitKeys = new Set(saved.playedUnits);
            } else if (Array.isArray(saved.finishedSegments)) {
                // ignore legacy keys
            }
            if (typeof saved.currentIndex === 'number') {
                currentIndex = snapToUnitStart(saved.currentIndex);
            }
            if (saved.sectionPhase === 'reading' || saved.sectionPhase === 'intro-reading') {
                if (saved.sectionPhase === 'reading' && remaining > 0) {
                    sectionPhase = 'reading';
                    if (hasReading && currentIndex < firstReadingIndex) currentIndex = firstReadingIndex;
                    hideSectionIntro();
                    renderQuestion();
                    updateAnsweredCount();
                    startTimer();
                    return;
                }
                showSectionIntro('reading');
                updateAnsweredCount();
                return;
            }
            if (saved.sectionPhase === 'listening') {
                sectionPhase = 'listening';
                if (remaining > LISTENING_SECONDS) remaining = LISTENING_SECONDS;
                hideSectionIntro();
                renderQuestion();
                updateAnsweredCount();
                startTimer();
                return;
            }
        }

        const q = questions[currentIndex];
        const anyReadingAnswer = questions.some(qq => qq.part >= 5 && answers[qq.id]);

        if (anyReadingAnswer || (q && q.part >= 5)) {
            if (remaining > READING_SECONDS) remaining = READING_SECONDS;
            showSectionIntro('reading');
            updateAnsweredCount();
            return;
        }

        if (maxReachedIndex > 0 || Object.keys(answers).length > 0) {
            if (remaining > LISTENING_SECONDS) remaining = LISTENING_SECONDS;
            sectionPhase = 'listening';
            hideSectionIntro();
            renderQuestion();
            updateAnsweredCount();
            startTimer();
            return;
        }

        if (remaining > LISTENING_SECONDS || remaining <= 0) {
            remaining = LISTENING_SECONDS;
        }
        showSectionIntro('listening');
        updateAnsweredCount();
    }

    function bootstrapPractice() {
        sectionPhase = 'active';
        currentIndex = snapToUnitStart(currentIndex);
        hideSectionIntro();
        buildPalette();
        updateAnsweredCount();
        renderQuestion();
        if (remaining > 0) startTimer();
    }

    if (mode === 'full') {
        bootstrapFullTest();
    } else {
        bootstrapPractice();
    }
})();
