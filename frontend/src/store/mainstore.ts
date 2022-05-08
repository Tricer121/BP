import { defineStore } from "pinia";
import type Activity from "@/models/activity";

export const useMainStore = defineStore("mainStore", {
  // arrow function recommended for full type inference
  state: () => {
    return {
      // all these properties will have their type inferred automatically
      isLoggedIn: false,
      requestFailed: false,
      requestSuccess: false,
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
