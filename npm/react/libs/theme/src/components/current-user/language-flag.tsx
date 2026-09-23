type LanguageFlagProps = {
  language: string;
};

const STAR =
  '0,-1 .2245,-.309 .9511,-.309 .3633,.118 .5878,.809 0,.382 -.5878,.809 -.3633,.118 -.9511,-.309 -.2245,-.309';

export function LanguageFlag({ language }: LanguageFlagProps) {
  return (
    <svg
      viewBox="0 0 30 20"
      aria-hidden="true"
      className="h-4 w-6 shrink-0 overflow-hidden rounded-sm"
    >
      {language === 'en' && (
        <>
          <path fill="#fff" d="M0 0h30v20H0z" />
          {Array.from({ length: 7 }, (_, i) => (
            <path
              key={i}
              fill="#b22234"
              d={`M0 ${(i * 40) / 13}h30v${20 / 13}H0z`}
            />
          ))}
          <path fill="#3c3b6e" d="M0 0h12v10.77H0z" />
          {Array.from({ length: 9 }, (_, row) =>
            Array.from({ length: row % 2 ? 5 : 6 }, (_, col) => (
              <polygon
                key={`${row}-${col}`}
                points={STAR}
                fill="#fff"
                transform={`translate(${1 + col * 2 + (row % 2 ? 1 : 0)} ${1.1 + row * 1.07}) scale(.45)`}
              />
            )),
          )}
        </>
      )}
      {language === 'tr' && (
        <>
          <path fill="#e30a17" d="M0 0h30v20H0z" />
          <circle cx="11" cy="10" r="5" fill="#fff" />
          <circle cx="12.3" cy="10" r="4" fill="#e30a17" />
          <polygon
            points={STAR}
            fill="#fff"
            transform="translate(17.5 10) rotate(18) scale(2.5)"
          />
        </>
      )}
      {language === 'es' && (
        <>
          <path fill="#aa151b" d="M0 0h30v20H0z" />
          <path fill="#f1bf00" d="M0 5h30v10H0z" />
          <path fill="#aa151b" d="M8 8h4v4a2 2 0 0 1-4 0z" />
          <path fill="#f1bf00" d="M9 8h1v3H9zM10 11h1v2h-1z" />
          <path fill="#aa151b" d="M8 6h4v1H8z" />
        </>
      )}
      {language === 'zh' && (
        <>
          <path fill="#de2910" d="M0 0h30v20H0z" />
          <polygon
            points={STAR}
            fill="#ffde00"
            transform="translate(5 5) scale(3)"
          />
          {[
            [10, 2, -30],
            [12, 4, -60],
            [12, 7, -90],
            [10, 9, -120],
          ].map(([x, y, rotation]) => (
            <polygon
              key={y}
              points={STAR}
              fill="#ffde00"
              transform={`translate(${x} ${y}) rotate(${rotation})`}
            />
          ))}
        </>
      )}
      {language === 'de' && (
        <>
          <path fill="#000" d="M0 0h30v7H0z" />
          <path fill="#d00" d="M0 6.67h30v6.67H0z" />
          <path fill="#ffce00" d="M0 13.33h30V20H0z" />
        </>
      )}
      {language === 'fr' && (
        <>
          <path fill="#002395" d="M0 0h10v20H0z" />
          <path fill="#fff" d="M10 0h10v20H10z" />
          <path fill="#ed2939" d="M20 0h10v20H20z" />
        </>
      )}
      {language === 'ja' && (
        <>
          <path fill="#fff" d="M0 0h30v20H0z" />
          <circle cx="15" cy="10" r="6" fill="#bc002d" />
        </>
      )}
    </svg>
  );
}
