import Login from "./components/Login/Login";
import Register from "./components/Register/Register";

function App() {
  return (
    <div className="container" style={styles.wrapper}>

      {/* Login */}
      <div style={styles.card}>
        <Login />
      </div>

      {/* Register */}
      <div style={styles.card}>
        <Register />
      </div>

    </div>
  );
}

export default App;

const styles = {
  wrapper: {
    display: "flex",
    justifyContent: "center",
    gap: "30px",
    marginTop: "50px",
    flexWrap: "wrap",
  },
  card: {
    width: "300px",
    padding: "20px",
    border: "1px solid #ddd",
    borderRadius: "10px",
    background: "white",
  },
};