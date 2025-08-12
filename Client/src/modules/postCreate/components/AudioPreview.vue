<template>
  <div class="media-preview audio-preview">
    <div v-if="isLoading" class="placeholder">
      Loading audio...
    </div>

    <template v-else-if="mediaSrc">
      <audio controls :src="mediaSrc">
        Your browser does not support the audio element.
      </audio>
      <span class="file-id" :title="fileId">ID: {{ fileId.substring(0, 8) }}...</span>
    </template>

    <div v-else class="placeholder error">
      Failed to load audio
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
    console.error(`Failed to load audio (fileId: ${props.fileId}):`, error);
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
.audio-preview {
  display: flex;
  align-items: center;
  gap: 1em;
  padding: 0.5em;
  background-color: #1f2937;
  border-radius: 8px;
  border: 1px solid #334155;
  margin: 0.5em 0;
  min-height: 54px; /* Ensure consistent height */
}
.audio-preview audio {
  flex-grow: 1;
}
.file-id {
  font-size: 0.8em;
  color: #94a3b8;
  flex-shrink: 0; /* Prevent the ID from shrinking */
}
.placeholder {
  width: 100%;
  text-align: center;
  color: #9ca3af;
}
.placeholder.error {
  color: #fca5a5;
}
</style>
