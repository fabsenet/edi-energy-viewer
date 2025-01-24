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

const checkidentifierRules = ref<((v: string | null) => true | string)[]>([

  (v: string | null) => v == null || v.length <= 5 || 'Prüfidentifikatoren haben maximal 5 Stellen!',
  (v: string | null) => v == null || /^\d*$/.test(v) || 'Nur Zahlen erlaubt!',
]);

</script>

<template>
  <v-card title="Filter" elevation="6" class="ma-4">

    <v-container fluid>
      <v-row>
        <v-col cols="12">
          <h5>Gültigkeitszeitraum</h5>

          <v-btn-toggle v-model="filter.timeFrame" variant="outlined" mandatory>
            <v-btn color="primary" :value="'ALL'" primary :ripple="false">Alle</v-btn>
            <v-btn v-for="timeFrame in availableTimeFrames" :key="timeFrame" :value="timeFrame"
              :color="filter.timeFrame === timeFrame ? 'primary' : undefined" :ripple="false">
              {{ timeFrame }}
            </v-btn>
          </v-btn-toggle>
        </v-col>
      </v-row>

      <v-row>
        <v-col cols="12">
          <h5>Dokumenttyp</h5>

          <div class="d-flex flex-row align-start">
            <v-btn-toggle v-model="filter.type" variant="outlined" mandatory class="mt-1">
              <v-btn color="primary" :value="'ALL'" primary :ripple="false">Alle</v-btn>
              <v-btn v-for="type in availableTypes" :key="type" :value="type"
                :color="filter.type === type ? 'primary' : undefined" :ripple="false">
                {{ type }}
              </v-btn>
            </v-btn-toggle>
            <v-switch v-model="filter.onlyLatestVersion" color="primary" class="ml-4"
              label="Nur neueste Version anzeigen"></v-switch>
          </div>
        </v-col>
      </v-row>


      <v-row v-if="filter.type !== 'Allgemein'">
        <v-col cols="12">
          <h5>Nachrichtentyp</h5>

          <v-btn :variant="filter.messageType === 'ALL' ? undefined : 'outlined'" color="primary" :value="'ALL'" primary
            @click="filter.messageType = 'ALL'" rounded="xs" :ripple="false">Alle</v-btn>

          <v-btn class="ma-1" v-for="messageType in availableMessageTypes" :key="messageType" :value="messageType"
            :variant="filter.messageType === messageType ? undefined : 'outlined'"
            :color="filter.messageType === messageType ? 'primary' : undefined"
            @click="filter.messageType = messageType" rounded="xs" :ripple="false">
            {{ messageType }}
          </v-btn>

        </v-col>
      </v-row>

      <v-row v-if="filter.type === 'ALL' || filter.type === 'AHB'">
        <v-col cols="12">
          <h5>Prüfidentifikator</h5>
          <v-text-field class="w-25" v-model.trim="filter.checkIdentifier" label="Prüfidentifikator" placeholder="55002"
            clearable :rules="checkidentifierRules" variant="underlined"></v-text-field>
        </v-col>
      </v-row>
    </v-container>
  </v-card>
</template>
