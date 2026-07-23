export interface OAuth {
  authority: string;
  clientId: string;
  scope: string;
  responseType: 'password' | 'code';
  redirectUri: string;
}
