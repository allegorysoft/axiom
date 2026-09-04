import * as React from 'react';
import { EyeIcon, EyeOffIcon, LockIcon } from 'lucide-react';
import { InputGroup, InputGroupAddon, InputGroupInput } from './ui/input-group';

export function PasswordInput({
  ...inputProps
}: React.ComponentProps<typeof InputGroupInput>) {
  const [visible, setVisible] = React.useState(false);

  return (
    <InputGroup>
      <InputGroupAddon>
        <LockIcon />
      </InputGroupAddon>

      <InputGroupInput {...inputProps} type={visible ? 'text' : 'password'} />

      <InputGroupAddon align="inline-end">
        <button
          type="button"
          onClick={() => setVisible((v) => !v)}
          aria-label={visible ? 'Hide password' : 'Show password'}
          tabIndex={-1}
        >
          {visible ? <EyeOffIcon /> : <EyeIcon />}
        </button>
      </InputGroupAddon>
    </InputGroup>
  );
}
