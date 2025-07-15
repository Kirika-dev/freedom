global.entity = null;
global.nearestObject = null;

var lastEntCheck = 0;
var checkInterval = 200;

var backlightColor = [196, 17, 21];

var blockcontrols = false;
global.cuffed = false;
var hasmoney = false;
var isInSafeZone = false;

var lastCuffUpdate = new Date().getTime();

function getLookingAtEntity() {
    let startPosition = localplayer.getBoneCoords(12844, 0.5, 0, 0);
    var resolution = mp.game.graphics.getScreenActiveResolution(1, 1);
    let secondPoint = mp.game.graphics.screen2dToWorld3d([resolution.x / 2, resolution.y / 2, (2 | 4 | 8)]);
    if (secondPoint == undefined) return null;

    startPosition.z -= 0.3;
    const result = mp.raycasting.testPointToPoint(startPosition, secondPoint, localplayer, (2 | 4 | 8 | 16));

    if (typeof result !== 'undefined') {
        if (typeof result.entity.type === 'undefined') return null;
        if (result.entity.type == 'object' && result.entity.getVariable('TYPE') == undefined) return null;

        let entPos = result.entity.position;
        let lPos = localplayer.position;
        if (mp.game.gameplay.getDistanceBetweenCoords(entPos.x, entPos.y, entPos.z, lPos.x, lPos.y, lPos.z, true) > 8) return null;
		if (result.entity.type == "ped" || result.entity.type == "object") return;
        if (localplayer.isInAnyVehicle(false)) return;
        return result.entity;
    }
    return null;
}

function getNearestObjects() {

    var tempO = null;
    if (localplayer.isInAnyVehicle(false)) {

    }
    else {
        var objects = mp.objects.toArray();
        objects.forEach(
            (object) => {
                var posL = localplayer.position;
                var posO = object.position;
                var distance = mp.game.gameplay.getDistanceBetweenCoords(posL.x, posL.y, posL.z, posO.x, posO.y, posO.z, true);
                if (object.getVariable('TYPE') != undefined && localplayer.dimension === object.dimension && distance < 3) {
                    if (tempO === null) tempO = object;
                    else if (mp.game.gameplay.getDistanceBetweenCoords(posL.x, posL.y, posL.z, posO.x, posO.y, posO.z, true) <
                        mp.game.gameplay.getDistanceBetweenCoords(posL.x, posL.y, posL.z, tempO.position.x, tempO.position.y, tempO.position.z, true))
                        tempO = object;
                }
            });
    }
    nearestObject = tempO;
}

let state = 0;
let enter = false;

mp.events.add("playerLeaveVehicle", (vehicle) => {
    try {
        if (state == 0) return;
		mp.players.local.setConfigFlag(32, true);
		state = 0;
		mp.events.call("updremen", false);
    } catch (e) { }
});

mp.events.add('safetyBeltKey', function () {
	if (!localplayer.isInAnyVehicle(false)) return;
    if(!loggedin || chatActive || editing || cuffed || global.menuOpened || localplayer.getVariable('InDeath') == true || mp.players.local.isFalling() || mp.players.local.isShooting() || mp.players.local.isSwimming() || mp.players.local.isSwimmingUnderWater()) return;
    if (state == 0) {
		mp.events.call("updremen", true);
        mp.players.local.setConfigFlag(32, false);
		mp.events.call('client::soundplay', "./sounds/buckle.ogg", 0.1);
        state = 1;
    } else {
		mp.events.call("updremen", false);
        mp.players.local.setConfigFlag(32, true);
		mp.events.call('client::soundplay', "./sounds/unbuckle.ogg", 0.1);
        state = 0;
    }
});

mp.events.add('blockMove', function (argument) {
    blockcontrols = argument;
});

mp.events.add('CUFFED', function (argument) {
    cuffed = argument;
});

mp.events.add('hasMoney', function (argument) {
    hasmoney = argument;
    if (!argument) localplayer.setEnableHandcuffs(false);
});

mp.events.add('safeZone', function (argument, max, name) {
	mp.gui.execute(`HUD.zones.name = ${JSON.stringify(name)}; HUD.zones.active = ${argument};`);
    isInSafeZone = argument;
	if (max != 540) {
		if (argument) 
		{
			if (global.localplayer.isInAnyVehicle(true)) {
				var vehspeed = mp.game.vehicle.getVehicleModelMaxSpeed(mp.players.local.vehicle.model); 
				mp.players.local.vehicle.setMaxSpeed(max == 100 ? vehspeed / 3 : max/3.6);
			}
		}
		else {
			if (global.localplayer.isInAnyVehicle(true)) { 
				mp.players.local.vehicle.setMaxSpeed(540/3.6);
			}
		}
	}
});

