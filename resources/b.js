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