/**
 * 消息处理相关工具函数
 */

/**
 * 渲染消息内容，处理emoji等特殊格式
 * @param content 消息内容
 * @returns 渲染后的HTML字符串
 */
export function renderContent(content: string): string {
    let html = content.replace(/\[emoji:(\w+)\]/g, (_match, p1) => {
        return `<img src="/emoji/${p1}.png" alt="${p1}" class="emoji-img" />`
    })
    return html
}

/**
 * 复制消息内容到剪贴板
 * @param message 消息对象
 * @returns Promise<boolean> 是否复制成功
 */
export async function copyMessageContent(message: {
    type: string
    content: string
}): Promise<boolean> {
    try {
        let textToCopy = ''

        if (message.type === 'text') {
            // 移除HTML标签，获取纯文本
            const tempDiv = document.createElement('div')
            tempDiv.innerHTML = renderContent(message.content)
            textToCopy = tempDiv.textContent || tempDiv.innerText || ''
        } else if (message.type === 'image') {
            textToCopy = message.content // 图片URL
        }

        await navigator.clipboard.writeText(textToCopy)
        return true
    } catch (error) {
        console.error('复制失败:', error)
        return false
    }
}

/**
 * 格式化消息时间
 * @param timestamp 时间戳
 * @returns 格式化后的时间字符串
 */
export function formatMessageTime(timestamp: string | Date): string {
    const date = new Date(timestamp)
    const now = new Date()
    const diff = now.getTime() - date.getTime()

    // 小于1分钟
    if (diff < 60000) {
        return '刚刚'
    }

    // 小于1小时
    if (diff < 3600000) {
        const minutes = Math.floor(diff / 60000)
        return `${minutes}分钟前`
    }

    // 小于24小时
    if (diff < 86400000) {
        const hours = Math.floor(diff / 3600000)
        return `${hours}小时前`
    }

    // 超过24小时，显示具体日期
    return date.toLocaleDateString('zh-CN', {
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    })
}
