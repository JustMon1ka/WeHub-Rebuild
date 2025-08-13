import type { Ref } from 'vue'
import { ref } from 'vue'
import { state, User } from '@/modules/auth/scripts/User.ts'
import router from '@/router.ts'
import styles from '@/modules/auth/scripts/Styles.ts'
import { AuthCode, Email, Password, UserName } from '@/modules/auth/scripts/UserMetaData.ts'

enum AuthType {
  PasswordLogin,
  AuthCodeLogin,
  Register,
  PasswordResetVerify,
  PasswordReset,
}

class AuthData {
  static errorMsg = {
    "CheckError": "请检查输入的内容是否正确",
    "AuthCodeError": "验证码错误，请重新输入",
    "PasswordError": "密码错误，请重新输入",
    "NetworkError": "网络连接错误，请稍后再试",
    "DefaultError": "发生未知错误，请稍后再试",
  }

  static #singleton : AuthData = new AuthData();


  static getInstance() : AuthData {
    return AuthData.#singleton;
  }

  authType : Ref<AuthType> = ref(AuthType.PasswordLogin); // 'password' or 'email'
  useAuthCode : Ref<boolean> = ref(false); // 'password' or 'email'

  userName : UserName = new UserName();
  password : Password = new Password();
  email = new Email();
  authCode = new AuthCode();
  verified : Ref<boolean> = ref(false); // 是否已验证邮箱

  error : Ref<boolean> = ref(false);
  errorMsg : Ref<string> = ref('');

  submitBtnStyle = ref(styles.value.submitBtnShape + ' ' + styles.value.BtnNormal);


  changeAuthType(type: AuthType) {
    this.authType.value = type;
    this.error.value = false;
    this.errorMsg.value = '';
    if (this.authType.value !== AuthType.PasswordReset) {
      this.verified.value = false; // 重置 verified 状态
    }

    if (type === AuthType.PasswordLogin) {
      this.useAuthCode.value = false;
    } else if (type === AuthType.AuthCodeLogin || type === AuthType.Register
      || type === AuthType.PasswordReset) {
      this.useAuthCode.value = true;
    }
  }

  sendAuthCode() {
    this.email.checkValidity();
    if (this.email.error.value) {
      this.authCode.emailNotSet();
      return;
    }
    this.authCode.sendAuthCode();
  }

  async submit() {
    if (!this.checkValidity()) {
      return;
    }

    this.submitBtnStyle.value = styles.value.submitBtnShape + ' ' + styles.value.BtnLoading;
    await this.authenticate();
    this.submitBtnStyle.value = styles.value.submitBtnShape + ' ' + styles.value.BtnNormal;
  }
  checkValidity() : boolean {
    switch (this.authType.value) {
      case AuthType.PasswordLogin:
        this.userName.checkEmpty();
        this.password.checkEmpty();
        this.email.error.value = false; // 邮箱不需要验证
        this.authCode.error.value = false; // 验证码不需要验证
        break;
      case AuthType.AuthCodeLogin:
      case AuthType.PasswordResetVerify:
        // 检查输入的内容是否正确
        this.email.checkValidity();
        this.authCode.checkValidity();
        this.userName.error.value = false; // 用户名不需要验证
        this.password.error.value = false; // 密码不需要验证
        break;
      case AuthType.Register:
        // 检查输入的内容是否正确
        this.email.checkValidity();
        this.userName.checkValidity();
        this.password.checkValidity();
        this.authCode.checkValidity();
        break;
      case AuthType.PasswordReset:
        this.email.error.value = false; // 邮箱不需要验证
        this.userName.error.value = false; // 用户名不需要验证
        this.password.checkValidity();
        this.password.checkConfirmValidity();
        break;
      default:
        console.error('Unknown auth type');
        return false;
    }

    if (this.email.error.value || this.userName.error.value ||
      this.password.error.value || this.authCode.error.value) {
      this.error.value = true;
      this.errorMsg.value = AuthData.errorMsg.CheckError;
      return false;
    }
    return true;
  }

  async checkResponse(cur_state: state) {
    switch(cur_state) {
      case state.Success:
        if (this.authType.value === AuthType.PasswordResetVerify) {
          this.changeAuthType(AuthType.PasswordReset);
          this.verified.value = true;
          this.error.value = false;
          this.errorMsg.value = '';
          break;
        }
        this.error.value = false;
        this.errorMsg.value = '';
        await router.push('/');
        break;
      case state.PasswordError:
        this.error.value = true;
        this.errorMsg.value = AuthData.errorMsg.PasswordError;
        break;
      case state.AuthCodeError:
        this.error.value = true;
        this.errorMsg.value = AuthData.errorMsg.AuthCodeError;
        break;
      case state.NetworkError:
        this.error.value = true;
        this.errorMsg.value = AuthData.errorMsg.NetworkError;
        break;
      default:
        this.error.value = true;
        this.errorMsg.value = AuthData.errorMsg.DefaultError;
        break;
    }
  }

  async authenticate() {

    const userName = this.userName.userName.value;
    const password = this.password.password.value;
    const email = this.email.email.value;
    const authCode = this.authCode.authCode.value;

    let cur_state : state = state.LoggedOut;
    switch (this.authType.value) {
      case AuthType.PasswordLogin:
        cur_state = await User.login(userName, password);
        break;
      case AuthType.AuthCodeLogin:
      case AuthType.PasswordResetVerify:
        cur_state = await User.verifyAuthCode(email, authCode);
        break;
      case AuthType.Register:
        cur_state = await User.register(userName, password, email);
        break;
      case AuthType.PasswordReset:
        cur_state = await User.resetPassword(userName, email, password);
        break;
      default:
        console.error('Unknown auth type');
        this.error.value = true;
        this.errorMsg.value = AuthData.errorMsg.DefaultError;
        break;
    }
    return this.checkResponse(cur_state);
  }
}

export default AuthData;
export { AuthType, AuthData };
