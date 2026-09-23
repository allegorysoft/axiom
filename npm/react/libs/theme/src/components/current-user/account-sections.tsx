import { useRef, useState } from 'react';
import { getAvatarFallbackText } from '@axiomframework/react-core';
import { Avatar, AvatarFallback, AvatarImage } from '../ui/avatar';
import { Button } from '../ui/button';
import { Input } from '../ui/input';
import { Field, FieldGroup, FieldLabel, FieldDescription } from '../ui/field';
import { Separator } from '../ui/separator';
import {
  Dialog,
  DialogContent,
  DialogTitle,
  DialogDescription,
  DialogHeader,
  DialogFooter,
  DialogClose,
} from '../ui/dialog';
import { userProfileStore, useUserProfile } from './user-profile-store';

export function ProfilePhotoSection() {
  const user = useUserProfile((state) => state);
  const [preview, setPreview] = useState(user.photo);
  const [error, setError] = useState('');
  const [reading, setReading] = useState(false);
  const [saved, setSaved] = useState(false);
  const uploadRef = useRef<HTMLInputElement>(null);
  const readerRef = useRef<FileReader | null>(null);

  function selectPhoto(file?: File) {
    if (!file) return;
    setSaved(false);
    if (!['image/jpeg', 'image/png', 'image/webp'].includes(file.type)) {
      setError('Choose a JPG, PNG or WebP image.');
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      setError('Choose an image smaller than 5 MB.');
      return;
    }
    readerRef.current?.abort();
    const reader = new FileReader();
    readerRef.current = reader;
    setReading(true);
    setError('');
    reader.onload = () => {
      setPreview(String(reader.result));
      setReading(false);
    };
    reader.onerror = () => {
      setError('This image could not be read. Please try another.');
      setReading(false);
    };
    reader.readAsDataURL(file);
  }

  return (
    <FieldGroup>
      <div className="flex flex-col items-start gap-6 sm:flex-row sm:items-center">
        <Avatar className="size-28">
          <AvatarImage
            src={preview || undefined}
            alt={user.name}
            onError={() =>
              setError(
                'This image could not be displayed. Please choose another.',
              )
            }
          />
          <AvatarFallback className="text-3xl">
            {getAvatarFallbackText(user.name)}
          </AvatarFallback>
        </Avatar>
        <div className="space-y-3">
          <div className="flex flex-wrap gap-2">
            <Button
              variant="outline"
              disabled={reading}
              onClick={() => uploadRef.current?.click()}
            >
              {preview ? 'Change photo' : 'Upload photo'}
            </Button>
            <Button
              variant="ghost"
              className="text-destructive hover:bg-destructive/10 hover:text-destructive"
              disabled={!preview || reading}
              onClick={() => {
                setPreview('');
                setError('');
                setSaved(false);
              }}
            >
              Remove photo
            </Button>
          </div>
          <p className="text-sm text-muted-foreground">
            JPG, PNG or WebP. Maximum 5 MB.
          </p>
        </div>
      </div>
      <Input
        ref={uploadRef}
        type="file"
        accept="image/jpeg,image/png,image/webp"
        className="sr-only"
        aria-label="Upload profile photo"
        onChange={(event) => {
          selectPhoto(event.target.files?.[0]);
          event.target.value = '';
        }}
      />
      {error && (
        <p role="alert" className="text-sm text-destructive">
          {error}
        </p>
      )}
      <Separator />
      <FieldDescription>
        Changes are kept for this session. Account sync will be available later.
      </FieldDescription>
      <div className="flex gap-2">
        <Button
          disabled={preview === user.photo || reading || !!error}
          onClick={() => {
            userProfileStore.set((state) => ({ ...state, photo: preview }));
            setSaved(true);
          }}
        >
          Save changes
        </Button>
        <Button
          variant="outline"
          disabled={reading || preview === user.photo}
          onClick={() => {
            setPreview(user.photo);
            setError('');
            setSaved(false);
          }}
        >
          Cancel
        </Button>
      </div>
      {saved && (
        <p role="status" className="text-sm text-muted-foreground">
          Profile photo updated for this session.
        </p>
      )}
    </FieldGroup>
  );
}

export function AccountInfoSection() {
  const user = useUserProfile((state) => state);
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
        userProfileStore.set((state) => ({
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

export function SecuritySection() {
  const [setupOpen, setSetupOpen] = useState(false);
  return (
    <FieldGroup>
      <Field>
        <h3 className="font-medium">Change Password</h3>
        <FieldDescription>
          Password changes will be available when account services are
          connected.
        </FieldDescription>
      </Field>
      <Field>
        <FieldLabel htmlFor="current-password">Current Password</FieldLabel>
        <Input
          id="current-password"
          type="password"
          autoComplete="current-password"
          disabled
        />
      </Field>
      <Field>
        <FieldLabel htmlFor="new-password">New Password</FieldLabel>
        <Input
          id="new-password"
          type="password"
          autoComplete="new-password"
          disabled
        />
      </Field>
      <Field>
        <FieldLabel htmlFor="confirm-password">Confirm New Password</FieldLabel>
        <Input
          id="confirm-password"
          type="password"
          autoComplete="new-password"
          disabled
        />
      </Field>
      <div>
        <Button disabled>Update password</Button>
      </div>
      <Separator />
      <Field>
        <h3 className="font-medium">Two-Factor Authentication</h3>
        <FieldDescription>
          Add another layer of protection with a verification code from an
          authenticator app.
        </FieldDescription>
        <div className="mt-2">
          <Button variant="outline" onClick={() => setSetupOpen(true)}>
            Set Up Two-Factor Authentication
          </Button>
        </div>
      </Field>
      <Dialog open={setupOpen} onOpenChange={setSetupOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Set Up Two-Factor Authentication</DialogTitle>
            <DialogDescription>
              Authenticator setup will be available when account services are
              connected.
            </DialogDescription>
          </DialogHeader>
          <ol className="list-decimal space-y-3 pl-5 text-sm text-muted-foreground">
            <li>Scan your account QR code with an authenticator app.</li>
            <li>Enter the six-digit code to verify your device.</li>
            <li>Save your recovery codes in a safe place.</li>
          </ol>
          <Field>
            <FieldLabel htmlFor="verification-code">
              Verification Code
            </FieldLabel>
            <Input
              id="verification-code"
              placeholder="000000"
              inputMode="numeric"
              autoComplete="one-time-code"
              maxLength={6}
              disabled
            />
          </Field>
          <DialogFooter>
            <DialogClose render={<Button variant="outline" />}>
              Close
            </DialogClose>
            <Button disabled>Enable two-factor authentication</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </FieldGroup>
  );
}
