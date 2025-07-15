global.playerheading = require("./src/utils/rotatorplayer");
global.chatActive = false;
global.loggedin = false;
global.localplayer = mp.players.local;

mp.gui.execute("window.location = 'http://package/browser/index.html'");
if (mp.storage.data.chatcfg == undefined) {
    mp.storage.data.chatcfg = {
		timestamp: 0,
		chatsize: 0,
		fontstep: 0,
		alpha: 1
	};
    mp.storage.flush();
}
global.sleep = function(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}
var pedsaying = null;
var pedtext = "";
var pedtext2 = null;
var pedtimer = false;

var pressedraw = false;
var pentloaded = false;
var emsloaded = false;

const walkstyles = [null,"move_m@brave","move_m@confident","move_m@drunk@verydrunk","move_m@fat@a","move_m@shadyped@a","move_m@hurry@a","move_m@injured","move_m@intimidation@1h","move_m@quick","move_m@sad@a","move_m@tool_belt@a"];
const moods = [null,"mood_aiming_1", "mood_angry_1", "mood_drunk_1", "mood_happy_1", "mood_injured_1", "mood_stressed_1", "mood_sulk_1"];
mp.game.streaming.requestClipSet("move_m@brave");
mp.game.streaming.requestClipSet("move_m@confident");
mp.game.streaming.requestClipSet("move_m@drunk@verydrunk");
mp.game.streaming.requestClipSet("move_m@fat@a");
mp.game.streaming.requestClipSet("move_m@shadyped@a");
mp.game.streaming.requestClipSet("move_m@hurry@a");
mp.game.streaming.requestClipSet("move_m@injured");
mp.game.streaming.requestClipSet("move_m@intimidation@1h");
mp.game.streaming.requestClipSet("move_m@quick");
mp.game.streaming.requestClipSet("move_m@sad@a");
mp.game.streaming.requestClipSet("move_m@tool_belt@a");
var admingm = false;

mp.game.object.doorControl(mp.game.joaat('gabz_mrpd_cells_door'), 484.1764, -1007.734, 26.4800520, true, 0.0, 50.0, 0); //door close
mp.game.audio.setAudioFlag("DisableFlightMusic", true);

function SetWalkStyle(entity, walkstyle) {

		if (walkstyle == '') entity.resetMovementClipset(0.0);
		else entity.setMovementClipset(walkstyle, 0.0);

}

global.getMinimapAnchor = function() {
    let sfX = 1.0 / 20.0;
    let sfY = 1.0 / 20.0;
    let safeZone = mp.game.graphics.getSafeZoneSize();
    let aspectRatio = mp.game.graphics.getScreenAspectRatio(false);
    let resolution = mp.game.graphics.getScreenActiveResolution(0, 0);
    let scaleX = 1.0 / resolution.x;
    let scaleY = 1.0 / resolution.y;

    let minimap = {
        width: scaleX * (resolution.x / (4 * aspectRatio)),
        height: scaleY * (resolution.y / 5.674),
        scaleX: scaleX,
        scaleY: scaleY,
        leftX: scaleX * (resolution.x * (sfX * (Math.abs(safeZone - 1.0) * 10))),
        bottomY: 1.0 - scaleY * (resolution.y * (sfY * (Math.abs(safeZone - 1.0) * 10))),
    };

    minimap.rightX = minimap.leftX + minimap.width;
    minimap.topY = minimap.bottomY - minimap.height;
    return minimap;
}

function SetMood(entity, mood) {

		if (mood == null) entity.clearFacialIdleAnimOverride();
		else mp.game.invoke('0xFFC24B988B938B38', entity.handle, mood, 0);

}

mp.events.add('chatconfig', function (a, b) {
	if(a == 0) mp.storage.data.chatcfg.timestamp = b;
    else if(a == 1) mp.storage.data.chatcfg.chatsize = b;
	else if(a == 2) mp.storage.data.chatcfg.fontstep = b;
	else mp.storage.data.chatcfg.alpha = b;
	mp.storage.flush();
});

mp.events.add('setFriendList', function (friendlist) {
	friends = {};
	friendlist.forEach(friend => {
		friends[friend] = true;
    });
});

mp.events.add('setClientRotation', function (player, rots) {
	if (player !== undefined && player != null && localplayer != player) player.setRotation(0, 0, rots, 2, true);
});

