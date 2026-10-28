import React, { useState } from 'react';

export default function UserProfileScreen() {
  const [carModel] = useState<string>('Tesla Model 3 Long Range');
  const [communityPoints] = useState<number>(150);

  return (
    <div className="container">
      <div className="screen-view">
        
        <div className="screen-header">
          <span>פרופיל משתמש ונקודות קהילה</span>
        </div>

        <div style={{ maxWidth: '500px', margin: '0 auto' }}>
          
          <div className="form-group">
            <label>דגם רכב:</label>
            <input 
              type="text" 
              className="form-control" 
              value={carModel} 
              readOnly 
              style={{ textAlign: 'center', fontWeight: 'bold' }}
            />
          </div>

          <div className="form-group">
            <label>נקודות קהילה ובונוסים:</label>
            <input 
              type="text" 
              className="form-control" 
              value={`${communityPoints} נקודות`} 
              readOnly 
              style={{ 
                color: 'var(--primary)', 
                fontWeight: 'bold', 
                fontSize: '1.2rem',
                textAlign: 'center' 
              }}
            />
          </div>

        </div>

      </div>
    </div>
  );
}