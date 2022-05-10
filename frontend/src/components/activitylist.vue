<template>
  <v-container id="listBody" class="align-center" style="height: 100%">
    <v-row>
        <v-col v-if="ready == false" class="d-flex justify-center">
          <LoadingComponent :indeterminate="true" />
        </v-col> 
    </v-row>
    <v-row>
        <v-col class="d-flex justify-center">
        <v-expansion-panels v-if="ready" style="width: 75%">
          <v-expansion-panel
            v-for="(item,index) in activities[currentPage - 1]"
            :key="index"
            class="space-out"
            :id="item.id"
            @click="scroll(item.id)"
          >
            <v-expansion-panel-title expand-icon="mdi-menu-down">
              <v-container class="d-flex flex-row justify-center align-center">
                <v-row>
                  <v-col class="align-center">
                    <span style="text-h5">{{ item.name }}</span>
                  </v-col>
                  <v-col>
                    <span style="text-h5">{{ item.startDate }}</span>
                  </v-col>
                <v-col class="justify-end text-right">
                  <v-btn 
                  @click.native.stop 
                  @click="deleteDialogFc(item.id)"
                  style="width:10px;"
                  :disabled="store.fullyLoaded == false"
                  > 
                    <v-icon>mdi-delete-circle</v-icon>
                    <v-tooltip
                      activator="parent"
                      anchor="bottom">
                        Smazat aktivitu
                    </v-tooltip>
                  </v-btn>
                </v-col>
                </v-row>
              </v-container>
            </v-expansion-panel-title>
            <v-expansion-panel-text>
              <ActivityVue :id="item.id" :map="true" />
            </v-expansion-panel-text>
          </v-expansion-panel>
        </v-expansion-panels>
      </v-col>
    </v-row>
    <v-row>
      <v-col class="d-flex flex-row justify-center">
        <v-pagination
          v-if="activityBar"
          v-model="currentPage"
          :length="activityPages"
          show-first-last-page
        ></v-pagination>
        <div v-if="ready" style="width:10px;margin-left:30px">
          <v-select
            :items="perPageOptions"
            v-model="activityPerPage"
            outlined
            dense
          ></v-select>
          <v-tooltip 
              activator="parent"
              anchor="bottom">
              Množství aktivit na stránce
          </v-tooltip>
        </div>
      </v-col>
    </v-row>
    <v-dialog v-model="deleteDialog" >
        <DeleteDialog :id="activeId" @close="deleteDialog = false"/>
    </v-dialog>
  </v-container>
</template>

<script lang="ts" setup>
import type Activity from "@/models/activity";
import ActivityList from "@/models/activityList";
import UserService from "@/services/UserService";
import ActivityVue from "@/components/activity.vue";
import DeleteDialog from '@/components/deleteDialog.vue';
import LoadingComponent from '@/components/loading.vue';
import router from "@/router";

import { useMainStore } from "@/store/mainstore";
import { computed, ref, watch} from "vue";

const store = useMainStore();
const ready = ref(false);
const activityPerPage = ref(5);
const currentPage = ref(1);
const perPageOptions = [2,3,4,5,6,7,10,15,20,50];
const activityPages = ref(1);
const deleteDialog = ref(false);
const activeId = ref(-1);
const activities = ref<Activity[][]>();
const activityBar = computed(() => {
  return activityPages.value > 1 ? true : false
});
if(store.centeredRequest !=0)
  window.clearInterval(store.centeredRequest);
if(store.averagedRequest !=0)
  window.clearInterval(store.averagedRequest);
setTimeout( function() {
  request();
}, 500);
let myInterval = setInterval(request, 5000);
function scroll(id:number){
  setTimeout(function(){
    let element = document.getElementById(`${id}`)
    element?.scrollIntoView({behavior:"smooth"})
  },350)
}
watch(()=>activityPerPage.value,(newValue)=>{
  request()
})
function request(){ UserService.getActivities(activityPerPage.value)
  .then((x) => {
    clearInterval(myInterval);
    activities.value = [];
    if(x.data.length == 0){
        store.stravaEmpty = true;
        ready.value = false;
        return;
    }
    store.stravaEmpty = false;
    let activity: ActivityList = new ActivityList(
      x.data.pages,
      x.data.perPage,
      x.data.activities
    );
    

    if(currentPage.value > activity.pages)
      currentPage.value = activity.pages
    activityPages.value = activity.pages;

    activities.value = activity.activities;

    var colorArray = 
    ['#ff4900','#5303ff','#B33300','#ffd503','#030bff',
    '#9aff03','#03a7ff','#53ff03','#03ff74','#e61010',
    '#03ffc0','#03fffb','#0346ff',
    '#8103ff','#c803ff','#ff03dd','#ff0374'];
    activities.value.forEach(
      x=>x.forEach(
        function (y,index){
          if(!store.colorExists(y.id))
            store.addColor({id:y.id,color:colorArray[index%colorArray.length]})
        }
    ));
    ready.value = true;
    
  })
  .catch(function (err) {
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
    store.requestFailed = true;
    clearInterval(myInterval);
    myInterval = setInterval(function(){
      request()
    }, 10000);
  });
}

function deleteDialogFc(id:number){
  activeId.value = id;
  deleteDialog.value = true;
}

</script>

<style scoped>
.space-out {
  margin: 10px 0;
}
</style>
