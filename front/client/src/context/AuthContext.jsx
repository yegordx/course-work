import React, { createContext, useContext, useEffect, useState } from "react";
import {
  login as loginRequest,
  logout as logoutRequest,
  register as registerRequest,
  getProfile
} from "../api/clientApi";

const AuthContext = createContext();

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);      // user = ClientProfileDto
  const [loading, setLoading] = useState(true);

  // Завантаження профілю при завантаженні програми
  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const res = await getProfile();
        setUser(res.data);
      } catch (error) {
        setUser(null);
        console.log(error);
      } finally {
        setLoading(false);
      }
    };

    fetchProfile();
  }, []);

  // 🔁 Оновлення профілю вручну
  const refreshProfile = async () => {
    try {
      const res = await getProfile();
      setUser(res.data);
    } catch (err) {
      console.error("Не вдалося оновити профіль", err);
    }
  };

  // 🔐 Вхід
  const login = async (credentials) => {
    await loginRequest(credentials);
    const res = await getProfile();
    setUser(res.data);
  };

  // 🔓 Вихід
  const logout = async () => {
    await logoutRequest();
    setUser(null);
  };

  // 🆕 Реєстрація
  const register = async (credentials) => {
    await registerRequest(credentials);
    const res = await getProfile();
    setUser(res.data);
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        loading,
        login,
        logout,
        register,
        refreshProfile 
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);