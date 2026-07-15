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
    },

    /**
     * Clear a file input + related preview UI.
     * @param {object} opts
     * @param {HTMLInputElement|string|null} opts.fileInput
     * @param {HTMLElement|string|null} [opts.removeBtn]
     * @param {HTMLElement|string|null} [opts.previewContainer]
     * @param {HTMLImageElement|string|null} [opts.previewImg]
     * @param {HTMLAudioElement|string|null} [opts.audioEl]
     * @param {HTMLInputElement|string|null} [opts.hiddenUrl]
     * @param {HTMLElement|string|null} [opts.icon]
     * @param {HTMLElement|string|null} [opts.label]
     * @param {string} [opts.defaultIcon]
     * @param {string} [opts.defaultLabel]
     * @param {HTMLElement|string|null} [opts.extraHide]
     * @param {string|null} [opts.objectUrl] current object URL to revoke
     * @param {function|null} [opts.onCleared]
     * @returns {null} always returns null (cleared object URL)
     */
    clearMedia(opts) {
        const el = (ref) => {
            if (!ref) return null;
            return typeof ref === 'string' ? document.getElementById(ref) : ref;
        };

        const fileInput = el(opts.fileInput);
        const removeBtn = el(opts.removeBtn);
        const previewContainer = el(opts.previewContainer);
        const previewImg = el(opts.previewImg);
        const audioEl = el(opts.audioEl);
        const hiddenUrl = el(opts.hiddenUrl);
        const icon = el(opts.icon);
        const label = el(opts.label);
        const extraHide = el(opts.extraHide);

        if (opts.objectUrl) {
            try { URL.revokeObjectURL(opts.objectUrl); } catch (_) { /* ignore */ }
        }

        if (fileInput) {
            fileInput.value = '';
        }
        if (hiddenUrl) {
            hiddenUrl.value = '';
        }
        if (previewImg) {
            previewImg.removeAttribute('src');
        }
        if (audioEl) {
            audioEl.pause?.();
            audioEl.removeAttribute('src');
            audioEl.load?.();
        }
        if (previewContainer) {
            previewContainer.classList.add('hidden');
        }
        if (extraHide) {
            extraHide.classList.add('hidden');
        }
        if (removeBtn) {
            removeBtn.classList.add('hidden');
        }
        if (icon && opts.defaultIcon) {
            icon.textContent = opts.defaultIcon;
        }
        if (label && opts.defaultLabel) {
            label.textContent = opts.defaultLabel;
        }
        if (typeof opts.onCleared === 'function') {
            opts.onCleared();
        }
        return null;
    },

    showMediaRemove(removeBtnId) {
        const btn = document.getElementById(removeBtnId);
        if (btn) btn.classList.remove('hidden');
    },

    /**
     * Assign a File to an <input type="file"> and fire change so existing preview handlers run.
     */
    assignFileToInput(input, file) {
        if (!input || !file) return false;
        try {
            const dt = new DataTransfer();
            dt.items.add(file);
            input.files = dt.files;
            input.dispatchEvent(new Event('change', { bubbles: true }));
            return true;
        } catch (_) {
            return false;
        }
    },

    isVisible(el) {
        return !!(el && (el.offsetWidth || el.offsetHeight || el.getClientRects().length));
    },

    isImageFileInput(el) {
        if (!el || el.tagName !== 'INPUT' || el.type !== 'file' || el.disabled) return false;
        const accept = (el.getAttribute('accept') || '').toLowerCase();
        return accept.includes('image');
    },

    getImageFileInputs(root = document) {
        return Array.from(root.querySelectorAll('input[type="file"]'))
            .filter((input) => {
                if (!this.isImageFileInput(input)) return false;
                // Treat as available when the input or its paste zone is laid out (visible step/section).
                const zone = this.getPasteZone(input);
                return this.isVisible(zone) || this.isVisible(input);
            });
    },

    getPasteZone(input) {
        return input.closest('[data-image-paste]')
            || input.closest('.qc-upload-zone')
            || input.closest('.qc-section')
            || input.parentElement;
    },

    enhanceImageInput(input) {
        if (!input || input.dataset.imagePasteReady === '1') return;
        input.dataset.imagePasteReady = '1';

        const zone = this.getPasteZone(input);
        if (!zone) return;

        zone.setAttribute('data-image-paste', '');
        if (!zone.hasAttribute('tabindex')) {
            zone.setAttribute('tabindex', '0');
        }

        const existingHint = zone.querySelector('.qc-paste-hint');
        if (!existingHint) {
            const uploadName = zone.querySelector('#imageUploadName, [id$="UploadName"]');
            if (uploadName && !/paste|ctrl\+v|⌘/i.test(uploadName.textContent || '')) {
                uploadName.textContent = `${uploadName.textContent.trim()} · or paste (Ctrl+V / ⌘V)`;
            } else {
                const hint = document.createElement('p');
                hint.className = 'qc-paste-hint';
                hint.textContent = 'Tip: click here, then paste an image (Ctrl+V / ⌘V)';
                // Prefer placing after the file input; in overlay zones, append to zone.
                if (zone.classList.contains('qc-upload-zone')) {
                    zone.appendChild(hint);
                } else {
                    input.insertAdjacentElement('afterend', hint);
                }
            }
        }

        const markActive = () => {
            document.querySelectorAll('[data-image-paste].qc-paste-target').forEach((el) => {
                el.classList.remove('qc-paste-target');
            });
            zone.classList.add('qc-paste-target');
            this._lastImagePasteInput = input;
        };

        zone.addEventListener('mouseenter', markActive);
        zone.addEventListener('focusin', markActive);
        input.addEventListener('focus', markActive);
        input.addEventListener('click', markActive);
    },

    resolvePasteTarget(event) {
        const candidates = this.getImageFileInputs();
        if (!candidates.length) return null;

        const fromZone = event.target?.closest?.('[data-image-paste]')
            ?.querySelector('input[type="file"]');
        if (fromZone && candidates.includes(fromZone)) return fromZone;

        if (this._lastImagePasteInput && candidates.includes(this._lastImagePasteInput)) {
            return this._lastImagePasteInput;
        }

        if (candidates.length === 1) return candidates[0];
        return null;
    },

    shouldIgnorePasteTarget(activeEl) {
        if (!activeEl || activeEl === document.body) return false;
        if (activeEl.isContentEditable) return true;
        if (activeEl.tagName === 'TEXTAREA') return true;
        if (activeEl.tagName === 'INPUT') {
            const type = (activeEl.type || '').toLowerCase();
            // Allow paste when focused on the file input itself; block text-like fields.
            return type !== 'file' && type !== 'button' && type !== 'submit' && type !== 'checkbox' && type !== 'radio';
        }
        return false;
    },

    extractImageFileFromClipboard(clipboardData) {
        if (!clipboardData) return null;

        const items = clipboardData.items;
        if (items) {
            for (let i = 0; i < items.length; i++) {
                const item = items[i];
                if (item.type && item.type.startsWith('image/')) {
                    const file = item.getAsFile();
                    if (file) return file;
                }
            }
        }

        const files = clipboardData.files;
        if (files) {
            for (let i = 0; i < files.length; i++) {
                if (files[i].type && files[i].type.startsWith('image/')) {
                    return files[i];
                }
            }
        }
        return null;
    },

    normalizePastedImageFile(file) {
        const type = file.type || 'image/png';
        let ext = type.split('/')[1] || 'png';
        if (ext === 'jpeg') ext = 'jpg';
        const name = file.name && file.name !== 'image.png' && file.name !== 'blob'
            ? file.name
            : `pasted-image.${ext}`;
        return new File([file], name, { type, lastModified: Date.now() });
    },

    /**
     * Enable Ctrl/Cmd+V paste into image file inputs on the page.
     */
    initImagePaste(root = document) {
        if (root.__acadPrepImagePasteBound) return;
        root.__acadPrepImagePasteBound = true;
        this._lastImagePasteInput = null;

        const refresh = () => {
            this.getImageFileInputs(root).forEach((input) => this.enhanceImageInput(input));
        };
        refresh();

        // Re-scan when wizard steps / sections become visible.
        root.addEventListener('click', () => {
            window.requestAnimationFrame(refresh);
        }, true);

        root.addEventListener('paste', (event) => {
            if (this.shouldIgnorePasteTarget(document.activeElement)) return;

            const imageFile = this.extractImageFileFromClipboard(event.clipboardData);
            if (!imageFile) return;

            refresh();
            const target = this.resolvePasteTarget(event);
            if (!target) {
                if (typeof showToast === 'function') {
                    showToast('Click an image upload field first, then paste.', 'info');
                }
                return;
            }

            event.preventDefault();
            const file = this.normalizePastedImageFile(imageFile);
            const ok = this.assignFileToInput(target, file);
            if (ok && typeof showToast === 'function') {
                showToast('Image pasted into upload field.', 'success');
            }
        });
    }
};

(function bootImagePaste() {
    const start = () => window.AcadPrepQuestionUI.initImagePaste();
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', start);
    } else {
        start();
    }
})();
