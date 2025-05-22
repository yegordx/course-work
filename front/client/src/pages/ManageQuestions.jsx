import React, { useState } from "react";
import { getDocument, GlobalWorkerOptions } from "pdfjs-dist";
import workerUrl from "pdfjs-dist/build/pdf.worker.min?url";
import { uploadFaq } from "../api/clientApi";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext"; 

import backIcon from "../assets/back.svg";

GlobalWorkerOptions.workerSrc = workerUrl;

export default function ManageQuestions() {
  const [parsedText, setParsedText] = useState("");
  const [isSending, setIsSending] = useState(false);
  const [dragActive, setDragActive] = useState(false);
  const { refreshProfile } = useAuth();

  const navigate = useNavigate();

  const handleFile = async (file) => {
    if (!file || file.type !== "application/pdf") {
      alert("Будь ласка, виберіть PDF файл.");
      return;
    }

    const reader = new FileReader();
    reader.onload = async function () {
      const typedArray = new Uint8Array(this.result);
      const pdf = await getDocument({ data: typedArray }).promise;

      let text = "";
      for (let i = 1; i <= pdf.numPages; i++) {
        const page = await pdf.getPage(i);
        const content = await page.getTextContent();
        text += content.items.map(item => item.str).join(" ") + "\n";
      }

      setParsedText(text.trim());
    };

    reader.readAsArrayBuffer(file);
  };

  const handleDrop = (e) => {
    e.preventDefault();
    setDragActive(false);
    const file = e.dataTransfer.files[0];
    handleFile(file);
  };

  const handleDragOver = (e) => {
    e.preventDefault();
    setDragActive(true);
  };

  const handleDragLeave = () => {
    setDragActive(false);
  };

  const handleFileInput = (e) => {
    const file = e.target.files[0];
    handleFile(file);
  };

  const parseTextToQuestions = (text) => {
    const result = [];
    const parts = text.split(/\d+\.\s*Питання[:.]?/g).filter(Boolean);

    parts.forEach(part => {
      const [questionPart, answerPart] = part.split(/Відповідь[:.]?/);
      if (questionPart?.trim() && answerPart?.trim()) {
        result.push({
          question: questionPart.trim(),
          answer: answerPart.trim()
        });
      }
    });

    return result;
  };

  const handleSend = async () => {
    const parsed = parseTextToQuestions(parsedText);
    if (parsed.length === 0) {
      alert("Не вдалося знайти жодного питання.");
      return;
    }
    console.log(parsed);
    setIsSending(true);
    try {
      await uploadFaq(parsed);
      alert("✅ Питання успішно надіслано!");
      refreshProfile();
      navigate("/api-key");
    } catch (err) {
      console.error(err);
      alert("❌ Помилка при надсиланні.");
    } finally {
      setIsSending(false);
    }
  };

  const handleBack = () => navigate("/profile");

  return (
    <>
    <button className="back-button" onClick={handleBack}>
      <img src={backIcon} alt="Назад" />
    </button>
    
    <div className="centered-container">
      <div className="form-card">
        <h2 className="section-title">Завантаження питань із PDF</h2>

        <div
          className={`drop-zone input-spacing ${dragActive ? "selected-card" : ""}`}
          onDrop={handleDrop}
          onDragOver={handleDragOver}
          onDragLeave={handleDragLeave}
        >
          <p>Перетягніть PDF-файл сюди або <label htmlFor="file-upload" className="link-like">виберіть</label></p>
          <input
            id="file-upload"
            type="file"
            accept="application/pdf"
            onChange={handleFileInput}
            className="hidden-file"
          />
        </div>

        <textarea
          className="input"
          value={parsedText}
          readOnly
          placeholder="Текст з PDF буде тут..."
          rows={10}
        />

        <button
          className="button"
          onClick={handleSend}
          disabled={isSending}
        >
          Відправити на сервер
        </button>
      </div>
    </div>
    </>
  );
}