var cam = mp.cameras.new('default', new mp.Vector3(0, 0, 0), new mp.Vector3(0, 0, 0), false);
var effect = '';
global.loggedin = false;
global.lastCheck = 0;
global.chatLastCheck = 0;
global.pocketEnabled = false;

mp.events.add('ragdoll', function (arg) {
    mp.players.local.setToRagdoll(5000, 5000, arg, true, true, true);
});

mp.events.add('outVeh', (flag) => {
	const player = mp.players.local;
	if (player.vehicle != null)
		player.taskLeaveVehicle(player.vehicle.handle, 0);
});

mp.game.gameplay.disableAutomaticRespawn(true);
mp.game.gameplay.ignoreNextRestart(true);
mp.game.gameplay.setFadeInAfterDeathArrest(false);
mp.game.gameplay.setFadeOutAfterDeath(false);
mp.game.gameplay.setFadeInAfterLoad(false);

mp.events.add('freeze', function (toggle) {
    localplayer.freezePosition(toggle);
});

mp.events.add('destroyCamera', function () {
    if(cam!=null)
    {       
        cam.destroy();
        mp.game.cam.renderScriptCams(false, false, 3000, true, true);
    }
});

mp.events.add('carRoom', function (x, y, z, x2, y2, z2) {
    cam = mp.cameras.new('default', new mp.Vector3(x, y, z), new mp.Vector3(0, 0, 0), 45);
    cam.pointAtCoord(x2, y2, z2);
    cam.setActive(true);
    mp.game.cam.renderScriptCams(true, false, 0, true, false);
});

mp.events.add('screenFadeOut', function (duration) {
    mp.game.cam.doScreenFadeOut(duration);
});

mp.events.add('screenFadeIn', function (duration) {
    mp.game.cam.doScreenFadeIn(duration);
});

var lastScreenEffect = "";
mp.events.add('startScreenEffect', function (effectName, duration, looped) {
	try {
		lastScreenEffect = effectName;
		mp.game.graphics.startScreenEffect(effectName, duration, looped);
	} catch (e) { }
});

mp.events.add('stopScreenEffect', function (effectName) {
	try {
		var effect = (effectName == undefined) ? lastScreenEffect : effectName;
		mp.game.graphics.stopScreenEffect(effect);
	} catch (e) { }
});

mp.events.add('stopAndStartScreenEffect', function (stopEffect, startEffect, duration, looped) {
	try {
		mp.game.graphics.stopScreenEffect(stopEffect);
		mp.game.graphics.startScreenEffect(startEffect, duration, looped);
	} catch (e) { }
});

mp.events.add('setHUDVisible', function (arg) {
    mp.game.ui.displayHud(arg);
    mp.gui.chat.show(arg);
    mp.game.ui.displayRadar(arg);
});

mp.events.add('setPocketEnabled', function (state) {
    pocketEnabled = state;
    if (state) {
        mp.gui.execute("fx.set('inpocket')");
        mp.game.invoke(getNative("SET_FOLLOW_PED_CAM_VIEW_MODE"), 4);
    }
    else {
        mp.gui.execute("fx.reset()");
    }
});

mp.keys.bind(Keys.VK_K, false, function () {
    if (!loggedin || chatActive || editing || new Date().getTime() - lastCheck < 1000 || global.menuOpened) return;
    mp.events.callRemote('cancelPressed');
    lastCheck = new Date().getTime();
});

mp.events.add('ready', function () {
    mp.game.ui.displayHud(true);
    mp.gui.chat.show(true);
});

mp.events.add('kick', function (notify) {
    mp.events.call('notify', 4, 9, notify, 10000);
    mp.events.callRemote('kickclient');
});

mp.events.add('setFollow', function (toggle, entity) {
    if (toggle) {
        if (entity && mp.players.exists(entity))
            localplayer.taskFollowToOffsetOf(entity.handle, 0, 0, 0, 1, -1, 1, true)
    }
    else
        localplayer.clearTasks();
});

