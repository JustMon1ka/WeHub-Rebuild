import router from '@/router.ts'
import { UserInfo } from '@/modules/user/public.ts'
import { loginAPI, meAPI, registerAPI } from '@/modules/auth/scripts/UserAuthAPI.ts'

enum state {
  LoggedOut,
  NetworkError,
  DataError,
  Success,
}

interface resultState {
  state: state;
  error?: string;
  data?: string;
}


class User {
  static #singleton: User | undefined = undefined;
  // 获取当前登录用户的用户名，如果未登录则返回undefined
  static getInstance(): User | undefined {
    return User.#singleton;
  }

  static readonly MAX_COOKIE_AGE = 3600; // 1 hour
  static readonly MIN_COOKIE_AGE = 3600; // 1 hour

  static {
    let userId: string | undefined = undefined;
    let token: string | undefined = undefined;
    document.cookie.split('; ').forEach(cookie => {
      const [name, value] = cookie.split('=');
      if (name === 'userId') {
        userId = value;
      } else if (name === 'token') {
        token = value;
      }
    });
    if (userId && token) {
      User.#singleton = new User(userId, token);
      // User.#singleton?.logout(); //TODO: TEST ONLY
    }
  }

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

  static errorHandler(error: any): resultState {
    if (error.message === 'Network Error') {
      return {
        state: state.NetworkError,
        error: error.message,
      }
    }

    if (error.message === 'Unauthorized') {
      return {
        state: state.LoggedOut,
        error: 'Unauthorized',
      };
    }
    return {
      state: state.DataError,
      error: error.message || 'Unexpected error during login',
    };
  }

  static async login(username: string, password: string, rememberMe: boolean = false) : Promise<resultState> {
    let passwordHash = await User.generateHash(password);
    try {
      // 发送登录请求
      const login = await loginAPI({
        identifier: username,
        password: passwordHash,
      });
      if (login.code !== 200) {
        return {
          state: state.DataError,
          error: login.data.message || 'Login failed',
        };
      }

      // 获取用户信息
      const me = await meAPI(login.data);
      if (me.code !== 200) {
        return {
          state: state.DataError,
          error: me.data.message || 'Failed to retrieve user information',
        };
      }

      User.#singleton = new User(me.data.id, login.data);
      User.#singleton?.saveToCookie(rememberMe ? User.MAX_COOKIE_AGE : User.MIN_COOKIE_AGE);
      return {
        state: state.Success,
        data: "",
      };
    }
    catch (error: any) {
      return User.errorHandler(error);
    }
  }

  static async register(username: string, password: string, email: string, code: string, rememberMe: boolean = false) : Promise<resultState> {
    let passwordHash = await User.generateHash(password);
    try {
      // 发送注册请求
      const register = await registerAPI({
        username: username,
        password: passwordHash,
        email: email,
        code: code,
        phone: '', // TODO: 手机号可以留空
      });
      if (register.code !== 200) {
        return {
          state: state.DataError,
          error: register.data.message || 'Registration failed',
        };
      }

      return await User.login(username, password, rememberMe);
    }
    catch (error: any) {
      return User.errorHandler(error);
    }
  }

  static async sendAuthCode(email: string): Promise<resultState> {
    // TODO: 这里可以添加发送验证码请求的逻辑
    return {
      state: state.Success,
      data: '',
      error: '',
    }
  }

  static async verifyAuthCode(email: string, code: string, rememberMe: boolean) : Promise<resultState> {
    // TODO: 这里可以添加发送验证码验证请求的逻辑
    if (rememberMe){

    }
    return {
      state: state.Success,
      data: '',
      error: '',
    }
  }

  readonly #userid: string;
  readonly #token: string;
  followList: Set<string> = new Set<string>();
  userInfo: UserInfo | undefined = undefined;

  constructor(userid: string, token: string) {
    this.#userid = userid;
    this.#token = token;
    // 延迟加载 UserInfo 实例，避免在User未初始化时就尝试获取用户信息
    setTimeout(() => {this.userInfo = new UserInfo(userid, false)}, 0);
  }

  saveToCookie(cookieAge: number = User.MIN_COOKIE_AGE) {
    document.cookie = `userId=${this.#userid}; max-age=${cookieAge}`;
    document.cookie = `token=${this.#token}; max-age=${cookieAge}`;
  }

  async logout() {
    document.cookie.split('; ').forEach(cookie => {
      const [name] = cookie.split('=');
      document.cookie = `${name}=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/`;
    });
    User.#singleton = undefined;
    // 刷新当前页面
    await router.push('/');
    location.reload();
  }

  async resetPassword(newPassword: string): Promise<resultState> {
    // TODO: 这里可以添加重置密码的逻辑
    return {
      state: state.Success,
      data: '',
      error: '',
    }
  }

  get userAuth() {
    return {
      userId: this.#userid.slice(), // 防止外部修改
      token: this.#token.slice(),
    }
  }

  followUser(userId: string) {
    this.followList.add(userId);
    // TODO: 这里可以添加发送关注请求的逻辑
  }

  unfollowUser(userId: string) {
    this.followList.delete(userId);
    // TODO: 这里可以添加发送取消关注请求的逻辑
  }
}

export default User;
export { User, state, type resultState };
