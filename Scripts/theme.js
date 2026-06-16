// ============================================================
// CyberShield Academy — theme.js
// Persists dark/light mode via localStorage
// ============================================================

(function () {
    var STORAGE_KEY = 'csa_theme';

    // Apply saved theme immediately on load (before paint)
    var saved = localStorage.getItem(STORAGE_KEY);
    if (saved === 'light') {
        document.body.classList.add('light');
    }

    window.toggleTheme = function () {
        var isLight = document.body.classList.toggle('light');
        localStorage.setItem(STORAGE_KEY, isLight ? 'light' : 'dark');
        updateThemeBtn(isLight);
    };

    function updateThemeBtn(isLight) {
        var btn = document.getElementById('themeBtn');
        if (!btn) return;
        btn.innerHTML = isLight
            ? '<i class="ti ti-moon" aria-hidden="true"></i>'
            : '<i class="ti ti-sun"  aria-hidden="true"></i>';
        btn.setAttribute('title', isLight ? 'Switch to dark mode' : 'Switch to light mode');
    }

    // Set correct icon once DOM is ready
    document.addEventListener('DOMContentLoaded', function () {
        updateThemeBtn(document.body.classList.contains('light'));
    });
})();
