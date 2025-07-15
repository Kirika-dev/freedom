const vehicleNames = require('./autos.js');

global.menuOpened = true;
global.menu = null;
global.casinoOpened = false;

global.menuCheck = function() {
  if (global.menu !== null) {
    if (global.menuOpened) return true;
    return false;
  } 
  else return true;
};

global.hudWasOpened = true;
global.chatWasOpened = true;

global.casinoClose = function () {
    global.casinoOpened = false;
    global.menuOpened = false;
    mp.gui.cursor.visible = false;
}
global.casinoOpen = function () {
    mp.gui.cursor.visible = true;
    global.casinoOpened = true;
    global.menuOpened = true;
}

global.menuClose = function(showhud = true, blur = false, cursor = true) {
	global.menuOpened = false;
	if (showhud) {
		mp.events.call("showHUD", hudWasOpened);
		mp.gui.chat.show(global.chatWasOpened);
		if (global.bigmap.status != 3)
			mp.game.ui.displayRadar(true);
	}
	if (blur) 
		mp.game.graphics.transitionFromBlurred(100);
	if (cursor)
		mp.gui.cursor.visible = false;
};
global.menuOpen = function(showhud = true, blur = false, cursor = true) {
	global.menuOpened = true;
	hudWasOpened = global.showhud;
	global.chatWasOpened = global.showchat;
	if (showhud) {
		mp.events.call("showHUD", false);
		mp.gui.chat.show(false);
		mp.game.ui.displayRadar(false);
	}
	if (blur)
		mp.game.graphics.transitionToBlurred(100);
	if (cursor)
		mp.gui.cursor.visible = true;
};

mp.events.add("playerQuit", (player, exitType, reason) => {
	if (global.menu !== null) {
		if (player.name === localplayer.name) {
			global.menuClose();
			global.menu.destroy();
			global.menu = null;
			mp.game.graphics.transitionFromBlurred(100);
		}
	}
});

casinoroullete = mp.browsers.new('http://package/interface/modules/Casino/Roullete/index.html');
global.casinoOpened = false;

global.casinoClose = function () {
    global.casinoOpened = false;
    global.menuOpened = false;
    mp.gui.cursor.visible = false;
}
global.casinoOpen = function () {
    mp.gui.cursor.visible = true;
    global.casinoOpened = true;
    global.menuOpened = true;
}

mp.events.add('closecasino', () => {
    global.casinoClose();
    mp.events.call('showHUD', true);
    casinoroullete.execute(`casino.hide()`);
});
mp.events.add('client_casino_bet', (act, data) => {
    switch (act) {
        case 'open':
            if (global.menuCheck()) return;
            global.casinoOpen();
            mp.events.call('showHUD', false);
            casinoroullete.execute(`casino.show('${data}')`);
            break;
        case 'close':

            global.casinoClose();
            mp.events.call('showHUD', true);
            casinoroullete.execute(`casino.hide()`);
            break;
    }
});

mp.events.add('updateCasinoTime', (data) => {
    casinoroullete.execute(`casino.setTimeToStart('${data}')`);
});

mp.events.add('updateCasinoChips', (data) => {
    casinoroullete.execute(`casino.setChips('${data}')`);
});


global.casinoKeys = mp.browsers.new('http://package/interface/modules/Casino/UI/CasinoKeys/index.html');
casinoKeys.active = false; // скрываем нахой

mp.events.add('casinoKeys', (act, ...data) => {
    switch (act) {
        case 'setChips':
            global.casinoKeys.execute(`casinoKeys.setChips('${data[0]}', '${data[1]}')`);
            break;
        case 'setBet':
            global.casinoKeys.execute(`casinoKeys.setBet('${data[0]}', '${data[1]}')`);
            break;
        case 'setTime':
            global.casinoKeys.execute(`casinoKeys.setTime('${data[0]}', '${data[1]}')`);
            break;
        case 'toggleStart':
            global.casinoKeys.execute(`casinoKeys.toggleStart(${data[0]})`);
            break;
        case 'show':
            mp.events.call('showHUD', false);
            casinoKeys.active = true; // показываем
            global.casinoKeys.execute(`casinoKeys.show()`);
            break;
        case 'hide':
            mp.events.call('showHUD', true);
            casinoKeys.active = false; // показываем
            global.casinoKeys.execute(`casinoKeys.hide()`);
            break;
    }
});

mp.keys.bind(Keys.VK_P, false, function () {
    if (!loggedin || chatActive || editing || global.menuCheck() || cuffed || localplayer.getVariable('InDeath') == true) return;
    if (new Date().getTime() - global.lastCheck < 500) return;
    global.lastCheck = new Date().getTime();
	mp.events.callRemote('server::openfracmenu');
	
});

global.input = {
  head: "",
  desc: "",
  len: "",
  cBack: "",
  set: function(h, d, l, c) {
    (this.head = h), (this.desc = d);
    (this.len = l), (this.cBack = c);
    if (global.menuCheck()) return;
    mp.gui.execute(
      `input.set("${this.head}","${this.desc}","${this.len}");`
    );
  },
  open: function() {
    if (global.menuCheck()) return;
    mp.gui.execute("input.active=1");
    global.menuOpen(false);
  },
  close: function() {
    global.menuClose(false);
    mp.gui.execute("input.active=0");
  },
};
mp.events.add("input", (text) => {
  if (input.cBack == "") return;
  if (input.cBack == "setCruise") mp.events.call("setCruiseSpeed", text);
  else mp.events.callRemote("inputCallback", input.cBack, text);
  input.cBack = "";
  input.close();
});
mp.events.add("openInput", (h, d, l, c) => {
  if (global.menuCheck()) return;
  input.set(h, d, l, c);
  input.open();
});
mp.events.add("closeInput", () => {
  input.close();
});

global.dialog = {
  question: "",
  cBack: "",
  menuWasopened: false,
  lastTime: false,
  open: function() {
    global.dialog.lastTime = 0;
	mp.gui.execute(`dialog.title='${global.dialog.question}'`);
	mp.gui.execute(`dialog.active=1`);
    global.dialog.menuWasopened = global.menuOpened;
    mp.gui.cursor.visible = true;
    if (!global.menuOpened) global.menuOpen(false);
  },
  openMED: function() {
    global.dialog.lastTime = 0;
	mp.gui.execute(`deathMenu.active=1; deathMenu.title='${global.dialog.cBack}'; deathMenu.title2='${global.dialog.question}'; deathMenu.disableBTN=false; deathMenu.time=""`);
    global.dialog.menuWasopened = global.menuOpened;
    mp.gui.cursor.visible = true;
    if (!global.menuOpened) global.menuOpen(false);
  },
  close: function() {
    mp.gui.execute("dialog.active=0");
    if (!global.dialog.menuWasopened) global.menuClose(false);
  },
  closeMED: function() {
    mp.gui.execute(`deathMenu.active=0`);
	if (global.dialog.menuWasopened == undefined) return;
    if (!global.dialog.menuWasopened) global.menuClose(false);
  },
};
mp.events.add("openDialog", (c, q) => {
  global.dialog.cBack = c;
  global.dialog.question = q;
  global.dialog.open();
  mp.gui.cursor.visible = true;
});

mp.events.add("openDialogMED", (c, q) => {
  global.dialog.cBack = c;
  global.dialog.question = q;
  global.dialog.openMED();
  mp.gui.cursor.visible = true;
});

mp.events.add("closeDialog", () => {
  global.dialog.close();
});
mp.events.add("closeDialogMED", () => {
  global.dialog.closeMED();
});
mp.events.add("dialogCallback", (state) => {
  if (global.dialog.cBack == "tuningbuy") mp.events.call("tunbuy", state);
  else mp.events.callRemote("dialogCallback", global.dialog.cBack, state);
  global.dialog.close();
});

mp.events.add("closekpz", () => { global.menuClose(); });
mp.events.add("sendkpz", (title, time) => {mp.events.callRemote("sendkpz", title, time); });
mp.events.add("openkpz", (title) => { if (global.menuCheck()) return; global.menu.execute(`kpz.open('${title}')`); global.menuOpen();});

mp.events.add("dialogCallbackMED", (state) => {
  mp.events.callRemote("dialogCallbackMEDIC", state);
});
// DIAL //
mp.events.add("dial", (act, val, reset) => {
  switch (act) {
    case "open":
      if (reset == true) {
        global.menu.execute("dial.hide()");
        global.menuClose();
      }
      var off = Math.random(2, 5);
      global.menu.execute(`dial.val=${val};dial.off=${off};dial.show();`);
      global.menuOpen();
      break;
    case "close":
      global.menu.execute("dial.hide()");
      global.menuClose();
      break;
    case "call":
      mp.events.callRemote("dialPress", val);
      global.menuClose();
      break;
  }
});
// STOCK //
mp.events.add("openStock", (data) => {
  if (global.menuCheck()) return;
  global.menu.execute(`stock.count=JSON.parse('${data}');stock.show();`);
  global.menuOpen();
});
mp.events.add("closeStock", () => {
  global.menuClose();
});
mp.events.add("stockTake", (index) => {
  global.menuClose();
  switch (index) {
    case 3: //mats
      mp.events.callRemote("setStock", "mats");
      global.input.set("Взять маты", "Введите кол-во матов", 10, "take_stock");
      global.input.open();
      break;
    case 0: //cash
      mp.events.callRemote("setStock", "money");
      global.input.set(
        "Взять деньги",
        "Введите кол-во денег",
        10,
        "take_stock"
      );
      global.input.open();
      break;
    case 1: //healkit
      mp.events.callRemote("setStock", "medkits");
      global.input.set(
        "Взять аптечки",
        "Введите кол-во аптечек",
        10,
        "take_stock"
      );
      global.input.open();
      break;
    case 2: //weed
      mp.events.callRemote("setStock", "drugs");
      global.input.set(
        "Взять наркотики",
        "Введите кол-во наркоты",
        10,
        "take_stock"
      );
      global.input.open();
      break;
    case 4: //weapons stock
      mp.events.callRemote("openWeaponStock");
      break;
	 case 5:
	 mp.events.callRemote("setStock", "armor");
      global.input.set(
        "Взять бронежилеты",
        "Введите кол-во бронежилетов",
        10,
        "take_stock"
      );
      global.input.open();
      break;
	  case 6:
	 mp.events.callRemote("setStock", "koko");
      global.input.set(
        "Взять листья коки",
        "Введите кол-во листьев",
        10,
        "take_stock"
      );
      global.input.open();
      break;
  }
});
mp.events.add("stockPut", (index) => {
  global.menuClose();
  switch (index) {
    case 3: //mats
      mp.events.callRemote("setStock", "mats");
      global.input.set(
        "Положить маты",
        "Введите кол-во матов",
        10,
        "put_stock"
      );
      global.input.open();
      break;
    case 0: //cash
      mp.events.callRemote("setStock", "money");
      global.input.set(
        "Положить деньги",
        "Введите кол-во денег",
        10,
        "put_stock"
      );
      global.input.open();
      break;
    case 1: //healkit
      mp.events.callRemote("setStock", "medkits");
      global.input.set(
        "Положить аптечки",
        "Введите кол-во аптечек",
        10,
        "put_stock"
      );
      global.input.open();
      break;
    case 2: //weed
      mp.events.callRemote("setStock", "drugs");
      global.input.set(
        "Положить наркотики",
        "Введите кол-во наркоты",
        10,
        "put_stock"
      );
      global.input.open();
      break;
    case 4: //weapons stock
      mp.events.callRemote("openWeaponStock");
      break;
	case 5:
	mp.events.callRemote("setStock", "armor");
      global.input.set(
        "Положить бронежилеты",
        "Введите кол-во бронежилетов",
        10,
        "put_stock"
      );
      global.input.open();
      break;
	case 6:
	mp.events.callRemote("setStock", "koko");
      global.input.set(
        "Положить листья коки",
        "Введите кол-во листьев",
        10,
        "put_stock"
      );
      global.input.open();
      break;
  }
});
mp.events.add("stockExit", () => {
  global.menuClose();
});
// POLICE PC //
var pcSubmenu;
mp.events.add("pcMenu", (index) => {
  switch (index) {
    case 1:
      global.menu.execute("pc.clearWanted()");
      pcSubmenu = "clearWantedLvl";
      return;
    case 2:
      global.menu.execute("pc.openCar()");
      pcSubmenu = "checkNumber";
      return;
    case 3:
      global.menu.execute("pc.openPerson()");
      pcSubmenu = "checkPerson";
      return;
    case 4:
      mp.events.callRemote("checkWantedList");
      pcSubmenu = "wantedList";
      return;
    case 5:
      global.menu.execute("pc.hide()");
      global.menuClose();
      return;
  }
});
mp.events.add("pcMenuInput", (data) => {
  mp.events.callRemote(pcSubmenu, data);
});
mp.events.add("executeWantedList", (data) => {
  global.menu.execute(`pc.openWanted('${data}')`);
});
mp.events.add("executeCarInfo", (model, holder) => {
  global.menu.execute(`pc.openCar("${model}","${holder}")`);
});
mp.events.add(
  "executePersonInfo",
  (name, lastname, uuid, gender, wantedlvl, lic, number, states) => {
    global.menu.execute(
      `pc.openPerson("${name}","${lastname}","${uuid}","${gender}","${wantedlvl}","${lic}","${number}", "${states}")`
    );
  }
);

mp.events.add('client::acceptPressed', function () {
	menuClose();
	global.menu.execute(`infosss.hides()`);
	mp.events.callRemote("acceptPressed");
});

