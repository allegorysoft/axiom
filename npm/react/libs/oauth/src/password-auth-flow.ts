import { genericGrantRequest } from 'openid-client';
import { BaseAuthFlow } from './base-auth-flow';

export class PasswordAuthFlow extends BaseAuthFlow {
  override async login(username: string, password: string): Promise<void> {
    const grant_type = 'password';
    const body = new URLSearchParams({
      grant_type,
      client_id: this.options.clientId,
      username,
      password,
      scope: this.options.scope,
    });

    const response = await genericGrantRequest(
      this.configuration!,
      grant_type,
      body,
    );

    this.storage.set({
      accessToken: response.access_token,
      refreshToken: response.refresh_token,
      expiresAt: response.expires_in,
    });
  }
}
