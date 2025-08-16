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
    "UserNotFound": "用户不存在，请检查用户名或邮箱是否正确",
    "PasswordError": "密码错误，请重新输入",
    "LoggedOut": "用户已登出，请重新登录",
    "EmailExistError": `邮箱已被注册，请更换邮箱或使用其他方式登录`,
    "NetworkError": "网络连接错误，请稍后再试",
    "DefaultError": "发生未知错误，请稍后再试",
  }

  authType : Ref<AuthType> = ref(AuthType.PasswordLogin); // 'password' or 'email'
  useAuthCode : Ref<boolean> = ref(false); // 'password' or 'email'

  userName : UserName = new UserName();
  password : Password = new Password();
  email = new Email();
  authCode = new AuthCode();

  verified : Ref<boolean> = ref(false); // 是否已验证邮箱
  rememberMe : Ref<boolean> = ref(false); // 是否记住登录状态

  error : Ref<boolean> = ref(false);
  errorMsg : Ref<string> = ref('');

  submitBtnStyle = ref(styles.value.submitBtnShape + ' ' + styles.value.BtnNormal);


  changeAuthType(type: AuthType) {
    this.authType.value = type;
    this.error.value = false;
    this.errorMsg.value = '';
    if (type !== AuthType.PasswordReset) {
      this.verified.value = false; // 重置 verified 状态
    }

    if (type === AuthType.PasswordLogin || type === AuthType.PasswordResetVerify) {
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
    this.authCode.sendAuthCode(this.email.email.value);
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
    // 检查输入的内容是否正确
    this.email.checkValidity();
    this.userName.checkValidity();
    this.password.checkValidity();
    this.password.checkConfirmValidity();
    this.authCode.checkValidity();

    switch (this.authType.value) {
      case AuthType.PasswordLogin:
        this.userName.checkEmpty();
        this.password.checkEmpty();
        this.password.confirmError.value = false;
        this.email.error.value = false;
        this.authCode.error.value = false;
        break;
      case AuthType.AuthCodeLogin:
      case AuthType.PasswordResetVerify:
        this.userName.error.value = false;
        this.password.error.value = false;
        this.password.confirmError.value = false;
        break;
      case AuthType.PasswordReset:
        this.email.error.value = false;
        this.userName.error.value = false;
        this.authCode.error.value = false;
        break;
      default:
        this.error.value = true;
        this.errorMsg.value = AuthData.errorMsg.DefaultError;
        return false;
    }

    if (this.email.error.value || this.userName.error.value ||
      this.password.error.value || this.authCode.error.value || this.password.confirmError.value) {
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
        if (this.authType.value === AuthType.Register) {
          await router.push('/user_guide');
        }
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
      case state.UserNotFound:
        this.error.value = true;
        this.errorMsg.value = AuthData.errorMsg.UserNotFound;
        break;
      case state.NetworkError:
        this.error.value = true;
        this.errorMsg.value = AuthData.errorMsg.NetworkError;
        break;
      case state.EmailExistError:
        this.error.value = true;
        this.errorMsg.value = AuthData.errorMsg.EmailExistError;
        break;
      case state.LoggedOut:
        this.error.value = true;
        this.errorMsg.value = AuthData.errorMsg.LoggedOut;
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
    const rememberMe = this.rememberMe.value;
    let cur_state : state = state.LoggedOut;
    switch (this.authType.value) {
      case AuthType.PasswordLogin:
      case AuthType.AuthCodeLogin:
      case AuthType.PasswordResetVerify:
        cur_state = await User.login(userName, password, email, authCode, rememberMe);
        break;
      case AuthType.Register:
        cur_state = await User.register(userName, password, email, authCode, rememberMe);
        break;
      case AuthType.PasswordReset:
        cur_state = await User.getInstance()?.resetPassword(password) || state.LoggedOut;
        break;
      default:
        console.error('Unknown auth type');
        this.error.value = true;
        this.errorMsg.value = AuthData.errorMsg.DefaultError;
        break;
    }
    // cur_state = state.Success; // For testing purposes, force success
    return this.checkResponse(cur_state);
  }
}

export default AuthData;
export { AuthType, AuthData };
