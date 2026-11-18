import type {
  RowData,
  TableFeatures,
  TableOptions,
} from '@tanstack/react-table';

export type TFeat = TableFeatures;
export type TData = RowData;

export type DataTableProps = TableOptions<TFeat, TData> & {
  showPageButtons?: boolean;
  columnResizing?: boolean;
};
