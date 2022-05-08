import type User from "@/models/user";
import type Activity from "@/models/activity";
import type ActivityDetailed from "@/models/activityDetailed";
import type ActivityList from "@/models/activityList";
import http from "./axios";
import type ActivityProcessed from "@/models/activityProcessed";
import type ActivityRegions from "@/models/activityRegions";

class UserService {
  authorize() {
    return http.post<string>("/api/authorize");
  }
  register(code: string, scope: string) {
    return http.post<string>(
      `/api/register?state=&code=${code}&scope=${scope}`
    );
  }
  logout() {
    return http.post("/api/logout");
  }
  loadActsFromStrava(){
    return http.post("/user/loadallactivities");
  }
  loadNewActsFromStrava(){
    return http.post("/user/loadnewactivities");
  }
  getLoggedInUser() {
    return http.get<User>("/user");
  }
  getActivities(count: number) {
    return http.get<ActivityList>(`/user/activities?perPage=${count}`);
  }
  getActivity(id: number) {
    return http.get<ActivityDetailed>(`/user/activity/${id}`);
  }
  refreshToken() {
    return http.post("/user/refresh");
  }
  getAveragedActivites() {
    return http.get<ActivityProcessed[]>("/user/averagedactivities");
  }
  getCenteredActivites(centered: Boolean) {
    return http.get<ActivityRegions>(`/user/centeredactivities?centered=${centered}`);
  }
  deleteActivityById(id:number){
    return http.delete(`/user/activity/${id}`);
  }
  deleteUserAccount(){
    return http.delete(`/user/account`);
  }
  resetUserAccount(){
    return http.post(`/user/reset`);
  }
}
export default new UserService();
