export default class Activity {
  public name: string;
  public id: number;
  public elapsedTime: number;
  public startDate: string;
  constructor(
    name: string,
    id: number,
    elapsedTime: number,
    startDate: string
  ) {
    this.name = name;
    this.id = id;
    this.elapsedTime = elapsedTime;
    this.startDate = new Date(startDate).toLocaleDateString();
  }
}
