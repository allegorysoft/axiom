'use client';

import { useState } from 'react';

import type { ColumnVisibilityState } from '@tanstack/react-table';

import type { DataTableProps } from '../models';

export function useColumnVisibility(columns: DataTableProps['columns']) {
  const [columnVisibility, setColumnVisibility] =
    useState<ColumnVisibilityState>({});

  return { columnVisibility, setColumnVisibility };
}
