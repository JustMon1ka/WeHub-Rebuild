<template>
  <div class="report-dialog-overview">
    <div class="report-dialog-content">
      <div class="dialog-header">
        <span class="dialog-title">我要举报</span>
      </div>

      <div class="report-content">
        <span>我要举报的是：{{ reportTargetUserName }}的{{ getReportType(reportTargetType) }}</span>
        <div v-if="reportTargetContent" class="report-target-content">
          <strong>举报内容：</strong>
          <div class="content-preview">{{ reportTargetContent }}</div>
        </div>

        <div class="report-section">
          <span class="report-section-title">违反法律法规</span>
          <div class="report-reason">
            <label class="reason-button" :class="{ active: reportReasons.includes('illegal') }">
              <input type="checkbox" v-model="reportReasons" value="illegal" hidden />违法违禁
            </label>
            <label class="reason-button" :class="{ active: reportReasons.includes('gambling') }">
              <input type="checkbox" v-model="reportReasons" value="gambling" hidden />赌博诈骗
            </label>
            <label
              class="reason-button"
              :class="{ active: reportReasons.includes('infringement') }"
            >
              <input type="checkbox" v-model="reportReasons" value="infringement" hidden />侵权申诉
            </label>
          </div>
        </div>

        <div class="report-section">
          <span class="report-section-title">谣言及虚假信息</span>
          <div class="report-reason">
            <label
              class="reason-button"
              :class="{ active: reportReasons.includes('political_rumor') }"
            >
              <input
                type="checkbox"
                v-model="reportReasons"
                value="political_rumor"
                hidden
              />涉政谣言
            </label>
            <label
              class="reason-button"
              :class="{ active: reportReasons.includes('social_rumor') }"
            >
              <input
                type="checkbox"
                v-model="reportReasons"
                value="social_rumor"
                hidden
              />涉社会事件谣言
            </label>
            <label class="reason-button" :class="{ active: reportReasons.includes('false_info') }">
              <input
                type="checkbox"
                v-model="reportReasons"
                value="false_info"
                hidden
              />虚假不实信息
            </label>
          </div>
        </div>

        <div class="report-section">
          <span class="report-section-title">投稿不规范</span>
          <div class="report-reason">
            <label
              class="reason-button"
              :class="{
                active: reportReasons.includes('promotion_violation'),
              }"
            >
              <input
                type="checkbox"
                v-model="reportReasons"
                value="promotion_violation"
                hidden
              />违规推广
            </label>
            <label
              class="reason-button"
              :class="{ active: reportReasons.includes('reprint_error') }"
            >
              <input
                type="checkbox"
                v-model="reportReasons"
                value="reprint_error"
                hidden
              />转载/自制错误
            </label>
            <label
              class="reason-button"
              :class="{ active: reportReasons.includes('other_irregular') }"
            >
              <input
                type="checkbox"
                v-model="reportReasons"
                value="other_irregular"
                hidden
              />其他不规范行为
            </label>
          </div>
        </div>

        <div class="report-section">
          <span class="report-section-title">不友善行为</span>
          <div class="report-reason">
            <label
              class="reason-button"
              :class="{ active: reportReasons.includes('personal_attack') }"
            >
              <input
                type="checkbox"
                v-model="reportReasons"
                value="personal_attack"
                hidden
              />人身攻击
            </label>
            <label
              class="reason-button"
              :class="{ active: reportReasons.includes('inciting_conflict') }"
            >
              <input type="checkbox" v-model="reportReasons" value="inciting_conflict" hidden />引战
            </label>
          </div>
        </div>

        <div class="report-section">
          <span class="report-section-title">违反公序良俗</span>
          <div class="report-reason">
            <label
              class="reason-button"
              :class="{
                active: reportReasons.includes('dangerous_behavior'),
              }"
            >
              <input
                type="checkbox"
                v-model="reportReasons"
                value="dangerous_behavior"
                hidden
              />危险行为
            </label>
            <label
              class="reason-button"
              :class="{ active: reportReasons.includes('bloody_violence') }"
            >
              <input
                type="checkbox"
                v-model="reportReasons"
                value="bloody_violence"
                hidden
              />血腥暴力
            </label>
            <label
              class="reason-button"
              :class="{
                active: reportReasons.includes('pornographic'),
              }"
            >
              <input type="checkbox" v-model="reportReasons" value="pornographic" hidden />色情低俗
            </label>

            <label
              class="reason-button"
              :class="{
                active: reportReasons.includes('uncomfortable'),
              }"
            >
              <input type="checkbox" v-model="reportReasons" value="uncomfortable" hidden />观感不适
            </label>
            <label
              class="reason-button"
              :class="{
                active: reportReasons.includes('teenager_bad_info'),
              }"
            >
              <input
                type="checkbox"
                v-model="reportReasons"
                value="teenager_bad_info"
                hidden
              />青少年不良信息
            </label>
            <label
              class="reason-button"
              :class="{
                active: reportReasons.includes('other'),
              }"
            >
              <input type="checkbox" v-model="reportReasons" value="other" hidden />其他
            </label>
          </div>
        </div>

        <div class="report-section">
          <span class="report-section-title">详情描述（必填）</span>
          <textarea
            class="report-reason-discription"
            v-model="description"
            placeholder="请详细描述举报原因..."
            rows="4"
          ></textarea>
        </div>

        <button class="submit-button" @click="handleSubmitClick">提交</button>
      </div>
    </div>
  </div>
