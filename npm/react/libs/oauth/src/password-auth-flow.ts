import { genericGrantRequest } from 'openid-client';
import { BaseAuthFlow } from './base-auth-flow';

export class PasswordAuthFlow extends BaseAuthFlow {
  override async login(username: string, password: string): Promise<void> {
    if (this.storage.get()?.accessToken) {
      return;
    }

    const grant_type = 'password';
    const body = new URLSearchParams({
      grant_type,
      client_id: this.options.clientId,
      username,
      password,
      scope: this.options.scope,
    });

    const token = await genericGrantRequest(
      this.configuration!,
      grant_type,
      body,
    );

    this.setToken(token);
  }
}
