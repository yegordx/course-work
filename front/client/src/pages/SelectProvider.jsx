import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { setProvider } from "../api/clientApi";
import { useAuth } from "../context/AuthContext";

export default function SelectProvider() {
  const navigate = useNavigate();
  const { refreshProfile } = useAuth();
  const [selected, setSelected] = useState(null);
  const [apiKey, setApiKey] = useState("");

  const providers = [
    { name: "OpenAI", label: "OpenAI" },
    { name: "Cohere", label: "Cohere" },
    { name: "HuggingFace", label: "HuggingFace" },
  ];

  const handleSubmit = async () => {
    if (!selected || !apiKey) {
      alert("Будь ласка, оберіть провайдера та введіть API ключ");
      return;
    }

    try {
      await setProvider({ providerName: selected, providedKey: apiKey });
      await refreshProfile();
      navigate("/profile");
    } catch (err) {
      console.error("Помилка збереження провайдера:", err);
      alert("Не вдалося зберегти провайдера");
    }
  };

  return (
    <div className="centered-container">
      <div className="form-card">
        <h2 className="section-title">Оберіть embedding-провайдера</h2>

        <div className="card-row">
          {providers.map((prov) => (
            <div
              key={prov.name}
              className={`card card-small ${selected === prov.name ? "selected-card" : ""}`}
              onClick={() => setSelected(prov.name)}
            >
              <h2>{prov.label}</h2>
            </div>
          ))}
        </div>

        <input
          className="input input-spacing"
          type="text"
          placeholder="Ваш API ключ"
          value={apiKey}
          onChange={(e) => setApiKey(e.target.value)}
        />

        <button className="button" onClick={handleSubmit}>
          Продовжити
        </button>

        <p className="skip-label" onClick={() => navigate("/profile")}>
            Пропустити
        </p>
      </div>
    </div>
  );
}