</template>
  
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()

// 从URL参数获取举报信息
const reportTargetId = ref<number>(0)
const reportTargetType = ref<'message' | 'user' | 'post' | 'comment'>('message')
const reportTargetUserName = ref<string>('')
const reportTargetContent = ref<string>('')

// 举报表单数据
const reportReasons = ref<string[]>([])
const description = ref<string>('')

// 获取举报类型的中文描述
const getReportType = (type: string) => {
  switch (type) {
    case 'user':
      return '用户'
    case 'post':
      return '帖子'
    case 'comment':
      return '评论'
    case 'message':
      return '消息'
    default:
      return ''
  }
}

// 根据举报类型获取目标内容
const fetchReportTargetInfo = async () => {
  try {
    const type = route.params.type as string
    const id = route.params.id as string

    if (type && id) {
      reportTargetType.value = type as 'message' | 'user' | 'post' | 'comment'
      reportTargetId.value = parseInt(id)

      // 根据不同类型获取相应的信息
      switch (type) {
        case 'message':
          await fetchMessageInfo(parseInt(id))
          break
        case 'user':
          await fetchUserInfo(parseInt(id))
          break
        case 'post':
          await fetchPostInfo(parseInt(id))
          break
        case 'comment':
          await fetchCommentInfo(parseInt(id))
          break
      }
    }
  } catch (error) {
    console.error('获取举报目标信息失败:', error)
  }
}

// 获取消息信息
const fetchMessageInfo = async (messageId: number) => {
  // 这里应该调用实际的API
  // 暂时使用模拟数据
  reportTargetUserName.value = '用户小明'
  reportTargetContent.value = '这是一条测试消息内容，用于演示举报功能。'
}

// 获取用户信息
const fetchUserInfo = async (userId: number) => {
  // 这里应该调用实际的API
  reportTargetUserName.value = '用户小红'
  reportTargetContent.value = '用户资料页面'
}

// 获取帖子信息
const fetchPostInfo = async (postId: number) => {
  // 这里应该调用实际的API
  reportTargetUserName.value = '用户小李'
  reportTargetContent.value = '这是一个测试帖子的内容，包含了一些文字描述。'
}

// 获取评论信息
const fetchCommentInfo = async (commentId: number) => {
  // 这里应该调用实际的API
  reportTargetUserName.value = '用户小王'
  reportTargetContent.value = '这是一条评论内容，用户发表了个人观点。'
}

// 处理提交举报
const handleSubmitClick = () => {
  if (!reportReasons.value.length) {
    alert('请选择举报原因')
    return
  }
  if (!description.value.trim()) {
    alert('请详细描述举报原因')
    return
  }

  // 构建举报数据
  const reportData = {
    reasons: reportReasons.value,
    description: description.value.trim(),
    targetId: reportTargetId.value,
    targetType: reportTargetType.value,
    reportTime: new Date().toISOString(),
    reporterId: route.query.reporterId || 0, // 举报人ID
    reportedId: route.query.reportedId || 0, // 被举报人ID
  }

  console.log('提交举报:', reportData)

  // 这里应该调用实际的举报API
  // await submitReport(reportData)

  alert('举报提交成功')
  router.push('/') // 跳转到首页
}

// 处理取消举报
const handleCloseClick = () => {
  router.back() // 返回上一页
}

// 组件挂载时获取举报目标信息
onMounted(() => {
  fetchReportTargetInfo()
})
</script>
  
<style scoped>
.report-dialog-overview {
  position: fixed;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(0, 0, 0, 0.2);
  z-index: 1000;
}

.report-dialog-content {
  background: #fff;
  border-radius: 10px;
  padding: 10px 20px;
  box-shadow: 0 12px 32px rgba(0, 0, 0, 0.24), 0 8px 12px rgba(0, 0, 0, 0.12);
}

.dialog-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 0 16px 0;
}

.report-section {
  margin-bottom: 10px;
}

.dialog-title {
  font-weight: bold;
  flex: 1;
  text-align: center;
  display: flex;
  align-items: center;
  justify-content: center;
}

.close-button:hover {
  background-color: #f0f0f0;
}

.report-section-title {
  display: flex;
  padding: 2px 0;
  font-size: 12px;
  color: gray;
}

.report-reason {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 8px;
  font-size: 12px;
}

.reason-button {
  padding: 8px 12px;
  border: 1px solid #ddd;
  border-radius: 6px;
  text-align: center;
  cursor: pointer;
  background-color: #f8f9fa;
  color: #333;
  font-size: 12px;
  white-space: nowrap;
  overflow: hidden;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
}

.reason-button:hover {
  background-color: #e9ecef;
  border-color: #adb5bd;
}

.reason-button.active {
  background-color: #007bff;
  color: white;
  border-color: #007bff;
}

.report-reason-discription {
  width: 100%;
  height: 60px;
  border: 1.5px solid #ddd;
  border-radius: 6px;
  font-size: 12px;
}

.report-reason-discription:focus {
  outline: none;
  border-color: #007bff;
  box-shadow: 0 0 0 2px rgba(0, 123, 255, 0.25);
}

.submit-button {
  width: 80px;
  height: 30px;
  font-size: 12px;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto;
  background-color: rgba(0, 174, 236);
  border: 1px solid gray;
  border-radius: 8px;
  cursor: pointer;
}
</style>