setInterval(function () {
    if (localplayer.getArmour() <= 0 && localplayer.getVariable('HASARMOR') === true) {
        mp.events.callRemote('deletearmor');
    }
}, 600);

mp.keys.bind(Keys.VK_E, false, function () { // E key
    if (!loggedin || chatActive || editing || new Date().getTime() - lastCheck < 1000 || global.menuOpened) return;
    if(global.casinoOpened){
        mp.events.callRemote('interactionPressed');
    }
    mp.events.callRemote('interactionPressed');
    lastCheck = new Date().getTime();
    global.acheat.pos();
});

mp.events.add('openCarDoor', function () { // L key
    if (!loggedin || chatActive || editing || new Date().getTime() - lastCheck < 1000 || global.menuOpened) return;
    mp.events.callRemote('lockCarPressed');
    lastCheck = new Date().getTime();
});


mp.keys.bind(Keys.VK_LEFT, true, () => {
	if(mp.gui.cursor.visible || !loggedin) return;
	if(localplayer.vehicle) {
		if(localplayer.vehicle.getPedInSeat(-1) != localplayer.handle) return;
		if(new Date().getTime() - lastCheck > 500) {
			lastCheck = new Date().getTime();
			if(localplayer.vehicle.getVariable('leftlight') == true) {
                mp.events.callRemote("VehStream_SetIndicatorLightsData", localplayer.vehicle, 0, 0);
                UPDATE_INDICATORS(0, localplayer.vehicle.getVariable('rightlight'));
            }
			else { 
                mp.events.callRemote("VehStream_SetIndicatorLightsData", localplayer.vehicle, 1, 0);
                UPDATE_INDICATORS(1, localplayer.vehicle.getVariable('rightlight'));
            }
		}
	}
});

mp.keys.bind(Keys.VK_RIGHT, true, () => {
	if(mp.gui.cursor.visible || !loggedin) return;
	if(localplayer.vehicle) {
		if(localplayer.vehicle.getPedInSeat(-1) != localplayer.handle) return;
		if(new Date().getTime() - lastCheck > 500) {
			lastCheck = new Date().getTime();
			if(localplayer.vehicle.getVariable('rightlight') == true) { 
                mp.events.callRemote("VehStream_SetIndicatorLightsData", localplayer.vehicle, 0, 0);
                UPDATE_INDICATORS(localplayer.vehicle.getVariable('leftlight'), 0);
            }
			else { 
                mp.events.callRemote("VehStream_SetIndicatorLightsData", localplayer.vehicle, 0, 1);
                UPDATE_INDICATORS(localplayer.vehicle.getVariable('leftlight'), 1);
            }
		}
	}
});

mp.keys.bind(Keys.VK_DOWN, true, () => {
	if(mp.gui.cursor.visible || !loggedin) return;
	if(localplayer.vehicle) {
		if(localplayer.vehicle.getPedInSeat(-1) != localplayer.handle) return;
		if(new Date().getTime() - lastCheck > 500) {
			lastCheck = new Date().getTime();
			if(localplayer.vehicle.getVariable('leftlight') == true && localplayer.vehicle.getVariable('rightlight') == true) { 
                mp.events.callRemote("VehStream_SetIndicatorLightsData", localplayer.vehicle, 0, 0);
                UPDATE_INDICATORS(0, 0);
            }
			else { 
                mp.events.callRemote("VehStream_SetIndicatorLightsData", localplayer.vehicle, 1, 1);
                UPDATE_INDICATORS(1, 1);
            }
            
		}
	}
});

//thx koltr
function UPDATE_INDICATORS(left, right)
{
    let total = 0;

    if (left == true && right == true)
        total = 3;
    else if (left == true)
        total = 1;
    else if (right == true)
        total = 2;

    mp.gui.execute(`HUD.indicator=${total}`);
}
//=========