mp.events.add('setWorldLights', function (toggle) {

		mp.game.graphics.resetLightsState();
		for (let i = 0; i <= 16; i++) {
			if(i != 6 && i != 7) mp.game.graphics.setLightsState(i, toggle);
		}

});

mp.events.add('setDoorLocked', function (model, x, y, z, locked, angle) {
    mp.game.object.doorControl(model, x, y, z, locked, 0, 0, angle);
});

mp.events.add('changeChatState', function (state) {
    chatActive = state;
	mp.gui.execute(`HUD.active=${state}`);
});

var JobMenusBlip = [];
mp.events.add('JobMenusBlip', function (uid, type, position, names, dir) {
    if (typeof JobMenusBlip[uid] != "undefined") {
        JobMenusBlip[uid].destroy();
        JobMenusBlip[uid] = undefined;
    }
    if (dir != undefined) {
        JobMenusBlip[uid] = mp.blips.new(type, position,
            {
                name: names,
                scale: 1,
                color: 4,
                alpha: 255,
                drawDistance: 100,
                shortRange: false,
                rotation: 0,
                dimension: 0
            });
    }

});

mp.events.add('deleteJobMenusBlip', function (uid) {
    if (typeof JobMenusBlip[uid] == "undefined") return;
    JobMenusBlip[uid].destroy();
    JobMenusBlip[uid] = undefined;
});

var player = mp.players.local;
mp.events.add("startdiving", () => {
    player.setMaxTimeUnderwater(1000);
});
mp.events.add("stopdiving", () => {
    player.setMaxTimeUnderwater(10);
});


mp.peds.new(0x49EA5685, new mp.Vector3(144.8581, -373.5612, 43.65), 35.74032);
mp.peds.new(0xEAC2C7EE, new mp.Vector3(1695.806, 43.05446, 161.7473), 99.60);

mp.events.add('JobStatsInfo', (money) => {
	mp.gui.execute(`HUD.SetJobInfo(${money})`);
});
mp.events.add('CloseJobStatsInfo', () => {
    mp.gui.execute(`HUD.jobs.active = false`);
});
mp.events.add('client::postal:hud', (a) => {
	mp.gui.execute(`HUD.postal.parcel = ${a}`);
});

mp.events.add("blackday", (check) => {
	for (let i = 0; i <= 16; i++)
	{
		mp.game.graphics.setLightsState(i, check);
	}
});

require('./src/utils/keys.js');

global.formatIntZero = function(num, length) { 
    return ("0" + num).slice(length); 
} 

global.rotator = require("./src/utils/VehicleRotator");

const MeleeWeapon = ["weapon_dagger", "weapon_bat", "weapon_bottle", "weapon_crowbar", "weapon_unarmed", "weapon_flashlight", "weapon_golfclub", "weapon_hammer", "weapon_hatchet","weapon_knuckle","weapon_knife","weapon_machete","weapon_switchblade","weapon_nightstick","weapon_wrench","weapon_battleaxe","weapon_poolcue","weapon_stone_hatchet"]
setInterval(() => {
	MeleeWeapon.map(name => {
	let hash = mp.game.joaat(name.toUpperCase());
	if (mp.game.invoke(0x39, mp.players.local.handle, hash, false)) {
			mp.game.controls.enableControlAction(32, 142, true);
	} else {
		mp.game.controls.disableControlAction(32, 142, true);
		}
	});
}, 1000);
mp.objects.new(mp.game.joaat('prop_ss1_14_garage_door'), [-7.422924, -658.9207, 33.35459],{rotation: [0, 0, 365],alpha: 255,dimension: 0});

mp.game.audio.setAudioFlag("DisableFlightMusic", true);

require('./src/family/familycreator.js');
require('./src/family/familymanager.js');

//casino========================
require('./src/casino/blackjack');
//require('./src/casino/insidetrack');
require('./src/casino/casino');
require('./src/casino/wall');
require('./src/casino/casimarket');
require('./src/casino/barcasino');
require('./src/casino/luckywheel/index');
require('./src/casino/carlottery/index');

require('main.js');

