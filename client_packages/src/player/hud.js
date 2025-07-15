global.showhud = true;

global.rgb = { r: 67, g: 140, b: 239};
mp.game.invoke('0xF314CF4F0211894E', 143, rgb.r, rgb.g, rgb.b, 255);
mp.game.ui.setHudColour(142, rgb.r, rgb.g, rgb.b, 255);

for (let i = 18; i < 19; i++) {
   mp.game.ui.setHudColour(i, rgb.r, rgb.g, rgb.b, 255);
}
for (let i = 9; i < 10; i++) {
   mp.game.ui.setHudColour(i, 220, 220, 220, 255);
}

let keysess = [
    {id: 48, key: "0"},
    {id: 49, key: "1"},
    {id: 50, key: "2"},
    {id: 51, key: "3"},
    {id: 52, key: "4"},
    {id: 53, key: "5"},
    {id: 54, key: "6"},
    {id: 55, key: "7"},
    {id: 56, key: "8"},
    {id: 57, key: "9"},

    {id: 65, key: "A"},
    {id: 66, key: "B"},
    {id: 67, key: "C"},
    {id: 68, key: "D"},
    {id: 69, key: "E"},
    {id: 70, key: "F"},
    {id: 71, key: "G"},
    {id: 72, key: "H"},
    {id: 73, key: "I"},
    {id: 74, key: "J"},
    {id: 75, key: "K"},

    {id: 76, key: "L"},
    {id: 77, key: "M"},
    {id: 78, key: "N"},
    {id: 79, key: "O"},

    {id: 80, key: "P"},
    {id: 81, key: "Q"},
    {id: 82, key: "R"},
    {id: 83, key: "S"},

    {id: 84, key: "T"},
    {id: 85, key: "U"},
    {id: 86, key: "V"},
    {id: 87, key: "W"},

    {id: 88, key: "X"},
    {id: 89, key: "Y"},
    {id: 90, key: "Z"},
    {id: 91, key: "S"},
    {id: 189, key: "-"},
    {id: 187, key: "="}
]

var hudstatus =
{
    safezone: null, // Last safezone size
    online: 0, // Last online int
	reonline: 0,

    street: null,
    area: null,

    invehicle: true,
    updatespeedTimeout: 0, // Timeout for optimization speedometer
    engine: false,
	remen: false,
    doors: true,
    fuel: 0,
    health: 0,
	mile: -1
}

mp.events.add('playerLeaveVehicle', function () {
	if (global.state === 0) return;
    global.localplayer.setConfigFlag(32, true);
	global.state = 0;
	global.enter = false;
	mp.events.call("updremen");
});

var typetostring = ["Оповещение", "Ошибка", "Успешно!", "Информация", "Предупреждение" ];
var statustostring = [ "alert", "error", "success", "info", "warning" ];

mp.events.add('notify', (type, layout, msg, time) => {
    mp.gui.execute(`notify.push(${type},'${msg}',${time})`);
});

mp.events.add('showHUD', (show) => {
	mp.gui.execute(`HUD.show=${show}`);
	
	mp.gui.execute(`HUD.mic=true`);

    var screen = mp.game.graphics.getScreenActiveResolution(0,0);
	
    mp.gui.execute(`updateSafeZoneSize(${screen.x},${screen.y},${hudstatus.safezone})`);
	var minimap = global.getMinimapAnchor();
    mp.gui.execute(`HUD.minimapFix=${minimap.rightX * 100}`);
    mp.gui.execute(`HUD.minimapFix2=${minimap.rightX * 110}`);
	
	var playerId = localplayer.getVariable('REMOTE_ID');
	var staticId = localplayer.getVariable('UID');

	mp.gui.execute(`HUD.id='${playerId}'`);
	mp.gui.execute(`HUD.static='${staticId}'`);
	
    mp.game.ui.displayAreaName(show);
    mp.game.ui.displayRadar(show);
    mp.game.ui.displayHud(show);
});

mp.events.add("client::hud:updatebonus", (a, b) => {
    mp.gui.execute(`HUD.setBonus(${a},${b})`);
});

global.showchat = true;
global.showhelp = true;

global.showveh = true;
global.showpos = true;
global.showeat = true;

mp.events.add('showHUDHELP', () => {
    global.showhelp = !global.showhelp;
	mp.gui.execute(`HUD.showhelp=${global.showhelp}`);
});

mp.events.add('showVEH', () => {
    global.showveh = !global.showveh;
    mp.gui.execute(`HUD.vehs=${global.showveh}`);
});

mp.events.add('showPOS', () => {
    global.showpos = !global.showpos;
    mp.gui.execute(`HUD.poss=${global.showpos}`);
});

