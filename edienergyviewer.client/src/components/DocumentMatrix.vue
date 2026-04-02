<script setup lang="ts">
import type { components } from '@/api/EdiDocsApi';
import { formatD } from '@/utils/dateUtils';
import { DateTime } from 'luxon';
import { computed, ref } from 'vue';

const { documents } = defineProps<{ documents: components["schemas"]["EdiDocumentSlim"][] }>();

const showCompleteHistory = ref(false);

const migDocuments = computed(() =>
  documents.filter((doc) => doc.isMig === true)
);

const cutoff = computed(() => DateTime.now().minus({ months: 6 }));

/** All unique boundary dates (only validFrom) sorted chronologically */
const allBoundaries = computed((): DateTime[] => {
  const set = new Set<string>();
  for (const doc of migDocuments.value) {
    if (doc.validFrom) set.add(doc.validFrom);
  }
  return [...set]
    .map((d) => DateTime.fromISO(d))
    .sort((a, b) => a.valueOf() - b.valueOf());
});

type Period = { start: DateTime; end: DateTime | null };

const allPeriods = computed((): Period[] => {
  const boundaries = allBoundaries.value;
  if (!boundaries.length) return [];
  return boundaries.map((start, i) => ({
    start,
    end: i + 1 < boundaries.length ? boundaries[i + 1] : null,
  }));
});

/** Periods shown as individual columns (historical ones collapsed when switch is off) */
const periods = computed((): Period[] => {
  if (showCompleteHistory.value) return allPeriods.value;
  return allPeriods.value.filter((p) => p.start >= cutoff.value);
});

/** Whether to show the condensed "Historisch" column */
const hasHistoryColumn = computed(() =>
  !showCompleteHistory.value && allPeriods.value.some((p) => p.start < cutoff.value)
);

/** The last historical period (closest to cutoff) – used to populate the history column */
const lastHistoricalPeriod = computed((): Period | null => {
  const hist = allPeriods.value.filter((p) => p.start < cutoff.value);
  return hist[hist.length - 1] ?? null;
});

/** All unique message types found across all MIG documents, sorted */
const messageTypes = computed((): string[] => {
  const set = new Set<string>();
  for (const doc of migDocuments.value) {
    for (const mt of doc.containedMessageTypes ?? []) {
      set.add(mt);
    }
  }
  return [...set].sort();
});

type Variant = 'strom' | 'gas' | 'all';

type MatrixRow = {
  messageType: string;
  variant: Variant;
  /** Unique key used as record index */
  key: string;
  /** Display label for the row */
  label: string;
};

/** Build rows: split message types that have both Strom and Gas documents into two rows */
const matrixRows = computed((): MatrixRow[] => {
  const rows: MatrixRow[] = [];
  for (const mt of messageTypes.value) {
    const docs = migDocuments.value.filter((d) => d.containedMessageTypes?.includes(mt));
    const hasStrom = docs.some((d) => d.isStrom);
    const hasGas = docs.some((d) => d.isGas);
    if (hasStrom && hasGas) {
      rows.push({ messageType: mt, variant: 'strom', key: `${mt}|strom`, label: `${mt} Strom` });
      rows.push({ messageType: mt, variant: 'gas', key: `${mt}|gas`, label: `${mt} Gas` });
    } else if (hasStrom) {
      rows.push({ messageType: mt, variant: 'strom', key: `${mt}|strom`, label: `${mt} Strom` });
    } else if (hasGas) {
      rows.push({ messageType: mt, variant: 'gas', key: `${mt}|gas`, label: `${mt} Gas` });
    } else {
      rows.push({ messageType: mt, variant: 'all', key: mt, label: mt });
    }
  }
  return rows.sort((a, b) => a.label.localeCompare(b.label, 'de'));
});

type CellData = {
  version: string;
  doc: components["schemas"]["EdiDocumentSlim"];
} | null;

type CellGroup = { cell: CellData; colspan: number };

/** Precomputed history column cells: historyCells[row.key] = CellData | null */
const historyCells = computed((): Record<string, CellData> => {
  const period = lastHistoricalPeriod.value;
  if (!period) return {};
  const result: Record<string, CellData> = {};
  for (const row of matrixRows.value) {
    result[row.key] = getCell(row, period);
  }
  return result;
});

