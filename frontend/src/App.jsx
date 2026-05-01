import { useState, useEffect } from "react";
import {
  Navigate,
  Route,
  Routes,
  useLocation,
  useNavigate,
} from "react-router-dom";
import "./App.css";
import Login from "./components/Login/Login";
import Register from "./components/Register/Register";
import MyAppointments from "./components/MyAppointments/MyAppointments";
import CreateAppointment from "./components/CreateAppointment/CreateAppointment";
import NotFound from "./components/NotFound/NotFound";

function App() {
  const [token, setToken] = useState(localStorage.getItem("token"));
  const [refreshTrigger, setRefreshTrigger] = useState(0);
  const [authMode, setAuthMode] = useState("login");
  const navigate = useNavigate();
  const location = useLocation();
  const isKnownRoute =
    location.pathname === "/" || location.pathname === "/appointments";

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
    <div className={`container ${isKnownRoute ? "" : "container-plain"}`}>
      <Routes>
        <Route
          path="/"
          element={
            token ? (
              <Navigate to="/appointments" replace />
            ) : (
              <main className="home-grid">
                <section className="hero-panel">
                  <h1>Dog Grooming Queue</h1>
                  <p>
                    Welcome to the dog grooming queue system. Register quickly
                    and login to manage your appointments, view your queue, and
                    get discounts for loyal customers.
                  </p>
                  <div className="hero-features">
                    <span>Secure login</span>
                    <span>Easy booking</span>
                    <span>Discounts after 3 appointments</span>
                  </div>
                </section>
                <section className="auth-grid">
                  {authMode === "login" ? (
                    <Login
                      onLogin={(nextToken) => setToken(nextToken)}
                      onSwitchToRegister={() => setAuthMode("register")}
                    />
                  ) : (
                    <Register onSwitchToLogin={() => setAuthMode("login")} />
                  )}
                </section>
              </main>
            )
          }
        />

        <Route
          path="/appointments"
          element={
            token ? (
              <>
                <div className="logout-container">
                  <p>שלום, {localStorage.getItem("fullname") || "משתמש"}</p>
                  <button
                    className="logout-button"
                    onClick={() => {
                      localStorage.removeItem("token");
                      localStorage.removeItem("fullname");
                      setToken(null);
                      navigate("/");
                    }}
                  >
                    Logout
                  </button>
                </div>
                <MyAppointments refreshTrigger={refreshTrigger} />
                <CreateAppointment
                  onSuccess={() => setRefreshTrigger((prev) => prev + 1)}
                />
              </>
            ) : (
              <Navigate to="/" replace />
            )
          }
        />

        <Route
          path="*"
          element={<NotFound fallbackPath={token ? "/appointments" : "/"} />}
        />
      </Routes>
    </div>
  );
}

export default App;