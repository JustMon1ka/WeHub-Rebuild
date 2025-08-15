import router from '@/router.ts'

enum state {
  LoggedOut,
  NetworkError,
  PasswordError,
  AuthCodeError,
  UserNotFound,
  EmailExistError,
  Success,
}


class User {
  static #singleton: User | undefined = undefined;
  // 获取当前登录用户的用户名，如果未登录则返回undefined
  static getInstance(): User | undefined {
    return User.#singleton;
  }

  static {
    document.cookie.split('; ').forEach(cookie => {
      const [name, value] = cookie.split('=');
      let userid : string | null = null;
      let token : string | null = null;
      if (name === 'userid') {
        userid = value;
      } else if (name === 'token') {
        token = value;
      }
      if (userid && token) {
        User.#singleton = new User(userid, token);
      }
    });
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
        User.#singleton = new User(result.userid, result.token);
        return state.Success;
      } else {
        return state.PasswordError;
      }
    }
  }

  static async login(username: string, password: string, email: string, code: string, rememberMe: boolean = false) {
    let passwordHash = await User.generateHash(password);
    const url = '/api/auth/login';
    const data = { username, passwordHash, email, code };
    const response = await User.sendData(url, data);
    if (response === state.Success && rememberMe) {
      User.#singleton?.saveToCookie(User.#singleton.#userid, User.#singleton.#token);
    }
    return response;
  }

  static async register(username: string, password: string, email: string, code: string, rememberMe: boolean = false) {
    let passwordHash = await User.generateHash(password);
    const url = '/api/auth/register';
    const data = { username, passwordHash, email, code };
    const response = await User.sendData(url, data);
    if (response === state.Success && rememberMe) {
      User.#singleton?.saveToCookie(User.#singleton.#userid, User.#singleton.#token);
    }
    return response;
  }

  static async sendAuthCode(email: string) {
    const url = '/api/auth/login';
    const data = { action: 'sendAuthCode', email };
  }


  #userid: string;
  #token: string;
  followList: string[] = ["Me"];

  constructor(userid: string, token: string) {
    this.#userid = userid;
    this.#token = token;
  }

  saveToCookie(userid: string, token: string) {
    document.cookie = `userid=${this.#userid}; max-age=5184000`; // 60 days
    document.cookie = `token=${this.#token}; max-age=5184000`; // 60 days
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

  async resetPassword(newPassword: string) {
    let passwordHash = await User.generateHash(newPassword);
    const url = '/api/auth/login';
    const data = { password: passwordHash };
    const response = await User.sendData(url, data);
  }

  get userAuth() {
    return {
      userId: this.#userid.slice(), // 防止外部修改
      token: this.#token.slice(),
    }
  }

  followUser(userId: string) {
    if (!this.followList.includes(userId)) {
      this.followList.push(userId);
      // TODO: 这里可以添加发送关注请求的逻辑
    }
  }

  unfollowUser(userId: string) {
    this.followList = this.followList.filter(id => id !== userId);
    // TODO: 这里可以添加发送取消关注请求的逻辑
  }
}

export default User;
export { User, state};
