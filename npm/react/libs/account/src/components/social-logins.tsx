import { Button, FieldSeparator } from '@axiomframework/react-theme/components';
import { useTranslation } from '@axiomframework/react-core';

export function SocialLogins() {
  const t = useTranslation();

  return (
    <>
      <FieldSeparator className="*:data-[slot=field-separator-content]:bg-card">
        {t('AxiomAccount:OrContinueWith')}
      </FieldSeparator>
      <div className="flex justify-center gap-3">
        {[
          ['Google', new URL('../../assets/google-logo.svg', import.meta.url).href],
          ['Apple', new URL('../../assets/apple-logo.svg', import.meta.url).href],
          ['GitHub', new URL('../../assets/github-logo.svg', import.meta.url).href],
          ['Facebook', new URL('../../assets/facebook.svg', import.meta.url).href],
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
