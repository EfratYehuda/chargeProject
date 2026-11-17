using System;
using System.Collections.Generic;

namespace BLL.Algorithm
{
    /// <summary>
    /// Merge Sort (מיון מיזוג) – מימוש עצמי, רקורסיבי, "הפרד ומשול".
    ///
    /// הרעיון:
    ///   1. הפרד  – מחלקים את המערך לשני חצאים.
    ///   2. משול  – ממיינים כל חצי (באותה שיטה, ברקורסיה).
    ///   3. מזג   – ממזגים שני חצאים ממוינים למערך ממוין אחד.
    ///   תנאי עצירה: מערך באורך 0 או 1 כבר ממוין.
    ///
    /// סיבוכיות זמן: O(n log n) בכל מקרה (טוב, ממוצע וגרוע).
    /// סיבוכיות מקום: O(n) – מערך עזר אחד בגודל n.
    /// יציב (Stable): איברים שווים שומרים על הסדר המקורי שלהם.
    /// </summary>
    public static class MergeSorter
    {
        /// <summary>
        /// ממיין רשימה ומחזיר רשימה חדשה ממוינת.
        /// compare(a, b) &lt; 0  ⇒ a צריך לבוא לפני b.
        /// כך אותה פונקציה ממיינת גם בסדר עולה וגם בסדר יורד – רק משנים את compare.
        /// </summary>
        public static List<T> Sort<T>(List<T> items, Comparison<T> compare)
        {
            T[] arr = items.ToArray();          // עובדים על מערך – גישה לפי אינדקס ב-O(1)
            T[] temp = new T[arr.Length];       // מערך עזר אחד לכל המיזוגים (במקום ליצור חדש בכל שלב)

            SortRange(arr, temp, 0, arr.Length - 1, compare);

            return new List<T>(arr);
        }

        /// <summary>ממיין את הקטע arr[left..right] (כולל שני הקצוות)</summary>
        private static void SortRange<T>(T[] arr, T[] temp, int left, int right, Comparison<T> compare)
        {
            // תנאי עצירה: קטע של איבר אחד (או ריק) כבר ממוין
            if (left >= right) return;

            // 1. הפרד: נקודת האמצע (כתיבה שמונעת גלישה של int במערכים ענקיים)
            int mid = left + (right - left) / 2;

            // 2. משול: ממיינים כל חצי ברקורסיה
            SortRange(arr, temp, left, mid, compare);
            SortRange(arr, temp, mid + 1, right, compare);

            // 3. מזג: מאחדים את שני החצאים הממוינים
            Merge(arr, temp, left, mid, right, compare);
        }

        /// <summary>
        /// ממזג שני קטעים ממוינים: arr[left..mid] ו-arr[mid+1..right]
        /// בכל צעד משווים את האיבר הראשון שנשאר בכל חצי, ולוקחים את "המנצח".
        /// </summary>
        private static void Merge<T>(T[] arr, T[] temp, int left, int mid, int right, Comparison<T> compare)
        {
            int i = left;      // מצביע לחצי השמאלי
            int j = mid + 1;   // מצביע לחצי הימני
            int k = left;      // מצביע למקום הבא במערך העזר

            // כל עוד יש איברים בשני החצאים – לוקחים את הקטן (לפי compare)
            while (i <= mid && j <= right)
            {
                // "<= 0" ולא "< 0": בשוויון לוקחים קודם מהשמאלי ⇒ המיון יציב
                if (compare(arr[i], arr[j]) <= 0)
                    temp[k++] = arr[i++];
                else
                    temp[k++] = arr[j++];
            }

            // אחד החצאים נגמר – מעתיקים את מה שנשאר בשני (הוא כבר ממוין)
            while (i <= mid) temp[k++] = arr[i++];
            while (j <= right) temp[k++] = arr[j++];

            // מחזירים את הקטע הממוזג מהמערך העזר למערך המקורי
            for (int x = left; x <= right; x++)
                arr[x] = temp[x];
        }
    }
}