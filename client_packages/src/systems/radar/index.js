var config = {
    distance: 25,    
    kilometers: true,
}
let localplayer = mp.players.local;

var raycasting = {

    camera: mp.cameras.new("gameplay"),

    directions: {
        front: 0,
        rear: 1
    },

    getEntity: function(directionf, distance = 10)
    {
        let veh = localplayer.vehicle;
        const rotation = veh.getRotation(5);

        const size = mp.game.gameplay.getModelDimensions(veh.model);
        /*const position = mp.game.object.getObjectOffsetFromCoords(veh.position.x, veh.position.y, veh.position.z, veh.getHeading(), 0, directionf == 0 ? size.max.y + 0.1 : size.min.y - 0.1 , 0);
        const target = mp.game.object.getObjectOffsetFromCoords(veh.position.x, veh.position.y, veh.position.z, veh.getHeading(), 0, 
            directionf == 0 ? size.max.y + 0.1 + distance : size.min.y - 0.1 - distance, 
            rotation.y * 1.25);*/

        if (directionf == 0) {
            var position = veh.getOffsetFromInWorldCoords(0.0, 5.0, 1.0);
            var target = veh.getOffsetFromInWorldCoords(0.0, 70.0, 0.0);
        }
        else {
            var position = veh.getOffsetFromInWorldCoords(0.0, -5.0, 1.0);
            var target = veh.getOffsetFromInWorldCoords(0.0, -70.0, 0.0);
        }

        let result = mp.raycasting.testCapsule(position, target, 10.0, [localplayer.vehicle.handle], [10]);
        if (frontcar.entity && typeof result !== 'undefined' && frontcar.entity.type == "vehicle" && result.entity.handle != localplayer.vehicle.handle) {
            if (typeof result.entity.type === 'undefined') return null;
            if (result.entity != null) {
                return result.entity;
            } 
        }
    }

}
var radar = {
    active: false,
    browser: mp.browsers.new("package://interface/modules/radar/index.html"),

    data: {
        buttons: { radar: false, front: true, rear: true, beep: true},
        up: { number: "", front: 0, fast: 0, patrol: 0 },
        down: { number: "", rear: 0, fast: 0, limit: 90 },
    },

    default: {
        buttons: { radar: false, front: true, rear: true, beep: true},
        up: { number: "", front: 0, fast: 0, patrol: 0 },
        down: { number: "", rear: 0, fast: 0, limit: 90 },
    },

    execute: function(str) {
        radar.browser.execute(str);
    },
}

mp.events.add("client::changeradarstate", (state) => {
	radar.data.buttons.radar = state
	radar.default.buttons.radar = state
});

mp.events.add("client::changeradarfrontstate", (state) => {
	radar.data.buttons.front = state
	radar.default.buttons.front = state
});

mp.events.add("client::changeradarrearstate", (state) => {
	radar.data.buttons.rear = state
	radar.default.buttons.rear = state
});

mp.events.add("client::changeradarbeepstate", (state) => {
	radar.data.buttons.beep = state
	radar.default.buttons.beep = state
});

mp.events.add("client::changeradarlimitstate", (count) => {
	radar.data.down.limit = count;
	radar.default.down.limit = count;
});

radar.toDefault = function() {
    for(let sub in radar.data)
        for(let key in radar.data[sub] )
            radar.data[sub][key] = radar.default[sub][key];
}


radar.Open = function() {
    if (radar.active || global.menuOpened || chatActive || localplayer.vehicle == null) return;
    radar.toDefault();
    radar.execute(`radar.open()`);
	 mp.gui.execute(`HUD.isVeh=0`);
    radar.active = true;
    mp.events.add("render", radar.Render);
	
	radar.data.buttons.radar = false
	radar.default.buttons.radar = false
	radar.data.buttons.front = true
	radar.default.buttons.front = true
	radar.data.buttons.rear = true
	radar.default.buttons.rear = true
	radar.data.buttons.beep = true
	radar.default.buttons.beep = true
	radar.data.down.limit = 90
	radar.default.down.limit = 90
}

radar.Close = function() {
    radar.execute(`radar.close()`);
	mp.gui.execute(`HUD.isVeh=1`);
    radar.active = false;
    mp.events.remove("render", radar.Render);
}

