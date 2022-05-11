<template> 
    <v-navigation-drawer
        width="300"
        position="right"
        v-model="drawer"
        :permanent="mdAndUp"
        class="d-flex flew-column justify-center"
        >
        
          <v-expansion-panels variant="accordion"> 
            <div style="width:100%">
                <v-btn v-if="!mdAndUp" variant="text" style="width:100%" color="deep-orange" @click="drawer=false">
                    <v-spacer/>
                    Zavřit menu
                    <v-spacer/>
                    <v-icon color="deep-orange">mdi-arrow-collapse-right</v-icon>
                </v-btn>
            </div>
            <v-expansion-panel>
                <v-expansion-panel-title>
                    <v-spacer/><v-btn variant="text">Nastavení tras</v-btn><v-spacer/>
                </v-expansion-panel-title>
                <v-expansion-panel-text>
                    <v-card v-if="region">
                        <v-card-title class="justify-center">Nastavení regionu</v-card-title>
                        <v-card-text class="d-flex flex-row justify-center">
                            <v-btn class="btnspacearound" :color="store.regionColor" @click="regionFillToggle = true">
                            <v-icon color="white">mdi-format-color-fill</v-icon>
                            <v-tooltip 
                                activator="parent"
                                anchor="bottom">
                                Změna barvy regionu
                            </v-tooltip>
                            </v-btn>
                            <v-btn class="btnspacearound"  :color="store.regionStrokeColor" @click="regionStrokeColorToggle = true">
                                <v-icon color="white">mdi-pencil-circle-outline</v-icon>
                                <v-tooltip 
                                    activator="parent"
                                    anchor="bottom">
                                    Změna barvy hranice
                                </v-tooltip>
                            </v-btn>
                            <v-btn :color="store.regionStrokeColor" @click="regionStrokeWidthToggle = true">
                                <v-icon  color="white">mdi-format-color-highlight</v-icon>
                                <v-tooltip 
                                    activator="parent"
                                    anchor="bottom">
                                    Změna tlouštky hranice
                                </v-tooltip>
                            </v-btn>
                        </v-card-text>
                    </v-card>
                    <div style="padding:5px"/>
                    <v-card class="d-flex flex-column">
                        <v-card-text>
                            <v-switch 
                                color="deep-orange-darken-1" 
                                :label="typeView"
                                v-model="regionSwitch"
                                :disabled="!requestDone"
                                v-if="region"
                                hide-details
                            >
                            </v-switch>
                            <v-switch 
                                color="deep-orange-darken-1" 
                                label="Zvýraznění souřadnic"
                                v-model="coordinateSwitch"
                                hide-details
                                >
                            </v-switch>
                        </v-card-text>
                    </v-card>
                </v-expansion-panel-text>
            </v-expansion-panel>
            <v-expansion-panel
              v-for="item in activities"
              :key="item.id"
              :value="item"
              :active-color=store.getColorById(item.id)
            >
                <v-expansion-panel-title  @click="$emit('centerOnId',item.id)" expand-icon="mdi-menu-down">
                    <v-container class="d-flex flex-row">
                        <v-row>
                        <v-col>
                            <p >{{ item.name }}</p>
                        </v-col>
                        <v-col>
                            <p>{{ item.startDate }}</p>
                        </v-col>
                        </v-row>
                    </v-container>
                </v-expansion-panel-title>
                <v-expansion-panel-text>
                    <div class="d-flex justify-center">
                        <v-btn class="btnspacearound" color="primary" @click="viewDetail(item.id)">
                            <v-icon>mdi-eye-circle</v-icon>
                            <v-tooltip 
                                activator="parent"
                                anchor="bottom">
                                Detail trasy
                            </v-tooltip>
                        </v-btn>
                        <v-btn class="btnspacearound" :color="store.getColorById(item.id)" @click="colorPickToggle(item.id)">
                            <v-icon>mdi-pencil-circle-outline</v-icon>
                            <v-tooltip 
                                activator="parent"
                                anchor="bottom">
                                Změna barvy
                            </v-tooltip>
                        </v-btn>
                        <v-btn class="btnspacearound" 
                        color="error" 
                        @click="deletePrompt(item.id)"
                        :disabled="!store.fullyLoaded"
                        >
                            <v-icon>mdi-delete-circle</v-icon>
                            <v-tooltip 
                                activator="parent"
                                anchor="bottom">
                                Smazat trasu
                            </v-tooltip>
                        </v-btn>
                    </div>
                </v-expansion-panel-text>
            </v-expansion-panel>
        </v-expansion-panels>
    </v-navigation-drawer>
    <v-btn justify-end v-if="!mdAndUp" @click="drawer=true" color="transparent" style="plain">
        <v-icon color="deep-orange">mdi-arrow-expand-left</v-icon>
    </v-btn>

    <v-dialog v-model="picker">
        <v-color-picker v-model="colorPick"></v-color-picker>
    </v-dialog>
    <v-dialog v-model="regionFillToggle">
        <v-color-picker v-model="regionFillPicker"></v-color-picker>
    </v-dialog>
    <v-dialog v-model="regionStrokeColorToggle">
        <v-color-picker v-model="regionStrokePicker"></v-color-picker>
    </v-dialog>
    <v-dialog v-model="regionStrokeWidthToggle">
        <v-card>
            <v-card-title>Změna tloušťky hranice</v-card-title>
            <v-card-text>
                <v-slider v-model="regionStrokeWidthPicker"
                    :min="0"
                    :max="10"
                    :step="0.05"
                    thumb-label>
                </v-slider> 
            </v-card-text>
            
        </v-card>
    </v-dialog>
    <v-dialog v-model="deleteDialog" >
        <DeleteDialog :id="activeId" @close="close"/>
    </v-dialog>
    <v-dialog v-model="activityView">
        <ActivityVue :id="activeId" :map="false"/>
    </v-dialog>
