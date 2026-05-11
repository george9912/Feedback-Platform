import DashboardLayout from "./layout/DashboardLayout";
import Login from "./pages/Login";
import {
  AuthenticatedTemplate,
  UnauthenticatedTemplate,
} from "@azure/msal-react";

const AUTH_ENABLED = true;

function App() {
  if (!AUTH_ENABLED) {
    return <DashboardLayout />;
  }

  return (
    <>
      <AuthenticatedTemplate>
        <DashboardLayout />
      </AuthenticatedTemplate>

      <UnauthenticatedTemplate>
        <Login />
      </UnauthenticatedTemplate>
    </>
  );
}

export default App;