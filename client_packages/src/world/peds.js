const Natives = {
    TASK_START_SCENARIO_IN_PLACE: '0x142A02425FF02BD9',
};
let pedlist = [
    { Name: 'Bruno', Hash: 0x62018559, Pos: new mp.Vector3(-1031.3076, -2736.0532, 20.21441), Angle: 120, cameraRotate: 110.0, label: "Бруно Коллинз \n Проводник", Scenario: "", Anim1: "", Anim2: "", AnimFlag: 1 }, // 
    { Name: 'Gloria', Hash: 0x38BAD33B, Pos: new mp.Vector3(-1061.9181, -1712.5243, 4.56516), Angle: 50, cameraRotate: 210.0, label: "Глория Коллинз \n Местная", Scenario: "", Anim1: "", Anim2: "", AnimFlag: 1 }, // 
//фракции	
    { Name: 'NancySpungen', Hash: 368603149, Pos: new mp.Vector3(443.50964, -983.9889, 30.689332), Angle: 90, cameraRotate: 180.0, label: "Нэнси Спункен \n Сдача Награбленного", Scenario: "", Anim1: "", Anim2: "", AnimFlag: 1 }, // 
    { Name: 'SteeveHains', Hash: 941695432, Pos: new mp.Vector3(149.1317, -758.3485, 242.152), Angle: 66, cameraRotate: 154.0, label: "Стив Хейнс \n Помощник FIB", Scenario: "", Anim1: "", Anim2: "", AnimFlag: 1 }, // 
    { Name: 'MichaelBisping', Hash: 1558115333, Pos: new mp.Vector3(120.0836, -726.7773, 242.152), Angle: 248, cameraRotate: 340.0, label: "Михаил Биспинг \n Агент FIB", Scenario: "", Anim1: "", Anim2: "", AnimFlag: 1 }, // 
    { Name: 'RonnyPain', Hash: 1925237458, Pos: new mp.Vector3(-2347.958, 3268.936, 32.81076), Angle: 240, cameraRotate: 330.0, label: "Рони Пэйн \n Командир", Scenario: "", Anim1: "", Anim2: "", AnimFlag: 1 }, // 
    { Name: 'InformatorGOV', Hash: 0x5D71A46F, Pos: new mp.Vector3(-555.65356, -185.80292, 38.2211), Angle: -144, cameraRotate: -240.0, label: "Бэлла Стюарт \n Сотрудник GOV", Scenario: "", Anim1: "", Anim2: "", AnimFlag: 1 }, // 
    { Name: 'NPCMedCard', Hash: 0xE7565327, Pos: new mp.Vector3(309.57202, -593.7936, 43.284093), Angle: 29, cameraRotate: -240.0, label: "Доктор Хэйнс \n Медицинская карта", Scenario: "", Anim1: "", Anim2: "", AnimFlag: 1 }, // 
    { Name: 'NPCGunLic', Hash: 0xA5C787B6, Pos: new mp.Vector3(443.5182, -979.90936, 30.689315), Angle: 90, cameraRotate: 220.0, label: "Марк Руффало \n Лицензия на оружие", Scenario: "", Anim1: "", Anim2: "", AnimFlag: 1 }, // 
//работы	
	{ Name: 'RonBuilder', Hash: 0xD7DA9E99, Pos: new mp.Vector3(-169.25494, -1026.9185, 27.292077), Angle: 167, cameraRotate: 30.0, label: "Рон \n Прораб стройки" }, //
	{ Name: 'DeliveryClub', Hash: 0x7461A0B0, Pos: new mp.Vector3(-1037.7709, -1397.0588, 5.5531913), Angle: 79, cameraRotate: 210.0, label: "Брэндон Кремер \n Работодатель" }, //
//аренда
	{ Name: 'RENT1', Hash: 0x2AD8921B, Pos: new mp.Vector3(-767.13214 -1472.6307, 4.0005227), Angle: 74, cameraRotate: 140.0, label: "Дэниел \n Арендодеталь" }, //
	{ Name: 'RENT2', Hash: 0x2AD8921B, Pos: new mp.Vector3(-207.65958, -1010.3089, 29.145309), Angle: 74, cameraRotate: 140.0, label: "Джо \n Арендодеталь" }, //
	{ Name: 'RENT3', Hash: 0x2AD8921B, Pos: new mp.Vector3(327.24774, -1367.8464, 32.060142), Angle: 38, cameraRotate: 30.0, label: "Эндрю \n Арендодеталь" }, //
	{ Name: 'RENT4', Hash: 0x2AD8921B, Pos: new mp.Vector3(-498.68393, -289.67007, 35.650645), Angle: 167, cameraRotate: 30.0, label: "Мэйсон \n Арендодеталь" }, //
	{ Name: 'RENT5', Hash: 0x2AD8921B, Pos: new mp.Vector3(819,0702, -44.77016, 80.65926), Angle: 167, cameraRotate: 30.0, label: "Уильям \n Арендодеталь" }, //
	{ Name: 'RENT6', Hash: 0x2AD8921B, Pos: new mp.Vector3(-1241.6735, 251.6, 64.98314), Angle: 167, cameraRotate: 30.0, label: "Александр \n Арендодеталь" }, //
	{ Name: 'RENT7', Hash: 0x2AD8921B, Pos: new mp.Vector3(-475.0324, -608.9132, 32.204247), Angle: 167, cameraRotate: 30.0, label: "Майкл \n Арендодеталь " }, //
    { Name: 'RENT8', Hash: 0x2AD8921B, Pos: new mp.Vector3(-1039.2018, -2730.537, 20.214412), Angle: -79, cameraRotate: 30.0, label: "Фёдор \n Арендодеталь" }, //
    { Name: 'RENT9', Hash: 0x2AD8921B, Pos: new mp.Vector3(245.17807, -573.1641, 43.283887), Angle: -78, cameraRotate: 30.0, label: "Виктор \n Арендодеталь" }, //
	
    { Name: 'ChangeNumNPC', Hash: 0x0703F106, Pos: new mp.Vector3(443.53763, -986.5556, 30.689344), Angle: 90, cameraRotate: 190.0, label: "Каролина \n Создание Номера", Scenario: "", Anim1: "", Anim2: "", AnimFlag: 1 }, // номера

    { Name: 'DrugDeller', Hash: 0x4117D39B, Pos: new mp.Vector3(308.30505, -595.4678, 43.284066), Angle: 78, cameraRotate: 30.0, label: "Павел Техник \n Нарколог" },
    { Name: 'EmsHealNpc', Hash: 0xD47303AC, Pos: new mp.Vector3(311.67944, -594.0574, 43.284066), Angle: -28, cameraRotate: 30.0, label: "Артур Патинсон \n Лечащий врач" },
    { Name: 'ParcelNPC', Hash: 0x62599034, Pos: new mp.Vector3(82.71602, 132.91577, 80.53348), Angle: 72, cameraRotate: 210.0, label: "Эдвард Харисон \n Оператор GoPostal" },
    { Name: 'GOPOSTAL', Hash: 0x7367324F, Pos: new mp.Vector3(83.48956, 135.21211, 80.53348), Angle: 72, cameraRotate: 210.0, label: "Джэймс Фостер \n Работодатель GoPostal" },
	
    { Name: 'MNPC_Miner', Hash: 0x867639D1, Pos: new mp.Vector3(2832.54, 2798.8704, 57.452718), Angle: 107, cameraRotate: 155.0, label: "Роберт Маккензи \n Покупка инструментa" },
    { Name: 'MNPC_Sawmill', Hash: 0xC5FEFADE, Pos: new mp.Vector3(-687.3896, 5487.0215, 47.313115), Angle: 14, cameraRotate: 320.0, label: "Карл Магнум \n Покупка инструментa" },
    { Name: 'MNPC_SellerOrePile', Hash: 0xAD9EF1BB, Pos: new mp.Vector3(182.11086, 2790.9521, 45.617017), Angle: -44, cameraRotate: 100.0, label: "Этан Стоукс \n Продажа ресурсов" },
];

