<template>
  <Navbar />
  <v-main style="background: #111414">
    <ActivityList />
  </v-main>
</template>

<script lang="ts" setup>
// Components
import ActivityList from "@/components/activitylist.vue";
import Navbar from "@/components/navbar.vue";
</script>
