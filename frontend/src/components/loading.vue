<template>
    <div v-if="store.stravaEmpty == false" class="d-flex flex-column align-center" style="min-height:120px;margin-top:30px">
        <p>Aktivity se momentálně stále zpracovávají. Stránka se obnoví, jakmile budou připraveny. Toto může trvat několik minut.</p>
        <v-spacer/>
        <v-progress-circular
            :size="60"
            :model-value="progress"
            color="deep-orange"
            :indeterminate="indeterminate"
            >
            <div v-if="!indeterminate">
            {{progress}}%
            </div>
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
  progress: Number,
  indeterminate: Boolean
});
</script>