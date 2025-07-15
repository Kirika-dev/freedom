global.changeNumberCEF = null;
global.changeNumberVEHCEF = null;

mp.events.add("client::openmenunpcnumber", (a) => {
    if (global.menuCheck()) return;

    if (global.changeNumberCEF == null)
    {
        global.changeNumberCEF = mp.browsers["new"]('package://interface/modules/NumberChanger/index.html');
        global.changeNumberCEF.active = true;
    }
    global.menuOpen();
	global.changeNumberCEF.execute(`numberChanger.price=${JSON.stringify(a)}`);
});
mp.events.add("closeChangeNumber", () => {
    if(global.changeNumberCEF != null) {
		global.changeNumberCEF.destroy();
		global.changeNumberCEF = null;
	}
    global.menuClose();
	mp.events.call('NPC.cameraOff', 500);
});

mp.events.add('client::setvehnum', (a) => {
	global.changeNumberCEF.execute(`numberChanger.number=${JSON.stringify(a)}`);
});

mp.events.add('client::buynumbers', (a) => {
	mp.events.callRemote('server::buynumbers', a);
	mp.events.call('closeChangeNumber');
});

mp.events.add('client::changevehnummenuChangenum', (a) => {
	mp.events.callRemote('server::setnumberveh', a);
});

mp.events.add('client::randomvehnum', (a) => {
	if(new Date().getTime() - global.lastCheck < 500) return; 
	global.lastCheck = new Date().getTime();
	mp.events.callRemote('server::randomvehnum', a);
});

mp.events.add("VEHICLE::FREEZE", (vehicle, state) => {
    vehicle.freezePosition(state);
});

mp.events.add("client::openvehmenuchangenum", (a, b) => {
    if (global.menuCheck()) return;

    if (global.changeNumberVEHCEF == null)
    {
        global.changeNumberVEHCEF = mp.browsers["new"]('package://interface/modules/NumberChanger/SetNum/index.html');
        global.changeNumberVEHCEF.active = true;
    }
    global.menuOpen();
	global.changeNumberVEHCEF.execute(`numberChanger.num=${JSON.stringify(a)}`);
	global.changeNumberVEHCEF.execute(`numberChanger.oldnum=${JSON.stringify(a)}`);
	global.changeNumberVEHCEF.execute(`numberChanger.numbers=${b}`);
});

mp.events.add("client::closemenuchangenumveh", () => {
    if(global.changeNumberVEHCEF != null) {
		global.changeNumberVEHCEF.destroy();
		global.changeNumberVEHCEF = null;
	}
    global.menuClose();
});