'use client';

import { useState } from 'react';

import type { ColumnVisibilityState } from '@tanstack/react-table';

export function useColumnVisibility() {
  const [columnVisibility, setColumnVisibility] = useState<ColumnVisibilityState>({});
  return { columnVisibility, setColumnVisibility };
}
