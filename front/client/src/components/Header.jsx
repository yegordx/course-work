import React from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext"; 
import logoutIcon from "../assets/logout.svg";

export default function Header() {
  const navigate = useNavigate();
  const { user, logout } = useAuth(); 

  const handleLogout = async () => {
    await logout();      
    navigate("/login");
  };

  return (
    <header className="app-header">
      <div className="header-left">
        <span className="logo-text">🌿 AskMe</span>
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