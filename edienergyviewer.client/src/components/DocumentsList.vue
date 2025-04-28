<script setup lang="ts">
import type { components } from '@/api/EdiDocsApi';
import { DateTime } from 'luxon';
import EdiDocsClient from '@/api/EdiDocsClient';
import type { Filter } from '@/models/Filter';
import { computed, ref } from 'vue';
import type { VDataTable } from 'vuetify/components';
import { formatD } from '@/utils/dateUtils';

const { filter, documents } = defineProps<{ filter: Filter, documents: components["schemas"]["EdiDocumentSlim"][] }>();
const loading = ref(false);

const filterCheckidentifierMinimumBoundary = computed(() => {
  if (filter.checkIdentifier == null || filter.checkIdentifier.length == 0) return 0;

  const value = parseInt(filter.checkIdentifier);
  if (isNaN(value)) return 0;
  if (value < 10) return value * 10000;
  if (value < 100) return value * 1000;
  if (value < 1000) return value * 100;
  if (value < 10000) return value * 10;
  return value;
});
const filterCheckidentifierMaximumBoundary = computed(() => {
  if (filter.checkIdentifier == null || filter.checkIdentifier.length == 0) return 99999;

  const value = parseInt(filter.checkIdentifier);
  if (isNaN(value)) return 99999;
  if (value < 10) return value * 10000 + 9999;
  if (value < 100) return value * 1000 + 999;
  if (value < 1000) return value * 100 + 99;
  if (value < 10000) return value * 10 + 9;
  return value;
});

const filteredDocuments = computed(() => {
  return documents.filter((doc) => {

    if (filter.type !== "ALL") {
      if (filter.type === 'AHB' && doc.isAhb === false) return false;
      if (filter.type === 'MIG' && doc.isMig === false) return false;
      if (filter.type === 'Allgemein' && doc.isGeneralDocument === false) return false;
    }

    if (filter.onlyLatestVersion && doc.isLatestVersion !== true) return false;


    if (filter.timeFrame !== "ALL") {
      const now = DateTime.now();
      const validFrom = DateTime.fromISO(doc.validFrom);
      const validTo = doc.validTo && DateTime.fromISO(doc.validTo);

      if (filter.timeFrame === 'Aktuell' && !(validFrom < now && (!validTo || validTo > now))) return false;
      if (filter.timeFrame === 'Zukünftig' && !(validFrom > now || !validTo)) return false;
      if (filter.timeFrame === 'Vergangen' && !(validTo && validTo < now)) return false;
    }

    if (filter.type !== "Allgemein" && filter.messageType !== "ALL") {
      if (!doc.containedMessageTypes || !doc.containedMessageTypes.includes(filter.messageType)) return false;
    }

    if (filter.type !== "Allgemein" && filter.checkIdentifier !== "" && filter.checkIdentifier !== null) {
      if (!doc.checkIdentifiersWithStats
        || doc.checkIdentifiersWithStats.length === 0
        || !doc.checkIdentifiersWithStats.some(c => c.checkIdentifier >= filterCheckidentifierMinimumBoundary.value && c.checkIdentifier <= filterCheckidentifierMaximumBoundary.value)) {
        return false;
      }
    }
    return true;
  });
});

const filteredEdiDocuments = computed(() => filteredDocuments.value.filter((doc) => !doc.isGeneralDocument));
const filteredGeneralDocuments = computed(() => filteredDocuments.value.filter((doc) => doc.isGeneralDocument));

// Nachrichtentyp 	Version 	Typ 	Veröffentlicht am 	Gültigkeit 	PDF
const ediHeaders = ref<DataTableHeader[]>([
  { title: 'Nachrichtentyp', value: 'containedMessageTypes' },
  { title: 'Version', value: 'version' },
  { title: 'Typ', value: 'type' },
  { title: "Veröffentlicht am", value: "documentDate" },
  { title: 'Gültigkeit', value: 'validFrom' },
  { title: 'PDF', value: 'documentUri' },
]);

//Dokumentname 	                    Veröffentlicht am 	Gültigkeit 	PDF
const generalHeaders = ref<DataTableHeader[]>([
  { title: 'Dokumentname', value: 'documentName' },
  { title: "Veröffentlicht am", value: "documentDate" },
  { title: 'Gültigkeit', value: 'validFrom' },
  { title: 'PDF', value: 'documentUri' },
]);

const getFullXmlUri = (xmlDocument: components["schemas"]["EdiDocumentSlim"]["xmlDocuments"][0]) => {
  return "/api/" + xmlDocument.documentUri;
}
const getFullMirrorUri = (ediDocument: components["schemas"]["EdiDocumentSlim"], checkId?: number) => {
  if (checkId) {
    return "/api/" + ediDocument.id + "/part/" + checkId;
  } else {
    return "/api/" + ediDocument.id + "/full";
  }
}
</script>

