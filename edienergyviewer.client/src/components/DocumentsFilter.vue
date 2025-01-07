<script setup lang="ts">
import { ref } from 'vue';
import ediDocsClient from '@/api/EdiDocsClient';
import { availableTimeFrames, availableTypes, filterFromLocalStorageOrDefault, saveFilterToLocalStorage, type Filter } from '@/models/Filter';
import type { RefSymbol } from '@vue/reactivity';

defineProps<{ filter: Filter }>();

const loading = ref(false);
const availableMessageTypes = ref<string[]>([]);
async function loadFilterData() {
  try {
    loading.value = true;
    const response = await ediDocsClient.GET("/api/FilterData");
    if (response.data) availableMessageTypes.value = response.data.availableMessageTypes;

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
  <v-card title="Filter" elevation="6" class="ma-4">

    <v-container fluid>
      <v-row>
        <v-col cols="12">
          <h5>Gültigkeitszeitraum</h5>
        </v-col>
        <v-col cols="12">
          <v-btn-toggle v-model="filter.timeFrame" class="flex-wrap" variant="outlined" mandatory>
            <v-btn color="primary" :value="'ALL'" primary>Alle</v-btn>
            <v-btn v-for="timeFrame in availableTimeFrames" :key="timeFrame" :value="timeFrame">
              {{ timeFrame }}
            </v-btn>
          </v-btn-toggle>
        </v-col>
      </v-row>

      <v-row>
        <v-col cols="12">
          <h5>Typ</h5>
        </v-col>
        <v-col cols="6">
          <v-btn-toggle v-model="filter.type" class="flex-wrap" variant="outlined" mandatory>
            <v-btn color="primary" :value="'ALL'" primary>Alle</v-btn>
            <v-btn v-for="type in availableTypes" :key="type" :value="type">
              {{ type }}
            </v-btn>
          </v-btn-toggle>
        </v-col>
        <v-col cols="6">
          <v-switch v-model="filter.onlyLatestVersion" color="primary" label="Nur neueste Version anzeigen"></v-switch>
        </v-col>
      </v-row>


      <v-row v-if="filter.type !== 'Allgemein'">
        <v-col cols="12">
          <h5>Nachrichtentyp</h5>
        </v-col>
        <v-col cols="12">
          <v-btn-toggle v-model="filter.messageType" class="flex-wrap" variant="outlined" mandatory>
            <v-btn color="primary" :value="'ALL'" primary>Alle</v-btn>
            <v-btn v-for="messageType in availableMessageTypes" :key="messageType" :value="messageType">
              {{ messageType }}
            </v-btn>
          </v-btn-toggle>
        </v-col>
      </v-row>

      <v-row v-if="filter.type === 'ALL' || filter.type === 'AHB'">
        <v-col cols="12">
          <h5>Prüfidentifikator</h5>
        </v-col>
        <v-col cols="3">
          <v-text-field v-model="filter.checkIdentifier" outlined></v-text-field>
        </v-col>
      </v-row>
    </v-container>
  </v-card>
</template>
