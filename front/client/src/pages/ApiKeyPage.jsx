import React, { useEffect, useState } from "react";
import { getApiKey } from "../api/clientApi";
import { useNavigate } from "react-router-dom";

export default function ApiKeyPage() {
  const [apiKey, setApiKey] = useState("");
  const [error, setError] = useState("");
  const navigate = useNavigate();

  useEffect(() => {
    const fetchApiKey = async () => {
      try {
        const response = await getApiKey();
        setApiKey(response.data.apiKey);
      } catch (err) {
        setError("Не вдалося отримати API ключ 😢");
        console.error(err);
      }
    };

    fetchApiKey();
  }, []);

  return (
    <div className="centered-container">
      <div className="form-card">
        <h2 className="section-title"> Ваш API ключ</h2>

        {error ? (
          <div className="warning-card">{error}</div>
        ) : apiKey ? (
          <>
            <h1 className="profile-info" >
              Скопіюйте цей ключ і збережіть у надійному місці:
            </h1>
            <code className="apikey">{apiKey}</code>
            <button
              className="button"
              style={{ marginTop: "1.5rem" }}
              onClick={() => navigate("/profile")}
            >
              Далі
            </button>
          </>
        ) : (
          <p className="profile-info">Завантажуємо ключ...</p>
        )}
      </div>
    </div>
  );
}