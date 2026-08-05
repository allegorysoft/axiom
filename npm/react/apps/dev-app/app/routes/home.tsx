import { useTranslation } from '@axiomframework/react-core';

export default function Home() {
  const t = useTranslation('AxiomIdentity');
  return (
    <>
      <span>{t('Welcome', { name: 'Masum' })}</span>
    </>
  );
}
