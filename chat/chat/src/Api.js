import axios from "axios";


const API = axios.create({
  baseURL: "https://localhost:7008/api",
  withCredentials: true
});

export const SendMessage = (data) => API.post("/customers/ask", data);
