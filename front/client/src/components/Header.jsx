import React from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext"; // 👈 Підключаємо контекст
import logoutIcon from "../assets/logout.svg";

export default function Header() {
  const navigate = useNavigate();
  const { user, logout } = useAuth(); // 🔥 Отримуємо користувача та функцію logout

  const handleLogout = async () => {
    await logout();      // 👈 викликає функцію з контексту
    navigate("/login");  // 👈 перенаправляє користувача
  };

  return (
    <header className="app-header">
      <div className="header-left">
        <span className="logo-text">🌿 MyApp</span>
      </div>
      <div className="header-right">
        {user && (
          <button className="logout-button" onClick={handleLogout}>
            <img src={logoutIcon} alt="Logout" className="logout-icon" />
          </button>
        )}
      </div>
    </header>
  );
}