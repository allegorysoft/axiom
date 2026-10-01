import { Button, FieldSeparator } from '@axiomframework/react-theme/components';
import { useTranslation } from '@axiomframework/react-core';
import google from '../assets/google-logo.svg';
import apple from '../assets/apple-logo.svg';
import github from '../assets/github-logo.svg';
import facebook from '../assets/facebook.svg';

export function SocialLogins() {
  const t = useTranslation();
  return (
    <>
      <FieldSeparator className="*:data-[slot=field-separator-content]:bg-card">
        {t('AxiomAccount:OrContinueWith')}
      </FieldSeparator>
      <div className="flex justify-center gap-3">
        {[
          ['Google', google],
          ['Apple', apple],
          ['GitHub', github],
          ['Facebook', facebook],
        ].map(([name, src]) => (
          <Button
            key={name}
            type="button"
            variant="outline"
            size="icon"
            aria-label={`${t('AxiomAccount:SignIn')}: ${name}`}
          >
            <img
              src={src}
              alt=""
              className={`size-4 object-contain ${name === 'Apple' || name === 'GitHub' ? 'invert dark:invert-0' : ''}`}
            />
          </Button>
        ))}
      </div>
    </>
  );
}
