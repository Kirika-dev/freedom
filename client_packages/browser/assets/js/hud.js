// var reverse = false;
// setInterval(() => {
	// if (!reverse) {
		// if (HUD.turnovers <= 50) {
			// reverse = true;
			// return;
		// }
		// HUD.turnovers-= 30;
		// HUD.speed--;
	// }
	// else {
		// if (HUD.turnovers >= 8000) {
			// reverse = false;
			// return;
		// }
		// HUD.turnovers+=30;
		// HUD.speed++;
	// }
// }, 10);
var HUD = new Vue({
    el: "#hud",
    data: {
        show: false,
		mic: false,
		id: 228,
        bank: 2500,
        money: 231312,
        date: "22.10.2020",
        time: "22:10",
        online: 100,
        maxOnline: 1000,
		crossingRoad: "Рокфорд",
		street: "Альта мерумонд",
		
		static: 1488,
		ammo: -1,
		maxAmmo: 500,

		interactionEntity: null,
		
		isVeh: false,
		speed: 190,
		fuel: 10,
		maxfuel: 120,
		hp: 0,
		maxhp: 1000,
		isEngine: false,
		isDoors: false,
		isBelt: false,
		isLights: false,
		isTrunk: false,
		isCruise: false,
		indicator: 0,
		transmission : 1,
		turnovers : 1000,
		mile: 30,
		
		water: 40,
		hunger: 29,
		bind1: "a",
		bind2: "s",
		bind3: "f",
		showhintsBind: "F7",
		activeStarts: 0,
		
		minimapFix: 0,
		minimapFix2: 0,
		
		quest: {
			active: false,
			text: "Вира, вира!",
			desc: "Перенесите 20 стройматериалов работая на любой стройке штата",
			progress: 10,
			progressmax: 10,
		},
		
		addmoney: "-1",
		moneyaddstate: false,
		
		hint: false,
		hinttext: "Нажмите для разговора",
		
		helpnoty: false,
		helpnotytext: null,

		bonus: {
			timefirst: [0, 0, 0],
			time: [0,0,0],
			state: [false, false, false]
		},
		radar: {
			state: -1,
			list: 
			[
				"Стандартный",
				"Отдаленный",
				"Расширенный",
				"Отключен"
			],
		},
		fines: {
			active: false,
			speed: 100,
			cost: 100,
			img: "",
		},

		binds:[
		  "A",
		  "A",
		  "A",
		  "A",
		  "A",
		  "A",
		  "A",
		  "A",
		],
		jobs: 
		{
			active: false,
			money: 100,
		},
		postal: 
		{
			parcel: 0,
		},
		zones: {
			active: false,
			name: "Carrier"
		},
		
		help: {
			active: true,
			list: 
			{
				"U" : "Анимации",
				"M" : "Телефон",
				"I" : "Инвентарь",
				"B" : "Двигатель",
				"L" : "Двери транспорта",
				"F2" : "Меню игрока",
				"F3" : "Перезагрузить микрофон",
				"F5" : "Скрыть интерфейс"
			},
		},
		
		quests: {
			active : false,
			name : "Знакомство с сервером",
			text : "Купите телефон в магазине 24/7",
		},
		
		week: "Вторник",
		weather: "CLEAR",
		gradusWeather: 20,
		compass: "NE",
		weatherType: 
		{
			"EXTRASUNNY": "Солнечно",
			"CLEAR": "Ясно",
			"CLOUDS": "Облачно",
			"SMOG": "Смог",
			"FOGGY": "Туман",
			"RAIN": "Дождь",
			"THUNDER": "Гром",
			"CLEARING": "Туман",
			"NEUTRAL": "Туман",
			"SNOW": "Снежно",
			"BLIZZARD": "Ветренно",
			"SNOWLIGHT": "Мелкий снег",
			"XMAS": "Снег",
			"HALLOWEEN": "Хэллоуин",
		},
		weatherTypeEmoji: 
		{
			"EXTRASUNNY": "☀️",
			"CLEAR": "🌤",
			"CLOUDS": "⛅️",
			"SMOG": "☁️",
			"FOGGY": "🌫",
			"RAIN": "🌧",
			"THUNDER": "⛈",
			"CLEARING": "🌫",
			"NEUTRAL": "🌫",
			"SNOW": "🌨",
			"BLIZZARD": "💨",
			"SNOWLIGHT": "🌨",
			"XMAS": "⛄️",
			"HALLOWEEN": "🧛‍♀️",
		},
		
		fisingGame: {
			state: false,
			line: 18,
			gran: 16,
			gran2: 16,
			interval: null,
		},
		fishing: {
			state: false,
			type: 2,
			list: ["Очень мало","Нормально","Много"],
			typeplayer: 0,
		},
		postalAdd: false,
    },
    mounted: function() {
		document.addEventListener('keyup', this.keyUp);
		document.addEventListener('keydown', this.keyDown);
	},
    methods: {
		keyDown: () => {
            if (!HUD.fisingGame.state) return
        },
        keyUp: () => {
            if (!HUD.fisingGame.state) return
            if (event.keyCode == 32) 
			{				
				if (HUD.fisingGame.line >= HUD.fisingGame.gran && HUD.fisingGame.line <= HUD.fisingGame.gran2) {
					HUD.fishGameFinish(true);
				}
				else {
					HUD.fishGameFinish(false);
				}
			}
		},
        getSpeed(){
            let num = 1000 + (this.speed * 100 / 300) * 8.6;
            return num > 1583 ? 1583 : num;
        },
		openPostalAdd: function() {
			this.postalAdd = true;
			setTimeout(() => {
				this.postalAdd = false;
			}, 6000);
		},
		showStar(i){
            return (i<=this.activeStarts)?true:false;
        },
		SetJobInfo: function(money) {
			this.jobs.active = true;
			this.jobs.money = money
		},
		addFine: function(speed, cost, img, id, number) {
			if (img === undefined) {
                mp.trigger("client::speedcheker:load");
                return;
            }
			this.fines.cost = cost;
			this.fines.speed = speed;
			toDataUrl(img, (dataURL) => {
                this.fines.img = dataURL;
            });
			this.fines.active = true;
			mp.trigger('client::fine:add', speed, cost, id, number, this.fines.img)
			setTimeout(() => {
				this.fines.active = false;
				clearTimeout(this);
			}, 8000);
		},
		openFishGame: function() {
			this.fisingGame.state = true;
			this.fisingGame.line = 0;
			this.fisingGame.gran = this.randomInteger(3, 20);
			this.fisingGame.gran2 = this.fisingGame.gran + this.randomInteger(7, 14)
			let reverse = false;
			let time = 0; 
			this.fisingGame.interval = setInterval(() => {
				if (!reverse) {
					if (this.fisingGame.line >= 36) {
						reverse = true;
						return;
					}
					this.fisingGame.line+= 0.5;
				}
				else {
					if (this.fisingGame.line <= 0) {
						reverse = false;
						return;
					}
					this.fisingGame.line-=0.5;
				}
				time++;
				if (time >= 290) {
					clearInterval(this.fisingGame.interval);
					this.fisingGame.interval = null;
					this.fishGameFinish(false);
				}
			}, 20);
		},
		fishGameFinish: function(a) {
			clearInterval(this.fisingGame.interval);
			this.fisingGame.interval = null;
			this.fisingGame.state = false;
			mp.trigger('client::fish::game:finish', a);
		},
		randomInteger: function(min, max) {
		  let rand = min + Math.random() * (max - min);
		  return Math.round(rand);
		},
		addKeysInList: function(OpenCarDoor, OpenInventory, OpenAnimMenu, OpenPhone, HandCuff, CruiseControl, StartEngineVehicle, safetyBelt){
		  this.binds[0] = OpenInventory
		  this.binds[1] = OpenAnimMenu
		  this.binds[2] = OpenPhone
		  this.binds[3] = OpenCarDoor
		  this.binds[4] = CruiseControl
		  this.binds[5] = StartEngineVehicle
		  this.binds[6] = safetyBelt
		  this.binds[7] = HandCuff
		},
		setQuest: function(state, label, text, progress, progressmax) {
			this.quests.active = state;
			this.quests.name = label;
			this.quests.text = text;
			// this.progress = progress;
			// this.progressmax = progressmax;
		},
		setHint: function(state, text) {
			this.hint = state;
			this.hinttext = text;
		},
		setHelpNoty: function(state, text) {
			this.helpnoty = state;
			this.helpnotytext = text;
		},
		setBonus: function(a, b) {
			this.bonus.time = a;
			this.bonus.state = b;
		},
        showNotify(title, status2, text2) {
            $('.notify_list').append(`
            <div class="notify ${status2} animate__animated animate__fadeInUp">
                <div class="line"></div>
                <img src="./images/player_hud/noty_${status2}.png" alt="" class="icon">
                <div class="content">
                    <div class="title"></div>
                    <div class="text">${text2}</div>
                </div>
            </div>`);
				var notify = $(' .notify_list .notify:last');
				setInterval(function () {
					notify.removeClass('animate__fadeInUp');

					notify.addClass('animate__fadeOutUp');
					setInterval(function () {
						notify.remove();
					}, 600)

				}, 6000);
        },
        
    }
});


