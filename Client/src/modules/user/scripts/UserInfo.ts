import { User } from '@/modules/auth/public.ts'
import { ref } from 'vue'
import { toggleLoginHover } from '@/App.vue'

enum state {
  LoggedOut,
  NetworkError,
  Success,
}

class UserInfo {
  static errorMsg = {
    NetworkError: '网络错误，请稍后再试。',
    LoggedOut: '您已登出，请重新登录。',
    PictureFormatError: '图片格式有误，请上传有效的图片文件。',
    PictureSizeError: '图片大小超过限制，请上传小于2MB的图片。',
  }

  userId: string;

  profilePictureUrl: string = '';
  userAvatarUrl: string = '';

  nickName: string = '';
  birthday: string = '';
  level: number = 0;
  address: string = '';
  bio: string = '';
  isMale: boolean = true;

  followerCount: number = 0;
  followingCount: number = 0;

  userTags :Set<string> = new Set(); // Loading only when isMe is true

  isMe: boolean = false;

  // Edit related
  changed: boolean = false;
  error: boolean = false;
  errorMsg: string = '';
  bgInput = ref<HTMLInputElement | null>(null);
  avatarInput = ref<HTMLInputElement | null>(null);


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
  }

  copy() {
    const copyUserInfo = new UserInfo(this.userId, true);
    copyUserInfo.profilePictureUrl = this.profilePictureUrl;
    copyUserInfo.userAvatarUrl = this.userAvatarUrl;

    copyUserInfo.nickName = this.nickName;
    copyUserInfo.birthday = this.birthday;
    copyUserInfo.address = this.address;
    copyUserInfo.bio = this.bio;
    copyUserInfo.level = this.level;

    copyUserInfo.isMale = this.isMale;
    copyUserInfo.followerCount = this.followerCount;
    copyUserInfo.followingCount = this.followingCount;
    copyUserInfo.isMe = this.isMe;

    copyUserInfo.userTags = new Set(this.userTags);

    return copyUserInfo;
  }

  static async sendData(url: string, data: object) {
    const response = await fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(data)
    });

    if (!response.ok) {
      return state.NetworkError;
    } else {
      const result = await response.json();
      if (result.success) {
        return state.Success;
      }
    }
    return state.LoggedOut;
  }

  async updateProfile() {
    const cur_state = await UserInfo.sendData('/api/user/update', {});
    switch (cur_state){
      case state.NetworkError:
        this.error = true;
        this.errorMsg = UserInfo.errorMsg.NetworkError;
        break;
      case state.LoggedOut:
        toggleLoginHover(true);
        this.error = true;
        this.errorMsg = UserInfo.errorMsg.LoggedOut;
        break;
      case state.Success:
        this.changed = false;
        this.error = false;
        this.errorMsg = '';
        return true;
      default:
    }
    return false;
  }

  async uploadPicture() {
    this.changed = true;
    if (!this.avatarInput.value || !this.bgInput.value) {
      this.error = true;
      this.errorMsg = UserInfo.errorMsg.PictureFormatError;
      return false;
    }
    // TODO: Check file format and size
  }
}

export default  UserInfo;
export { state, UserInfo };