mp.events.add('client::closeinfo', function () {
	menuClose();
	global.menu.execute(`infosss.hides()`);
});

mp.events.add('client::openanswer', function (ayf) {
	if (global.menuCheck()) return;
    menuOpen();
	global.menu.execute(`infosss.show('${ayf}')`);
});

mp.events.add("openPc", () => {
  if (global.menuCheck()) return;
  global.menu.execute("pc.show()");
  global.menuOpen();
});
mp.events.add("closePc", () => {
  if (global.menu !== null) {
    global.menu.execute("pc.hide()");
    global.menuClose();
  }
});
// DOCS //
mp.events.add("passport", (data) => {
  if (global.menuCheck()) return;
  if (global.menu !== null) {
    global.menu.execute(`passport.set('${data}'); passport.show()`);
    global.menuOpen();
  }
});
mp.events.add("dochide", () => {
  global.menuClose();
});

mp.events.add("plastic", (data) => {
  if (global.menu !== null) {
    global.menu.execute(`plastic.set('${data}');`);
    if (global.menuCheck()) return;
    global.menu.execute("plastic.show()");
    global.menuOpen();
  }
});
mp.events.add("licenses", (data) => {
  global.menu.execute(`license.set('${data}');`);
  if (global.menuCheck()) return;
  global.menu.execute("license.show()");
  global.menuOpen();
});

mp.events.add('ydovgov', (data) => {
        if (global.menu !== null) {
            global.menu.execute(`ydovgov.set('${data}');`);
            if (global.menuCheck()) return;
            global.menu.execute('ydovgov.show()');
            global.menuOpen();
        }
    });

mp.events.add('ydovpolice', (data) => {
        if (global.menu !== null) {
            global.menu.execute(`ydovpolice.set('${data}');`);
            if (global.menuCheck()) return;
            global.menu.execute('ydovpolice.show()');
            global.menuOpen();
        }
    });

mp.events.add('ydovems', (data) => {
        if (global.menu !== null) {
            global.menu.execute(`ydovems.set('${data}');`);
            if (global.menuCheck()) return;
            global.menu.execute('ydovems.show()');
            global.menuOpen();
        }
    });

mp.events.add('ydovfib', (data) => {
        if (global.menu !== null) {
            global.menu.execute(`ydovfib.set('${data}');`);
            if (global.menuCheck()) return;
            global.menu.execute('ydovfib.show()');
            global.menuOpen();
        }
    });

mp.events.add('ydovarmy', (data) => {
        if (global.menu !== null) {
            global.menu.execute(`ydovarmy.set('${data}');`);
            if (global.menuCheck()) return;
            global.menu.execute('ydovarmy.show()');
            global.menuOpen();
        }
    });

mp.events.add('ydovnews', (data) => {
        if (global.menu !== null) {
            global.menu.execute(`ydovnews.set('${data}');`);
            if (global.menuCheck()) return;
            global.menu.execute('ydovnews.show()');
            global.menuOpen();
        }
    });

mp.events.add('ydovmws', (data) => {
        if (global.menu !== null) {
            global.menu.execute(`ydovmws.set('${data}');`);
            if (global.menuCheck()) return;
            global.menu.execute('ydovmws.show()');
            global.menuOpen();
        }
    });	
mp.events.add('ydovgr6', (data) => {
        if (global.menu !== null) {
            global.menu.execute(`ydovgr6.set('${data}');`);
            if (global.menuCheck()) return;
            global.menu.execute('ydovgr6.show()');
            global.menuOpen();
        }
    });		

mp.events.add("showJobMenu", (level, currentjob, id) => {
  mp.gui.cursor.visible = true;
  menu.execute(`openWorks(${level},${currentjob},${id});`);
});

mp.events.add("closeJobMenu", () => {
  mp.gui.cursor.visible = false;
  menu.execute(`jobselector.active=false;`);
});

mp.events.add("selectJob", (jobid) => {
  if (new Date().getTime() - global.lastCheck < 1000) return;
  global.lastCheck = new Date().getTime();
  mp.events.callRemote("jobjoin", jobid);
});

// SIMPLE MENU //
function openSM(type, data) {
  if (global.menuCheck()) return;
  global.menu.execute("menu.show()");
  switch (type) {
        //case 0: menu.execute(`openWorks(${data});`); break;
        case 1: menu.execute(`openShop('${data}');`); break;
        case 2: menu.execute(`openBlack('${data}');`); break;
        case 3: menu.execute(`openFib('${data}');`); break;
        case 4: menu.execute(`openLspd('${data}');`); break;
        case 5: menu.execute(`openArmy('${data}');`); break;
        case 6: menu.execute(`openGov('${data}');`); break;
        case 7: menu.execute(`openArmygun('${data}');`); break;
        case 8: menu.execute(`openGang('${data}');`); break;
        case 9: menu.execute(`openMafia('${data}');`); break;
		case 10:menu.execute(`openMwc('${data}');`);   break;
		case 11: menu.execute(`openFishShop('${data}');`); break;
		case 12:menu.execute(`openNac('${data}');`);   break;
		case 13: menu.execute(`openFracAuto('${data}');`); break;
		case 14: menu.execute(`openAutoShop('${data}');`); break;
		case 15: menu.execute(`openContainer('${data}');`); break;
		case 16: menu.execute(`openGarage('${data}');`); break;
		case 17: menu.execute(`openAirShop('${data}');`); break;
		case 18: menu.execute(`openGarageARMY('${data}');`); break;
  }
  global.menuOpen();
}
function closeSM() {
  global.menu.execute("menu.hide()");
  global.menuClose();
}
mp.events.add("smExit", () => {
  //mp.gui.chat.push('exit');
  closeSM();
});
mp.events.add("smOpen", (type, data) => {
  openSM(type, data);
});
mp.events.add("menu", (action, data) => {
  switch (action) {
    case "resign":
            mp.events.callRemote('jobjoin', -1);
            break;
        case "work":
            mp.events.callRemote('jobjoin', data);
            break;
        case "shop":
            mp.events.callRemote('shop', data);
            break;
        case "black":
            mp.events.callRemote('mavrbuy', data);
            break;
        case "fib":
            mp.events.callRemote('fbigun', data);
            break;
        case "lspd":
            mp.events.callRemote('lspdgun', data);
            break;
        case "gov":
            mp.events.callRemote('govgun', data);
            break;
		    case "army":
            mp.events.callRemote('armygun', data);
            break;
        case "gang":
            mp.events.callRemote('gangmis', data);
            break;
        case "mafia":
            mp.events.callRemote('mafiamis', data);
            break;
        case "mwc":
            mp.events.callRemote("mwcgun", data);
          break;
        case "fishshop":
          mp.events.callRemote('fishshop', data);
            break;
        case "nac":
            mp.events.callRemote('nacgun', data);
            break;	
        case "fracauto":
                mp.events.callRemote('fracauto', data);
                break;
        case "garage":
                mp.events.callRemote('garageauto', data);
                break;
        case "garagearmy":
                mp.events.callRemote('garagearmy', data);
                break;
        case "autoshop":
                mp.events.callRemote('autoshop', data);
                break;				
  }
});

// SM DATA //


mp.events.add('policeg', () => {
    let data = [
        "Дубинка",
        "Пистолет",
        "Combat PDW",
        "Сайга",
        "Tazer",
        "Бронежилет",
        "Аптечка",
        "Пистолетный калибр x12",
        "Малый калибр x30",
        "Дробь x6",
	"Коптер",
    ];
    openSM(4, JSON.stringify(data));
});
mp.events.add('fbiguns', () => {
    let data = [
        "Tazer",
        "Пистолет",
        "ПОС",
        "Карабин",
        "Снайперская винтовка",
        "Бронежилет",
        "Аптечка",
        "Пистолетный калибр x12",
        "Малый калибр x30",
        "Автоматный калибр x30",
        "Снайперский калибр x5",
        "Бейдж",
	"Коптер",
    ];
    openSM(3, JSON.stringify(data));
});
mp.events.add('govguns', () => {
    let data = [
        "Tazer",
        "Пистолет",
        "Advanced Rifle",
        "Gusenberg Sweeper",
        "Бронежилет",
        "Аптечка",
        "Пистолетный калибр x12",
        "Малый калибр x30",
        "Автоматный калибр x30",
    ];
    openSM(6, JSON.stringify(data));
});
mp.events.add('armyguns', () => {
    let data = [
        "Шокер",
        "Пистолет",
        "Пистолет-пулемёт",
        "Дробовик",
        "Укор. Пистолет",	
		"Штурмовой SMG",
		"Боевой пулемёт",
		"Продвинутая Винтовка",
		"Компактная винтовка",
		"Тяжелая снайп. винт.",
		"Снайперская винтовка",
		"Мощный дробовик",
		"Тяжелый дробовик",	
        "Бронежилет",
        "Аптечка",
        "Пистолетный калибр x12",
        "Малый калибр x30",
        "Автоматный калибр x30",
        "Снайперский калибр x5",
		"Дробь x6",
    ];
    openSM(7, JSON.stringify(data));
});
mp.events.add('mavrshop', () => {
    let data = [
        ["Услуга по отмыву денег", ""],
        ["Дрель для взлома", "1420$"],
        ["Отмычка для замков", "15$"],
        ["Военная отмычка", "85$"],
        ["Стяжки", "43$"],
        ["Мешок", "72$"],
        ["Понизить розыск", "14300$"],
    ];
    openSM(2, JSON.stringify(data));
});
mp.events.add('gangmis', () => {
    let data = [
        "Угон автотранспорта",
        "Перевозка автотранспорта",
    ];
    openSM(8, JSON.stringify(data));
});
mp.events.add('mafiamis', () => {
    let data = [
        "Перевозка оружия",
        "Перевозка денег",
        "Перевозка трупов",
    ];
    openSM(9, JSON.stringify(data));
});

mp.events.add("mwcguns", () => {
  let data = [
        "Шокер",
        "Пистолет",
        "Пистолет-пулемёт",
        "Дробовик",
        "Укор. Пистолет",
        "Карабин",
		"Пистолет 50 калибр",		
		"Винтажный пистолет",
		"Автомат. пистолет",		
		"Штурмовой SMG",
		"Боевой пулемёт",
		"Обрез",
		"Мощный дробовик",
		"Тяжелый дробовик",	
        "Бронежилет",
        "Аптечка",
        "Пистолетный калибр x12",
        "Малый калибр x30",
        "Автоматный калибр x30",
		"Дробь x6",
  ];
  openSM(10, JSON.stringify(data));
});

mp.events.add("nacguns", () => {
  let data = [
        "Tazer",
        "Пистолет",
        "ПОС",
        "Карабин",
        "Бронежилет",
        "Аптечка",
        "Пистолетный калибр x12",
        "Малый калибр x30",
        "Автоматный калибр x30",
  ];
  openSM(12, JSON.stringify(data));
});
var garage;
mp.events.add('fracauto', (json) => {
	garage = mp.browsers["new"]('http://package/interface/modules/GarageAutos/index.html');
    let data = JSON.parse(json);
    garage.execute(`wrapper.active = true`)
    garage.execute(`wrapper.num = 3`)
    garage.execute(`wrapper.cars=${JSON.stringify(data)}`);
	global.menuOpen(false, true);
});

mp.events.add('autoshop', (json) => {
    let data = JSON.parse(json);
    openSM(14, JSON.stringify(data));
});

mp.events.add('container', (json) => {
    let data = JSON.parse(json);
    openSM(15, JSON.stringify(data));
});

mp.events.add('client::closegarage', () => {
	if (garage != null) {
		garage.destroy();
		garage = null;
		global.menuClose(false, true);
	}
});

mp.events.add('garagearmy', (json) => {
	garage = mp.browsers["new"]('http://package/interface/modules/GarageAutos/index.html');
    let data = JSON.parse(json);
    garage.execute(`wrapper.active = true`)
    garage.execute(`wrapper.num = 2`)
    garage.execute(`wrapper.cars=${JSON.stringify(data)}`);
	global.menuOpen(false, true);
});

// ATM //
var atmIndex = 0;
mp.events.add("openatm", () => {
  if (global.menuCheck()) return;
  global.menu.execute("atm.active=1");
  global.menuOpen();
});
mp.events.add("closeatm", () => {
  global.menuClose();
  global.menu.execute("atm.reset();atm.active=0");
});
mp.events.add("setatm", (num, name, bal, sub) => {
  global.menu.execute(`atm.set('${num}','${name}','${bal}','${sub}')`);
});
mp.events.add("setbank", (bal) => {
  global.menu.execute(`atm.balance="${bal}"`);
});
mp.events.add("atmCB", (type, data) => {
  mp.events.callRemote("atmCB", type, data);
});
var atmTcheck = 0;
mp.events.add("atmVal", (data) => {
  if (new Date().getTime() - atmTcheck < 1000) {
    mp.events.callRemote("atmDP");
  } else {
    mp.events.callRemote("atmVal", data);
    atmTcheck = new Date().getTime();
  }
});
mp.events.add("atmOpen", (data) => {
  global.menu.execute(`atm.open(${data})`);
});
mp.events.add("atmOpenBiz", (data1, data2) => {
  global.menu.execute(`atm.open([3, ${data1}, ${data2}])`);
});

mp.events.add("atmOpenComp", (data1, data2) => {
  global.menu.execute(`atm.open([4, ${data1}, ${data2}])`);
});

mp.events.add("atmOpenBizp", (data1, data2) => {
  global.menu.execute(`atm.open([5, ${data1}, ${data2}])`);
});

