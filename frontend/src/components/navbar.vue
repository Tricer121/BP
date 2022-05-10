<template>
  <v-app-bar  density="compact">
    <v-row id="appbar" class="align-center">
        <v-app-bar-title v-if="!xs" class="d-flex flex-row" color="deep-orange">
          <v-btn id="namebtn" size="small" variant="outlined" to="/">
            <div class="text-h6">Fit</div>
            <div class="text-h6">map</div>
            </v-btn>
          </v-app-bar-title>
        <v-btn v-if="xs" icon="mdi-menu" style="margin:0; padding:0" color="deep-orange" @click="leftBar = !leftBar"/>
        <v-spacer/>
          <v-menu>
            <template v-slot:activator="{ props }">
              <v-btn id="userbtn"  class="small" color="deep-orange" v-bind="props">
                <v-icon>mdi-account</v-icon>
                <div v-if="!xs"><v-spacer/>{{ store.userName }}</div>
                <v-tooltip 
                activator="parent"
                anchor="bottom">
                Akce účtu
              </v-tooltip>
              </v-btn>
            </template>
            <v-list>
              <v-list-item>
                <v-btn variant="outlined" color="error" @click="resetAccPrompt=true"
                :disabled="!store.fullyLoaded">
                          Resetovat účet  
                      </v-btn>
              </v-list-item>
              <v-spacer/>
              <v-list-item>
                <v-btn variant="outlined" color="error" @click="deleteAccPrompt=true">
                  Smazat účet
                </v-btn>
              </v-list-item>
            </v-list>
          </v-menu>
          <v-btn
            color="deep-orange"
            class="btnmargin"
            icon
            
            @click="loadNewActivities()"
          >
            <v-icon>mdi-download-circle</v-icon>
            <v-tooltip 
              activator="parent"
              anchor="bottom">
              Aktualizovat aktivity
            </v-tooltip>
          </v-btn>
      
          <v-btn
            class="btnmargin"
            icon
            @click="signOutClick()"
            >
            <v-icon color="error" >mdi-logout-variant</v-icon>
            <v-tooltip 
              activator="parent"
              anchor="bottom">
              Odhlásit
            </v-tooltip>
          </v-btn>
    </v-row>
  </v-app-bar>

  <v-navigation-drawer v-model="leftBar" :rail="!xs" :expand-on-hover="!xs" :permanent="!xs">
    <v-list density="default">
      <v-list-item to="/" prepend-icon="mdi-format-list-text" title="Seznam aktivit" />
      <v-list-item
        to="/route"
        prepend-icon="mdi-gesture"
        title="Filtrované trasy"
      />
      <v-list-item
        to="/regions"
        prepend-icon="mdi-timeline"
        title="Zabrané území"
        value="starred"
      />
    </v-list>
  </v-navigation-drawer>
  <v-dialog v-model="signOutPrompt"
    transition="dialog-bottom-transition"
    min-height="100px"
    :persistent="inProgress"
  >
    <v-card>
      <v-toolbar color="#3b444b">Doopravdy se chcete odhlásit? </v-toolbar>
      <v-card-actions class="d-flex justify-center">
          <div v-if="inProgress">
            <v-progress-circular indeterminate color="white" />
          </div>
          <v-container v-else>
            <v-row>
              <v-col class="d-flex justify-center">
                <v-btn id="deleteAcc" class="ma-2" color="error" @click="signOut()">
                    <v-icon>mdi-logout-variant</v-icon>
                    Odhlásit
                </v-btn>
              </v-col>
            <v-col  class="d-flex justify-center">
              <v-btn class="ma-2" color="primary" @click="signOutPrompt=false">
                  Storno
              </v-btn>
            </v-col>
            </v-row>
          </v-container>
      </v-card-actions>
    </v-card>
  </v-dialog>
  <v-dialog v-model="deleteAccPrompt" :persistent="inProgress" style="padding: 30px;">
    <v-card
    title="Potvrdit smazání"
    subtitle="Doopravdy chcete smazat svůj účet?"
    >
    <v-card-text>Následující akce je nezvratná, do aplikace se lze znovu přihlásit po autorizaci skrze Stravu, ale všechny data budou muset být znovu načtena a zpracována.</v-card-text>
    <v-card-actions class="d-flex justify-center">
          <div v-if="inProgress">
            <v-progress-circular indeterminate color="white" />
          </div>
          <v-container v-else>
            <v-row>
              <v-col class="d-flex justify-center">
                <v-btn id="deleteAcc" class="ma-2" color="error" @click="deleteAccount()">
                    Smazat účet
                </v-btn>
              </v-col>
            <v-col  class="d-flex justify-center">
              <v-btn class="ma-2" color="primary" @click="deleteAccPrompt=false">
                  Storno
              </v-btn>
            </v-col>
            </v-row>
          </v-container>
      </v-card-actions>
    </v-card>
  </v-dialog>
  <v-dialog v-model="resetAccPrompt" :persistent="inProgress">
    <v-card 
    style="margin: 30px;"
    title="Potvrdit reset účtu"
    >
    <v-card-text> Následující akce je nezvratná, všechny data budou znovu načtena a zpracována ze Stravy. <br>
    Tato akce bude trvat několik minut. </v-card-text>
      <v-card-actions class="d-flex justify-center">
          <div v-if="inProgress">
            <v-progress-circular indeterminate color="white" />
          </div>
          <v-container v-else>
            <v-row>
              <v-col class="d-flex justify-center">
                <v-btn id="deleteAcc" class="ma-2" color="error" @click="resetAccount()">
                    Resetovat účet
                </v-btn>
              </v-col>
              <v-col class="d-flex justify-center">
                <v-btn class="ma-2" color="primary" @click="resetAccPrompt=false">
                    Storno
                </v-btn>
              </v-col>
            </v-row>
          </v-container>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script lang="ts" setup>
