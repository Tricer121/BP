<template>
  <v-app fill-height>
    <v-snackbar
      multi-line
      v-model="store.requestFailed"
      :timeout="5000"
      tile
      color="red accent-2"
      top
      vertical
    >
      <div v-if="store.error401">
      Nastala chyba přihlášení. <br> Je nutné se znovu přihlásit.
      </div>
      <div v-else>
      Nastala chyba připojení při zpracování požadavku. <br> Opakuji za 10 vteřin.
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
watch(() => store.requestFailed.value,(newValue,oldValue)=>{
    if(newValue == false && oldValue == true){
        store.error401 = false;
    }
})
watch(()=>store.fullyLoaded, (newValue) => {
  setTimeout( function() {
    isFullyLoadedRequest()
    store.fullyLoadedRequestID = window.setInterval(isFullyLoadedRequest, 10000);
  }, 500);
})
watch(()=>store.isLoggedIn, (newValue) => {
  if(newValue == true){    
    setTimeout( function() {
      isFullyLoadedRequest()
      store.fullyLoadedRequestID = window.setInterval(isFullyLoadedRequest, 10000);
    }, 3000);
  }
})
if(!store.fullyLoaded && store.isLoggedIn){
    isFullyLoadedRequest()
    store.fullyLoadedRequestID = window.setInterval(isFullyLoadedRequest, 10000);
}
function isFullyLoadedRequest(){
  UserService.isLoaded().then(result=>{
    if(result.data == true){
      clearInterval(store.fullyLoadedRequestID);
      store.fullyLoaded = true;
    }
    else{
      store.fullyLoaded = false
    }
  })
  .catch(_=>{})
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
