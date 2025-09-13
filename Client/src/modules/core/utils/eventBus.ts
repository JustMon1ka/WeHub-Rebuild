/**
 * 全局事件总线
 * 用于组件间通信
 */

type EventCallback = (...args: any[]) => void

class EventBus {
    private events: Map<string, EventCallback[]> = new Map()

    /**
     * 监听事件
     * @param event 事件名称
     * @param callback 回调函数
     */
    on(event: string, callback: EventCallback): void {
        if (!this.events.has(event)) {
            this.events.set(event, [])
        }
        this.events.get(event)!.push(callback)
    }

    /**
     * 移除事件监听
     * @param event 事件名称
     * @param callback 回调函数
     */
    off(event: string, callback: EventCallback): void {
        const callbacks = this.events.get(event)
        if (callbacks) {
            const index = callbacks.indexOf(callback)
            if (index > -1) {
                callbacks.splice(index, 1)
            }
        }
    }

    /**
     * 触发事件
     * @param event 事件名称
     * @param args 参数
     */
    emit(event: string, ...args: any[]): void {
        const callbacks = this.events.get(event)
        if (callbacks) {
            callbacks.forEach(callback => {
                try {
                    callback(...args)
                } catch (error) {
                    console.error(`Event callback error for ${event}:`, error)
                }
            })
        }
    }

    /**
     * 移除所有事件监听
     */
    clear(): void {
        this.events.clear()
    }
}

// 创建全局事件总线实例
export const eventBus = new EventBus()

// 定义事件类型
export const EVENTS = {
    // 消息相关事件
    MESSAGE_UNREAD_COUNT_CHANGED: 'message:unread-count-changed',
    MESSAGE_MARKED_AS_READ: 'message:marked-as-read',

    // 用户相关事件
    USER_LOGIN: 'user:login',
    USER_LOGOUT: 'user:logout',

    // 其他事件
    NOTIFICATION_RECEIVED: 'notification:received'
} as const
