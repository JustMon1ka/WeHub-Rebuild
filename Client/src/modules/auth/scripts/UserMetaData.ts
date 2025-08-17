import type { Ref } from 'vue'
import { ref } from 'vue'
import styles from '@/modules/auth/scripts/Styles.ts'
import { User, state } from '@/modules/auth/scripts/User.ts'

class UserName {
  static readonly errorMsg = {
    'NameFormatError': '用户名格式不正确，只能包含中文、字母和数字',
    'NameEmptyError': '用户名不能为空',
    'NameLengthError': '用户名长度需在4到20个字符之间',
  }

  static usernameRegex = /^[\u4e00-\u9fa5a-zA-Z0-9]+$/;

  userName : Ref<string> = ref('');
  error : Ref<boolean> = ref(false);
  errorMsg : Ref<string> = ref('');

  checkValidity() {
    this.userName.value = this.userName.value.trim();
    const username = this.userName.value;
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

  checkEmpty() {
    this.userName.value = this.userName.value.trim();
    if (!this.userName.value) {
      this.error.value = true;
      this.errorMsg.value = UserName.errorMsg.NameEmptyError;
      return;
    }
    this.error.value = false;
    this.errorMsg.value = '';
  }
}


class Email {
  static readonly emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
  static readonly commonEmailDomains = [
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
  static readonly errorMsg = {
    'EmailFormatError': '电子邮件格式不正确，请输入有效的电子邮件地址',
    'EmailDomainError': '请使用常见邮箱域名，如：outlook.com, qq.com, 163.com等',
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

    if (!Email.commonEmailDomains.some(domain => email.endsWith(domain))) {
      this.error.value = true;
      this.errorMsg.value = Email.errorMsg.EmailDomainError;
      return;
    }

    this.error.value = false;
    this.errorMsg.value = '';
  }
}

class Phone {
  static readonly phoneRegex = /^1[3-9]\d{9}$/; // 中国大陆手机号格式
  static readonly errorMsg = {
    'PhoneFormatError': '手机号格式不正确，请输入11位数字的中国大陆手机号',
    'PhoneEmptyError': '手机号不能为空',
  }

  phone : Ref<string> = ref('');
  error : Ref<boolean> = ref(false);
  errorMsg : Ref<string> = ref('');

  checkValidity() {
    this.phone.value = this.phone.value.trim();
    if (!this.phone.value) {
      this.error.value = true;
      this.errorMsg.value = Phone.errorMsg.PhoneEmptyError;
      return;
    }

    if (!Phone.phoneRegex.test(this.phone.value)) {
      this.error.value = true;
      this.errorMsg.value = Phone.errorMsg.PhoneFormatError;
      return;
    }

    this.error.value = false;
    this.errorMsg.value = '';
  }
}

class Password {
  static readonly errorMsg = {
    'PasswordFormatError': '密码必须包含至少一个大写字母、一个小写字母和一个数字',
    'PasswordEmptyError': '密码不能为空',
    'PasswordLengthError': '密码长度须在8到32个字符之间',
    'PasswordMismatch': '两次输入的密码不一致，请重新输入',
  }

  static readonly passwordRegex = /^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).+$/;

  password : Ref<string> = ref('');
  confirmPassword : Ref<string> = ref('');
  error : Ref<boolean> = ref(false);
  confirmError : Ref<boolean> = ref(false);
  errorMsg : Ref<string> = ref('');
  confirmErrorMsg : Ref<string> = ref('');

  checkEmpty() {
    this.password.value = this.password.value.trim();
    if (!this.password.value) {
      this.error.value = true;
      this.errorMsg.value = Password.errorMsg.PasswordEmptyError;
      return;
    }
    this.error.value = false;
    this.errorMsg.value = '';
  }

  checkValidity() {
    this.password.value = this.password.value.trim();

    if (this.confirmPassword.value) {
      this.checkConfirmValidity();
    }

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
    this.errorMsg.value = '';
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
  static readonly errorMsg = {
    'EmailNotSetError': '请先设置电子邮件地址',
    'CodeEmptyError': '验证码不能为空',
    'EmailNotFoundError': '未找到与该电子邮件地址关联的用户',
    'NetWorkError': '网络连接错误，请稍后再试',
    'CodeFormatError': '验证码格式不正确，请输入6位数字验证码',
    'DefaultError': '发生未知错误，请稍后再试',
  }

  static readonly BtnMsg = {
    'AuthBtnNormal': '获取验证码',
    'AuthBtnCooldown': '重新获取验证码',
  }

  static readonly countdownTime = 30;
  static readonly codeRegex = /^\d{6}$/; // 验证码格式：6位数字

  authCode : Ref<string> = ref('');
  error : Ref<boolean> = ref(false);
  errorMsg : Ref<string> = ref('');
  counter : any = null;

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

    if (!AuthCode.codeRegex.test(this.authCode.value)) {
      this.error.value = true;
      this.errorMsg.value = AuthCode.errorMsg.CodeFormatError;
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

  async sendAuthCode(email: string) {
    this.authBtnStyle.value = styles.value.AuthBtnShape + ' ' + styles.value.BtnLoading;

    const result = await User.sendAuthCode(email);
    const cur_state = result.state;

    switch (cur_state){
      case state.Success:
        this.error.value = false;
        this.errorMsg.value = '';
        break;
      case state.NetworkError:
        this.error.value = true;
        this.errorMsg.value = AuthCode.errorMsg.NetWorkError;
        break;
      case state.DataError:
        this.error.value = true;
        this.errorMsg.value = result?.error || AuthCode.errorMsg.DefaultError;
        break;
      default:
        this.error.value = true;
        this.errorMsg.value = AuthCode.errorMsg.DefaultError;
        break;
    }

    if (cur_state !== state.Success) {
      this.authBtnStyle.value = styles.value.AuthBtnShape + ' ' + styles.value.BtnNormal;
      return;
    }

    this.authBtnStyle.value = styles.value.AuthBtnShape + ' ' + styles.value.BtnDisabled;
    this.btnMsg.value = `${AuthCode.BtnMsg.AuthBtnCooldown}(${this.countdown.value}s)`;

    this.counter = setInterval(() => {
      if (this.countdown.value > 0) {
        this.countdown.value -= 1;
        this.btnMsg.value = `${AuthCode.BtnMsg.AuthBtnCooldown}(${this.countdown.value}s)`;
      } else {
        this.authBtnStyle.value = styles.value.AuthBtnShape + ' ' + styles.value.BtnNormal;
        this.btnMsg.value = AuthCode.BtnMsg.AuthBtnNormal;
        this.countdown.value = AuthCode.countdownTime;
        clearInterval(this.counter);
      }
    }, 1000);
  }
}

export { Email, Phone , UserName, Password, AuthCode };
