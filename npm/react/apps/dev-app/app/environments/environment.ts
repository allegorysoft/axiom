import { Environment } from '@axiomframework/react-core';

export const environment: Environment = {
  production: false,
  oauth: {
    authority: 'http://127.0.0.1:8080/realms/master',
    clientId: 'account',
    flow: 'code',
    scope: 'openid profile email',
    redirectUri: 'http://localhost:5173',
  },
  endpoints: {
    Default: { url: 'http://localhost:3000' },
  },
};
