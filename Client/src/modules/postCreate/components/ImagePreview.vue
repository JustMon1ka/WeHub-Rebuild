<template>
  <div class="media-preview image-preview">
    <div v-if="isLoading" class="placeholder">
      Loading image...
    </div>

    <img
      v-else-if="mediaSrc"
      :src="mediaSrc"
      alt="User uploaded image"
      @error="handleError"
    />

    <div v-else class="placeholder error">
      Failed to load image
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
  mediaSrc.value = ''; // Reset on new fetch

  try {
    // Call the API function to get the blob URL
    const url = await getMediaUrl(props.fileId);
    mediaSrc.value = url;
  } catch (error) {
    console.error(`Failed to load image (fileId: ${props.fileId}):`, error);
    mediaSrc.value = ''; // Ensure it's empty on failure
  } finally {
    isLoading.value = false;
  }
}

function handleError() {
  console.error('Image resource could not be rendered:', mediaSrc.value);
  mediaSrc.value = ''; // Setting src to empty will trigger the error display
}

// Fetch media when the component is first mounted
onMounted(fetchMedia);

// Refetch if the fileId prop ever changes
watch(() => props.fileId, fetchMedia);
</script>

<style scoped>
.image-preview {
  max-width: 100%;
  min-height: 60px; /* Give some space for placeholder text */
  margin: 0.5em 0;
  display: block;
  border-radius: 8px;
  overflow: hidden;
  background-color: #1f2937;
  border: 1px solid #334155;
}
.image-preview img {
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
  color: #9ca3af;
}
.placeholder.error {
  color: #fca5a5; /* A reddish color for errors */
}
</style>
