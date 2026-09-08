import { useFormContext, type FieldValues, type Path } from 'react-hook-form';
import { useTranslation } from '@axiomframework/react-core';

type FormFieldProps<T extends FieldValues> = {
  name: Path<T>;
  children: React.ReactNode;
  Container?: React.ElementType;
};

export function FormField<T extends FieldValues>({
  name,
  children,
  Container,
}: FormFieldProps<T>) {
  const { getFieldState, formState } = useFormContext<T>();
  const { error } = getFieldState(name, formState);
  const t = useTranslation();
  const localizedMessage = tryParse(error?.message);

  const content = (
    <>
      {children}
      {localizedMessage && (
        <p className="text-red-400">
          {t(String(localizedMessage?.key), localizedMessage?.args)}
        </p>
      )}
    </>
  );

  if (!Container) {
    return content;
  }

  return <Container>{content}</Container>;
}

type ParsedErrorMessage = {
  key: string;
  args?: Record<string, unknown> | readonly unknown[];
};

function tryParse(message: string | undefined): ParsedErrorMessage | null {
  if (!message?.length) {
    return null;
  }

  try {
    const parsed = JSON.parse(message);
    if (parsed && typeof parsed === 'object' && 'key' in parsed) {
      return {
        key: String(parsed.key),
        args: parsed.args ?? {},
      };
    }

    return { key: message };
  } catch {
    return { key: message };
  }
}
