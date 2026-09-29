import { useRef, useState } from 'react';
import {
  getAvatarFallbackText,
  userStore,
  useUser,
} from '@axiomframework/react-core';
import { Avatar, AvatarFallback, AvatarImage } from '../../ui/avatar';
import { Button } from '../../ui/button';
import { Input } from '../../ui/input';
import { FieldGroup, FieldDescription } from '../../ui/field';
import { Separator } from '../../ui/separator';

export default function ProfilePhotoSection() {
  const user = useUser((state) => state);
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
            userStore.set((state) => ({ ...state, photo: preview }));
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