require('./src/systems/crosshair/index')
require('./src/systems/rentcar/index')
require('./src/systems/containers/index')
//   UTILS   //
require('./src/utils/checkpoints.js');
require('./src/utils/VehicleAttach');
require('./src/utils/afksystem');
require('./src/utils/methods');
require('./src/utils/drone');
require('./src/utils/shootingrange')
require('./src/itemsnames.js')
require('./src/utils/discord')
require('./src/utils/timer')
//   HOUSE   //
require('./src/house/furniture');
//VEHICLE========================
require('./src/vehicle/vehiclesync');
require('./src/vehicle/tuningauto');
require('./src/vehicle/changenum');
require('./src/vehicle/pushcar')
require('./src/vehicle/driftmode')
//===============================

//ADMIN==========================
require('./src/admin/fly');
require('./src/admin/admesp');
require("./src/admin/spmenu");
require('./src/admin/adminpanel');
require('./src/admin/wtp');
//===============================

//PLAYER=========================
require('./src/player/menus');
require('./src/player/circle');
require('./src/player/basicsync');
require('./src/player/gangzones');
require('./src/player/board');  
require('./src/player/gamertag');
require('./src/player/voice');
require('./src/player/finger');
require('./src/player/character');
require('./src/player/render');
require('./src/player/housemanager');
require('./src/player/hud');
require('./src/player/gangarena');
require('./src/player/aparts');
require('./src/player/weaponsDamage_1');
require('./src/player/animation');
require('./src/player/animList');
//===============================

//OTHER=========================
require('./src/systems/rebinder/RebindKeys')
require('./src/systems/cinema/index.js');
require('./src/systems/RadioTalk/RadioTalk')
require('./src/systems/boombox/index');
require('./src/systems/boombox/xmr.js');
require('./src/systems/radar/raycasting');
require('./src/systems/radar/index');
require('./src/systems/roulette/roulette');
require('./src/systems/metro');
//===============================

//WORLD==========================
require('./src/world/bigmap');
require('./src/world/rappel');
require('./src/world/environment');
require('./src/world/ipls');
require('./src/world/peds');
//================================
//  CONFIGS  //
require('./src/configs/tattoo.js');
require('./src/configs/barber.js');
require('./src/configs/clothes.js');
require('./src/configs/natives.js');
require('./src/configs/tuning.js');


//CAYO PERICO ISLAND===========================
let g_bIslandLoaded = false 
const islandVec = new mp.Vector3(4840.571, -5174.425, 2.0)
mp.game.invoke("0x9A9D1BA639675CF1", "HeistIsland", g_bIslandLoaded);
mp.game.invoke("0x5E1460624D194A38", g_bIslandLoaded);
setInterval(()=> { 
    let localPlayerPos = mp.players.local.position
    if(islandVec.subtract(new mp.Vector3(localPlayerPos.x, localPlayerPos.y, localPlayerPos.z)).length() < 1500){
        if(!g_bIslandLoaded){
            g_bIslandLoaded = true
            mp.game.invoke("0x9A9D1BA639675CF1", "HeistIsland", g_bIslandLoaded);
            mp.game.invoke("0x5E1460624D194A38", g_bIslandLoaded);
        }
    }else{
        if(g_bIslandLoaded){
            g_bIslandLoaded = false
            mp.game.invoke("0x9A9D1BA639675CF1", "HeistIsland", g_bIslandLoaded);
            mp.game.invoke("0x5E1460624D194A38", g_bIslandLoaded);
        }
    }
},1000)
//===============================================

var friends = {};

if (mp.storage.data.friends == undefined) {
    mp.storage.data.friends = {};
    mp.storage.flush();
}

mp.events.add('newFriend', function (player, pass) {
    if (player && mp.players.exists(player)) {
        mp.storage.data.friends[player.name] = true;
        mp.storage.flush();
    }
});

// // // // // // //
const mSP = 30;
var prevP = mp.players.local.position;
var localWeapons = {};

function distAnalyze() {
	if(new Date().getTime() - global.lastCheck < 100) return; 
	global.lastCheck = new Date().getTime();
    let temp = mp.players.local.position;
    let dist = mp.game.gameplay.getDistanceBetweenCoords(prevP.x, prevP.y, prevP.z, temp.x, temp.y, temp.z, true);
    prevP = mp.players.local.position;
    if (mp.players.local.isInAnyVehicle(true)) return;
    if (dist > mSP) {
        mp.events.callRemote("acd", "fly");
    }
}

global.serverid = 1;

mp.events.add('ServerNum', (server) => {
   global.serverid = server;
});

