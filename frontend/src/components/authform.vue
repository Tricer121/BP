<template>
  <v-container
    style="height: 100vh"
    class="d-flex align-center justify-center"
    fill-height
  >
    <v-card hover class="align-center justify-center">
      <v-card-title elevation="20" class="justify-center" primary-title>
        Přihlásit pomocí Strava
      </v-card-title>
      <v-card-text>
        Aplikace používá výhradně Stravu pro přečtení uživatelských dat.<br />
        Je nutné autorizovat čtení různých prvků profilu pro správnou funkci
        aplikace.
      </v-card-text>
      <v-card-actions class="justify-center">
        <v-btn
          v-if="requestMade == false && isLoading == false"
          elevation="4"
          color="deep-orange"
          @click="authClick()"
          >Přihlásit</v-btn
        >
        <v-progress-circular v-else indeterminate color="white" />
      </v-card-actions>
    </v-card>
  </v-container>
</template>

<script lang="ts" setup>
import { useMainStore } from "@/store/mainstore";
import UserService from "@/services/UserService";
import { toRef, ref } from "vue";

const props = defineProps(["isLoading"]);
const store = useMainStore();
const isLoading = toRef(props, "isLoading");
const failedLoad = ref(false);
const requestMade = ref(false);
const errorText = ref("");

const interval = ref(0);

function request(){
  UserService.authorize()
    .then((response) => {
      window.location.assign(response.data) 
      clearInterval(interval.value)})
    .catch(function (err) {
      store.requestFailed = true;
      requestMade.value = false;
    });
}
async function authClick() {
  requestMade.value = true;
  request();
  interval.value = setInterval(function(){
    request()
    }, 10000);
}
</script>

<style scoped>
@media only screen and (max-width: 1000px) {
  .v-card {
    width: 50%;
  }
}
@media only screen and (max-width: 600px) {
  .v-card {
    width: 80%;
  }
}
@media only screen and (max-width: 300px) {
  .v-card {
    width: 80%;
  }
}
</style>