/** Precomputed matrix: matrix[row.key][periodIndex] = CellData | null */
const matrixData = computed((): Record<string, CellData[]> => {
  const result: Record<string, CellData[]> = {};
  for (const row of matrixRows.value) {
    result[row.key] = periods.value.map((period) => getCell(row, period));
  }
  return result;
});

/** Groups consecutive cells with the same document into colspan spans */
const groupedMatrixData = computed((): Record<string, CellGroup[]> => {
  const result: Record<string, CellGroup[]> = {};
  for (const row of matrixRows.value) {
    const cells = matrixData.value[row.key];
    const groups: CellGroup[] = [];
    for (const cell of cells) {
      const last = groups.at(-1);
      const sameDoc = last !== undefined &&
        ((last.cell === null && cell === null) ||
          (last.cell !== null && cell !== null && last.cell.doc.id === cell.doc.id));
      if (sameDoc) {
        last!.colspan++;
      } else {
        groups.push({ cell, colspan: 1 });
      }
    }
    result[row.key] = groups;
  }
  return result;
});

function getCell(row: MatrixRow, period: Period): CellData {
  const start = period.start;
  for (const doc of migDocuments.value) {
    if (!doc.containedMessageTypes?.includes(row.messageType)) continue;
    if (row.variant === 'strom' && !doc.isStrom) continue;
    if (row.variant === 'gas' && !doc.isGas) continue;
    const validFrom = DateTime.fromISO(doc.validFrom);
    const validTo = doc.validTo ? DateTime.fromISO(doc.validTo) : null;
    if (validFrom <= start && (!validTo || validTo >= start)) {
      return { version: doc.messageTypeVersion ?? '?', doc };
    }
  }
  return null;
}

function getMirrorUri(doc: components["schemas"]["EdiDocumentSlim"]): string {
  return '/api/' + doc.id + '/full';
}

const MONTH_ABBR = ['Jan', 'Feb', 'Mär', 'Apr', 'Mai', 'Jun', 'Jul', 'Aug', 'Sep', 'Okt', 'Nov', 'Dez'];

function formatPeriodShortLabel(period: Period): string {
  return MONTH_ABBR[period.start.month - 1] + period.start.toFormat('yy');
}

function formatPeriodHeader(period: Period): string {
  const start = formatD(period.start.toISO());
  if (!period.end) return `ab ${start}`;
  return `${start} – ${formatD(period.end.toISO())}`;
}
</script>

