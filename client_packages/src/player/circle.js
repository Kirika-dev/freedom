global.circleEntity = null;
global.circleOpen = false;
var circleTitle = "";


global.OpenCircle = function(title, data) {
    if (global.menuCheck() || circleOpen) return;
	if (global.localplayer.getVariable("attachToVehicleTrunk")) return;
    // board.active = true
    global.board.execute(`circle.show("${title}",${data})`);
    circleTitle = title;
    circleOpen = true;
    global.menuOpen(false);
}
global.CloseCircle = function(hide) {
    if(hide) global.board.execute("circle.hide()");
    // board.active = false
    circleOpen = false;
    global.menuClose(false);
}

global.OpenFracData = function(title){
if (global.menuCheck() || circleOpen) return;
    global.board.execute(`circle.show("${title}",${pFraction})`);
    circleTitle = title;
    circleOpen = true;
    global.menuOpen();
}


// // //
mp.events.add('circleCallback', (index) => {
    if (index == -1) {
        global.CloseCircle(false);
    } else {
        global.CloseCircle(false);
        switch (circleTitle) {
            case "Машина":
    switch (index) {
        case 0:
        case 1:
        case 2:
        case 3:
		case 4:
		case 5:
        case 6:
        case 7:
            // if (entity == null) return;
            // mp.events.callRemote('vehicleSelected', entity, index);
            // return;
			let localPlayer = mp.players.local;
			if (global.entity == null) return;
			if(entity.getVariable("ACCESS") == "DUMMY") return;
			if (index == 1 || index == 3 || index == 6) {
				const boneID = entity.getBoneIndexByName("boot"); 
				var trunkpos = entity.getWorldPositionOfBone(boneID);
				if(boneID <= 0) trunkpos = localPlayer.position;
				mp.events.callRemote('vehicleSelected', entity, index, trunkpos.x, trunkpos.y, trunkpos.z);
				return;
			}
			else if (index == 0) {
				const boneID = entity.getBoneIndexByName("bonnet");
				var bonnetpos = entity.getWorldPositionOfBone(boneID);
				if(boneID <= 0) bonnetpos = localPlayer.position;
				mp.events.callRemote('vehicleSelected', entity, index, bonnetpos.x, bonnetpos.y, bonnetpos.z);
				return;
			}
			else {
				const boneID = entity.getBoneIndexByName("bodyshell");
				var bodyshell = entity.getWorldPositionOfBone(boneID);
				if(boneID <= 0) bodyshell = localPlayer.position;
				mp.events.callRemote('vehicleSelected', entity, index, bodyshell.x, bodyshell.y, bodyshell.z);
				return;
			}
    }
    return;
            case "Игрок":
                if (global.entity == null) return;
                switch (index) {
                    case 0:
                        mp.events.callRemote('pSelected', entity, "Передать деньги");
                        return;
                    case 1:
                        mp.events.callRemote('pSelected', entity, "Предложить обмен");
                        return;
                    case 2:
                        if (pFraction === 0 || pFraction === 15) return;
                        global.OpenCircle("Фракция", pFraction);
                        return;
                    case 3:
                        mp.events.callRemote('passport', entity);
                        return;
                    case 4:
                        mp.events.callRemote('licenses', entity);
                        return;	
                    case 5:
                        global.OpenCircle("Социальное", 0);
                        return;
                    case 6:
                        global.OpenCircle("Дом", 0);
                        return;
					case 7:
                        global.OpenCircle("Квартира", 0);
                        return;					
					
                }
                return;
            case "Социальное":
                switch (index) {
                    case 0:
                        mp.events.callRemote('pSelected', entity, "Вылечить");
                        return; 
                    case 1:
                        mp.events.callRemote('pSelected', entity, "Показать пластик");
                        return;
                    case 2:
                        mp.events.callRemote('pSelected', entity, "Пожать руку");
                        return;
                    case 3:
                        mp.events.callRemote('pSelected', entity, "Сыграть в кости");
                        return;
                    case 4:
                        mp.events.callRemote('pSelected', entity, "Поцеловать");
                        return; 
                }
            case "Дом":
                switch (index) {
                    case 0:
                        mp.events.callRemote('pSelected', entity, "Продать машину");
                        return;
                    case 1:
                        mp.events.callRemote('pSelected', entity, "Продать дом");
                        return;
                    case 2:
                        mp.events.callRemote('pSelected', entity, "Заселить в дом");
                        return;
                    case 3:
                        mp.events.callRemote('pSelected', entity, "Пригласить в дом");
                        return;
                }
                return;
			case "Квартира":
                switch (index) {
                    case 0:
                        mp.events.callRemote('pSelected', entity, "Продать машину");
                        return;
                    case 1:
                        mp.events.callRemote('pSelected', entity, "Продать квартиру");
                        return;
                    case 2:
                        mp.events.callRemote('pSelected', entity, "Заселить в квартиру");
                        return;
                    case 3:
                        mp.events.callRemote('pSelected', entity, "Пригласить в квартиру");
                        return;
                }
                return;
            case "Фракция":
                if (global.entity == null) return;
                global.circleEntity = entity;
                if (fractionActions[pFraction] == undefined) return;
                mp.events.callRemote('pSelected', entity, fractionActions[pFraction][index]);
                return;
			case "Семья":
                if (global.entity == null) return;
                global.circleEntity = entity;
                mp.events.callRemote('pSelected', entity, fractionActions[1][index]);
                return;
        }
    }
});

