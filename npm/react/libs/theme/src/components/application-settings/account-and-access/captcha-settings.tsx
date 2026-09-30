import { form } from '../form-wrapper';
import { text, toggle } from '../utils';

export default function CaptchaSettings() {
  return form('captcha', [
    toggle('enabled', 'Enable CAPTCHA'),
    {
      key: 'provider',
      label: 'Provider',
      type: 'select',
      initial: 'turnstile',
      options: [
        { value: 'turnstile', label: 'Cloudflare Turnstile' },
        { value: 'recaptcha', label: 'Google reCAPTCHA' },
        { value: 'hcaptcha', label: 'hCaptcha' },
      ],
    },
    text('siteKey', 'Site Key'),
    toggle('login', 'Use On Sign In', true),
    toggle('registration', 'Use On Registration', true),
    toggle('passwordReset', 'Use On Password Reset', true),
  ]);
}
