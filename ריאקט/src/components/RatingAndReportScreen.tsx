import React, { useState } from 'react';

export default function RatingAndReportScreen() {
  const [rating, setRating] = useState<number>(5);
  const [hoverRating, setHoverRating] = useState<number>(0);
  const [reviewText, setReviewText] = useState<string>('');
  const [selectedIssue, setSelectedIssue] = useState<string>('שקע טעינה אינו מגיב');
  const [isSubmitted, setIsSubmitted] = useState<boolean>(false);
  const [earnedPoints, setEarnedPoints] = useState<number>(10);

  const handleSubmitRating = () => {
    setEarnedPoints(10);
    setIsSubmitted(true);
  };

  const handleSubmitReport = () => {
    setEarnedPoints(20);
    setIsSubmitted(true);
  };

  if (isSubmitted) {
    return (
      <div className="container">
        <div className="screen-view" style={{ textAlign: 'center', padding: '40px 20px' }}>
          <div style={{ fontSize: '3rem', marginBottom: '10px' }}>⚡ 🎉</div>
          <h2 style={{ color: 'var(--light-blue)', fontSize: '1.8rem', marginBottom: '10px' }}>
            תודה רבה! נתראה בפעם הבאה!
          </h2>
          <p style={{ color: 'var(--text-muted)', marginBottom: '30px' }}>
            המשוב שלך מסייע לכל הקהילה למצוא עמדות אמינות ופנויות בזמן אמת.
          </p>

          <div style={{ 
            background: '#062f22', 
            border: '2px solid var(--primary)', 
            borderRadius: '16px', 
            padding: '20px', 
            maxWidth: '400px', 
            margin: '0 auto 30px auto' 
          }}>
            <span style={{ color: 'var(--text-main)', fontSize: '1rem', display: 'block', marginBottom: '8px' }}>
              תגמול קהילה התווסף לחשבונך!
            </span>
            <span style={{ color: 'var(--primary)', fontSize: '1.6rem', fontWeight: 'bold' }}>
              ⭐ +{earnedPoints} נקודות קהילה!
            </span>
          </div>

          <button 
            className="btn" 
            style={{ maxWidth: '400px', margin: '0 auto', padding: '14px', fontSize: '1rem' }}
            onClick={() => setIsSubmitted(false)}
          >
            🏠 חזרה למסך הבית / חיפוש חדש
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="container">
      <div className="screen-view">
        
        <div className="screen-header">
          <span>סיכום טעינה, דירוג ודיווח תקלות</span>
        </div>

        <div style={{ maxWidth: '550px', margin: '0 auto' }}>
          
          {/* קופסת דירוג */}
          <div style={{ background: '#121212', padding: '24px', borderRadius: '16px', border: '1px solid var(--card-border)', marginBottom: '20px', textAlign: 'center' }}>
            <h3 style={{ marginTop: 0, color: 'var(--text-main)', fontSize: '1.1rem', marginBottom: '15px' }}>
              ⭐ הטרמינל פנוי? דרג את חוויות הטעינה:
            </h3>

            {/* כוכבים אינטראקטיביים עם אנימציה */}
            <div style={{ display: 'flex', justifyContent: 'center', gap: '8px', marginBottom: '20px', direction: 'ltr' }}>
              {[1, 2, 3, 4, 5].map((star) => (
                <span 
                  key={star}
                  style={{ 
                    fontSize: '2rem', 
                    cursor: 'pointer', 
                    color: (hoverRating || rating) >= star ? 'var(--yellow)' : '#4b5563',
                    transition: 'transform 0.2s ease, color 0.2s ease',
                    transform: (hoverRating || rating) >= star ? 'scale(1.15)' : 'scale(1)'
                  }}
                  onMouseEnter={() => setHoverRating(star)}
                  onMouseLeave={() => setHoverRating(0)}
                  onClick={() => setRating(star)}
                >
                  ★
                </span>
              ))}
            </div>

            <div className="form-group" style={{ textAlign: 'right', marginBottom: '15px' }}>
              <label>חוות דעת קצרה:</label>
              <input 
                type="text" 
                className="form-control" 
                placeholder="לדוגמה: עמדה מהירה, נקייה ופנויה." 
                value={reviewText}
                onChange={(e) => setReviewText(e.target.value)}
              />
            </div>

            <button 
              className="btn" 
              style={{ width: '100%', padding: '12px' }}
              onClick={handleSubmitRating}
            >
              שלח דירוג וסיום נסיעה 📥
            </button>
          </div>

          {/* קופסת דיווח תקלות (אדומה ומודגשת) */}
          <div style={{ background: '#180505', padding: '24px', borderRadius: '16px', border: '1.5px solid var(--danger)', textAlign: 'right' }}>
            <h4 style={{ marginTop: 0, color: '#f87171', fontSize: '1.05rem', marginBottom: '15px', display: 'flex', alignItems: 'center', gap: '8px' }}>
              ⚠️ זיהית תקלה בעמדה? דווח לנו (וקבל 20 נקודות בונוס!):
            </h4>

            <div className="form-group" style={{ marginBottom: '15px' }}>
              <label style={{ color: '#fca5a5' }}>סוג התקלה:</label>
              <select 
                className="form-control" 
                value={selectedIssue}
                onChange={(e) => setSelectedIssue(e.target.value)}
                style={{ background: '#121212', borderColor: '#7f1d1d', color: 'white' }}
              >
                <option value="שקע טעינה אינו מגיב">שקע טעינה אינו מגיב</option>
                <option value="כבל קרוע או פגום">כבל קרוע או פגום</option>
                <option value="העמדה תפוסה פיזית אך מופיעה כפנויה">העמדה תפוסה פיזית אך מופיעה כפנויה</option>
                <option value="תקלת תקשורת / אפליקציה">תקלת תקשורת / אפליקציה</option>
              </select>
            </div>

            <button 
              className="btn" 
              style={{ background: 'var(--danger)', width: '100%', padding: '12px', fontWeight: 'bold' }}
              onClick={handleSubmitReport}
            >
              🚨 דווח על תקלה וסיום נסיעה 🎊
            </button>
          </div>

        </div>

      </div>
    </div>
  );
}