global.acheat = {
    pos: () => prevP = mp.players.local.position,
    guns: () => localWeapons = playerLocal.getAllWeapons(),
    start: () => {
        setInterval(distAnalyze, 2000);
    }
}
global.auth = {
	browser: null,
	loading: true,
	state: false,
	ending: false,
	hour: 12
}
mp.events.add('authready', () => {
    require('./src/player/auth.js');
});


mp.events.add('acpos', () => {
    global.acheat.pos();
})
// // // // // // //
var spectating = false;
var sptarget = null;

mp.keys.bind(Keys.VK_R, false, function () { // R key
		if (!loggedin || chatActive || new Date().getTime() - global.lastCheck < 1000 || mp.gui.cursor.visible) return;
		var current = currentWeapon();
		if (current == -1569615261 || current == 911657153) return;
		var ammo = mp.game.invoke(getNative("GET_AMMO_IN_PED_WEAPON"), localplayer.handle, current);
		if (mp.game.weapon.getWeaponClipSize(current) == ammo) return;
		mp.events.callRemote("playerReload", current, ammo);
		global.lastCheck = new Date().getTime();

});

var ammosweap = 0;
var givenWeapon = -1569615261;
var to = false;
const currentWeapon = () => mp.game.invoke(getNative("GET_SELECTED_PED_WEAPON"), localplayer.handle);

// on player give weapon
mp.events.add('client::setweapon', function (weaponHash) {
	weaponHash = parseInt(weaponHash);
	givenWeapon = weaponHash;
	mp.game.invoke(getNative("MAKE_PED_RELOAD"), localplayer.handle);
	mp.game.invoke(getNative("SET_PED_AMMO"), mp.players.local.handle, weaponHash, 9000);
	mp.game.invoke(getNative("GIVE_WEAPON_TO_PED"), mp.players.local.handle, weaponHash, 1500, false, true);
	mp.game.invoke(getNative("MAKE_PED_RELOAD"), mp.players.local.handle);
});

// on player remove weapon
mp.events.add('client::removeweapon', function (weaponHash) {
	weaponHash = parseInt(weaponHash);
	givenWeapon = -1569615261;
	mp.game.invoke(getNative("SET_PED_AMMO"), mp.players.local.handle, weaponHash, 0);
	mp.game.invoke(getNative("REMOVE_WEAPON_FROM_PED"), mp.players.local.handle, weaponHash);
});


mp.events.add('wgive', (weaponHash, ammo, isReload, equipNow) => {
    weaponHash = parseInt(weaponHash);
	if (weaponHash == 126349499)
	{
		to = weaponHash;
	}
	
    ammo = parseInt(ammo);
    ammo = ammo >= 9999 ? 9999 : ammo;
    givenWeapon = weaponHash;
    ammo += mp.game.invoke(getNative("GET_AMMO_IN_PED_WEAPON"), localplayer.handle, weaponHash);
    mp.game.invoke(getNative("SET_PED_AMMO"), localplayer.handle, weaponHash, 0);
	ammosweap = ammo;
    mp.gui.execute(`HUD.ammo=${ammo};`);
    mp.game.invoke(getNative("GIVE_WEAPON_TO_PED"), localplayer.handle, weaponHash, ammo, false, equipNow);

    if (isReload) {
        mp.game.invoke(getNative("MAKE_PED_RELOAD"), localplayer.handle);
    }
});

mp.events.add('takeOffWeapon', (weaponHash) => {

        weaponHash = parseInt(weaponHash);
        var ammo = mp.game.invoke(getNative("GET_AMMO_IN_PED_WEAPON"), localplayer.handle, weaponHash);
		if(ammo == ammosweap) mp.events.callRemote('playerTakeoffWeapon', weaponHash, ammo, 0);
		else mp.events.callRemote('playerTakeoffWeapon', weaponHash, ammosweap, 1);
		ammosweap = 0;
		mp.game.invoke(getNative("SET_PED_AMMO"), localplayer.handle, weaponHash, 0);
		mp.game.invoke(getNative("REMOVE_WEAPON_FROM_PED"), localplayer.handle, weaponHash);
		givenWeapon = -1569615261;
		mp.gui.execute(`HUD.ammo=0;`);

});
mp.events.add('serverTakeOffWeapon', (weaponHash) => {

        weaponHash = parseInt(weaponHash);
        var ammo = mp.game.invoke(getNative("GET_AMMO_IN_PED_WEAPON"), localplayer.handle, weaponHash);
		if(ammo == ammosweap) mp.events.callRemote('takeoffWeapon', weaponHash, ammo, 0);
		else mp.events.callRemote('takeoffWeapon', weaponHash, ammosweap, 1);
		ammosweap = 0;
		mp.game.invoke(getNative("SET_PED_AMMO"), localplayer.handle, weaponHash, 0);
		mp.game.invoke(getNative("REMOVE_WEAPON_FROM_PED"), localplayer.handle, weaponHash);
		givenWeapon = -1569615261;
		mp.gui.execute(`HUD.ammo=0;`);

});

