const positions = [
    { 'position': { 'x': 1481.6602, 'y': -1435.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 1481.6602, 'y': -1575.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 1341.6602, 'y': -1575.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 1061.6602, 'y': -1715.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 1201.6602, 'y': -1715.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 1341.6602, 'y': -1715.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 1201.6602, 'y': -1575.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 1201.6602, 'y': -2415.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 1341.6602, 'y': -1855.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 1341.6602, 'y': -1995.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 1341.6602, 'y': -2135.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 1201.6602, 'y': -1855.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 1201.6602, 'y': -1995.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 1201.6602, 'y': -2135.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 1061.6602, 'y': -1855.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 1061.6602, 'y': -1995.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 1061.6602, 'y': -2275.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 1201.6602, 'y': -2275.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 1061.6602, 'y': -2135.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 1061.6602, 'y': -2415.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 921.6602, 'y': -2415.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 921.6602, 'y': -2135.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 921.6602, 'y': -1855.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 921.6602, 'y': -1995.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 921.6602, 'y': -1715.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 921.6602, 'y': -1575.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 921.6602, 'y': -2275.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 921.6602, 'y': -1435.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 781.6602, 'y': -2415.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 781.6602, 'y': -2135.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 781.6602, 'y': -1855.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 781.6602, 'y': -1995.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 781.6602, 'y': -1715.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 781.6602, 'y': -1575.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 781.6602, 'y': -2275.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 781.6602, 'y': -1435.6931, 'z': 67.30145 }, 'color': 10 },
	{ 'position': { 'x': 511.44025, 'y': -1315.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': 511.44025, 'y': -1455.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': 511.44025, 'y': -1595.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': 511.44025, 'y': -1735.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': 511.44025, 'y': -1875.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': 511.44025, 'y': -2015.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': 511.44025, 'y': -2155.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': 371.44025, 'y': -1315.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': 371.44025, 'y': -1455.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': 371.44025, 'y': -1595.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': 371.44025, 'y': -1735.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': 371.44025, 'y': -1875.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': 371.44025, 'y': -2015.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': 371.44025, 'y': -2155.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': 231.44025, 'y': -1315.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': 231.44025, 'y': -1455.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': 231.44025, 'y': -1595.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': 231.44025, 'y': -1735.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': 231.44025, 'y': -1875.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': 231.44025, 'y': -2015.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': 231.44025, 'y': -2155.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': 91.44025, 'y': -1455.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': 91.44025, 'y': -1595.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': 91.44025, 'y': -1735.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': 91.44025, 'y': -1875.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': 91.44025, 'y': -2015.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': 91.44025, 'y': -2155.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': -49.44025, 'y': -1455.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': -49.44025, 'y': -1595.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': -49.44025, 'y': -1735.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': -49.44025, 'y': -1875.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': -189.44025, 'y': -1455.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': -189.44025, 'y': -1595.7262, 'z': 28.146452 }, 'color': 10 },
	{ 'position': { 'x': -189.44025, 'y': -1735.7262, 'z': 28.146452 }, 'color': 10 },
]

var blips = [];
if (mp.storage.data.gangzones == undefined) {
    mp.storage.data.gangzones = [];
    mp.storage.flush();
}
mp.events.add('loadCaptureBlips', function (json) {
    var colors = JSON.parse(json);
    for (var i = 0; i < colors.length; i++) {
        positions[i].color = colors[i];
	}
	if(mp.storage.data.gangzones.length !== 0) {
		mp.events.call('unloadCaptureBlips');
	}
    positions.forEach(element => {
        const blip = mp.game.ui.addBlipForRadius(element.position.x, element.position.y, element.position.z, 70);
        mp.game.invoke(getNative("SET_BLIP_SPRITE"), blip, 5);
        mp.game.invoke(getNative("SET_BLIP_ALPHA"), blip, 75);
		mp.game.invoke(getNative("SET_BLIP_COLOUR"), blip, element.color);
        mp.storage.data.gangzones.push(blip);
		blips.push(blip);
    });
});
mp.events.add('unloadCaptureBlips', () =>{
	mp.storage.data.gangzones.forEach(element =>{
		mp.game.ui.removeBlip(element);
	});
	mp.storage.data.gangzones = [];
	mp.storage.flush();
});

