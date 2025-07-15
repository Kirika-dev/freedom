global.spmenu = mp.browsers["new"]('http://package/interface/modules/SPMenu/index.html');
global.spmenu.active = false;
mp.events.add("spmode", (target, toggle, data = null) => {
    global.localplayer.freezePosition(toggle);
    if (toggle) {
        if (target && mp.players.exists(target)) {
            global.sptarget = target;
            global.spectating = true;
            global.localplayer.attachTo(target.handle, -1, -1.5, -1.5, 2, 0, 0, 0, true, false, false, false, 0, false);
            global.spmenu.active = true;
            global.menuOpen(false);
            global.spmenu.execute(`app.open(${data})`);
        } else mp.events.callRemote("UnSpectate");
    } else {
        global.sptarget = null;
        global.localplayer.detach(true, true);
        global.spectating = false;
        global.menuClose(false);
        if (global.spmenu != null) {
            global.spmenu.active = false;
        }
    }
});
mp.events.add('Client:UnSpectate', () => {
    mp.events.callRemote('UnSpectate');
});
mp.events.add('Client:SpectateSelect', (a) => {
    if (a == 0) mp.events.callRemote("SpectateSelect", false);
    else mp.events.callRemote("SpectateSelect", true);
});
mp.events.add('Client:Refresh', () => {
    mp.events.call("spmode", global.sptarget, true);
});