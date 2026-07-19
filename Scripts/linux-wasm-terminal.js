// Real Linux terminal in the browser: boots a Buildroot Linux kernel inside
// the v86 WebAssembly PC emulator and wires its serial console (ttyS0) to an
// xterm.js terminal. Nothing runs on the server — the whole OS runs client-side.
//
// Persistence: when #termRoot has a data-savekey attribute, the full machine
// state (filesystem, history, everything) is saved gzip-compressed to the
// server (data-stateurl -> TerminalState.ashx), keyed to the signed-in user,
// and restored automatically on the next visit — from any browser or device.
// IndexedDB is kept as a local fallback for when the server can't be reached.
//
// Reads config from #termRoot's data-user/host/flag/motd/savekey/stateurl
// attributes and the data-v86base attribute (URL of the folder holding
// libv86/xterm assets). Shared by StartLab.aspx and Lecturer/Labpreview.aspx.

(function () {
    var root = document.getElementById('termRoot');
    if (!root) return;
    var screenEl = document.getElementById('termScreen');

    var USER = root.getAttribute('data-user') || 'student';
    var HOST = root.getAttribute('data-host') || 'cybershield-lab';
    var FLAG = root.getAttribute('data-flag') || 'CSA{explore_the_box}';
    var MOTD = root.getAttribute('data-motd') || 'CyberShield practice shell';
    var BASE = root.getAttribute('data-v86base') || 'Scripts/v86/';
    var SAVEKEY = root.getAttribute('data-savekey') || '';
    var STATEURL = root.getAttribute('data-stateurl') || '';

    var term = new Terminal({
        cols: 80,
        rows: 24,
        cursorBlink: true,
        fontSize: 13,
        fontFamily: "'Cascadia Code','Cascadia Mono',Consolas,monospace",
        theme: {
            background: '#0a0f0e',
            foreground: '#B0E4CC',
            cursor: '#B0E4CC',
            green: '#6FCF97',
            red: '#E24B4A',
            blue: '#5bc8ff'
        }
    });
    term.open(screenEl);
    term.writeln('\x1b[1;32mCyberShield Browser Lab\x1b[0m — real Linux via WebAssembly (v86)');

    // ---- IndexedDB store for saved machine states ----
    var DB_NAME = 'csa-linux-wasm', STORE = 'vmstates';
    function idb() {
        return new Promise(function (resolve, reject) {
            var req = indexedDB.open(DB_NAME, 1);
            req.onupgradeneeded = function () { req.result.createObjectStore(STORE); };
            req.onsuccess = function () { resolve(req.result); };
            req.onerror = function () { reject(req.error); };
        });
    }
    function idbGet(key) {
        return idb().then(function (db) {
            return new Promise(function (resolve, reject) {
                var req = db.transaction(STORE).objectStore(STORE).get(key);
                req.onsuccess = function () { resolve(req.result); };
                req.onerror = function () { reject(req.error); };
            });
        });
    }
    function idbPut(key, value) {
        return idb().then(function (db) {
            return new Promise(function (resolve, reject) {
                var req = db.transaction(STORE, 'readwrite').objectStore(STORE).put(value, key);
                req.onsuccess = function () { resolve(); };
                req.onerror = function () { reject(req.error); };
            });
        });
    }
    function idbDelete(key) {
        return idb().then(function (db) {
            return new Promise(function (resolve, reject) {
                var req = db.transaction(STORE, 'readwrite').objectStore(STORE).delete(key);
                req.onsuccess = function () { resolve(); };
                req.onerror = function () { reject(req.error); };
            });
        });
    }

    var canGzip = typeof CompressionStream !== 'undefined';
    function gzip(buffer) {
        if (!canGzip) return Promise.resolve(new Blob([buffer]));
        var stream = new Blob([buffer]).stream().pipeThrough(new CompressionStream('gzip'));
        return new Response(stream).blob();
    }
    function gunzip(record) {
        if (!record.gz) return record.blob.arrayBuffer();
        var stream = record.blob.stream().pipeThrough(new DecompressionStream('gzip'));
        return new Response(stream).arrayBuffer();
    }
    // States stored on the server are opaque bytes; sniff the gzip magic number
    // to know whether this one needs decompressing.
    function maybeGunzip(buffer) {
        var b = new Uint8Array(buffer);
        if (b.length > 2 && b[0] === 0x1f && b[1] === 0x8b && typeof DecompressionStream !== 'undefined') {
            var stream = new Blob([buffer]).stream().pipeThrough(new DecompressionStream('gzip'));
            return new Response(stream).arrayBuffer();
        }
        return Promise.resolve(buffer);
    }
    function stateUrl() {
        return STATEURL + '?key=' + encodeURIComponent(SAVEKEY);
    }

    // ---- boot (optionally from a previously saved state) ----
    var emulator = null;
    var restored = false;
    var configured = false; // true once the box has been personalised (or restored)

    // Prefer the copy saved on the server (follows the account to any browser);
    // fall back to this browser's IndexedDB copy if the server has none.
    function loadSavedState() {
        if (!SAVEKEY) return Promise.resolve(null);
        var fromServer = STATEURL
            ? fetch(stateUrl(), { credentials: 'same-origin' }).then(function (r) {
                if (r.status !== 200) return null;
                var savedAt = parseInt(r.headers.get('X-CSA-Saved-At'), 10) || Date.now();
                return r.arrayBuffer().then(maybeGunzip).then(function (buf) {
                    return { buffer: buf, savedAt: savedAt };
                });
            }).catch(function () { return null; })
            : Promise.resolve(null);
        return fromServer.then(function (state) {
            if (state) return state;
            return idbGet(SAVEKEY).then(function (record) {
                if (!record || !record.blob) return null;
                return gunzip(record).then(function (buf) {
                    return { buffer: buf, savedAt: record.savedAt };
                });
            }).catch(function () { return null; });
        });
    }

    loadSavedState().then(function (state) {
        if (state) {
            term.writeln('Found a saved session from ' + new Date(state.savedAt).toLocaleString() + ' — restoring...');
            boot(state.buffer);
            return;
        }
        term.writeln('Downloading Linux image (~10 MB on first visit)...');
        boot(null);
    }, function () {
        term.writeln('\x1b[1;31mSaved session could not be read\x1b[0m — starting fresh.');
        boot(null);
    });

    function boot(stateBuffer) {
        restored = !!stateBuffer;
        configured = restored;

        var options = {
            wasm_path: BASE + 'v86.wasm',
            memory_size: 128 * 1024 * 1024,
            vga_memory_size: 2 * 1024 * 1024,
            bios: { url: BASE + 'seabios.bin' },
            vga_bios: { url: BASE + 'vgabios.bin' },
            bzimage: { url: BASE + 'buildroot-bzimage.bin' },
            cmdline: 'tsc=reliable mitigations=off random.trust_cpu=on console=ttyS0',
            disable_speaker: true,
            // All input goes through the serial console (xterm.js), so the
            // emulated PS/2 keyboard and mouse are never used. Left enabled,
            // v86 captures key and mouse events page-wide, which blocked
            // typing in other form fields and the browser's right-click menu.
            disable_keyboard: true,
            disable_mouse: true,
            autostart: true
        };
        if (stateBuffer) options.initial_state = { buffer: stateBuffer };

        emulator = new V86(options);

        var lastPct = -1;
        emulator.add_listener('download-progress', function (e) {
            if (!e.total || e.file_name.indexOf('bzimage') < 0) return;
            var pct = Math.floor(e.loaded / e.total * 100);
            if (pct !== lastPct) {
                lastPct = pct;
                term.write('\r\x1b[2KDownloading Linux image... ' + pct + '%');
            }
        });
        emulator.add_listener('download-error', function (e) {
            term.writeln('\r\n\x1b[1;31mDownload failed:\x1b[0m ' + e.file_name +
                ' — check your connection and reload the page.');
        });
        emulator.add_listener('emulator-started', function () {
            if (restored) {
                term.writeln('\x1b[1;32mSession restored\x1b[0m — your files are as you left them.');
                setTimeout(function () { emulator.serial0_send('\n'); }, 300);
            } else {
                term.writeln('\r\x1b[2KImage loaded — booting the kernel (takes a few seconds)...');
            }
        });

        // serial console -> terminal. Once the first shell prompt of a fresh
        // boot appears, personalise the box with a one-shot setup line.
        var tail = '';
        emulator.add_listener('serial0-output-byte', function (byte) {
            term.write(new Uint8Array([byte]));
            if (configured) return;
            tail += String.fromCharCode(byte);
            if (tail.length > 120) tail = tail.slice(-120);
            if (/[#$%] $/.test(tail)) {
                configured = true;
                setupLab();
            }
        });

        // terminal -> serial console
        term.onData(function (data) {
            emulator.serial0_send(data);
        });

        // Auto-save once a minute so at most ~1 minute of work can be lost.
        if (SAVEKEY) {
            setInterval(function () {
                if (configured) saveSession(true);
            }, 60000);
        }
    }

    function shq(s) {
        return "'" + String(s).replace(/'/g, "'\\''") + "'";
    }
    function setupLab() {
        var ps1 = (USER + '@' + HOST).replace(/['\\]/g, '') + ':\\w\\$ ';
        // The BusyBox image is minimal — add shims so commands students reach
        // for anyway (man, getent, sudo, apt) do something sensible instead
        // of "not found". They live in the filesystem, so they are kept in
        // saved sessions like any other file. Sent as separate lines because
        // the shell's input line buffer is limited (~1 KB).
        var shims = [
            'printf \'#!/bin/sh\\necho "man: no manual pages in this minimal image - try: $1 --help"\\n\' > /usr/bin/man',
            'printf \'#!/bin/sh\\ndb="$1"; shift 2>/dev/null; if [ -z "$db" ]; then echo "usage: getent database [key]"; elif [ ! -f "/etc/$db" ]; then echo "getent: unknown database $db"; elif [ $# -eq 0 ]; then cat "/etc/$db"; else grep "^$1:" "/etc/$db"; fi\\n\' > /usr/bin/getent',
            // Already root in this box, so sudo just runs the command.
            'printf \'#!/bin/sh\\nif [ $# -eq 0 ]; then echo "usage: sudo command"; else exec "$@"; fi\\n\' > /usr/bin/sudo',
            'printf \'#!/bin/sh\\necho "apt: no package manager on this box - it is a fixed minimal Linux image with no internet access, so nothing can be installed"\\n\' > /usr/bin/apt',
            'cp /usr/bin/apt /usr/bin/apt-get',
            'chmod +x /usr/bin/man /usr/bin/getent /usr/bin/sudo /usr/bin/apt /usr/bin/apt-get'
        ];
        var cmds = [
            'hostname ' + shq(HOST),
            "export PS1='" + ps1 + "'",
            'mkdir -p /root/.secret',
            'printf "%s\\n" ' + shq(FLAG) + ' > /root/.secret/flag.txt',
            'printf "%s\\n" ' + shq(MOTD) + ' > /etc/motd',
            'printf "%s\\n" ' + shq('Welcome, ' + USER + '! ' + MOTD +
                ' This is a real Linux shell. A flag is hidden somewhere on this box — try ls -a.') +
                ' > /root/welcome.txt',
            'cd /root',
            'clear',
            'cat /etc/motd'
        ];
        shims.forEach(function (line) { emulator.serial0_send(line + '\n'); });
        emulator.serial0_send(cmds.join(' && ') + '\n');
    }

    // ---- save / reset ----
    var saving = false;
    function saveSession(silent) {
        if (!SAVEKEY || !emulator || !configured || saving) return;
        saving = true;
        if (!silent) term.writeln('\r\n\x1b[1;33mSaving session...\x1b[0m');
        return emulator.save_state()
            .then(gzip)
            .then(function (blob) {
                // Local copy first (fast, works offline), then the copy that
                // matters: the server one tied to the account.
                var local = idbPut(SAVEKEY, { blob: blob, gz: canGzip, savedAt: Date.now() })
                    .catch(function () { });
                if (!STATEURL) return local;
                return local.then(function () {
                    return fetch(stateUrl(), { method: 'POST', credentials: 'same-origin', body: blob });
                }).then(function (r) {
                    if (!r.ok) throw new Error('server replied HTTP ' + r.status);
                });
            })
            .then(function () {
                if (!silent) {
                    term.writeln('\x1b[1;32mSession saved to your account.\x1b[0m It will be restored next time you sign in and open this lab. Press Enter to continue.');
                }
            })
            .catch(function (err) {
                if (!silent) term.writeln('\x1b[1;31mSaving to the server failed\x1b[0m (' + err + ') — kept a copy in this browser only.');
            })
            .then(function () { saving = false; });
    }
    // Reboot the terminal from the last save without touching the files:
    // save what's there now, then reload the page (which restores it). If the
    // machine is hung and the save never finishes, reload anyway after 6 s.
    function reloadSession() {
        var done = false;
        function go() { if (!done) { done = true; location.reload(); } }
        setTimeout(go, 6000);
        var p = saveSession(true);
        if (p && p.then) p.then(go, go); else go();
    }
    function resetSession() {
        if (!SAVEKEY) { location.reload(); return; }
        if (!confirm('Discard your saved session and start this lab fresh?')) return;
        var delServer = STATEURL
            ? fetch(stateUrl() + '&delete=1', { method: 'POST', credentials: 'same-origin' }).catch(function () { })
            : Promise.resolve();
        Promise.all([delServer, idbDelete(SAVEKEY).catch(function () { })])
            .then(function () { location.reload(); });
    }

    // Best-effort save when the tab is hidden (switching tabs, closing, or
    // navigating to the logout page) so recent work isn't lost to the
    // once-a-minute autosave window.
    document.addEventListener('visibilitychange', function () {
        if (document.visibilityState === 'hidden') saveSession(true);
    });

    screenEl.addEventListener('click', function () { term.focus(); });

    // Page-header buttons use this; keep the old API name.
    window.csaTerm = {
        clear: function () { term.clear(); },
        save: function () { saveSession(false); },
        reload: reloadSession,
        reset: resetSession,
        get emulator() { return emulator; },
        term: term
    };
})();
