// modules/Founding/store.ts
import { defineStore } from "pinia"
import { fetchHotTopics, fetchRecommendUsers, fetchTodayHot } from "./api"
import type { HotTopic, RecommendUser, TodayHot } from "./types"

export const useFoundingStore = defineStore("founding", {
  state: () => ({
    hotTopics: [] as HotTopic[],
    recommendUsers: [] as RecommendUser[],
    todayHot: [] as TodayHot[],
    loading: false,
  }),
  actions: {
    async loadAll() {
      this.loading = true
      try {
        const [topics, users, hots] = await Promise.all([
          fetchHotTopics(),
          fetchRecommendUsers(),
          fetchTodayHot(),
        ])
        this.hotTopics = topics
        this.recommendUsers = users
        this.todayHot = hots
      } finally {
        this.loading = false
      }
      console.log("store.loadAll called ✅")
    },
  },
})