</template>
<script lang="ts" setup>

import ActivityProcessed from '@/models/activityProcessed';
import ActivityVue from "@/components/activity.vue";
import {useMainStore} from "@/store/mainstore";
import {ref, watch, computed} from 'vue';
import {useDisplay} from 'vuetify';
import UserService from '@/services/UserService';
import router from '@/router';
import DeleteDialog from '@/components/deleteDialog.vue';

const store = useMainStore();
const emit = defineEmits(['centerOnId','mapRefresh', 'reload','activityDeleted'])

const props = defineProps({
  activities: Array,
  region: Boolean
});

const picker = ref(false);

const regionFillPicker = ref("#FFFFFFFF");
const regionFillToggle = ref(false);
const regionStrokePicker = ref("#FFFFFFFF");
const regionStrokeColorToggle = ref(false);
const regionStrokeWidthToggle = ref(false);
const regionStrokeWidthPicker = ref(0.1);

const regionSwitch = ref(false);
const coordinateSwitch = ref(false);

const deleteDialog = ref(false);
const activityView = ref(false);
const { mdAndUp } = useDisplay()
const colorPick = ref('#FFFFFF');
const drawer = ref(true);
if(mdAndUp.value == false)
    drawer.value=false;
const activeId = ref(-1);
function colorPickToggle(id:number){
    activeId.value = id;
    colorPick.value = store.getColorById(id);
    picker.value=true;
}
function deletePrompt(id:number){
    deleteDialog.value = true;
    activeId.value = id;
}
function close(){
    deleteDialog.value = false;
    emit('activityDeleted');
}
function viewDetail(id:number){
    activeId.value = id;
    activityView.value = true;
}
function expandMenu(){
    drawer.value = true;
}
const requestDone = ref(true);
const typeView = computed(()=>{
    return regionSwitch.value ?  "Centrované cesty" : "Nezpracované cesty"
})
watch(() => picker.value,(newValue,oldValue)=>{
    if(newValue == false && oldValue == true){
        store.setColor(activeId.value,colorPick.value);
        emit('mapRefresh');
    }
})
watch(() => regionFillToggle.value,(newValue,oldValue)=>{
    if(newValue == false && oldValue == true){
        store.regionColor = regionFillPicker.value;
        emit('mapRefresh');
    }
})
watch(() => regionStrokeColorToggle.value,(newValue,oldValue)=>{
    if(newValue == false && oldValue == true){
        store.regionStrokeColor = regionStrokePicker.value;
        emit('mapRefresh');
    }
})
watch(() => regionStrokeWidthToggle.value,(newValue,oldValue)=>{
    if(newValue == false && oldValue == true){
        store.regionStrokeWidth = regionStrokeWidthPicker.value;
        if(regionStrokeWidthPicker.value == 0.0){
            regionStrokePicker.value = regionStrokePicker.value.substr(0,7)+"00";
            store.regionStrokeColor =  regionStrokePicker.value;
        }
        else if(regionStrokePicker.value.substr(7,9)=="00"){
            regionStrokePicker.value = regionStrokePicker.value.substr(0,7)+"FF";
            store.regionStrokeColor =  regionStrokePicker.value;
        }
        emit('mapRefresh');
    }
})
watch(() => [regionSwitch.value,coordinateSwitch.value],(newValue,oldValue)=>{
    requestDone.value = false;
    emit('reload',newValue[0],newValue[1]);
    setTimeout( function() {requestDone.value = true}, 3000);

})
watch(() => mdAndUp.value,newValue=>{
    drawer.value = mdAndUp.value
})
const collapseArrow = computed(() => {
  return drawer.value == true ? false : true
});
</script>

<style scoped>
.v-expansion-panel-content:deep(.v-expansion-panel-text__wrapper){
    justify-content: center !important; 
    padding: 0px !important;
}
.btnspacearound{
    margin-right: 5px;
}
</style>