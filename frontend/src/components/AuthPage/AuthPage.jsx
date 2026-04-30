import { useState } from "react";
import Login from "../Login/Login";
import Register from "../Register/Register";
import "./AuthPage.css";

export default function AuthPage({ setPage }) {
  const [isLogin, setIsLogin] = useState(true);

  return (
    <div className="auth-container">
      
      {/* LOGIN CARD */}
      <div className={`card ${isLogin ? "active" : "inactive"}`}>
        <h2>🔐 Login</h2>
        <Login setPage={setPage} />
        <p onClick={() => setIsLogin(false)} className="switch">
          אין לך משתמש? הירשם
        </p>
      </div>

      {/* REGISTER CARD */}
      <div className={`card ${!isLogin ? "active" : "inactive"}`}>
        <h2>📝 Register</h2>
        <Register setPage={setPage} />
        <p onClick={() => setIsLogin(true)} className="switch">
          כבר רשום? התחבר
        </p>
      </div>

    </div>
  );
}