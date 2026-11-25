import { useEffect, useState } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { ProtectedRoute } from './components/Layout/ProtectedRoute';
import { authService } from './services/auth.service';
import { useAuthStore } from './store/authStore';
import { Layout } from './components/Layout/Layout';
import { LoginPage } from './pages/Login/LoginPage';
import { DashboardPage } from './pages/Dashboard/DashboardPage';
import { OrdersPage } from './pages/Orders/OrdersPage';
import { KanbanPage } from './pages/Kanban/KanbanPage';
import { CustomersPage } from './pages/Customers/CustomersPage';
import { VehiclesPage } from './pages/Vehicles/VehiclesPage';
import { MechanicsPage } from './pages/Mechanics/MechanicsPage';
import { SchedulePage } from './pages/Schedule/SchedulePage';
import { LoyaltyPage } from './pages/Loyalty/LoyaltyPage';
import { WarehousePage } from './pages/Warehouse/WarehousePage';
import { FinancePage } from './pages/Finance/FinancePage';
import { ReportsPage } from './pages/Reports/ReportsPage';
import { SettingsPage } from './pages/Settings/SettingsPage';
import { OperationsPage } from './pages/Operations/OperationsPage';
import { BranchesPage } from './pages/Branches/BranchesPage';

function App() {
  const [bootstrapped, setBootstrapped] = useState(false);
  const setAuth = useAuthStore((s) => s.setAuth);
  const clearAuth = useAuthStore((s) => s.clearAuth);

  useEffect(() => {
    authService
      .refresh()
      .then((res) => setAuth(res.user, res.accessToken))
      .catch(() => clearAuth())
      .finally(() => setBootstrapped(true));
  }, [setAuth, clearAuth]);

  if (!bootstrapped) return null;

  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />

        <Route element={<ProtectedRoute />}>
          <Route element={<Layout />}>
            <Route element={<ProtectedRoute roles={['Admin', 'Manager', 'Accountant']} />}>
              <Route path="/dashboard" element={<DashboardPage />} />
            </Route>
            <Route element={<ProtectedRoute roles={['Admin', 'Manager']} />}>
              <Route path="/orders" element={<OrdersPage />} />
              <Route path="/customers" element={<CustomersPage />} />
              <Route path="/vehicles" element={<VehiclesPage />} />
              <Route path="/schedule" element={<SchedulePage />} />
              <Route path="/loyalty" element={<LoyaltyPage />} />
              <Route path="/warehouse" element={<WarehousePage />} />
              <Route path="/operations" element={<OperationsPage />} />
            </Route>
            <Route element={<ProtectedRoute roles={['Admin', 'Manager', 'Mechanic']} />}>
              <Route path="/kanban" element={<KanbanPage />} />
            </Route>
            <Route element={<ProtectedRoute roles={['Admin', 'Manager', 'Accountant']} />}>
              <Route path="/mechanics" element={<MechanicsPage />} />
            </Route>
            <Route element={<ProtectedRoute roles={['Admin', 'Accountant']} />}>
              <Route path="/finance" element={<FinancePage />} />
              <Route path="/reports" element={<ReportsPage />} />
            </Route>
            <Route element={<ProtectedRoute roles={['Admin']} />}>
              <Route path="/branches" element={<BranchesPage />} />
              <Route path="/settings" element={<SettingsPage />} />
            </Route>
          </Route>
        </Route>

        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        <Route path="*" element={<Navigate to="/dashboard" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
