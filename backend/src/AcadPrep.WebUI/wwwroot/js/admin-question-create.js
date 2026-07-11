/**
 * Shared UI helpers for admin question creation pages.
 */
window.AcadPrepQuestionUI = {
    previewOptionDefault: 'qc-preview-option',
    previewOptionCorrect: 'qc-preview-option qc-preview-option--correct',
    previewBadgeDefault: 'qc-preview-option-badge',
    previewBadgeCorrect: 'qc-preview-option-badge',

    applyPreviewOption(card, badge, isCorrect) {
        if (!card || !badge) return;
        card.className = isCorrect ? this.previewOptionCorrect : this.previewOptionDefault;
        badge.className = this.previewBadgeDefault;
    },

    setStepperActive(stepNumber, totalSteps) {
        for (let i = 1; i <= totalSteps; i++) {
            const indicator = document.getElementById('stepIndicator' + i);
            const icon = document.getElementById('stepIcon' + i);
            const text = document.getElementById('stepText' + i);
            if (!indicator || !icon) continue;

            indicator.classList.remove('qc-stepper__step--active', 'qc-stepper__step--done', 'opacity-50');
            icon.classList.remove('qc-stepper__icon');

            if (i < stepNumber) {
                indicator.classList.add('qc-stepper__step--done');
                icon.className = 'qc-stepper__icon material-symbols-outlined text-[16px]';
                icon.textContent = 'check';
                if (text) text.className = 'qc-stepper__label text-on-surface';
            } else if (i === stepNumber) {
                indicator.classList.add('qc-stepper__step--active');
                icon.className = 'qc-stepper__icon';
                icon.textContent = String(i);
                if (text) text.className = 'qc-stepper__label font-semibold text-on-surface';
            } else {
                indicator.classList.add('opacity-50');
                icon.className = 'qc-stepper__icon';
                icon.textContent = String(i);
                if (text) text.className = 'qc-stepper__label';
            }

            const connector = document.getElementById('stepConnector' + i);
            if (connector) {
                connector.classList.toggle('qc-stepper__connector--done', i < stepNumber);
            }
        }
    }
};
