//see https://vuetifyjs.com/en/api/v-data-table/#props-headers for full list

type DataTableHeader = {
  key?: (string & {})
  | 'data-table-group'
  | 'data-table-select'
  | 'data-table-expand'
  | undefined;
  value?: SelectItemKey;
  title?: string;
  colspan?: number;
  rowspan?: number;
  fixed?: boolean;
  align?: 'start' | 'end' | 'center' | undefined;
  width?: string | number | undefined;
  minWidth?: string;
  maxWidth?: string;
  sortable?: boolean;
  sort?: DataTableCompareFunction;
  sortRaw?: DataTableCompareFunction | undefined;
  nowrap?: boolean | undefined;
  headerProps?: { [x: string]: any } | undefined;
  filter?: FilterFunction | undefined;
  mobile?: boolean | undefined;
  children?: any[] | undefined;
  cellProps?:
  | { [x: string]: any }
  | ((
    data: Pick<
      ItemKeySlot<any>,
      'value' | 'item' | 'index' | 'internalItem'
    >,
  ) => Record<string, any>)
  | undefined
};
