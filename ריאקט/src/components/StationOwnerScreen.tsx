import React, { useState } from 'react';

interface ChargingRecord {
  id: string;
  driverName: string;
  date: string;
  kwhUsed: string;
  payout: string;
}

export default function StationOwnerScreen() {
  const [stationName] = useState<string>('העמדה הביתית שלי (#WB-4412)');
  const [isStationActive] = useState<boolean>(true);

  // נתוני טבלת ההיסטורית טעינות
  const [history] = useState<ChargingRecord[]>([
    {
      id: '#801',
      driverName: 'דני כהן',
      date: '05/10/2026',
      kwhUsed: '35 kWh',
      payout: '₪52.50'
    }
  ]);

  return (
    <div className="container">
      <div className="screen-view">
        
        <div className="screen-header">
          <span>פנאל ניהול בעל תחנה</span>
        </div>

        {/* שורת סטטוס עמדה פעילה */}
        <div style={{ display: 'flex', alignItems: 'center', gap: '12px', marginBottom: '20px', flexWrap: 'wrap' }}>
          <span style={{ fontSize: '1.05rem', fontWeight: 'bold', color: 'var(--text-main)' }}>
            עמדה פעילה רשומה: {stationName}
          </span>
          <span style={{ 
            background: isStationActive ? '#064e3b' : '#7f1d1d', 
            color: isStationActive ? 'var(--primary)' : '#fca5a5', 
            padding: '4px 12px', 
            borderRadius: '20px', 
            fontSize: '0.85rem', 
            fontWeight: 'bold' 
          }}>
            {isStationActive ? 'פעילה ותקינה' : 'לא פעילה'}
          </span>
        </div>

        {/* שתי קופסאות מידע צד בצד */}
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(260px, 1fr))', gap: '16px', marginBottom: '25px' }}>
          
          {/* קופסת דירוגים אחרונים */}
          <div style={{ background: '#121212', padding: '16px', borderRadius: '12px', border: '1px solid var(--card-border)' }}>
            <h4 style={{ margin: '0 0 12px 0', color: 'var(--light-blue)', fontSize: '0.95rem', display: 'flex', alignItems: 'center', gap: '6px' }}>
              ⭐ דירוגים אחרונים מנהגים:
            </h4>
            <div style={{ borderTop: '1px solid #262626', paddingTop: '10px', fontSize: '0.88rem' }}>
              <span style={{ fontWeight: 'bold', color: 'white' }}>דני כהן: </span>
              <span style={{ color: 'var(--yellow)' }}>⭐⭐⭐⭐⭐ </span>
              <span style={{ color: 'var(--text-muted)', fontStyle: 'italic' }}>"עמדה מצויינת!"</span>
            </div>
          </div>

          {/* קופסת דיווחי תקלות */}
          <div style={{ background: '#121212', padding: '16px', borderRadius: '12px', border: '1px solid var(--card-border)' }}>
            <h4 style={{ margin: '0 0 12px 0', color: '#f87171', fontSize: '0.95rem', display: 'flex', alignItems: 'center', gap: '6px' }}>
              🚨 דיווחי תקלות מהשטח:
            </h4>
            <p style={{ margin: 0, fontSize: '0.85rem', color: 'var(--text-muted)', fontStyle: 'italic' }}>
              אין דיווחי תקלות פעילים כרגע. התחנה במצב תקין.
            </p>
          </div>

        </div>

        {/* טבלת היסטוריית טעינות ותגמולים */}
        <div>
          <h4 style={{ margin: '0 0 12px 0', color: 'var(--light-blue)', fontSize: '1rem' }}>
            היסטוריית טעינות ותגמולים כספיים מהחברה הראשית:
          </h4>

          <div style={{ overflowX: 'auto' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'right', fontSize: '0.9rem' }}>
              <thead>
                <tr style={{ background: '#121212', color: 'var(--text-muted)', borderBottom: '1px solid var(--card-border)' }}>
                  <th style={{ padding: '10px' }}>מזהה</th>
                  <th style={{ padding: '10px' }}>שם הנהג</th>
                  <th style={{ padding: '10px' }}>תאריך</th>
                  <th style={{ padding: '10px' }}>kWh נצרך</th>
                  <th style={{ padding: '10px' }}>תגמול כספי (₪)</th>
                </tr>
              </thead>
              <tbody>
                {history.map((row) => (
                  <tr key={row.id} style={{ borderBottom: '1px solid #262626' }}>
                    <td style={{ padding: '12px 10px', color: 'var(--text-muted)' }}>{row.id}</td>
                    <td style={{ padding: '12px 10px', color: 'white', fontWeight: 'bold' }}>{row.driverName}</td>
                    <td style={{ padding: '12px 10px', color: 'var(--text-muted)' }}>{row.date}</td>
                    <td style={{ padding: '12px 10px', color: 'white' }}>{row.kwhUsed}</td>
                    <td style={{ padding: '12px 10px', color: 'var(--primary)', fontWeight: 'bold' }}>{row.payout}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

      </div>
    </div>
  );
}