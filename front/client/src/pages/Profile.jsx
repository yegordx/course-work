import React from "react";
import { Navigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext"; // 👈 Імпортуємо контекст
import "../styles/Profile.css";

export default function Profile() {
  const { user, loading } = useAuth();
  //const navigate = useNavigate();

  if (loading) {
    return <div className="centered-container"><p>Завантаження профілю...</p></div>;
  }

  if (!user) {
    return <Navigate to="/login" />;
  }

  const isUnfinished =
    user.expiration === "0001-01-01T00:00:00Z" ||
    !user.embeddingProvider ||
    !user.isActivated;

  return (
    <div className="profile-wrapper">
      <div className="profile-card">
        <h2 className="section-title">Профіль користувача</h2>

        {isUnfinished && (
          <div className="warning-card">
            <p>⚠️ Ваш обліковий запис ще не завершено:</p>
            {user.expiration === "0001-01-01T00:00:00Z" && <p>• Ви не обрали тарифний план</p>}
            {!user.embeddingProvider && <p>• Ви не вказали embedding-провайдера</p>}
            {!user.isActivated && <p>• Ви не завантажили жодного питання</p>}
          </div>
        )}

        <div className="profile-info">
          <p><strong>Email:</strong> {user.email}</p>
          <p><strong>Створено:</strong> {new Date(user.createdAt).toLocaleDateString()}</p>
          <p><strong>Провайдер:</strong> {user.embeddingProvider || "—"}</p>
          <p><strong>Підписка активна до:</strong> {
            user.expiration === "0001-01-01T00:00:00Z"
              ? "Не вказано"
              : new Date(user.expiration).toLocaleDateString()
          }</p>
          <p><strong>Акаунт активовано:</strong> {user.isActivated ? "✅ Так" : "Ні"}</p>
          {user.isActivated && (
            <p><strong>API ключ:</strong> <code className="apikey">{user.apiKey}</code></p>
          )}
        </div>
      </div>
    </div>
  );
}