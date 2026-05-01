import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import "./App.css";
import Login from "./components/Login/Login";
import Register from "./components/Register/Register";
import NavBar from "./components/NavBar/NavBar";
import MyAppointments from "./components/MyAppointments/MyAppointments";
import CreateAppointment from "./components/CreateAppointment/CreateAppointment";

function App() {
  const [token, setToken] = useState(localStorage.getItem("token"));
  const [refreshTrigger, setRefreshTrigger] = useState(0);
  const navigate = useNavigate();

  // מקשיב לשינויים ב-localStorage
  useEffect(() => {
    const checkToken = () => {
      setToken(localStorage.getItem("token"));
    };

    window.addEventListener("storage", checkToken);

    return () => {
      window.removeEventListener("storage", checkToken);
    };
  }, []);

  // אם אין token ומנסים להכנס לנתיב שלא הבית, חזור לבית
  useEffect(() => {
    if (!token && window.location.pathname !== "/") {
      navigate("/");
    }
  }, [token, navigate]);

  return (
    <div className="container">
      {!token && (
        <main className="home-grid">
          <section className="hero-panel">
            <h1>Dog Grooming Queue</h1>
            <p>
              Welcome to the dog grooming queue system. Register quickly and
              login to manage your appointments, view your queue, and get
              discounts for loyal customers.
            </p>
            <div className="hero-features">
              <span>Secure login</span>
              <span>Easy booking</span>
              <span>Discounts after 3 appointments</span>
            </div>
          </section>
          <section className="auth-grid">
            <Register />
            <Login onLogin={(token) => setToken(token)} />
          </section>
        </main>
      )}

      {token && (
        <>
          <div className="logout-container">
            <p>שלום, {localStorage.getItem("fullname") || "משתמש"}</p>
            <button className="logout-button" onClick={() => { localStorage.removeItem("token"); localStorage.removeItem("fullname"); setToken(null); navigate("/"); }}>Logout</button>
          </div>
          <MyAppointments refreshTrigger={refreshTrigger} />
          <CreateAppointment onSuccess={() => setRefreshTrigger(prev => prev + 1)} />
        </>
      )}
    </div>
  );
}

export default App;