mp.events.add('client::showhintHUD', (a,b) => {
	mp.gui.execute(`HUD.setHint(${a},${JSON.stringify(b)})`);
	if (a == true) {
		mp.game.audio.playSoundFrontend(-1, 'BACK', 'HUD_AMMO_SHOP_SOUNDSET', true);
	}
})

mp.events.add('client::sethudNotyHelp', (state, text = null) => {
	mp.gui.execute(`HUD.setHelpNoty(${state},${JSON.stringify(text)})`);
});

mp.events.add('client::addToMissionsOnHud', (a, b, c, d = 0, e = 0) => {
	mp.gui.execute(`HUD.setQuest(${a},${JSON.stringify(b)},${JSON.stringify(c)},${d},${e})`);
});

mp.events.add('showEAT', () => {
    global.showeat = !global.showeat;
    mp.gui.execute(`HUD.eats=${global.showeat}`);
});

mp.events.add('showCHAT', () => {
	global.showchat = !global.showchat;
	if (global.menuCheck())
		global.chatWasOpened = global.showchat;
	else
		mp.gui.chat.show(global.showchat);
});

mp.events.add('UpdateMoney', function (temp, amount) {
    let money = temp.toString().replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1 ");
    mp.gui.execute(`HUD.money="${money}"`);
});

mp.events.add('UpdateBank', function (temp, amount) {
    let money = temp.toString().replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1 ");
    mp.gui.execute(`HUD.bank="${money}"`);
});

mp.events.add('testsound', (a,b) => {
	mp.game.audio.playSoundFrontend(-1, a, b, true);
});

mp.events.add('client::addmoney', (a) => {
	mp.game.audio.playSoundFrontend(-1, 'LOCAL_PLYR_CASH_COUNTER_INCREASE', 'DLC_HEISTS_GENERAL_FRONTEND_SOUNDS', true);
	mp.gui.execute(`HUD.moneyaddstate='true'`);
	mp.gui.execute(`HUD.addmoney=${a}`);
	setTimeout(() => {
		var boolen = false;
		mp.gui.execute(`HUD.moneyaddstate=${boolen}`);
	}, 3000);
});

mp.events.add('setWanted', function (lvl) {
    mp.gui.execute(`HUD.activeStarts=${lvl}`);
});


var showHelp = false;

var showHelpF = false;

var passports = {};
mp.events.add('newPassport', function (player, pass) {
    if (player && mp.players.exists(player))
        passports[player.name] = pass;
});

mp.events.add('newFriend', function (player, pass) {
    if (player && mp.players.exists(player)) {
        mp.storage.data.friends[player.name] = true;
        mp.storage.flush();
    }
});


var showAltTabHint = false;
mp.events.add('showAltTabHint', function () {
    showAltTabHint = true;
    setTimeout(function () { showAltTabHint = false; }, 10000);
});
mp.events.add('setRpmNull', function () {
    mp.gui.execute(`HUD.turnovers=${0}`);
});

mp.events.add('sendRPMessage', (type, msg, players) => {

    var chatcolor = ``;

    players.forEach((id) => {
        var player = mp.players.atRemoteId(id);
        if (mp.players.exists(player)) {

            if (type === "chat" || type === "s") {
                let localPos = localplayer.position;
                let playerPos = player.position;
                let dist = mp.game.system.vdist(playerPos.x, playerPos.y, playerPos.z, localPos.x, localPos.y, localPos.z);
                var color = (dist < 2) ? "FFFFFF" :
                    (dist < 4) ? "F7F9F9" :
                        (dist < 6) ? "DEE0E0" :
                            (dist < 8) ? "C5C7C7" : "ACAEAE";

                chatcolor = color;
            }
			var bGender = true;
			if (player.model != 1885233650) bGender = false;
			var namegender = bGender ? 'Незнакомец ' : 'Heзнaкoмкa ';
			
            var name = (player === localplayer || localplayer.getVariable('IS_ADMIN') == true || passports[player.name] != undefined || mp.storage.data.friends[player.name] != undefined) ? `${player.name.replace("_", " ")} (${player.getVariable('REMOTE_ID')})` : `${namegender} (${id})`;
            msg = msg.replace("{name}", name);
        }
    });

    if (type === "chat" || type === "s")
        msg = `!{#${chatcolor}}${msg}`;

    mp.gui.chat.push(msg);
});
let intervalFishing;
let isIntervalCreated = false;
let isShowPrompt = false;
let isInZone = false;
let isEnter = false;
const checkConditions = () => {
    return (
        localplayer.getVariable('ROD_IN_HAND') == true&&
        !localplayer.isSwimming() &&
        !localplayer.vehicle &&
        !localplayer.getVehicleIsTryingToEnter() &&
        !localplayer.isInAir() &&
        !localplayer.isJumping() &&
        !localplayer.isDiving() &&
        !localplayer.isEvasiveDiving() &&
        !localplayer.isFalling() &&
        !localplayer.isSwimmingUnderWater() &&
        !localplayer.isClimbing()
    );
};
function degreesToIntercardinalDirection(dgr) {
  dgr %= 360.0;
  if (dgr >= 0.0 && dgr < 22.5 || dgr >= 337.5) return "N";
  if (dgr >= 22.5 && dgr < 67.5) return "NE";
  if (dgr >= 67.5 && dgr < 112.5) return "E";
  if (dgr >= 157.5 && dgr < 202.5) return "SE";
  if (dgr >= 112.5 && dgr < 157.5) return "S";
  if (dgr >= 202.5 && dgr < 247.5 || dgr > -112.5 && dgr <= -65.7) return "SW";
  if (dgr >= 247.5 && dgr <= 292.5 || dgr > -65.7 && dgr <= -22.5) return "W";
  if (dgr >= 292.5 && dgr < 337.5 || dgr > -22.5 && dgr <= 0) return "NW";
  return "NaN";
}

