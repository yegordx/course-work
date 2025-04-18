import axios from "axios";
import Cookies from "js-cookie";

const API = axios.create({
  baseURL: "https://localhost:7008/api/clients",
  withCredentials: true
});

// 👉 Реєстрація
export const register = (data) => API.post("/register", data);

// 👉 Логін
export const login = (data) => API.post("/login", data);

// 👉 Вибір тарифу
export const selectPlan = (data) => API.post(`/plan`, data);

// 👉 Вибір провайдера
export const setProvider = (data) => API.post(`/embedding-provider`, data);

// 👉 Завантаження питань
export const uploadFaq = (clientId, data) => API.post(`/${clientId}/faq`, data);

// 👉 Отримання ключа
export const getApiKey = (clientId) => API.get(`/${clientId}/apikey`);

// ✅ 👉 Отримання даних профілю (через токен в cookie)
export const getProfile = () => API.get("/profile");

//logout 
export const logout = () => API.post(`/logout`);

export const isAuthenticated = () => !!Cookies.get("access_token");