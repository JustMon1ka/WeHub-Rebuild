<script setup lang="ts">
import TheWelcome from '../components/TheWelcome.vue'
import {loginForm} from "@/modules/auth/public.ts";
</script>

<template>
  <main>
    <TheWelcome />
  </main>
  <loginForm />
</template>