mp.keys.bind(0x47, false, function () { // G Menu Veh
    if (global.menuCheck() || global.localplayer.getVariable('InDeath') == true && !global.localplayer.isInAnyVehicle(false)) return;
    if (global.circleOpen) {
        global.CloseCircle();
        return;
    }
    if (!loggedin || chatActive || entity == null || new Date().getTime() - lastCheck < 1000) return;
    switch (global.entity.type) {
        case "object":
            if (entity && mp.objects.exists(global.entity)) {
                mp.events.callRemote('oSelected', global.entity);
            }
            global.entity = null;
            return;
        case "player":
            mp.gui.cursor.visible = true;
            global.OpenCircle('Игрок', 0);
            break;
        case "vehicle":
			if (global.localplayer.getVariable("TakeHijackingItem") >= 0) {
                mp.events.callRemote("PutHijackingItemHouseInVehicle", global.entity);
                return;
            }
            
            if(global.entity.getVariable("ACCESS") == "DUMMY") return;
			if (global.localplayer.getVariable("DELIVERYCLUB_ORDER_TAKEN") == true && global.entity.getVariable("DELIVERY_CAR") == true) {
				mp.events.callRemote("server::deliveryclub:takefood", global.entity);
				return
			}
			if (global.localplayer.getVariable('PLAYERHASITEMHH') == true)
			{
			   mp.events.callRemote("server::addininvhouseheist", global.entity);
			   return;
			}
            mp.gui.cursor.visible = true;
            global.OpenCircle('Машина', 0);
            break;
    }
    lastCheck = new Date().getTime();
});
global.AltmenuPopUpOpen = false;
mp.keys.bind(0x47, false, function () { // G Menu Player
    if (global.menuCheck() || global.localplayer.getVariable('InDeath') == true) return;
    // player
    if (circleOpen) {
        global.CloseCircle();
        return;
    }

    if (nearestObject && mp.players.exists(nearestObject) && !global.localplayer.isInAnyVehicle(false)) {
		if (!loggedin || chatActive || nearestObject == null || new Date().getTime() - lastCheck < 1000 || global.localplayer.isInAnyVehicle(false)) return;
        global.entity = nearestObject;
        mp.gui.cursor.visible = true;
        global.OpenCircle('Игрок', 0);
    }
	else {
		if (!loggedin || chatActive || editing || global.menuOpened && !AltmenuPopUpOpen || new Date().getTime() - lastCheck < 1000 || localplayer.isInAnyVehicle(false)) return;
		if (!AltmenuPopUpOpen) {
			global.menuOpen(false);
			AltmenuPopUpOpen = true;
			global.menu.execute(`popup.boomboxplaced=${global.localplayer.getVariable("BOOMBOXON")}; popup.active = true`);
		}
		else {
			global.menuClose(false);
			AltmenuPopUpOpen = false
		}
		lastCheck = new Date().getTime();
	}

    lastCheck = new Date().getTime();
});

mp.keys.bind(0x45, false, function () { // Take Item
    if (global.menuCheck() || global.localplayer.getVariable('InDeath') == true) return;	
    if (nearestObject && nearestObject.type == 'object' && mp.objects.exists(nearestObject)) {
        mp.events.callRemote('oSelected', nearestObject);
    }

    lastCheck = new Date().getTime();
});


var truckorderveh = null;

mp.events.add('SetOrderTruck', (vehicle) => {
    try {
        if(truckorderveh == null) truckorderveh = vehicle;
		else truckorderveh = null;
    } catch (e) {
	}
});

global.playerMovingDisabled = false;

