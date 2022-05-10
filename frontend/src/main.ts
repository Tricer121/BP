import { createApp } from "vue";
import App from "./App.vue";
import router from "./router";
import vuetify from "./plugins/vuetify";
import { loadFonts } from "./plugins/webfontloader";
import { createPinia } from "pinia";
import piniaPluginPersistedstate from "pinia-plugin-persistedstate";
import OpenLayersMap from "vue3-openlayers";
import "vue3-openlayers/dist/vue3-openlayers.css";

loadFonts();

const app = createApp(App)
  .use(createPinia().use(piniaPluginPersistedstate))
  .use(router)
  .use(vuetify)
  .use(OpenLayersMap)
  .mount("#app");
