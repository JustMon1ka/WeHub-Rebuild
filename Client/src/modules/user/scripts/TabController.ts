import { ref } from 'vue'
import type { Ref } from 'vue'
import styles from '@/modules/user/scripts/Styles.ts'

interface TabLabel {
  key: string;
  label: string;
}

class Button {
  label : Ref<string> = ref('');
  class : Ref<string> = ref('');
  focused : boolean = false;

  constructor(label: string) {
    this.label.value = label;
    this.class.value = styles.value.TabNormal;
  }

  setFocused(focused: boolean) {
    this.focused = focused;
    this.class.value = styles.value.TabFocus;
  }

  setBlur() {
    this.focused = false;
    this.class.value = styles.value.TabNormal;
  }
}

class TabController{
  tablabels: Array<TabLabel>;
  buttons: Button[] = [];
  currentTab: number = 0;

  constructor(tabLabels: Array<TabLabel>) {
    this.tablabels = tabLabels;
    for (let label of tabLabels) {
      this.buttons.push(new Button(label.label));
    }
    this.buttons[0].setFocused(true);
    this.currentTab = 0;
  }

  switchTab(num: number) {
    for (let button of this.buttons) {
      button.setBlur();
    }
    this.buttons[num].setFocused(true);
    this.currentTab = num;

    if (num < 0 || num >= this.tablabels.length) {
      throw new Error('Tab index out of bounds');
    }
  }


}

export default TabController;
export { Button, TabController};
export type { TabLabel };