function GetTypeDepth() {
	let p = localplayer.position;
	if (p.y > 2000 && p.y < 5500 && p.x < 2500 && p.x > -700)
		return 1;
	else if (p.y > 2000 && p.y < 6000 && p.x > 2500 && p.x < -1800)
		return 2;
	else
		return 0;
}

mp.events.add('client::fish::game:finish', (a) => {
	mp.events.callRemote('server::fish::game:finish', a, parseInt(GetTypeDepth()));
	isEnter = false;
	mp.gui.execute(`HUD.fishing.typeplayer = 0`);
});

mp.events.add('client::fish::game:start', () => {
	mp.gui.execute(`HUD.openFishGame()`);
	mp.gui.execute(`HUD.fishing.typeplayer = 2`);
});

mp.keys.bind(Keys.VK_SPACE, false, function () { 
	if (!loggedin || chatActive || editing || !isShowPrompt || isEnter || global.menuCheck()) return;
	mp.events.callRemote('server::fish:start');
	mp.gui.execute(`HUD.fishing.typeplayer = 1`);
	isEnter = true;
});

mp.events.add('render', (nametags) => {
    if (!global.loggedin) return;
	// COMPASS====================================================
	mp.gui.execute(`HUD.compass=${JSON.stringify(degreesToIntercardinalDirection(localplayer.getHeading()))}`);
	// ===========================================================
	
    // Disable HUD components.    
    mp.game.ui.hideHudComponentThisFrame(1); // HUD_STARS
    mp.game.ui.hideHudComponentThisFrame(2); // HUD_WEAPON_ICON
    mp.game.ui.hideHudComponentThisFrame(3); // HUD_CASH
    mp.game.ui.hideHudComponentThisFrame(6); // HUD_VEHICLE_NAME
    mp.game.ui.hideHudComponentThisFrame(7); // HUD_AREA_NAME
    mp.game.ui.hideHudComponentThisFrame(8); // HUD_VEHICLE_CLASS
    mp.game.ui.hideHudComponentThisFrame(9); // HUD_STREET_NAME

    mp.game.ui.hideHudComponentThisFrame(19); // HUD_WEAPON_WHEEL
    mp.game.ui.hideHudComponentThisFrame(20); // HUD_WEAPON_WHEEL_STATS
    mp.game.ui.hideHudComponentThisFrame(22); // MAX_HUD_WEAPONS

	if (hudstatus.online != mp.players.length) {

        hudstatus.online = mp.players.length;
        mp.gui.execute(`HUD.online=${hudstatus.online}`);
    }
	
	if (isShowPrompt) {
		mp.game.controls.disableControlAction(0, 22, true);
	}
	if ((mp.game.controls.isControlPressed(0, 32) || 
        mp.game.controls.isControlPressed(0, 33) || 
        mp.game.controls.isControlPressed(0, 34) || 
        mp.game.controls.isControlPressed(0, 35)) && isEnter) 
    {
		isEnter = false;
		mp.events.callRemote("server::fish:stop");
		mp.gui.execute(`HUD.fishing.typeplayer = 0`);
	}
	if (checkConditions()) {

          if (!isIntervalCreated) {
            isIntervalCreated = true;
            intervalFishing = mp.timer.addInterval(() => {
                let heading = localplayer.getHeading() + 90;
                let point = {
                    x: localplayer.position.x + 3*Math.cos(heading * Math.PI / 180.0),
                    y: localplayer.position.y + 3*Math.sin(heading * Math.PI / 180.0),
                    z: localplayer.position.z
                };
				let ground = mp.game.gameplay.getGroundZFor3dCoord(point.x, point.y, point.z, 0.0, false);
				let water = Math.abs(mp.game.water.getWaterHeight(point.x, point.y, point.z, 0));

                if (water > 0 && ground < water && ground != 0) {
                    isShowPrompt = true;
                    isInZone = true;
					let depth = parseInt(GetTypeDepth());
                    mp.gui.execute(`HUD.fishing.state = true; HUD.fishing.type=${depth}`);
                } else {
                    if (isShowPrompt) {
                        mp.gui.execute(`HUD.fishing.state = false`);
                        isShowPrompt = false;
                    }
                    isInZone = false;
                }
            }, 1000);
        }
    }
    else {
        if (isIntervalCreated) {
			mp.gui.execute(`HUD.fishing.state = false`);
            isInZone = false;
            isShowPrompt = false;
            mp.timer.remove(intervalFishing);
            isIntervalCreated = false;
        }
    }
	
	
	
    var street = mp.game.pathfind.getStreetNameAtCoord(localplayer.position.x, localplayer.position.y, localplayer.position.z, 0, 0);
    let area  = mp.game.zone.getNameOfZone(localplayer.position.x, localplayer.position.y, localplayer.position.z);
    if(hudstatus.street != street || hudstatus.area != area)
    {
        hudstatus.street = street;
        hudstatus.area = area;   
        
        mp.gui.execute(`HUD.street='${mp.game.ui.getStreetNameFromHashKey(street.streetName)}'`);
        mp.gui.execute(`HUD.crossingRoad='${mp.game.ui.getLabelText(hudstatus.area)}'`);
    }
    
        var screen = mp.game.graphics.getScreenActiveResolution(0,0);
        mp.gui.execute(`updateSafeZoneSize(${screen.x},${screen.y},${hudstatus.safezone})`);

        var safezone = mp.game.graphics.getSafeZoneSize();
        var screen = mp.game.graphics.getScreenActiveResolution(0,0);
        var ratio = mp.game.graphics.getScreenAspectRatio(true);
        mp.gui.execute(`updatehud(${screen.x},${ratio},${safezone})`);

    
    if (localplayer.isInAnyVehicle(false) && !global.train) {
		

		if(localplayer.vehicle.getPedInSeat(-1) == localplayer.handle) {
			
			mp.game.audio.setRadioToStationName("OFF");
			
			if (!hudstatus.invehicle) 
			{ 
				mp.gui.execute(`HUD.isVeh=true`);
				let mile = localplayer.vehicle.getVariable('MILE');
				mp.gui.execute(`HUD.mile=${parseInt(mile)}`);
				hudstatus.invehicle = true;
			}

			var veh = localplayer.vehicle;

			let speed = (veh.getSpeed() * 3.6).toFixed();
			mp.gui.execute(`HUD.speed=${speed}`);
			mp.gui.execute(`HUD.turnovers=${veh.rpm.toFixed(20) * 9000}`);
			mp.gui.execute(`HUD.transmission=${veh.gear}`);
			
			mp.gui.execute(`HUD.isLights=${veh.getLightsState(1, 0).lightsOn}`);
			 
            var hp = veh.getEngineHealth(); //getHealth
			hp = hp.toFixed();
			if (hp !== hudstatus.health) {
				mp.gui.execute(`HUD.hp=${hp}`);
				hudstatus.health = hp;
			}
			
			var mile = veh.getVariable('MILE');
			if (mile !== hudstatus.mile) {
				mp.gui.execute(`HUD.mile=${parseInt(mile)}`);
				hudstatus.mile = mile;
			}

			if (veh.getVariable('PETROL') !== undefined && veh.getVariable('MAXPETROL') !== undefined) {
				let petrol = veh.getVariable('PETROL');
				let maxpetrol = veh.getVariable('MAXPETROL');

				if (hudstatus.fuel != petrol && petrol >= 0) {
					mp.gui.execute(`HUD.fuel=${petrol}`);
					hudstatus.fuel = petrol;
					
					if (petrol <= (maxpetrol * 0.2)) ifuel = 0;
					else if (petrol <= (maxpetrol * 0.6)) ifuel = 1;
					else ifuel = 2;
					mp.gui.execute(`HUD.ifuel=${ifuel}`);
				}
			}

		}
    } 
    else 
    {
        if (hudstatus.invehicle) mp.gui.execute(`HUD.isVeh=false`);
        hudstatus.invehicle = false;
    }
});

