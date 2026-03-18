<template>
  <nav v-if="breadcrumbs.length > 1" class="breadcrumb" aria-label="Breadcrumb">
    <ol>
      <li v-for="(crumb, index) in breadcrumbs" :key="index">
        <span v-if="index < breadcrumbs.length - 1">
          <a :href="crumb.link">{{ crumb.text }}</a>
          <span class="separator">/</span>
        </span>
        <span v-else class="current">{{ crumb.text }}</span>
      </li>
    </ol>
  </nav>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useData, useRoute } from 'vitepress'

const { theme } = useData()
const route = useRoute()

function flattenSidebar(items: any[], parents: { text: string; link?: string }[] = []): { path: string; crumbs: { text: string; link?: string }[] }[] {
  const result: { path: string; crumbs: { text: string; link?: string }[] }[] = []
  for (const item of items) {
    const current = { text: item.text, link: item.link }
    if (item.link) {
      result.push({ path: item.link, crumbs: [...parents, current] })
    }
    if (item.items) {
      result.push(...flattenSidebar(item.items, item.link ? [...parents, current] : [...parents, { text: item.text }]))
    }
  }
  return result
}

const breadcrumbs = computed(() => {
  const sidebar = theme.value.sidebar ?? []
  const allItems = Array.isArray(sidebar) ? sidebar : Object.values(sidebar).flat()
  const flat = flattenSidebar(allItems)

  const currentPath = route.path.replace(/\.html$/, '').replace(/\/$/, '')
  const match = flat.find(item => {
    const itemPath = item.path.replace(/\.html$/, '').replace(/\/$/, '')
    return currentPath.endsWith(itemPath)
  })

  return match?.crumbs ?? []
})
</script>

<style scoped>
.breadcrumb {
  margin-bottom: 24px;
  font-size: 14px;
}

.breadcrumb ol {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 4px;
  list-style: none;
  padding: 0;
  margin: 0;
}

.breadcrumb li {
  display: flex;
  align-items: center;
  gap: 4px;
}

.breadcrumb a {
  color: var(--vp-c-brand-1);
  text-decoration: none;
}

.breadcrumb a:hover {
  text-decoration: underline;
}

.separator {
  color: var(--vp-c-text-3);
}

.current {
  color: var(--vp-c-text-2);
}
</style>
