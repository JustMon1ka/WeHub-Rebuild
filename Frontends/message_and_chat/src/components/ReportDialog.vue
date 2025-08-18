<template>
  <teleport to="body">
    <div
      v-if="visible"
      class="report-dialog-overview"
      @click.self="handleCloseClick"
    >
      <div class="report-dialog-content">
        <div class="dialog-header">
          <span class="dialog-title">我要举报</span>
          <span class="close-button" @click="handleCloseClick">×</span>
        </div>

        <div class="report-section">
          <span class="report-section-title">违反法律法规</span>
          <div class="report-reason">
            <label
              class="reason-button"
              :class="{ active: reportReasons.includes('illegal') }"
            >
              <input
                type="checkbox"
                v-model="reportReasons"
                value="illegal"
                hidden
              />违法违禁
            </label>
            <label
              class="reason-button"
              :class="{ active: reportReasons.includes('gambling') }"
            >
              <input
                type="checkbox"
                v-model="reportReasons"
                value="gambling"
                hidden
              />赌博诈骗
            </label>
            <label
              class="reason-button"
              :class="{ active: reportReasons.includes('infringement') }"
            >
              <input
                type="checkbox"
                v-model="reportReasons"
                value="infringement"
                hidden
              />侵权申诉
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
            <label
              class="reason-button"
              :class="{ active: reportReasons.includes('false_info') }"
            >
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
              <input
                type="checkbox"
                v-model="reportReasons"
                value="inciting_conflict"
                hidden
              />引战
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
              <input
                type="checkbox"
                v-model="reportReasons"
                value="pornographic"
                hidden
              />色情低俗
            </label>

            <label
              class="reason-button"
              :class="{
                active: reportReasons.includes('uncomfortable'),
              }"
            >
              <input
                type="checkbox"
                v-model="reportReasons"
                value="uncomfortable"
                hidden
              />观感不适
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
              <input
                type="checkbox"
                v-model="reportReasons"
                value="other"
                hidden
              />其他
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

        <div class="dialog-footer">
          <button class="cancel-button" @click="handleCloseClick">取消</button>
          <button class="submit-button" @click="handleSubmitClick">提交</button>
        </div>
      </div>
    </div>
  </teleport>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, onUnmounted } from "vue";

interface Props {
  visible: boolean;
  reportTargetId: number;
  reportTargetType: "message" | "user" | "post";
}
const props = defineProps<Props>();

interface Emits {
  (e: "close"): void; // 关闭弹窗
  (
    e: "submit",
    payload: {
      reasons: string[];
      description: string;
      targetId?: number;
      targetType?: string;
    }
  ): void; // 提交举报
}
const emits = defineEmits<Emits>();

const reportReasons = ref<string[]>([]);
const description = ref<string>("");

const handleCloseClick = () => {
  emits("close");
  reportReasons.value = [];
  description.value = "";
};

const handleSubmitClick = () => {
  if (!reportReasons.value) {
    alert("请选择举报原因");
    return;
  }
  if (!description.value) {
    alert("请详细描述举报原因");
    return;
  }

  emits("submit", {
    reasons: reportReasons.value,
    description: description.value.trim(),
    targetId: props.reportTargetId,
    targetType: props.reportTargetType,
  });
  handleCloseClick();
};
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

.close-button {
  font-size: 20px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  transition: background-color 0.2s;
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

.dialog-footer {
  padding: 6px 12px;
  display: flex;
  justify-content: space-between;
  gap: 12px;
}

.cancel-button {
  width: 80px;
  font-size: 12px;
  color: gray;
  height: 30px;
  background-color: #fff;
  border: 1px solid gray;
  border-radius: 8px;
  cursor: pointer;
}

.submit-button {
  width: 80px;
  font-size: 12px;
  color: white;
  background-color: rgba(0, 174, 236);
  border: 1px solid gray;
  border-radius: 8px;
  cursor: pointer;
}
</style>