import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router';
import { useTranslation } from '@axiomframework/react-core';
import { oAuthProvider, useOAuth } from '@axiomframework/react-oauth';
import { Button } from '@axiomframework/react-theme/components';

export default function Home() {
  const navigate = useNavigate();
  const t = useTranslation('AxiomIdentity');
  const auth = useOAuth((s) => s.token);
  const [isAuthenticated, setIsAuthenticated] = useState(false);

  useEffect(() => {
    setIsAuthenticated(!!auth?.accessToken.length);
  }, [auth?.accessToken]);

  function logIn() {
    oAuthProvider.get().redirectToLogin(() => navigate('/login'));
  }

  function logOut() {
    oAuthProvider.get().logout();
  }

  return (
    <>
      <span className="mx-3">{t('Welcome', { name: 'Masum' })}</span>
      {!isAuthenticated && <Button onClick={logIn}>{t('Login')}</Button>}
      {isAuthenticated && <Button onClick={logOut}>{t('Logout')}</Button>}
    </>
  );
}
