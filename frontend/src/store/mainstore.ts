import { defineStore } from "pinia";
import type Activity from "@/models/activity";

export const useMainStore = defineStore("mainStore", {
  state: () => {
    return {
      isLoggedIn: false,
      requestFailed: false,
      requestSuccess: false,
      error401: false,
      successMessage: "",
      errorMessage: "",
      userName: "",
      activityList: [] as Activity[],
      locale: "",
      IdAndColors: [] as IdColor[],
      stravaEmpty: false,
      regionColor: "rgba(255,255,0,0.4)",
      regionStrokeColor: "green",
      regionStrokeWidth: 1,
      fullyLoaded: false,
      mapCenter:[0,0],
      mapZoom: 14,
      fullyLoadedRequestID: 0,
    };
  },
  actions:{
    colorExists(id:number){
      if(this.IdAndColors.some(x=>x.id == id)){
        return true;
      }
      else 
        return false;
    },
    addColor(object: IdColor){
      this.IdAndColors.push(object);
    },
    setColor(id:number,color:string){
      var index = this.IdAndColors.findIndex(x=>x.id == id);
      this.IdAndColors[index].color = color;
    }
  },
  getters: {
    getColorById: (state)=>{
      return (colorId) => state.IdAndColors.find(x=>x.id == colorId)?.color;
    }
  },
  persist: true,
});
