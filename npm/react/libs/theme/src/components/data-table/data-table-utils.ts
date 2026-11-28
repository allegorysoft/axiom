import {
  type RowData,
  type TableFeatures,
  createColumnHelper,
} from '@tanstack/react-table';

export function createDataTableColumnHelper<D extends RowData>() {
  return createColumnHelper<TableFeatures, D>();
}

export function isFixedOrderColumn(column: { id?: string; meta?: object }) {
  return column.id === 'actions' || isSelectionColumn(column);
}

export function isSelectionColumn(column: { meta?: object }) {
  return (
    column.meta !== undefined &&
    'fixedSelection' in column.meta &&
    column.meta.fixedSelection === true
  );
}
