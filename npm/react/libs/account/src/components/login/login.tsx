import { useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router';
import { EyeIcon, EyeOffIcon, LockIcon, MailIcon } from 'lucide-react';

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';

import {
  getOrSetAuthProvider,
  useTranslation,
} from '@axiomframework/react-core';
import {
  Button,
  Field,
  FieldGroup,
  FieldLabel,
  FieldSeparator,
  InputGroup,
  InputGroupAddon,
  InputGroupInput,
} from '@axiomframework/react-theme/components';

import { schema, LoginParams } from './login-schema';

export function loader() {
  return null;
}

export function ErrorBoundary() {
  return <div>Failed to load login!</div>;
}

export function Component() {
  const t = useTranslation();
  const navigate = useNavigate();
  const [params] = useSearchParams();

  const [showPassword, setShowPassword] = useState(false);
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginParams>({
    resolver: zodResolver(schema),
    mode: 'onSubmit',
  });

  const onSubmit = async (input: LoginParams) => {
    const provider = getOrSetAuthProvider();
    await provider.get().login(input.usernameOrEmail, input.password);
    const returnUrl = params.get('returnUrl') || '/';
    navigate(returnUrl);
  };

  return (
    <form
      className="p-6 md:p-8 flex flex-col"
      onSubmit={handleSubmit(onSubmit)}
    >
      <FieldGroup>
        <FieldGroup>
          <div className="flex flex-col items-center gap-2 text-center">
            <AxiomLogo />

            <h1 className="text-lg md:text-2xl font-bold">
              {t('AxiomAccount:GetStartedWithAxiom')}
            </h1>
            <p>{t('AxiomAccount:SignInOrCreateAccount')}</p>
          </div>

          <Field>
            <FieldLabel htmlFor="usernameOrEmail">
              {t('AxiomAccount:EmailOrUsername')}
            </FieldLabel>
            <InputGroup>
              <InputGroupAddon>
                <MailIcon />
              </InputGroupAddon>

              <InputGroupInput
                {...register('usernameOrEmail')}
                id="usernameOrEmail"
                name="usernameOrEmail"
                type="text"
                placeholder={t('AxiomAccount:EmailOrUsername')}
              />
            </InputGroup>
            <p>{errors.usernameOrEmail?.message}</p>
          </Field>

          <Field>
            <FieldLabel htmlFor="password">
              {t('AxiomAccount:Password')}
            </FieldLabel>

            <InputGroup>
              <InputGroupAddon>
                <LockIcon />
              </InputGroupAddon>

              <InputGroupInput
                {...register('password')}
                id="password"
                name="password"
                type={showPassword ? 'text' : 'password'}
                placeholder={t('AxiomAccount:Password')}
              />

              <InputGroupAddon align="inline-end">
                <button
                  type="button"
                  className="text-muted-foreground transition-colors hover:text-foreground"
                  onClick={() => setShowPassword((value) => !value)}
                  aria-label={showPassword ? 'Hide password' : 'Show password'}
                >
                  {showPassword ? <EyeOffIcon /> : <EyeIcon />}
                </button>
              </InputGroupAddon>
            </InputGroup>
            <p>{errors.password?.message}</p>
          </Field>

          <Field>
            <Button className="h-10" type="submit">
              {t('AxiomAccount:SignIn')}
            </Button>
          </Field>

          <FieldSeparator>{t('AxiomAccount:OrContinueWith')}</FieldSeparator>

          <Field className="grid grid-cols-4 gap-4">
            <Button variant="outline" type="button">
              <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
                <path
                  d="M12.48 10.92v3.28h7.84c-.24 1.84-.853 3.187-1.787 4.133-1.147 1.147-2.933 2.4-6.053 2.4-4.827 0-8.6-3.893-8.6-8.72s3.773-8.72 8.6-8.72c2.6 0 4.507 1.027 5.907 2.347l2.307-2.307C18.747 1.44 16.133 0 12.48 0 5.867 0 .307 5.387.307 12s5.56 12 12.173 12c3.573 0 6.267-1.173 8.373-3.36 2.16-2.16 2.84-5.213 2.84-7.667 0-.76-.053-1.467-.173-2.053H12.48z"
                  fill="currentColor"
                />
              </svg>
              <span className="sr-only">Login with Google</span>
            </Button>
            <Button variant="outline" type="button">
              <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
                <path
                  d="M12.152 6.896c-.948 0-2.415-1.078-3.96-1.04-2.04.027-3.91 1.183-4.961 3.014-2.117 3.675-.546 9.103 1.519 12.09 1.013 1.454 2.208 3.09 3.792 3.039 1.52-.065 2.09-.987 3.935-.987 1.831 0 2.35.987 3.96.948 1.637-.026 2.676-1.48 3.676-2.948 1.156-1.688 1.636-3.325 1.662-3.415-.039-.013-3.182-1.221-3.22-4.857-.026-3.04 2.48-4.494 2.597-4.559-1.429-2.09-3.623-2.324-4.39-2.376-2-.156-3.675 1.09-4.61 1.09zM15.53 3.83c.843-1.012 1.4-2.427 1.245-3.83-1.207.052-2.662.805-3.532 1.818-.78.896-1.454 2.338-1.273 3.714 1.338.104 2.715-.688 3.559-1.701"
                  fill="currentColor"
                />
              </svg>
              <span className="sr-only">Login with Apple</span>
            </Button>
            <Button variant="outline" type="button">
              <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
                <path
                  d="M12 .297c-6.63 0-12 5.373-12 12 0 5.303 3.438 9.8 8.205 11.385.6.113.82-.258.82-.577 0-.285-.01-1.04-.015-2.04-3.338.724-4.042-1.61-4.042-1.61-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.084-.729.084-.729 1.205.084 1.838 1.237 1.838 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.605-2.665-.303-5.466-1.332-5.466-5.93 0-1.31.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23a11.51 11.51 0 0 1 3.003-.404c1.02.005 2.047.138 3.006.404 2.29-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.61-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222 0 1.604-.014 2.896-.014 3.286 0 .322.216.696.825.577C20.565 22.092 24 17.592 24 12.297c0-6.627-5.373-12-12-12"
                  fill="currentColor"
                />
              </svg>
              <span className="sr-only">Login with Meta</span>
            </Button>
            <Button variant="outline" type="button">
              <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
                <path
                  d="M6.915 4.03c-1.968 0-3.683 1.28-4.871 3.113C.704 9.208 0 11.883 0 14.449c0 .706.07 1.369.21 1.973a6.624 6.624 0 0 0 .265.86 5.297 5.297 0 0 0 .371.761c.696 1.159 1.818 1.927 3.593 1.927 1.497 0 2.633-.671 3.965-2.444.76-1.012 1.144-1.626 2.663-4.32l.756-1.339.186-.325c.061.1.121.196.183.3l2.152 3.595c.724 1.21 1.665 2.556 2.47 3.314 1.046.987 1.992 1.22 3.06 1.22 1.075 0 1.876-.355 2.455-.843a3.743 3.743 0 0 0 .81-.973c.542-.939.861-2.127.861-3.745 0-2.72-.681-5.357-2.084-7.45-1.282-1.912-2.957-2.93-4.716-2.93-1.047 0-2.088.467-3.053 1.308-.652.57-1.257 1.29-1.82 2.05-.69-.875-1.335-1.547-1.958-2.056-1.182-.966-2.315-1.303-3.454-1.303zm10.16 2.053c1.147 0 2.188.758 2.992 1.999 1.132 1.748 1.647 4.195 1.647 6.4 0 1.548-.368 2.9-1.839 2.9-.58 0-1.027-.23-1.664-1.004-.496-.601-1.343-1.878-2.832-4.358l-.617-1.028a44.908 44.908 0 0 0-1.255-1.98c.07-.109.141-.224.211-.327 1.12-1.667 2.118-2.602 3.358-2.602zm-10.201.553c1.265 0 2.058.791 2.675 1.446.307.327.737.871 1.234 1.579l-1.02 1.566c-.757 1.163-1.882 3.017-2.837 4.338-1.191 1.649-1.81 1.817-2.486 1.817-.524 0-1.038-.237-1.383-.794-.263-.426-.464-1.13-.464-2.046 0-2.221.63-4.535 1.66-6.088.454-.687.964-1.226 1.533-1.533a2.264 2.264 0 0 1 1.088-.285z"
                  fill="currentColor"
                />
              </svg>
              <span className="sr-only">Login with Meta</span>
            </Button>
          </Field>
        </FieldGroup>
      </FieldGroup>
    </form>
  );
}

function AxiomLogo() {
  return (
    <div className="relative flex size-14 items-center justify-center">
      <svg
        width="500"
        height="500"
        viewBox="0 0 500 500"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
      >
        <path
          d="M382.734 214.592C388.108 211.286 395.646 213.425 396.918 219.606C398.939 229.421 400 239.587 400 250C400 332.843 332.843 400 250 400C239.587 400 229.421 398.939 219.606 396.918C213.425 395.646 211.286 388.108 214.592 382.734C216.718 379.278 220.77 377.349 224.751 378.13C232.92 379.731 241.362 380.572 250 380.572C322.112 380.572 380.572 322.112 380.572 250C380.572 241.362 379.731 232.92 378.13 224.751C377.349 220.77 379.278 216.718 382.734 214.592Z"
          fill="url(#paint0_linear_14_444)"
        />
        <path
          d="M250 100C260.413 100 270.578 101.061 280.393 103.081C286.574 104.353 288.713 111.89 285.406 117.265C283.281 120.72 279.229 122.649 275.248 121.869C267.079 120.268 258.638 119.428 250 119.428C177.887 119.428 119.428 177.887 119.428 250C119.428 258.638 120.268 267.079 121.869 275.248C122.649 279.229 120.72 283.281 117.265 285.406C111.89 288.713 104.353 286.574 103.081 280.393C101.061 270.578 100 260.413 100 250C100 167.157 167.157 100 250 100Z"
          fill="url(#paint1_linear_14_444)"
        />
        <path
          d="M309.422 115.05L345.845 151.759L395.848 138.571L359.425 101.861L309.422 115.05Z"
          fill="#77C8FF"
        />
        <path
          d="M345.845 151.759L332.225 201.805L382.228 188.617L395.848 138.571L345.845 151.759Z"
          fill="#0096FC"
        />
        <path
          d="M345.845 151.759L309.422 115.05L295.802 165.096L332.225 201.805L345.845 151.759Z"
          fill="#007CEC"
        />
        <path
          d="M102.762 334.956L153.799 344.033L187.178 304.373L136.142 295.296L102.762 334.956Z"
          fill="#FF73AF"
        />
        <path
          d="M153.799 344.033L171.508 392.915L204.887 353.255L187.178 304.373L153.799 344.033Z"
          fill="#FF318E"
        />
        <path
          d="M153.799 344.033L102.762 334.956L120.472 383.838L171.508 392.915L153.799 344.033Z"
          fill="#C30A5B"
        />
        <defs>
          <linearGradient
            id="paint0_linear_14_444"
            x1="400.054"
            y1="219.889"
            x2="226.576"
            y2="393.885"
            gradientUnits="userSpaceOnUse"
          >
            <stop stopColor="#0096FC" />
            <stop offset="1" stopColor="#FF318E" />
          </linearGradient>
          <linearGradient
            id="paint1_linear_14_444"
            x1="287.008"
            y1="106.844"
            x2="113.531"
            y2="280.839"
            gradientUnits="userSpaceOnUse"
          >
            <stop stopColor="#0096FC" />
            <stop offset="1" stopColor="#FF318E" />
          </linearGradient>
        </defs>
      </svg>
    </div>
  );
}
