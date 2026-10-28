import React, { useState } from 'react';

interface Station {
  id: number;
  name: string;
  address: string;
  distance: string;
  price?: string;
  rating: string;
  availableSockets?: string;
  details?: string;
  timeEstimate?: string;
  matchBadge?: string;
  distanceBadge?: string;
}

export default function StationResultsScreen() {
  const [activeFilter, setActiveFilter] = useState<string>('הכל');
  
  // ברירת המחדל היא עמדה מס' 1, אבל זה משתנה בלחיצה!
  const [selectedStationId, setSelectedStationId] = useState<number>(1);
  const [showConfirmation, setShowConfirmation] = useState<boolean>(false);

  const [stations] = useState<Station[]>([
    {
      id: 1,
      name: "Sonol E-V Energy (שפיים)",
      address: "מתחם שפיים",
      distance: "12 ק\"מ",
      details: "💡 דירוג משתמשים גבוה במיוחד ואפס דיווחי תקלות השבוע.",
      timeEstimate: "⏱️ זמן משוער: 25 דקות סך הכל",
      rating: "⭐ 4.9",
      matchBadge: "התאמה 98%"
    },
    {
      id: 2,
      name: "Afeka Charging Hub (תל אביב)",
      address: "תל אביב",
      distance: "2.4 ק\"מ",
      details: "⏱️ סך הכל 37 דקות.",
      timeEstimate: "",
      rating: "⭐ 4.2",
      distanceBadge: "2.4 ק\"מ"
    },
    {
      id: 3,
      name: "תחנת קניון עזריאלי - חניון תחתון P2",
      address: "דרך מנחם בגין 132, תל אביב",
      distance: "1.2 ק\"מ (3 דקות נסיעה)",
      price: "1.20 ₪ / קו\"ש",
      rating: "⭐ 4.8",
      availableSockets: "Type 2 (22kW) • CCS2 (50kW)"
    }
  ]);

  const handleSelectStation = (stationId: number) => {
    setSelectedStationId(stationId);
    setShowConfirmation(true);
  };

  const currentSelectedStation = stations.find(s => s.id === selectedStationId);

  return (
    <div className="container">
      <div className="screen-view">
        
        <div className="screen-header">
          <span>תוצאות מומלצות לפי האלגוריתם</span>
        </div>

        {/* סרגל סינון ומיון */}
        <div className="filters-bar" style={{ display: 'flex', gap: '8px', flexWrap: 'wrap', marginBottom: '15px', padding: '12px', background: '#121212', borderRadius: '10px', border: '1px solid var(--card-border)', alignItems: 'center' }}>
          <span style={{ fontWeight: 'bold', fontSize: '0.85rem', color: 'var(--text-main)' }}>🔍 סנן/מיין לפי:</span>
          {['הכל', '⚡ פנויות כעת בלבד', '🚀 טעינה אולטרה-מהירה (150kW+)', '⭐ דירוג 4.5 ומעלה'].map((filter) => (
            <div 
              key={filter}
              className={`filter-chip ${activeFilter === filter ? 'active' : ''}`}
              onClick={() => setActiveFilter(filter)}
              style={{
                padding: '6px 12px',
                background: activeFilter === filter ? 'var(--accent-blue)' : 'var(--card-bg)',
                border: '1px solid var(--card-border)',
                borderRadius: '20px',
                fontSize: '0.85rem',
                cursor: 'pointer',
                color: activeFilter === filter ? 'white' : 'var(--text-muted)',
                fontWeight: activeFilter === filter ? 'bold' : 'normal'
              }}
            >
              {filter}
            </div>
          ))}
        </div>

        {/* רשימת התוצאות - ההדגשה הירוקה עוקבת אחרי selectedStationId */}
        <div>
          {stations.map((st) => {
            const isSelected = st.id === selectedStationId;

            return (
              <div 
                key={st.id} 
                style={{
                  background: isSelected ? '#062f22' : '#121212',
                  border: isSelected ? '2px solid var(--primary)' : '1px solid var(--card-border)',
                  borderRadius: '12px',
                  padding: '16px',
                  marginBottom: '16px',
                  transition: 'all 0.3s ease'
                }}
              >
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '6px' }}>
                  <strong style={{ fontSize: '1.1rem', color: isSelected ? 'var(--primary)' : 'var(--text-main)' }}>
                    {isSelected ? `🌟 עמדה נבחרת: ${st.name}` : st.name}
                  </strong>
                  <div style={{ display: 'flex', gap: '8px', alignItems: 'center' }}>
                    <span style={{ color: 'var(--yellow)', fontWeight: 'bold' }}>{st.rating}</span>
                    {st.matchBadge && <span className="badge badge-green" style={{ background: '#064e3b', color: 'var(--primary)', padding: '4px 10px', borderRadius: '20px', fontSize: '0.8rem' }}>{st.matchBadge}</span>}
                    {st.distanceBadge && <span className="badge badge-blue" style={{ background: '#075985', color: 'var(--light-blue)', padding: '4px 10px', borderRadius: '20px', fontSize: '0.8rem' }}>{st.distanceBadge}</span>}
                  </div>
                </div>

                {st.details && (
                  <p style={{ margin: '6px 0', fontSize: '0.9rem', color: 'var(--text-muted)' }}>
                    {st.details}
                  </p>
                )}

                {st.price && (
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', margin: '8px 0', fontSize: '0.9rem' }}>
                    <span style={{ color: 'var(--primary)', fontWeight: 'bold' }}>💳 {st.price}</span>
                  </div>
                )}

                {st.availableSockets && (
                  <div style={{ fontSize: '0.8rem', color: 'var(--light-blue)', marginBottom: '8px' }}>
                    🔌 שקעים זמינים: {st.availableSockets}
                  </div>
                )}

                {st.timeEstimate && (
                  <div style={{ fontSize: '0.85rem', color: 'var(--light-blue)', marginBottom: '12px' }}>
                    {st.timeEstimate}
                  </div>
                )}

                <button 
                  style={{ 
                    padding: '10px 20px', 
                    fontSize: '0.9rem',
                    background: isSelected ? 'var(--accent-blue)' : '#333',
                    border: 'none',
                    borderRadius: '8px',
                    color: 'white',
                    fontWeight: 'bold',
                    marginTop: '8px',
                    width: '100%',
                    cursor: 'pointer'
                  }}
                  onClick={() => handleSelectStation(st.id)}
                >
                  {isSelected ? '✓ עמדה זו נבחרה ושוריינה' : 'בחר עמדה זו ועבור לניווט ושריון ➔'}
                </button>
              </div>
            );
          })}
        </div>

        {/* הודעת אישור שריון מעודכנת */}
        {showConfirmation && currentSelectedStation && (
          <div style={{
            marginTop: '20px',
            background: '#14532d',
            border: '1px solid var(--primary)',
            borderRadius: '12px',
            padding: '16px',
            textAlign: 'center'
          }}>
            <h4 style={{ margin: '0 0 8px 0', color: 'white' }}>
              🎉 השקע שוריין בהצלחה ב-{currentSelectedStation.name}!
            </h4>
            <p style={{ margin: '0 0 12px 0', fontSize: '0.85rem', color: '#bbf7d0' }}>
              השקע שמור עבורך ל-15 הדקות הקרובות. העמדה מוכנה להגעתך!
            </p>
            <button 
              className="btn" 
              style={{ background: 'var(--primary)', color: 'black', width: 'auto', padding: '6px 16px', fontWeight: 'bold' }}
              onClick={() => setShowConfirmation(false)}
            >
              סגור אישור
            </button>
          </div>
        )}

      </div>
    </div>
  );
}