var aCategory = -1;

// // //
var pFraction = 0;
var fractionActions = [];
fractionActions[1] = ["Вести за собой", "Мешок", "Ограбить", "Сорвать маску"];
fractionActions[2] = ["Вести за собой", "Мешок", "Ограбить", "Сорвать маску"];
fractionActions[3] = ["Вести за собой", "Мешок", "Ограбить", "Сорвать маску"];
fractionActions[4] = ["Вести за собой", "Мешок", "Ограбить", "Сорвать маску"];
fractionActions[5] = ["Вести за собой", "Мешок", "Ограбить", "Сорвать маску"];
fractionActions[6] = ["Вести за собой", "Показать удостоверение", "Сорвать маску", "Обыскать"];
fractionActions[7] = ["Вести за собой", "Обыскать", "Изъять оружие", "Изъять нелегал", "Сорвать маску", "Выписать штраф", "Показать удостоверение", "Выдать пластик", "Посадить в КПЗ"];
fractionActions[8] = ["Продать аптечку", "Предложить лечение", "Показать удостоверение"];
fractionActions[9] = ["Вести за собой", "Обыскать", "Изъять оружие", "Изъять нелегал", "Сорвать маску", "Показать удостоверение"];
fractionActions[10] = ["Вести за собой", "Мешок", "Ограбить", "Сорвать маску"];
fractionActions[11] = ["Вести за собой", "Мешок", "Ограбить", "Сорвать маску"];
fractionActions[12] = ["Вести за собой", "Мешок", "Ограбить", "Сорвать маску"];
fractionActions[13] = ["Вести за собой", "Мешок", "Ограбить", "Сорвать маску", "Выдать сертификат"];
fractionActions[14] = ["Вести за собой", "Мешок", "Сорвать маску", "Показать удостоверение", "Обыскать"];
fractionActions[15] = ["Вести за собой", "Показать удостоверение"];
fractionActions[16] = ["Вести за собой"];
fractionActions[17] = ["Вести за собой", "Мешок", "Обыскать", "Изъять нелегал", "Сорвать маску", "Показать удостоверение", "Изъять оружие"];
fractionActions[18] = ["Вести за собой", "Мешок", "Обыскать", "Изъять нелегал", "Сорвать маску", "Показать удостоверение", "Изъять оружие"];
mp.events.add('fractionChange', (fraction) => {
    pFraction = fraction;
});