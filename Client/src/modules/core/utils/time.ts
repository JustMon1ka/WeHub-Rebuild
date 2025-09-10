export function formatTime(timeStr: string) {
    const now = new Date();
    const newestMessageTime = new Date(timeStr);

    // 检查日期是否有效
    if (isNaN(newestMessageTime.getTime())) {
        console.warn('无效的时间字符串，使用当前时间:', timeStr);
        return '刚刚';
    }

    //console.log("newestMessageTime:", newestMessageTime.getTime());
    //console.log("now:", now.getTime());
    const diffms = now.getTime() - newestMessageTime.getTime();
    //console.log("diffms:", diffms);
    const diffSecond = Math.floor(diffms / 1000);
    const diffMinute = Math.floor(diffSecond / 60);
    const diffHour = Math.floor(diffMinute / 60);
    const diffDay = Math.floor(diffHour / 24);

    if (diffSecond < 60) return '刚刚'
    if (diffMinute < 60) return `${diffMinute}分钟前`
    if (diffHour < 24) return `${diffHour}小时前`
    if (diffDay < 7) return `${diffDay}天前`
    return timeStr;
}