mp.events.add("atm", (index, data) => {
  if (index == 4) {
    ATMTemp = data;
    global.menu.execute("atm.change(44)");
  } else if (index == 44) {
    mp.events.callRemote("atm", 4, data, ATMTemp);
    global.menu.execute("atm.reset()");
    return;
  } else if (index == 33) {
    mp.events.callRemote("atm", 3, data, ATMTemp);
  } else {
    mp.events.callRemote("atm", index, data);
    global.menu.execute("atm.reset()");
  }
});
let ATMTemp = "";
// ELEVATOR //
var liftcBack = "";
function openLift(type, cBack) {
  if (global.menuCheck()) return;
  let floors = [["Гараж", "1 этаж", "49 этаж", "Крыша"]];
  let json = JSON.stringify(floors[type]);
  global.menu.execute(`lift.set('${json}');lift.active=1;`);
  global.menuOpen();
  liftcBack = cBack;
}
function closeLift() {
  global.menuClose();
  global.menu.execute("lift.active=0;lift.reset();");
  liftcBack = "";
}
mp.events.add("openlift", (type, cBack) => {
  openLift(type, cBack);
});
mp.events.add("lift", (act, data) => {
  switch (act) {
    case "stop":
      closeLift();
      break;
    case "start":
      mp.events.callRemote(liftcBack, data);
      closeLift();
      break;
  }
});

// FRACTION MENU //

mp.events.add("openfm", (name, logs, ranks) => {
  if (global.menuCheck()) return;
  global.menuOpen();
  global.menu.execute("fmenu.active=1");
  global.menu.execute(`fmenu.menu=1`);
  global.menu.execute(`fmenu.btnactive[1]=true`);
  global.menu.execute(`fmenu.submenu=true`);
  global.menu.execute(`fmenu.name='${name}'`);
  global.menu.execute(`fmenu.setlogs(${logs})`);
  global.menu.execute(`fmenu.setranks(${ranks})`);
});

mp.events.add("setmem", (json, count, on, off) => {
  global.menu.execute(`fmenu.set('${json}',${count},${on},${off});`);
});

mp.events.add("closefm", () => {
  global.menuClose();
});

mp.events.add("fmenu", (act, data1, data2) => {
  mp.events.callRemote("fmenu", act, data1, data2);
  global.menuClose();
});
mp.events.add("SaveRanks", (data) => {
  mp.events.callRemote("SaveRanks", data);
});

mp.events.add("matsOpen", (isArmy, isMed) => {
  if (global.menuCheck()) return;
  global.menuOpen();
  global.menu.execute(`mats.show(${isArmy},${isMed})`);
});
mp.events.add("matsL", (act) => {
  //load
  global.menuClose();
  switch (act) {
    case 1:
      global.input.set("Загрузить маты", "Введите кол-во матов", 4, "loadmats");
      global.input.open();
      break;
    case 2:
      global.input.set("Загрузить маты", "Введите кол-во матов", 4, "loadmats");
      global.input.open();
      break;
    case 3:
      global.input.set(
        "Загрузить наркоту",
        "Введите кол-во наркоты",
        4,
        "loaddrugs"
      );
      global.input.open();
      break;
    case 4:
      global.input.set(
        "Загрузить аптечки",
        "Введите кол-во аптечек",
        4,
        "loadmedkits"
      );
      global.input.open();
      break;
  }
});
mp.events.add("matsU", (act) => {
  //unload
  global.menuClose();
  switch (act) {
    case 1:
      global.input.set(
        "Выгрузить маты",
        "Введите кол-во матов",
        4,
        "unloadmats"
      );
      global.input.open();
      break;
    case 2:
      global.input.set(
        "Выгрузить маты",
        "Введите кол-во матов",
        4,
        "unloadmats"
      );
      global.input.open();
      break;
    case 3:
      global.input.set(
        "Выгрузить наркоту",
        "Введите кол-во наркоты",
        4,
        "unloaddrugs"
      );
      global.input.open();
      break;
    case 4:
      global.input.set(
        "Выгрузить аптечки",
        "Введите кол-во аптечек",
        4,
        "unloadmedkits"
      );
      global.input.open();
      break;
  }
});

mp.events.add("bsearch", (act) => {
  global.menuClose();
  switch (act) {
    case 1:
      mp.events.callRemote("pSelected", global.circleEntity, "Посмотреть лицензии");
      break;
    case 2:
      mp.events.callRemote("pSelected", global.circleEntity, "Посмотреть паспорт");
      break;
  }
});
mp.events.add("bsearchOpen", (data) => {
  if (global.menuCheck()) return;
  global.menuOpen();
  global.menu.execute(`bsearch.active=true`);
  global.menu.execute(`bsearch.set('${data}')`);
});
// BODY CUSTOM //
global.getCameraOffset = function(pos, angle, dist) {
  //mp.gui.chat.push(`Sin: ${Math.sin(angle)} | Cos: ${Math.cos(angle)}`);

  angle = angle * 0.0174533;

  pos.y = pos.y + dist * Math.sin(angle);
  pos.x = pos.x + dist * Math.cos(angle);

  //mp.gui.chat.push(`X: ${pos.x} | Y: ${pos.y}`);

  return pos;
}
var bodyCamValues = {
  hair: { Angle: 0, Dist: 0.5, Height: 0.7 },
  beard: { Angle: 0, Dist: 0.5, Height: 0.7 },
  eyebrows: { Angle: 0, Dist: 0.5, Height: 0.7 },
  chesthair: { Angle: 0, Dist: 1, Height: 0.2 },
  lenses: { Angle: 0, Dist: 0.5, Height: 0.7 },
  lipstick: { Angle: 0, Dist: 0.5, Height: 0.7 },
  blush: { Angle: 0, Dist: 0.5, Height: 0.7 },
  makeup: { Angle: 0, Dist: 0.5, Height: 0.7 },

  torso: [
    { Angle: 0, Dist: 1, Height: 0.2 },
    { Angle: 0, Dist: 1, Height: 0.2 },
    { Angle: 0, Dist: 1, Height: 0.2 },
    { Angle: 180, Dist: 1, Height: 0.2 },
    { Angle: 180, Dist: 1, Height: 0.2 },
    { Angle: 180, Dist: 1, Height: 0.2 },
    { Angle: 180, Dist: 1, Height: 0.2 },
    { Angle: 305, Dist: 1, Height: 0.2 },
    { Angle: 55, Dist: 1, Height: 0.2 },
  ],
  head: [
    { Angle: 0, Dist: 1, Height: 0.5 },
    { Angle: 305, Dist: 1, Height: 0.5 },
    { Angle: 55, Dist: 1, Height: 0.5 },
    { Angle: 180, Dist: 1, Height: 0.5 },
    { Angle: 0, Dist: 0.5, Height: 0.5 },
    { Angle: 0, Dist: 0.5, Height: 0.5 },
  ],
  leftarm: [
    { Angle: 55, Dist: 1, Height: 0.0 },
    { Angle: 55, Dist: 1, Height: 0.1 },
    { Angle: 55, Dist: 1, Height: 0.1 },
  ],
  rightarm: [
    { Angle: 305, Dist: 1, Height: 0.0 },
    { Angle: 305, Dist: 1, Height: 0.1 },
    { Angle: 305, Dist: 1, Height: 0.1 },
  ],
  leftleg: [
    { Angle: 55, Dist: 1, Height: -0.6 },
    { Angle: 55, Dist: 1, Height: -0.6 },
  ],
  rightleg: [
    { Angle: 305, Dist: 1, Height: -0.6 },
    { Angle: 305, Dist: 1, Height: -0.6 },
  ],
};
global.bodyCam = null;
global.bodyCamStart = new mp.Vector3(0, 0, 0);

var tattooValues = [0, 0, 0, 0, 0, 0];
var tattooIds = ["torso", "head", "leftarm", "rightarm", "leftleg", "rightleg"];

var barberValues = {};
barberValues["hair"] = { Style: 0, Color: 0 };
barberValues["beard"] = { Style: 255, Color: 0 };
barberValues["eyebrows"] = { Style: 255, Color: 0 };
barberValues["chesthair"] = { Style: 255, Color: 0 };
barberValues["lenses"] = { Style: 0, Color: 0 };
barberValues["lipstick"] = { Style: 255, Color: 0 };
barberValues["blush"] = { Style: 255, Color: 0 };
barberValues["makeup"] = { Style: 255, Color: 0 };
var barberIds = [
  "hair",
  "beard",
  "eyebrows",
  "chesthair",
  "lenses",
  "lipstick",
  "blush",
  "makeup",
];

