import { defineConfig } from 'vitepress'

export default defineConfig({
  srcDir: "../en",
  lastUpdated: true,
  cleanUrls: true,
  sitemap: {
    hostname: 'https://axiomframework.dev'
  },

  title: "Axiom Documentation",
  description: "A modular, extensible .NET application framework",
  head: [
    ['link', {  rel: 'icon', type: 'image/svg+xml', href: '/logo.svg' }]
  ],
  themeConfig: {
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
          { text: 'Unit of Work', link: '/concepts/unit-of-work' },
          { text: 'File Providers', link: '/concepts/file-providers' },
          { text: 'Localization', link: '/concepts/localization' },
        ]
      }
    ],

    search: {
      provider: 'local'
    },

    editLink: {
      pattern: 'https://github.com/allegorysoft/axiom/edit/main/etc/docs/en/:path',
      text: 'Edit this page on GitHub'
    },

    lastUpdated: {
      text: 'Last updated',
      formatOptions: {
        dateStyle: 'medium',
        timeStyle: 'short'
      }
    },

    socialLinks: [
      { icon: 'github', link: 'https://github.com/allegorysoft/axiom' }
    ]
  }
})
