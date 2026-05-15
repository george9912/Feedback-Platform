import { useState } from "react";
import Topbar from "../components/Topbar";
import Sidebar from "../components/Sidebar";
import Home from "../pages/Home";
import Notifications from "../pages/Notifications";
import Feedbacks from "../pages/Feedbacks";
import Campaigns from "../pages/Campaigns";
import Directory from "../pages/Directory";
import MyProfile from "../pages/MyProfile";
import AdminPanel from "../pages/AdminPanel";
import ChatWidget from "../pages/ChatWidget";
import "../styles/dashboard.css";

function DashboardLayout() {
  const [selectedMenu, setSelectedMenu] = useState("Home");
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);
  const [prefilledFeedbackRecipient, setPrefilledFeedbackRecipient] = useState(null);

  const handleGiveFeedback = (user) => {
    setPrefilledFeedbackRecipient(user);
    setSelectedMenu("Feedbacks");
  };

  const clearPrefilledRecipient = () => {
    setPrefilledFeedbackRecipient(null);
  };

  const renderPage = () => {
    switch (selectedMenu) {
      case "Home":
        return <Home />;
      case "Notifications":
        return <Notifications />;
      case "Feedbacks":
        return (
          <Feedbacks
            preselectedRecipient={prefilledFeedbackRecipient}
            onPreselectedRecipientConsumed={clearPrefilledRecipient}
          />
        );
      case "Campaigns":
        return <Campaigns />;
      case "Directory":
        return <Directory onGiveFeedback={handleGiveFeedback} />;
      case "MyProfile":
        return <MyProfile />;
      case "AdminPanel":
        return <AdminPanel />;
      default:
        return <Home />;
    }
  };

  return (
    <div className="app-shell">
      <Topbar />
      <div className="app-body">
        <Sidebar
          selectedMenu={selectedMenu}
          setSelectedMenu={setSelectedMenu}
          collapsed={sidebarCollapsed}
          onToggleSidebar={() => setSidebarCollapsed(!sidebarCollapsed)}
        />
        <main className="page-content">{renderPage()}</main>
           <ChatWidget />
      </div>
    </div>
  );
}

export default DashboardLayout;