mp.events.add("barber", (act, id, val) => {
  if (new Date().getTime() - global.lastCheck < 50) return;
  global.lastCheck = new Date().getTime();
  switch (act) {
    case "buy":
      mp.events.callRemote(
        "buyBarber",
        id,
        barberValues[id].Style,
        barberValues[id].Color
      );
      break;
    case "style":
      switch (id) {
        case "hair":
          let gender = localplayer.getVariable("GENDER") ? 0 : 1;
          barberValues["hair"].Style = hairIDList[gender][val];
          localplayer.setComponentVariation(
            2,
            barberValues["hair"].Style,
            0,
            0
          );
          localplayer.setHairColor(barberValues["hair"].Color, 0);
          break;
        case "beard":
          barberValues["beard"].Style = val == 0 ? 255 : val - 1;
          localplayer.setHeadOverlay(
            1,
            barberValues["beard"].Style,
            100,
            barberValues["beard"].Color,
            barberValues["beard"].Color
          );
          break;
        case "eyebrows":
          barberValues["eyebrows"].Style = val == 0 ? 255 : val - 1;
          localplayer.setHeadOverlay(
            2,
            barberValues["eyebrows"].Style,
            100,
            barberValues["eyebrows"].Color,
            barberValues["eyebrows"].Color
          );
          break;
        case "chesthair":
          barberValues["chesthair"].Style = val == 0 ? 255 : val - 1;
          localplayer.setHeadOverlay(
            10,
            barberValues["chesthair"].Style,
            100,
            barberValues["chesthair"].Color,
            barberValues["chesthair"].Color
          );
          break;
        case "lenses":
          barberValues["lenses"].Style = val;
          localplayer.setEyeColor(val);
          break;
        case "lipstick":
          barberValues["lipstick"].Style = val == 0 ? 255 : val - 1;
          localplayer.setHeadOverlay(
            8,
            barberValues["lipstick"].Style,
            100,
            barberValues["lipstick"].Color,
            barberValues["lipstick"].Color
          );
          break;
        case "blush":
          barberValues["blush"].Style = val == 0 ? 255 : val - 1;
          localplayer.setHeadOverlay(
            5,
            barberValues["blush"].Style,
            100,
            barberValues["blush"].Color,
            barberValues["blush"].Color
          );
          break;
        case "makeup":
          barberValues["makeup"].Style = val == 0 ? 255 : val - 1;
          localplayer.setHeadOverlay(
            4,
            barberValues["makeup"].Style,
            100,
            barberValues["makeup"].Color,
            barberValues["makeup"].Color
          );
          break;
      }

      const camValues = bodyCamValues[id];
      const camPos = global.getCameraOffset(
        new mp.Vector3(
          bodyCamStart.x,
          bodyCamStart.y,
          bodyCamStart.z + camValues.Height
        ),
        localplayer.getRotation(2).z + 90 + camValues.Angle,
        camValues.Dist
      );

      bodyCam.setCoord(camPos.x, camPos.y, camPos.z);
      bodyCam.pointAtCoord(
        bodyCamStart.x,
        bodyCamStart.y,
        bodyCamStart.z + camValues.Height
      );
      break;
    case "color":
      switch (id) {
        case "hair":
          barberValues["hair"].Color = val;
          localplayer.setComponentVariation(
            2,
            barberValues["hair"].Style,
            0,
            0
          );
          localplayer.setHairColor(barberValues["hair"].Color, 0);
          break;
        case "beard":
          barberValues["beard"].Color = val;
          localplayer.setHeadOverlay(
            1,
            barberValues["beard"].Style,
            100,
            barberValues["beard"].Color,
            barberValues["beard"].Color
          );
          break;
        case "eyebrows":
          barberValues["eyebrows"].Color = val;
          localplayer.setHeadOverlay(
            2,
            barberValues["eyebrows"].Style,
            100,
            barberValues["eyebrows"].Color,
            barberValues["eyebrows"].Color
          );
          break;
        case "chesthair":
          barberValues["chesthair"].Color = val;
          localplayer.setHeadOverlay(
            10,
            barberValues["chesthair"].Style,
            100,
            barberValues["chesthair"].Color,
            barberValues["chesthair"].Color
          );
          break;
        case "lipstick":
          barberValues["lipstick"].Color = val;
          localplayer.setHeadOverlay(
            8,
            barberValues["lipstick"].Style,
            100,
            barberValues["lipstick"].Color,
            barberValues["lipstick"].Color
          );
          break;
        case "blush":
          barberValues["blush"].Color = val;
          localplayer.setHeadOverlay(
            5,
            barberValues["blush"].Style,
            100,
            barberValues["blush"].Color,
            barberValues["blush"].Color
          );
          break;
      }
      break;
  }
});
mp.events.add("tattoo", (act, id, val) => {
  if (new Date().getTime() - global.lastCheck < 50) return;
  global.lastCheck = new Date().getTime();
  switch (act) {
    case "buy":
      mp.events.callRemote(
        "buyTattoo",
        tattooIds.indexOf(id),
        tattooValues[tattooIds.indexOf(id)]
      );
      break;
    case "style":
      var tId = tattooIds.indexOf(id);
      tattooValues[tId] = val;

      const tattoo = tattoos[id][val];
      var hash = localplayer.getVariable("GENDER")
        ? tattoo.MaleHash
        : tattoo.FemaleHash;
      localplayer.clearDecorations();

      var playerTattoos = JSON.parse(localplayer.getVariable("TATTOOS"));
      for (let x = 0; x < playerTattoos[tId].length; x++) {
        // Очищаем ненужные татушки

        for (let i = 0; i < tattoo.Slots.length; i++) {
          if (playerTattoos[tId][x].Slots.indexOf(tattoo.Slots[i]) != -1) {
            playerTattoos[tId][x] = null;
            break;
          }
        }
      }

      for (
        let x = 0;
        x < 6;
        x++ // Восстанавливаем старые татуировки игрока, кроме тех, которые занимают очищенные слоты
      )
        if (playerTattoos[x] != null)
          for (let i = 0; i < playerTattoos[x].length; i++)
            if (playerTattoos[x][i] != null)
              localplayer.setDecoration(
                mp.game.joaat(playerTattoos[x][i].Dictionary),
                mp.game.joaat(playerTattoos[x][i].Hash)
              );

      localplayer.setDecoration(
        mp.game.joaat(tattoo.Dictionary),
        mp.game.joaat(hash)
      ); // Ну и применяем выбранную татуировку

      const camValues = bodyCamValues[id][tattoo.Slots[0]];
      const camPos = global.getCameraOffset(
        new mp.Vector3(
          bodyCamStart.x,
          bodyCamStart.y,
          bodyCamStart.z + camValues.Height
        ),
        localplayer.getRotation(2).z + 90 + camValues.Angle,
        camValues.Dist
      );

      bodyCam.setCoord(camPos.x, camPos.y, camPos.z);
      bodyCam.pointAtCoord(
        bodyCamStart.x,
        bodyCamStart.y,
        bodyCamStart.z + camValues.Height
      );
      break;
  }
});
mp.events.add("openBody", (isBarber, price) => {
  if (global.menuCheck()) return;

  if (isBarber) {
    barberValues["hair"] = { Style: 0, Color: 0 };
    barberValues["beard"] = { Style: 255, Color: 0 };
    barberValues["eyebrows"] = { Style: 255, Color: 0 };
    barberValues["chesthair"] = { Style: 255, Color: 0 };
    barberValues["lenses"] = { Style: 0, Color: 0 };
    barberValues["lipstick"] = { Style: 255, Color: 0 };
    barberValues["blush"] = { Style: 255, Color: 0 };
    barberValues["makeup"] = { Style: 255, Color: 0 };

    for (let i = 0; i < 8; i++) {
      let id = barberIds[i];
      let bizBarberPrices = [];
      let barberSkip = [];

      for (let x = 0; x < barberPrices[id].length; x++) {
        let tempPrice = (barberPrices[id][x] / 100) * price;
        bizBarberPrices[x] = tempPrice.toFixed();
      }

      mp.events.call(
        "setBody",
        id,
        JSON.stringify(bizBarberPrices),
        JSON.stringify(barberSkip)
      );
    }

    bodyCamStart = localplayer.position;
  } else {
    tattooValues = [0, 0, 0, 0, 0, 0];

    let gender = localplayer.getVariable("GENDER");

    for (let i = 0; i < 6; i++) {
      let id = tattooIds[i];

      let tattooPrices = [];
      let tattooSkips = [];

      for (let x = 0; x < tattoos[id].length; x++) {
        let tempPrice = (tattoos[id][x].Price / 100) * price;
        tattooPrices[x] = tempPrice.toFixed();

        let hash = gender ? tattoos[id][x].MaleHash : tattoos[id][x].FemaleHash;
        if (hash == "") tattooSkips.push(x);
      }

      bodyCamStart = new mp.Vector3(324.9798, 180.6418, 103.6665);

      mp.events.call(
        "setBody",
        id,
        JSON.stringify(tattooPrices),
        JSON.stringify(tattooSkips)
      );
    }
  }

  var camValues = isBarber ? bodyCamValues["hair"] : bodyCamValues["torso"][0];
  var pos = global.getCameraOffset(
    new mp.Vector3(
      bodyCamStart.x,
      bodyCamStart.y,
      bodyCamStart.z + camValues.Height
    ),
    localplayer.getRotation(2).z + 90 + camValues.Angle,
    camValues.Dist
  );
  bodyCam = mp.cameras.new("default", pos, new mp.Vector3(0, 0, 0), 50);
  bodyCam.pointAtCoord(
    bodyCamStart.x,
    bodyCamStart.y,
    bodyCamStart.z + camValues.Height
  );
  bodyCam.setActive(true);
  mp.game.cam.renderScriptCams(true, false, 500, true, false);

  global.menuOpen();
  global.menu.execute(`body.isBarber=${isBarber}`);
  global.menu.execute(`body.active=true`);

  mp.events.call("camMenu", true);
});
mp.events.add("closeBody", () => {
  if (new Date().getTime() - global.lastCheck < 50) return;
  global.lastCheck = new Date().getTime();
  global.menuClose();

  bodyCam.destroy();
  mp.game.cam.renderScriptCams(false, false, 500, true, false);

  localplayer.clearDecorations();
  mp.events.callRemote("cancelBody");

  mp.events.call("camMenu", false);
});
mp.events.add("setBody", (id, data1, data2) => {
  global.menu.execute(`body.set('${id}','${data1}','${data2}')`);
});

//AUTOSHOP================
let autoColors = ["Белый", "Серый", "Черный", "Красный", "Оранжевый", "Фиолетовый", "Синий"];
let autoModels = null;

let colors = {};
colors["Белый"] = [255, 255, 255];
colors["Серый"] = [110, 110, 110];
colors["Черный"] = [0, 0, 0];
colors["Красный"] = [255, 0, 0];
colors["Оранжевый"] = [255, 132, 57];
colors["Фиолетовый"] = [127, 57, 255];
colors["Синий"] = [57, 126, 255];

let auto = {
  model: null,
  color: "Белый",
  entity: null,
  x: null,
  y: null,
  z: null
};
const cameraRotator = require("interface/js/vie.js");
function createCam(x, y, z, rx, ry, rz, viewangle, up, sf = 0) {
    camera = mp.cameras.new("default");
    camera.setCoord(x, y, z);
    camera.setRot(rx, ry, rz, 2);
    camera.setFov(viewangle);
    camera.setActive(true);

    var vehPosition = new mp.Vector3(x, y, z); 
    cameraRotator.start(camera, vehPosition, vehPosition, new mp.Vector3(-3.0 - up, 3.5 + up, 0.5), 180, undefined, sf);
    cameraRotator.setZBound(-0.8, 1.8);
    cameraRotator.setZUpMultipler(5);
    cameraRotator.pause(true);

    mp.game.cam.renderScriptCams(true, false, 3000, true, false);
}

mp.events.add("auto", (act, value) => {
  switch (act) {
    case "model":
	  auto.model = autoModels[value];
	  mp.events.callRemote('createlveh', autoModels[value], colors[auto.color][0], colors[auto.color][1], colors[auto.color][2], auto.x, auto.y, auto.z);
      break;
    case "color":
      auto.color = autoColors[value];
      mp.events.callRemote('vehchangecolor', colors[auto.color][0], colors[auto.color][1], colors[auto.color][2]);
      break;
  }
});

mp.events.add('client::sendkilogramsinfoCar', (a) => {
	mp.gui.execute(`auto.numpas=${a}`);
});

mp.events.add("buyAuto", async (card) => {
  if (new Date().getTime() - global.lastCheck < 1500) return;
  global.lastCheck = new Date().getTime();
  cameraRotator.stop();

  global.menuClose();
  if (inair)
	  mp.gui.execute("air.active=0");
  else
	  mp.gui.execute("auto.active=0");
  
  disableTraffic();
  localplayer.setAlpha(255);

  if (inair)
    mp.events.callRemote("carroomBuy", auto.model, auto.color, false);
  else
    mp.events.callRemote("carroomBuy", auto.model, auto.color, card);
  inair = false;
});
mp.events.add("closeAuto", async () => {
  if (new Date().getTime() - global.lastCheck < 1500) return;
  mp.events.call("screenFadeOut", 500);
   global.lastCheck = new Date().getTime();
  await global.sleep(400)
  cameraRotator.stop();
  mp.gui.execute(`auto.cur='$'`);
  global.menuClose();
  if (inair)
	mp.gui.execute("air.active=0");
  else
	mp.gui.execute("auto.active=0");

  disableTraffic();
  localplayer.setAlpha(255);
  mp.events.callRemote("carroomCancel");
  inair = false;
});

mp.events.add('testAuto', async (testdrive) => {
    if(new Date().getTime() - global.lastCheck < 1500) return; 
    global.lastCheck = new Date().getTime();
    mp.events.call("screenFadeOut", 500);
	  await global.sleep(700)
	  global.menuClose();
    if (inair)
		  mp.gui.execute("air.active=0");
    else
		  mp.gui.execute("auto.active=0");
	  localplayer.setAlpha(255);
	  if (testdrive)
		  enableTraffic();
    mp.events.callRemote('carromtestdrive', auto.model, colors[auto.color][0], colors[auto.color][1], colors[auto.color][2]);
	  inair = false;
    if (auto.entity == null) return;
    auto.entity.destroy();
    auto.entity = null;
})

function enableTraffic() {
	mp.game.streaming.setPedPopulationBudget(1);
	mp.game.streaming.setVehiclePopulationBudget(1);
}
function disableTraffic() {
	mp.game.streaming.setPedPopulationBudget(0);
	mp.game.streaming.setVehiclePopulationBudget(0);
}

mp.events.add('guiReady', () => {
	disableTraffic();
});

let inair = 0;
global.modalAuto = false;
mp.events.add('client::changestateMODAL', (bool) => {
	global.modalAuto = bool
})
mp.events.add('openAuto', async (models, prices, x,y,z, header) => {
    if (global.menuCheck()) return;
    autoModels = JSON.parse(models);
    let autoNames = [];
    autoModels.forEach(model => {
        autoNames.push(vehicleNames.get(model) || "undefined")
    });
	
	let fov = 40;
	let add = 0.0;
	let up = 2.3;
	let upz = 0.0;
	let upx = 0.0;
	
	if (autoModels[0] === 'microlight') { inair = 1}
	
	if (autoModels[0] === 'boxville4'){ up = 6; }
	else if (inair) { upz = 2.5; up = 6; }
		
	createCam(x + upx, y - add, z + add + upz, 0, 0, 285.854, fov, up, 400); // координаты камеры и ротация
	cameraRotator.pause(false);

    // setAuto('models', models);
    setAuto('models', JSON.stringify(autoNames));
    setAuto('colors', JSON.stringify(autoColors));
    setAuto('prices', prices);
	
	if (autoModels[0] === 'amgone') mp.gui.execute(`auto.cur='MC'`);
	else mp.gui.execute(`auto.cur='$'`);

	auto.x = x;
	auto.y = y;
	auto.z = z;

	disableTraffic();
	
    mp.events.callRemote('createlveh', autoModels[0], 255, 255, 255, x, y, z);
    auto.color = "Белый";
    auto.model = autoModels[0];
	
	localplayer.setAlpha(0);
	await global.sleep(400)
	cameraRotator.pause(false);
	mp.events.call('screenFadeIn', 500);
	mp.gui.execute(`auto.header=${JSON.stringify(header)}; auto.open(); auto.getAllModelsInCat()`);
    global.menuOpen();
	if (inair)
		mp.gui.execute(`air.active=true`);
	else
		mp.gui.execute(`auto.active=true`);
});
function setAuto(type, jsonstr) {
	if (inair)
		mp.gui.execute(`air.${type}=${jsonstr}`);
	else
		mp.gui.execute(`auto.${type}=${jsonstr}`);
}
//========================

let wshop = {
  lid: -1,
  tab: 0,
  data: [],
};
mp.events.add("wshop", (act, value, sub) => {
  if (new Date().getTime() - global.lastCheck < 50) return;
  global.lastCheck = new Date().getTime();
  switch (act) {
    case "cat":
      if (value == 4) return;
      wshop.tab = value;
      global.menu.execute(
        `wshop.set(${value},'${JSON.stringify(wshop.data[value])}')`
      );
      break;
    case "buy":
      mp.events.callRemote("wshop", wshop.tab, value);
      break;
    case "rangebuy":
      mp.events.callRemote("wshopammo", value, sub);
      break;
  }
});
mp.events.add("closeWShop", () => {
  global.menuClose();
  wshop.tab = 0;
});
mp.events.add("openWShop", (id, json) => {
  if (global.menuCheck()) return;
  global.menuOpen();
  if (id !== wshop.lid) wshop.data = JSON.parse(json);
  global.menu.execute(`wshop.set(0,'${JSON.stringify(wshop.data[0])}')`);
  global.menu.execute("wshop.active=1");
  wshop.lid = id;
});
// WEAPON CRAFT //
/*mp.keys.bind(0x78, false, function () { // F9
    mp.events.call('openWCraft', 0, '[[0,1,0,1,0,1,0]]');
});*/
let wcraft = {
  tab: 0,
  frac: 0,
  data: [],
};
mp.events.add("wcraft", (act, value, sub) => {
  switch (act) {
    case "cat":
      wcraft.tab = value;
      global.menu.execute(
        `wcraft.set(${wcraft.frac},${value},'${JSON.stringify(
          wcraft.data[value]
        )}')`
      );
      break;
    case "buy":
      mp.events.callRemote("wcraft", wcraft.frac, wcraft.tab, value);
      break;
    case "rangebuy":
      mp.events.callRemote("wcraftammo", wcraft.frac, value, sub);
      break;
  }
});
mp.events.add("closeWCraft", () => {
  global.menuClose();
  wcraft.top = 0;
});
mp.events.add("openWCraft", (frac, json) => {
  //mp.gui.chat.push(`${frac}:${json}`);
  wcraft.data = JSON.parse(json);
  wcraft.data[4] = [];
  wcraft.frac = frac;
  global.menu.execute(
    `wcraft.set(${frac}, 0,'${JSON.stringify(wcraft.data[0])}')`
  );
  global.menu.execute("wcraft.active=1");
  global.menuOpen();
});

