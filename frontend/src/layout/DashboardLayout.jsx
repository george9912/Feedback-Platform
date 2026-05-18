import { useEffect, useState } from "react";
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
  const [isMobile, setIsMobile] = useState(window.innerWidth <= 900);
  const [prefilledFeedbackRecipient, setPrefilledFeedbackRecipient] = useState(null);

  useEffect(() => {
    const mediaQuery = window.matchMedia("(max-width: 900px)");

    const handleViewportChange = (event) => {
      setIsMobile(event.matches);
      setSidebarCollapsed(event.matches);
    };

    mediaQuery.addEventListener("change", handleViewportChange);

    return () => {
      mediaQuery.removeEventListener("change", handleViewportChange);
    };
  }, []);

  const handleGiveFeedback = (user) => {
    setPrefilledFeedbackRecipient(user);
    setSelectedMenu("Feedbacks");
  };

  const clearPrefilledRecipient = () => {
    setPrefilledFeedbackRecipient(null);
  };

  const toggleSidebar = () => {
    setSidebarCollapsed((prev) => !prev);
  };

  const handleMenuSelectionComplete = () => {
    if (isMobile) {
      setSidebarCollapsed(true);
    }
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
      <Topbar
        isMobile={isMobile}
        onToggleSidebar={toggleSidebar}
        sidebarCollapsed={sidebarCollapsed}
      />
      <div className="app-body">
        {isMobile && !sidebarCollapsed && (
          <button
            className="sidebar-backdrop"
            onClick={() => setSidebarCollapsed(true)}
            aria-label="Close navigation"
          />
        )}
        <Sidebar
          selectedMenu={selectedMenu}
          setSelectedMenu={setSelectedMenu}
          collapsed={sidebarCollapsed}
          onToggleSidebar={toggleSidebar}
          onMenuSelect={handleMenuSelectionComplete}
        />
        <main className="page-content">{renderPage()}</main>
           <ChatWidget />
      </div>
    </div>
  );
}

export default DashboardLayout;