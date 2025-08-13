import type { Ref } from 'vue'
import { ref } from 'vue'
import styles from '@/modules/auth/scripts/Styles.ts'

class UserName {
  static errorMsg = {
    'NameFormatError': '用户名格式不正确，只能包含中文、字母和数字',
    'NameLengthError': '用户名长度需在4到20个字符之间',
  }

  static usernameRegex = /^[\u4e00-\u9fa5a-zA-Z0-9]+$/;

  username : Ref<string> = ref('');
  error : Ref<boolean> = ref(false);
  errorMsg : Ref<string> = ref('');

  checkValidity() {
    this.username.value = this.username.value.trim();
    const username = this.username.value;
    if (username.length < 4 || username.length > 20) {
      this.error.value = true;
      this.errorMsg.value = UserName.errorMsg.NameLengthError;
      return;
    }
    if (!UserName.usernameRegex.test(username)) {
      this.error.value = true;
      this.errorMsg.value = UserName.errorMsg.NameFormatError;
      return;
    }

    this.error.value = false;
    this.errorMsg.value = '';
  }
}


class Email {
  static emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
  static commonEmailDomains = [
    // 国际主流邮箱
    'gmail.com',
    'yahoo.com',
    'outlook.com',
    'hotmail.com',
    'aol.com',
    'protonmail.com',
    'icloud.com',
    'mail.com',
    'zoho.com',

    // 中国常用邮箱
    'qq.com',
    '163.com',
    '126.com',
    'sina.com',
    'sohu.com',
    'aliyun.com',
    'foxmail.com',

    // 企业/教育邮箱（部分）
    'edu.cn',  // 中国教育机构
    'ac.uk',   // 英国教育
    'edu',     // 通用教育
    'company.com' // 示例企业域名（实际使用时需替换）
  ];
  static errorMsg = {
    'EmailFormatError': '电子邮件格式不正确，请输入有效的电子邮件地址',
    'EmailDomainError': '请使用常见邮箱域名注册，如：outlook.com, qq.com, 163.com等',
    'EmailEmptyError': '电子邮件不能为空',
  }


  email : Ref<string> = ref('');
  error : Ref<boolean> = ref(false);
  errorMsg : Ref<string> = ref('');

  checkValidity() {
    this.email.value = this.email.value.trim();
    const email = this.email.value;
    if (!email) {
      this.error.value = true;
      this.errorMsg.value = Email.errorMsg.EmailEmptyError;
      return;
    }

    if (!Email.emailRegex.test(email)) {
      this.error.value = true;
      this.errorMsg.value = Email.errorMsg.EmailFormatError;
      return;
    }

    const domain = email.split('@')[1];
    if (!Email.commonEmailDomains.includes(domain)) {
      this.error.value = true;
      this.errorMsg.value = Email.errorMsg.EmailDomainError;
      return;
    }

    this.error.value = true;
    this.errorMsg.value = '';
  }
}

class Password {
  static errorMsg = {
    'PasswordFormatError': '密码必须包含至少一个大写字母、一个小写字母和一个数字',
    'PasswordLengthError': '密码长度须在8到32个字符之间',
    'PasswordMismatch': '两次输入的密码不一致，请重新输入',
  }

  static passwordRegex = /^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).+$/;

  password : Ref<string> = ref('');
  confirmPassword : Ref<string> = ref('');
  error : Ref<boolean> = ref(false);
  confirmError : Ref<boolean> = ref(false);
  errorMsg : Ref<string> = ref('');
  confirmErrorMsg : Ref<string> = ref('');

  checkValidity() {
    this.password.value = this.password.value.trim();
    if (this.password.value.length < 8 || this.password.value.length > 32) {
      this.error.value = true
      this.errorMsg.value = Password.errorMsg.PasswordLengthError;
      return;
    }
    if (!Password.passwordRegex.test(this.password.value)) {
      this.error.value = true;
      this.errorMsg.value = Password.errorMsg.PasswordFormatError;
      return;
    }
    this.error.value = false;
  }

  checkConfirmValidity() {
    this.confirmPassword.value = this.confirmPassword.value.trim();
    if (this.confirmPassword.value !== this.password.value) {
      this.confirmError.value = true;
      this.confirmErrorMsg.value = Password.errorMsg.PasswordMismatch;
      return;
    }
    this.confirmError.value = false;
    this.confirmErrorMsg.value = '';
  }
}

class AuthCode {
  static errorMsg = {
    'EmailNotSetError': '请先设置电子邮件地址',
    'CodeEmptyError': '验证码不能为空',
    'CodeMismatchError': '验证码不正确，请重新输入',
    'NetWorkError': '网络连接错误，请稍后再试',
  }

  static BtnMsg = {
    'AuthBtnNormal': '获取验证码',
    'AuthBtnCooldown': '重新获取验证码',
  }

  static countdownTime = 30;

  authCode : Ref<string> = ref('');
  error : Ref<boolean> = ref(false);
  errorMsg : Ref<string> = ref('');

  authBtnStyle : Ref<string> = ref(styles.value.AuthBtnShape + ' ' + styles.value.BtnNormal);
  btnMsg : Ref<string> = ref(AuthCode.BtnMsg.AuthBtnNormal);
  countdown : Ref<number> = ref(AuthCode.countdownTime);

  checkValidity() {
    this.authCode.value = this.authCode.value.trim();
    if (!this.authCode.value) {
      this.error.value = true;
      this.errorMsg.value = AuthCode.errorMsg.CodeEmptyError;
      return;
    }

    this.error.value = false;
    this.errorMsg.value = '';
  }

  emailNotSet() {
    this.error.value = true;
    this.errorMsg.value = AuthCode.errorMsg.EmailNotSetError;
    this.authCode.value = '';
  }

  codeMismatch() {
    this.error.value = true;
    this.errorMsg.value = AuthCode.errorMsg.CodeMismatchError;
  }

  async sentAuthCode() {
    this.authBtnStyle.value = styles.value.AuthBtnShape + ' ' + styles.value.BtnLoading;
    // TODO: 模拟发送验证码的异步操作

    // if (false) {
    //   this.error.value = true;
    //   this.errorMsg.value = AuthCode.errorMsg.NetWorkError;
    //   this.authBtnStyle.value = styles.value.AuthBtnShape + ' ' + styles.value.BtnNormal;
    //   return;
    // }
    this.authBtnStyle.value = styles.value.AuthBtnShape + ' ' + styles.value.BtnDisabled;
    this.btnMsg.value = `${AuthCode.BtnMsg.AuthBtnCooldown}(${this.countdown.value}s)`;

    setInterval(() => {
      if (this.countdown.value > 0) {
        this.countdown.value -= 1;
        this.btnMsg.value = `${AuthCode.BtnMsg.AuthBtnCooldown}(${this.countdown.value}s)`;
      } else {
        this.authBtnStyle.value = styles.value.AuthBtnShape + ' ' + styles.value.BtnNormal;
        this.btnMsg.value = AuthCode.BtnMsg.AuthBtnNormal;
        this.countdown.value = AuthCode.countdownTime; // 重置倒计时
      }
    }, 1000);
  }
}

export { Email, UserName, Password, AuthCode };
export default AuthCode;
