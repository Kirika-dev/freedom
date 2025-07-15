//AUTH CAMERA===================================
mp.game.ui.displayRadar(true);
mp.game.ui.displayHud(true);

const Natives = {
    SWITCH_OUT_PLAYER: '0xAAB3200ED59016BC',
    SWITCH_IN_PLAYER: '0xD8295AF639FD9CB8',
    IS_PLAYER_SWITCH_IN_PROGRESS: '0xD9D2CFFF49FAB35F',
    STOP_PLAYER_SWITCH: '0x95C0A5BBDC189AA1',
};
mp.game.invoke(Natives.STOP_PLAYER_SWITCH);

var cam = mp.cameras.new('default', new mp.Vector3(-1622.3545, -1098.4525, 27.562925), new mp.Vector3(0, 0, 0), 40);
cam.pointAtCoord(-1626.2261, -1099.9116, 27.897396);
cam.setActive(true);
mp.game.cam.renderScriptCams(true, false, 0, true, false);
global.menuOpen(true, false, false);

localplayer.freezePosition(true);

mp.events.add("render", () => {
	if (global.auth.ending == false) {
		mp.game.time.setClockTime(global.auth.hour, 0, 0);
		mp.gui.chat.show(false);
	}
	if(mp.game.controls.isDisabledControlJustReleased(0, 24) && !mp.gui.cursor.visible && !global.auth.loading && !global.auth.state) // ЛКМ
	{
		//mp.gui.chat.show(false);
		global.auth.state = true;
		mp.game.invoke(Natives.STOP_PLAYER_SWITCH);
		mp.game.invoke(Natives.SWITCH_OUT_PLAYER, localplayer.handle, 0, parseInt(1));
		setTimeout(function () { 
			
			global.auth.browser.execute(`Auth.openauth(${JSON.stringify(localplayer.name)})`);
			mp.gui.cursor.visible = true;
			if (mp.storage.data.login != undefined)
			{
				global.auth.browser.execute(`Auth.login="${mp.storage.data.login}"`);
				global.auth.browser.execute(`Auth.password="${mp.storage.data.pass}"`);
				global.auth.browser.execute(`Auth.$forceUpdate()`);
			}
		}, 500);
	}
});
//AUTH CAMERA===================================
setTimeout(() => {
    global.auth.browser = mp.gui;
}, 1000);
mp.events.add('guiReady', () => { 
	setTimeout(() => {
		loadAuthMenu();
	}, 1000);
});
function loadAuthMenu() {
	if (auth.browser != null) {
		auth.loading = false;
		mp.events.call('showHUD', false);
	}
	else loadAuthMenu();
}

var lastButAuth = 0;

mp.events.add('client::auth:restorepage', () => {
	if (global.auth.browser != null) {
		global.auth.browser.execute(`Auth.restoreActive = true`);
	}
});

var wrongPass = 3;
mp.events.add('client::auth:wrongpass', () => {
	if (wrongPass == 1) {
		global.auth.browser.execute(`Auth.tryes=0`);
		mp.events.callRemote('server::player:kick');
		return;
	}
	wrongPass--;
	global.auth.browser.execute(`Auth.tryes=${wrongPass}`);
});

mp.events.add('signin', function (username, pass, remember) {
    if (new Date().getTime() - lastButAuth < 3000) {
        mp.events.call('notify', 2, "Аккуратнее", "Слишком быстро", 5000);
        return;
    }
    lastButAuth = new Date().getTime();
	
	if (remember)
	{
		mp.storage.data.login = username;
		mp.storage.data.pass = pass;
		mp.storage.flush();
	}

    mp.events.callRemote('signin', username, pass)
});

mp.events.add('restorepass', function (state, nameorcode) {
    if (new Date().getTime() - lastButAuth < 3000) {
        mp.events.call('notify', 2, "Аккуратнее", "Слишком быстро", 5000);
        return;
    }
    lastButAuth = new Date().getTime();
    mp.events.callRemote('restorepass', state, nameorcode)
});

mp.events.add('AuthClose', function () {
	if (global.auth.browser != null) {
		global.auth.browser.execute(`Auth.active=false`);
	}
});

