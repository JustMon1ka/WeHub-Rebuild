import { User } from '@/modules/auth/public.ts'
import {
  getUserDataAPI,
  setUserProfileAPI,
  type UserData,
  type userProfileData
} from '@/modules/user/scripts/UserDataAPI.ts'
import { getTagsAPI, setTagsAPI } from '@/modules/user/scripts/UserTagAPI.ts'
import tags from '@/modules/user/scripts/tags.json'

class UserInfo implements UserData{
  static readonly errorMsg = {
    PictureFormatError: '图片格式有误，请上传有效的图片文件。',
    PictureSizeError: '图片大小超过限制，请上传小于3MB的图片。',
    NicknameLengthError: '昵称长度必须在1到20个字符之间。',
    BioLengthError: '个人简介长度不能超过100个字符。',
    DefaultError: '发生未知错误，请稍后再试。',
  }

  static readonly msgTranslation = new Map<string, string>([
    ["Username or Email or Phone already exists.", "用户名或邮箱或手机号已存在"],
    ["User not found", "用户未找到"],
    ["User profile not found", "用户资料未找到"],
    ["Username already exists.", "用户名已存在"],
    ["User info updated successfully", "用户信息更新成功"],
    ["Email or phone already exists.", "邮箱或手机号已存在"],
    ["User updated successfully", "用户信息更新成功"],
    ["User deleted successfully", "用户已删除"],
    ["User info updated successfully", "用户信息更新成功"],
    ["User deleted successfully", "用户已删除"],
    ["Failed to fetch", "获取数据失败，请检查网络连接"],
    ["Tags updated successfully", "标签更新成功"],
    ["Tag added successfully", "标签添加成功"],
    ["Tag deleted successfully", "标签删除成功"],
    ["You are not authorized to modify this user", "您无权修改此用户"],
    ["Unauthorized", "未授权访问"],
    ["Failed to update tags", "更新标签失败"],
  ]);

  static TagMap: Map<string, string> = new Map([]);

  static {
    for (const [tagGroup, tagNames] of Object.entries(tags)) {
      for (const [tagId, tagName] of Object.entries(tagNames)) {
        // 使用 tagGroup 作为前缀，保证 tagId 的唯一性
        UserInfo.TagMap.set(tagId, tagName);
      }
    }
  }

  static async getTagName(tagId: string): Promise<string> {
    // Get tag name by tag id, if not exist in TagMap, fetch from server
    if (UserInfo.TagMap.has(tagId)) {
      return UserInfo.TagMap.get(tagId) || "";
    } else {
      // TODO: 从服务器获取标签名
      return "";
    }
  }

  static async getTagId(tagName: string) {
    for (const [id, name] of UserInfo.TagMap.entries()) {
      if (name === tagName) {
        return id;
      }
    }
    return '';
    // TODO: 如果 TagMap 中没有该标签名，向服务器请求获取或者创建新标签
  }

  userId: string;
  profileLoaded: boolean = false;
  followLoaded: boolean = false;
  tagsLoaded: boolean = false;


  username: string = '';
  phone: string = '';
  email: string = '';
  createdAt: string = '';
  password: string = '';

  profileURL: string = '';
  avatarURL: string = '';
  status: string = '';
  nickname: string = '';
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
    if (this.nickname === '') {
      this.nickname = 'Anonymous';
    }
    if (this.username === '') {
      this.username = 'Anonymous';
    }
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
    this.nickname = userProfile.nickname || this.username;
    this.birthday = userProfile.birthday.split('T')[0] || this.createdAt;
    if (this.birthday === '0001-01-01'){
      this.birthday = '2000-01-01'; // 默认生日
    }

    this.location = userProfile.location || '未知';
    this.bio = userProfile?.bio || '';
    this.gender = userProfile.gender || '未知';
    this.level = userProfile.level || 0;
    this.profileLoaded = true;
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
      const tagName = await UserInfo.getTagName(tag.toString());
      if (tagName) {
        this.userTags.set(tag, tagName);
      }
    }
    this.tagsLoaded = true;
  }

  async loadUserData() {
    try {
      await this.loadProfile();
      await this.loadFollow();
      if (this.isMe) await this.loadTags();
    } catch (error:any) {
      this.handleError(error);
    }
  }

  copy(copyUserInfo: UserInfo = new UserInfo(this.userId, true)): UserInfo {
    // 只需要复制可编辑的字段，
    // 假如传入了一个 UserInfo 实例，则复制到该实例中，防止放弃编辑时错误修改原主页内容
    copyUserInfo.userId = this.userId;
    copyUserInfo.isMe = this.isMe;
    copyUserInfo.username = this.username;

    copyUserInfo.profileURL = this.profileURL;
    copyUserInfo.avatarURL = this.avatarURL;

    copyUserInfo.nickname = this.nickname;
    copyUserInfo.birthday = this.birthday;
    copyUserInfo.location = this.location;
    copyUserInfo.bio = this.bio;

    copyUserInfo.userTags = new Map(this.userTags);
    return copyUserInfo;
  }

  async updateProfile() {
    if (!this.isMe || !this.changed) {
      this.error = true;
      this.errorMsg = UserInfo.errorMsg.DefaultError;
      return false;
    }

    if (this.nickname.length < 1 || this.nickname.length > 20) {
      this.error = true;
      this.errorMsg = UserInfo.errorMsg.NicknameLengthError;
      return false;
    }

    if (this.bio.length > 100) {
      this.error = true;
      this.errorMsg = UserInfo.errorMsg.BioLengthError;
      return false;
    }

    try {
      await setUserProfileAPI(this.userId, <userProfileData>{
        profileURL: this.profileURL,
        avatarURL: this.avatarURL,
        experience: this.experience,
        gender: this.gender,
        level: this.level,
        nickname: this.nickname,
        birthday: this.birthday,
        location: this.location,
        bio: this.bio
      })

      await setTagsAPI(this.userId, {
        tags: [...this.userTags.keys()].map((tagId) => Number(tagId)),
      });
      this.error = false;
      this.errorMsg = '';
      return true;
    } catch (error:any) {
      this.handleError(error);
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

  async updateTags() {
    try {
      const result = await setTagsAPI(this.userId, {
        tags: [...this.userTags.keys()].map((tagId) => Number(tagId)),
      });
    } catch (error : any) {
      this.handleError(error);
    }
  }

  handleError(error: any) {
    this.error = true;
    if (error.message && UserInfo.msgTranslation.has(error.message)) {
      this.errorMsg = UserInfo.msgTranslation.get(error.message) || UserInfo.errorMsg.DefaultError;
    } else {
      this.errorMsg = error.message || UserInfo.errorMsg.DefaultError;
    }
  }
}

export default  UserInfo;
export { UserInfo };
