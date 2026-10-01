import { HugeiconsIcon } from '@hugeicons/react';
import { UserIcon, MailIcon, AtSignIcon } from '@hugeicons/core-free-icons';
import { useTranslation } from '@axiomframework/react-core';
import {
  Button,
  Field,
  FieldGroup,
  FieldLabel,
  InputGroup,
  InputGroupAddon,
  InputGroupInput,
  PasswordInput,
} from '@axiomframework/react-theme/components';
import { SocialLogins } from '../social-logins';

export function Component() {
  const t = useTranslation();
  return (
    <form noValidate onSubmit={(event) => event.preventDefault()}>
      <FieldGroup className="gap-4">
        <div className="grid gap-4 sm:grid-cols-2">
          <TextField
            name="name"
            label={t('AxiomAccount:Name')}
            autoComplete="given-name"
          />
          <TextField
            name="surname"
            label={t('AxiomAccount:Surname')}
            autoComplete="family-name"
          />
        </div>
        <TextField
          name="username"
          label={t('AxiomAccount:Username')}
          autoComplete="username"
        />
        <TextField
          name="email"
          label={t('AxiomAccount:EmailAddress')}
          autoComplete="email"
          type="email"
        />
        <Field className="gap-1.5">
          <FieldLabel htmlFor="new-password">
            {t('AxiomAccount:Password')} *
          </FieldLabel>
          <PasswordInput
            id="new-password"
            name="password"
            autoComplete="new-password"
            placeholder={t('AxiomAccount:Password')}
          />
        </Field>
        <Field className="gap-1.5">
          <FieldLabel htmlFor="confirm-password">
            {t('AxiomAccount:ConfirmPassword')} *
          </FieldLabel>
          <PasswordInput
            id="confirm-password"
            name="confirmPassword"
            autoComplete="new-password"
            placeholder={t('AxiomAccount:ConfirmPassword')}
          />
        </Field>
        <Field className="gap-1.5">
          <Button type="submit">{t('AxiomAccount:SignUp')}</Button>
        </Field>
        <SocialLogins />
      </FieldGroup>
    </form>
  );
}

function TextField({
  name,
  label,
  autoComplete,
  type = 'text',
}: {
  name: string;
  label: string;
  autoComplete: string;
  type?: string;
}) {
  return (
    <Field className="gap-1.5">
      <FieldLabel htmlFor={`signup-${name}`}>{label} *</FieldLabel>
      <InputGroup>
        <InputGroupAddon>
          <HugeiconsIcon
            icon={
              name === 'username'
                ? AtSignIcon
                : type === 'email'
                  ? MailIcon
                  : UserIcon
            }
            strokeWidth={2}
          />
        </InputGroupAddon>
        <InputGroupInput
          id={`signup-${name}`}
          name={name}
          type={type}
          autoComplete={autoComplete}
          placeholder={label}
        />
      </InputGroup>
    </Field>
  );
}