<template>
  <v-card title="MIG Gültigkeitsübersicht" class="ma-4" elevation="6">
    <v-container fluid>
      <v-row v-if="!migDocuments.length">
        <v-col>
          <v-alert type="info" text="Keine MIG-Dokumente gefunden." variant="tonal" />
        </v-col>
      </v-row>
      <v-row v-else>
        <v-col cols="12" style="overflow-x: auto;">
          <table class="matrix-table">
            <thead>
              <tr>
                <th class="sticky-col">Nachrichtentyp</th>
                <th v-if="hasHistoryColumn" class="period-header history-header">
                  <div class="period-short-label">Historie</div>
                  <div class="period-date-range" v-if="periods.length">
                    vor {{ formatPeriodShortLabel(periods[0]) }}
                  </div>
                </th>
                <th v-for="period in periods" :key="period.start.toISO()!" class="period-header">
                  <div class="period-short-label">{{ formatPeriodShortLabel(period) }}</div>
                  <div class="period-date-range">{{ formatPeriodHeader(period) }}</div>
                </th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="row in matrixRows" :key="row.key">
                <td class="sticky-col message-type-cell">
                  {{ row.messageType }}
                  <v-icon v-if="row.variant === 'strom'" size="small">mdi-lightning-bolt</v-icon>
                  <v-icon v-if="row.variant === 'gas'" size="small">mdi-fire</v-icon>
                </td>
                <td v-if="hasHistoryColumn" class="version-cell history-cell"
                  :class="{ 'cell-active': historyCells[row.key] !== null }">
                  <v-tooltip v-if="historyCells[row.key]" location="top">
                    <template #activator="{ props: tooltipProps }">
                      <v-chip
                        v-bind="tooltipProps"
                        color="secondary"
                        density="comfortable"
                        :href="getMirrorUri(historyCells[row.key]!.doc)"
                        target="_blank"
                        link
                        class="version-chip"
                        style="width: 100%; justify-content: center;"
                      >
                        {{ historyCells[row.key]!.version }}
                      </v-chip>
                    </template>
                    <div class="chip-tooltip">
                      <div><strong>{{ row.messageType }}<span v-if="row.variant === 'strom'"> (Strom)</span><span v-if="row.variant === 'gas'"> (Gas)</span></strong></div>
                      <div>Version: {{ historyCells[row.key]!.version }}</div>
                      <div>Gültigkeit: {{ formatD(historyCells[row.key]!.doc.validFrom) }} – {{ historyCells[row.key]!.doc.validTo ? formatD(historyCells[row.key]!.doc.validTo) : 'unbegrenzt' }}</div>
                      <div>{{ historyCells[row.key]!.doc.documentNameRaw ?? historyCells[row.key]!.doc.documentName }}</div>
                    </div>
                  </v-tooltip>
                  <span v-else class="text-disabled">–</span>
                </td>
                <td
                  v-for="(group, i) in groupedMatrixData[row.key]"
                  :key="i"
                  :colspan="group.colspan"
                  class="version-cell"
                  :class="{ 'cell-active': group.cell !== null }"
                >
                  <v-tooltip v-if="group.cell" location="top">
                    <template #activator="{ props: tooltipProps }">
                      <v-chip
                        v-bind="tooltipProps"
                        color="primary"
                        density="comfortable"
                        :href="getMirrorUri(group.cell.doc)"
                        target="_blank"
                        link
                        class="version-chip"
                        style="width: 100%; justify-content: center;"
                      >
                        {{ group.cell.version }}
                      </v-chip>
                    </template>
                    <div class="chip-tooltip">
                      <div><strong>{{ row.messageType }}<span v-if="row.variant === 'strom'"> (Strom)</span><span v-if="row.variant === 'gas'"> (Gas)</span></strong></div>
                      <div>Version: {{ group.cell.version }}</div>
                      <div>Gültigkeit: {{ formatD(group.cell.doc.validFrom) }} – {{ group.cell.doc.validTo ? formatD(group.cell.doc.validTo) : 'unbegrenzt' }}</div>
                      <div>{{ group.cell.doc.documentNameRaw ?? group.cell.doc.documentName }}</div>
                    </div>
                  </v-tooltip>
                  <span v-else class="text-disabled">–</span>
                </td>
              </tr>
            </tbody>
          </table>
        </v-col>
      </v-row>
      <v-row>
        <v-col cols="12">
          <v-switch v-model="showCompleteHistory" color="primary" density="compact" hide-details
            label="Komplette Historie anzeigen" class="ml-2 mb-2" />
        </v-col>
      </v-row>
    </v-container>
  </v-card>
</template>

<style scoped>
.matrix-table {
  border-collapse: collapse;
  width: 100%;
  min-width: max-content;
}

.matrix-table th,
.matrix-table td {
  border: 1px solid rgba(128, 128, 128, 0.3);
  padding: 6px 10px;
  text-align: center;
  white-space: nowrap;
}

.period-header {
  font-size: 0.8rem;
  font-weight: 600;
  min-width: 140px;
}

.period-short-label {
  font-size: 1rem;
  font-weight: 700;
  margin-bottom: 2px;
}

.period-date-range {
  font-size: 0.7rem;
  font-weight: 400;
  opacity: 0.75;
}

.sticky-col {
  position: sticky;
  left: 0;
  background: rgb(var(--v-theme-surface));
  z-index: 1;
  text-align: left;
  font-weight: 600;
  min-width: 120px;
}

.message-type-cell {
  font-weight: 500;
}

.version-cell {
  min-width: 120px;
}

.version-chip {
  text-decoration: none;
}

.chip-tooltip {
  line-height: 1.6;
  white-space: nowrap;
}

.cell-active {
  background-color: rgba(var(--v-theme-primary), 0.08);
}

.history-header {
  opacity: 0.7;
  font-style: italic;
  min-width: 100px;
}

.history-cell {
  opacity: 0.7;
}
</style>
