'use client';

import type { OnChangeFn } from '@tanstack/react-table';
import { HugeiconsIcon } from '@hugeicons/react';
import {
  FilterIcon,
  FilterResetIcon,
  FloppyDiskIcon,
  Search,
} from '@hugeicons/core-free-icons';

import { useTranslation } from '@axiomframework/react-core';

import { Button } from '../../ui/button';
import {
  Popover,
  PopoverContent,
  PopoverDescription,
  PopoverHeader,
  PopoverTitle,
  PopoverTrigger,
} from '../../ui/popover';
import {
  InputGroup,
  InputGroupAddon,
  InputGroupInput,
} from '../../ui/input-group';

import type { DataTableProps } from '../models';
import { Separator } from '../../ui/separator';

type FilterProps = Pick<DataTableProps, 'filter'>;
type Props = {
  globalFilter?: string;
  setGlobalFilter?: OnChangeFn<string>;
} & FilterProps;

export function DataTableFilter({
  globalFilter,
  setGlobalFilter,
  filter,
}: Props) {
  const t = useTranslation();
  const Content = filter?.content;

  return (
    <section className="flex items-center gap-2">
      <InputGroup className="max-w-3xs">
        <InputGroupInput
          placeholder={t('AxiomTheme:Search')}
          value={globalFilter ?? ''}
          onChange={(event) => {
            if (setGlobalFilter) {
              setGlobalFilter(() => event.target.value);
            }
          }}
        />
        <InputGroupAddon>
          <HugeiconsIcon icon={Search} />
        </InputGroupAddon>
      </InputGroup>

      <Popover>
        <PopoverTrigger
          render={
            <Button variant="outline" className="cursor-pointer">
              {t('AxiomTheme:Filters')}
              <HugeiconsIcon icon={FilterIcon} />
            </Button>
          }
        />

        <PopoverContent
          align="end"
          className="max-h-[min(32rem,80vh)] w-[min(35rem,calc(100vw-2rem))] overflow-y-auto"
        >
          {(filter?.title || filter?.description) && (
            <PopoverHeader>
              <PopoverTitle>{filter?.title}</PopoverTitle>
              <PopoverDescription>{filter?.description}</PopoverDescription>
            </PopoverHeader>
          )}

          {Content && (
            <form
              className="grid gap-2 p-1"
              autoComplete="off"
              onSubmit={filter.onSubmit}
            >
              <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                {Content}
              </div>

              <Separator />

              <div className="col-span-full flex justify-end gap-1">
                <Button
                  type="button"
                  variant="outline"
                  onClick={(event) => {
                    filter?.onClear();
                    event.currentTarget.form?.reset();
                  }}
                >
                  <HugeiconsIcon
                    icon={FilterResetIcon}
                    className="mr-2 size-4"
                  />
                  Clear
                </Button>
                <Button type="submit">
                  <HugeiconsIcon
                    icon={FloppyDiskIcon}
                    className="mr-2 size-4"
                  />
                  Save
                </Button>
              </div>
            </form>
          )}
        </PopoverContent>
      </Popover>
    </section>
  );
}
