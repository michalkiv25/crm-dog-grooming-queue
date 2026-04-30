import { useState } from "react";
import Login from "./components/Login/Login";
import Register from "./components/Register/Register";
import CreateAppointment from "./components/CreateAppointment/CreateAppointment";
import MyAppointments from "./components/MyAppointments/MyAppointments";

function App() {
  const [page, setPage] = useState("login");

  const logout = () => {
    localStorage.removeItem("token");
    setPage("login");
  };

return (
  <div className="container">
    <h1>🐶 Dog Grooming Queue</h1>

    <div className="nav">
      <button onClick={() => setPage("login")}>Login</button>
      <button onClick={() => setPage("register")}>Register</button>
      <button onClick={() => setPage("dashboard")}>Dashboard</button>
      <button onClick={logout}>Logout</button>
    </div>

    {page === "login" && <Login />}
    {page === "register" && <Register />}
    {page === "dashboard" && (
      <>
        <CreateAppointment />
        <MyAppointments />
      </>
    )}
  </div>
);
}

export default App;