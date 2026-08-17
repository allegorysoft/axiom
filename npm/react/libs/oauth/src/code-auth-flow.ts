import {
  authorizationCodeGrant,
  buildAuthorizationUrl,
  calculatePKCECodeChallenge,
  randomPKCECodeVerifier,
  randomState,
} from 'openid-client';
import { BaseAuthFlow } from './base-auth-flow';

export class CodeAuthFlow extends BaseAuthFlow {
  override async initialize(): Promise<void> {
    await super.initialize();
    const currentUrl = new URL(window.location.href);

    if (currentUrl.searchParams.has('code')) {
      await this.handleCallback(currentUrl);
    }
  }

  override async login(): Promise<void> {
    await this.redirectToAuthorization();
  }

  private async redirectToAuthorization(): Promise<void> {
    if (this.storage.get()?.accessToken) {
      return;
    }

    if (!this.options.redirectUri) {
      throw new Error(
        'redirectUri is required but was not provided in options',
      );
    }

    const codeVerifier = randomPKCECodeVerifier();
    const codeChallenge = await calculatePKCECodeChallenge(codeVerifier);
    const state = randomState();

    sessionStorage.setItem('pkce_code_verifier', codeVerifier);
    sessionStorage.setItem('oauth_state', state);

    const authorizationUrl = buildAuthorizationUrl(this.configuration!, {
      redirect_uri: ensureEndsWithSlash(this.options.redirectUri),
      scope: this.options.scope,
      code_challenge: codeChallenge,
      code_challenge_method: 'S256',
      state,
    });

    window.location.assign(authorizationUrl.href);
  }

  private async handleCallback(currentUrl: URL): Promise<void> {
    const pkceCodeVerifier =
      sessionStorage.getItem('pkce_code_verifier') ?? undefined;
    const expectedState = sessionStorage.getItem('oauth_state') ?? undefined;

    try {
      const token = await authorizationCodeGrant(
        this.configuration!,
        currentUrl,
        {
          pkceCodeVerifier,
          expectedState,
        },
      );

      sessionStorage.removeItem('pkce_code_verifier');
      sessionStorage.removeItem('oauth_state');

      this.setToken(token);
    } catch {}

    window.history.replaceState({}, '', window.location.pathname);
  }
}

function ensureEndsWithSlash(value?: string | null): string {
  if (!value) {
    return '/';
  }

  return value.endsWith('/') ? value : `${value}/`;
}
