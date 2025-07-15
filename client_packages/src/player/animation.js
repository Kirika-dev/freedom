var animationSlots = mp.storage.data.animation;
var OpenAnimation = false;
var AnimMenuCam = null;
mp.events.add('openAnimMenu', function () {  
    if (!loggedin || chatActive || editing || cuffed || localplayer.isInAnyVehicle(true) || localplayer.getVariable('InDeath') == true || mp.players.local.isFalling() || mp.players.local.isShooting() || mp.players.local.isSwimming() || mp.players.local.isSwimmingUnderWater()) return;
	if (OpenAnimation) {
		AnimMenuClose();
	}
	else {
		if (global.menuOpened) return;
		if (!animationSlots) {
            animationSlots = [null,null,null,null,null];            
        }
		global.menuOpen();
		global.menu.execute(`AnimMenu.open(${JSON.stringify(animationSlots)})`);
		OpenAnimation = true;
		var bodyCamStart = localplayer.position;
		localplayer.freezePosition(true);
		var camValues = { Angle: localplayer.getRotation(2).z + 90, Dist: 2, Height: 0.3 };
		var pos = global.getCameraOffset(new mp.Vector3(bodyCamStart.x, bodyCamStart.y, bodyCamStart.z + camValues.Height), camValues.Angle, camValues.Dist);
		AnimMenuCam = mp.cameras.new('default', pos, new mp.Vector3(0, 0, 0), 70);
		AnimMenuCam.pointAtCoord(bodyCamStart.x, bodyCamStart.y, bodyCamStart.z);
		AnimMenuCam.setActive(true);
		mp.game.cam.renderScriptCams(true, true, 500, true, false);
	}
});

mp.events.add('client::animmenu:close', AnimMenuClose);

function AnimMenuClose() {
	global.menuClose();
	global.menu.execute(`AnimMenu.active = false; AnimMenu.select = 0; AnimMenu.search = ''`);
	OpenAnimation = false;
	localplayer.freezePosition(false);
	mp.game.cam.renderScriptCams(false, true, 500, true, false);
	if (AnimMenuCam != null) {
		AnimMenuCam.destroy();
		AnimMenuCam = null;
	}
}
mp.events.add('client::animmenu:saveslots',(fastSlots)=>{
    animationSlots = JSON.parse(fastSlots);
    mp.storage.data.animation = animationSlots;
    mp.storage.flush();    
})
mp.events.add('client::animmenu:dance', (a) => {
	if(mp.players.local.vehicle || !loggedin || chatActive || editing || cuffed || localplayer.getVariable('InDeath') == true || mp.players.local.isFalling() || mp.players.local.isShooting() || mp.players.local.isSwimming() || mp.players.local.isSwimmingUnderWater()) return;  
	var animation = JSON.parse(a);
	// mp.gui.chat.push(`${animation.ad} ${animation.an} ${animation.af}`);
    mp.events.callRemote('server::animation:play', animation.ad, animation.an, animation.af);
});

mp.events.add('client::animmenu:walk', (a) => { 
	var animation = JSON.parse(a);
    mp.events.callRemote('server::walkstyle:set', animation.style);
});

setInterval(() => {     
    for(let i = 49; i<=54;i++)
    {
        if(new Date().getTime() - global.lastCheck < 1000 || mp.players.local.vehicle || !loggedin || chatActive || editing || cuffed || global.menuOpened || localplayer.getVariable('InDeath') == true || mp.players.local.isFalling() || mp.players.local.isShooting() || mp.players.local.isSwimming() || mp.players.local.isSwimmingUnderWater()) return;
        let x = mp.keys.isDown(i)
        let y = mp.keys.isDown(18);
        if (x&&y) {
            if (animationSlots[i-49] != null) { mp.events.callRemote('server::animation:play', animationSlots[i-49].ad, animationSlots[i-49].an, animationSlots[i-49].af); global.lastCheck = new Date().getTime();};
        }
    }
}, 10);

mp.keys.bind(Keys.VK_BACK, false, function () {
    if(mp.players.local.vehicle || !loggedin || chatActive || editing || cuffed || global.menuOpened || localplayer.getVariable('InDeath') == true || mp.players.local.isFalling() || mp.players.local.isShooting() || mp.players.local.isSwimming() || mp.players.local.isSwimmingUnderWater()) return; 
	mp.events.callRemote("server::animation:stop");
});