import Login from "./components/Login";
import Register from "./components/Register";
import CreateAppointment from "./components/CreateAppointment";
import MyAppointments from "./components/MyAppointments";

function App() {
  return (
    <div style={{ padding: 20 }}>
      <h1>🐶 Dog Queue System</h1>

      <Login />

      <Register />

      <CreateAppointment />

     <MyAppointments />
    </div>
  );
}

export default App;