// src/App.jsx
import { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import Login from './components/Login';
import Dashboard from './components/Dashboard';
import PrinterList from './components/PrinterList';
import PrinterDetail from './components/PrinterDetail';
import UserList from './components/UserList';
import PolicyList from './components/PolicyList';

function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);

  useEffect(() => {
    const token = localStorage.getItem('token');
    setIsAuthenticated(!!token);
  }, []);

  return (
    <Router>
      <div className="App">
        <Routes>
          <Route
            path="/login"
            element={isAuthenticated ? <Navigate to="/dashboard" /> : <Login />}
          />
          <Route
            path="/dashboard"
            element={isAuthenticated ? <Dashboard /> : <Navigate to="/login" />}
          />
          <Route
            path="/printers"
            element={isAuthenticated ? <PrinterList /> : <Navigate to="/login" />}
          />
          <Route
            path="/printers/:id"
            element={isAuthenticated ? <PrinterDetail /> : <Navigate to="/login" />}
          />
          <Route
            path="/users"
            element={isAuthenticated ? <UserList /> : <Navigate to="/login" />}
          />
          <Route
            path="/policies"
            element={isAuthenticated ? <PolicyList /> : <Navigate to="/login" />}
          />
          <Route path="/" element={<Navigate to="/dashboard" />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;
