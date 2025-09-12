import router from '@/router.ts'
import { UserInfo } from '@/modules/user/public.ts'
import {
  loginAPI,
  meAPI, refreshTokenAPI,
  registerAPI,
  sendCodeAPI,
  verifyCodeAPI
} from '@/modules/auth/scripts/UserAuthAPI.ts'
import { setUserAuthDataAPI } from '@/modules/user/scripts/UserDataAPI.ts'
import { ref, type Ref } from 'vue'
import {
  addFollowingAPI,
  getFollowCountAPI,
  getFollowingAPI, removeFollowingAPI
} from '@/modules/user/scripts/FollowAPI.ts'

interface resultState {
  success: boolean;
  error?: string;
}


class User {
  static #singleton: User | undefined = undefined;
  // 获取当前登录用户的用户名，如果未登录则返回undefined
  static getInstance(): User | undefined {
    return User.#singleton;
  }

  static readonly COOKIE_AGE = 60 * 60 * 24 * 60; // 60 days
  static readonly CHECK_INTERVAL = 1000 * 60 * 5; // 每5分钟检查一次
  static readonly REFRESH_THRESHOLD = 1000 * 60 * 6; // 提前6分钟刷新
  static loading = false;
  static afterLoadCallbacks: (() => void)[] = [];

  // 静态块用于初始化用户实例
  static {
    const token_s = sessionStorage.getItem('token');
    let creating = false;
    if (token_s) {
      const tokenExpiration = User.getTokenExpiration(token_s);
      if(tokenExpiration && Date.now() < tokenExpiration - User.REFRESH_THRESHOLD) {
        User.create(token_s, 'session').catch(async () => {
          sessionStorage.removeItem('token');
          await User.createFromCookie();
        }).catch((e) => {}) // 捕获可能的错误，避免影响页面加载
        creating = true;
      }
    }
    if (!creating) {
      User.createFromCookie().catch((e) => {}) // 捕获可能的错误，避免影响页面加载
    }
  }

  static async createFromCookie() {
    for (const cookie of document.cookie.split('; ')) {
      const [name, value] = cookie.split('=');
      if (name === 'token') {
        User.create(value, 'auth').catch(() => {
          document.cookie = 'token=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/'; // 清除无效的token
        })
      }
    }
  }

  // 以下为静态工具函数
  static async generateHash(password: string) {
    // 1. 将密码转换为 Uint8Array
    const encoder = new TextEncoder();
    const data = encoder.encode(password);

    // 2. 使用 Web Crypto API 计算哈希
    const hashBuffer = await crypto.subtle.digest('SHA-256', data);

    // 3. 将哈希结果转换为十六进制字符串
    const hashArray = Array.from(new Uint8Array(hashBuffer));
    return hashArray.map(b => b.toString(16).padStart(2, '0')).join('');
  }