mp.events.add('startEngineCar', function () { // ENGINE KEY
    if (!loggedin || chatActive || editing || new Date().getTime() - lastCheck < 400 || global.menuOpened) return;
    if (localplayer.isInAnyVehicle(false) && localplayer.vehicle.getSpeed() <= 3) {
        lastCheck = new Date().getTime();
        mp.events.callRemote('engineCarPressed');
    }
});
global.phoneOpen = false;
mp.events.add('openPhoneMenu', function () { // PHONE KEY
    if (!loggedin || chatActive || editing || global.menuCheck() && !phoneOpen || cuffed || localplayer.getVariable('InDeath') == true || new Date().getTime() - lastCheck < 1600 || mp.players.local.isFalling() || mp.players.local.isShooting() || mp.players.local.isSwimming() || mp.players.local.isSwimmingUnderWater() || localplayer.getVariable("PickAxe.InHands") == true) return;
    if (!phoneOpen) {
		mp.events.callRemote('server::phone:open');
	}
	else {
		mp.events.callRemote('server::phone:close');
	}
    lastCheck = new Date().getTime();
});

mp.events.add('cuffPlayerKey', function () {
    if (!loggedin || chatActive || editing || new Date().getTime() - lastCheck < 1000 || global.menuOpened) return;
    mp.events.callRemote('playerPressCuffBut');
    lastCheck = new Date().getTime();
});

//old scripts
mp.keys.bind(Keys.VK_Z, false, function () { // Z key
    if (!loggedin || chatActive || editing || new Date().getTime() - lastCheck < 1000 || global.menuOpened) return;
    
    if(localplayer.vehicle) {
        CheckMyWaypoint();
    } else mp.events.callRemote('playerPressFollowBut');
    lastCheck = new Date().getTime();
});

function CheckMyWaypoint() {
    try {
        if(mp.game.invoke('0x1DD1F58F493F1DA5')) {
            let foundblip = false;
            let blipIterator = mp.game.invoke('0x186E5D252FA50E7D');
            let totalBlipsFound = mp.game.invoke('0x9A3FF3DE163034E8');
            let FirstInfoId = mp.game.invoke('0x1BEDE233E6CD2A1F', blipIterator);
            let NextInfoId = mp.game.invoke('0x14F96AA50D6FBEA7', blipIterator);
            for (let i = FirstInfoId, blipCount = 0; blipCount != totalBlipsFound; blipCount++, i = NextInfoId) {
                if (mp.game.invoke('0x1FC877464A04FC4F', i) == 8) {
                    var coord = mp.game.ui.getBlipInfoIdCoord(i);
                    foundblip = true;
                    break;
                }
            }
            if(foundblip) mp.events.callRemote('syncWaypoint', coord.x, coord.y, coord.z);
        }
    } catch (e) { }
}

var gproute = null;

mp.events.add('syncWP', function (bX, bY, type) {
    if(!mp.game.invoke('0x1DD1F58F493F1DA5')) {
        //mp.game.ui.setNewWaypoint(bX, bY);
        gproute = mp.blips.new(38, new mp.Vector3(bX, bY), { alpha: 255, name: "", scale: 1, color: 1 });
        gproute.setRoute(true);
        gproute.setRouteColour(5);
        if(type == 0) mp.events.call('notify', 2, 9, "Пассажир передал Вам информацию о своём маршруте!", 3000);
        else if(type == 1) mp.events.call('notify', 2, 9, "Человек из списка контактов Вашего телефона передал Вам метку его местоположения!", 3000);
    } else {
        if(type == 0) mp.events.call('notify', 4, 9, "Пассажир попытался передать Вам информацию о маршруте, но у Вас уже установлен другой маршрут.", 5000);
        else if(type == 1) mp.events.call('notify', 4, 9, "Человек из списка контактов Вашего телефона попытался передать Вам метку его местоположения, но у Вас уже установлена другая метка.", 5000);
    }
});

mp.events.add('removeGRoute', function(){
	try 
	{
		if (gproute != null)
		{
			gproute.destroy();
			gproute = null;
		}
	}
	catch (e) {}
});
//old scripts end

