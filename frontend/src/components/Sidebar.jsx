import { useState } from "react";
import { useMsal } from "@azure/msal-react";
import "../styles/dashboard.css";

function Sidebar({
  selectedMenu,
  setSelectedMenu,
  collapsed,
  onToggleSidebar,
  onMenuSelect,
}) {
  const [usersOpen, setUsersOpen] = useState(true);
  const { instance } = useMsal();

  const handleLogout = () => {
    instance.logoutRedirect();
  };

  const handleMenuSelect = (menuName) => {
    setSelectedMenu(menuName);
    onMenuSelect?.();
  };

  const handleUsersToggle = () => {
    if (collapsed) {
      onToggleSidebar?.();
      return;
    }

    setUsersOpen((prev) => !prev);
  };

  return (
    <aside className={`sidebar ${collapsed ? "collapsed" : ""}`} aria-label="Primary">
      <div className="sidebar-top">
        <button
          className="sidebar-toggle"
          onClick={onToggleSidebar}
          aria-label={collapsed ? "Open navigation" : "Collapse navigation"}
        >
          ☰
        </button>
      </div>

      <nav className="sidebar-nav">
        <button
          className={`sidebar-item ${selectedMenu === "Home" ? "active" : ""}`}
          onClick={() => handleMenuSelect("Home")}
          title={collapsed ? "Home" : ""}
          aria-current={selectedMenu === "Home" ? "page" : undefined}
        >
          <span className="sidebar-icon">🏠</span>
          {!collapsed && <span className="sidebar-label">Home</span>}
        </button>

        <button
          className={`sidebar-item ${selectedMenu === "Notifications" ? "active" : ""}`}
          onClick={() => handleMenuSelect("Notifications")}
          title={collapsed ? "Notifications" : ""}
          aria-current={selectedMenu === "Notifications" ? "page" : undefined}
        >
          <span className="sidebar-icon">🔔</span>
          {!collapsed && <span className="sidebar-label">Notifications</span>}
        </button>

        <button
          className={`sidebar-item ${selectedMenu === "Feedbacks" ? "active" : ""}`}
          onClick={() => handleMenuSelect("Feedbacks")}
          title={collapsed ? "Feedbacks" : ""}
          aria-current={selectedMenu === "Feedbacks" ? "page" : undefined}
        >
          <span className="sidebar-icon">💬</span>
          {!collapsed && <span className="sidebar-label">Feedbacks</span>}
        </button>

        <button
          className={`sidebar-item ${selectedMenu === "Campaigns" ? "active" : ""}`}
          onClick={() => handleMenuSelect("Campaigns")}
          title={collapsed ? "Campaigns" : ""}
          aria-current={selectedMenu === "Campaigns" ? "page" : undefined}
        >
          <span className="sidebar-icon">📊</span>
          {!collapsed && <span className="sidebar-label">Campaigns</span>}
        </button>

        <button
          className={`sidebar-item ${selectedMenu === "AdminPanel" ? "active" : ""}`}
          onClick={() => handleMenuSelect("AdminPanel")}
          title={collapsed ? "Admin Panel" : ""}
          aria-current={selectedMenu === "AdminPanel" ? "page" : undefined}
        >
          <span className="sidebar-icon">🛡️</span>
          {!collapsed && <span className="sidebar-label">Admin Panel</span>}
        </button>

        <div className="sidebar-group">
          <button
            className={`sidebar-item ${
              selectedMenu === "Directory" || selectedMenu === "MyProfile" ? "active" : ""
            }`}
            onClick={handleUsersToggle}
            title={collapsed ? "Users" : ""}
            aria-expanded={!collapsed && usersOpen}
            aria-controls="sidebar-users-submenu"
          >
            <span className="sidebar-icon">👥</span>
            {!collapsed && (
              <>
                <span className="sidebar-label">Users</span>
                <span className="sidebar-arrow">{usersOpen ? "▾" : "▸"}</span>
              </>
            )}
          </button>

          {!collapsed && usersOpen && (
            <div className="sidebar-submenu" id="sidebar-users-submenu">
              <button
                className={`sidebar-subitem ${selectedMenu === "Directory" ? "active-sub" : ""}`}
                onClick={() => handleMenuSelect("Directory")}
                aria-current={selectedMenu === "Directory" ? "page" : undefined}
              >
                Directory
              </button>

              <button
                className={`sidebar-subitem ${selectedMenu === "MyProfile" ? "active-sub" : ""}`}
                onClick={() => handleMenuSelect("MyProfile")}
                aria-current={selectedMenu === "MyProfile" ? "page" : undefined}
              >
                My Profile
              </button>
            </div>
          )}
        </div>
      </nav>

      <div className="sidebar-bottom">
        <button
          className="sidebar-item logout-item"
          onClick={handleLogout}
          title={collapsed ? "Logout" : ""}
        >
          <span className="sidebar-icon">🚪</span>
          {!collapsed && <span className="sidebar-label">Logout</span>}
        </button>
      </div>
    </aside>
  );
}

export default Sidebar;