var isCapture = false;
var captureAtt = 0;
var captureDef = 0;
var captureMin = 0;
var captureSec = 0;
var AttName = '';
var DefName = '';
var wrapper = mp.browsers.new('http://package/interface/modules/CaptHud/index.html');
wrapper.active = false;
mp.events.add('sendCaptureInformation', function (att, def, min, sec) {
    captureAtt = att;
    captureDef = def;
    captureMin = min;
    captureSec = sec;
});
mp.events.add('sendGangName', function (a, b) {
    AttName = a;
    DefName = b
});
mp.events.add('captureHud', function (argument) {
    isCapture = argument;
});

mp.events.add('setZoneColor', function (id, color) {
    if (blips.length == 0) return;
    mp.game.invoke(getNative("SET_BLIP_COLOUR"), blips[id], color);
});
mp.events.add('setZoneFlash', function (id, state, color) {
    if (blips.length == 1 || blips.length == 0) {
        if (state) {
            const blip = mp.game.ui.addBlipForRadius(positions[id].position.x, positions[id].position.y, positions[id].position.z, 71);
            mp.game.invoke(getNative("SET_BLIP_SPRITE"), blip, 5);
            mp.game.invoke(getNative("SET_BLIP_ALPHA"), blip, 175);
            mp.game.invoke(getNative("SET_BLIP_COLOUR"), blip, color);
            blips[id] = blip;
        }
        else {
            if (blips.length == 0) return;
            mp.game.invoke(getNative("SET_BLIP_ALPHA"), blips[id], 0);
        }
    }
	zonestatus.id = id;
    mp.game.invoke(getNative("SET_BLIP_FLASH_TIMER"), blips[id], 1000);
    mp.game.invoke(getNative("SET_BLIP_FLASHES"), blips[id], state);
});

var zonestatus =
{
    active: false,
    capDef: 0,
    capAtt: 0,
    capMin: 0,
    capSec: 0,
	id: 0,
}
mp.events.add('sendkillinfo', (object) => {
	wrapper.execute(`wrapper.addKills('${object}');`);
});
mp.events.add('render', () => {
	if (isCapture) {
        if (zonestatus.active == false)
        {
            zonestatus.active = true;
            wrapper.execute(`wrapper.open(${JSON.stringify(AttName)},${JSON.stringify(DefName)},${JSON.stringify(captureAtt)}, ${JSON.stringify(captureDef)},${JSON.stringify(captureMin)},${JSON.stringify(captureSec)})`);
			if (wrapper.active == false) {
				wrapper.active = true;
			}		
            wrapper.execute(`wrapper.updatekills(${JSON.stringify(zonestatus.capAtt)}, ${JSON.stringify(zonestatus.capDef)})`);
        }
        if (captureDef !== zonestatus.capDef || captureAtt !== zonestatus.capAtt)
        {
            zonestatus.capAtt = captureAtt;
            zonestatus.capDef = captureDef;
            wrapper.execute(`wrapper.updatekills(${JSON.stringify(zonestatus.capAtt)}, ${JSON.stringify(zonestatus.capDef)})`);   
           
        }
        if (captureMin !== zonestatus.capMin || captureSec !== zonestatus.capSec)
        {
            zonestatus.capMin = captureMin;
            zonestatus.capSec = captureSec;
            wrapper.execute(`wrapper.updatetime(${JSON.stringify(captureMin)}, ${JSON.stringify(captureSec)})`);           
        }
    }
    else
    {
        if (zonestatus.active == true)
        {
            zonestatus.active = false;
            wrapper.execute(`wrapper.active=false`);
        }
    }

    if (blips.length !== 0) {
        blips.forEach(blip => {
            mp.game.invoke(getNative("SET_BLIP_ROTATION"), blip, 0);
        })
    }
});

mp.events.add('quitcmd', function () {
    mp.events.callRemote('kickclient');
});