mp.keys.bind(Keys.VK_U, false, function () { // u key
    if (!loggedin || chatActive || editing || global.menuOpened || new Date().getTime() - lastCheck < 1000) return;
    mp.events.callRemote('openCopCarMenu');
    lastCheck = new Date().getTime();
});

mp.keys.bind(Keys.VK_OEM_3, false, function () { // ` key
    if (chatActive || (global.menuOpened && mp.gui.cursor.visible)) return;
    mp.gui.cursor.visible = !mp.gui.cursor.visible;
});

var lastPos = new mp.Vector3(0, 0, 0);

mp.game.gameplay.setFadeInAfterDeathArrest(false);
mp.game.gameplay.setFadeInAfterLoad(false);

var deathTimerOn = false;
var deathTimer = 0;

mp.events.add('DeathTimer', (time) => {
    if (time === false) {
        deathTimerOn = false;
		global.dialog.closeMED();
	}
    else {
		mp.gui.execute(`deathMenu.disableBTN=true`);
        deathTimerOn = true;
        deathTimer = new Date().getTime() + time;
    }
});

mp.events.add('render', () => {
   if (localplayer.getVariable('InDeath') == true || intrunk) {
        mp.game.controls.disableAllControlActions(2);
        mp.game.controls.enableControlAction(2, 1, true);
        mp.game.controls.enableControlAction(2, 2, true);
        mp.game.controls.enableControlAction(2, 3, true);
        mp.game.controls.enableControlAction(2, 4, true);
        mp.game.controls.enableControlAction(2, 5, true);
        mp.game.controls.enableControlAction(2, 6, true);
    }
	if (intrunk)
		mp.game.controls.enableControlAction(2, 27, false);

    if (deathTimerOn) {
        var secondsLeft = Math.trunc((deathTimer - new Date().getTime()) / 1000);
		if (secondsLeft <= 0) {
			secondsLeft = 0;
		}
		mp.gui.execute(`deathMenu.time=${secondsLeft}`);
    }

    if (mp.game.controls.isControlPressed(0, 32) || 
        mp.game.controls.isControlPressed(0, 33) || 
        mp.game.controls.isControlPressed(0, 321) ||
        mp.game.controls.isControlPressed(0, 34) || 
        mp.game.controls.isControlPressed(0, 35) || 
        mp.game.controls.isControlPressed(0, 24) || 
        localplayer.getVariable('InDeath') == true) 
    {
        afkSecondsCount = 0;
    }
    else if (!global.train && localplayer.isInAnyVehicle(false) && localplayer.vehicle.getSpeed() != 0) 
    {
        afkSecondsCount = 0;
    } 
    else if(global.spectating) 
    {
		afkSecondsCount = 0;
	}
});

mp.events.add("playerRuleTriggered", (rule, counter) => {
    if (rule === 'ping' && counter > 5) {
        mp.events.call('notify', 4, 2, "Ваш ping слишком большой. Зайдите позже", 5000);
        mp.events.callRemote("kickclient");
    }
});



//JOBS===================
global.Falling = false;
mp.events.add('render', () => {
	if (localplayer.getVariable("ShapeBuilderState")) {
		mp.game.controls.disableControlAction(2, 22, true);
		mp.game.controls.disableControlAction(2, 24, true);
		mp.game.controls.disableControlAction(2, 25, true);
		if(!localplayer.isPlayingAnim("anim@heists@box_carry@", "idle", 3)) mp.events.callRemote('serverplayerPlayAnimBuilder')
		if (localplayer.isFalling()) 
		{
			if (global.Falling) return;
			global.Falling = true;
			mp.events.callRemote("serverplayerstopboxBuilder");
		}
		else {
			global.Falling = false;
		}
	}
});


