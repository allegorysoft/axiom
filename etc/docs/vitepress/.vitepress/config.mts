import { defineConfig } from 'vitepress'

// https://vitepress.dev/reference/site-config
export default defineConfig({
  srcDir: "../en",

  title: "Axiom Documentation",
  description: "A modular, extensible .NET application framework",
  themeConfig: {
    // https://vitepress.dev/reference/default-theme-config
    nav: [
      { text: 'Home', link: '/' },
      { text: 'Documentation', link: '/get-started/overview' }
    ],

    sidebar: [
      {
        text: 'Get Started',
        items: [
          { text: 'Overview', link: '/get-started/overview' },
          { text: 'Installation', link: '/get-started/installation' },
        ]
      },
      {
        text: 'Concepts',
        collapsed: false,
        items: [
          { text: 'Dependency Injection', link: '/concepts/dependency-injection' },
          {
            text: 'Modularity',
            collapsed: true,
            items: [
              { text: 'Overview', link: '/concepts/modularity/overview' },
              { text: 'Application Options', link: '/concepts/modularity/application-options' },
              { text: 'Plugins', link: '/concepts/modularity/plugins' },
            ]
          },
          { text: 'Interception', link: '/concepts/interception' },
        ]
      }
    ],

    socialLinks: [
      { icon: 'github', link: 'https://github.com/allegorysoft/axiom' }
    ]
  }
})
