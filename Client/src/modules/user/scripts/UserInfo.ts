import { User } from '@/modules/auth/public.ts'
import {
  getUserDataAPI, setUserProfileAPI,
  type UserData, type userProfileData
} from '@/modules/user/scripts/UserDataAPI.ts'
import { getTagsAPI, setTagsAPI } from '@/modules/user/scripts/UserTagAPI.ts'
import { uploadMediaAPI, MEDIA_BASE_URL } from '@/modules/user/scripts/MediaAPI.ts'
import { addTagsAPI, getTagsNameAPI, type SingleTagData } from '@/modules/user/scripts/TagAPI.ts'

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

  // 以下为用户信息
  userId: string;

  username: string = 'Loading...';
  phone: string = '';
  email: string = '';
  createdAt: string = '';
  password: string = '';

  profileId: string = '';
  profileUrl: string = '';
  avatarId: string = '';
  avatarUrl: string = '';
  status: string = '';
  nickname: string = 'Loading...';
  birthday: string = '';
  location: string = '';
  bio: string = '';
  experience: number = 0;
  gender: string = '';
  level: number = 0;

  followingCount: number = 0;  // 用户的关注人数
  followerCount: number = 0;  // 用户的粉丝人数

  userTags :Map<string, string> = new Map(); // Loading only when isMe is true
  newTagNames: Set<string> = new Set(); // 用于存储新添加的标签名称
  noConflictId: number = -1; // 用于生成不冲突的标签ID

  // 以下为状态量
  profileLoaded: boolean = false;
  followLoaded: boolean = false;
  tagsLoaded: boolean = false;

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


  async loadProfile() {
    const result = await getUserDataAPI(this.userId);
    if (result.code !== 200){
      this.error = true;
      throw new Error(result.msg || UserInfo.errorMsg.DefaultError);
    }
    const userProfile: UserData = result.data;

    this.username = userProfile.username;
    this.phone = userProfile.phone;
    this.email = userProfile.email;
    this.createdAt = userProfile.createdAt.split('T')[0]; // 只保留日期部分
    this.password = '';

    this.profileId = userProfile.profileUrl || '';
    this.profileUrl = this.profileId ? `${MEDIA_BASE_URL}/${this.profileId}` : '';
    this.avatarId = userProfile.avatarUrl || '';
    this.avatarUrl = this.avatarId ? `${MEDIA_BASE_URL}/${this.avatarId}` : '';
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
      throw new Error(result.msg || UserInfo.errorMsg.DefaultError);
    }

    if (result.data.tags.length === 0) {
      this.tagsLoaded = true;
      return;
    }

    const tagResult = await getTagsNameAPI(result.data.tags);
    if (tagResult.code !== 200) {
      throw new Error(tagResult.msg || UserInfo.errorMsg.DefaultError);
    }

    for (const tag of tagResult.data) {
      this.userTags.set(tag.tagId.toString(), tag.tagName);
    }
    this.tagsLoaded = true;
  }

  copy(copyUserInfo: UserInfo = new UserInfo(this.userId, true)): UserInfo {
    // 只需要复制可编辑的字段，
    // 假如传入了一个 UserInfo 实例，则复制到该实例中，防止放弃编辑时错误修改原主页内容
    copyUserInfo.userId = this.userId;
    copyUserInfo.isMe = this.isMe;
    copyUserInfo.username = this.username;

    copyUserInfo.profileUrl = this.profileUrl;
    copyUserInfo.avatarUrl = this.avatarUrl;

    copyUserInfo.nickname = this.nickname;
    copyUserInfo.birthday = this.birthday;
    copyUserInfo.location = this.location;
    copyUserInfo.bio = this.bio;

    copyUserInfo.userTags = new Map(JSON.parse(JSON.stringify(Array.from(this.userTags))));
    return copyUserInfo;
  }

  addTag(newTagName: string) {
    for (const tagName of this.userTags.values()) {
      if (tagName === newTagName) {
        return;
      }
    }
    this.newTagNames.add(newTagName);
    this.userTags.set(this.noConflictId.toString(), newTagName);
    this.noConflictId -= 1;
  }

  removeTag(oldTagId: string, oldTagName: string) {
    if (this.newTagNames.has(oldTagName)) {
      this.newTagNames.delete(oldTagName);
    }
    this.userTags.delete(oldTagId);
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
        profileUrl: this.profileId,
        avatarUrl: this.avatarId,
        experience: this.experience,
        gender: this.gender,
        level: this.level,
        nickname: this.nickname,
        birthday: this.birthday,
        location: this.location,
        bio: this.bio
      })

      await this.updateTags();
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

    try {
      const result = await uploadMediaAPI(this.userId, file);
      if (type === 'avatar') {
        this.avatarId = result.data.fileId;
        this.avatarUrl = this.avatarId ? `${MEDIA_BASE_URL}/${this.avatarId}` : '';
      } else {
        this.profileId = result.data.fileId;
        this.profileUrl = this.profileId ? `${MEDIA_BASE_URL}/${this.profileId}` : '';
      }
    } catch (error) {
      this.handleError(error);
      return;
    }

    this.changed = true;
  }

  async updateTags() {
    const tagId: Array<number> = [];
    for (const tag of this.userTags.keys()){
      if (Number(tag) > 0) {
        tagId.push(Number(tag));
      }
    }

    if (this.newTagNames.size !== 0) {
      const tagResult = await addTagsAPI([...this.newTagNames]); // 添加新标签
      if (tagResult.code !== 200) {
        throw new Error(tagResult.msg || UserInfo.errorMsg.DefaultError);
      }
      for (const tag of tagResult.data)
        tagId.push(tag.tagId);
    }

    const result = await setTagsAPI(this.userId, {
      tags: tagId,
    });
    if (result.code !== 200) {
      throw new Error(result.msg || UserInfo.errorMsg.DefaultError);
    }
  }

  handleError(error: any) {
    this.error = true;
    console.error(error);
    if (error.message && UserInfo.msgTranslation.has(error.message)) {
      this.errorMsg = UserInfo.msgTranslation.get(error.message) || UserInfo.errorMsg.DefaultError;
    } else {
      this.errorMsg = error.message || UserInfo.errorMsg.DefaultError;
    }
  }
}

export default  UserInfo;
export { UserInfo };