var Miner = {
	last: 0,
}
var Sawmill = {
	last: 0,
}
mp.events.add('render', () => {
	if (localplayer.getVariable("MINER_ON_ORE") == true && localplayer.getVariable("PickAxe.InHands") == true) {
		if (mp.game.controls.isDisabledControlJustReleased(0, 24) && !mp.gui.cursor.visible) 
		{
			if (new Date().getTime() - Miner.last > 2000) {
				mp.events.callRemote("server::miner:click");
				Miner.last = new Date().getTime();
			}
		}
	}
	if (localplayer.getVariable("SAWMILL_ON_TREE") == true && localplayer.getVariable("Axe.InHands") == true) {
		if (mp.game.controls.isDisabledControlJustReleased(0, 24) && !mp.gui.cursor.visible) 
		{
			if (new Date().getTime() - Sawmill.last > 2000) {
				mp.events.callRemote("server::sawmill:click");
				Sawmill.last = new Date().getTime();
			}
		}
	}
	if (localplayer.getVariable("MINER_PLAYANIM") == true || localplayer.getVariable("SAWMILL_PLAYANIM") == true) {
		mp.game.controls.disableControlAction(0, 257, true); // стрельба
		mp.game.controls.disableControlAction(0, 22, true);
		mp.game.controls.disableControlAction(2, 25, true);
		mp.game.controls.disableControlAction(0, 23, true); // INPUT_ENTER
		
		mp.game.controls.disableControlAction(2, 24, true);
		mp.game.controls.disableControlAction(2, 69, true);
		mp.game.controls.disableControlAction(2, 70, true);
		mp.game.controls.disableControlAction(2, 92, true);

		mp.game.controls.disableControlAction(2, 140, true);
		mp.game.controls.disableControlAction(2, 141, true);
		mp.game.controls.disableControlAction(2, 263, true);
		mp.game.controls.disableControlAction(2, 264, true);

		mp.game.controls.disableControlAction(0, 21, true);
		mp.game.controls.disableControlAction(0, 23, true);
		mp.game.controls.disableControlAction(0, 32, true);
		mp.game.controls.disableControlAction(0, 33, true);
		mp.game.controls.disableControlAction(0, 34, true);
		mp.game.controls.disableControlAction(0, 35, true);
	}
	mp.objects.forEach(
	(object) => {
		var posL = localplayer.position;
		var posO = object.position;
		if (object.getVariable("MINER_OBJECT") == true) {
			if (calculateDistance(posL, posO) < 15) {
				if (posO.x != undefined && posO != undefined) {
					const xy = mp.game.graphics.world3dToScreen2d(posO.x, posO.y, posO.z+2);
					if (xy != null && xy.x != null) {
						global.drawText("Залежи руды", xy.x, xy.y - 0.04 * 0.7, [0.35,0.35], 255, 255, 255, 255, 0, 3);
						global.drawText("Используйте [ЛКМ] для сбора", xy.x, xy.y, [0.25,0.25], 255, 255, 255, 200, 0, 3);
						global.drawSprite('majestic_textures_001', 'miner', [0.4, 0.4], 0, {
						r: 255,
						g: 255,
						b: 255,
						a: 255
						},  mp.game.graphics.world3dToScreen2d(posO.x, posO.y, posO.z+2).x - 0.03 * 0.7,  mp.game.graphics.world3dToScreen2d(posO.x, posO.y, posO.z+2).y);
					}
				}
			}
		}
		if (object.getVariable("SAWMILL_OBJECT") == true) {
			if (calculateDistance(posL, posO) < 15) {
				if (posO.x != undefined && posO != undefined) {
					const xy = mp.game.graphics.world3dToScreen2d(posO.x, posO.y, posO.z+2);
					if (xy != null && xy.x != null) {
						global.drawText("Поваленное дерево", xy.x, xy.y - 0.04 * 0.7, [0.35,0.35], 255, 255, 255, 255, 0, 3);
						global.drawText("Используйте [ЛКМ] для сбора", xy.x, xy.y, [0.25,0.25], 255, 255, 255, 200, 0, 3);
						global.drawSprite('majestic_textures_001', 'sawmill', [0.4, 0.4], 0, {
						r: 255,
						g: 255,
						b: 255,
						a: 255
						},  mp.game.graphics.world3dToScreen2d(posO.x, posO.y, posO.z+2).x - 0.03 * 0.7,  mp.game.graphics.world3dToScreen2d(posO.x, posO.y, posO.z+2).y);
					}
				}
			}
		}
	});
});

