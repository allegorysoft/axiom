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

export type OptionalRow<T extends TableFeatures, D extends RowData> = Partial<
  Row_ColumnVisibility<T, D> & Row_RowSelection
>;
export type DataTableProps<
  T extends TableFeatures,
  D extends RowData,
> = TableOptions<T, D> & {
  showPageButtons?: boolean;
  columnResizing?: boolean;
};

export type ColumnResizing = Header_ColumnSizing & Partial<Header_ColumnResizing & Column_ColumnResizing & Column_ColumnSizing>;
