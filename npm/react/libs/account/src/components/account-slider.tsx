import { useEffect, useState } from 'react';
import { useTranslation } from '@axiomframework/react-core';
import { Card } from '@axiomframework/react-theme/components';
import slider1 from '../assets/slider1.png';
import slider2 from '../assets/slider2.png';
import slider3 from '../assets/slider3.png';
import slider4 from '../assets/slider4.png';
import './account-slider.css';

const images = [slider2, slider3, slider4, slider1];
const slideOrder = [2, 3, 4, 1];

export function AccountSlider() {
  const [{ active, cycle }, setSlide] = useState({ active: 0, cycle: 0 });
  const t = useTranslation();
  useEffect(() => {
    const timer = window.setTimeout(
      () =>
        setSlide(({ active, cycle }) => ({
          active: (active + 1) % images.length,
          cycle: cycle + 1,
        })),
      5000,
    );
    return () => window.clearTimeout(timer);
  }, [active, cycle]);

  return (
    <Card
      className="relative hidden min-h-[600px] flex-col justify-between overflow-hidden bg-muted/40 p-8 lg:flex"
      role="region"
      aria-roledescription="carousel"
      aria-label={t('AxiomAccount:AboutAxiom')}
    >
      <div
        aria-hidden="true"
        className="account-slider-background pointer-events-none absolute inset-0"
      />
      <div className="relative my-auto flex flex-col items-center gap-5 py-5 text-center">
        <img
          src={images[active]}
          alt=""
          className="aspect-square w-full max-w-[340px] object-contain"
          width={340}
          height={340}
        />
        <div className="min-h-36 space-y-3">
          <h2 className="text-xl font-semibold tracking-tight text-balance">
            {t(`AxiomAccount:Slide${slideOrder[active]}Title`)}
          </h2>
          <p className="text-sm leading-relaxed text-muted-foreground text-pretty">
            {t(`AxiomAccount:Slide${slideOrder[active]}Description`)}
          </p>
        </div>
      </div>
      <div
        className="relative flex gap-3"
        aria-label={t('AxiomAccount:Slides')}
      >
        {images.map((_, index) => (
          <button
            key={index}
            type="button"
            className="flex h-8 min-w-0 flex-1 cursor-pointer items-center bg-transparent p-0 outline-none focus-visible:ring-2 focus-visible:ring-ring"
            aria-label={`${t('AxiomAccount:Slide')} ${index + 1}`}
            aria-current={active === index ? 'true' : undefined}
            onClick={() =>
              setSlide(({ cycle }) => ({ active: index, cycle: cycle + 1 }))
            }
          >
            <span className="h-1.5 w-full overflow-hidden rounded-full bg-muted-foreground/30">
              <span
                key={cycle}
                className={`block h-full origin-left rounded-full bg-primary ${index === active ? 'account-slide-progress' : ''}`}
                style={{
                  transform: index < active ? 'scaleX(1)' : 'scaleX(0)',
                }}
              />
            </span>
          </button>
        ))}
      </div>
    </Card>
  );
}