mp.events.add('render', () => {
	try {
		if (!loggedin) return;
			mp.game.player.restoreStamina(100);
			mp.game.player.setLockonRangeOverride(1.5);
			mp.game.controls.disableControlAction(1, 7, true);
			mp.game.controls.disableControlAction(1, 199, true); //Pause Menu (P)
		
		if (global.pocketEnabled) {
	        mp.game.controls.disableControlAction(2, 0, true);
	    }
		
		if (playerMovingDisabled) {
			mp.game.controls.disableControlAction(0, 21, true); /// бег
			mp.game.controls.disableControlAction(0, 22, true); /// прыжок
			mp.game.controls.disableControlAction(0, 31, true); /// вперед назад
			mp.game.controls.disableControlAction(0, 30, true); /// влево вправо
			mp.game.controls.disableControlAction(0, 24, true); /// удары
			mp.game.controls.disableControlAction(0, 25, true); /// INPUT_AIM
			mp.game.controls.disableControlAction(0, 257, true); /// стрельба
			mp.game.controls.disableControlAction(1, 200, true); // esc
			mp.game.controls.disableControlAction(0, 140, true); /// удары R
			mp.game.controls.disableControlAction(0, 257, true); // INPUT_ATTACK2
		}

	    if (blockcontrols) {
		    mp.game.controls.disableAllControlActions(2);
			mp.game.controls.enableControlAction(2, 30, true);
	        mp.game.controls.enableControlAction(2, 31, true);
		    mp.game.controls.enableControlAction(2, 32, true);
			mp.game.controls.enableControlAction(2, 1, true);
	        mp.game.controls.enableControlAction(2, 2, true);
		}
		if (hasmoney) {
	        global.localplayer.setEnableHandcuffs(true);
        }
        if (isInSafeZone) {
            mp.game.controls.disableControlAction(2, 24, true);
            mp.game.controls.disableControlAction(2, 69, true);
            mp.game.controls.disableControlAction(2, 70, true);
            mp.game.controls.disableControlAction(2, 92, true);
            mp.game.controls.disableControlAction(2, 114, true);
            mp.game.controls.disableControlAction(2, 121, true);
            mp.game.controls.disableControlAction(2, 140, true);
            mp.game.controls.disableControlAction(2, 141, true);
            mp.game.controls.disableControlAction(2, 142, true);
            mp.game.controls.disableControlAction(2, 257, true);
            mp.game.controls.disableControlAction(2, 263, true);
            mp.game.controls.disableControlAction(2, 264, true);
            mp.game.controls.disableControlAction(2, 331, true);
            // mp.gui.execute(`HUD.green=${true}`)
        }
        else{
            // mp.gui.execute(`HUD.green=${false}`)
        }
		
		if (mp.keys.isDown(32) && cuffed && new Date().getTime() - lastCuffUpdate >= 3000) {
			mp.events.callRemote("cuffUpdate");
	        lastCuffUpdate = new Date().getTime();
		}
		
		if (!global.localplayer.isInAnyVehicle(false) && !global.localplayer.isDead()) {
	        if (!global.circleOpen)
            global.entity = getLookingAtEntity();
	        getNearestObjects();
		    if (global.entity != null && global.entity.getVariable('INVISIBLE') == true) global.entity = null;
		}
        else {
            getNearestObjects();
            if (global.entity != nearestObject) global.entity = null;
		}
		
		//if (global.localplayer.isInWater())
		//{
		//	mp.events.callRemote('deattachfromtrunk', true);		
		//}
		
        if (!global.menuOpened && nearestObject && mp.players.exists(nearestObject)) {
		    const xy = mp.game.graphics.world3dToScreen2d(global.entity.position.x, global.entity.position.y, global.entity.position.z);
            if (xy != null && xy.x != null) {    
                global.drawText("Взаимодействие (G)", xy.x, xy.y, 0.2, 255,255,255,255)
            }
		}         
		else if (!global.menuOpened && nearestObject && nearestObject.type == 'object' && mp.objects.exists(nearestObject)) {
            if (nearestObject.position != null) {
                const xy = mp.game.graphics.world3dToScreen2d(nearestObject.position.x, nearestObject.position.y, nearestObject.position.z);
                if (xy != null && xy.x != null) {    
                    global.drawText(global.itemsData[JSON.stringify(nearestObject.getVariable("ITEM").ID)] + "\n Поднять (E)", xy.x, xy.y, 0.2, 255,255,255,255)
                }
            }
		}
        else if (!global.menuOpened && global.entity != null && !global.localplayer.isInAnyVehicle(false)) {
			if(truckorderveh == null || global.entity != truckorderveh) {
                if(global.entity.getVariable("ACCESS") != "DUMMY") {
                    const xy = mp.game.graphics.world3dToScreen2d(global.entity.position.x, global.entity.position.y, global.entity.position.z);
                    if (xy != null && xy.x != null) {    
                        global.drawText("Взаимодействие (G)", xy.x, xy.y, 0.2, 255,255,255,255);
                    }
                }
			} else if(global.entity == truckorderveh) {
				mp.game.graphics.drawText("Ваш трейлер", [global.entity.position.x, global.entity.position.y, global.entity.position.z], {
					font: 4,
					color: [255, 255, 255, 255],
					scale: [1.2, 1.2],
					outline: true
				});
			}
        }
	} catch (e) {
        mp.game.graphics.notify('RE:' + e.toString());
    }
});