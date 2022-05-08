export default class ActivityProcessed {
  public id: number;
  public name: string;
  public startDate: string;
  public rawRoute: [];
  constructor(id: number, name: string, rawRoute: [], startDate: string) {
    this.name = name;
    this.id = id;
    this.rawRoute = rawRoute;
    this.startDate = new Date(startDate).toLocaleDateString();

  }
}
