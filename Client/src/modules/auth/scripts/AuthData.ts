import type { Ref } from 'vue'
import { ref } from 'vue'
import { type resultState,User } from '@/modules/auth/scripts/User.ts'
import router from '@/router.ts'
import styles from '@/modules/auth/scripts/Styles.ts'
import { AuthCode, Email, Password, Phone, UserName } from '@/modules/auth/scripts/UserMetaData.ts'
import { toggleLoginHover } from '@/router.ts'

enum AuthType {
  PasswordLogin,
  AuthCodeLogin,
  Register,
  PasswordResetVerify,
  PasswordReset,
}

class AuthData {
  static readonly errorMsg = {
    "CheckError": "请检查输入的内容是否正确",
    "PrivacyError": "请先阅读并同意隐私政策",
    "NetworkError": "网络连接错误，请稍后再试",
    "DefaultError": "发生未知错误，请稍后再试",
  }

  authType : Ref<AuthType> = ref(AuthType.PasswordLogin); // 'password' or 'email'

  userName : UserName = new UserName();
  password : Password = new Password();
  email = new Email();
  authCode = new AuthCode();
  phone = new Phone();

  verified : Ref<boolean> = ref(false); // 是否已验证邮箱
  rememberMe : Ref<boolean> = ref(false); // 是否记住登录状态
  agreeToTerms : Ref<boolean> = ref(false); // 是否同意条款

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
  }

  async sendAuthCode() {
    this.email.checkValidity();
    if (this.email.error.value) {
      this.authCode.emailNotSet();
      return;
    }
    await this.authCode.sendAuthCode(this.email.email.value);
  }

  async submit() {
    if (!this.checkValidity()) {
      return;
    }
    if (this.authType.value === AuthType.Register && !this.agreeToTerms.value) {
      this.error.value = true;
      this.errorMsg.value = AuthData.errorMsg.PrivacyError;
      return;
    }

    this.submitBtnStyle.value = styles.value.submitBtnShape + ' ' + styles.value.BtnLoading;
    await this.authenticate();
    this.submitBtnStyle.value = styles.value.submitBtnShape + ' ' + styles.value.BtnNormal;
  }
  checkValidity() : boolean {
    // 重置错误状态
    this.userName.error.value = false;
    this.password.error.value = false;
    this.password.confirmError.value = false;
    this.email.error.value = false;
    this.authCode.error.value = false;
    this.phone.error.value = false;
    this.error.value = false;
    this.errorMsg.value = '';

    // 检查输入的内容是否符合要求
    switch (this.authType.value) {
      case AuthType.PasswordLogin:
        this.userName.checkEmpty();
        this.password.checkEmpty();
        break;
      case AuthType.AuthCodeLogin:
      case AuthType.PasswordResetVerify:
        this.email.checkValidity();
        this.authCode.checkValidity();
        break;
      case AuthType.PasswordReset:
        this.password.checkValidity();
        this.password.checkConfirmValidity();
        break;
      case AuthType.Register:
        this.userName.checkEmpty();
        this.email.checkValidity();
        this.password.checkEmpty();
        this.password.checkConfirmValidity();
        this.phone.checkValidity();
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

  async checkResponse(result: resultState) {
    if (result.success){
      if (this.authType.value === AuthType.PasswordResetVerify) {
        this.changeAuthType(AuthType.PasswordReset);
        this.verified.value = true;
        this.error.value = false;
        this.errorMsg.value = '';
        return;
      }
      this.error.value = false;
      this.errorMsg.value = '';
      if (this.authType.value === AuthType.Register) {
        await router.push('/user_guide');
      } else {
        await router.push('/');
      }
      toggleLoginHover(false);
    } else {
      this.error.value = true;
      this.errorMsg.value = result.error || AuthData.errorMsg.DefaultError;
    }
  }

  async authenticate() {
    const userName = this.userName.userName.value;
    const password = this.password.password.value;
    const email = this.email.email.value;
    const authCode = this.authCode.authCode.value;
    const phone = this.phone.phone.value;
    const rememberMe = this.rememberMe.value;

    let result : resultState | undefined = undefined;
    switch (this.authType.value) {
      case AuthType.PasswordLogin:
        result = await User.login(userName, password, rememberMe);
        break;
      case AuthType.AuthCodeLogin:
      case AuthType.PasswordResetVerify:
        result = await User.verifyAuthCode(email, authCode, rememberMe);
        break;
      case AuthType.Register:
        result = await User.register(userName, password, email, authCode, phone, rememberMe);
        break;
      case AuthType.PasswordReset:
        result = await User.getInstance()?.resetPassword(password);
        if (result === undefined){
          await router.push('/password_reset');
          return;
        }
        break;
      default:
        this.error.value = true;
        this.errorMsg.value = AuthData.errorMsg.DefaultError;
        return;
    }
    return this.checkResponse(result);
  }
}
export { AuthType, AuthData };
