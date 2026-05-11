import { useState } from "react";
import "../styles/dashboard.css";

function Sidebar({ selectedMenu, setSelectedMenu, collapsed, onToggleSidebar }) {
  const [usersOpen, setUsersOpen] = useState(true);

  return (
    <aside className={`sidebar ${collapsed ? "collapsed" : ""}`}>
      <div className="sidebar-top">
        <button className="sidebar-toggle" onClick={onToggleSidebar}>
          ☰
        </button>
      </div>

      <nav className="sidebar-nav">
        <button
          className={`sidebar-item ${selectedMenu === "Home" ? "active" : ""}`}
          onClick={() => setSelectedMenu("Home")}
          title={collapsed ? "Home" : ""}
        >
          <span className="sidebar-icon">🏠</span>
          {!collapsed && <span className="sidebar-label">Home</span>}
        </button>

        <button
          className={`sidebar-item ${selectedMenu === "Notifications" ? "active" : ""}`}
          onClick={() => setSelectedMenu("Notifications")}
          title={collapsed ? "Notifications" : ""}
        >
          <span className="sidebar-icon">🔔</span>
          {!collapsed && <span className="sidebar-label">Notifications</span>}
        </button>

        <button
          className={`sidebar-item ${selectedMenu === "Feedbacks" ? "active" : ""}`}
          onClick={() => setSelectedMenu("Feedbacks")}
          title={collapsed ? "Feedbacks" : ""}
        >
          <span className="sidebar-icon">💬</span>
          {!collapsed && <span className="sidebar-label">Feedbacks</span>}
        </button>

        <div className="sidebar-group">
          <button
            className={`sidebar-item ${
              selectedMenu === "Directory" || selectedMenu === "MyProfile" ? "active" : ""
            }`}
            onClick={() => setUsersOpen(!usersOpen)}
            title={collapsed ? "Users" : ""}
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
            <div className="sidebar-submenu">
              <button
                className={`sidebar-subitem ${selectedMenu === "Directory" ? "active-sub" : ""}`}
                onClick={() => setSelectedMenu("Directory")}
              >
                Directory
              </button>

              <button
                className={`sidebar-subitem ${selectedMenu === "MyProfile" ? "active-sub" : ""}`}
                onClick={() => setSelectedMenu("MyProfile")}
              >
                My Profile
              </button>
            </div>
          )}
        </div>
      </nav>
    </aside>
  );
}

export default Sidebar;