<template>
  <v-card v-if="filter.type !== 'Allgemein'" title="Edi Dokumente" class="ma-4" elevation="6">
    <v-container fluid>
      <v-row>
        <v-col cols="12">
          <v-data-table items-per-page="-1" hide-default-footer :items="filteredEdiDocuments" :loading="loading"
            :headers="ediHeaders" loading-text="Lade Dokumente..."
            no-data-text="Keine Dokumente für diese Filterkritierien">

            <template #item.containedMessageTypes="{ item }">
              {{ item.containedMessageTypes ? item.containedMessageTypes.join(", ") : "Keine" }}
              <v-icon v-if="item.isGas">mdi-fire</v-icon>
              <v-icon v-if="item.isStrom">mdi-lightning-bolt</v-icon>
            </template>

            <template #item.documentDate="{ item }">
              {{ formatD(item.documentDate!) }}
              <v-icon v-if="item.isHot" color="red">mdi-alert-decagram</v-icon>
            </template>

            <template #item.validFrom="{ item }">
              {{ formatD(item.validFrom) }} - {{ item.validTo ? formatD(item.validTo) : 'unbegrenzt' }}
            </template>

            <template #item.version="{ item }">
              {{ item.messageTypeVersion }}
            </template>

            <template #item.type="{ item }">
              <v-chip v-if="item.isAhb" color="primary" class="ma-1">{{ item.bdewProcess ?? 'AHB' }}</v-chip>
              <v-chip v-if="item.isMig" color="primary" class="ma-1">MIG</v-chip>
            </template>

            <template #item.documentUri="{ item: document }">
              <v-btn class="ma-1" v-if="document.documentUri" :href="document.documentUri" target="_blank"
                density="comfortable">Quelle</v-btn>
              <v-btn color="primary" density="comfortable" class="ma-1" :href="getFullMirrorUri(document)"
                target="_blank">Mirror</v-btn>

              <template v-if="filter.showXmlDownloadButtons" v-for="xmlDoc in document.xmlDocuments"
                :key="xmlDoc.documentUri">
                <v-tooltip location="bottom">
                  <template #activator="{ props }">
                    <v-btn color="secondary" density="comfortable" class="ma-1" variant="outlined"
                      :href="getFullXmlUri(xmlDoc)" target="_blank" v-bind="props">{{ xmlDoc.name }} XML</v-btn>
                  </template>
                  Hinweis: Die bereitgestellten XML-Dokumente sind ausschließlich für den internen Gebrauch bei
                  Vattenfall lizensiert.
                </v-tooltip>
              </template>

              <div
                v-if="filter.checkIdentifier != '' && filter.checkIdentifier != null && document.checkIdentifiersWithStats"
                class="mb-4">
                <v-chip
                  v-for="checkIdWithStats in document.checkIdentifiersWithStats.filter(c => c.checkIdentifier.toString().startsWith(filter.checkIdentifier ?? ''))"
                  :key="document.id + '-' + checkIdWithStats.checkIdentifier"
                  :variant="checkIdWithStats.largestPageBlock <= 3 ? 'text' : 'outlined'"
                  :color="checkIdWithStats.largestPageBlock <= 3 ? 'grey' : 'primary'"
                  :size="checkIdWithStats.largestPageBlock <= 3 ? 'small' : 'comfortable'" class="ma-1">
                  <v-btn variant="text" :href="getFullMirrorUri(document, checkIdWithStats.checkIdentifier)"
                    target="_blank" class="ma-2"
                    :size="checkIdWithStats.largestPageBlock <= 3 ? 'small' : 'comfortable'"
                    :density="checkIdWithStats.largestPageBlock <= 3 ? 'compact' : 'comfortable'">{{
                      checkIdWithStats.checkIdentifier }}
                    ({{ checkIdWithStats.largestPageBlock }})
                  </v-btn>
                </v-chip>
              </div>
            </template>

          </v-data-table>
        </v-col>
      </v-row>
    </v-container>
  </v-card>
  <v-card class="ma-4" elevation="6"
    v-if="(filter.type === 'ALL' && filter.messageType === 'ALL' && filter.checkIdentifier === '') || filter.type === 'Allgemein'"
    title="
    Allgemeine Dokumente">
    <v-container fluid>
      <v-row>
        <v-col cols="12">
          <v-data-table items-per-page="-1" hide-default-footer :items="filteredGeneralDocuments" :loading="loading"
            :headers="generalHeaders" loading-text="Lade Dokumente..."
            no-data-text="Keine Dokumente für diese Filterkritierien">

            <template #item.documentName="{ item }">
              {{ item.documentNameRaw }}
            </template>

            <template #item.documentDate="{ item }">
              {{ formatD(item.documentDate) }}
              <v-icon v-if="item.isHot" color="red">mdi-alert-decagram</v-icon>
            </template>

            <template #item.validFrom="{ item }">
              {{ formatD(item.validFrom) }} - {{ item.validTo ? formatD(item.validTo) : 'unbegrenzt' }}
            </template>

            <template #item.documentUri="{ item: document }">
              <v-btn class="ma-1" v-if="document.documentUri" :href="document.documentUri" target="_blank"
                density="comfortable">Quelle</v-btn>
              <v-btn color="primary" density="comfortable" class="ma-1" :href="getFullMirrorUri(document)"
                target="_blank">Mirror</v-btn>
            </template>

          </v-data-table>
        </v-col>
      </v-row>
    </v-container>
  </v-card>
</template>

<style lang="scss" scoped>
.unimportant {
  opacity: 0.8;
  font-size: 80%;
}
</style>
