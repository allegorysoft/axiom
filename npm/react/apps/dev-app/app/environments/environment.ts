import { Environment } from '@axiomframework/react-core';

export const environment: Environment = {
  production: false,
  oauth: {
    authority: 'http://localhost',
    clientId: 'react_app',
    redirectUri: 'http://localhost:5173',
    responseType: 'password',
    scope: 'react_app email',
  },
  endpoints: {
    default: { url: 'http://localhost/api' },
  },
};
