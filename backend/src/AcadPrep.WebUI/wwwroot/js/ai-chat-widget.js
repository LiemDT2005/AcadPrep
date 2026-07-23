document.addEventListener('DOMContentLoaded', () => {
    const toggleBtn = document.getElementById('ai-chat-toggle');
    const closeBtn = document.getElementById('ai-chat-close-btn');
    const clearBtn = document.getElementById('ai-chat-clear');
    const chatWindow = document.getElementById('ai-chat-window');
    const inputField = document.getElementById('ai-chat-input');
    const sendBtn = document.getElementById('ai-chat-send');
    const messagesContainer = document.getElementById('ai-chat-messages');
    const tokenDisplay = document.getElementById('ai-remaining-tokens');

    const STORAGE_KEY = 'acadprep_ai_chat_history';
    let chatHistory = [];
    let isWaitingForResponse = false;

    // --- Init ---
    function loadHistory() {
        const stored = sessionStorage.getItem(STORAGE_KEY);
        if (stored) {
            try {
                chatHistory = JSON.parse(stored);
                chatHistory.forEach(msg => {
                    appendMessage(msg.role, msg.content, false);
                });
            } catch (e) {
                console.error('Error parsing chat history', e);
                chatHistory = [];
            }
        }
    }

    function saveHistory() {
        // Keep only last 10 messages to avoid token bloat
        if (chatHistory.length > 10) {
            chatHistory = chatHistory.slice(chatHistory.length - 10);
        }
        sessionStorage.setItem(STORAGE_KEY, JSON.stringify(chatHistory));
    }

    // --- UI Actions ---
    function toggleChat() {
        if (chatWindow.style.display === 'none' || chatWindow.style.display === '') {
            chatWindow.style.display = 'flex';
            // small delay to allow display:flex to apply before animating opacity/scale
            setTimeout(() => {
                chatWindow.classList.remove('scale-95', 'opacity-0');
                chatWindow.classList.add('scale-100', 'opacity-100');
            }, 10);
            inputField.focus();
            scrollToBottom();
        } else {
            chatWindow.classList.remove('scale-100', 'opacity-100');
            chatWindow.classList.add('scale-95', 'opacity-0');
            setTimeout(() => {
                chatWindow.style.display = 'none';
            }, 300); // match transition duration
        }
    }

    function scrollToBottom() {
        messagesContainer.scrollTop = messagesContainer.scrollHeight;
    }

    function appendMessage(role, content, animate = true) {
        const div = document.createElement('div');
        div.className = `flex gap-2 max-w-[85%] ${role === 'user' ? 'self-end flex-row-reverse' : 'self-start items-end'}`;
        
        let avatarHtml = '';
        if (role === 'user') {
            avatarHtml = `<div class="w-8 h-8 rounded-full bg-primary flex items-center justify-center flex-shrink-0 text-white"><span class="material-symbols-outlined text-[16px]">person</span></div>`;
        } else {
            avatarHtml = `<div class="w-8 h-8 rounded-full flex items-center justify-center flex-shrink-0 overflow-hidden bg-white border border-outline-variant/30"><img src="/images/ai-logo.png" class="w-full h-full object-cover" alt="AI Logo" /></div>`;
        }

        const bubbleClasses = role === 'user' 
            ? 'bg-primary text-white rounded-2xl rounded-br-none px-3 py-2 shadow-sm'
            : 'bg-surface-container-lowest text-on-surface border border-outline-variant rounded-2xl rounded-bl-none px-4 py-3 shadow-sm ai-message-bubble';

        let innerContent = '';
        if (role === 'user') {
            innerContent = escapeHtml(content);
        } else {
            // Parse Markdown and Sanitize
            if (typeof marked !== 'undefined' && typeof DOMPurify !== 'undefined') {
                const rawHtml = marked.parse(content, { breaks: true });
                innerContent = DOMPurify.sanitize(rawHtml);
            } else {
                innerContent = escapeHtml(content).replace(/\n/g, '<br>');
            }
        }

        div.innerHTML = `
            ${avatarHtml}
            <div class="${bubbleClasses}">${innerContent}</div>
        `;
        
        messagesContainer.appendChild(div);
        if (animate) scrollToBottom();
    }

    function showTypingIndicator() {
        const div = document.createElement('div');
        div.id = 'ai-typing-indicator';
        div.className = 'flex items-end gap-2 self-start max-w-[85%]';
        div.innerHTML = `
            <div class="w-8 h-8 rounded-full flex items-center justify-center flex-shrink-0 overflow-hidden bg-white border border-outline-variant/30">
                <img src="/images/ai-logo.png" class="w-full h-full object-cover" alt="AI Logo" />
            </div>
            <div class="bg-surface-container-lowest border border-outline-variant px-4 py-3 rounded-2xl rounded-bl-none shadow-sm flex gap-1">
                <div class="w-2 h-2 rounded-full bg-primary/50 animate-bounce" style="animation-delay: 0ms"></div>
                <div class="w-2 h-2 rounded-full bg-primary/50 animate-bounce" style="animation-delay: 150ms"></div>
                <div class="w-2 h-2 rounded-full bg-primary/50 animate-bounce" style="animation-delay: 300ms"></div>
            </div>
        `;
        messagesContainer.appendChild(div);
        scrollToBottom();
    }

    function hideTypingIndicator() {
        const indicator = document.getElementById('ai-typing-indicator');
        if (indicator) indicator.remove();
    }

    function escapeHtml(unsafe) {
        return unsafe
             .replace(/&/g, "&amp;")
             .replace(/</g, "&lt;")
             .replace(/>/g, "&gt;")
             .replace(/"/g, "&quot;")
             .replace(/'/g, "&#039;");
    }

    function clearChat() {
        chatHistory = [];
        sessionStorage.removeItem(STORAGE_KEY);
        // keep only the first welcome message
        while (messagesContainer.children.length > 1) {
            messagesContainer.removeChild(messagesContainer.lastChild);
        }
        tokenDisplay.innerText = '--';
    }

    // --- API Call ---
    async function sendMessage() {
        const text = inputField.value.trim();
        if (!text || isWaitingForResponse) return;

        // UI Update
        inputField.value = '';
        inputField.style.height = 'auto'; // reset textarea height
        isWaitingForResponse = true;
        sendBtn.disabled = true;

        appendMessage('user', text);
        
        // Prepare API request
        const requestBody = {
            message: text,
            history: chatHistory.map(h => ({ role: h.role, content: h.content })) // omit extra fields
        };

        // Save to local state early so it's there even if we refresh
        chatHistory.push({ role: 'user', content: text });
        saveHistory();

        showTypingIndicator();

        try {
            const response = await fetch('/api/ai-qna/ask', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(requestBody)
            });

            const data = await response.json();
            hideTypingIndicator();

            if (response.ok) {
                appendMessage('assistant', data.reply);
                chatHistory.push({ role: 'assistant', content: data.reply });
                saveHistory();
                
                if (data.remainingTokensToday !== undefined) {
                    tokenDisplay.innerText = data.remainingTokensToday.toLocaleString();
                }
            } else {
                appendMessage('assistant', `⚠️ Lỗi: ${data.error || 'Có lỗi xảy ra'}`);
            }
        } catch (error) {
            hideTypingIndicator();
            appendMessage('assistant', `⚠️ AI đang bận hoặc mất kết nối mạng. Vui lòng thử lại sau.`);
            console.error('AI QnA Error:', error);
        } finally {
            isWaitingForResponse = false;
            sendBtn.disabled = false;
            inputField.focus();
        }
    }

    // --- Event Listeners ---
    toggleBtn.addEventListener('click', toggleChat);
    closeBtn.addEventListener('click', toggleChat);
    clearBtn.addEventListener('click', () => {
        if(confirm('Bạn có chắc chắn muốn xóa lịch sử cuộc trò chuyện này không?')) {
            clearChat();
        }
    });

    sendBtn.addEventListener('click', sendMessage);

    inputField.addEventListener('keydown', (e) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            sendMessage();
        }
    });

    // Auto-resize textarea
    inputField.addEventListener('input', function() {
        this.style.height = 'auto';
        this.style.height = (this.scrollHeight) + 'px';
        if (this.value.trim() === '') {
            sendBtn.disabled = true;
        } else {
            sendBtn.disabled = isWaitingForResponse;
        }
    });

    // Initialize
    loadHistory();
});
