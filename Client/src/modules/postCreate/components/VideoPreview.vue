<template>
  <div class="media-preview video-preview">
    <div v-if="isLoading" class="placeholder">
      Loading video...
    </div>

    <video
      v-else-if="mediaSrc"
      controls
      :src="mediaSrc"
      preload="metadata"
    >
      Your browser does not support the video tag.
    </video>

    <div v-else class="placeholder error">
      Failed to load video
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
// Make sure this path to your API file is correct
import { getMediaUrl } from '../public';

const props = defineProps({
  fileId: {
    type: String,
    required: true,
  },
});

const mediaSrc = ref('');
const isLoading = ref(true);

async function fetchMedia() {
  if (!props.fileId) return;

  isLoading.value = true;
  mediaSrc.value = '';

  try {
    const url = await getMediaUrl(props.fileId);
    mediaSrc.value = url;
  } catch (error) {
    console.error(`Failed to load video (fileId: ${props.fileId}):`, error);
    mediaSrc.value = '';
  } finally {
    isLoading.value = false;
  }
}

// Fetch media when the component is first mounted
onMounted(fetchMedia);

// Refetch if the fileId prop ever changes
watch(() => props.fileId, fetchMedia);
</script>

<style scoped>
.video-preview {
  max-width: 100%;
  margin: 0.5em 0;
  display: block;
  border-radius: 8px;
  overflow: hidden;
  background-color: #1f2937;
  border: 1px solid #334155;
}
.video-preview video {
  max-width: 100%;
  height: auto;
  display: block;
}
.placeholder {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100%;
  min-height: 60px;
  padding: 1rem;
  color: #9ca3af;
}
.placeholder.error {
  color: #fca5a5;
}
</style>
