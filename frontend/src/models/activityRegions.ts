import type ActivityProcessed from '@/models/activityProcessed';
export default class activityRegion{
    public routes: ActivityProcessed[];
    public regions: [];
    constructor(_routes:ActivityProcessed[],_regions:[]){
        this.routes=_routes;
        this.regions=_regions;
        
    }
}