var petathouse = null;
mp.events.add('petinhouse', (petName, petX, petY, petZ, petC, Dimension) => {
	if(petathouse != null) {
		petathouse.destroy();
		petathouse = null;
	}
	switch(petName) {
		case "Хаски":
			petName = 1318032802;
			break;
		case "Пудель":
			petName = 1125994524;
			break;
		case "Мопс":
			petName = 1832265812;
			break;
		case "Ретривер":
			petName = 882848737;
			break;
		case "Ротвейлер":
			petName = 2506301981;
			break;
		case "Шеперд":
			petName = 1126154828;
			break;
		case "Вест-терьер":
			petName = 2910340283;
			break;
		case "Кошка":
			petName = 1462895032;
			break;
		case "Кролик":
			petName = 3753204865;
			break;
			
	}
	petathouse = mp.peds.new(petName, new mp.Vector3(petX, petY, petZ), petC, Dimension);
});

var checkTimer = setInterval(function () {
    var current = currentWeapon();
    if (localplayer.isInAnyVehicle(true)) {
        var vehicle = localplayer.vehicle;
        if (vehicle == null) return;

        if (vehicle.getClass() == 15) {
            if (vehicle.getPedInSeat(-1) == localplayer.handle || vehicle.getPedInSeat(0) == localplayer.handle) return;
        }
        else {
            if (canUseInCar.indexOf(current) == -1) return;
        }
    }

    if (currentWeapon() != givenWeapon) {
		ammosweap = 0;
        mp.game.invoke(getNative("GIVE_WEAPON_TO_PED"), localplayer.handle, givenWeapon, 1, false, true);
        mp.game.invoke(getNative("SET_PED_AMMO"), localplayer.handle, givenWeapon, 0);
        localplayer.taskReloadWeapon(false);
        localplayer.taskSwapWeapon(false);
        mp.gui.execute(`HUD.ammo=0;`);
    }
}, 100);
var canUseInCar = [
    453432689,
    1593441988,
    -1716589765,
    -1076751822,
    -771403250,
    137902532,
    -598887786,
    -1045183535,
    584646201,
    911657153,
    1198879012,
    324215364,
    -619010992,
    -1121678507,
];
mp.events.add('playerWeaponShot', (targetPosition, targetEntity) => {
	if (match) return;
    var current = currentWeapon();
    var ammo = mp.game.invoke(getNative("GET_AMMO_IN_PED_WEAPON"), localplayer.handle, current);
	
	/*if (ammo -1) {
        mp.game.cam.shakeGameplayCam("SMALL_EXPLOSION_SHAKE", 0.05);
    }*/ //Тряска
	
    mp.gui.execute(`HUD.ammo=${ammo};`);
	
	if (current != -1569615261 && current != 911657153) {
		if(ammosweap > 0) ammosweap--;
		if(ammosweap == 0 && ammo != 0) {
			mp.events.callRemote('takeoffWeapon', current, 0, 1);
			ammosweap = 0;
			mp.game.invoke(getNative("SET_PED_AMMO"), localplayer.handle, current, 0);
			mp.game.invoke(getNative("REMOVE_WEAPON_FROM_PED"), localplayer.handle, current);
			givenWeapon = -1569615261;
			mp.gui.execute(`HUD.ammo=0;`);
		}
	}
	
	if (ammo <= 0) {
		ammosweap = 0;
        localplayer.taskSwapWeapon(false);
        mp.gui.execute(`HUD.ammo=0;`);
    }
	
	if (to)
	{
        var ammo = mp.game.invoke(getNative("GET_AMMO_IN_PED_WEAPON"), localplayer.handle, current);
		if(ammo == ammosweap) mp.events.callRemote('playerTakeoffWeapon', current, ammo, 0);
		else mp.events.callRemote('playerTakeoffWeapon', current, ammosweap, 1);
		ammosweap = 0;
		mp.game.invoke(getNative("SET_PED_AMMO"), localplayer.handle, current, 0);
		mp.game.invoke(getNative("REMOVE_WEAPON_FROM_PED"), localplayer.handle, current);
		givenWeapon = -1569615261;
		mp.gui.execute(`HUD.ammo=0;`);
		to = false;
		mp.events.callRemote('takeoffWeapon', current, 0, 1);
	}
	
	
	
});

