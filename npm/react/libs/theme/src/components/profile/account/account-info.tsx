import { useState } from 'react';

import { userStore, useUser } from '@axiomframework/react-core';

import { Button } from '../../ui/button';
import { Input } from '../../ui/input';
import { Field, FieldGroup, FieldLabel, FieldDescription } from '../../ui/field';
import { Separator } from '../../ui/separator';

export default function AccountInfoSection() {
  const user = useUser((state) => state);
  const [draft, setDraft] = useState({
    firstName: user.firstName,
    surname: user.surname,
    email: user.email,
    phone: user.phone,
  });
  const [saved, setSaved] = useState(false);
  const fields = [
    {
      key: 'firstName',
      label: 'Name',
      type: 'text',
      autoComplete: 'given-name',
      required: true,
    },
    {
      key: 'surname',
      label: 'Surname',
      type: 'text',
      autoComplete: 'family-name',
      required: true,
    },
    {
      key: 'email',
      label: 'Email Address',
      type: 'email',
      autoComplete: 'email',
      required: true,
    },
    {
      key: 'phone',
      label: 'Phone Number',
      type: 'tel',
      autoComplete: 'tel',
      required: false,
    },
  ] as const;
  const dirty = fields.some(({ key }) => draft[key] !== user[key]);
  return (
    <form
      onSubmit={(event) => {
        event.preventDefault();
        userStore.set((state) => ({
          ...state,
          ...draft,
          name: [draft.firstName.trim(), draft.surname.trim()]
            .filter(Boolean)
            .join(' '),
          firstName: draft.firstName.trim(),
          surname: draft.surname.trim(),
          email: draft.email.trim(),
          phone: draft.phone.trim(),
        }));
        setSaved(true);
      }}
    >
      <FieldGroup>
        {fields.map(({ key, label, ...props }) => (
          <Field key={key}>
            <FieldLabel htmlFor={`account-${key}`}>{label}</FieldLabel>
            <Input
              id={`account-${key}`}
              {...props}
              value={draft[key]}
              maxLength={key === 'phone' ? 30 : 254}
              pattern={
                key === 'firstName' || key === 'surname' ? '.*\\S.*' : undefined
              }
              onChange={(event) => {
                setDraft({ ...draft, [key]: event.target.value });
                setSaved(false);
              }}
            />
          </Field>
        ))}
        <Separator />
        <FieldDescription>
          Changes are kept for this session. Account sync will be available
          later.
        </FieldDescription>
        <div className="flex gap-2">
          <Button type="submit" disabled={!dirty}>
            Save changes
          </Button>
          <Button
            type="button"
            variant="outline"
            disabled={!dirty}
            onClick={() => {
              setDraft({
                firstName: user.firstName,
                surname: user.surname,
                email: user.email,
                phone: user.phone,
              });
              setSaved(false);
            }}
          >
            Cancel
          </Button>
        </div>
        {saved && (
          <p role="status" className="text-sm text-muted-foreground">
            Account information updated for this session.
          </p>
        )}
      </FieldGroup>
    </form>
  );
}
