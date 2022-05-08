import axios from "axios";

const config = {
  baseURL: "http://localhost:7040/",
  headers: {
    "Content-type": "application/json",
  },
  withCredentials: true,
  crossDomain: true,
};
const http = axios.create(config);
export default http;
