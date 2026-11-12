import type { Column, RowData, TableFeatures } from '@tanstack/react-table';

export interface DataTableColumnProps<
  TFeatures extends TableFeatures,
  TData extends RowData,
  TValue = unknown,
> extends React.HTMLAttributes<HTMLDivElement> {
  column: Column<TFeatures, TData, TValue>;
}
