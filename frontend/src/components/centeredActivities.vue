<template> 
<v-container id="listBody2" class="align-center" style="height: 100%">
  <v-row>
    <v-col v-if="ready == false" class="d-flex justify-center">
      <LoadingComponent :progress="processedActivities" :indeterminate="false" />
    </v-col>
    <v-col cols="0" md="1"  class="d-flex justify-end">
      <div v-if="ready == true && store.stravaEmpty == false" >
        <RightMenu @mapRefresh="refresh()" @centerOnId="centerOnId" @reload="reload" :region="true" :activities="activities" />
      </div>
    </v-col>
  </v-row>
  <v-row>
    <v-col v-if="ready && refreshNow">
      <ol-map ref="mapView" class="mapClass">
        <ol-view
          :center="center"
          :rotation="rotation"
          :zoom="zoom"
          :projection="projection"
          @zoomChanged="zoomChanged"
          @centerChanged="centerChanged"
        />
        <ol-tile-layer>
          <ol-source-osm />
        </ol-tile-layer>
        <ol-vector-layer>
          <ol-source-vector ref="vectorLayer">
            <div v-for="item in activities"
                  :key="item.id"
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
              <div v-for="item in activities"
                    :key="item.id"
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
            <ol-feature>
              <ol-geom-multi-polygon :coordinates="regions"></ol-geom-multi-polygon>
              <ol-style>
                <ol-style-fill :color="store.regionColor"></ol-style-fill>
                <ol-style-stroke  
                  :color="store.regionStrokeColor"
                  :width="store.regionStrokeWidth"
                ></ol-style-stroke>
              </ol-style>
            </ol-feature>
    
          </ol-source-vector>
        </ol-vector-layer>
    </ol-map>
      
    </v-col>

  </v-row>
  </v-container>
</template>

<script lang="ts" setup>
import {ref, watch,onMounted, computed} from 'vue';
import UserService from "@/services/UserService";
import {useMainStore} from "@/store/mainstore";
import ActivityProcessed from '@/models/activityProcessed';
import RightMenu from '@/components/rightRouteMenu.vue';
import LoadingComponent from '@/components/loading.vue';

const store = useMainStore();
const ready = ref(false);


const projection = ref("EPSG:4326");
const rotation = ref(0);
const view = ref();

const zoom = ref(14);
const center = ref([0,0]);

const processedActivities = ref(0);
const strokeWidth = ref(3);
const activities = ref<ActivityProcessed[]>([]);
const regions = ref<[][]>([]);
const coordinateHighlight = ref(false);
const mapView = ref(null)
const refreshNow = ref(true); 

function centerOnId(id:number){
  const activity = activities.value.find(x=>x.id == id);
  if(activity.rawRoute.coordinates.length == 1)
    center.value = activity?.rawRoute.coordinates[0]
  else{
    center.value = activity?.rawRoute.coordinates[
            Math.round(activity.rawRoute.coordinates.length / 2)
          ];
  }
  zoom.value = store.mapZoom
}

request(false);
let myInterval = setInterval(function(){
  request(false)
  }, 5000);
function refresh(){
  refreshNow.value = false;
  setTimeout(function(){
    refreshNow.value = true;
    center.value = store.mapCenter;
    zoom.value = store.mapZoom;
  }, 1);
}
function reload(centered:boolean | undefined, coordinates:boolean){
  if(coordinateHighlight.value != coordinates)
    coordinateHighlight.value = coordinates;
  else
    if(centered != undefined)
      request(centered)
    else
      request(false)
}

function request(centered: Boolean){ 
  if(store.fullyLoadedRequestID != 0){
    window.clearInterval(store.fullyLoadedRequestID)
    store.fullyLoadedRequestID = 0;
  }
  UserService.getCenteredActivites(centered).then(result=>{
    if(result.status == 202){
      store.fullyLoaded = false;
      processedActivities.value = parseInt(result.data);
      return;
    }
    activities.value = []
    clearInterval(myInterval);
    
    if(result.data.length == 0){
        store.stravaEmpty = true;
        ready.value = false;
        return;
    }
    store.fullyLoaded = true;
    store.stravaEmpty = false;
    result.data.routes.forEach(x=>activities.value.push(new ActivityProcessed(x.id,x.name,x.rawRoute,x.startDate)))
    regions.value = result.data.regions;
    var colorArray = 
    ['#ff4900','#5303ff','#B33300','#ffd503','#030bff',
    '#9aff03','#03a7ff','#53ff03','#03ff74','#e61010',
    '#03ffc0','#03fffb','#0346ff',
    '#8103ff','#c803ff','#ff03dd','#ff0374'];
    activities.value.forEach(
      function (x,index){
        if(!store.colorExists(x.id))
            store.addColor({id:x.id,color:colorArray[index%colorArray.length]})
        }
    );
    if(store.mapCenter[0] == 0){
      center.value = activities.value[0].rawRoute.coordinates[
            Math.round(activities.value[0].rawRoute.coordinates.length / 2)
          ];
      store.mapCenter = center.value
    }
    else{
      center.value = store.mapCenter;
    }
    zoom.value = store.mapZoom;
    
    ready.value = true;
    refresh();
}).catch(function (err) {
    store.requestFailed = true;
    clearInterval(myInterval);
    myInterval = setInterval(function(){
      request(false)
    }, 10000);
  })
}
window.onresize = function()
{
  setTimeout( function() {refresh()}, 200);
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