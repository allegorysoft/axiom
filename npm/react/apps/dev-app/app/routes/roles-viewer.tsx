import { useTranslation } from '@axiomframework/react-core';

export function Component() {
  const t = useTranslation();
  return <h1>{t('AxiomBase:RolesViewer')}</h1>;
}
