import EventHandler from "../../_content/BootstrapBlazor/modules/event-handler.js"

export function init(id) {
    const el = document.getElementById(id)

    EventHandler.on(el, 'keyup', e => {
        if (e.key === 'Enter' && !e.shiftKey && el.value.trim('\n').length > 0) {
            el.blur()
            document.querySelector('.btn-send').click()
            
            // Scroll to bottom immediately when user sends message
            setTimeout(() => {
                const body = document.querySelector('.chat-body')
                if (body) {
                    body.scrollTop = body.scrollHeight
                }
            }, 100) // Small delay to allow DOM update
        }
    })
}

export function scroll(id, shouldScrollToBottom = true) {
    const body = document.querySelector('.chat-body')
    
    // Only scroll to bottom when explicitly requested (new messages)
    if (shouldScrollToBottom && body.offsetHeight < body.scrollHeight) {
        body.scrollTop = body.scrollHeight
    }

    // focus input
    const el = document.getElementById(id)
    if (el) {
        el.focus()
    }
}

export function scrollToBottom(id) {
    const body = document.querySelector('.chat-body')
    if (body) {
        body.scrollTop = body.scrollHeight
    }
}

export function applyJumpingEffect() {
    // Tìm tất cả elements có class jumping-line
    const jumpingElements = document.querySelectorAll('.jumping-line');
    
    jumpingElements.forEach(element => {
        // Chỉ process nếu chưa được process (tránh duplicate)
        if (element.hasAttribute('data-jumping-processed')) return;
        
        // Tìm markdown-body bên trong
        const markdownBody = element.querySelector('.markdown-body');
        if (!markdownBody) return;
        
        // Lấy text content
        const text = markdownBody.textContent || markdownBody.innerText;
        if (!text) return;
        
        // Tạo HTML mới với từng ký tự được wrap trong span
        const wrappedText = text.split('').map((char, index) => {
            if (char === ' ') {
                return '<span class="jump-char">&nbsp;</span>';
            }
            return `<span class="jump-char" style="animation-delay: ${(index % 20) * 0.1}s">${char}</span>`;
        }).join('');
        
        // Thay thế nội dung
        markdownBody.innerHTML = wrappedText;
        
        // Đánh dấu đã được process
        element.setAttribute('data-jumping-processed', 'true');
    });
}

export function dispose(id) {
    const el = document.getElementById(id)
    EventHandler.off(el, 'onkeyup')
}