mp.events.add('render', () => {
        mp.game.controls.disableControlAction(2, 45, true);
		mp.game.controls.disableControlAction(1, 243, true); 
		
        mp.game.controls.disableControlAction(2, 12, true);
        mp.game.controls.disableControlAction(2, 13, true);
        mp.game.controls.disableControlAction(2, 14, true);
        mp.game.controls.disableControlAction(2, 15, true);
        mp.game.controls.disableControlAction(2, 16, true);
        mp.game.controls.disableControlAction(2, 17, true);

        mp.game.controls.disableControlAction(2, 37, true);
        mp.game.controls.disableControlAction(2, 99, true);
        mp.game.controls.disableControlAction(2, 100, true);

        mp.game.controls.disableControlAction(2, 157, true);
        mp.game.controls.disableControlAction(2, 158, true);
        mp.game.controls.disableControlAction(2, 159, true);
        mp.game.controls.disableControlAction(2, 160, true);
        mp.game.controls.disableControlAction(2, 161, true);
        mp.game.controls.disableControlAction(2, 162, true);
        mp.game.controls.disableControlAction(2, 163, true);
        mp.game.controls.disableControlAction(2, 164, true);
        mp.game.controls.disableControlAction(2, 165, true);

        mp.game.controls.disableControlAction(2, 261, true);
        mp.game.controls.disableControlAction(2, 262, true);

        if (currentWeapon() != -1569615261) { // heavy attack controls
            mp.game.controls.disableControlAction(2, 140, true);
            mp.game.controls.disableControlAction(2, 141, true);
            mp.game.controls.disableControlAction(2, 143, true);
            mp.game.controls.disableControlAction(2, 263, true);
        }

});

mp.events.add("Player_SetMood", (player, index) => {

        if (player !== undefined) {
            if (index == 0) player.clearFacialIdleAnimOverride();
			else mp.game.invoke('0xFFC24B988B938B38', player.handle, moods[index], 0);
        }

});

mp.events.add("Player_SetWalkStyle", (player, walk) => {
	try{
        if (player !== undefined) {
            if (walk == '') player.resetMovementClipset(0.0);
			else player.setMovementClipset(walk, 0.0);
        }
	}
	catch (e) {}
});

mp.events.add("playerDeath", function (player, reason, killer) {
    givenWeapon = -1569615261;
});

mp.events.add("removeAllWeapons", function () {
    givenWeapon = -1569615261;
});


mp.events.add('svem', (pm, tm) => {
	var vehc = localplayer.vehicle;
	vehc.setEnginePowerMultiplier(pm);
	vehc.setEngineTorqueMultiplier(tm);
});

var f10rep = new Date().getTime();

mp.events.add('f10report', (report) => {
	if (!loggedin || new Date().getTime() - f10rep < 3000) return;
    f10rep = new Date().getTime();
	mp.events.callRemote('f10helpreport', report);
});

mp.events.add('dmgmodif', (multi) => {
	mp.game.ped.setAiWeaponDamageModifier(multi);
});

mp.game.ped.setAiWeaponDamageModifier(0.5);
mp.game.ped.setAiMeleeWeaponDamageModifier(0.4);

mp.game.player.setMeleeWeaponDefenseModifier(0.25);
mp.game.player.setWeaponDefenseModifier(1.3);

var resistStages = {
    0: 0.0,
    1: 0.05,
    2: 0.07,
    3: 0.1,
};
mp.events.add("setResistStage", function (stage) {
    mp.game.player.setMeleeWeaponDefenseModifier(0.25 + resistStages[stage]);
    mp.game.player.setWeaponDefenseModifier(1.3 + resistStages[stage]);
});

mp.events.add('render', () =>
{
	const controls = mp.game.controls;	
	controls.enableControlAction(0, 23, true);
	controls.disableControlAction(0, 197, true);
});