radar.CloseExitveh = function() {
    radar.execute(`radar.close()`);
    radar.active = false;
    mp.events.remove("render", radar.Render);
}

let directions = raycasting.directions;
let kilo = config.kilometers ? 3.7 : 1;
let keys = [ "up", "down" ]
mp.game.graphics.requestStreamedTextureDict("majestic_textures_001", true);
radar.Render = function() {
    if (!radar.active || localplayer.vehicle == null || !radar.data.buttons.radar) return;

    for(let keyu in keys) {

        let key = keys[keyu];
        let kyf = key == "up" ? "front" : "rear";

        if (radar.data.buttons[kyf])
        {
            let entity = raycasting.getEntity(directions[kyf], config.distance);
            

            if (entity != null && entity.type == "vehicle")
            {
                let speed = (entity.getSpeed() * kilo).toFixed();
                radar.data[key][kyf] = speed;
                radar.execute(`radar.info.${key}.${kyf}=${radar.data[key][kyf]}`);
				
                if (entity.getCoords(true).x != undefined && entity.getCoords(true) != undefined) {
                    const xyradar = mp.game.graphics.world3dToScreen2d(entity.getCoords(true).x, entity.getCoords(true).y, entity.getCoords(true).z+3);
                    if (xyradar != null && xyradar.x != null) {
                        global.drawText("< " + entity.getNumberPlateText().toString() + " >", xyradar.x, xyradar.y - 0.03 * 0.7, [0.3,0.3], 255, 255, 255, 90, 0, 3);
                        global.drawText(speed + " км/ч", xyradar.x, xyradar.y, [0.3,0.3], 255, 255, 255, 255, 0, 3);
                        global.drawSprite("commonmenu", 'common_medal', [0.6, 0.6], 0, {
                        r: 255,
                        g: 255,
                        b: 255,
                        a: 255
                        },  mp.game.graphics.world3dToScreen2d(entity.getCoords(true).x, entity.getCoords(true).y, entity.getCoords(true).z+2).x,  mp.game.graphics.world3dToScreen2d(entity.getCoords(true).x, entity.getCoords(true).y, entity.getCoords(true).z+2).y);
                        global.drawSprite('majestic_textures_001', 'speed', [0.4, 0.4], 0, {
                        r: 255,
                        g: 255,
                        b: 255,
                        a: 255
                        },  mp.game.graphics.world3dToScreen2d(entity.getCoords(true).x, entity.getCoords(true).y, entity.getCoords(true).z+3).x - 0.03 * 0.7,  mp.game.graphics.world3dToScreen2d(entity.getCoords(true).x, entity.getCoords(true).y, entity.getCoords(true).z+3).y);
                    }
                }
                
				if (speed > radar.data.down.limit)
                {
                    mp.game.audio.playSoundFrontend(-1, "TIMER_STOP", "HUD_MINI_GAME_SOUNDSET", true);
                }
				
                let numberPlate = entity.getNumberPlateText().toString();
                if (radar.data[key].number != numberPlate)
                {
                    radar.data[key].number = numberPlate;
                    radar.execute(`radar.info.${key}.number='${radar.data[key].number}'`);
                }

                if (radar.data[key].fast < speed)
                {
                    radar.data[key].fast = speed;
                    radar.execute(`radar.info.${key}.fast=${speed}`);
                }

            }
        }
    }
    
    let speed = (localplayer.vehicle.getSpeed() * kilo).toFixed();

    if (radar.data.up.patrol != speed && speed < 999)
    {
        radar.data.up.patrol = speed;
        radar.execute(`radar.info.up.patrol=${radar.data.up.patrol}`)
    }
}

radar.Toggle = function() {
    if(!radar.active)
        radar.Open();
    else
        radar.Close();
}

mp.events.add("client::openRadarCop", function() {
	radar.Toggle();
});

mp.keys.bind(Keys.VK_O, false, function () { // U key
    if (!loggedin || chatActive || editing || global.menuOpened || new Date().getTime() - lastCheck < 1000) return;
    mp.events.callRemote('openRadarCop');
    lastCheck = new Date().getTime();
});

mp.events.add("playerLeaveVehicle", function (vehicle, seat) {
    if (radar.active)
        radar.CloseExitveh();
})