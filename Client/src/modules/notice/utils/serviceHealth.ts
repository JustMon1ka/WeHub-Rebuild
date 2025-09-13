/**
 * 服务健康检查工具
 * 用于检查通知系统相关服务的可用性
 */

export interface ServiceStatus {
    name: string
    url: string
    status: 'healthy' | 'unhealthy' | 'unknown'
    lastCheck: Date
    responseTime?: number
    error?: string
}

export interface HealthCheckResult {
    overall: 'healthy' | 'degraded' | 'unhealthy'
    services: ServiceStatus[]
    summary: {
        total: number
        healthy: number
        unhealthy: number
        unknown: number
    }
}

/**
 * 检查单个服务的健康状态
 * @param name 服务名称
 * @param url 服务URL
 * @param timeout 超时时间(ms)
 * @returns Promise<ServiceStatus>
 */
export async function checkServiceHealth(
    name: string,
    url: string,
    timeout: number = 5000
): Promise<ServiceStatus> {
    const startTime = Date.now()

    try {
        const controller = new AbortController()
        const timeoutId = setTimeout(() => controller.abort(), timeout)

        const response = await fetch(`${url}/health`, {
            method: 'HEAD',
            signal: controller.signal,
            cache: 'no-cache'
        })

        clearTimeout(timeoutId)
        const responseTime = Date.now() - startTime

        return {
            name,
            url,
            status: response.ok ? 'healthy' : 'unhealthy',
            lastCheck: new Date(),
            responseTime,
            error: response.ok ? undefined : `HTTP ${response.status}`
        }
    } catch (error: any) {
        const responseTime = Date.now() - startTime

        return {
            name,
            url,
            status: 'unhealthy',
            lastCheck: new Date(),
            responseTime,
            error: error.name === 'AbortError' ? '请求超时' : error.message
        }
    }
}

/**
 * 检查所有相关服务的健康状态
 * @returns Promise<HealthCheckResult>
 */
export async function checkAllServices(): Promise<HealthCheckResult> {
    const services = [
        { name: 'PostService', url: 'http://localhost:5006' },
        { name: 'NoticeService', url: 'http://localhost:5000' },
        { name: 'UserDataService', url: 'http://localhost:5001' },
        { name: 'MessageService', url: 'http://localhost:5002' }
    ]

    console.log('[ServiceHealth] 开始检查服务健康状态...')

    const serviceStatuses = await Promise.allSettled(
        services.map(service => checkServiceHealth(service.name, service.url))
    )

    const results: ServiceStatus[] = serviceStatuses.map((result, index) => {
        if (result.status === 'fulfilled') {
            return result.value
        } else {
            return {
                name: services[index].name,
                url: services[index].url,
                status: 'unknown',
                lastCheck: new Date(),
                error: result.reason?.message || '检查失败'
            }
        }
    })

    // 计算摘要
    const summary = {
        total: results.length,
        healthy: results.filter(s => s.status === 'healthy').length,
        unhealthy: results.filter(s => s.status === 'unhealthy').length,
        unknown: results.filter(s => s.status === 'unknown').length
    }

    // 确定整体状态
    let overall: 'healthy' | 'degraded' | 'unhealthy'
    if (summary.healthy === summary.total) {
        overall = 'healthy'
    } else if (summary.healthy > 0) {
        overall = 'degraded'
    } else {
        overall = 'unhealthy'
    }

    const healthResult: HealthCheckResult = {
        overall,
        services: results,
        summary
    }

    console.log('[ServiceHealth] 服务健康检查完成:', healthResult)
    return healthResult
}

/**
 * 在控制台输出服务健康状态
 * @param result 健康检查结果
 */
export function logServiceHealth(result: HealthCheckResult) {
    console.group('🏥 服务健康状态检查')

    // 整体状态
    const statusEmoji = {
        healthy: '✅',
        degraded: '⚠️',
        unhealthy: '❌'
    }[result.overall]

    console.log(`${statusEmoji} 整体状态: ${result.overall}`)
    console.log(`📊 服务统计: ${result.summary.healthy}/${result.summary.total} 健康`)

    // 详细状态
    result.services.forEach(service => {
        const emoji = {
            healthy: '✅',
            unhealthy: '❌',
            unknown: '❓'
        }[service.status]

        console.log(`${emoji} ${service.name}: ${service.status}`)
        if (service.responseTime) {
            console.log(`   ⏱️ 响应时间: ${service.responseTime}ms`)
        }
        if (service.error) {
            console.log(`   ⚠️ 错误: ${service.error}`)
        }
    })

    console.groupEnd()
}

/**
 * 检查关键服务是否可用
 * @returns Promise<boolean>
 */
export async function isCriticalServiceAvailable(): Promise<boolean> {
    try {
        const result = await checkAllServices()

        // 检查关键服务（PostService 和 NoticeService）
        const criticalServices = result.services.filter(s =>
            s.name === 'PostService' || s.name === 'NoticeService'
        )

        const criticalHealthy = criticalServices.every(s => s.status === 'healthy')

        if (!criticalHealthy) {
            console.warn('[ServiceHealth] 关键服务不可用，将使用降级模式')
            logServiceHealth(result)
        }

        return criticalHealthy
    } catch (error) {
        console.error('[ServiceHealth] 服务健康检查失败:', error)
        return false
    }
}

/**
 * 定期检查服务健康状态
 * @param interval 检查间隔(ms)
 * @param callback 状态变化回调
 * @returns 清理函数
 */
export function startHealthMonitoring(
    interval: number = 30000, // 30秒
    callback?: (result: HealthCheckResult) => void
): () => void {
    let isRunning = true

    const checkHealth = async () => {
        if (!isRunning) return

        try {
            const result = await checkAllServices()
            if (callback) {
                callback(result)
            }
        } catch (error) {
            console.error('[ServiceHealth] 定期健康检查失败:', error)
        }

        if (isRunning) {
            setTimeout(checkHealth, interval)
        }
    }

    // 立即执行一次
    checkHealth()

    // 返回清理函数
    return () => {
        isRunning = false
    }
}
