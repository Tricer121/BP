<template>
  <Navbar />
  <v-main style="background: #111414; min-width:290px">
    <CenteredActivities />
  </v-main>
</template>

<script lang="ts" setup>
// Components
import CenteredActivities from "@/components/centeredActivities.vue";
import Navbar from "@/components/navbar.vue";
</script>