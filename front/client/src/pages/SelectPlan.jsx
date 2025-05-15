import React from "react";
import { useNavigate } from "react-router-dom";
import { selectPlan } from "../api/clientApi";
import { useAuth } from "../context/AuthContext";

export default function SelectPlan() {
  const navigate = useNavigate();
  const { refreshProfile } = useAuth();
  const plans = [
    { months: 1, label: "1 місяць" },
    { months: 2, label: "2 місяці" },
    { months: 3, label: "3 місяці" },
  ];

  const handleSelect = async (months) => {
    try {
      await selectPlan({ durationMonths: months });
      await refreshProfile(); // 👈 оновлюємо після вибору
      navigate("/select-provider");
    } catch (err) {
      console.error("Помилка при виборі тарифу", err);
      alert("Не вдалося вибрати тариф");
    }
  };

  const handleSkip = async () => {
    await refreshProfile(); // 👈 оновлюємо при пропуску також
    navigate("/profile");
  };

  return (
    <div className="centered-container">
      
      <div style={{ textAlign: "center", width: "100%", maxWidth: "900px" }}>
      <h2 className="section-title">Оберіть тарифний план</h2>
        
        <div className="card-row">
          {plans.map((plan) => (
            <div key={plan.months} className="card card-small" onClick={() => handleSelect(plan.months)}>
              <h2>{plan.label}</h2>
            </div>
          ))}
        </div>

        <p className="skip-label" onClick={handleSkip}>
          Пропустити
        </p>
      </div>
    </div>
  );
}