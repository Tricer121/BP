// Styles
import "@mdi/font/css/materialdesignicons.css";
import "vuetify/styles";
import { cs } from 'vuetify/locale'

// Vuetify
import { createVuetify } from "vuetify";

export default createVuetify({
  theme: {
    defaultTheme: "dark",
  },
  locale: {
    defaultLocale: 'cs',
    fallbackLocale: 'en',
    messages: { cs }
  },

});

// https://vuetifyjs.com/en/introduction/why-vuetify/#feature-guides
