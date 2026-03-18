import DefaultTheme from 'vitepress/theme'
import { h } from 'vue'
import Breadcrumb from './Breadcrumb.vue'
import './custom.css'

export default {
  extends: DefaultTheme,
  Layout() {
    return h(DefaultTheme.Layout, null, {
      'doc-before': () => h(Breadcrumb),
    })
  },
}