  static getTokenExpiration(token: string): number | null {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.exp ? payload.exp * 1000 : null; // 返回毫秒时间戳
    } catch {
      return null;
    }
  }

  static readonly msgTranslation = new Map<string, string>([
    ["Username or Email or Phone already exists.", "用户名或邮箱或手机号已存在"],
    ["User not found", "用户未找到"],
    ["User profile not found", "用户资料未找到"],
    ["Username already exists.", "用户名已存在"],
    ["User info updated successfully", "用户信息更新成功"],
    ["Registration successful.", "注册成功"],
    ["Email or phone already exists.", "邮箱或手机号已存在"],
    ["User updated successfully", "用户信息更新成功"],
    ["User deleted successfully", "用户已删除"],
    ["Failed to fetch", "获取数据失败，请检查网络连接"],
    ["Invalid credentials", "用户名或密码错误"],
    ["You are not authorized to modify this user", "您无权修改此用户"],
    ["Unauthorized", "请登录后再操作"],
    ["Email not registered", "邮箱未注册"],
    ["Invalid code.", "验证码错误"],
    ["Login failed, no token generated", "登录失败，未生成令牌"],
  ]);

  static handleError(error: any): resultState {
    let errorMsg = '未知错误';
    if (error.message && User.msgTranslation.has(error.message)) {
      errorMsg = User.msgTranslation.get(error.message) || errorMsg;
    } else {
      errorMsg = error.message || errorMsg;
    }
    return {
      success: false,
      error: errorMsg,
    }
  }

  // 以下为接入后端API的方法
  static async login(username: string, password: string, rememberMe: boolean = false) : Promise<resultState> {
    let passwordHash = await User.generateHash(password);
    try {
      // 发送登录请求
      const login = await loginAPI({
        identifier: username,
        password: passwordHash,
      });

      await User.create(login.data, 'auth');
      if (rememberMe) {
        document.cookie = `token=${login.data}; path=/; max-age=${User.COOKIE_AGE}`;
      }

      return {
        success: true,
      };
    }
    catch (error: any) {
      return User.handleError(error);
    }
  }

  static async register(username: string, password: string, email: string, code: string, phone: string, rememberMe: boolean = false) : Promise<resultState> {
    let passwordHash = await User.generateHash(password);
    try {
      // 发送注册请求
      const register = await registerAPI({
        username: username,
        password: passwordHash,
        email: email,
        code: code,
        phone: phone,
      });

      return await User.login(username, password, rememberMe);
    }
    catch (error: any) {
      return User.handleError(error);
    }
  }

  static async getSessionToken(token: string) {
    const sessionToken = (await refreshTokenAPI(token)).data;
    sessionStorage.setItem('token', sessionToken);
    return sessionToken || '';
  }

  static async sendAuthCode(email: string): Promise<resultState> {
    try {
      await sendCodeAPI(email);
      return {
        success: true,
      }
    } catch (error: any) {
      return User.handleError(error);
    }
  }

  static async verifyAuthCode(email: string, code: string, rememberMe: boolean) : Promise<resultState> {
    try {
      const login = await verifyCodeAPI({
        email: email,
        code: code,
      });

      await User.create(login.data, 'auth');
      if (rememberMe) {
        document.cookie = `token=${login.data}; path=/; max-age=${User.COOKIE_AGE}`;
      }

      return {
        success: true,
      }
    } catch (error: any) {
      return User.handleError(error);
    }
  }

  // 以下为用户实例的相关方法和属性
  readonly #userId: string;
  #token: string = '';
  #expiresTime: number | null = null;
  refreshTimer: any;
  followingList: Set<string> = new Set<string>();
  followerList: Set<string> = new Set<string>();
  userInfo: Ref<UserInfo>;

  // 工厂函数，用于创建用户实例，并在创建前检查错误
  static async create(token: string, tokenType: 'session' | 'auth') {
    try {
      User.loading = true;
      if (User.#singleton) return;
      if (!token) return;

      if (tokenType === 'auth') {
        token = await User.getSessionToken(token)
      }
      const userId = (await meAPI(token)).data.id
      const expiresTime = User.getTokenExpiration(token)

      if (!userId || !token || !expiresTime || Date.now() >= expiresTime) return
      User.#singleton = new User(userId, token, expiresTime)
    } finally {
      User.loading = false;
      User.afterLoadCallbacks.forEach(callback => {
        try {
          callback();
        } catch (e) {
          console.error('USER_ERROR_REPORT: Error in afterLoad callback:',callback.toString(),'ERROR_MESSAGE: ', e);
        }
      });
      User.afterLoadCallbacks = [];
    }
  }

  constructor(userid: string, token: string, expiresTime: number) {
    this.#userId = userid;
    this.#token = token;
    this.#expiresTime = expiresTime;

    this.refreshTimer = setInterval(async () => {
      if (this.#expiresTime && Date.now() >= this.#expiresTime - User.REFRESH_THRESHOLD) {
        this.#token = await User.getSessionToken(this.#token);
        this.#expiresTime = User.getTokenExpiration(this.#token);

        if (!this.#token || !this.#expiresTime) {
          User.#singleton?.logout();
          clearInterval(this.refreshTimer);
          return;
        }
      }
    }, User.CHECK_INTERVAL, { immediate: true }); // 每5分钟检查一次
    this.userInfo = ref(new UserInfo(this.#userId));

    setTimeout(async () => {
      await this.userInfo.value.loadUserData();
      await this.loadFollowList();
    }, 0);
  }

  async logout() {
    document.cookie.split('; ').forEach(cookie => {
      const [name] = cookie.split('=');
      if (name === 'token')
        document.cookie = `${name}=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/`;
    });
    sessionStorage.removeItem('token');
    User.#singleton = undefined;
    // 刷新当前页面
    await router.push('/');
    location.reload();
  }

  get userAuth() {
    return {
      userId: this.#userId.slice(), // 防止外部修改
      token: this.#token.slice(),
    }
  }

  async resetPassword(newPassword: string): Promise<resultState> {
    try {
      const passwordHash = await User.generateHash(newPassword);
      const result = await setUserAuthDataAPI(this.#userId , {
        username: this.userInfo.value?.username || '',
        password: passwordHash,
        email: this.userInfo.value?.email || '',
        phone: this.userInfo.value?.phone || '',
      });

      return {
        success: true,
      }
    } catch (error: any) {
      return User.handleError(error);
    }
  }

  async loadFollowList() {
    try {
      const followCountResult = await getFollowCountAPI(this.#userId);
      const followingCount = followCountResult.followingCount;
      const followerCount = followCountResult.followerCount;

      if (followingCount) {
        const followingResult = await getFollowingAPI(1, followingCount, this.#userId)
        for (const followData of followingResult.items) {
          this.followingList.add(followData.followeeId.toString());
        }
      }

      if (followerCount) {
        const followerResult = await getFollowingAPI(1, followerCount, this.#userId)
        for (const followData of followerResult.items) {
          this.followerList.add(followData.followerId.toString());
        }
      }

    } catch (error: any) {
      return User.handleError(error);
    }
  }

  followUser(userId: string) {
    try {
      const result = addFollowingAPI(userId);
      console.log(result);
      this.followingList.add(userId);
    } catch (error: any) {
      return User.handleError(error);
    }
  }

  unfollowUser(userId: string) {
    try {
      const result = removeFollowingAPI(userId);
      console.log(result);
      this.followingList.delete(userId);
    } catch (error: any) {
      return User.handleError(error);
    }
  }
}

export default User;
export { User, type resultState };
