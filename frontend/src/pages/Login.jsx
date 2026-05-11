import { useMsal } from "@azure/msal-react";
import "../styles/login.css";

function Login() {
  const { instance } = useMsal();

  const handleLogin = async () => {
    try {
      await instance.loginRedirect();
    } catch (error) {
      console.error("Microsoft login failed:", error);
    }
  };

  return (
    <div className="login-shell">
      <div className="login-panel login-panel-left">
        <div className="login-left-overlay"></div>

        <div className="login-left-content">
          <div className="login-topbar">
            <div className="login-logo">
              <div className="login-logo-mark">FH</div>
              <div className="login-logo-text">
                <span className="login-logo-label">Enterprise Suite</span>
                <h1>Feedback Hub</h1>
              </div>
            </div>
          </div>

          <div className="login-hero">
            <span className="login-badge">Internal Product</span>
            <h2>Collect feedback, drive action, and improve faster.</h2>
            <p>
              A secure internal platform for managing employee and stakeholder
              feedback, reviewing insights, and tracking improvement initiatives
              across teams.
            </p>

            <div className="login-feature-grid">
              <div className="login-feature-card">
                <div className="login-feature-icon">01</div>
                <div>
                  <h3>Centralized insights</h3>
                  <p>Keep all feedback flows, reviews, and actions in one place.</p>
                </div>
              </div>

              <div className="login-feature-card">
                <div className="login-feature-icon">02</div>
                <div>
                  <h3>Enterprise security</h3>
                  <p>Access controlled through Microsoft identity and company policies.</p>
                </div>
              </div>

              <div className="login-feature-card">
                <div className="login-feature-icon">03</div>
                <div>
                  <h3>Built for scale</h3>
                  <p>Designed to support modern teams, processes, and reporting needs.</p>
                </div>
              </div>
            </div>
          </div>

          <div className="login-left-footer">
            <span>Powered by Azure AD authentication</span>
          </div>
        </div>
      </div>

      <div className="login-panel login-panel-right">
        <div className="login-card">
          <div className="login-card-header">
            <p className="login-card-eyebrow">Welcome back</p>
            <h2>Sign in to continue</h2>
            <p className="login-card-subtitle">
              Use your corporate Microsoft account to access the platform dashboard.
            </p>
          </div>

          <div className="login-card-body">
            <button className="microsoft-login-button" onClick={handleLogin}>
              <span className="microsoft-icon" aria-hidden="true">
                <span className="ms-square ms-red"></span>
                <span className="ms-square ms-green"></span>
                <span className="ms-square ms-blue"></span>
                <span className="ms-square ms-yellow"></span>
              </span>
              <span>Continue with Microsoft</span>
            </button>

            <div className="login-security-box">
              <h3>Secure sign-in</h3>
              <p>
                Authentication is managed through Microsoft Entra ID / Azure AD.
                Your organization’s access rules, roles, and security controls apply automatically.
              </p>
            </div>

            <div className="login-meta">
              <div className="login-meta-item">
                <span className="login-meta-label">Authentication</span>
                <strong>Microsoft Identity</strong>
              </div>
              <div className="login-meta-item">
                <span className="login-meta-label">Access type</span>
                <strong>Corporate account</strong>
              </div>
            </div>
          </div>

          <div className="login-card-footer">
            <span>Need access? Contact your IT administrator.</span>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Login;