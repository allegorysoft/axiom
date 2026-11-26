'use client';

import { useId, type ComponentProps } from 'react';
import { closestCenter, DndContext } from '@dnd-kit/core';
import { restrictToHorizontalAxis } from '@dnd-kit/modifiers';

type Props = Pick<
  ComponentProps<typeof DndContext>,
  'children' | 'sensors' | 'onDragEnd'
>;

export function DataTableDndContext({ children, ...props }: Props) {
  return (
    <DndContext
      id={useId()}
      collisionDetection={closestCenter}
      modifiers={[restrictToHorizontalAxis]}
      {...props}
    >
      {children}
    </DndContext>
  );
}
