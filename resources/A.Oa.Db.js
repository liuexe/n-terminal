A.Oa.Db
(function() {
    var y = A.Oa.Xf.Db = {};
    y.iG = function(x) {
        if (!x)
            return null;
        var q = new ArrayBuffer(64 + (x.ao ? 16 : 0));
        (new DataView(q)).La(0, (x.ye || "").substr(0, 32));
        q = new Uint8Array(q);
        x.ao && q.set(new Uint8Array(x.ao), 64);
        return q.buffer
    }
    ;
    y.cU = function(x) {
        if (!x)
            return null;
        var q = new ArrayBuffer(64);
        (new DataView(q)).G(0, x);
        return q
    }
}
)();