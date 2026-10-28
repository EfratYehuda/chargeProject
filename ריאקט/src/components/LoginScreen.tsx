import React, { useState } from 'react';

export default function LoginScreen() {
  const [authMode, setAuthMode] = useState<'login' | 'register'>('login');
  const [selectedRole, setSelectedRole] = useState<string>('driver');
  const [hasStation, setHasStation] = useState<boolean>(true);

  return (
    <div className="container">
      <div className="screen-view">
        
        <div className="screen-header">
          <span>⚡ כניסה למערכת / הרשמה</span>
        </div>

        <div style={{ maxWidth: '450px', margin: '0 auto' }}>
          
          <div className="auth-toggle">
            <div 
              className={`auth-toggle-btn ${authMode === 'login' ? 'active' : ''}`}
              onClick={() => setAuthMode('login')}
            >
              התחברות
            </div>
            <div 
              className={`auth-toggle-btn ${authMode === 'register' ? 'active' : ''}`}
              onClick={() => setAuthMode('register')}
            >
              הרשמה מחדש
            </div>
          </div>

          {authMode === 'login' ? (
            <div>
              <div className="form-group">
                <label>📧 שם משתמש / אימייל:</label>
                <input type="text" className="form-control" defaultValue="israel@ev.com" />
              </div>
              <div className="form-group">
                <label>🔒 סיסמה:</label>
                <input type="password" className="form-control" defaultValue="123456" />
              </div>
            </div>
          ) : (
            <div>
              <div className="form-group">
                <label>👤 שם מלא:</label>
                <input type="text" className="form-control" placeholder="ישראל ישראלי" />
              </div>
              <div className="form-group">
                <label>📧 אימייל:</label>
                <input type="email" className="form-control" placeholder="user@example.com" />
              </div>
               <div className="form-group">
                <label>📱 טלפון:</label>
                <input type="phone" className="form-control" placeholder="055-5555555" />
              </div>
              <div className="form-group">
                <label>🔒 סיסמה:</label>
                <input type="password" className="form-control" defaultValue="123456" />
              </div>
            </div>
          )}

          <div className="form-group">
            <label>👤 בחר אופן כניסה למערכת:</label>
            <select 
              className="form-control"
              value={selectedRole} 
              onChange={(e) => setSelectedRole(e.target.value)}
            >
              <option value="driver">לקוח / נהג המחפש טעינה (Driver)</option>
              <option value="owner">משתמש תחנה (Station Owner)</option>
            </select>
          </div>

          {selectedRole === 'owner' && (
            <div className="form-group" style={{ background: '#121212', padding: '12px', borderRadius: '8px', border: '1px solid #333' }}>
              <label style={{ fontWeight: 'normal', display: 'flex', alignItems: 'center', gap: '8px', cursor: 'pointer' }}>
                <input type="checkbox" checked={hasStation} onChange={(e) => setHasStation(e.target.checked)} />
                <strong>האם קיימת לך כבר תחנה רשומה במערכת?</strong>
              </label>
            </div>
          )}

          <button className="btn" style={{ marginTop: '10px' }}>
            המשך למערכת ➔
          </button>
        </div>

      </div>
    </div>
  );
}