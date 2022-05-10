<script lang="ts" setup>
// Components
import AppAuthForm from "@/components/authform.vue";
import router from "@/router";
import UserService from "@/services/UserService";
import { useMainStore } from "@/store/mainstore";
import { ref } from "vue";
import { useRoute } from "vue-router";

const route = useRoute();
const code = route.query.code?.toString();
const scope = route.query.scope?.toString();
const register = route.query.register?.toString();
const error = route.query.error?.toString();

const authFailed = ref(false);
const isLoading = ref(false);
const store = useMainStore();

if (error != undefined) {
  authFailed.value = true;
} else if (code != undefined && scope != undefined && register != undefined) {
  isLoading.value = true;
  const scopes = ['read_all','activity:read_all','profile:read_all']
  scopes.forEach(x=>{
      if(!scope.includes(x)){
        authFailed.value = true;
        isLoading.value = false;
      }
  })
  if(authFailed.value == false)
    UserService.register(code, scope)
      .then(() => {
        UserService.loadNewActsFromStrava().then((result)=>{
          if(result.status == 401){
            store.error401 = true;
            setTimeout( function() {store.$reset(); router.push('/');router.go(0)}, 1500);
            return;
          }
          store.isLoggedIn = true;
          UserService.getLoggedInUser()
            .then((x) => {
              store.userName = x.data.name;
              isLoading.value = false;
              router.replace("/");
            })
            .catch(function (err) {
              store.requestFailed = true;
            });
          });
      })
      .catch(function (err) {
        store.requestFailed = true;
      });
}
</script>

<style scoped></style>
<template>
   <v-snackbar
      v-if="authFailed == true"
      multi-line
      v-model="authFailed"
      :timeout="10000"
      tile
      color="red accent-2"
      top
      vertical
    >
      Nastala chyba při přidělování povolení. <br> Zkuste prosím znovu a povolte aplikaci veškerý přístup ke čtení.
      <template v-slot:actions>
        <v-btn
          color="black"
          variant="text"
          @click="authFailed = false"
        >
          Zavřít
        </v-btn>
      </template>
    </v-snackbar>
  <v-main>
    <AppAuthForm :isLoading="isLoading" />
  </v-main>
</template>
