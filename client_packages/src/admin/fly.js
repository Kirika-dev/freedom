// credits to ragempdev

const controlsIds = {
    F11: 0x7A,
    W: 32,
    S: 33,
    A: 34,
    D: 35, 
    Space: 321,
    LCtrl: 326,
    LMB: 24,
	RMB: 25
};

global.constfly = {
    flying: false, f: 2.0, w: 2.0, h: 2.0, point_distance: 1000,
};
global.gameplayCam = mp.cameras.new('gameplay');

let direction = null;
let coords = null;

function pointingAt(distance) {
    const farAway = new mp.Vector3((direction.x * distance) + (coords.x), (direction.y * distance) + (coords.y), (direction.z * distance) + (coords.z));

    const result = mp.raycasting.testPointToPoint(coords, farAway, [1, 16]);
    if (result === undefined) {
        return 'undefined';
    }
    return result;
}

mp.events.add("AGM", (toggle) => {
	admingm = toggle;
	localplayer.setInvincible(toggle);
	mp.game.graphics.notify(toggle ? '~g~Бессмертие включено' : '~r~Бессмертие выключено');
});

mp.events.add("AGMTestDrive", (toggle) => {
	admingm = toggle;
	localplayer.setInvincible(toggle);
});

mp.keys.bind(Keys.VK_O, false, () => { 
  if (global.menuCheck() || localplayer.getVariable('InDeath') == true || chatActive || editing || cuffed | !loggedin || localplayer.getVariable('IS_ADMIN') !== true) return;
  if (!mp.game.recorder.isRecording()) {
    mp.game.recorder.start(1);
  } else {
    mp.game.recorder.stop();
  }
});

mp.keys.bind(Keys.VK_F9, false, function () {
    if (!loggedin || localplayer.getVariable('IS_ADMIN') !== true) return;

    direction = global.gameplayCam.getDirection();
    coords = global.gameplayCam.getCoord();

    global.constfly.flying = !global.constfly.flying;


    localplayer.setInvincible(global.constfly.flying);
    localplayer.freezePosition(global.constfly.flying);
    localplayer.setAlpha(global.constfly.flying ? 0 : 255);

    if (!global.constfly.flying && !controls.isControlPressed(0, controlsIds.Space)) {
        let position = localplayer.position;
        // position.z = mp.game.gameplay.getGroundZFor3dCoord(position.x, position.y, position.z, 0.0, false);
        localplayer.setCoordsNoOffset(position.x, position.y, position.z, false, false, false);
    }

    mp.events.callRemote('invisible', fly.flying);
    mp.game.graphics.notify(fly.flying ? '~g~Полёт включен' : '~r~Полёт выключен');
});

mp.events.add('render', () => {
    if (global.constfly.flying) {
        const controls = mp.game.controls;
        const fly = global.constfly;
        direction = global.gameplayCam.getDirection();
        coords = global.gameplayCam.getCoord();

        let updated = false;
        const position = mp.players.local.position;
		var speed;
        if(controls.isControlPressed(0, controlsIds.LMB)) speed = 1.0
		else if(controls.isControlPressed(0, controlsIds.RMB)) speed = 0.02
		else speed = 0.2
		if (controls.isControlPressed(0, controlsIds.W)) {
            if (fly.f < 8.0) fly.f *= 1.025;
            position.x += direction.x * fly.f * speed;
            position.y += direction.y * fly.f * speed;
            position.z += direction.z * fly.f * speed;
            updated = true;
        } else if (controls.isControlPressed(0, controlsIds.S)) {
            if (fly.f < 8.0) fly.f *= 1.025;
            position.x -= direction.x * fly.f * speed;
            position.y -= direction.y * fly.f * speed;
            position.z -= direction.z * fly.f * speed;
            updated = true;
        } else fly.f = 2.0;
        if (controls.isControlPressed(0, controlsIds.A)) {
            if (fly.l < 8.0) fly.l *= 1.025;
            position.x += (-direction.y) * fly.l * speed;
            position.y += direction.x * fly.l * speed;
            updated = true;
        } else if (controls.isControlPressed(0, controlsIds.D)) {
            if (fly.l < 8.0) fly.l *= 1.05;
            position.x -= (-direction.y) * fly.l * speed;
            position.y -= direction.x * fly.l * speed;
            updated = true;
        } else fly.l = 2.0;
        if (controls.isControlPressed(0, controlsIds.Space)) {
            if (fly.h < 8.0) fly.h *= 1.025;
            position.z += fly.h * speed;
            updated = true;
        } else if (controls.isControlPressed(0, controlsIds.LCtrl)) {
            if (fly.h < 8.0) fly.h *= 1.05;
            position.z -= fly.h * speed;
            updated = true;
        } else fly.h = 2.0;
        if (updated) mp.players.local.setCoordsNoOffset(position.x, position.y, position.z, false, false, false);
    }
});

mp.events.add('getCamCoords', (name) => {
    mp.events.callRemote('saveCamCoords', JSON.stringify(coords), JSON.stringify(pointingAt(fly.point_distance)), name);
});
