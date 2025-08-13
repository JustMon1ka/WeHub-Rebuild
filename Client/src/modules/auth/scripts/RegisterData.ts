import { ref } from 'vue'
import type { Ref } from 'vue';
import styles from '@/modules/auth/scripts/Styles.ts'
import { Email, UserName, Password, AuthCode } from '@/modules/auth/scripts/UserMetaData.ts'


class RegisterData {
  static errorMsg = {
    "CheckError": "请检查输入的内容是否正确",
    "NetWorkError": "网络连接错误，请稍后再试",
    "DefaultError": "发生未知错误，请稍后再试",
  }

  email: Email = new Email();
  password: Password = new Password();
  username: UserName = new UserName();
  authCode: AuthCode = new AuthCode();

  error : Ref<boolean> = ref(false);
  errorMsg : Ref<string> = ref('');

  submitBtnStyle = ref(styles.value.submitBtnShape + ' ' + styles.value.BtnNormal);

  sentAuthCode() {
    let result = 0;
    this.email.checkValidity();
    if (this.email.error.value) {
      this.authCode.emailNotSet();
      return;
    }
    this.authCode.sentAuthCode();
  }

  async submit() {
    // 检查输入的内容是否正确
    this.email.checkValidity();
    this.username.checkValidity();
    this.password.checkValidity();
    this.authCode.checkValidity();
    if (this.email.error.value || this.username.error.value ||
        this.password.error.value || this.authCode.error.value) {
      this.error.value = true;
      this.errorMsg.value = RegisterData.errorMsg.CheckError;
    }

    // TODO: 提交数据
    this.submitBtnStyle.value = styles.value.submitBtnShape + ' ' + styles.value.BtnLoading;


    this.submitBtnStyle.value = styles.value.submitBtnShape + ' ' + styles.value.BtnNormal;
  }

}

export default RegisterData
