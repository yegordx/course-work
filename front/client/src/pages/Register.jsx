import React, { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { register } from "../api/clientApi";

export default function Register() {
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");

  const handleRegister = async () => {
    if (!email || !password || !confirmPassword) {
      alert("Заповніть всі поля!");
      return;
    }

    if (password !== confirmPassword) {
      alert("Паролі не збігаються!");
      return;
    }

    try {
      await register({ email, password });
      navigate("/select-plan");
    } catch (err) {
      console.error("Помилка реєстрації", err);
    }
  };

  return (
    <div className="centered-container">
    <div className="form-card">
    <h2 className="section-title">Реєстрація</h2>

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

        <input
          className="input"
          type="password"
          placeholder="Підтвердіть пароль"
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
        />

        <button className="button" onClick={handleRegister}>
          Зареєструватись
        </button>

        <p style={{ textAlign: "center", marginTop: "1rem" }}>
          Вже маєте акаунт?{" "}
          <Link to="/login" className="link">
            Увійти
          </Link>
        </p>
      </div>
    </div>
  );
}