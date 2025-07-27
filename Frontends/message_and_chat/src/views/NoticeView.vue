<template>
  <div class = "container">
    <div class = "left">
      <SideNavigationBar />

    </div>
    <div class="divider-vertical"></div>
    <div class = "center">
      <div class = "notice-heading">
        <h2>通知</h2>
      </div>
      <div class="divider-horizontal"></div>
      <div class = "notice-type">
        <button>全部消息</button>
        <button>@我</button>
        <button>回复我的</button>
        <button>收到的赞</button>  
      </div>
      
      <div class = "notice-information">
      
      </div>
    
    </div>
    <div class="divider-vertical"></div>
    <div class = "right">
        <!--todo 搜索框-->
      <input
          type = "text"
          placeholder="搜索..."
        />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import SideNavigationBar from '../components/SideNavigationBar.vue'

const selectedNoticeType = ref(0);

</script>

<style scoped>
.container {
  display: flex;
  height: 100vh;
  width: 1200px; 
  max-width: 100%;
  margin:0 auto;
  box-sizing: border-box;
}

.left{
  width:20%;
  display:flex;
  align-items: center;
  justify-content: center;
}

.center{
  width:60%;
  display: flex;
  flex-direction: column;
  overflow-wrap: break-word;
  word-break: break-all;
}

.notice-heading{
  flex:1;
  display: flex;
  padding-left:32px;
}

.notice-type{
  flex:1;
  display: flex;
}

.notice-type button{
  flex:1;
  align-items: center;           
  justify-content: center;        
  text-align: center;           
}

.notice-type button.active{
  font-weight: bold;   
}

.notice-information{
  flex:14;
  display: flex;
}
.right{
  width:20%;
}

.divider-horizontal {
  width: 100%;                  
  border-bottom: 1px solid #444c5c;              
}

.divider-vertical {
  width: 1px;               
  background-color: #444c5c; 
  margin: 0 0px;           
}


</style>