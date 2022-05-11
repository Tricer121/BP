<template>
    <v-card>
        <v-toolbar color="#3b444b">Doopravdy chcete aktivitu smazat? </v-toolbar>
        <p style="margin:15px">Pokud budete tuto aktivitu v budoucnu chtít znovu vidět, je nutno využít reset účtu.</p>
        <v-card-actions class="d-flex justify-center">
        <div v-if="inProgress">
        <v-progress-circular indeterminate color="white" />
        </div>
        <v-container v-else>
        <v-row>
            <v-col class="d-flex justify-center">
            <v-btn id="deleteAcc" class="ma-2" color="error" @click="deleteById()">
                <v-icon>mdi-delete-circle</v-icon>
                Smazat
            </v-btn>
            </v-col>
            <v-col class="d-flex justify-center">
            <v-btn class="ma-2" color="primary" @click="$emit('close')">
                Storno
            </v-btn>
        </v-col>
        </v-row>
        </v-container>
      </v-card-actions>
    </v-card>
</template>
<script setup>
import {useMainStore} from "@/store/mainstore";
import UserService from '@/services/UserService';
import router from '@/router'
const store = useMainStore();
import {ref} from 'vue'
const props = defineProps({
  id: Number,
});
const emit = defineEmits(['close'])
const inProgress = ref(false)
function deleteById(){
    inProgress.value = true;
    UserService.deleteActivityById(props.id).then((result)=>{
        store.successMessage = "Aktivita smazána";
        store.requestSuccess = true;
        store.fullyLoaded = false;
        emit('close');
    }).catch(function (err){
      if(err.response)
        if(err.response.status == 401){
          store.error401 = true;
          store.requestFailed = true;
          setTimeout( function() {
              document.cookie = 'auth_cookie=; Path=/; Expires=Thu, 01 Jan 1970 00:00:01 GMT;'
              store.$reset(); router.push('/');router.go(0)
          }, 3500);
          return;
        }
      store.requestFailed=true;});
}
</script>