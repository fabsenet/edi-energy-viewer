<script setup lang="ts">
import DocumentsFilter from './DocumentsFilter.vue';
import DocumentsList from './DocumentsList.vue';
import ediDocsClient from '../api/EdiDocsClient';
import { ref } from 'vue';

const loading = ref(false);

async function loadCheckIdentifier() {
  loading.value = true;
  try {
    const response = await ediDocsClient.GET("/api/CheckIdentifier");
    console.log(response.data);
  } catch (error) {
    console.error(error);
  } finally {
    loading.value = false;
  }
}
loadCheckIdentifier();

</script>

<template>
  <h1 v-if="loading" >Wird geladen...</h1>
 <v-container v-else>
    <v-row>
      <v-col cols="12">
        <DocumentsFilter />
      </v-col>
      <v-col cols="12">
        <DocumentsList />
      </v-col>
    </v-row>
  </v-container>
</template>
