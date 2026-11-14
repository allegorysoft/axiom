import type {
  Column_ColumnResizing,
  Column_ColumnSizing,
  Header_ColumnResizing,
  Header_ColumnSizing,
  RowData,
  Row_ColumnVisibility,
  Row_RowSelection,
  TableFeatures,
  TableOptions,
} from '@tanstack/react-table';

export type TFeat = TableFeatures;
export type TData = RowData;

export type OptionalRow = Partial<Row_ColumnVisibility<TFeat, TData> & Row_RowSelection>;
export type DataTableProps = TableOptions<TFeat, TData> & {
  showPageButtons?: boolean;
  columnResizing?: boolean;
};

export type ColumnResizing = Header_ColumnSizing & Partial<Header_ColumnResizing & Column_ColumnResizing & Column_ColumnSizing>;
