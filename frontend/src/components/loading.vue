<template>
    <div v-if="empty == false" class="d-flex flex-column align-center" style="min-height:120px;margin-top:30px">
        <p>Aktivity se momentálně stále zpracovávají. Stránka se obnoví, jakmile budou připraveny. Toto může trvat několik minut.</p>
        <v-spacer/>
        <v-progress-circular
            :size="60"
            :model-value="progress"
            color="deep-orange"
            >
            {{progress}}%
        </v-progress-circular>
    </div>
    <div v-else class="d-flex flex-column align-center" style="min-height:120px;margin-top:30px">
        <p>Aplikace neobsahuje žádné aktivity. Zvažte reset účtu</p>
    </div>
</template>

<script  lang="ts" setup>

import {useMainStore} from "@/store/mainstore"
import {ref} from 'vue';

const store = useMainStore()
const props = defineProps({
  progress: String,
});
const empty = ref(false)
empty.value = store.stravaEmpty
</script>