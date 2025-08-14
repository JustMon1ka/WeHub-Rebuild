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
  tabs : Array<any>;
  tablabels: Array<TabLabel>;
  buttons: Button[] = [];
  props : any;
  currentTab: string = '';

  constructor(tabs: Array<any>, tabLabels: Array<TabLabel>, props: any = {}) {
    this.tabs = tabs;
    this.tablabels = tabLabels;
    for (let label of tabLabels) {
      this.buttons.push(new Button(label.label));
    }
    this.buttons[0].setFocused(true);
    this.currentTab = tabs[0];
    this.props = props;
  }

  switchTab(event : Event, num : number) {
    event.preventDefault();
    let element = event.target as HTMLElement;

    for (let button of this.buttons) {
      button.setBlur();
    }
    this.buttons[num].setFocused(true);
    this.currentTab = this.tabs[num];

    if (num < 0 || num >= this.tabs.length) {
      throw new Error('Tab index out of bounds');
    }
  }


}

export default TabController;
export { Button, TabController};
export type { TabLabel };
