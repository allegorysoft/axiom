import { localizerStore } from '@axiomframework/react-core';

export default function Home() {
  return (
    <>
      <p>
        {localizerStore.localize('Welcome', 'AxiomIdentity', { name: 'Masum' })}
      </p>
    </>
  );
}
