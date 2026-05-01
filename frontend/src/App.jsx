import { useState, useEffect } from "react";
import "./App.css";
import Login from "./components/Login/Login";
import Register from "./components/Register/Register";
import NavBar from "./components/NavBar/NavBar";
import MyAppointments from "./components/MyAppointments/MyAppointments";
import CreateAppointment from "./components/CreateAppointment/CreateAppointment";

function App() {
  const [token, setToken] = useState(localStorage.getItem("token"));

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
          <NavBar />
          <MyAppointments />
          <CreateAppointment />
        </>
      )}
    </div>
  );
}

export default App;