<script setup lang="ts">
import { ref } from 'vue';
import EdiDocsClient from '@/api/EdiDocsClient';

const loading = ref(false);
const availableMessageTypes = ref<string[]>([]);
async function loadFilterData(){
try {
  loading.value = true;
  const response = await EdiDocsClient.GET("/api/FilterData");
  if(response.data) availableMessageTypes.value = response.data.availableMessageTypes;

  console.log(response.data);
} catch (error) {
  console.error(error);
} finally {
  loading.value = false;
}
}
loadFilterData();
</script>

<template>
<v-card title="Filter">
  {{ availableMessageTypes }}
</v-card>
</template>