function calculateDistance(v1, v2) {
    var dx = v1.x - v2.x;
    var dy = v1.y - v2.y;
    var dz = v1.z - v2.z;

    return Math.sqrt(dx * dx + dy * dy + dz * dz);
}
//========================

//ANIMSYNC================
mp.events.add('render', () => {
	if (localplayer.getVariable("PLAYER_ANIM_STATE") == true) {
		if (!localplayer.isPlayingAnim(JSON.stringify(localplayer.getVariable("PLAYER_ANIM_DICT")), JSON.stringify(localplayer.getVariable("PLAYER_ANIM_ANIMATION")), parseInt(localplayer.getVariable("PLAYER_ANIM_FLAG"))) && localplayer.getVariable("PLAYER_ANIM_STATE")) localplayer.taskPlayAnim(JSON.stringify(localplayer.getVariable("PLAYER_ANIM_DICT")), JSON.stringify(localplayer.getVariable("PLAYER_ANIM_ANIMATION")), 8.0, -8, -1, parseInt(localplayer.getVariable("PLAYER_ANIM_FLAG")), 0, false, false, false)
	}
});
//========================

//========================
mp.events.add('render', () => {
    let player = mp.players.local;
    mp.vehicles.forEachInStreamRange(vehicle => {
        if (vehicle != undefined && distanceVector(player.position, vehicle.position) < 100.0) {
            mp.players.forEachInStreamRange(pl => {
                if (pl != undefined && distanceVector(pl.position, vehicle.position) < 100.0 && player.getVariable("InSafeZone")) 
				{
					pl.setNoCollision(vehicle.handle, false);
				}
			});
        }
    });
});

mp.events.add('render', () => {
    let player = mp.players.local;
    mp.objects.forEachInStreamRange(object => {
        if (object != undefined && object.getVariable("TYPE") == "DROPPED") {
            mp.players.forEachInStreamRange(pl => {
                if (pl != undefined) 
				{
					pl.setNoCollision(object.handle, false);
				}
			});
			mp.vehicles.forEachInStreamRange(pl => {
                if (pl != undefined) 
				{
					pl.setNoCollision(object.handle, false);
				}
			});
        }
    });
});

function distanceVector(v1, v2) {
    var dx = (v1.x - v2.x), dy = (v1.y - v2.y), dz = (v1.z - v2.z);
    return Math.sqrt( dx * dx + dy * dy + dz * dz );
}
//========================

//Fireworks===============
let remCnt = 0;
mp.events.add('client::fireshow:play', async (x, y, z, countTry) => {
  remCnt += countTry;
  await global.sleep(2000);
  const object = mp.objects.new('ind_prop_firework_03', new mp.Vector3(x, y, z - 1), {
    dimension: 0
  });
  await global.sleep(10);
  object.placeOnGroundProperly();
  object.freezePosition(true);
  await global.sleep(5000);

  for (let count = 0; count < countTry; count++) {
    while (!mp.game.streaming.hasNamedPtfxAssetLoaded('scr_indep_fireworks')) {
      mp.game.streaming.requestNamedPtfxAsset('scr_indep_fireworks');
      await global.sleep(10);
    }

    mp.game.graphics.setPtfxAssetNextCall('scr_indep_fireworks');
    let part1 = mp.game.graphics.startParticleFxLoopedAtCoord('scr_indep_firework_trailburst', x, y, z - 1.5, 0.0, 0.0, 0.0, 1.0, false, false, false, false);
    await global.sleep(2500);
    remCnt--;
  }

  if (remCnt <= 0) mp.game.streaming.removeNamedPtfxAsset('scr_indep_fireworks');
  if (remCnt < 0) remCnt = 0;
  await global.sleep(2000);
  if (mp.objects.exists(object)) object.destroy();
});
//========================