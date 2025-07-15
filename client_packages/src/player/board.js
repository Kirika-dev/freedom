let cBack = ""

global.board = mp.gui;

global.rangeslider = {
    max: "",
    set: function (h) {
        this.max = h,
		global.board.execute(`rangeslider.max=${h}`);
    },
    open: function () {
		global.board.execute(`rangeslider.active=1`);
    },
    close: function () {
        global.board.execute(`rangeslider.active=0`);
        global.board.execute(`rangeslider.hide();`);
    }
};
mp.events.add('rangesliderboard', (text) => {
	mp.events.callRemote('rangesliderCallback', cBack, text);
    cBack = "";
    rangeslider.close();
});
mp.events.add('openModalRangeSlider', (h, c) => {
    rangeslider.set(h);
    cBack = c
    rangeslider.open();
})
mp.events.add('closeModalRangeSlider', () => {
    rangeslider.close();
})
global.openOutType = -1;

mp.events.add('openInventory', function () {
    if (!loggedin || chatActive || editing || cuffed || localplayer.getVariable('InDeath') == true) return;
    if (global.boardOpen)
        mp.events.callRemote('server::inventory:open', false);
    else {
        mp.events.callRemote('server::inventory:open', true);
		mp.events.callRemote("server::setbackpack");
	}
});

mp.keys.bind(Keys.VK_ESCAPE, false, function() {
    if (global.boardOpen) {
        mp.game.ui.setPauseMenuActive(false);
        mp.events.call('client::inventory:setinfo', 1);
    }
});

var reds = 0;
var donateOpened = false;

mp.events.add('redset', (reds_) => {
    reds = reds_;
    if (board != null)
        board.execute(`board.balance=${reds}`);
});

function openBoard() {
	if(board == null || global.menuCheck()) return;
    menuOpen(false);
	board.execute('board.active=true');
	board.execute(`board.nameplayer=${JSON.stringify(localplayer.name.replace("_"," "))}`)
	global.boardOpen = true;
	mp.events.call('client::soundplay', './sounds/open_inv.ogg', 0.5);
}

function closeBoard() {
	if(board == null) return;
    menuClose(false);
    board.execute('context.hide()');
	board.execute('board.active=false');
	board.execute('board.style=0');
	board.execute('board.selectedcraft=null');
    board.execute('board.outside=false');
    global.boardOpen = false;
	mp.events.call('client::soundplay', './sounds/close_inv.ogg', 0.5);
	
    if (global.openOutType != -1) {
        mp.events.callRemote('server::inventory:close');
        global.openOutType = -1;
    }
}

var last
mp.events.add('client::inventory:setinfo', (act, data, index, weight, itemground) => {
	switch(act){
		case 0:
			openBoard();
			break;
        case 1:
			closeBoard();
			break;
        case 2:
			board.execute(`board.stats=(${data})`);
			break;
		case 3:
			board.execute(`board.itemsSet(${index}, ${data}, ${weight})`);
			break;
		case 4:
			board.execute(`board.outSet(${data})`);
			break;
		case 5:
            board.execute(`board.outside=${data}`);
            global.openOutType = 0;
			break;
        case 6:
            board.execute(`board.itemUpd(${index},${data}, ${weight}, ${itemground})`);
        	break;
        case 11:
            global.openOutType = -1;
            closeBoard();
        	break;
	}
});



mp.events.add('boardCB', (act, type, index) => {
	if(new Date().getTime() - global.lastCheck < 100) return; 
	global.lastCheck = new Date().getTime();
	switch(act){
		case 1:
			mp.events.callRemote('server::inventory:cases', type, index, 'use');
		break;
		case 2:
			mp.events.callRemote('server::inventory:cases', type, index, 'transfer');
		break;
		case 3:
			mp.events.callRemote('server::inventory:cases', type, index, 'take');
		break;
		case 4:
			mp.events.callRemote('server::inventory:cases', type, index, 'drop');
		break;
		case 5:
			mp.events.callRemote('server::inventory:cases', type, index, 'takeground');
		break;
		case 10:
			mp.events.callRemote('server::inventory:cases', 10, index, 'backpack');
		break;
		case 21:
			mp.events.callRemote('server::inventory:cases', 21, index, 'garbage');
		break;
	}
});

mp.events.add('client::inventory:setslot', (a,b) => {
	mp.events.callRemote("server::inventory:setslot",a,b);
});

mp.events.add('client::inventory:removeslot', (a) => {
	mp.events.callRemote("server::inventory:removeslot",a);
});

let TimeOutSwap = 1000;

mp.keys.bind(Keys.VK_1, false, function() {
	if (!loggedin || chatActive || (new Date).getTime() - global.lastCheck < TimeOutSwap || global.menuOpened || mp.gui.cursor.visible) return;
	mp.events.callRemote("server::inventory:useitemslot", 1);
	global.lastCheck = (new Date).getTime();
});

mp.keys.bind(Keys.VK_2, false, function() {
	if (!loggedin || chatActive || (new Date).getTime() - global.lastCheck < TimeOutSwap || global.menuOpened || mp.gui.cursor.visible) return;
	mp.events.callRemote("server::inventory:useitemslot", 2);
	global.lastCheck = (new Date).getTime();
});

mp.keys.bind(Keys.VK_3, false, function() {
	if (!loggedin || chatActive || (new Date).getTime() - global.lastCheck < TimeOutSwap || global.menuOpened || mp.gui.cursor.visible) return;
	mp.events.callRemote("server::inventory:useitemslot", 3);
	global.lastCheck = (new Date).getTime();
});

mp.keys.bind(Keys.VK_4, false, function() {
	if (!loggedin || chatActive || (new Date).getTime() - global.lastCheck < TimeOutSwap || global.menuOpened || mp.gui.cursor.visible) return;
	mp.events.callRemote("server::inventory:useitemslot", 4);
	global.lastCheck = (new Date).getTime();
});

mp.keys.bind(Keys.VK_5, false, function() {
	if (!loggedin || chatActive || (new Date).getTime() - global.lastCheck < TimeOutSwap || global.menuOpened || mp.gui.cursor.visible) return;
	mp.events.callRemote("server::inventory:useitemslot", 5);
	global.lastCheck = (new Date).getTime();
});

mp.keys.bind(Keys.VK_0, false, function() {
	if (!loggedin || chatActive || (new Date).getTime() - global.lastCheck < TimeOutSwap || global.menuOpened || mp.gui.cursor.visible) return;
	mp.events.callRemote("server::inventory:swap");
	global.lastCheck = (new Date).getTime();
});