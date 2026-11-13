import type {
  Column,
  Column_RowSorting,
  RowData,
  TableFeatures,
} from '@tanstack/react-table';

export interface DataTableSortingHeaderProps<
  T extends TableFeatures,
  D extends RowData,
  V = unknown,
> extends React.HTMLAttributes<HTMLDivElement> {
  column: Column<T, D, V> & Pick<Column_RowSorting<T, D>, 'getCanSort' | 'getIsSorted' | 'toggleSorting'>;
  title: string;
}
