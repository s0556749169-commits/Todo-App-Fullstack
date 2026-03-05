import React, { useEffect, useState } from 'react';
import service from './service.js';

function App() {
  const [newTodo, setNewTodo] = useState("");
  const [todos, setTodos] = useState([]);
  
  // הוספת מצב למשתמש מחובר ושדות התחברות
  const [token, setToken] = useState(localStorage.getItem("token"));
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");

  async function getTodos() {
    try {
      const todos = await service.getTasks();
      setTodos(todos);
    } catch (err) {
      console.error("נכשל בטעינת משימות");
    }
  }
async function handleRegister(e) {
  e.preventDefault();
  try {
    await service.register(username, password);
    alert("נרשמת בהצלחה! עכשיו אפשר להתחבר");
  } catch (err) {
    alert("ההרשמה נכשלה. אולי השם כבר תפוס?");
  }
}
  // פונקציית התחברות
  async function handleLogin(e) {
    e.preventDefault();
    try {
      await service.login(username, password);
      setToken(localStorage.getItem("token")); // עדכון הסטייט כדי שיוצגו המשימות
    } catch (err) {
      alert("שם משתמש או סיסמה שגויים");
    }
  }

  // פונקציית יציאה
  function handleLogout() {
    localStorage.removeItem("token");
    setToken(null);
    setTodos([]);
  }

  // שאר הפונקציות שלך (נשארות אותו דבר)
  async function createTodo(e) {
    e.preventDefault();
    await service.addTask(newTodo);
    setNewTodo("");
    await getTodos();
  }

  async function updateCompleted(todo, isComplete) {
    await service.setCompleted(todo.id, isComplete);
    await getTodos();
  }

  async function deleteTodo(id) {
    await service.deleteTask(id);
    await getTodos();
  }

  useEffect(() => {
    if (token) {
      getTodos();
    }
  }, [token]); // יתבצע מחדש כשהטוקן משתנה

  // אם המשתמש לא מחובר - נציג טופס לוגין פשוט
  if (!token) {
    return (
      <section className="todoapp">
        <header className="header">
          <h1>Login</h1>
          <form onSubmit={handleLogin} style={{ padding: "20px" }}>
            <input 
              className="new-todo" 
              placeholder="Username" 
              style={{ position: "static", border: "1px solid #ccc" }}
              onChange={(e) => setUsername(e.target.value)} 
            />
            <input 
              className="new-todo" 
              type="password" 
              placeholder="Password" 
              style={{ position: "static", border: "1px solid #ccc", marginTop: "10px" }}
              onChange={(e) => setPassword(e.target.value)} 
            />
            <button type="submit" style={{ marginTop: "10px", padding: "10px", width: "100%" }}>כניסה</button>
            <button 
                type="button" 
                onClick={handleRegister} 
                style={{ flex: 1, padding: "10px", backgroundColor: "#2196F3", color: "white", border: "none", cursor: "pointer" }}
              >
                הרשמה (חדש)
              </button>
          </form>
        </header>
      </section>
    );
  }

  // אם המשתמש מחובר - נציג את רשימת המשימות המקורית שלך
  return (
    <section className="todoapp">
      <header className="header">
        <button onClick={handleLogout} style={{ float: "right", margin: "10px" }}>יציאה</button>
        <h1>todos</h1>
        <form onSubmit={createTodo}>
          <input className="new-todo" placeholder="Well, let's take on the day" value={newTodo} onChange={(e) => setNewTodo(e.target.value)} />
        </form>
      </header>
      <section className="main" style={{ display: "block" }}>
        <ul className="todo-list">
          {todos.map(todo => {
            return (
              <li className={todo.isComplete ? "completed" : ""} key={todo.id}>
                <div className="view">
                  <input className="toggle" type="checkbox" checked={todo.isComplete} onChange={(e) => updateCompleted(todo, e.target.checked)} />
                  <label>{todo.name}</label>
                  <button className="destroy" onClick={() => deleteTodo(todo.id)}></button>
                </div>
              </li>
            );
          })}
        </ul>
      </section>
    </section >
  );
}

export default App;