export interface OAuth {
  authority: string;
  clientId: string;
  scope: string;
  flow: 'password' | 'code';
  redirectUri: string;
}
