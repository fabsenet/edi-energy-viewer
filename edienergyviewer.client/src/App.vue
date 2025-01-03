<script setup lang="ts">
import DocumentsFilter from './components/DocumentsFilter.vue';
import DocumentsList from './components/DocumentsList.vue';

</script>

<template>
  <v-container>
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

<style scoped></style>
