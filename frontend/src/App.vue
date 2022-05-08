<template>
  <v-app fill-height>
    <v-snackbar
      v-if="store.requestFailed == true"
      multi-line
      v-model="store.requestFailed"
      :timeout="5000"
      tile
      color="red accent-2"
      top
      vertical
    >
      Nastala chyba připojení při zpracování požadavku. <br> Opakuji za 10 vteřin.
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
import {watch} from 'vue';
watch(()=>store.isLoggedIn, newValue => {
  if(newValue == true){
    isFullyLoadedRequest()
    store.fullyLoadedRequestID = setInterval(isFullyLoadedRequest, 10000);
  }
})


function isFullyLoadedRequest(){
  UserService.getCenteredActivites(false).then(result=>{
    if(result.status == 202){
      store.fullyLoaded = false;
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
}
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
