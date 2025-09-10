// src/modules/circle/store.ts
import { ref, reactive } from 'vue'
import { CircleAPI } from './api'

interface Community {
  id: number
  name: string
  description: string
  memberCount: number
  category: string
  isPrivate: boolean
  isJoined: boolean
  isLoading: boolean
  createdAt: string
  ownerId?: number
}

// 全局状态
const allCommunities = ref<Community[]>([])
const joinedCommunities = ref<Community[]>([])
const loadingStates = reactive<Record<number, boolean>>({})

export const useCommunityStore = () => {
  // 设置所有社区列表
  const setAllCommunities = (communities: Community[]) => {
    allCommunities.value = communities
  }

  // 设置已加入社区列表
  const setJoinedCommunities = (communities: Community[]) => {
    joinedCommunities.value = communities
  }

  // 获取社区的加入状态
  const getCommunityJoinStatus = (communityId: number): boolean => {
    return joinedCommunities.value.some((c) => c.id === communityId)
  }

  // 加入社区
  const joinCommunity = async (communityId: number) => {
    try {
      loadingStates[communityId] = true

      const response = await CircleAPI.joinCircle(communityId)

      if (response && (response.success || response.code === 200)) {
        // 更新所有社区列表中的状态
        const allCommunity = allCommunities.value.find((c) => c.id === communityId)
        if (allCommunity) {
          allCommunity.isJoined = true
          allCommunity.memberCount += 1

          // 添加到已加入列表
          const joinedCommunity = { ...allCommunity }
          joinedCommunities.value.push(joinedCommunity)
        }

        console.log('成功加入社区')
        return { success: true }
      }

      return response
    } catch (error) {
      console.error('加入社区失败:', error)
      throw error
    } finally {
      loadingStates[communityId] = false
    }
  }

  // 退出社区
  const leaveCommunity = async (communityId: number) => {
    try {
      loadingStates[communityId] = true

      const response = await CircleAPI.leaveCircle(communityId)

      if (response && (response.success || response.code === 200)) {
        // 更新所有社区列表中的状态
        const allCommunity = allCommunities.value.find((c) => c.id === communityId)
        if (allCommunity) {
          allCommunity.isJoined = false
          allCommunity.memberCount = Math.max(allCommunity.memberCount - 1, 0)
        }

        // 从已加入列表中移除
        joinedCommunities.value = joinedCommunities.value.filter((c) => c.id !== communityId)

        console.log('成功退出社区')
        return { success: true }
      }

      return response
    } catch (error) {
      console.error('退出社区失败:', error)
      throw error
    } finally {
      loadingStates[communityId] = false
    }
  }

  // 切换加入状态
  const toggleCommunityMembership = async (communityId: number) => {
    const isJoined = getCommunityJoinStatus(communityId)

    if (isJoined) {
      return await leaveCommunity(communityId)
    } else {
      return await joinCommunity(communityId)
    }
  }

  // 获取加载状态
  const getCommunityLoadingState = (communityId: number): boolean => {
    return !!loadingStates[communityId]
  }

  // 更新单个社区信息
  const updateCommunity = (communityId: number, updates: Partial<Community>) => {
    const allCommunity = allCommunities.value.find((c) => c.id === communityId)
    if (allCommunity) {
      Object.assign(allCommunity, updates)
    }

    const joinedCommunity = joinedCommunities.value.find((c) => c.id === communityId)
    if (joinedCommunity) {
      Object.assign(joinedCommunity, updates)
    }
  }

  return {
    // 状态
    allCommunities: allCommunities,
    joinedCommunities: joinedCommunities,

    // 方法
    setAllCommunities,
    setJoinedCommunities,
    getCommunityJoinStatus,
    joinCommunity,
    leaveCommunity,
    toggleCommunityMembership,
    getCommunityLoadingState,
    updateCommunity,
  }
}
