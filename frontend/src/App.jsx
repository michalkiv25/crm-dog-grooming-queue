import { useState, useEffect } from "react";
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
        <div style={{ display: "flex", gap: "20px" }}>
          <Register />
          <Login />
        </div>
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