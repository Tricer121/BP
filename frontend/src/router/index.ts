import { createRouter, createWebHistory } from "vue-router";
import HomeView from "../views/ActivityView.vue";
import { useMainStore } from "@/store/mainstore";

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: "/",
      name: "home",
      meta: {
        requiresAuth: true,
        requiresGuest: false,
      },
      component: HomeView,
    },
    {
      path: "/auth",
      name: "auth",
      meta: {
        requiresGuest: true,
      },
      component: () => import("@/views/AuthView.vue"),
    },
    {
      path: "/route",
      name: "routeAveraged",
      meta: {
        requiresAuth: true,
        requiresGuest: false,
      },
      component: () => import("@/views/AveragedRouteView.vue"),
    },
    {
      path: "/regions",
      name: "voronoiRegions",
      meta: {
        requiresAuth: true,
        requiresGuest: false,
      },
      component: () => import("@/views/VoronoiView.vue"),
    },
  ],
});
router.beforeEach((to, from, next) => {
  const requiresAuth = to.matched.some((x) => x.meta.requiresAuth);
  const requiresGuest = to.matched.some((x) => x.meta.requiresGuest);
  const store = useMainStore();

  if(requiresAuth && (store.isLoggedIn==false)){
    return next({ path: '/auth' });
  }
  if(store.isLoggedIn && requiresGuest){
    return next({ path: '/' });
  }
  
  next();
});
export default router;