var Peds = [];
pedlist.forEach(ped => {
	Peds[ped.Name] = {
		uid : ped.Name,
		entity : mp.peds.new(ped.Hash, ped.Pos, ped.Angle, 0),
		extra_rotate : ped.cameraRotate,
		labelText : ped.label,
		position : ped.Pos,
		labelObject : createLabel(ped.label, ped.Pos),
		colShape : null,
		createShape() { 
			this.removeShape();
			this.colShape = mp.colshapes.newSphere(this.position.x, this.position.y, this.position.z, 2); 
			this.colShape.interact = ped.Name; 
			this.colShape.in = false;
		},
		removeShape() { 
			if(this.colShape!=null){
				if(this.colShape.in){
					mp.players.local.npcInteract = null;
				}
				this.colShape.destroy(); 
			}
			this.colShape = null;
		},
	}
});

function calculateDistance(v1, v2) {
    var dx = v1.x - v2.x;
    var dy = v1.y - v2.y;
    var dz = v1.z - v2.z;

    return Math.sqrt(dx * dx + dy * dy + dz * dz);
}

function createLabel(label, position){
    if (calculateDistance(localplayer.position, position) < 6.0) {
		const xy = mp.game.graphics.world3dToScreen2d(position.x, position.y, position.z);
		if (xy != null) {
			return label=="" ? null : global.drawText("~y~NPC ~w~" + label, xy.x, xy.y, 0.25)
		}
	}
}
var PedName = [];
mp.events.add('render', () => {
	pedlist.forEach(ped => {
        PedName[ped.Name] = {
            labelObject : createLabel(ped.label, ped.Pos),
        }
    });	
});

