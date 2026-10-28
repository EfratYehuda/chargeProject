import React from 'react';
import StationOwnerScreen from './components/StationOwnerScreen';
import './App.css';

export default function App() {
  return (
    <div className="container" dir="rtl">
      <header>
        <div>
          <h1>⚡ Smart EV Charging</h1>
          <p>מערכת חכמה לחיפוש עמדות, שריון שקע וניהול קהילתי</p>
        </div>
      </header>

      {/* הצגת מסך פאנל בעל תחנה בלבד - ללא מנגנון ניווט */}
      <StationOwnerScreen />
    </div>
  );
}