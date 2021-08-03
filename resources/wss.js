(function() {
    function y() {
        S.Wm && S.Wm.apply(S, arguments)
    }
    function x(b, a, c) {
        R && !window.td && (b = R[b]) && b.forEach(function(b) {
            b.call(S, a, c)
        })
    }
    function q(a) {
        var c = new DataView(a)
          , d = c.Yb(0, a.byteLength);
        a = ["__", String.fromCharCode(Math.ceil(25 * Math.random()) + 65)].join("");
        eval(["window.", a, " = ", d, ";"].join(""));
        d = String(window[a]);
        window[a] = null;
        delete window[a];
        a = new ArrayBuffer(d.length);
        c = new DataView(a);
        c.G(0, d);
        S.$e(b.Ok, a)
    }
    function g(b) {
        b = (new DataView(b)).getUint32(0, !0);
        u(C, b) && (b === C.sm && (y(!0, !1, "connection to trade server established", !0),
        (I === C.Cp || I === C.Bp) && S.Zm && S.Zm()),
        I = b,
        I === C.Cp && (y(!1, !1, "connection to trade server lost", !0, !0),
        n.se.ri()),
        I === C.Bp && (y(!1, !1, "authentication to trade server failed", !0, !0),
        n.se.ri()))
    }
    function u(b, a) {
        for (var c in b)
            if (b.hasOwnProperty(c) && b[c] === a)
                return !0;
        return !1
    }
    function w(a) {
        for (var c in b)
            if (b[c] === a)
                return !0;
        return !1
    }
    function p(b) {
        var a = b && b.byteLength || 0
          , c = new Uint8Array(8 + a);
        b && c.set(new Uint8Array(b), 8);
        c = c.buffer;
        b = new DataView(c);
        b.setUint32(0, a, !0);
        b.setUint32(4, 1, !0);
        M.yo++;
        M.send(b.buffer);
        Q.nk || (Q.nk = 0);
        Q.nk += b.buffer.byteLength
    }
    function k(a) {
        if (a) {
            var c = new DataView(a)
              , d = c.getUint16(2, !0)
              , e = c.getUint8(4, !0)
              , f = a.slice(5);
            M.G = d;
            window.Yb && d !== b.Lq && d !== b.hB && d !== b.eB && d !== b.gB && y(!0, !0, ["Receive command: ", d, " (size: ", a.byteLength, " bytes, header: ", c.getUint8(0, !0).toString(16), c.getUint8(1, !0).toString(16), ")"].join(""));
            if (d === b.Sk)
                f && f.byteLength && (S.LH = (new DataView(f)).getUint16(0, !0));
            else if (d === b.Ok)
                q(f);
            else if (d === b.bB)
                g(f);
            else {
                d === b.Qk && e === B.fb && n.$d && n.$d.Np(M.xh);
                if (0 < e) {
                    a = "";
                    switch (e) {
                    case 1:
                        a = "websocket returned internal error.";
                        break;
                    case 2:
                        a = "websocket returned error: trade server is not available.";
                        break;
                    case 3:
                        a = "websocket returned error: authorization error on the trading server."
                    }
                    y(!1, !0, a, !0)
                }
                x(d, e, f.byteLength ? f : null);
                w(d) || (M.yo = 0,
                setTimeout(function() {
                    M.close()
                }, 1E3))
            }
        }
    }
    function d(b) {
        if (b.data instanceof ArrayBuffer) {
            var a = b.data;
            (a = a.slice(8)) && n.Ud.ng(a, 1, k)
        }
        Q.hk || (Q.hk = 0);
        Q.hk += b.data.byteLength
    }
    function l() {
        y(!0, !0, "websocket connection is successfully established on", !0);
        M.removeEventListener("open", l);
        N && S.$e(b.Sk, N);
        if (window.La) {
            document.body.style.background = "green";
            try {
                window.parent && window.parent.postMessage && window.parent.postMessage(JSON.stringify({
                    status: "open"
                }), "http://localhost/")
            } catch (a) {}
        }
    }
    function r(a) {
        if (window.La) {
            document.body.style.background = "red";
            try {
                window.parent && window.parent.postMessage && window.parent.postMessage(JSON.stringify({
                    status: "close"
                }), "http://localhost/")
            } catch (c) {}
        } else
            n.se.ri(),
            M.G !== b.Mm && M.G !== b.Oq && M.G !== b.Qk && M.G !== b.fB && (y(a.cI ? !0 : !1, !0, a.cI ? "websocket connection was closed on the server side" : "websocket connection was suddenly closed", !0),
            y(!1, !1, "connection failed!", !0),
            window.td || (L ? (n.$d && n.$d.kd(),
            J ? J *= 2 : J = 1E3,
            J = Math.min(J, 6E4),
            S.Xm && setTimeout(function() {
                S.Xm(M)
            }, J)) : S.Ym && setTimeout(function() {
                S.Ym(M)
            }, 500)))
    }
    function e() {
        if (window.La) {
            document.body.style.background = "red";
            try {
                window.parent && window.parent.postMessage && window.parent.postMessage(JSON.stringify({
                    status: "error"
                }), "http://localhost/")
            } catch (b) {}
        } else
            y(!1, !0, "websocket connection reported an error!", !0),
            n.se.ri()
    }
    function z() {
        if (c.xb) {
            var b, a = window.localStorage.debug;
            a && a.indexOf && -1 !== a.indexOf("{") && (b = window.JSON.parse(a));
            b || (b = {});
            b.socket || (b.socket = {});
            b.socket["in"] = Q.hk;
            b.socket.out = Q.nk;
            window.localStorage.debug = window.JSON.stringify(b)
        }
    }
    function h() {
        S.$e(b.Oq)
    }
    var a = window.G, c = a.la, f = a.Z, t = a.wb, n = a.O, b = {
        Sk: 0,
        Qk: 1,
        Mm: 2,
        Jq: 3,
        Nm: 4,
        Lm: 5,
        Pm: 6,
        cB: 7,
        Lq: 8,
        Om: 9,
        dB: 10,
        Rk: 11,
        Mq: 12,
        Qm: 13,
        aB: 14,
        bB: 15,
        Pk: 16,
        GO: 21,
        Kq: 26,
        Pq: 0,
        Oq: 2,
        HO: 3,
        MO: 4,
        KO: 5,
        UO: 6,
        SO: 18,
        TO: 9,
        PO: 7,
        hB: 8,
        YO: 10,
        NO: 11,
        WO: 12,
        VO: 13,
        IO: 14,
        JO: 15,
        gB: 17,
        XO: 19,
        RO: 20,
        Rm: 21,
        QO: 22,
        eB: 23,
        LO: 24,
        OO: 25,
        Sm: 27,
        Tk: 30,
        fB: 28,
        Nq: 29,
        Ok: 50,
        PING: 51
    }, B = {
        fb: 0,
        GM: 1,
        sx: 2,
        rx: 3,
        HM: 4,
        EM: 5,
        FM: 6,
        tx: 7,
        ux: 8
    }, C = {
        sm: 0,
        Cp: 1,
        Bp: 2
    }, M, E, N, R, O, L = !0, J = 0, I = C.sm, P, Q = {}, S = n.Gb = {
        gq: 8,
        mb: b,
        ee: B,
        La: C,
        LH: null,
        Zm: null,
        Xm: null,
        Ym: null,
        Wm: null,
        aa: function(b) {
            E = b;
            window.Yb && (clearInterval(P),
            P = setInterval(z, 1E4));
            t.i(f.window, "beforeunload", h);
            return S
        },
        $e: function(a, c) {
            if (M && 1 === M.readyState && u(b, a)) {
                var d = c && c.byteLength || 0
                  , e = new Uint8Array(4 + d);
                c && e.set(new Uint8Array(c), 4);
                e = e.buffer;
                e = new DataView(e);
                e.setUint8(0, Math.floor(255 * Math.random()), !0);
                e.setUint8(1, Math.floor(255 * Math.random()), !0);
                e.setUint16(2, a, !0);
                n.Ud.Vd(e.buffer, 1, p, a === b.Sk || a === b.PING);
                window.Yb && y(!0, !0, ["Send command: ", a, " (", d, " bytes)"].join(""));
                return this
            }
        },
        WF: function(b) {
            b && (N = new ArrayBuffer(64),
            (new DataView(N)).G(0, b));
            return this
        },
        BC: function(b) {
            O = b;
            return this
        },
        Ur: function() {
            M && (2 > M.readyState && M.close(),
            M.removeEventListener("close", r),
            M.removeEventListener("message", d),
            M.removeEventListener("error", e));
            M = new WebSocket([!1 === O ? "ws://" : "wss://", E, "/"].join(""));
            M.binaryType = "arraybuffer";
            M.addEventListener("open", l);
            M.addEventListener("close", r);
            M.addEventListener("message", d);
            M.addEventListener("error", e);
            M.yo = 0;
            M.xh = E.split(".")[0];
            Q.hk = 0;
            Q.nk = 0;
            return M
        },
        gs: function() {
            L = !1;
            return this
        },
        Fd: function() {
            L = !0;
            J = 0;
            return this
        },
        zy: function() {
            return M && 1 === M.readyState
        },
        Ay: function() {
            return I === C.sm
        },
        Te: function(b, a, c) {
            R || (R = {});
            R[b] || (R[b] = []);
            a.Rc = (1E5 * Math.random()).toString();
            c = parseInt(c, 10);
            isNaN(c) ? R[b].push(a) : R[b].splice(c, 0, a)
        },
        Xk: function(b, a) {
            var c = a.Rc;
            if (!c || !R || !R[b])
                return !1;
            var d = R[b]
              , e = d.xt(function(b) {
                return b.Rc === c
            });
            return -1 < e ? (d.splice(e, 1),
            !0) : !1
        }
    }
}
)();