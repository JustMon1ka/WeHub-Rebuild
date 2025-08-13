import type { Ref } from 'vue';
import { ref } from 'vue';
import { state, User } from '@/modules/auth/scripts/User.ts';
import router from '@/router.ts';
import styles from '@/modules/auth/scripts/Styles.ts';

class LoginData {
  userId : Ref<string> = ref('');
  password : Ref<string> = ref('');
  errorMsg : Ref<string> = ref('');
  failed : Ref<boolean> = ref(false);
  submitBtnStyle = ref('');

  constructor() {
    this.submitBtnStyle.value = styles.value.submitBtnShape + ' ' + styles.value.BtnNormal;
  }

  async submit() {
    this.submitBtnStyle.value = styles.value.submitBtnShape + ' ' + styles.value.BtnLoading;
    const cur_state : state = await User.login(this.userId.value, this.password.value);
    const result =  await new Promise(resolve => setTimeout(() => resolve('OK')  , 1000));

    switch(cur_state) {
      case state.LoggedIn:
        this.failed.value = false;
        this.errorMsg.value = '';
        await router.push({ name: '/' });
        break;
      case state.PasswordError:
        this.failed.value = true;
        this.errorMsg.value = User.errorMsg.PasswordMismatchError;
        break;
      case state.NetworkError:
        this.failed.value = true;
        this.errorMsg.value = User.errorMsg.NetworkError;
        break;
      default:
        this.failed.value = true;
        this.errorMsg.value = User.errorMsg.DefaultError;
        break;
    }
    this.submitBtnStyle.value = styles.value.submitBtnShape + ' ' + styles.value.BtnNormal;
  }
}

export default LoginData;
