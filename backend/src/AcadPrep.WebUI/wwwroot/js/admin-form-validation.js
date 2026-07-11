window.AcadPrepFormValidation = (function () {
    const ERROR_BORDER_CLASS = 'border-error';
    const DEFAULT_MESSAGE = 'Please fill in all required fields.';

    function isInputEmpty(input) {
        if (input.disabled) {
            return false;
        }

        if (input.type === 'file') {
            return !input.files || input.files.length === 0;
        }

        if (input.type === 'number') {
            return input.value === '' || input.value === null;
        }

        return !String(input.value ?? '').trim();
    }

    function markInvalid(input) {
        input.classList.add(ERROR_BORDER_CLASS);
    }

    function clearInvalid(input) {
        input.classList.remove(ERROR_BORDER_CLASS);
    }

    function notify(message) {
        if (typeof showToast === 'function') {
            showToast(message, 'error');
        } else {
            alert(message);
        }
    }

    function validateInputs(inputs, message) {
        let isValid = true;
        let firstInvalid = null;

        inputs.forEach((input) => {
            if (input.disabled || !input.required) {
                clearInvalid(input);
                return;
            }

            if (isInputEmpty(input)) {
                markInvalid(input);
                if (!firstInvalid) {
                    firstInvalid = input;
                }
                isValid = false;
            } else {
                clearInvalid(input);
            }
        });

        if (!isValid) {
            notify(message || DEFAULT_MESSAGE);
            firstInvalid?.focus();
        }

        return isValid;
    }

    function validateContainer(container, message) {
        if (!container) {
            return true;
        }

        const inputs = container.querySelectorAll('[required]');
        return validateInputs(inputs, message);
    }

    function validateChecks(checks, message) {
        let isValid = true;
        let firstInvalid = null;

        checks.forEach((check) => {
            const input = check.element;
            if (check.isValid) {
                if (input) {
                    clearInvalid(input);
                }
                return;
            }

            isValid = false;
            if (input) {
                markInvalid(input);
                if (!firstInvalid) {
                    firstInvalid = input;
                }
            }
        });

        if (!isValid) {
            const customMessage = checks.find((check) => check.message)?.message;
            notify(customMessage || message || DEFAULT_MESSAGE);
            firstInvalid?.focus();
        }

        return isValid;
    }

    function bindForm(form, options) {
        if (!form || form.dataset.acadprepValidationBound === 'true') {
            return;
        }

        form.dataset.acadprepValidationBound = 'true';
        form.setAttribute('novalidate', 'novalidate');

        form.addEventListener('submit', (event) => {
            if (options && typeof options.beforeValidate === 'function') {
                options.beforeValidate();
            }

            if (!validateContainer(form)) {
                event.preventDefault();
            }
        });

        form.addEventListener('input', (event) => {
            const target = event.target;
            if (!(target instanceof HTMLElement)) {
                return;
            }

            if (target.matches('[required]') && !isInputEmpty(target)) {
                clearInvalid(target);
            }
        });

        form.addEventListener('change', (event) => {
            const target = event.target;
            if (!(target instanceof HTMLElement)) {
                return;
            }

            if (target.matches('[required]') && !isInputEmpty(target)) {
                clearInvalid(target);
            }
        });
    }

    return {
        DEFAULT_MESSAGE,
        validateContainer,
        validateChecks,
        bindForm,
        markInvalid,
        clearInvalid
    };
})();
