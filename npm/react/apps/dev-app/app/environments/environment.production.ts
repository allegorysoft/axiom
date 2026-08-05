import { Environment } from '@axiomframework/react-core';

export const environment: Environment = {
  production: true,
  oauth: {
    authority: 'http://localhost',
    clientId: 'react_app',
    redirectUri: 'http://localhost:5173',
    responseType: 'password',
    scope: 'react_app email',
  },
  endpoints: {
    Default: { url: 'http://localhost:3000' },
  },
};
