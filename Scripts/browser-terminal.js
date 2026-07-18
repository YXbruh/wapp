// Self-contained in-browser Linux terminal (no server execution).
// Reads config from a #termRoot element's data-user/host/flag/motd attributes,
// and drives #termScreen / #termInput / #termPrompt. Shared by StartLab.aspx
// and Lecturer/Labpreview.aspx.

(function () {
    var root = document.getElementById('termRoot');
    if (!root) return;
    var screenEl = document.getElementById('termScreen');
    var inputEl = document.getElementById('termInput');
    var promptEl = document.getElementById('termPrompt');

    var USER = root.getAttribute('data-user') || 'student';
    var HOST = root.getAttribute('data-host') || 'cybershield-lab';
    var FLAG = root.getAttribute('data-flag') || 'CSA{explore_the_box}';
    var MOTD = root.getAttribute('data-motd') || 'CyberShield practice shell';

    // ---- virtual filesystem: objects are directories, strings are files ----
    var fs = {
        home: {},
        etc: {
            hostname: HOST + '\n',
            'os-release': 'PRETTY_NAME="CyberShield Lab (browser)"\nID=cslab\nVERSION="1.0"\n',
            passwd: 'root:x:0:0:root:/root:/bin/bash\n' + USER + ':x:1000:1000:' + USER + ':/home/' + USER + ':/bin/bash\n'
        },
        tmp: {},
        bin: {}
    };
    fs.home[USER] = {
        'welcome.txt': 'Welcome, ' + USER + '!\n' + MOTD + '\nType `help` for the command list. A flag is hidden somewhere on this box.\n',
        'README.md': '# ' + MOTD + '\n\nGoals:\n- explore the filesystem (ls, cd, cat)\n- reveal hidden files (ls -a)\n- read /etc\n- find the flag\n',
        notes: {
            'todo.txt': '- enumerate the machine\n- check hidden files\n- read the man pages\n'
        },
        '.secret': {
            'flag.txt': FLAG + '\n'
        }
    };

    var HOME = ['home', USER];
    var cwd = HOME.slice();
    var history = [];
    var histIdx = 0;

    // ---- helpers ----
    function isDir(node) { return node && typeof node === 'object'; }
    function getNode(parts) {
        var node = fs;
        for (var i = 0; i < parts.length; i++) {
            if (!isDir(node)) return undefined;
            node = node[parts[i]];
            if (node === undefined) return undefined;
        }
        return node;
    }
    function resolve(input) {
        var parts;
        if (input === undefined || input === '') return cwd.slice();
        if (input === '~' || input.indexOf('~/') === 0) {
            parts = HOME.concat(input === '~' ? [] : input.slice(2).split('/'));
        } else if (input.charAt(0) === '/') {
            parts = input.split('/');
        } else {
            parts = cwd.concat(input.split('/'));
        }
        var out = [];
        for (var i = 0; i < parts.length; i++) {
            var p = parts[i];
            if (p === '' || p === '.') continue;
            if (p === '..') { if (out.length) out.pop(); continue; }
            out.push(p);
        }
        return out;
    }
    function pathStr(parts) {
        if (parts.length >= 2 && parts[0] === 'home' && parts[1] === USER) {
            var rest = parts.slice(2);
            return '~' + (rest.length ? '/' + rest.join('/') : '');
        }
        return '/' + parts.join('/');
    }
    function pad(v, n) { v = String(v); while (v.length < n) v = ' ' + v; return v; }
    function esc(s) {
        return String(s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
    }
    function print(html) {
        var d = document.createElement('div');
        d.innerHTML = html === '' ? '&nbsp;' : html;
        screenEl.appendChild(d);
        screenEl.scrollTop = screenEl.scrollHeight;
    }
    function printText(t) { print(esc(t)); }
    function printErr(t) { print('<span class="err">' + esc(t) + '</span>'); }
    function updatePrompt() { promptEl.textContent = USER + '@' + HOST + ':' + pathStr(cwd) + '$'; }

    // ---- commands ----
    var manpages = {
        ls: 'ls [-a] [-l] [path] — list directory contents',
        cd: 'cd [path] — change the working directory',
        pwd: 'pwd — print the working directory',
        cat: 'cat FILE... — print file contents',
        echo: 'echo TEXT [> file] — print text, optionally to a file',
        grep: 'grep PATTERN FILE — print matching lines',
        find: 'find [path] — list everything under a path',
        whoami: 'whoami — print the current user'
    };

    var commands = {
        help: function () {
            printText('Available commands:');
            printText('  ls  cd  pwd  cat  echo  grep  find  head  tail  wc');
            printText('  whoami  id  hostname  uname  date  history  man  clear');
            printText('  mkdir  touch  rm  sudo  exit');
            printText('Hidden files start with a dot — reveal them with: ls -a');
        },
        ls: function (args) {
            var all = false, long = false, target;
            args.forEach(function (a) {
                if (a.charAt(0) === '-') { if (a.indexOf('a') >= 0) all = true; if (a.indexOf('l') >= 0) long = true; }
                else target = a;
            });
            var parts = resolve(target);
            var node = getNode(parts);
            if (node === undefined) { printErr("ls: cannot access '" + target + "': No such file or directory"); return; }
            if (!isDir(node)) { printText(target); return; }
            var names = Object.keys(node).filter(function (n) { return all || n.charAt(0) !== '.'; });
            if (all) names = ['.', '..'].concat(names);
            names.sort();
            if (!names.length) return;
            if (long) {
                names.forEach(function (n) {
                    var child = (n === '.' || n === '..') ? {} : node[n];
                    var d = isDir(child);
                    var size = d ? 4096 : String(child).length;
                    var perm = d ? 'drwxr-xr-x' : '-rw-r--r--';
                    var nm = d ? '<span class="dir">' + esc(n) + '</span>' : esc(n);
                    print(esc(perm + ' 1 ' + USER + ' ' + USER + ' ' + pad(size, 6) + ' Jan  1 00:00 ') + nm);
                });
            } else {
                var html = names.map(function (n) {
                    var d = (n === '.' || n === '..') ? true : isDir(node[n]);
                    return d ? '<span class="dir">' + esc(n) + '</span>' : esc(n);
                });
                print(html.join('  '));
            }
        },
        cd: function (args) {
            var target = args[0] === undefined ? '~' : args[0];
            var parts = resolve(target);
            var node = getNode(parts);
            if (node === undefined) { printErr('cd: ' + target + ': No such file or directory'); return; }
            if (!isDir(node)) { printErr('cd: ' + target + ': Not a directory'); return; }
            cwd = parts; updatePrompt();
        },
        pwd: function () { printText(pathStr(cwd).replace('~', '/home/' + USER)); },
        cat: function (args) {
            if (!args.length) { printErr('cat: missing operand'); return; }
            args.forEach(function (a) {
                var node = getNode(resolve(a));
                if (node === undefined) { printErr('cat: ' + a + ': No such file or directory'); return; }
                if (isDir(node)) { printErr('cat: ' + a + ': Is a directory'); return; }
                printText(String(node).replace(/\n$/, ''));
            });
        },
        echo: function (args) {
            var redir = null, file = null, out = [];
            for (var i = 0; i < args.length; i++) {
                if (args[i] === '>' || args[i] === '>>') { redir = args[i]; file = args[i + 1]; break; }
                out.push(args[i]);
            }
            var text = out.join(' ');
            if (!redir) { printText(text); return; }
            if (!file) { printErr('echo: missing redirect target'); return; }
            var parts = resolve(file), name = parts.pop(), parent = getNode(parts);
            if (!isDir(parent)) { printErr('echo: ' + file + ': No such file or directory'); return; }
            var prev = (redir === '>>' && typeof parent[name] === 'string') ? parent[name] : '';
            parent[name] = prev + text + '\n';
        },
        mkdir: function (args) {
            if (!args.length) { printErr('mkdir: missing operand'); return; }
            var parts = resolve(args[0]), name = parts.pop(), parent = getNode(parts);
            if (!isDir(parent)) { printErr('mkdir: cannot create directory: No such file or directory'); return; }
            if (parent[name] !== undefined) { printErr("mkdir: cannot create directory '" + args[0] + "': File exists"); return; }
            parent[name] = {};
        },
        touch: function (args) {
            if (!args.length) { printErr('touch: missing operand'); return; }
            var parts = resolve(args[0]), name = parts.pop(), parent = getNode(parts);
            if (!isDir(parent)) { printErr('touch: cannot touch: No such file or directory'); return; }
            if (parent[name] === undefined) parent[name] = '';
        },
        rm: function (args) {
            var rec = false, targets = [];
            args.forEach(function (a) { if (a.charAt(0) === '-') { if (a.indexOf('r') >= 0) rec = true; } else targets.push(a); });
            if (!targets.length) { printErr('rm: missing operand'); return; }
            targets.forEach(function (t) {
                var parts = resolve(t), name = parts.pop(), parent = getNode(parts);
                if (!isDir(parent) || parent[name] === undefined) { printErr('rm: cannot remove ' + t + ': No such file or directory'); return; }
                if (isDir(parent[name]) && !rec) { printErr('rm: cannot remove ' + t + ': Is a directory'); return; }
                delete parent[name];
            });
        },
        grep: function (args) {
            if (args.length < 2) { printErr('usage: grep PATTERN FILE'); return; }
            var node = getNode(resolve(args[1]));
            if (node === undefined) { printErr('grep: ' + args[1] + ': No such file or directory'); return; }
            if (isDir(node)) { printErr('grep: ' + args[1] + ': Is a directory'); return; }
            var re; try { re = new RegExp(args[0]); } catch (e) { re = { test: function (l) { return l.indexOf(args[0]) >= 0; } }; }
            String(node).split('\n').forEach(function (l) { if (l && re.test(l)) printText(l); });
        },
        find: function (args) {
            var start = resolve(args[0] || '.'), base = getNode(start);
            if (base === undefined) { printErr('find: no such path'); return; }
            (function walk(parts, node) {
                printText(pathStr(parts));
                if (isDir(node)) Object.keys(node).forEach(function (n) { walk(parts.concat(n), node[n]); });
            })(start, base);
        },
        head: function (args) { lines(args, function (arr, n) { return arr.slice(0, n); }); },
        tail: function (args) { lines(args, function (arr, n) { return arr.slice(-n); }); },
        wc: function (args) {
            var node = getNode(resolve(args[0]));
            if (node === undefined || isDir(node)) { printErr('wc: ' + args[0] + ': No such file'); return; }
            var s = String(node), l = s.split('\n').length - 1, w = (s.match(/\S+/g) || []).length;
            printText(pad(l, 7) + pad(w, 8) + pad(s.length, 8) + ' ' + args[0]);
        },
        whoami: function () { printText(USER); },
        id: function () { printText('uid=1000(' + USER + ') gid=1000(' + USER + ') groups=1000(' + USER + ')'); },
        hostname: function () { printText(HOST); },
        uname: function (args) {
            if (args.indexOf('-a') >= 0) printText('Linux ' + HOST + ' 6.6.0-cslab #1 SMP x86_64 GNU/Linux');
            else printText('Linux');
        },
        date: function () { printText(new Date().toUTCString()); },
        history: function () { history.forEach(function (h, i) { printText(pad(i + 1, 4) + '  ' + h); }); },
        man: function (args) {
            if (!args.length) { printErr('What manual page do you want?'); return; }
            printText(manpages[args[0]] || 'No manual entry for ' + args[0]);
        },
        sudo: function () { printErr(USER + ' is not in the sudoers file. This incident will be reported.'); },
        clear: function () { csaTerm.clear(); },
        exit: function () { printText('logout — reload the page to start a fresh session.'); }
    };

    function lines(args, pick) {
        var n = 10, f;
        for (var i = 0; i < args.length; i++) { if (args[i] === '-n') { n = parseInt(args[++i], 10) || 10; } else f = args[i]; }
        var node = getNode(resolve(f));
        if (node === undefined || isDir(node)) { printErr('cannot open ' + f); return; }
        var arr = String(node).replace(/\n$/, '').split('\n');
        pick(arr, n).forEach(printText);
    }

    // ---- input handling ----
    function runLine(line) {
        print('<span class="cmd">' + esc(USER + '@' + HOST + ':' + pathStr(cwd) + '$ ') + '</span>' + esc(line));
        var trimmed = line.trim();
        if (trimmed !== '') history.push(line);
        histIdx = history.length;
        if (trimmed === '') return;
        var toks = trimmed.match(/(?:[^\s"]+|"[^"]*")+/g) || [];
        toks = toks.map(function (t) { return t.replace(/^"|"$/g, ''); });
        var cmd = toks[0], rest = toks.slice(1);
        if (commands[cmd]) { try { commands[cmd](rest); } catch (e) { printErr(cmd + ': ' + e.message); } }
        else printErr(cmd + ': command not found');
    }

    inputEl.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            var v = inputEl.value; inputEl.value = '';
            runLine(v);
        } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            if (histIdx > 0) { histIdx--; inputEl.value = history[histIdx] || ''; }
        } else if (e.key === 'ArrowDown') {
            e.preventDefault();
            if (histIdx < history.length) { histIdx++; inputEl.value = history[histIdx] || ''; }
        } else if (e.key === 'Tab') {
            e.preventDefault();
            complete();
        }
    });

    function complete() {
        var val = inputEl.value, sp = val.lastIndexOf(' '), frag = val.slice(sp + 1);
        var pool;
        if (sp < 0) pool = Object.keys(commands);
        else { var node = getNode(cwd); pool = isDir(node) ? Object.keys(node) : []; }
        var hits = pool.filter(function (n) { return n.indexOf(frag) === 0; });
        if (hits.length === 1) inputEl.value = val.slice(0, sp + 1) + hits[0];
        else if (hits.length > 1) { printText(hits.join('  ')); }
    }

    screenEl.addEventListener('click', function () { inputEl.focus(); });

    window.csaTerm = {
        clear: function () {
            screenEl.innerHTML = '';
            print('<span class="ok">CyberShield Browser Lab — ' + esc(MOTD) + '</span>');
            printText('Type `help` to list commands. A flag is hidden on this box — try `ls -a`.');
            print('');
        }
    };

    updatePrompt();
    window.csaTerm.clear();
    inputEl.focus();
})();
