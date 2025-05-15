import React from "react";
import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";
import Login from "./pages/Login";
import Register from "./pages/Register";
import Profile from "./pages/Profile";
import SelectPlan from "./pages/SelectPlan";
import SelectProvider from "./pages/SelectProvider";
import Header from "./components/Header";
import ManageQuestions from "./pages/ManageQuestions";
import ApiKeyPage from "./pages/ApiKeyPage";

import "./styles/App.css";
import "./styles/Header.css";
import "./styles/Profile.css";

export default function App() {
  return ( 
     <Router>
     <Header/>
      <Routes>
        <Route path="/" element={<Navigate to="/login" />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/profile" element={<Profile />} />
        <Route path="/select-plan" element={<SelectPlan />} />
        <Route path="/select-provider" element={<SelectProvider />} />
        <Route path="/questions" element={<ManageQuestions />} />
        <Route path="/api-key" element={<ApiKeyPage />} />
      </Routes>
    </Router>
  );
}