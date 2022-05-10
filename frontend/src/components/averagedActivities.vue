<template> 
<v-container class="align-center" style="height: 100%">
  <v-row>
    <v-col v-if="ready == false" class="d-flex justify-center">
      <LoadingComponent :progress="processedActivities" :indeterminate="false" />
    </v-col>
    <v-col cols="0" md="1" class="d-flex justify-end">
      <div v-if="ready == true && store.stravaEmpty == false" >
        <RightMenu @mapRefresh="refresh()" @centerOnId="centerOnId" @reload="reload" :activities="activities" />
      </div>
    </v-col>
  </v-row>
  <v-row>
    <v-col cols="16">
      <ol-map v-if="ready && refreshNow" class="mapClass" ref="map">
      <ol-view
        ref="view"
        :center="center"
        :zoom="zoom"
        :projection="projection"
        @zoomChanged="zoomChanged"
        @centerChanged="centerChanged"
      />
      <ol-tile-layer>
        <ol-source-osm />
      </ol-tile-layer>
      <ol-vector-layer>
        <ol-source-vector>
          <div v-for="(item,index) in activities"
              :key="index"
              :value="item">
            <ol-feature>
              <ol-geom-line-string 
                :coordinates="item.rawRoute.coordinates"
              ></ol-geom-line-string>
              <ol-style>
                <ol-style-stroke
                  :color="store.getColorById(item.id)"
                  :width="strokeWidth"
                ></ol-style-stroke>
              </ol-style>
            </ol-feature>
          </div>
          <div v-if="coordinateHighlight">
              <div v-for="(item,index) in activities"
                    :key="index"
                    :value="item">
                <ol-feature>
                  <ol-geom-multi-point :coordinates="item.rawRoute.coordinates"></ol-geom-multi-point>
                  <ol-style>
                      <ol-style-circle :radius="4">
                          <ol-style-fill color="red"></ol-style-fill>
                          <ol-style-stroke color="green" :width="1"></ol-style-stroke>
                      </ol-style-circle>
                  </ol-style>
                </ol-feature>
              </div>
            </div>
        </ol-source-vector>
      </ol-vector-layer>
    </ol-map>
    </v-col>
    
  </v-row>
  </v-container>
</template>

<script lang="ts" setup>
import {ref, watch} from 'vue';
import {useDisplay} from 'vuetify';
import UserService from "@/services/UserService";
import {useMainStore} from "@/store/mainstore";
import ActivityProcessed from '@/models/activityProcessed';
import RightMenu from '@/components/rightRouteMenu.vue';
import LoadingComponent from '@/components/loading.vue';
import router from "@/router";

const store = useMainStore();
const ready = ref(false);
const coordinateHighlight = ref(false);
const strokeColor = ref("#ff5733");
const strokeWidth = ref(3);
const projection = ref("EPSG:4326");
const zoom = ref(15);
const center = ref([0,0]);
const map = ref(null)
const processedActivities = ref(0);
const refreshNow = ref(true); 

const activities = ref<ActivityProcessed[]>([]);
if(store.averagedRequest == 0)
  request();
store.averagedRequest = window.setInterval(request, 5000);
if(store.centeredRequest !=0)
  window.clearInterval(store.centeredRequest);
function request(){ 
  UserService.getAveragedActivites().then(result=>{
    if(result.status == 202){
      processedActivities.value = parseInt(result.data);
      store.fullyLoaded = false;
      return;
    }
    activities.value = []
    clearInterval(store.averagedRequest);
    store.averagedRequest = 0;
    if(result.data.length == 0){
        store.stravaEmpty = true;
        ready.value = false;
        return;
    }
    store.stravaEmpty = false;
    result.data.forEach(x=>activities.value.push(new ActivityProcessed(x.id,x.name,x.rawRoute,x.startDate)))
    var colorArray = 
    ['#ff4900','#5303ff','#B33300','#ffd503','#030bff',
    '#9aff03','#03a7ff','#53ff03','#03ff74','#e61010',
    '#03ffc0','#03fffb','#0346ff',
    '#8103ff','#c803ff','#ff03dd','#ff0374'];
    activities.value.forEach(
      function (x,index){
        if(!store.colorExists(x.id))
            store.addColor({id:x.id,color:colorArray[Math.floor(Math.random()*colorArray.length)]})
        }
    );
    ready.value = true;
    center.value = store.mapCenter;
    if(store.mapCenter[0] == 0)
      center.value = activities.value[0].rawRoute.coordinates[
            Math.round(activities.value[0].rawRoute.coordinates.length / 2)
          ];
    zoom.value = store.mapZoom;
    
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
    clearInterval(store.averagedRequest);
    store.averagedRequest = setInterval(function(){
      request()
    }, 10000);
  })}

window.onresize = function()
{
  setTimeout( function() {refresh()}, 200);
}

function centerOnId(id:number){
  const activity = activities.value.find(x=>x.id == id);
  center.value = activity?.rawRoute.coordinates[
          Math.round(activity.rawRoute.coordinates.length / 2)]
  zoom.value = store.mapZoom
}
function reload(centered:boolean, coordinates:boolean){
  if(coordinateHighlight.value != coordinates)
    coordinateHighlight.value = coordinates;
}
function refresh(){
  refreshNow.value = false;
  setTimeout(function(){
    refreshNow.value = true;
    center.value = store.mapCenter;
    zoom.value = store.mapZoom;
  }, 1);
}
function zoomChanged(changed: number){
  store.mapZoom = changed;
}
function centerChanged(changed:[]){
  store.mapCenter = changed;
}
</script>
<style scoped>
.mapClass{
  height: 90vh;
}
@media (max-height: 900px) {
  .mapClass{
    height: 80vh;
  }
}
@media (max-height: 640px) {
  .mapClass{
    height: 70vh;
  }
}
@media (max-height: 450px) {
  .mapClass{
    height: 60vh;
  }
}

</style>