var lastW = 0;
var lastR = 0;

function updatehud(width, ratio,safezone,offset=0) {

    lastW = width; lastR = ratio;

   let y1 = 316, y2 = 436;

    if(width > 2100) {
        offset += 98;
        
        document.querySelector(".mappings_block").style['bottom'] = '1px';
    }

    if(width < 1440) {
        offset -= 92;
        y2 = 404;

        document.querySelector(".mappings_block").style.bottom = '13px';
        document.querySelector(".mappings_block").style.transform = 'scale(0.75)';
    }

    if(width < 1320) {
        offset -= 38;
        document.querySelector(".mappings_block").style.bottom = '13px';
        document.querySelector(".mappings_block").style.transform = 'scale(0.75)';
    }

    const m = (y2 - y1) / (5/4 - 16/9);
    const b = y1 - (m * 16 / 9);

    document.querySelector(".mappings_block").style.left = `${m * ratio + b + offset}px`;
    
}
// HUD.openFishGame();
// HUD.openPostalAdd();	
function toDataUrl(url, callback) {
    
    var xhr = new XMLHttpRequest();
    xhr.onload = function() {
        var reader = new FileReader();
        reader.onloadend = function() {
            callback(reader.result);
        }
        reader.readAsDataURL(xhr.response);
    };

    
    xhr.open('GET', url);
    xhr.responseType = 'blob';
    xhr.send();
}
