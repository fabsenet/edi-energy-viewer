import { ref, type Ref } from "vue";


export type DocumentTypes = 'MIG' | 'AHB' | 'Allgemein';
export type TimeFrames = 'Vergangen' | 'Aktuell' | 'Zukünftig';

export type Filter = {
  onlyLatestVersion: boolean;
  messageType: string | 'ALL';
  type: DocumentTypes | 'ALL';
  timeFrame: TimeFrames | 'ALL';
  checkIdentifier: string | null;
  showXmlDownloadButtons: boolean; // Show/hide XML download buttons
};
export const availableTypes = ref<DocumentTypes[]>(['MIG', 'AHB', 'Allgemein']);
export const availableTimeFrames = ref<TimeFrames[]>(['Vergangen', 'Aktuell', 'Zukünftig']);

export const defaultFilter = () => ref<Filter>({
  onlyLatestVersion: false,
  messageType: 'ALL',
  type: 'ALL',
  timeFrame: 'ALL',
  checkIdentifier: '',
  showXmlDownloadButtons: false
});

export function filterFromLocalStorageOrDefault(): Ref<Filter> {
  try {

    const filterString = localStorage.getItem('edidocs_filter');
    if (filterString) {
      return ref(<Filter>JSON.parse(filterString));
    }
    return defaultFilter();
  }
  catch (error) {
    console.error('Error loading filter from local storage', error);
    return defaultFilter();
  }
}

export function saveFilterToLocalStorage(filter: Filter) {
  localStorage.setItem('edidocs_filter', JSON.stringify(filter));
}