// DONATE //
var reds = 0;


// Color picker //
global.colorp = mp.browsers.new("http://package/interface/color.html");
mp.events.add("showColorp", () => {
  global.colorp.execute(`show(${true})`);
});
mp.events.add("hideColorp", () => {
  global.colorp.execute(`show(${false})`);
});
// Button events
mp.events.add("colors", (btn) => {
  switch (btn) {
    case "apply":
      //onapply
      break;
    case "cancel":
      //onbreak
      break;
  }
});
// Selected color event
mp.events.add("scolor", (c) => {
  // JSON String
  // c = {r: 255, g: 255, b: 255}
  c = JSON.parse(c);
  mp.events.call("tunColor", c);
});

// Report menu
var report = mp.browsers.new("http://package/interface/ticket.html");
var reportactive = false;
mp.events.add('addreport', (id_, author_, authorId_, quest_) => {
  report.execute(`addReport(${id_},'${author_}','${authorId_}','${quest_}', false, '')`);
  mp.events.call('notify', 0, 2, "Пришел новый репорт!", 1500);
})
mp.events.add('setreport', (id, name) => {
  report.execute(`setStatus(${id}, '${name}')`);
})
mp.events.add('delreport', (id) => {
  report.execute(`deleteReport(${id})`);
})
mp.events.add('client::spectateOnReport', (id) => {
mp.events.callRemote('sever::spectateOnReport', id);
});
mp.events.add('client::teleportOnReport', (id) => {
mp.events.callRemote('sever::teleportOnReport', id);
});
mp.events.add('takereport', (id, r) => {
  mp.events.callRemote('takereport', id, r);
})
mp.events.add('sendreport', (id, a) => {  
  mp.events.callRemote('sendreport', id, a);
})
mp.events.add('exitreport', () => {
global.menuClose();
reportactive = false;
  mp.gui.cursor.visible = false;
})

// Advert menu
var adverts = null;
var advertsloaded = false;
var advertsactive = false;

mp.events.add("enableadvert", (toggle) => {
  try {
    if (toggle) adverts = mp.browsers.new("http://package/interface/adverts.html");
    advertsloaded = toggle;
  } catch (e) {}
});

mp.events.add("addadvert", (id_, author_, quest_) => {
  try {
    if (adverts != null)
      adverts.execute(`addAdvert(${id_},'${author_}','${quest_}', false, '')`);
    mp.events.call("notify", 0, 2, "Пришло новое объявление!", 3000);
  } catch (e) {}
});
mp.events.add("setadvert", (id, name) => {
  try {
    if (adverts != null) adverts.execute(`setStatus(${id}, '${name}')`);
  } catch (e) {}
});
mp.events.add("deladvert", (id) => {
  try {
    if (adverts != null) adverts.execute(`deleteAdvert(${id})`);
  } catch (e) {}
});
mp.events.add("takeadvert", (id, r) => {
  if (new Date().getTime() - global.lastCheck < 50) return;
  global.lastCheck = new Date().getTime();
  mp.events.callRemote("takeadvert", id, r);
});
mp.events.add("sendadvert", (id, a) => {
  if (new Date().getTime() - global.lastCheck < 50) return;
  global.lastCheck = new Date().getTime();
  mp.events.callRemote("sendadvert", id, a);
});
mp.events.add("exitadvert", () => {
  global.menuClose();
  advertsactive = false;
  mp.gui.cursor.visible = false;
});
mp.keys.bind(0x75, false, function () { // F6 key report menu
    if (!loggedin || chatActive || editing || advertsactive || new Date().getTime() - global.lastCheck < 1000 || localplayer.getVariable('IS_ADMIN') != true) return;
    global.lastCheck = new Date().getTime();
    if (!global.menuOpened) {
        global.menuOpen();
        mp.gui.cursor.visible = true;
        if (!reportactive) report.execute(`app.playerName='${localplayer.name}'`);
        reportactive = true;
        report.execute('app.active=true;');
    } else {
        report.execute('app.active=false;');
        global.menuClose();
        reportactive = false;
        mp.gui.cursor.visible = false
    }
});
mp.keys.bind(Keys.VK_F2, false, function() {
  // F2 key advert menu
  if (
    !loggedin ||
    chatActive ||
    editing ||
    reportactive ||
    !advertsloaded ||
    new Date().getTime() - global.lastCheck < 1000
  )
    return;
  global.lastCheck = new Date().getTime();
  if (!global.menuOpened) {
    global.menuOpen();
    mp.gui.cursor.visible = true;
    if (!advertsactive) adverts.execute(`app.playerName='${localplayer.name}'`);
    advertsactive = true;
    if (adverts != null) adverts.execute("app.active=true;");
  } else {
    if (adverts != null) adverts.execute("app.active=false;");
    global.menuClose();
    advertsactive = false;
    mp.gui.cursor.visible = false;
  }
});

//CLOTHES=================
var clothesCamValues = [
    { Angle: 0, Dist: 0.7, Height: 0.6 },
    { Angle: 0, Dist: 1.4, Height: 0.2 },
    { Angle: 0, Dist: 1.4, Height: 0.2 },
    { Angle: 0, Dist: 1.4, Height: -0.4 },
    { Angle: 0, Dist: 1.2, Height: -0.7 },
    { Angle: 0, Dist: 1, Height: -0.2 },
    { Angle: 74, Dist: 1, Height: 0 },
    { Angle: 0, Dist: 0.7, Height: 0.6 },
    { Angle: 0, Dist: 1, Height: 0.3 },
];
let clothes = {
    type: 0,
    style: 0,
    color: 0,
    colors: [0, 0, 0],
    price: 1,
	typeBiz: 1,
}
mp.events.add('clothes', (act, value) => {
    const gender = (localplayer.getVariable("GENDER")) ? 1 : 0;

    switch (act) {
        case "style":

            switch (clothes.type) {
                case 0:
					var thisCl = 0;
					clothesHats[gender].forEach(item => {
						if (item.Variation == value)
							thisCl = item;
					});
                    var colors = thisCl.Colors;
                    setClothes("colors", JSON.stringify(colors));

                    clothes.style = thisCl.Variation;
                    clothes.color = colors[0];
                    clothes.colors = colors;

                    localplayer.setPropIndex(0, clothes.style, clothes.color, true);
                    return;
                case 1:
					var thisCl = 0;
					clothesTops[gender].forEach(item => {
						if (item.Variation == value)
							thisCl = item;
					});
                    var colors = thisCl.Colors;
                    setClothes("colors", JSON.stringify(colors));

                    clothes.style = thisCl.Variation;
                    clothes.color = colors[0];
                    clothes.colors = colors;

                    localplayer.setComponentVariation(11, clothes.style, clothes.color, 0);
                    localplayer.setComponentVariation(3, validTorsos[gender][clothes.style], 0, 0);
                    return;
                case 2:
					var thisCl = 0;
					clothesUnderwears[gender].forEach(item => {
						if (item.Top == value)
							thisCl = item;
					});
                    var colors = thisCl.Colors;
                    setClothes("colors", JSON.stringify(colors));

                    clothes.style = thisCl.Top;
                    clothes.color = colors[0];
                    clothes.colors = colors;

                    localplayer.setComponentVariation(11, clothes.style, clothes.color, 0);
                    localplayer.setComponentVariation(3, validTorsos[gender][clothes.style], 0, 0);
                    return;
                case 3:
					var thisCl = 0;
					clothesLegs[gender].forEach(item => {
						if (item.Variation == value)
							thisCl = item;
					});
                    var colors = thisCl.Colors;
                    setClothes("colors", JSON.stringify(colors));

                    clothes.style = thisCl.Variation;
                    clothes.color = colors[0];
                    clothes.colors = colors;

                    localplayer.setComponentVariation(4, clothes.style, clothes.color, 0);
                    return;
                case 4:
					var thisCl = 0;
					clothesFeets[gender].forEach(item => {
						if (item.Variation == value)
							thisCl = item;
					});
                    var colors = thisCl.Colors;
                    setClothes("colors", JSON.stringify(colors));

                    clothes.style = thisCl.Variation;
                    clothes.color = colors[0];
                    clothes.colors = colors;

                    localplayer.setComponentVariation(6, clothes.style, clothes.color, 0);
                    return;
                case 5:
					var thisCl = 0;
					clothesGloves[gender].forEach(item => {
						if (item.Variation == value)
							thisCl = item;
					});
                    var colors = thisCl.Colors;
                    setClothes("colors", JSON.stringify(colors));

                    clothes.style = thisCl.Variation;
                    clothes.color = colors[0];
                    clothes.colors = colors;

                    localplayer.setComponentVariation(3, correctGloves[gender][clothes.style][15], clothes.color, 0);
                    return;
                case 6:
					var thisCl = 0;
					clothesWatches[gender].forEach(item => {
						if (item.Variation == value)
							thisCl = item;
					});
                    var colors = thisCl.Colors;
                    setClothes("colors", JSON.stringify(colors));

                    clothes.style = thisCl.Variation;
                    clothes.color = colors[0];
                    clothes.colors = colors;

                    localplayer.setPropIndex(6, clothes.style, clothes.color, true);
                    return;
                case 7:
					var thisCl = 0;
					clothesGlasses[gender].forEach(item => {
						if (item.Variation == value)
							thisCl = item;
					});
                    var colors = thisCl.Colors;
                    setClothes("colors", JSON.stringify(colors));

                    clothes.style = thisCl.Variation;
                    clothes.color = colors[0];
                    clothes.colors = colors;

                    localplayer.setPropIndex(1, clothes.style, clothes.color, true);
                    return;
                case 8:
					var thisCl = 0;
					clothesJewerly[gender].forEach(item => {
						if (item.Variation == value)
							thisCl = item;
					});
                    var colors = thisCl.Colors;
                    setClothes("colors", JSON.stringify(colors));

                    clothes.style = thisCl.Variation;
                    clothes.color = colors[0];
                    clothes.colors = colors;

                    localplayer.setComponentVariation(7, clothes.style, clothes.color, 0);
                    return;
            }
            break;
        case "color":
            switch (clothes.type) {
                case 0:
                    clothes.color = clothes.colors[value];
                    localplayer.setPropIndex(0, clothes.style, clothes.color, true);
                    return;
                case 1:
                    clothes.color = clothes.colors[value];
                    localplayer.setComponentVariation(11, clothes.style, clothes.color, 0);
                    return;
                case 2:
                    clothes.color = clothes.colors[value];
                    localplayer.setComponentVariation(11, clothes.style, clothes.color, 0);
                    return;
                case 3:
                    clothes.color = clothes.colors[value];
                    localplayer.setComponentVariation(4, clothes.style, clothes.color, 0);
                    return;
                case 4:
                    clothes.color = clothes.colors[value];
                    localplayer.setComponentVariation(6, clothes.style, clothes.color, 0);
                    return;
                case 5:
                    clothes.color = clothes.colors[value];
                    localplayer.setComponentVariation(3, correctGloves[gender][clothes.style][15], clothes.color, 0);
                    return;
                case 6:
                    clothes.color = clothes.colors[value];
                    localplayer.setPropIndex(6, clothes.style, clothes.color, true);
                    return;
                case 7:
                    clothes.color = clothes.colors[value];
                    localplayer.setPropIndex(1, clothes.style, clothes.color, true);
                    return;
                case 8:
                    clothes.color = clothes.colors[value];
                    localplayer.setComponentVariation(7, clothes.style, clothes.color, 0);
                    return;
            }
            break;
        case "cat":
            var clothesArr = {};
            if (value == 0) clothesArr = clothesHats[gender];
            else if (value == 1) clothesArr = clothesTops[gender];
            else if (value == 2) clothesArr = clothesUnderwears[gender];
            else if (value == 3) clothesArr = clothesLegs[gender];
            else if (value == 4) clothesArr = clothesFeets[gender];
            else if (value == 5) clothesArr = clothesGloves[gender];
            else if (value == 6) clothesArr = clothesWatches[gender];
            else if (value == 7) clothesArr = clothesGlasses[gender];
            else if (value == 8) clothesArr = clothesJewerly[gender];

            var styles = [];
            var prices = [];
			var names = [];
            var colors = clothesArr[0].Colors;
			
            clothesArr.forEach(item => {
                if (item.typeBiz != null && clothes.typeBiz == item.typeBiz) {
					let tempPrice = item.Price;
					prices.push(tempPrice.toFixed());

					if (value == 2) 
						styles.push(item.Top)
					else {
						styles.push(item.Variation)
					}
					names.push(item.Names)
				}
            });

            setClothes("styles", JSON.stringify(styles));
            setClothes("colors", JSON.stringify(colors));
            setClothes("names", JSON.stringify(names));
            setClothes("prices", JSON.stringify(prices));

            clothes.type = value;
            clothes.style = styles[0];
            clothes.color = colors[0];
            clothes.colors = colors;

            const camValues = clothesCamValues[value];
            const camPos = global.getCameraOffset(new mp.Vector3(bodyCamStart.x, bodyCamStart.y, bodyCamStart.z + camValues.Height), localplayer.getRotation(2).z + 90 + camValues.Angle, camValues.Dist);
			
			clearClothes();
			
            bodyCam.setCoord(camPos.x, camPos.y, camPos.z);
            bodyCam.pointAtCoord(bodyCamStart.x, bodyCamStart.y, bodyCamStart.z + camValues.Height);
            break;
    }
});
let lastpos = null; 
mp.events.add('buyClothes', () => {
    mp.events.callRemote('buyClothes', clothes.type, clothes.style, clothes.color);
})
mp.events.add('closeClothes', () => {
	if(new Date().getTime() - global.lastCheck < 50) return; 
	global.lastCheck = new Date().getTime();
    global.menuClose();
    mp.gui.execute('clothes.active=0');

	localplayer.position = lastpos;
    bodyCam.destroy();
    mp.game.cam.renderScriptCams(false, false, 500, true, false);
    global.playerheading.stop()

    mp.events.callRemote('cancelClothes');
})
mp.events.add('openClothes', (price, type) => {
    if (global.menuCheck()) return;
	lastpos = localplayer.position;
    localplayer.position = new mp.Vector3(-795.1452, 331.7789, 201.42437);
	mp.players.local.setHeading(-90);
    bodyCamStart = localplayer.position;
    var camValues = { Angle: localplayer.getRotation(2).z + 90, Dist: 1.3, Height: 0.3 };
    var pos = global.getCameraOffset(new mp.Vector3(bodyCamStart.x, bodyCamStart.y, bodyCamStart.z + camValues.Height), camValues.Angle, camValues.Dist);
    bodyCam = mp.cameras.new('default', pos, new mp.Vector3(0, 0, 0), 50);
    bodyCam.pointAtCoord(bodyCamStart.x, bodyCamStart.y, bodyCamStart.z + camValues.Height);
    bodyCam.setActive(true);
    mp.game.cam.renderScriptCams(true, false, 500, true, false);
    global.playerheading.startveh(mp.players.local);

    clearClothes();
	
	clothes.typeBiz = type;

    global.menuOpen();
    mp.gui.execute(`clothes.active=true; clothes.typeBiz = ${type}; clothes.gender = ${JSON.stringify(localplayer.getVariable("GENDER"))}`);
})