import router from "@/router";
import UserService from "@/services/UserService";
import { useMainStore } from "@/store/mainstore";
import { ref, watch } from "vue";
import {useDisplay} from 'vuetify';

const signOutPrompt = ref(false);
const resetAccPrompt = ref(false);
const deleteAccPrompt = ref(false);
const leftBar = ref(true);
const inProgress = ref(false);
const { xs, mdAndUp } = useDisplay()
if(xs.value == true)
  leftBar.value=false;

const store = useMainStore();

function signOutClick() {
  signOutPrompt.value = true;
}
function signOut() {
  signOutPrompt.value = false;
  store.isLoggedIn = false;
  router.push("/auth");
  router.go(0);
  UserService.logout()
    .then(() => {
      store.requestSuccess = true;
      store.successMessage = "Úspěšně odhlášen."
      store.$reset()
    })
    .catch(function (err) {
      
    });
}
function deleteAccount(){
    inProgress.value = true;
    UserService.deleteUserAccount()
    .then((result)=>{
      if(result.status == 401){
          store.error401 = true;
          setTimeout( function() {store.$reset()}, 1500);
          return;
      }
      store.requestSuccess = true;
      store.successMessage = "Účet byl smazán."
      deleteAccPrompt.value = false;
      setTimeout(function(){
        store.$reset();
        router.push("/auth");
      }, 10);
    }).catch(function(err){
      store.requestFailed=true;
      });
}
function resetAccount(){
    inProgress.value = true;
    UserService.resetUserAccount()
    .then((result)=>{
      if(result.status == 401){
        store.error401 = true;
        setTimeout( function() {store.$reset()}, 1500);
        return;
      }
      store.requestSuccess = true;
      store.successMessage = "Účet byl resetován. Aktivity budou znovu přepočítany."
      resetAccPrompt.value = false;
      store.stravaEmpty = false;
      inProgress.value = false;
      console.log(store.fullyLoadedRequestID)
      if(store.fullyLoadedRequestID != 0){
        window.clearInterval(store.fullyLoadedRequestID)
        store.fullyLoadedRequestID = 0;
      }
      store.fullyLoaded = false;
      setTimeout(function(){
        router.push("/");
        router.go(0);
      }, 300);
    }).catch(function (err){
      store.request});
}
function loadNewActivities(){
  UserService.loadNewActsFromStrava().then(x=>{
    if(x.status == 401){
      store.error401 = true;
      setTimeout( function() {store.$reset(); router.push('/');router.go(0)}, 1500);
      return;
    }
    store.successMessage = `Nalezeno ${x.data} nových aktivit`;
    store.requestSuccess = true;
    if(x.data > 0){
      store.stravaEmpty = false;
    }
    router.go(0);
  }).catch(function (err){
      store.requestFailed=true;
    });
}
watch(() => xs.value,newValue=>{
    leftBar.value = mdAndUp.value
    
})
</script>

<style scoped>
#namebtn{
  width:100px;
  padding:0;
  margin:0;
  margin-left:40px
}
.btnmargin {
  margin-left: 10px;
}
#appbar{
  padding:0 50px;
}
@media (max-width: 600px) {
  #userbtn{
    margin: 0;
    padding:0;
  }
  #appbar{
    padding:0px;
    margin:0px;
  }
}
@media (max-width: 670px) {
  #namebtn{
    margin: 0;
    padding:0;
  }
}
</style>
