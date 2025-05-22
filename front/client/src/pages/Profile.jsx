import React from "react";
import { Navigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext"; 
import { useNavigate } from "react-router-dom";
import "../styles/Profile.css";

export default function Profile() {
  const { user, loading } = useAuth();
  const navigate = useNavigate();

  if (loading) {
    return <div className="centered-container"><p>Завантаження профілю...</p></div>;
  }

  if (!user) {
    return <Navigate to="/login" />;
  }

  const isMissingPlan = user.expiration === "0001-01-01T00:00:00Z";
  const isMissingProvider = !user.embeddingProvider;
  const isMissingQuestions = !user.isActivated;

  const isUnfinished = isMissingPlan || isMissingProvider || isMissingQuestions;

  const renderWarningMessage = () => {
    if (isMissingPlan) {
      return (
        <>
          <p>⚠️ Ваш обліковий запис ще не завершено:</p>
          <p>• <span className="link-like" onClick={() => navigate("/select-plan")}>Оберіть тарифний план</span></p>
        </>
      );
    }

    if (isMissingProvider) {
      return (
        <>
          <p>⚠️ Ваш обліковий запис ще не завершено:</p>
          <p>• <span className="link-like" onClick={() => navigate("/select-provider")}>Вкажіть embedding-провайдера</span></p>
        </>
      );
    }

    if (isMissingQuestions) {
      return (
        <>
          <p>⚠️ Ваш обліковий запис ще не завершено:</p>
          <p>• <span className="link-like" onClick={() => navigate("/questions")}>Завантажте питання</span></p>
        </>
      );
    }

    return null;
  };

  return (
    <div className="profile-wrapper">
      <div className="profile-card">
        <h2 className="section-title">Профіль користувача</h2>

        {isUnfinished && (
          <div className="warning-card">
            {renderWarningMessage()}
          </div>
        )}

        {!isUnfinished && (
          <button
          className="button"
          onClick={() => navigate("/questions")}
          style={{ marginBottom: "1.5rem" }}
        >
          Перейти до питань
        </button>
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
          <p><strong>Акаунт активовано:</strong> {user.isActivated ? "Так" : "Ні"}</p>
          {user.isActivated && (
            <p><strong>API ключ:</strong> <code className="apikey">{user.apiKey}</code></p>
          )}
        </div>
      </div>
    </div>
  );
}