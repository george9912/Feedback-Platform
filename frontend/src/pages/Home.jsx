function Home() {
  return (
    <div className="content-card">
      <div className="page-header-block">
        <h1 className="section-title">Welcome to Feedback Hub</h1>
        <p className="section-text">
          Your internal platform for employee feedback, people insights, and
          organization-wide collaboration.
        </p>
      </div>

      <div className="stats-grid">
        <div className="stat-card">
          <div className="stat-icon">👥</div>
          <div className="stat-title">Employee Directory</div>
          <div className="stat-text">
            Browse company users synced from Microsoft Graph.
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon">💬</div>
          <div className="stat-title">Feedback</div>
          <div className="stat-text">
            Share and manage employee feedback in one central place.
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon">🔐</div>
          <div className="stat-title">Secure Access</div>
          <div className="stat-text">
            Sign in with your company Microsoft account.
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon">⚡</div>
          <div className="stat-title">Internal Tools</div>
          <div className="stat-text">
            Access company-only features built for your organization.
          </div>
        </div>
      </div>
    </div>
  );
}

export default Home;