---
layout: page
---
<script setup>
import { onMounted } from 'vue'
import { useRouter, withBase } from 'vitepress'

const router = useRouter()

onMounted(() => {
  router.go(withBase('/get-started/overview'))
})
</script>