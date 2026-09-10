import type { ReactNode, ElementType } from 'react';
import { useFormContext, type FieldValues, type Path } from 'react-hook-form';
import { useTranslation } from '@axiomframework/react-core';
import { tryParse } from '../utils/validation-message-parser';
import { hasKeyProperty } from '../utils/key-util';

type FormFieldProps<T extends FieldValues> = {
  name: Path<T>;
  children: ReactNode;
  Container?: ElementType;
};

export function FormField<T extends FieldValues>({
  name,
  children,
  Container,
}: FormFieldProps<T>) {
  const { getFieldState, formState } = useFormContext<T>();
  const { error } = getFieldState(name, formState);
  const t = useTranslation();

  const parsed = error?.message ? tryParse(error.message) : null;
  const localized = localizeArgs(parsed, t);

  const content = (
    <>
      {children}
      {localized && (
        <p
          id={`${name}-error`}
          role="alert"
          aria-live="polite"
          className="text-red-400"
        >
          {t(String(localized.key), localized.args)}
        </p>
      )}
    </>
  );

  return Container ? <Container>{content}</Container> : content;
}

type LocalizedArgs = ReturnType<typeof tryParse> | null;

function localizeArgs(
  parsed: LocalizedArgs,
  t: ReturnType<typeof useTranslation>,
) {
  if (!parsed) {
    return null;
  }

  if (!parsed.args || !hasKeyProperty(parsed.args)) {
    return parsed;
  }

  return {
    ...parsed,
    args: {
      ...parsed.args,
      key: t(parsed.args.key as string),
    },
  };
}
