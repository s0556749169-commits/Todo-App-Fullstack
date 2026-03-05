# 📝 Fullstack To-Do List Application

אפליקציית ניהול משימות (To-Do List) מקצה לקצה, המאפשרת למשתמשים להירשם, להתחבר ולנהל את המשימות האישיות שלהם בצורה מאובטחת.

## 🚀 תכונות עיקריות
- **מערכת אימות (Authentication):** הרשמה והתחברות משתמשים באמצעות **JWT Token**.
- **ניהול משימות אישי:** כל משתמש רואה, מוסיף ומוחק רק את המשימות המשויכות אליו.
- **צד שרת (Backend):** נבנה ב-ASP.NET Core Web API עם חיבור למסד נתונים MySQL.
- **צד לקוח (Frontend):** ממשק משתמש דינמי שנבנה ב-React.
- **אבטחה:** הגדרת מדיניות CORS ומפתחות הצפנה לטוקנים.

## 🛠 טכנולוגיות שבהן השתמשתי
- **Client Side:** React, Axios, CSS3.
- **Server Side:** .NET 8 / ASP.NET Core API.
- **Database:** MySQL, Entity Framework Core.
- **Auth:** JSON Web Tokens (JWT).

## 📋 מבנה מסד הנתונים
כדי שהפרויקט יעבוד, יש להקים ב-MySQL שתי טבלאות המקושרות ביניהן (One-to-Many):
1. **Users** - שמירת פרטי המשתמשים.
2. **Items** - שמירת המשימות עם שדה `UserId` המקשר למשתמש שיצר אותן.



## 💻 הוראות הפעלה

### הגדרת ה-API (Server)
1. נווט לתיקיית `TodoApi`.
2. וודא שקובץ ה-`ConnectionString` בתוך ה-`DbContext` מעודכן עם פרטי ה-MySQL שלך.
3. הרץ את הפקודה:
   ```bash
   dotnet run
