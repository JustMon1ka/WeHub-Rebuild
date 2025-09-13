/**
 * 通知系统错误处理工具
 * 用于统一处理API请求错误和提供降级方案
 */

export interface ApiErrorInfo {
    type: '404' | '500' | 'network' | 'timeout' | 'unknown'
    message: string
    fallbackData: any
}

/**
 * 分析API错误类型
 * @param error 错误对象
 * @returns 错误信息
 */
export function analyzeApiError(error: any): ApiErrorInfo {
    // 网络错误
    if (!navigator.onLine || error.code === 'NETWORK_ERROR' || error.message?.includes('Network Error')) {
        return {
            type: 'network',
            message: '网络连接错误',
            fallbackData: null
        }
    }

    // 超时错误
    if (error.code === 'ECONNABORTED' || error.message?.includes('timeout')) {
        return {
            type: 'timeout',
            message: '请求超时',
            fallbackData: null
        }
    }

    // HTTP状态码错误
    const status = error.response?.status
    if (status === 404) {
        return {
            type: '404',
            message: '资源不存在',
            fallbackData: null
        }
    }

    if (status >= 500) {
        return {
            type: '500',
            message: '服务器错误',
            fallbackData: null
        }
    }

    // 未知错误
    return {
        type: 'unknown',
        message: error.message || '未知错误',
        fallbackData: null
    }
}

/**
 * 生成降级数据
 * @param type 数据类型
 * @param id 资源ID
 * @param errorInfo 错误信息
 * @returns 降级数据
 */
export function generateFallbackData(type: 'post' | 'comment' | 'user', id: number, errorInfo: ApiErrorInfo) {
    const timestamp = new Date().toISOString()

    switch (type) {
        case 'post':
            return {
                postId: id,
                userId: 0,
                title: `帖子${id} (${errorInfo.message})`,
                content: '内容不可用',
                tags: [],
                createdAt: timestamp,
                views: 0,
                likes: 0,
                circleId: 0
            }

        case 'comment':
            return {
                commentId: id,
                userId: 0,
                postId: 0,
                content: '评论内容不可用',
                createdAt: timestamp,
                likes: 0
            }

        case 'user':
            return {
                nickname: `用户${id} (${errorInfo.message})`,
                avatar: ''
            }

        default:
            return null
    }
}

/**
 * 处理API错误的统一函数
 * @param error 错误对象
 * @param type 数据类型
 * @param id 资源ID
 * @param context 上下文信息
 * @returns 降级数据或抛出错误
 */
export function handleApiError(
    error: any,
    type: 'post' | 'comment' | 'user',
    id: number,
    context: string = ''
): any {
    const errorInfo = analyzeApiError(error)

    console.warn(`[ErrorHandler] ${context} - ${type} ${id}:`, {
        type: errorInfo.type,
        message: errorInfo.message,
        originalError: error
    })

    // 对于404错误，返回降级数据而不是抛出错误
    if (errorInfo.type === '404') {
        const fallbackData = generateFallbackData(type, id, errorInfo)
        console.log(`[ErrorHandler] 返回降级数据:`, fallbackData)
        return fallbackData
    }

    // 对于其他错误，仍然抛出
    throw error
}

/**
 * 检查服务是否可用
 * @param baseUrl 服务基础URL
 * @returns Promise<boolean>
 */
export async function checkServiceHealth(baseUrl: string): Promise<boolean> {
    try {
        const controller = new AbortController()
        const timeoutId = setTimeout(() => controller.abort(), 5000)

        const response = await fetch(`${baseUrl}/health`, {
            method: 'HEAD',
            signal: controller.signal
        })

        clearTimeout(timeoutId)
        return response.ok
    } catch {
        return false
    }
}

/**
 * 重试机制
 * @param fn 要重试的函数
 * @param maxRetries 最大重试次数
 * @param delay 重试延迟(ms)
 * @returns Promise<any>
 */
export async function retryWithBackoff<T>(
    fn: () => Promise<T>,
    maxRetries: number = 3,
    delay: number = 1000
): Promise<T> {
    let lastError: any

    for (let i = 0; i <= maxRetries; i++) {
        try {
            return await fn()
        } catch (error) {
            lastError = error

            if (i === maxRetries) {
                throw lastError
            }

            // 指数退避
            const waitTime = delay * Math.pow(2, i)
            console.log(`[ErrorHandler] 重试 ${i + 1}/${maxRetries + 1}，等待 ${waitTime}ms`)
            await new Promise(resolve => setTimeout(resolve, waitTime))
        }
    }

    throw lastError
}

/**
 * 批量处理错误
 * @param promises Promise数组
 * @param fallbackFn 降级函数
 * @returns Promise数组结果
 */
export async function batchWithFallback<T>(
    promises: Promise<T>[],
    fallbackFn: (index: number, error: any) => T
): Promise<T[]> {
    const results = await Promise.allSettled(promises)

    return results.map((result, index) => {
        if (result.status === 'fulfilled') {
            return result.value
        } else {
            console.warn(`[ErrorHandler] 批量处理中第${index}个请求失败:`, result.reason)
            return fallbackFn(index, result.reason)
        }
    })
}