function clearClothes() {
    const gender = (localplayer.getVariable("GENDER")) ? 1 : 0;

    localplayer.clearProp(0);
    localplayer.clearProp(1);
    localplayer.clearProp(2);
    localplayer.clearProp(6);
    localplayer.clearProp(7);

    localplayer.setComponentVariation(1, clothesEmpty[gender][1], 0, 0);
    localplayer.setComponentVariation(3, clothesEmpty[gender][3], 0, 0);
    localplayer.setComponentVariation(4, clothesEmpty[gender][4], 0, 0);
    localplayer.setComponentVariation(5, clothesEmpty[gender][5], 0, 0);
    localplayer.setComponentVariation(6, clothesEmpty[gender][6], 0, 0);
    localplayer.setComponentVariation(7, clothesEmpty[gender][7], 0, 0);
    localplayer.setComponentVariation(8, clothesEmpty[gender][8], 0, 0);
    localplayer.setComponentVariation(9, clothesEmpty[gender][9], 0, 0);
    localplayer.setComponentVariation(10, clothesEmpty[gender][10], 0, 0);
    localplayer.setComponentVariation(11, clothesEmpty[gender][11], 0, 0);
}

function setClothes(type, jsonstr) {
    mp.gui.execute(`clothes.${type}=${jsonstr}`);
    if (type == 'colors') mp.gui.execute(`clothes.indexC=0`);
    else if (type == 'styles') mp.gui.execute(`clothes.indexS='-1'`);
}

//========================


//MASKS===================
mp.events.add("closeMasks", () => {
  if (new Date().getTime() - global.lastCheck < 50) return;
  global.lastCheck = new Date().getTime();
  global.menuClose();
  mp.gui.execute("masks.active=0");

  localplayer.position = lastpos;
  lastpos = null;
  bodyCam.destroy();
  mp.game.cam.renderScriptCams(false, false, 500, true, false);
  global.playerheading.stop()

  mp.events.callRemote("cancelMasks");
});
mp.events.add("masks", (act, value) => {
  switch (act) {
    case "style":
      var colors = clothesMasks[value].Colors;
      setMaskCEF("colors", JSON.stringify(colors));

      clothes.style = clothesMasks[value].Variation;
      clothes.color = colors[0];
      clothes.colors = colors;

      localplayer.setComponentVariation(1, clothes.style, clothes.color, 0);
      return;
    case "color":
      clothes.color = clothes.colors[value];
      localplayer.setComponentVariation(1, clothes.style, clothes.color, 0);
      return;
  }
});
mp.events.add("buyMasks", () => {
  if (new Date().getTime() - global.lastCheck < 50) return;
  global.lastCheck = new Date().getTime();
  mp.events.callRemote("buyMasks", clothes.style, clothes.color);
});
mp.events.add("openMasks", (price) => {
  if (global.menuCheck()) return;
  lastpos = localplayer.position;
  localplayer.position = new mp.Vector3(-795.1452, 331.7789, 201.42437);
  mp.players.local.setHeading(-90);
  bodyCamStart = localplayer.position;
  var camValues = {
    Angle: localplayer.getRotation(2).z + 90,
    Dist: 0.7,
    Height: 0.6,
  };
  
  bodyCamStart = localplayer.position;
  var pos = global.getCameraOffset(
    new mp.Vector3(
      bodyCamStart.x,
      bodyCamStart.y,
      bodyCamStart.z + camValues.Height
    ),
    camValues.Angle,
    camValues.Dist
  );
  bodyCam = mp.cameras.new('default', pos, new mp.Vector3(0, 0, 0), 50);
  bodyCam.pointAtCoord(bodyCamStart.x, bodyCamStart.y, bodyCamStart.z + camValues.Height);
  bodyCam.setActive(true);
  mp.game.cam.renderScriptCams(true, false, 500, true, false);
  global.playerheading.startveh(mp.players.local);

  var styles = [];
  var prices = [];
  var colors = clothesMasks[0].Colors;

  clothesMasks.forEach((mask) => {
    let tempPrice = (mask.Price / 100) * price;
    prices.push(tempPrice.toFixed());

    styles.push(mask.Variation);
  });

  setMaskCEF("styles", JSON.stringify(styles));
  setMaskCEF("colors", JSON.stringify(colors));
  setMaskCEF("prices", JSON.stringify(prices));

  clothes = {
    type: 0,
    style: styles[0],
    color: colors[0],
    colors: colors,
    price: price,
	typeBiz: 0,
  };

  localplayer.setComponentVariation(1, styles[0], colors[0], 0);

  global.menuOpen();
  mp.gui.execute(`masks.active=true`);

  localplayer.clearProp(0);
  localplayer.clearProp(1);
});

function setMaskCEF(type, jsonstr) {
  mp.gui.execute(`masks.${type}=${jsonstr}`);
  if (type == "colors") mp.gui.execute(`masks.indexC=0`);
  else if (type == "styles") mp.gui.execute(`masks.indexS=0`);
}
//========================

//BAG=====================
mp.events.add('client::bag:close', () => {
	if(new Date().getTime() - global.lastCheck < 50) return; 
	global.lastCheck = new Date().getTime();
    global.menuClose();
    mp.gui.execute('backs.active=0');

	localplayer.position = lastpos;
	lastpos = null;
    bodyCam.destroy();
    mp.game.cam.renderScriptCams(false, false, 500, true, false);
    global.playerheading.stop()
    mp.events.callRemote('server::bag:exit');
})
mp.events.add('client::bag:change', (act, value) => {
    switch (act) {
        case "style":
            var colors = backs[value].Colors;
            BagShopSet("colors", JSON.stringify(colors));

            clothes.style = backs[value].Variation;
            clothes.color = colors[0];
            clothes.colors = colors;

            localplayer.setComponentVariation(5, clothes.style, clothes.color, 0);
            return;
        case "color":
            clothes.color = clothes.colors[value];
            localplayer.setComponentVariation(5, clothes.style, clothes.color, 0);
            return;
    }
})
mp.events.add('client::bag:buy', () => {
	if(new Date().getTime() - global.lastCheck < 50) return; 
	global.lastCheck = new Date().getTime();
    mp.events.callRemote('server::bag:buy', clothes.style, clothes.color);
	
})

mp.events.add('client::bag:open', (price) => {
    if (global.menuCheck()) return;
	lastpos = localplayer.position;
    localplayer.position = new mp.Vector3(-795.1452, 331.7789, 201.42437);
	mp.players.local.setHeading(-90);
	
    bodyCamStart = mp.players.local.position;
    var camValues = { Angle: localplayer.getRotation(2).z + 90, Dist: 1.5, Height: 0.2 };
    var pos = global.getCameraOffset(new mp.Vector3(bodyCamStart.x, bodyCamStart.y, bodyCamStart.z + camValues.Height), camValues.Angle, camValues.Dist);
    bodyCam = mp.cameras.new('default', pos, new mp.Vector3(0, 0, 0), 45);
    bodyCam.pointAtCoord(bodyCamStart.x, bodyCamStart.y, bodyCamStart.z + camValues.Height);
    bodyCam.pointAtPedBone(localplayer.handle, 0, 0, 0, 0, true);
    bodyCam.setActive(true);
    mp.game.cam.renderScriptCams(true, false, 500, true, false);
	
    global.playerheading.startveh(mp.players.local);

    var styles = [];
    var prices = [];
    var colors = backs[0].Colors;
	
    backs.forEach(back => {
        let tempPrice = (back.Price) * price;
        prices.push(tempPrice.toFixed());

        styles.push(back.Variation)
    });

    BagShopSet("styles", JSON.stringify(styles));
    BagShopSet("colors", JSON.stringify(colors));
    BagShopSet("prices", JSON.stringify(prices));

    clothes = {
        type: 0,
        style: styles[0],
        color: colors[0],
        colors: colors,
        price: price,
    }

    localplayer.setComponentVariation(5, styles[0], colors[0], 1);

    global.menuOpen();
    mp.gui.execute(`backs.active=true;`);

    localplayer.clearProp(0);
    localplayer.clearProp(1);
})

function BagShopSet(type, jsonstr) {
    mp.gui.execute(`backs.${type}=${jsonstr}`);
    if (type == 'colors') mp.gui.execute(`backs.indexC=0`);
    else if (type == 'styles') mp.gui.execute(`backs.indexS=-1`);
}
//========================

//QUEST AND DIALOGS MENU==
global.dialogMenu = mp.browsers.new('package://interface/modules/DialogMenu/index.html');
global.dialogOpen = false;
mp.events.add('client::opendialogmenu', (state, header, desc, text, answer) => {
	if (global.dialogOpen) {
		global.dialogOpen = false;
		global.dialogMenu.execute(`DialogMenu.closemenu(false)`);
	}
	global.dialogMenu.execute(`DialogMenu.set(${state}, ${JSON.stringify(header)}, ${JSON.stringify(desc)}, ${JSON.stringify(text)}, ${JSON.stringify(answer)})`)
	global.menuOpen();
	global.dialogOpen = true;
})

mp.events.add('client::closedialog', () => {
    mp.events.call('NPC.cameraOff', 500);
    global.dialogMenu.execute(`DialogMenu.closemenu(false)`);
	global.dialogOpen = false;
    global.menuClose();
});

mp.events.add('client::closedialog2', () => {
    global.dialogMenu.execute(`DialogMenu.closemenu(false)`);
	global.dialogOpen = false;
    global.menuClose();
});
mp.events.add('client::dialoganswer', (id) =>  {
    mp.events.callRemote('server::dialoganswer', id)
});



global.donequestMenu = mp.browsers.new('package://interface/modules/DoneQuest/index.html');
global.donequestopen = false;
mp.events.add('client::openquestdonemenu', (state, namequest, descquest, money, exp, itemsbonus, itemsbonustwo) => {
	if (global.donequestopen) {
		global.donequestopen = false;
	}
	global.donequestMenu.execute(`menu.set(${state}, ${JSON.stringify(namequest)}, ${JSON.stringify(descquest)}, ${JSON.stringify(money)}, ${JSON.stringify(exp)}, ${JSON.stringify(itemsbonus)}, ${JSON.stringify(itemsbonustwo)})`)
	global.menuOpen();
	mp.game.audio.playSoundFrontend(-1, 'BASE_JUMP_PASSED', 'HUD_AWARDS', true);
	global.donequestopen = true;
})

mp.events.add('client::closedonemenuquest', () => {
	if (global.donequestopen) {
		global.donequestopen = false;
		global.menuClose();
	}
});

global.checkquestMenu = mp.browsers.new('package://interface/modules/CheckQuest/index.html');
global.checkquestMenuopen = false;
mp.events.add('client::openmenucheckquest', (state, namequest, descquest, money, exp, itemsbonus, itemsbonustwo, num) => {
	if (global.checkquestMenuopen) {
		global.checkquestMenuopen = false;
	}
	global.checkquestMenu.execute(`menu.set(${state}, ${JSON.stringify(namequest)}, ${JSON.stringify(descquest)}, ${JSON.stringify(money)}, ${JSON.stringify(exp)}, ${JSON.stringify(itemsbonus)}, ${JSON.stringify(itemsbonustwo)}, ${JSON.stringify(num)})`)
	global.menuOpen();
	// mp.game.audio.playSoundFrontend(-1, 'BASE_JUMP_PASSED', 'HUD_AWARDS', true);
	global.checkquestMenuopen = true;
})


