<script setup lang="ts">
import TheWelcome from '../components/TheWelcome.vue'
import {loginForm} from "@/modules/auth/public.ts";
</script>

<template>
  <div class="flex flex-col p-6">
    <TheWelcome class="flex-auto"/>
    <loginForm class="flex-auto"/>
  </div>
</template>
