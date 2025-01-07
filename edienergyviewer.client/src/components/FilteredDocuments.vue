<script setup lang="ts">
import DocumentsFilter from './DocumentsFilter.vue';
import DocumentsList from './DocumentsList.vue';
import EdiDocsClient from '../api/EdiDocsClient';
import { ref, watch } from 'vue';
import { filterFromLocalStorageOrDefault, saveFilterToLocalStorage } from '@/models/Filter';
import type { components } from '@/api/EdiDocsApi';

const loading = ref(0);

const allDocuments = ref<components["schemas"]["EdiDocumentSlim"][]>([]);
async function loadDocuments() {
  loading.value++;
  try {
    const response = await EdiDocsClient.GET("/api/EdiDocuments");
    if (response.data) allDocuments.value = response.data;
    console.log("Documents loaded", allDocuments.value);
  } catch (error) {
    console.error(error);
  } finally {
    loading.value--;
  }
}
loadDocuments();

const filter = filterFromLocalStorageOrDefault();
watch(filter, (f) => {
  console.log("Filter changed", f);
  saveFilterToLocalStorage(f);
},
  { deep: true }
);
</script>

<template>

  <div v-if="loading > 0" class="text-center"
    style="position: fixed; left: 0; right: 0; top: 0; bottom: 0; background-color: white;">
    <div style="height: 100%; padding-top: 10%;">
      <h1 style="font-size: 10vh; opacity: 0.7;">Lade Anwendung...</h1>
      <img style="margin-top: 50px;" src="@/assets/spinner.gif" alt="" width="256" height="128" />
    </div>
  </div>

  <v-container fluid v-else>
    <v-row>
      <v-col cols="12">
        <DocumentsFilter :filter="filter" />
      </v-col>
      <v-col cols="12">
        <DocumentsList :filter="filter" :documents="allDocuments" />
      </v-col>
    </v-row>
  </v-container>

</template>