let hiding;
let handCamera;
let hidedPlayers = [];
let hidedLabels =[];
let oldpos;
function toRadian(x){
    return Math.PI*x/180;
}

mp.events.add('NPC.ColShape.Local', (npcName, flag)=>{
    if(Peds[npcName]!=null){
        if(flag) Peds[npcName].createShape();
            else Peds[npcName].removeShape();

    }
});

mp.events.add('NPC.cameraOn', (pedName, transitionTime = 0) => {
    handCamera = mp.cameras.new('default', new mp.Vector3(0,  0,  0), new mp.Vector3(0,0,0), 50);
    handCamera.setActive(true);
    handCamera.pointAtPedBone(Peds[pedName].entity.handle, 31086, 0, 0, 0, true);
    handCamera.setCoord(Peds[pedName].entity.getCoords(true).x + (Math.sin(toRadian(Peds[pedName].entity.getHeading()+Peds[pedName].extra_rotate))*2), Peds[pedName].entity.getCoords(true).y+(Math.cos(toRadian(Peds[pedName].entity.getHeading()+Peds[pedName].extra_rotate))*2), Peds[pedName].entity.getCoords(true).z+0.5);
    mp.game.cam.renderScriptCams(true, transitionTime>0, transitionTime, true, false);
	
    hiding = startHide(Peds[pedName].entity.getCoords(true));
    oldpos = mp.players.local.position;
    mp.players.local.position = new mp.Vector3(Peds[pedName].entity.getCoords(true).x + (Math.sin(toRadian(Peds[pedName].entity.getHeading()+Peds[pedName].extra_rotate))*2), Peds[pedName].entity.getCoords(true).y+(Math.cos(toRadian(Peds[pedName].entity.getHeading()+Peds[pedName].extra_rotate))*2), Peds[pedName].entity.getCoords(true).z+0.5);
});

mp.events.add('NPC.cameraOff', (transitionTime = 0)=>{
    if(hiding!=null){
    clearInterval(hiding);
    hiding = null;
    }
    if(handCamera!=null){
        mp.game.cam.renderScriptCams(false, transitionTime>0, transitionTime, true, true);
        handCamera.destroy();
        handCamera = null;
    }
    mp.players.local.position = oldpos;
    oldpos = null;
    
    
    setTimeout(()=>{
        while(hidedLabels.length>0){
            var pedName = hidedLabels.pop();
            Peds[pedName].labelObject = createLabel(Peds[pedName].labelText, Peds[pedName].position);
        }

        while(hidedPlayers.length>0)hidedPlayers.pop().setAlpha(255); 
    }, transitionTime/2);
});


function startHide(pos){
    mp.players.local.setAlpha(0);
    hidedPlayers.push(mp.players.local);
    return setInterval(function (vector){
        mp.players.forEachInStreamRange(player => {
            if(vector.subtract(player.position).length()<10){
                if(player.getAlpha()>0)
                {
                player.setAlpha(0);
                hidedPlayers.push(player);
                }
            }else if(hidedPlayers.includes(player)){
                hidedPlayers.splice(hidedPlayers.indexOf(player), 1);
            }
        });
    }, 1000, pos);
}

let questIcons = [];
let hidedIcon = null;

mp.events.add('Quest.NpcIcon.addQuest',(npcName)=>{
    questIcons.push(npcName);
});

mp.events.add('Quest.NpcIcon.removeQuest',(npcName)=>{
	if (questIcons.indexOf(npcName) != null)
		questIcons.splice(questIcons.indexOf(npcName), 1);
});

mp.events.add('Quest.NpcIcon.hideQuest',(npcName)=>{
    questIcons.splice(questIcons.indexOf(npcName), 1);
    hidedIcon = npcName;
});

mp.events.add('render', ()=>{
    if(questIcons.length>0){
        var vector = new mp.Vector3(mp.players.local.getCoords(true).x, mp.players.local.getCoords(true).y, mp.players.local.getCoords(true).z);
        questIcons.forEach(npcName => {
            var dist = vector.subtract(Peds[npcName].position).length();
            if(dist<6) {
				const xy = mp.game.graphics.world3dToScreen2d(Peds[npcName].entity.getCoords(true).x, Peds[npcName].entity.getCoords(true).y, Peds[npcName].entity.getCoords(true).z+1);
				if (xy != null) {
					global.drawSprite('majestic_textures_001', 'logo', [0.09, 0.09], 0, {
					  r: 238,
					  g: 199,
					  b: 80,
					  a: 255
					}, xy.x, xy.y - 0.07 * 0.7);
				}
			}
        });
    }
});


function calculateScale(distance){
    return [-Math.sqrt((distance/4)+10)+4.15, -Math.sqrt((distance/4)+10)+4.15];
}