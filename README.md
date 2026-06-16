# CyberShield Academy — Web Forms File Reference

## Project Structure

```
CyberShieldAcademy/
├── Web.config                      # DB connection string, auth settings
├── Site.Master                     # Master page (nav, theme toggle, footer)
├── Site.Master.cs                  # Code-behind: role-based nav visibility
│
├── Default.aspx                    # Home / Landing page
├── Default.aspx.cs
├── Login.aspx                      # Sign In form
├── Login.aspx.cs
├── Register.aspx                   # Registration form
├── Register.aspx.cs
├── Courses.aspx                    # Public course catalogue
├── Courses.aspx.cs
│
├── Student/
│   ├── Student_Dashboard.aspx              # Student dashboard (metrics, courses, activity)
│   ├── Student_Dashboard.aspx.cs
│   ├── MyCourses.aspx              # (to build)
│   ├── Labs.aspx                   # (to build)
│   ├── Challenges.aspx             # (to build)
│   ├── Analytics.aspx              # (to build)
│   ├── Certificates.aspx           # (to build)
│   ├── Achievements.aspx           # (to build)
│   └── Profile.aspx                # (to build)
│
├── Admin/
│   ├── Admin_Dashboard.aspx              # Admin dashboard (users table, metrics)
│   ├── Admin_Dashboard.aspx.cs
│   ├── Users.aspx                  # (to build)
│   ├── Courses.aspx                # (to build)
│   ├── ContentReview.aspx          # (to build)
│   ├── ActivityLogs.aspx           # (to build)
│   ├── Announcements.aspx          # (to build)
│   ├── Backup.aspx                 # (to build)
│   └── SecurityAlerts.aspx         # (to build)
│
├── Styles/
│   └── Site.css                    # Full CSS design system (dark + light themes)
│
├── Scripts/
│   └── theme.js                    # Dark/light mode toggle (localStorage)
│
└── Images/
    └── logo.png                    # Drop your CyberShield logo here
```

## Session Variables Used
| Key        | Type   | Set in       | Description              |
|------------|--------|--------------|--------------------------|
| UserID     | int    | Login.aspx   | Logged-in user's DB ID   |
| Role       | string | Login.aspx   | "Student" or "Admin"     |
| FullName   | string | Login.aspx   | Display name             |
| Email      | string | Login.aspx   | User email               |

## TODO: Back-end Integration
Search for `// TODO:` comments in each `.aspx.cs` file.
Each marks a point to wire in your DB service layer.

## Dark Mode
- Default: dark mode on load
- Toggle: ☀️/🌙 button in top-right nav
- Persisted via `localStorage` key `csa_theme`
- CSS variables in `Site.css` under `:root` (dark) and `body.light` (light)
