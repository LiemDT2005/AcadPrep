// Track selected parts for practice
// Tự động cập nhật tóm tắt khi trang vừa tải — mặc định tick tất cả Part
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('input.part-checkbox').forEach(cb => {
        if (!cb.hasAttribute('data-initialized')) {
            cb.checked = true;
            cb.setAttribute('data-initialized', 'true');
        }
        syncPartCardStyle(parseInt(cb.getAttribute('data-part')));
    });
    updateSummary();
});

function syncPartCardStyle(partNumber) {
    const card = document.getElementById('part-card-' + partNumber);
    const partCheck = document.getElementById('part-check-' + partNumber);
    if (!card || !partCheck) return;
    if (partCheck.checked) {
        card.classList.add('border-primary', 'bg-primary/5');
        card.classList.remove('border-outline-variant/30');
    } else {
        card.classList.remove('border-primary', 'bg-primary/5');
        card.classList.add('border-outline-variant/30');
    }
}

function switchTab(tabName) {
    // Hide all tab contents
    document.getElementById('content-practice').classList.add('hidden');
    document.getElementById('content-fulltest').classList.add('hidden');
    document.getElementById('content-info').classList.add('hidden');
    
    // Remove active styles from all buttons
    const buttons = ['practice', 'fulltest', 'info'];
    buttons.forEach(btn => {
        const el = document.getElementById('tab-' + btn);
        if (el) {
            el.classList.remove('text-primary', 'font-bold', 'border-b-2', 'border-primary');
            el.classList.add('text-on-surface-variant');
        }
    });
    
    // Show selected content
    const activeContent = document.getElementById('content-' + tabName);
    if (activeContent) {
        activeContent.classList.remove('hidden');
    }
    
    // Add active styles to selected button
    const activeEl = document.getElementById('tab-' + tabName);
    if (activeEl) {
        activeEl.classList.add('text-primary', 'font-bold', 'border-b-2', 'border-primary');
        activeEl.classList.remove('text-on-surface-variant');
    }
}

// Xử lý Checkbox: Click card tự động tick check Part
function togglePartCheckbox(partNumber, isCheckboxClicked = false) {
    const partCheck = document.getElementById('part-check-' + partNumber);
    if (!partCheck) return;

    // Nếu không phải click trực tiếp vào checkbox thì ta đảo trạng thái checkbox
    if (!isCheckboxClicked) {
        partCheck.checked = !partCheck.checked;
    }

    // Đồng bộ bật/tắt toàn bộ checkbox nhãn con thuộc Part đó
    const tagChecks = document.querySelectorAll(`input.tag-checkbox[data-parent-part="${partNumber}"]`);
    tagChecks.forEach(cb => {
        cb.checked = partCheck.checked;
    });

    syncPartCardStyle(partNumber);
    updateSummary();
}

// Khi thay đổi checkbox nhãn con, nếu không có nhãn con nào được chọn thì tắt checkbox Part cha
function syncParentPartCheckbox(partNumber) {
    const partCheck = document.getElementById('part-check-' + partNumber);
    if (!partCheck) return;

    const tagChecks = document.querySelectorAll(`input.tag-checkbox[data-parent-part="${partNumber}"]`);
    const anyTagChecked = Array.from(tagChecks).some(cb => cb.checked);

    // Chọn tag → bật Part cha; bỏ hết tag → giữ nguyên trạng thái Part (luyện cả Part)
    if (anyTagChecked) {
        partCheck.checked = true;
    }

    syncPartCardStyle(partNumber);
    updateSummary();
}

function updateSummary() {
    let totalQuestions = 0;
    let estimatedMins = 0;
    let selectedPartsCount = 0;

    const partCheckboxes = document.querySelectorAll('input.part-checkbox');
    partCheckboxes.forEach(cb => {
        if (cb.checked) {
            selectedPartsCount++;
            const partNum = parseInt(cb.getAttribute('data-part'));
            const part = window.EXAM_CONFIG && window.EXAM_CONFIG.partsData ? window.EXAM_CONFIG.partsData[partNum] : null;
            if (part) {
                totalQuestions += part.questionCount;
                if (partNum <= 4) {
                    estimatedMins += Math.ceil((part.questionCount * 45) / 60);
                } else {
                    estimatedMins += part.questionCount; 
                }
            }
        }
    });

    const summaryText = selectedPartsCount === 0
        ? 'No parts selected — please select at least one part'
        : `${selectedPartsCount} Parts Selected | ${totalQuestions} Questions | ~${estimatedMins} mins`;
    const summaryEl = document.getElementById('selection-summary');
    if (summaryEl) {
        summaryEl.innerText = summaryText;
    }
}

