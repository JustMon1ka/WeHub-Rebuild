enum state {
  NotLoggedIn,
  NetworkError,
  PasswordError,
  LoggedIn,
}
class User {

  static singleton: User | undefined = undefined;
  static errorMsg = {
    'PasswordMismatchError': '用户名或密码错误，请重试',
    'NetworkError': '网络错误，请检查您的网络连接',
    'DefaultError': '发生未知错误，请稍后再试',
  }

  static {
    const url = '/api/auth/login';
    fetch(url)
      .then(response => {
        if (!response.ok) {
          User.singleton = undefined;
          return;
        }
        return response.json();
      })
      .then(data => {
        if (data && data.username && data.passwordHash) {
          User.singleton = new User(data.username, data.passwordHash);
        } else {
          User.singleton = undefined;
        }
      })
      .catch(() => {
        User.singleton = undefined;
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

  static async login(username: string, password: string) {
    let passwordHash = await User.generateHash(password);
    const url = '/api/auth/login';
    const data = { username, password: passwordHash };
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
        User.singleton = new User(result.userid, result.token);
        return state.LoggedIn;
      } else {
        return state.PasswordError;
      }
    }
  }

  // 获取当前登录用户的用户名，如果未登录则返回undefined
  static getInstance(username: string, passwordHash: string): User | undefined {
    return User.singleton;
  }

  #userid: string;
  #token: string;
  constructor(userid: string, token: string) {
    this.#userid = userid;
    this.#token = token;
  }
}


export default User;
export { User, state};
