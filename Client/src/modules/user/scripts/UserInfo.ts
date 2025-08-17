import { User } from '@/modules/auth/public.ts'
import { nextTick, type Ref, ref } from 'vue'
import { toggleLoginHover } from '@/App.vue'
import {
  getUserDataAPI,
  setUserProfileAPI,
  type UserData,
  type userProfileData
} from '@/modules/user/scripts/UserDataAPI.ts'
import { getTagsAPI } from '@/modules/user/scripts/UserTagAPI.ts'

enum state {
  LoggedOut,
  NetworkError,
  Success,
}

class UserInfo implements UserData{
  static errorMsg = {
    NetworkError: '网络错误，请稍后再试。',
    LoggedOut: '您已登出，请重新登录。',
    PictureFormatError: '图片格式有误，请上传有效的图片文件。',
    PictureSizeError: '图片大小超过限制，请上传小于3MB的图片。',
    DefaultError: '发生未知错误，请稍后再试。',
  }
  static TagMap: Map<string, string> = new Map([]);

  static getTagName(tag: string): string {
    // TODO: Get tag name by tag id, if not exist, create a new tag.
    if (UserInfo.TagMap.has(tag)) {
      return UserInfo.TagMap.get(tag) || "";
    } else {
      // Fetch tag name from server or create a new tag
      return "";
    }
  }

  userId: string;
  profileLoaded: Ref<boolean> = ref(false);
  followLoaded: Ref<boolean> = ref(false);
  tagsLoaded: Ref<boolean> = ref(false);


  username: string = '';
  phone: string = '';
  email: string = '';
  createdAt: string = '';
  password: string = '';

  profileURL: string = '';
  avatarURL: string = '';
  status: string = '';
  nickName: string = '';
  birthday: string = '';
  location: string = '';
  bio: string = '';
  experience: number = 0;
  gender: string = '';
  level: number = 0;

  followerCount: number = 0;
  followingCount: number = 0;

  userTags :Map<string, string> = new Map(); // Loading only when isMe is true

  isMe: boolean = false;

  // Edit related
  changed: boolean = false;
  error: boolean = false;
  errorMsg: string = '';


  constructor(userId: string, copy: boolean = false) {
    this.userId = userId;
    if (copy) return;

    if (userId === User.getInstance()?.userAuth?.userId) {
      this.isMe = true;
    }

    // fetch from API
    if (this.nickName === '') {
      this.nickName = 'Anonymous';
    }

    nextTick(async () => {await this.loadUserData();});
  }

  async loadProfile() {
    const result = await getUserDataAPI(this.userId);
    if (result.code !== 200){
      this.error = true;
      throw new Error(result.message || UserInfo.errorMsg.DefaultError);
    }
    const userProfile: UserData = result.data;

    this.username = userProfile.username;
    this.phone = userProfile.phone;
    this.email = userProfile.email;
    this.createdAt = userProfile.createdAt.split('T')[0]; // 只保留日期部分
    this.password = '';

    this.profileURL = userProfile.profileURL || '';
    this.avatarURL = userProfile.avatarURL || '';
    this.status = userProfile.status || '';
    this.experience = userProfile.experience || 0;
    this.nickName = userProfile.nickName || this.username;
    this.birthday = userProfile.birthday.split('T')[0] || this.createdAt;
    if (this.birthday === '0001-01-01'){
      this.birthday = '2000-01-01'; // 默认生日
    }

    this.location = userProfile.location || '未知';
    this.bio = userProfile?.bio || '';
    this.gender = userProfile.gender || '未知';
    this.level = userProfile.level || 0;
    this.profileLoaded.value = true;
  }

  async loadFollow() {
    // TODO: Get following and follower count
  }

  async loadTags() {
    const result = await getTagsAPI(this.userId);
    if (result.code !== 200) {
      throw new Error(result.message || UserInfo.errorMsg.DefaultError);
    }
    for (const tag of result.data.tags) {
      const tagName = UserInfo.getTagName(tag);
      if (tagName) {
        this.userTags.set(tag, tagName);
      }
    }
    this.tagsLoaded.value = true;
  }

  async loadUserData() {
    try {
      await this.loadProfile();
      await this.loadFollow();
      if (this.isMe) await this.loadTags();
    } catch (error) {
      this.errorHandler(error);
      this.error = true;
      this.errorMsg = UserInfo.errorMsg.NetworkError;
    }
  }

  copy(copyUserInfo: UserInfo = new UserInfo(this.userId, true)): UserInfo {
    // 只需要复制可编辑的字段，
    // 假如传入了一个 UserInfo 实例，则复制到该实例中，防止放弃编辑时错误修改原主页内容
    copyUserInfo.profileURL = this.profileURL;
    copyUserInfo.avatarURL = this.avatarURL;

    copyUserInfo.nickName = this.nickName;
    copyUserInfo.birthday = this.birthday;
    copyUserInfo.location = this.location;
    copyUserInfo.bio = this.bio;

    copyUserInfo.userTags = new Map(this.userTags);
    return copyUserInfo;
  }

  async updateProfile() {
    if (!this.isMe || !this.changed || !this.profileLoaded) {
      this.error = true;
      this.errorMsg = UserInfo.errorMsg.DefaultError;
      return false;
    }
    try {
      const result = await setUserProfileAPI(this.userId, <userProfileData>{
        profileURL: this.profileURL,
        avatarURL: this.avatarURL,
        experience: this.experience,
        gender: this.gender,
        level: this.level,
        nickName: this.nickName,
        birthday: this.birthday,
        location: this.location,
        bio: this.bio
      })
      if (result.code != 200) {
        this.error = true;
        this.errorMsg = result.message || UserInfo.errorMsg.DefaultError;
        return false;
      }
      return true;
    } catch (error) {
      this.errorHandler(error);
      return false;
    }
  }

  async uploadPicture(event: Event, type: 'avatar' | 'profile') {
    const file = (event.target as HTMLInputElement)?.files?.[0];
    if (!file) return;
    const maxSize = 3 * 1024 * 1024;
    if (file.size > maxSize) {
      this.error = true;
      this.errorMsg = UserInfo.errorMsg.PictureSizeError;
      return;
    }

    this.changed = true;

    // TODO: Upload picture to server
  }

  errorHandler(error: any) {
    if (error.message === 'Network Error') {
      this.error = true;
      this.errorMsg = UserInfo.errorMsg.NetworkError;
    }

    if (error.message === 'Unauthorized') {
      this.error = true;
      this.errorMsg = UserInfo.errorMsg.LoggedOut;
      toggleLoginHover();
    }

    else {
      this.error = true;
      this.errorMsg = error.message || UserInfo.errorMsg.DefaultError;
    }
  }
}

export default  UserInfo;
export { state, UserInfo };
