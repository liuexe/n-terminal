(function() {
    function y(e, g, a, c, f) {
        f ? u(e, g, a, c, null) : d.gn.nf(e, g, a, c, u)
    }
    function x(e) {
        return A.controls.A.ym((new Date).getTime() + 1E3 * (d.J.hj + 3600 * d.J.ck), e)
    }
    function q() {
        var e = d.X.$b();
        if (e)
            a: {
                var g = e.symbol
                  , e = e.period;
                if (void 0 !== g && void 0 !== e) {
                    var a = x(e)
                      , c = Math.max(Math.floor(a - 6E4 * e * l.ic.Ik() * 3), 0)
                      , f = d.Nc.R(g, e);
                    if (f && f.length) {
                        if (f[f.length - 1][0] == a && f.length > l.ic.Ik())
                            break a
                    } else
                        d.Nc.Fd(g, e);
                    y(g, e, c, a)
                }
            }
    }
    function g(g, h) {
        var a = e.am;
        if (a) {
            var c = p.Gb;
            if (g == c.ee.fb) {
                var f = a[0];
                if (!f)
                    return;
                var k = r.ic.Rr(h);
                d.F.eb(f.I);
                if (!k.length || 1 == k.length && f.aj != k[0][0]) {
                    if (14400 > (f.Xl - f.aj) / 1E3 / 60) {
                        f.aj -= 6E4 * f.fi * l.ic.Ik() * 3;
                        c.$e(c.mb.Rk, r.ic.Fn(f));
                        return
                    }
                    var n = d.Nc.R(f.I, f.fi);
                    n && (n.cf = !0)
                }
                d.gn.si(f.I, f.fi, f.aj, f.Xl, r.ic.Rr(h));
                d.Nc.c(f.I, f.fi, k)
            }
            a.splice(0, 1);
            a.length && c.$e(c.mb.Rk, r.ic.Fn(a[0]))
        }
    }
    function u(g, h, a, c, f) {
        if (f && 1 < f.length) {
            d.F.eb(g);
            d.Nc.c(g, h, f);
            if (c !== x(h))
                return;
            a = c
        }
        var k = e.am;
        k || (k = e.am = []);
        f = {};
        f.I = g;
        f.fi = h;
        f.aj = a;
        f.Xl = c;
        f.iU = 0;
        k.push(f);
        1 == k.length && (g = p.Gb,
        g.$e(g.mb.Rk, r.ic.Fn(f)))
    }
    var w = A.Oa
      , p = A.O
      , k = w.control
      , d = w.Xa
      , l = w.view
      , r = w.Xf
      , e = k.ic = {};
    e.de = {
        Qc: 1,
        Mc: 5,
        Kc: 15,
        Lc: 30,
        qc: 60,
        xc: 240,
        Gc: 1440,
        Fc: 10080,
        ge: 43200
    };
    e.aa = function() {
        if (e.ja)
            return e;
        var k = p.Gb;
        k.Te(k.mb.Rk, g);
        d.X.i("select", q);
        e.ja = !0;
        return e
    }
    ;
    e.kd = function() {
        var g = d.X.bf, h;
        if (g && g.length) {
            for (var a = 0, c = g.length; a < c; a++)
                (h = g[a]) && k.F.te(h.symbol);
            d.X.v("start");
            (g = d.X.$b()) && d.X.Di(g.id)
        } else
            (g = d.tc.If()) && g.length && k.ic.Mh(g[0], A.controls.A.de.qc);
        return e
    }
    ;
    e.Mh = function(e, g, a) {
        if (e) {
            g || (g = A.controls.A.de.qc);
            var c = d.X.$b();
            if (a && c) {
                c.symbol = e;
                c.symbol_name = e;
                c.period = g;
                if (e = d.F.eb(e))
                    c.digits = e.S,
                    c.symbol_name = e.zb();
                d.X.v("replace", c);
                d.X.v("change");
                d.X.Di(c.id)
            } else
                (c = d.X.Nx(e, g)) || (c = d.X.tw(e, g)),
                d.X.Di(c.id)
        }
    }
    ;
    e.oi = function(g) {
        var h = d.X.Ox(g);
        d.X.rw(g);
        (h = (g = d.X.bf) && g[h]) || (h = g && g[g.length - 1]);
        d.X.Di(h && h.id || null);
        return e
    }
    ;
    e.kB = function(e, g) {
        var a = d.Nc.R(e, g);
        if (a && a.length < d.Nc.Pz && !a.cf) {
            var a = a[0], c;
            a && (a = a[0],
            c = Math.max(a - Math.round(6E4 * g * l.ic.Ik() * 3), 0),
            y(e, g, c, a))
        }
    }
    ;
    e.hb = function() {
        var d = e.am;
        if (d) {
            for (var g = 0, a = d.length; g < a; g++)
                d[g] = null;
            e.am = null
        }
        l.ic.hb()
    }
    ;
    e.uC = function() {
        var e = d.X.$b();
        if (e) {
            var g = d.Nc.R(e.symbol, e.period);
            g && g.length && y(e.symbol, e.period, g[0][0], g[g.length - 1][0], !0)
        }
    }
}
)();