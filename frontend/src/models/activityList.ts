import Activity from "@/models/activity";
export default class ActivityList {
  public pages: number;
  public perPage: number;
  public activities: Activity[][];
  constructor(pages: number, perPage: number, pageList: Activity[][]) {
    this.pages = pages;
    this.perPage = perPage;
    this.activities = [];
    pageList.forEach((page) => {
      const newPage: Activity[] = [];
      page.forEach((activity) => {
        newPage.push(
          new Activity(
            activity.name,
            activity.id,
            activity.elapsedTime,
            activity.startDate
          )
        );
      });
      this.activities.push(newPage);
    });
  }
}
