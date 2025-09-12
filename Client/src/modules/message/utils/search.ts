/**
 * 搜索相关工具函数
 */

// 搜索结果类型
export interface SearchResult {
    type: 'conversation' | 'message'
    conversation: any
    message?: {
        content: string
        time: string
        sender: string
    }
    relevance: number // 相关性评分
}

/**
 * 高亮搜索关键词
 * @param text 原始文本
 * @param searchTerm 搜索关键词
 * @returns 高亮后的HTML字符串
 */
export function highlightSearchTerm(text: string, searchTerm: string): string {
    if (!searchTerm || !text) return text

    const regex = new RegExp(`(${searchTerm})`, 'gi')
    return text.replace(regex, '<span class="highlight">$1</span>')
}

/**
 * 防抖搜索函数
 * @param text 搜索文本
 * @param callback 回调函数
 * @param delay 延迟时间（毫秒）
 * @returns 清理函数
 */
export function createDebounceSearch(
    callback: (text: string) => void,
    delay: number = 300
) {
    let timer: number | null = null

    return (text: string) => {
        if (timer) {
            clearTimeout(timer)
        }

        timer = setTimeout(() => {
            callback(text)
        }, delay)
    }
}

/**
 * 计算搜索相关性评分
 * @param text 文本内容
 * @param searchTerm 搜索关键词
 * @param type 搜索类型
 * @returns 相关性评分
 */
export function calculateRelevance(
    text: string,
    searchTerm: string,
    type: 'exact' | 'prefix' | 'contains' | 'message'
): number {
    const lowerText = text.toLowerCase()
    const lowerTerm = searchTerm.toLowerCase()

    switch (type) {
        case 'exact':
            return lowerText === lowerTerm ? 100 : 0
        case 'prefix':
            return lowerText.startsWith(lowerTerm) ? 80 : 0
        case 'contains':
            return lowerText.includes(lowerTerm) ? 60 : 0
        case 'message':
            if (lowerText === lowerTerm) return 50
            if (lowerText.startsWith(lowerTerm)) return 35
            if (lowerText.includes(lowerTerm)) return 20
            return 0
        default:
            return 0
    }
}
