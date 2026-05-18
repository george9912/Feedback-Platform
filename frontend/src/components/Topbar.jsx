import { useMsal } from "@azure/msal-react";

function Topbar({ isMobile, onToggleSidebar, sidebarCollapsed }) {
  const { accounts } = useMsal();

  const account = accounts[0];
  const name = account?.name || "User";

  const initials = name
    .split(" ")
    .map((x) => x[0])
    .join("")
    .slice(0, 2)
    .toUpperCase();

  return (
    <div className="topbar">
      {/* LEFT SIDE */}
      <div className="topbar-left">
        {isMobile && (
          <button
            className="icon-button"
            onClick={onToggleSidebar}
            aria-label={sidebarCollapsed ? "Open navigation" : "Close navigation"}
            title={sidebarCollapsed ? "Open navigation" : "Close navigation"}
          >
            ☰
          </button>
        )}
        <div className="brand">
          <div className="brand-logo">FH</div>
          <div className="brand-text-wrap">
            <span className="brand-text">FeedbackHub</span>
            <span className="brand-subtext">Enterprise Suite</span>
          </div>
        </div>
      </div>

      {/* RIGHT SIDE */}
      <div className="topbar-right">
        <div className="user-box">
          <div className="user-avatar">{initials}</div>
          <div className="user-name">{name}</div>
        </div>
      </div>
    </div>
  );
}

export default Topbar;