mp.events.add("playerEnterVehicle", playerEnterVehicleHandler);
function playerEnterVehicleHandler(player, vehicle, seat) {
	setTimeout(() => {
		try{
			mp.game.audio.setRadioToStationName("OFF");
			mp.events.call("updengine");
		}
		catch (e) {}
	}, 1000);
	mp.events.call("updlock");
}

mp.events.add('updengine', function (bool) {
	try {
		if (!localplayer.isInAnyVehicle(false)) return;
		
		var veh = localplayer.vehicle;
		
		var engine = bool;
		
		if (bool == undefined)
			engine = veh.getIsEngineRunning();
		
		
		if (engine != null && engine !== hudstatus.engine) 
		{
			mp.gui.execute(`HUD.isEngine=${engine}`);
			hudstatus.engine = engine;
		}
	} catch(e) {}
});

mp.events.add('updremen', function (stat) {
	try{
    var remen = stat;
    if (remen != null && remen !== hudstatus.remen) {
        mp.gui.execute(`HUD.isBelt=${remen}`);
        hudstatus.remen = remen;
    }
	}
	catch(e) {}
});


mp.events.add('updlock', function () {
	try{
	if (!localplayer.isInAnyVehicle(false)) return;
	
	var veh = localplayer.vehicle;
	
    if (veh.getVariable('LOCKED') !== undefined) 
    {
        var locked = veh.getVariable('LOCKED');
                
		if (hudstatus.doors !== locked) {
			mp.gui.execute(`HUD.isDoors=${locked}`)
			hudstatus.doors = locked;
		}
	}
	}
	catch(e) {}
});