async function startPracticeSession() {
    const examId = window.EXAM_CONFIG ? window.EXAM_CONFIG.examId : 0;
    
    // Thu thập các Part được tích chọn
    const selectedParts = [];
    document.querySelectorAll('input.part-checkbox:checked').forEach(cb => {
        selectedParts.push(parseInt(cb.getAttribute('data-part')));
    });

    // Thu thập các Tag được tích chọn
    const selectedTags = [];
    document.querySelectorAll('input.tag-checkbox:checked').forEach(cb => {
        selectedTags.push(cb.value);
    });

    // Lấy thời gian giới hạn
    const timeLimitVal = document.getElementById('customTimeLimit').value;
    const timeLimitMinutes = timeLimitVal ? parseInt(timeLimitVal) : null;

    if (selectedParts.length === 0) {
        alert("Please select at least one part to practice.");
        return;
    }

    const payload = {
        examId: examId,
        selectedPartNumbers: selectedParts,
        selectedTags: selectedTags,
        timeLimitMinutes: timeLimitMinutes
    };

    const btn = document.querySelector('button[onclick="startPracticeSession()"]');
    if (btn) {
        btn.disabled = true;
        btn.innerHTML = `<span class="animate-spin inline-block w-4 h-4 border-2 border-current border-t-transparent rounded-full mr-2"></span> Starting...`;
    }

    try {
        const response = await fetch('?handler=StartPractice', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(payload)
        });

        const result = await response.json();
        if (result.success) {
            // Chuyển hướng sang trang làm bài chi tiết với SessionId vừa nhận
            window.location.href = `/Exams/Practice?sessionId=${result.sessionId}`;
        } else {
            alert("Error: " + result.error);
            if (btn) {
                btn.disabled = false;
                btn.innerHTML = `START PRACTICE <span class="material-symbols-outlined">play_arrow</span>`;
            }
        }
    } catch (err) {
        console.error(err);
        alert("A connection error occurred. Please try again later.");
        if (btn) {
            btn.disabled = false;
            btn.innerHTML = `START PRACTICE <span class="material-symbols-outlined">play_arrow</span>`;
        }
    }
}

function clearAttemptLocalProgress(attemptId) {
    if (!attemptId) return;
    try {
        localStorage.removeItem(`acadprep-exam-${attemptId}`);
    } catch { /* ignore */ }
}

async function postStartFullTest(examId, startNewAttempt) {
    const response = await fetch('?handler=StartFullTest', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ examId, startNewAttempt })
    });
    return response.json();
}

async function startFullTestSession() {
    const confirmed = confirm(
        'Start Full Test?\n\n' +
        '• 120-minute countdown timer\n' +
        '• Audio P1-4: no rewind or pause\n' +
        '• Score is recorded only when you submit or time runs out'
    );
    if (!confirmed) return;

    let startNewAttempt = false;
    const inProgressAttemptId = window.EXAM_CONFIG ? window.EXAM_CONFIG.inProgressAttemptId : null;
    if (inProgressAttemptId) {
        const abandon = confirm(
            'You already have an unfinished test.\n\n' +
            'Start a NEW attempt? Your previous progress will be permanently discarded.\n\n' +
            'Tip: Cancel and use "Resume Test" if you want to continue the current attempt.'
        );
        if (!abandon) return;
        startNewAttempt = true;
    }

    const examId = window.EXAM_CONFIG ? window.EXAM_CONFIG.examId : 0;
    try {
        let result = await postStartFullTest(examId, startNewAttempt);

        // Race / stale page: server still has an in-progress attempt
        if (!result.success && result.inProgressAttemptId && !startNewAttempt) {
            const abandon = confirm(
                (result.error || 'You have an unfinished test.') + '\n\n' +
                'Start a NEW attempt? Your previous progress will be permanently discarded.\n\n' +
                'Tip: Cancel and use "Resume Test" if you want to continue.'
            );
            if (!abandon) return;
            result = await postStartFullTest(examId, true);
        }

        if (result.success) {
            clearAttemptLocalProgress(result.abandonedAttemptId || inProgressAttemptId);
            window.location.href = `/Exams/Take?attemptId=${result.attemptId}`;
        } else {
            alert('Error: ' + (result.error || 'Could not start the test.'));
        }
    } catch (err) {
        console.error(err);
        alert('A connection error occurred.');
    }
}

function resumeTest(attemptId) {
    window.location.href = `/Exams/Take?attemptId=${attemptId}`;
}
