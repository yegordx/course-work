import React, { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { useAuth } from "../context/AuthContext"; // 👈 нове

export default function Login() {
  const navigate = useNavigate();
  const { login } = useAuth(); // 👈 login з контексту
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const handleLogin = async () => {
    if (!email || !password) {
      alert("Заповніть всі поля!");
      return;
    }

    try {
      await login({ email, password }); // ⬅️ викликаємо login із контексту
      navigate("/profile");
    } catch (err) {
      console.error("Помилка входу:", err);
      alert("Невірні дані або помилка на сервері");
    }
  };

  return (
    <div className="centered-container">
      <div className="form-card">
        <h2 className="section-title">Вхід</h2>
        <input
          className="input"
          type="email"
          placeholder="Email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
        />
        <input
          className="input"
          type="password"
          placeholder="Пароль"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />
        <button className="button" onClick={handleLogin}>Увійти</button>
        <p style={{ textAlign: "center", marginTop: "1rem" }}>
          Немає акаунта?{" "}
          <Link to="/register" className="link">
            Зареєструватися
          </Link>
        </p>
      </div>
    </div>
  );
}