mp.events.add('UpdateEat', function (temp, amount) {
    mp.gui.execute(`HUD.hunger=${temp}`);
});

mp.events.add('UpdateDrug', function (temp, amount) {
    mp.gui.execute(`HUD.drug=${temp}`);
});

mp.events.add('UpdateWater', function (temp, amount) {
    mp.gui.execute(`HUD.water=${temp}`);
});
global.projectName = "Freedom Project"
mp.events.add('render', function() {
	var online = mp.players.length;
	var playernameononline = localplayer.name.replace("_", " ");
	if (online != mp.players.length) {
		online = mp.players.length;
		mp.game.gxt.set("PM_PAUSE_HDR", "~g~" + `${global.projectName}` + "~r~| ~w~" + `${playernameononline}` + " (#" + `${localplayer.getVariable("UID")}` + ")" + " ~r~| ~w~Online: " + `${online}`);
	}
	mp.game.gxt.set("PM_PAUSE_HDR", "~g~" + `${global.projectName}` + " ~r~| ~w~" + `${playernameononline}` + " (#" + `${localplayer.getVariable("UID")}` + ")" + " ~r~| ~w~Online: " + `${online}`);
});

let freeze = false;
mp.keys.bind(Keys.VK_X, false, function () { // R key
	try {
		if (!loggedin || chatActive || mp.gui.cursor.visible) return;
        if (mp.players.local.vehicle.getClass() == 14) {
            if(!freeze){
                let speed = (mp.players.local.vehicle.getSpeed() * 3.6).toFixed();
                if(speed>6){
                    mp.events.call('notify', 4, 9, "Чтобы бросить якорь нужно остановить судно.", 7000);
                    return;
                }
            }
            freeze = !freeze;
            mp.players.local.vehicle.freezePosition(freeze);    
        }
	} catch { }
});

mp.game.ui.setMinimapComponent(6, !0, -1); // "Vespucci Beach lifeguard building"
mp.game.ui.setMinimapComponent(7, !0, -1); // "Beam Me Up (Grand Senora Desert)"
mp.game.ui.setMinimapComponent(8, !0, -1); // "Paleto Bay fire station building"
mp.game.ui.setMinimapComponent(9, !0, -1); // "Land Act Dam"
mp.game.ui.setMinimapComponent(10, !0, -1); // "Paleto Forest cable car station"
mp.game.ui.setMinimapComponent(11, !0, -1); // "Galileo Observatory"
mp.game.ui.setMinimapComponent(12, !0, -1); // "Engine Rebuils building (Strawberry)"
mp.game.ui.setMinimapComponent(13, !0, -1); // "Mansion pool (Richman)"
mp.game.ui.setMinimapComponent(14, !0, -1); // "Beam Me Up (Grand Senora Desert) (2)"
mp.game.ui.setMinimapComponent(15, !0, -1); // "Fort Zancudo"