mp.events.add('signup', function (username, email, pass1, pass2) {
    if (new Date().getTime() - lastButAuth < 3000) {
        mp.events.call('notify', 4, "Ошибочка", "Слишком быстро", 3000);
        return;
    }
    lastButAuth = new Date().getTime();

    if (checkLgin(username) || username.length > 50) {
        mp.events.call('notify',  2, "Ошибочка...", "Логин не соответствует формату или слишком длинный!", 5000);
        return;
    }

    if (pass1 != pass2 || pass1.length < 3) {
        mp.events.call('notify',  2, "Ошибочка...", "Ошибка при вводе пароля!", 5000);
        return;
    }

    mp.events.callRemote('signup', username, pass1, email);
});

mp.events.add('spawn', function (data) {
    if (new Date().getTime() - lastButAuth < 1000) {
        mp.events.call('notify', 2, "Аккуратнее", "Слишком быстро", 5000);
        return;
    }
	if (global.auth.browser != null) {
        setTimeout(() => {
            lastButAuth = new Date().getTime();
            mp.events.callRemote('spawn', data);
        }, 200);
    }
});

mp.events.add('toslots', function (data) {
    mp.events.callRemote('selectchar', 1);
});

mp.events.add('spawnShow', function (data, pos) {
    try{
        if (data === false) {
            if (global.auth.browser != null) {
				global.auth.browser.execute(`Auth.active = false`);
            }
        }
        else {
            var streetName = mp.game.ui.getStreetNameFromHashKey(mp.game.pathfind.getStreetNameAtCoord(pos.x, pos.y, pos.z, 0, 0).streetName)
            global.auth.browser.execute(`Auth.set(${data}, ${JSON.stringify(streetName)})`);
        }
    }
    catch{}
});

mp.events.add('ready', async function () {
	mp.game.cam.renderScriptCams(false, true, 0, true, true);
	if (cam != null) {
		cam.destroy();
		cam = null;
	}
	global.auth.ending = true;
	await global.sleep(100);
	mp.events.callRemote('server::auth:smoothtime');
});

function ReadyPlayer() {
	global.loggedin = true;
	mp.events.callRemote("Enviroment_Set");
    mp.events.call('hideTun');
    mp.game.player.setHealthRechargeMultiplier(0);
	
	mp.game.invoke(Natives.SWITCH_IN_PLAYER, localplayer.handle);
	global.menuOpen(true, false, false);
	checkCamInAirCharSelect();
	
    global.menu = mp.browsers["new"]('http://package/interface/menu.html');
}
var inter = null;
function getTimeInMin(hour, min) {
	return Number(hour * 60 + min);
}
mp.events.add('client::auth:smoothtime', (data) => {
	var thistime = getTimeInMin(global.auth.hour, 0); //12
	var servertime = getTimeInMin(data[0], data[1]); //10
	var NewTime = thistime;
	var interval = 20;
	inter = setInterval(() => {
		if ((parseInt(servertime / interval) * interval) != NewTime) {
			if (servertime < thistime) 
			{
				NewTime = NewTime - interval;
			}
			if (servertime > thistime) 
			{
				NewTime = NewTime + interval;
			}
		}
		else {
			ReadyPlayer(); 
			clearInterval(inter); 
			return; 
		}
		var h = parseInt(NewTime / 60);
		var m = parseInt(NewTime % 60);
		// mp.gui.chat.push(`Hours ${h} ; Minutes ${m}`)
		mp.game.time.setClockTime(h, m, 0);
	}, 1);
});

function checkCamInAirCharSelect() {
    if (mp.game.invoke(Natives.IS_PLAYER_SWITCH_IN_PROGRESS)) {
        setTimeout(() => {
            checkCamInAirCharSelect();
        }, 400);
    } else {
        global.menuClose(true, false, true);
		mp.events.call('showHUD', true);
		localplayer.freezePosition(false);
    }
}


function checkLgin(str) {
    return !(/^[a-zA-Z1-9]*$/g.test(str));
}

global.checkName = function(str) {
    return !(/^[a-zA-Z]*$/g.test(str));
}

global.checkName2 = function(str) {
    let ascii = str.charCodeAt(0);
    if (ascii < 65 || ascii > 90) return false; // Если первый символ не заглавный, сразу отказ
    let bsymbs = 0; // Кол-во заглавных символов
    for (let i = 0; i != str.length; i++) {
        ascii = str.charCodeAt(i);
        if (ascii >= 65 && ascii <= 90) bsymbs++;
    }
    if (bsymbs > 2) return false; // Если больше 2х заглавных символов, то отказ. (На сервере по правилам разрешено иметь Фамилию, например McCry, то есть с приставками).
    return true; // string (имя или фамилия) соответствует
}