mp.events.add('client::closemenucheckquest', () => {
	if (global.checkquestMenuopen) {
		global.checkquestMenuopen = false;
		global.menuClose();
	}
});
mp.events.add('client::takequestcheckmenu', (a) => {
	if (global.checkquestMenuopen) {
		global.checkquestMenuopen = false;
		mp.events.callRemote('server::takequestcheckmenu', a)
		global.menuClose();
	}
});
//========================

//POPIT===================
global.popit = null;
global.checkpopit = false;
mp.events.add('client::openpopitmenu', () => {
  global.popit = mp.browsers.new('package://interface/modules/popit/index.html');
  global.checkpopit = true;
  global.menuOpen(false, true);
});
mp.events.add('client::closepopitmenu', () => {
  if (global.popit != null) {
    global.popit.destroy();
    global.popit = null;
    mp.events.callRemote('server::inventory::popit:close');
    global.menuClose(false, true);
  }
});
//========================

//FINES===================
var fines = {
  lastFine: null,
  lastCost: 0,
  lastSpeed: 0,
};
mp.events.add('client::speedcheker:load', () => {
  mp.gui.execute(`HUD.addFine('${parseInt(fines.lastSpeed)}', '${parseInt(fines.lastCost)}', ${JSON.stringify(fines.lastFine)})`);
});
mp.events.add("fine.show", (speed, cost, lastid, numberPlate) => {
  var date = new Date();
  var name = "multiproject-" + date.getDate() + "_" + date.getMonth() + "_" + date.getFullYear() + "-" + date.getHours() + "_" + date.getMinutes() + "_" + date.getSeconds() + ".jpg";
  mp.gui.takeScreenshot(name, 0, 25, 100);
  var screenurl = "http://screenshots/" + name;
  fines.lastFine = screenurl
  fines.lastCost = cost;
  fines.lastSpeed = speed;
  setTimeout(() => {
    mp.gui.execute(`HUD.addFine('${parseInt(speed)}', '${parseInt(cost)}', ${JSON.stringify(screenurl)}, ${lastid}, ${numberPlate})`);
  }, 200);
});

mp.events.add('client::fine:add', (speed, cost, lastid, numberPlate, photo) => {
	mp.events.callRemote("server::fine:add", speed, cost, lastid, numberPlate, photo);
});
//========================

//DRUG====================
mp.events.add("client::drug:open", () => {
  if (global.menuCheck() || localplayer.getVariable('InDeath') == true || chatActive || editing || cuffed) return;	
  global.menu.execute(`DrugMenu.active = true`);
  global.menuOpen(false);
});

mp.events.add('client::drug:buy', (id) => {
  mp.events.callRemote("server::drug:buy", id)
});

mp.events.add('client::drug:close', () => {
  global.menu.execute(`DrugMenu.active = false`);
  global.menuClose(false);
});
//========================

mp.keys.bind(0x46, false, function() {
	if (!loggedin || chatActive || editing || cuffed || localplayer.getVariable('InDeath') == true || !intrunk) return;
	localplayer.detach(true, true);
	mp.events.callRemote("fpress");
});

mp.events.add('opentableorg', function (members, cars, data, access) {
	if (global.menuCheck()) return;	
	global.menu.execute(`tabletorg.show(${members},${cars},${data},${access})`);
  global.menuOpen(false);   
});

mp.events.add('tabletmembers', function (nick) {
	mp.events.callRemote("memberscall", nick);
});

mp.events.add('tabletcars', function (number, id) {
	mp.events.callRemote("carscall", number, id);
});

mp.events.add('tabletinputfff', function (texts) {
	mp.events.callRemote("maladoyinput", texts);
});

mp.events.add('sellorg', function () {
	mp.events.callRemote("callsell");
});

mp.events.add('closetabletorg', function () {
  global.menuClose(false);
  global.menu.execute(`tabletorg.active = false`);
});

//POSTAL==================
mp.events.add('client::postal:card', (a, b, c) => {
	mp.events.callRemote('server::postal:card', a, b, c);
	mp.events.call('client::postal:cardclose');
});

mp.events.add('client::postal:opencard', (a, b, c, d, e) => {
	if (global.menuCheck() || localplayer.getVariable('InDeath') == true || chatActive || editing || cuffed) return;	
	mp.gui.execute(`PostCard.active = true; PostCard.open(${JSON.stringify(a)}, ${JSON.stringify(b)}, ${JSON.stringify(c)}, ${d}, ${e});`);
	global.menuOpen();
});

mp.events.add('client::postal:cardclose', () => {
	mp.gui.execute(`PostCard.active = false; PostCard.sender = false;`);
	global.menuClose();
});

mp.events.add('client::postalmenu:take', (a) => {
  mp.events.callRemote('server::gopostal:takeparcel', a);
});

mp.events.add('client::postalmenu:open', (json) => {
  if (global.menuCheck() || localplayer.getVariable('InDeath') == true || chatActive || editing || cuffed) return;	
  global.menuOpen();
  mp.gui.execute(`PostalMenu.active = true; PostalMenu.parcels=${json}`)
  mp.events.call('NPC.cameraOn', "ParcelNPC", 500)
});

mp.events.add('client::postalmenu:updateitems', (json) => {
  mp.gui.execute(`PostalMenu.parcels=${json}`)
});

mp.events.add('client::postalmenu:close', () => {
  mp.gui.execute(`PostalMenu.reset()`)
  global.menuClose();
  mp.events.call('NPC.cameraOff', 500)
});
//========================

mp.events.add('client::house:open', (houseid, houseowner, price, garage, roomates, type, lock, player) => {
	global.menuOpen(false);
	mp.gui.execute(`HMMenu.active = true; HMMenu.Open(${houseid}, ${houseowner}, ${price}, ${garage}, ${roomates}, ${parseInt(type)}, ${lock}, ${player})`);
});

mp.events.add('client::house::postal:open', (list) => {
	global.menuOpen(false);
	mp.gui.execute(`PostalHouse.active = true; PostalHouse.Open(${list})`);
});
mp.events.add('client::house::postal:exit', () => {
	global.menuClose(false);
	mp.gui.execute(`PostalHouse.active = false;`);
});
mp.events.add('client::house::postal:update', (list) => {
	mp.gui.execute(`PostalHouse.Open(${list}); PostalHouse.selected = -1`);
});

mp.events.add('client::house::postal:take', (a) => {
	mp.events.callRemote('server::house::postal:take', a);
});

mp.events.add('client::house:interaction', (a) => {
	mp.events.callRemote('server::house:interaction', a);
});

mp.events.add('client::house::postal:notify', () => {
	mp.gui.execute(`HUD.openPostalAdd()`);
});

mp.events.add('client::house:exit', () => {
	global.menuClose(false);
	mp.gui.execute(`HMMenu.active = false;`);
});

//PHONE===================
global.phone = mp.gui;
mp.events.add('client::phone:open', (house, apart, veh, forbes, bank, money, sim, work, deliveryorders, deliveryitemssell) => {
	global.menuOpen(false);
	global.phoneOpen = true;
	global.phone.execute(`phone.active = true; phone.Open(${house}, ${apart}, ${veh}, ${forbes}, ${bank}, ${money}, ${sim}, ${settings.wallpapper}, ${settings.airmode}, ${selectedDictorYandexIndex}, ${activeDictorNavigator}, ${work}, ${deliveryorders}, ${deliveryitemssell}, ${JSON.stringify(settings.appstore)}, ${JSON.stringify(localplayer.name.replace("_", " "))})`);
});

mp.events.add('client::phone::deliveryclub:ordersset', (a) => {
	global.phone.execute(`phone.deliveryclub.orders = ${a}; phone.deliveryclub.style = 4;`);
});

mp.events.add('client::phone::delivery:resetbasket', () => {
	global.phone.execute(`phone.deliveryclub.basket = [];`);
});

mp.events.add('client::phone::deliveryclub:ordersget', () => {
	mp.events.callRemote("server::phone::deliveryclub:ordersset");
});

mp.events.add('client::phone::deliveryclub:takeorder', (a) => {
	mp.events.callRemote("server::phone::deliveryclub:takeorder", a);
});

mp.events.add('client::phone::delivery:buyall', (a) => {
	mp.events.callRemote("server::phone::delivery:buyall", a);
});

mp.events.add('client::phone:appstore', (id, state) => {
	settings.appstore[id] = state;
	mp.storage.data.appstores = settings.appstore;
	mp.storage.flush();
});

mp.events.add('client::phone:transfermoney', (a, b) => {
	mp.events.callRemote('server::phone:transfermoney', parseInt(a), parseInt(b));
});

mp.events.add('client::phone:settingsset', (a) => {
	settings.wallpapper = a;
	mp.storage.data.wallpapper = a
	mp.storage.flush()
});

mp.events.add('client::phone:setaviamode', (a) => {
	settings.airmode = a;
	mp.storage.data.airmode = a
	mp.storage.flush()
});

mp.events.add('client::phone:navigatorvoiceactive', (a) => {
	activeDictorNavigator = a;
	mp.storage.data.navigatorState = a
	mp.storage.flush()
});

var settings = {
  wallpapper: 1,
  appstore: [false,false,false,false,false,false,false,true],
  airmode: false,
};

mp.events.add('client::phone:notify', (a,b,c) => {
	if (global.phoneOpen)
	phone.execute(`phone.pushNoty(${a},${JSON.stringify(b)},${c})`);
});

mp.events.add('client::phone:updateVeh', (veh) => {
	global.phone.execute(`phone.auto=${veh}; phone.modulemenu = 0; phone.modal = 0; phone.selectautos = -1; phone.pushNoty(4, "Вы продали автомобиль государству!", 2000)`);
});

mp.events.add('client::phone:updateHouseApart', (apart, house) => {
	global.phone.execute(`phone.apartaments=${apart}; phone.house=${house}; phone.modulemenu = 0; phone.modal = 0; phone.pushNoty(3, "Вы продали имущество государству", 2000)`);
});

mp.events.add('client::phone:switchhouse', (a) => {
	mp.events.callRemote('server::phone:switchhouse', a);
});
mp.events.add('client::phone:switchapart', (a) => {
	mp.events.callRemote('server::phone:switchapart', a);
});
mp.events.add('client::phone:switchvehicle', (a, b) => {
	mp.events.callRemote('server::phone:switchvehicle', a, b);
});

mp.events.add('client::phone:close', () => {
	global.menuClose(false);
	global.phoneOpen = false;
	global.phone.execute(`phone.active = false; phone.reset()`);
});
mp.events.add('client::phone:gps', (id) => {
	mp.events.callRemote('server::phone:gps', id)
});

mp.events.add('client::phone:callopen', (a,b) => {
	global.phone.execute(`phone.openCallMenu(${a},${b})`)
});

mp.events.add('client::phone:calloff', (a) => {
	global.phone.execute(`phone.closeCallMenu(${a})`)
});

mp.events.add('client::phone:callstart', (a) => {
	mp.events.callRemote('server::phone:callstart', a);
});

mp.events.add('client::phone:callaccept', () => {
	mp.events.callRemote('server::phone:callaccept');
});
mp.events.add('client::phone:callend', () => {
	mp.events.callRemote('server::phone:callend');
});

mp.events.add('client::phone:weather:set', (a) => {
	var res = JSON.parse(JSON.stringify(a));
	global.phone.execute(`phone.degrees=${res}`);
});

global.cameraPhoneOpen = false;
global.lastCamViewMode = 0;
mp.events.add('client::phone:opencamera', async () => {
	if (global.phoneOpen) {
		mp.events.callRemote('server::phone:close');
	}
	global.cameraPhoneOpen = true;
	mp.game.cam.doScreenFadeOut(400);
	await mp.game.waitAsync(410);
	global.menuOpen(true, false, false);
	lastCamViewMode = mp.game.invoke("0x8D4D46230B2C353A");
	mp.game.invoke("0x5A4F9EDF1673F704", 4);
	mp.game.cam.doScreenFadeIn(400);
});

mp.events.add('render', () => {
	if (global.cameraPhoneOpen) {
		mp.game.controls.disableControlAction(2, 24, true);
		mp.game.controls.disableControlAction(2, 25, true);
		mp.game.controls.disableControlAction(2, 0, true);
	}
});

mp.keys.bind(Keys.VK_RETURN, false, async function () {
	if (global.cameraPhoneOpen) {
		var date = new Date();
		var name = "gallery_" + date.getDate() + "_" + date.getMonth() + "_" + date.getFullYear() + "_" + date.getHours() + "_" + date.getMinutes() + "_" + date.getSeconds() + ".jpg";
		mp.gui.takeScreenshot(name, 0, 100, 0);
		var screenurl = "http://screenshots/" + name;
		await global.sleep(500);
		global.phone.execute(`phone.addPhoto(${JSON.stringify(screenurl)})`);
		mp.game.cam.doScreenFadeOut(100);
		mp.game.audio.playSoundFrontend(
		  -1,
		  "Camera_Shoot",
		  "Phone_SoundSet_Michael",
		  !0
		);
		await global.sleep(500);
		mp.game.cam.doScreenFadeIn(100);
	}
});

