import {
  type Configuration,
  type TokenEndpointResponse,
  type TokenEndpointResponseHelpers,
  allowInsecureRequests,
  discovery,
  tokenRevocation,
} from 'openid-client';
import {
  type OAuthToken,
  AbstractAuthFlow,
  isDevMode,
} from '@axiomframework/react-core';
import { oAuthStore } from './oauth-store';

const noop = (): void => {};

export abstract class BaseAuthFlow extends AbstractAuthFlow {
  protected configuration?: Configuration;

  /**
   * Entry of authentication flow
   */
  override async initialize(): Promise<void> {
    const token = this.storage.get();
    if (token !== null) {
      oAuthStore.setToken(token);
    }

    await this.discover();
  }

  protected async discover(): Promise<void> {
    const configuration = await discovery(
      new URL(`${this.options.authority}`),
      this.options.clientId,
      {},
      undefined,
      { execute: [isDevMode() ? allowInsecureRequests : noop] },
    );

    this.configuration = configuration;
  }

  protected setToken(
    token: TokenEndpointResponse & TokenEndpointResponseHelpers,
  ) {
    const oAuthToken: OAuthToken = {
      accessToken: token.access_token,
      refreshToken: token.refresh_token,
      expiresAt: token.expires_in
        ? Date.now() + token.expires_in * 1000
        : undefined,
    };
    this.storage.set(oAuthToken);
    oAuthStore.setToken(oAuthToken);
  }

  override async logout(): Promise<void> {
    if (!this.configuration) {
      return;
    }

    const token = this.storage.get()?.refreshToken;
    if (token) {
      await tokenRevocation(this.configuration, token);
    }

    this.storage.clear();
    oAuthStore.clear();
  }
}
