import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router';
import { useTranslation } from '@axiomframework/react-core';
import { oAuthProvider, useOAuth } from '@axiomframework/react-oauth';
import { Button } from '@axiomframework/react-theme/components';

export function Component() {
  const navigate = useNavigate();
  const t = useTranslation();
  const auth = useOAuth((s) => s.token);
  const [isAuthenticated, setIsAuthenticated] = useState(false);

  useEffect(() => {
    setIsAuthenticated(!!auth?.accessToken.length);
  }, [auth?.accessToken]);

  function logIn() {
    oAuthProvider.get().redirectToLogin(() => navigate('/account/login'));
  }

  function logOut() {
    oAuthProvider.get().logout();
  }

  return (
    <>
      {!isAuthenticated && (
        <Button onClick={logIn}>{t('AxiomAccount:SignIn')} </Button>
      )}
      {isAuthenticated && (
        <>
          <span className="mx-3">
            {t('AxiomIdentity:Welcome', { name: 'Masum' })}
          </span>
          <Button onClick={logOut}>{t('AxiomAccount:Logout')} </Button>
        </>
      )}
    </>
  );
}