mp.keys.bind(Keys.VK_ESCAPE, false, async function() {
	if (global.cameraPhoneOpen) {
		global.menuClose(true, false, false);
		if (!global.phoneOpen) {
			mp.events.callRemote('server::phone:open');
		}
		global.cameraPhoneOpen = false;
		mp.game.cam.doScreenFadeOut(400);
		await mp.game.waitAsync(600);
		mp.game.invoke("0x5A4F9EDF1673F704", lastCamViewMode);
		mp.game.cam.doScreenFadeIn(400);
	}
});
//========================

//SOUND===================
global.soundCEF = mp.browsers["new"]('http://package/interface/modules/sounds/index.html');
mp.events.add('client::soundplay', (url, vol) => {
	global.soundCEF.execute(`playSound(${JSON.stringify(url)}, ${vol})`);
});
mp.events.add('client::soundplayPetrol', (url, vol) => {
	global.soundCEF.execute(`playPetrolSound(${JSON.stringify(url)}, ${vol})`);
});

mp.events.add('client::sound:playMusic', (url, vol) => {
	global.soundCEF.execute(`playMusic(${JSON.stringify(url)}, ${vol})`);
});
//========================

//NAVIGATOR===============
var activeDictorNavigator = false;
var selectedDictorYandexIndex = 0;
var selectedDictorYandex = "alice";

var listaudios = [
	"alice",
	"basta",
	"buzova",
	"darthvader",
	"dzuba",
	"kharlamov",
	"mercedes",
	"optimus",
]

var navigatorstate = false;
var distance;
var distToNxJunction;
var posx;
var posy;
var posz;
var shapeEndNavigator;
var NavigatorBlip;

setTimeout(() => {
	if (mp.storage.data.navigatorvoice == undefined) {
		activeDictorNavigator = true;
		selectedDictorYandexIndex = JSON.parse(0);
		mp.storage.data.navigatorvoice = selectedDictorYandexIndex;
	}
	else {
		selectedDictorYandexIndex = mp.storage.data.navigatorvoice;
	}
	
	if (mp.storage.data.navigatorState == undefined) {
		mp.storage.data.navigatorState = activeDictorNavigator;
	}
	else {
		activeDictorNavigator = mp.storage.data.navigatorState;
	}
	
	if (mp.storage.data.appstores == undefined) {
		mp.storage.data.appstores = settings.appstore;
	}
	else {
		settings.appstore = mp.storage.data.appstores;
	}
	
	if (mp.storage.data.wallpapper == undefined) {
		mp.storage.data.wallpapper = settings.wallpapper;
		mp.storage.data.airmode = settings.airmode;
	}
	else {
		settings.wallpapper = mp.storage.data.wallpapper;
		settings.airmode = mp.storage.data.airmode;
	}
}, 3000);

mp.events.add('client::phone:setvoice', (id) => {
	selectedDictorYandex = listaudios[id]
	selectedDictorYandexIndex = JSON.parse(id);
    mp.storage.data.navigatorvoice = selectedDictorYandexIndex;
    mp.storage.flush(); 
});

mp.events.add('client::navigator:setPoint', (x, y, z) => {
	if (global.phoneOpen) {
		mp.events.callRemote('server::phone:close');
		global.phoneOpen = false;
	}
	if (localplayer.isInAnyVehicle(false) && activeDictorNavigator) 
		global.soundCEF.execute(`playSoundNavigator('./yandex_russian_` + selectedDictorYandex + `/start.ogg');`);
	posx = x;
	posy = y;
	posz = z;
	if (NavigatorBlip != null) {
		navigatorstate = false;
		NavigatorBlip.destroy();
		NavigatorBlip = null;
	}
	NavigatorBlip = mp.blips.new(162, new mp.Vector3(x,y,z),
	{
		name: "Пункт назначения",
		scale: 1,
		color: 5,
		dimension: 0,
		shortrange: true,
	});
	if (shapeEndNavigator != null) {
		shapeEndNavigator.destroy();
		shapeEndNavigator = null;
	}
	// mp.events.call('notify', 2, 8, "Метка установлена на вашем GPS", 3000);
    shapeEndNavigator = mp.colshapes.newSphere(x, y, z, 45, localplayer.dimension)
	NavigatorBlip.setRoute(true);
	mp.game.wait(3000);
	navigatorstate = true;
});
var AudioPlay = false;
var lastdist;
mp.events.add('render', async () => {
	if (activeDictorNavigator) {
		if (navigatorstate) {
			if (localplayer.isInAnyVehicle(false)) {
				var directionInfo = mp.game.pathfind.generateDirectionsToCoord(posx, posy, posz, true);
				distance = JSON.stringify(directionInfo.direction);
				distToNxJunction = JSON.stringify(directionInfo.distToNxJunction);
				if (distance == 3 && distToNxJunction < 80 && !AudioPlay) {
					global.soundCEF.execute(`playSoundNavigator('./yandex_russian_` + selectedDictorYandex + `/turn_left.ogg');`);
					lastdist = 3;
					AudioPlay = true;
					await global.sleep(1500)
				}
				if (distance == 4 && distToNxJunction < 80 && !AudioPlay) {
					global.soundCEF.execute(`playSoundNavigator('./yandex_russian_` + selectedDictorYandex + `/turn_right.ogg');`);
					lastdist = 4;
					AudioPlay = true;
					await global.sleep(1500)
				}
				if (distance == 1 && !AudioPlay) {
					global.soundCEF.execute(`playSoundNavigator('./yandex_russian_` + selectedDictorYandex + `/recomputing.ogg');`);
					lastdist = 1;
					AudioPlay = true;
					await global.sleep(2500)
				}
				if (distance != lastdist) {
					AudioPlay = false;
				}
			}
		}
	}
});

function playerEnterEndNavigatorColshape(shape) {
	if(shape === shapeEndNavigator) {
		StopNavigator();
		if (shapeEndNavigator != null) {
			shapeEndNavigator.destroy();
			shapeEndNavigator = null;
		}
	}
}

mp.events.add("playerEnterColshape", playerEnterEndNavigatorColshape);
function StopNavigator() {
	if (localplayer.isInAnyVehicle(false) && activeDictorNavigator) 
		global.soundCEF.execute(`playSoundNavigator('./yandex_russian_` + selectedDictorYandex + `/finish.ogg');`);
	navigatorstate = false;
	mp.events.call('notify', 2, 8, "Вы прибыли на точку", 3000);
	if (NavigatorBlip != null) {
		NavigatorBlip.destroy();
		NavigatorBlip = null;
	}
}
//========================

//MARKET==================
mp.events.add("client::market:open", (a, b, c, d, e, f, g) => {
	global.menuOpen();
	mp.gui.execute(`market.Open(${a},${JSON.stringify(c)},${JSON.stringify(d)},${e},${f},${g}); market.active = true;`);
	mp.events.call('NPC.cameraOn', b, 800);
});

mp.events.add('client::market:buy', (a, b) => {
	mp.events.callRemote('server::market:buy', a, b);
});

mp.events.add('client::market:sell', (a, b) => {
	mp.events.callRemote('server::market:sell', a, b);
});

mp.events.add('client::market:order', (a, b) => {
	mp.events.callRemote('server::market:order', a, b);
});

mp.events.add('client::market:close', (a, b) => {
	global.menuClose();
	mp.events.call('NPC.cameraOff', 800);
});
//========================

mp.events.add('client::shop:open', (a, b, c) => {
	var street = mp.game.pathfind.getStreetNameAtCoord(localplayer.position.x, localplayer.position.y, localplayer.position.z, 0, 0);
	mp.gui.execute(`shop.active = true; shop.open(${JSON.stringify(a)}, ${JSON.stringify(mp.game.ui.getStreetNameFromHashKey(street.streetName))}, ${b}, ${c})`);
	global.menuOpen();
});

mp.events.add('client::shop:close', () => {
	global.menuClose();
})

mp.events.add('client::shop:buy', (a) => {
	mp.events.callRemote('server::shop:buy', false, a);
});

//BIZINFO=================
mp.events.add('client::biz:info:open', (a, b) => {
	if (global.menuCheck() || localplayer.getVariable('InDeath') == true || chatActive || editing || cuffed) return;	
	global.menu.execute(`BusinessInfo.data = ${a}; BusinessInfo.owner = ${b}; BusinessInfo.page = 0; BusinessInfo.active = true`);
	global.menuOpen(false);
});

mp.events.add('client::biz:info:close', () => {
	global.menuClose(false);
});

mp.events.add('client::biz:info:buy', () => {
	mp.events.callRemote('server::biz:info:buy');
});

mp.events.add('client::biz:info:sell', () => {
	mp.events.callRemote('server::biz:info:sell');
});
//========================

//Petrol==================
mp.events.add('client::petrol:open', (a, b, c) => {
	if (!loggedin || chatActive || editing || cuffed || localplayer.getVariable('InDeath') == true) return;
	mp.gui.execute(`petrol.active = true; petrol.price = ${a}; petrol.fuel = ${b}; petrol.move = ${b}; petrol.maxfuel = ${c};`);
	global.menuOpen();
});

mp.events.add('client::petrol:buy', (a,b) => {
	mp.events.callRemote('server::petrol:buy', parseInt(a),b);
});

mp.events.add('client::petrol:load', (a) => {
	mp.gui.execute(`petrol.load.active = true; petrol.movefuel(${a});`);
	setTimeout(() => {
		mp.events.call('client::soundplayPetrol', "./sounds/loadPetrol.mp3", 1);
	}, 100);
});

mp.events.add('client::petrol:endload', () => {
	mp.gui.execute(`petrol.load.active = false`);
	mp.events.callRemote('server::petrol:endload')
});

mp.events.add('client::petrol:close', () => {
	global.menuClose();
	mp.gui.execute(`petrol.active = false`);
});

mp.events.add('client::petrol:buyother', (a) => {
	mp.events.callRemote('server::petrol:buyother', a);
});
//==========================

mp.events.add('client::battlepass:getrequest', () => {
	mp.events.callRemote("server::battlepass:open");
});

mp.events.add('client::battlepass:opennoif', () => {
	mp.gui.execute(`PlayerMenu.battlepass.active = true;`);
	global.menuOpen();
});
mp.events.add('client::battlepass:open', (free, prem, qlist, tops, lvl, exp, buyed, timetogiveexp, pricesbuy, daytoend,maxtimeexp) => {
	mp.gui.execute(`PlayerMenu.battlepass.active = true; PlayerMenu.bp_open(${free},${prem},${qlist},${tops},${lvl},${exp},${buyed},${timetogiveexp},${pricesbuy},${daytoend},${maxtimeexp})`);
	global.menuOpen();
});

mp.events.add('client::battlepass:close', () => {
	mp.gui.execute(`PlayerMenu.battlepass.active = false`);
	global.menuClose();
});

mp.events.add('client::battlepass:takeitem', (a,b,c) => {
	mp.events.callRemote('server::battlepass:takeitem', a, b,c);
});

mp.events.add('client::battlepass:buyexp', (a,b,c) => {
	mp.events.callRemote('server::battlepass:buyexp', a, b, c);
});

mp.events.add('client::battlepass:gift', (a) => {
	mp.events.callRemote('server::battlepass:gift', parseInt(a));
});

mp.events.add('client::battlepass:buypass', (a) => {
	mp.events.callRemote('server::battlepass:buypass', a);
});

mp.events.add('client::battlepass:update', (key, value) => {
	mp.gui.execute(`PlayerMenu.battlepass.${key} = ${value}`);
});


//===========================================================
mp.keys.bind(Keys.VK_F2, false, () => {
	if (global.menuCheck() || !loggedin || chatActive || editing || cuffed || localplayer.getVariable('InDeath') == true) return;
	mp.events.callRemote("server::playermenu:open");
});

mp.events.add('client::playermenu:open', (a, b, c, d, e, f, g) => {
	mp.gui.execute(`PlayerMenu.active = true; PlayerMenu.Open(${a}, ${b}, ${c}, ${d}, ${e}, ${f}, ${g})`); 
	global.menuOpen(false);
});

mp.events.add('client::playermenu:close', () => {
	mp.gui.execute(`PlayerMenu.active = false; PlayerMenu.Reset()`);
	global.menuClose(false);
});






//CREATOR BLIPS=========================
global.CBBrowser = mp.browsers["new"]('http://package/interface/modules/CreatorBlips/index.html');

mp.keys.bind(Keys.VK_F4, false, () => {
	if (global.menuCheck() || !loggedin || chatActive || editing || cuffed || localplayer.getVariable('InDeath') == true) return;
	mp.events.callRemote("server::creatorblips:open");
});

mp.events.add('client::creatorblips:create', () => {
	mp.events.callRemote('server::creatorblips:create');
});

mp.events.add('client::creatorblips:remove', (a) => {
	mp.events.callRemote('server::creatorblips:remove', a);
});
mp.events.add('client::creatorblips:changeposition', (a) => {
	mp.events.callRemote('server::creatorblips:changeposition', a);
});

mp.events.add('client::creatorblips:tptoblip', (a) => {
	mp.events.callRemote('server::creatorblips:tptoblip', a);
});

mp.events.add('client::creatorblips:close', () => {
	global.CBBrowser.execute(`CreatorBlips.active = false;`);
	global.menuClose();
});

mp.events.add('client::creatorblips:setsettings', (a, b) => {
	mp.events.callRemote('server::creatorblips:setsettings', a, b);
});

mp.events.add('client::creatorblips:open', (a) => {
	global.CBBrowser.execute(`CreatorBlips.blips=${a}; CreatorBlips.active = true;`);
	global.menuOpen();
});

mp.events.add('client::creatorblips:update', (a) => {
	global.CBBrowser.execute(`CreatorBlips.blips=${a}; CreatorBlips.$forceUpdate()`);
});