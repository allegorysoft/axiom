import type { Provider } from '../models/common';
import type { Translations } from '../models/localization';
import { provideInitializers } from '../initializer/registry';
import { seed } from '../utils/localization-utils';
import { jsonFileLocalizationProvider } from '../providers/localization-providers';
import { localizerStore } from '../store/localizer-store';

type LocalizationOptions = {
  readonly providers?: Provider<Translations>[];
};

export function configureLocalization(options?: LocalizationOptions) {
  provideInitializers({
    configure: async () => {
      await seed(
        [
          jsonFileLocalizationProvider({ fileNameOrPath: `i18n/en` }),
          ...(options?.providers ?? []),
        ],
        localizerStore,
      );
    },
  });
}
