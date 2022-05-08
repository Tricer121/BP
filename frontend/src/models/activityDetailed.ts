export default class ActivityDetailed {
  public name: string;
  public id: number;
  public distance: string;
  public elevationGain: string;
  public elapsedTime: string;
  public maxSpeed: string;
  public averageSpeed: string;
  public startDate: string;
  public rawRoute: [];
  constructor(
    name: string,
    id: number,
    distance: number,
    elevationGain: number,
    elapsedTime: number,
    maxSpeed: number,
    averageSpeed: number,
    startDate: number,
    rawRoute: []
  ) {
    this.name = name;
    this.id = id;
    this.distance = distance.toFixed(1).toLocaleString();
    this.elevationGain = elevationGain.toFixed(1).toLocaleString();
    this.elapsedTime = new Date(elapsedTime * 1000).toISOString().substr(11, 8);
    this.maxSpeed = maxSpeed.toFixed(1).toLocaleString();
    this.averageSpeed = averageSpeed.toFixed(1).toLocaleString();
    this.startDate = new Date(startDate).toLocaleDateString();
    this.rawRoute = rawRoute;
    
  }
}
