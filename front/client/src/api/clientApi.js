import axios from "axios";
import Cookies from "js-cookie";

const API = axios.create({
  baseURL: "https://localhost:7008/api",
  withCredentials: true
});

// 👉 Реєстрація
export const register = (data) => API.post("/clients/register", data);

// 👉 Логін
export const login = (data) => API.post("/clients/login", data);

// 👉 Вибір тарифу
export const selectPlan = (data) => API.post(`/clients/plan`, data);

// 👉 Вибір провайдера
export const setProvider = (data) => API.post(`/clients/embedding-provider`, data);

// 👉 Завантаження питань


// 👉 Отримання ключа
export const getApiKey = () => API.get(`/clients/key`);

// ✅ 👉 Отримання даних профілю (через токен в cookie)
export const getProfile = () => API.get("/clients/profile");

//logout 
export const logout = () => API.post(`/clients/logout`);

export const uploadFaq = (data) => API.post("/questions/upload", data);

// // Отримання всіх питань користувача
// export const getQuestions = () => API.get("/questions");

// // Видалення конкретного питання
// export const deleteQuestion = (id) => API.delete(`/questions/${id}`);

// // Додавання одного нового питання
// export const isAuthenticated = () => !!Cookies.get("access_token");
