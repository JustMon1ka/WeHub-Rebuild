import type { Ref } from 'vue'
import { ref } from 'vue'
import { User } from '@/modules/auth/public.ts'

class UserInfo {
  userId: Ref<string>;
  userName: Ref<string> = ref('');
  email: Ref<string> = ref('');
  profilePictureUrl?: Ref<string>;
  userAvatarUrl?: Ref<string>;
  createdAt: Date = new Date();
  address?: Ref<string>
  bio?: Ref<string>
  website?: Ref<string>;
  followersCount: Ref<number> = ref(0);
  followingCount: Ref<number> = ref(0);
  isMe: boolean = false;

  constructor(userId: string) {
    this.userId = ref(userId);
    this.isMe = true;
    if (userId === User.getInstance()?.userAuth?.userId) {

    }
    // fetch from API
  }

  updateProfile(){

  }
}

export default  UserInfo;
