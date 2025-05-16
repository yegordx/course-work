import React, { useState, useRef, useEffect } from "react";
import {SendMessage} from "./Api.js"

import './App.css'

const apiKey = import.meta.env.VITE_API_KEY;

function App() {
  const [messages, setMessages] = useState([
    { text: "Вітаю! Я твій асистент. Чим допомогти?", from: "bot" },
  ]);
  const [input, setInput] = useState("");
  const chatEndRef = useRef(null);

  useEffect(() => {
    chatEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  const handleSend = async () => {
  if (input.trim()) {
    setMessages((prev) => [
      ...prev,
      { text: input, from: "user" }
    ]);
    const userInput = input;
    setInput("");
    try {
      const res = await SendMessage({
        ApiKey: apiKey,
        Request: userInput
      });
      setMessages((prev) => [
        ...prev,
        { text: res.data.answer || "Відповідь відсутня.", from: "bot" }
      ]);
    } catch {
      setMessages((prev) => [
        ...prev,
        { text: "Помилка зв'язку з сервером.", from: "bot" }
      ]);
    }
  }
};

  return (
    <div className="chat-container">

      <div className="chat-header">
        <button className="chat-back">{/* ← */}</button>
        <div className="chat-avatar" />
        <div className="chat-icons">
          <span className="chat-icon-search" />
          <span className="chat-icon-menu" />
        </div>
      </div>

      <div className="chat-messages">
        {messages.map((msg, idx) => (
          <div
            key={idx}
            className={`chat-bubble ${msg.from === "user" ? "right" : "left"}`}
          >
            {msg.text}
          </div>
        ))}
        <div ref={chatEndRef} />
      </div>

      <div className="chat-input-wrapper">
        <input
          className="chat-input"
          type="text"
          placeholder="Message"
          value={input}
          onChange={(e) => setInput(e.target.value)}
          onKeyDown={e => e.key === "Enter" && handleSend()}
        />
        <button className="chat-send" onClick={handleSend}>➤</button>
      </div>
    </div>
  )
}

export default App
