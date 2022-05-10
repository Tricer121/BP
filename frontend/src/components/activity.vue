<template>
  <v-sheet v-model="activity" elevation="20">
    <v-card>
      <v-card-title class="text-h5"> Detail aktivity </v-card-title>
      <v-card-text>
        <v-container v-if="activity">
          <v-row>
            <v-col>
              Ucestovaná vzdálenost:<br />
              {{ activity.distance }} metrů
            </v-col>
            <v-col>
              Maximální rychlost:<br />
              {{ activity.maxSpeed }} km/h
            </v-col>

            <v-col>
              Výšková změna:<br />
              {{ activity.elevationGain }} metrů
            </v-col>
            <v-col>
              Délka trvání:<br />
              {{ activity.elapsedTime }}
            </v-col>
            <v-col>
              Průměrná rychlost:<br />
              {{ activity.averageSpeed }} km/h
            </v-col>
          </v-row>
        </v-container>
      </v-card-text>
    </v-card>

    <ol-map v-if="mapContainer && map" style="height: 57vh">
      <ol-view
        ref="view"
        :center="center"
        :rotation="rotation"
        :zoom="zoom"
        :projection="projection"
      />
      <ol-tile-layer>
        <ol-source-osm />
      </ol-tile-layer>
      <ol-vector-layer>
        <ol-source-vector>
          <ol-feature>
            <ol-geom-line-string
              :coordinates="activity.rawRoute.coordinates"
            ></ol-geom-line-string>
            <ol-style>
              <ol-style-stroke
                :color="strokeColor"
                :width="strokeWidth"
              ></ol-style-stroke>
            </ol-style>
          </ol-feature>
        </ol-source-vector>
      </ol-vector-layer>
    </ol-map>
  </v-sheet>
</template>
<style scoped>
.v-col {
  text-align: center;
}
</style>
<script lang="ts" setup>
import ActivityDetailed from "@/models/activityDetailed";
import UserService from "@/services/UserService";
import { ref, onBeforeMount } from "vue";
import { useMainStore } from "@/store/mainstore";
import router from "@/router";
const mapContainer = ref(false);

const store = useMainStore();
const center = ref([40, 40]);
const strokeColor = ref("#ff5733");
const strokeWidth = ref(4);
const projection = ref("EPSG:4326");
const zoom = ref(16);
const rotation = ref(0);
const props = defineProps({
  id: Number,
  map: Boolean,
});
const activity = ref<ActivityDetailed>();
onBeforeMount(() => {
  if (props.id != undefined) {
    UserService.getActivity(props.id)
      .then((x) => {
        activity.value = new ActivityDetailed(
          x.data.name,
          x.data.id,
          x.data.distance,
          x.data.elevationGain,
          x.data.elapsedTime,
          x.data.maxSpeed,
          x.data.averageSpeed,
          x.data.startDate,
          x.data.rawRoute
        );
        center.value =
          x.data.rawRoute.coordinates[
            Math.round(x.data.rawRoute.coordinates.length / 2)
          ];
        strokeColor.value = store.getColorById(props.id);
        mapContainer.value = true;
      })
      .catch(function (err) { 
        if(err.response)
          if(err.response.status == 401){
            store.error401 = true;
            store.requestFailed = true;
            setTimeout( function() {store.$reset(); router.push('/');router.go(0)}, 3000);
            return;
          }
        store.requestFailed = true;
      });
  }
});
</script>
