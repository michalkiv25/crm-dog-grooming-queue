import Login from "./components/Login/Login";
import Register from "./components/Register/Register";
import NavBar from "./components/NavBar/NavBar";
import MyAppointments from "./components/MyAppointments/MyAppointments";
import CreateAppointment from "./components/CreateAppointment/CreateAppointment";

function App() {
  return (
    <div className="container">

      <NavBar />

      <div style={{ display: "flex", gap: "20px", justifyContent: "center" }}>
        <Login />
        <Register />
      </div>

      <MyAppointments />
      <CreateAppointment />

    </div>
  );
}

export default App;