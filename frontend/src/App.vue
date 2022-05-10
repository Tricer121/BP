<template>
  <v-app fill-height>
    <v-snackbar
      v-if="errorHappened == true"
      multi-line
      v-model="errorHappened"
      :timeout="9000"
      tile
      color="red accent-2"
      top
      vertical
    >
      <div v-if="store.requestFailed">
      Nastala chyba připojení při zpracování požadavku. <br> Opakuji za 10 vteřin.
      </div>
      <div v-else>
      Nastala chyba přihlášení. <br> Je nutné se znovu přihlásit.
      </div>
      <template v-slot:actions>
        <v-btn
          color="black"
          variant="text"
          @click="store.requestFailed = false"
        >
          Zavřít
        </v-btn>
      </template>
    </v-snackbar>
    <v-snackbar
      multi-line
      v-model="store.requestSuccess"
      :timeout="6000"
      tile
      color="green accent-2"
      top
    >
      {{ store.successMessage }}
      <template v-slot:actions>
        <v-btn
          color="black"
          variant="text"
          @click="store.requestSuccess = false"
        >
          Zavřít
        </v-btn>
      </template>
    </v-snackbar>
    <router-view />
  </v-app>
</template>

<script lang="ts" setup>
import { useMainStore } from "@/store/mainstore";
const store = useMainStore();
import UserService from '@/services/UserService.ts'
import {watch, computed} from 'vue';
import router from "@/router";

watch(()=>store.fullyLoaded, (newValue) => {
  if(newValue == false){
    isFullyLoadedRequest()
    store.fullyLoadedRequestID = window.setInterval(isFullyLoadedRequest, 10000);
  }
})
if(!store.fullyLoaded){
    isFullyLoadedRequest()
    store.fullyLoadedRequestID = window.setInterval(isFullyLoadedRequest, 10000);
}
function isFullyLoadedRequest(){
  UserService.getCenteredActivites(false).then(result=>{
    if(result.status == 202){
      store.fullyLoaded = false;
      return;
    }
    if(result.status == 401){
      store.error401 = true;
      setTimeout( function() {store.$reset(); router.push('/');router.go(0)}, 1500);
      return;
    }
    else{
      clearInterval(store.fullyLoadedRequestID);
      store.fullyLoaded = true;
      if(result.data.routes.length == 0){
          store.stravaEmpty = true;
          return;
      }
    }
  })
  .catch(function (err) {
    store.requestFailed=true;
  })
}
const errorHappened = computed(()=>{return store.error401 || store.requestFailed})
</script>

<style>
.v-application {
  height: 100vh;
  min-width: 290px;
}
html,body{
  min-width: 290px;
}
</style>
