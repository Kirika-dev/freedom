const COLLDOWNS = 50;
var last_idS = -1;
var phone = new Vue({
    el: ".iphone",
    data: {
		active: false,
		loadpage: true,
		style: 0,
		modal: 0,
		modulemenu: 0,
		apps: [false, false, false, false, false, false, false],
		appstore: {
			downloading: [-1,-1,-1,-1,-1,-1,-1,-1,-1],
			interval: [null,null,null,null,null,null,null,null,]
		},
		workID: 0,
		deliveryclub: {
			style: 0,
			modal: 0,
			countadd: 1,
			items: [
				[0,"еКола",9,5000],
				[1,"Спрунк",10,5000],
				[2,"Коробка пиццы",5,5000],
				[3,"Бургер",6,5000],
				[4,"Пачка чипсов",3,12000],
				[5,"Пиво",4,12000]
			],
			basket: [],
			select: null,
			orders: [
				{ ID: 1, Price: 500, Distance: 400.2, Time: 2, Taken: false },
				{ ID: 2, Price: 500, Distance: 400.2, Time: 2, Taken: false },
				{ ID: 3, Price: 500, Distance: 400.2, Time: 2, Taken: false },
				{ ID: 4, Price: 500, Distance: 400.2, Time: 2, Taken: false },
				{ ID: 5, Price: 500, Distance: 400.2, Time: 2, Taken: false },
				{ ID: 6, Price: 500, Distance: 400.2, Time: 2, Taken: false },
				{ ID: 7, Price: 500, Distance: 400.2, Time: 2, Taken: false },
				{ ID: 8, Price: 500, Distance: 400.2, Time: 2, Taken: false },
				{ ID: 9, Price: 500, Distance: 400.2, Time: 2, Taken: false },
				{ ID: 10, Price: 500, Distance: 400.2, Time: 2, Taken: false },
				{ ID: 11, Price: 500, Distance: 400.2, Time: 2, Taken: false },
				{ ID: 12, Price: 500, Distance: 400.2, Time: 2, Taken: false },
				{ ID: 13, Price: 500, Distance: 400.2, Time: 2, Taken: false },
				{ ID: 14, Price: 500, Distance: 400.2, Time: 2, Taken: false },
				{ ID: 15, Price: 500, Distance: 400.2, Time: 2, Taken: false },
				{ ID: 16, Price: 500, Distance: 400.2, Time: 2, Taken: false },
				{ ID: 17, Price: 500, Distance: 400.2, Time: 2, Taken: false },
				{ ID: 18, Price: 500, Distance: 400.2, Time: 2, Taken: false },
				{ ID: 19, Price: 500, Distance: 400.2, Time: 2, Taken: false },
				{ ID: 20, Price: 500, Distance: 400.2, Time: 2, Taken: false },
			],
		},
		settings:  {
			sim: -1,
			airmode: false,
			wallpaper: 2,
		},
		house: {
			have: true,
			id: 103,
			price: 100000000,
			sellprice: 10,
		},
		apartaments: {
			have: true,
			id: 103,
			price: 100000000,
			sellprice: 10,
		},
		news: {
			list: 
			[
				{Date: "20:20 01.03.2022", Text: "Продам гараж срочно! Цена 2 000 000$", Author: "Ilya Merumond", NumberA: 234520},
				{Date: "20:20 01.03.2022", Text: "Продам почку дешево и быстро! Цена 13 000 000$", Author: "Maxim Borisow", NumberA: 234520},
				{Date: "20:20 01.03.2022", Text: "Продам гараж срочно! Цена 2 000 000$", Author: "Ilya Merumond", NumberA: 234520},
				{Date: "20:20 01.03.2022", Text: "Продам гараж срочно! Цена 2 000 000$", Author: "Ilya Merumond", NumberA: 234520},
				{Date: "20:20 01.03.2022", Text: "Продам гараж срочно! Цена 2 000 000$", Author: "Ilya Merumond", NumberA: 234520},
				{Date: "20:20 01.03.2022", Text: "Продам гараж срочно! Цена 2 000 000$", Author: "Ilya Merumond", NumberA: 234520},
				{Date: "20:20 01.03.2022", Text: "Продам гараж срочно! Цена 2 000 000$", Author: "Ilya Merumond", NumberA: 234520},
				{Date: "20:20 01.03.2022", Text: "Продам гараж срочно! Цена 2 000 000$", Author: "Ilya Merumond", NumberA: 234520},
				{Date: "20:20 01.03.2022", Text: "Продам гараж срочно! Цена 2 000 000$", Author: "Ilya Merumond", NumberA: 234520},
				{Date: "20:20 01.03.2022", Text: "Продам гараж срочно! Цена 2 000 000$", Author: "Ilya Merumond", NumberA: 234520},
			],
			typemsg: 0,
			msg: "",
		},
		gallery: 
		{
			list: [],
			index: -1,
		},
		bank: {
			input: '',
			trmoney: '',
			money: 10000000,
			card: 521034,
		},
		forbes: [
			{ Name: "Ilya Merumond", Money: 14235436 },
			{ Name: "Adamai Pidor", Money: 5000 },
			{ Name: "Trasher Yeban", Money: 1256000 },
			{ Name: "Ilya Merumond", Money: 14235436 },
			{ Name: "Ilya Merumond", Money: 14235436 },
			{ Name: "Ilya Merumond", Money: 14235436 },
			{ Name: "Ilya Merumond", Money: 14235436 },
			{ Name: "Ilya Merumond", Money: 14235436 },
			{ Name: "Ilya Merumond", Money: 14235436 },
			{ Name: "Ilya Merumond", Money: 14235436 },
			{ Name: "Ilya Merumond", Money: 14235436 },
			{ Name: "Ilya Merumond", Money: 14235436 },
			{ Name: "Ilya Merumond", Money: 14235436 },
			{ Name: "Ilya Merumond", Money: 14235436 },
			{ Name: "Ilya Merumond", Money: 14235436 },
		],
		selectautos: -1,
		autosinput: '',
		auto: [
			{Name: "Bentley Bentyaga 2018", Number: "X777XX30", Price: "2000000"},
			{Name: "Porsche 911", Number: "X777XX30", Price: "590000"},
			{Name: "BMW M8 Gran Coupe", Number: "TRANSIT_X777XX30", Price: "99995014"},
			{Name: "Bentley Bentyaga 2018", Number: "X777XX30", Price: "2000000"},
			{Name: "Bentley Bentyaga 2018", Number: "X777XX30", Price: "2000000"},
			{Name: "Bentley Bentyaga 2018", Number: "X777XX30", Price: "2000000"},
			{Name: "Bentley Bentyaga 2018", Number: "X777XX30", Price: "2000000"},
		],
		navigator: {
			voice: false,
			voiceSound: 0,
			listaudios: ["Яндекс Алиса", "Баста", "Ольга Бузова", "Дарт Вейдер", "Артем Дзюба", "Гарик Харламов", "Диктор Mercedes-Benz", "Оптимус Прайм"],
			index: 0,
			input: '',
			organizations: //0 to 20 
			[
				[0,"Los Santos Police Department", "Полиция"],
				[1,"Emergency Medical Center", "Больница"],
				[2,"Government", "Мэрия"],
				[3,"Federal Investigation Bureau", "Фиб"],
				[4,"San Andreas National Guard", "Армия"],
				[5,"Weazel News", "Новости"],
				[6,"Итальянское посольство", "Мафия"],
				[7,"Русская мафия", "Мафия"],
				[8,"Частная охранная организация", "ЧОО"],
				[9,"The Families", "Банда"],
				[10,"The Ballas Gang", "Банда"],
				[11,"Los Santos Vagos", "Банда"],
				[12,"Marabunta Grande", "Банда"],
				[13,"The Bloods Gang", "Банда"],
			],
			works: //21 to 40
			[
				[21, "Электрик"],
				[22, "Почтальон GoPostal"],
				[23, "Таксопарк"],
				[24, "Автобусная станция"],
				[25, "Дальнобойщик"],
				[26, "Инкассатор"],
				[27, "Мусоровозщик"],
				[28, "Строитель"],
				[29, "Каменьщик"],
				[30, "Дайвер"],
				[31, "Уборщик"],
				[32, "Ферма"],
			],
			business: //  41 to 70
			[
				[41, "АЗС"],
				[42, "Магазин 24/7"],
				[43, "Автосалон Эксклюзивного транспорта"],
				[44, "Автосалон Премиальных авто"],
				[45, "Автосалон Люксовых автомобилей"],
				[46, "Автосалон Экономных авто"],
				[47, "Мотосалон"],
				[48, "Салон грузового авто"],
				[49, "Воздушный транспорт"],
				[50, "Барбер-шоп"],
				[51, "Починка транспорта"],
				[52, "Автомастерская"],
				[53, "Автомойка"],
				[54, "Магазин одежды"],
				[55, "Тату салон"],
				[56, "Магазин оружия"],
				[57, "Магазин масок"],
				[58, "Магазин семян"],
				[59, "Аптека"],
				[60, "Рыболовный магазин"],
				[61, "Магазин электроники"],
			],
			other: //71 to 100 
			[
				[71, "Банкомат" ],
				[72, "Банк" ],
				[73, "Аренда скутера" ],
				[74, "Аренда лодок" ],
				[75, "Центр лицензирования" ],
				[76, "Казино" ],
				[77, "Рынок" ],
				[78, "Maze Bank Арена" ],
				[79, "Контейнеры" ],
				[80, "Авторынок" ],
				[81, "Церковь" ],
				[82, "Установка номеров" ],
				[83, "Переправа на Cayo Perico" ],
			]
		},
		thisweather: "CLOUDS",
		time: "20:10",
		hour: 15,
		degrees: {"0":["CLOUDS",5],"1":["CLEAR",17],"2":["CLEAR",8],"3":["CLOUDS",7],"4":["CLOUDS",3],"5":["CLOUDS",5],"6":["CLOUDS",6],"7":["CLOUDS",21],"8":["CLEAR",16],"9":["RAIN",11],"10":["CLEAR",3],"11":["CLOUDS",14],"12":["CLEAR",5],"13":["CLEAR",11],"14":["CLOUDS",5],"15":["FOGGY",9],"16":["CLOUDS",17],"17":["CLEAR",9],"18":["CLOUDS",24],"19":["CLEAR",17],"20":["CLEAR",11],"21":["CLOUDS",5],"22":["CLEAR",20],"23":["FOGGY",0],"24":["CLOUDS",0],"25":["CLOUDS",6],"26":["CLOUDS",3],"27":["CLOUDS",10],"28":["RAIN",9],"29":["CLOUDS",11]},
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
		weatherTypeIMG: 
		{
			"EXTRASUNNY": "sunny",
			"CLEAR": "sunny",
			"CLOUDS": "cloudy",
			"SMOG": "cloudy",
			"FOGGY": "murkly",
			"RAIN": "rain",
			"THUNDER": "rain",
			"CLEARING": "murkly",
			"NEUTRAL": "murkly",
			"SNOW": "snow",
			"BLIZZARD": "murkly",
			"SNOWLIGHT": "snow",
			"XMAS": "snow",
			"HALLOWEEN": "murkly",
		},
		list_notify: [],
        interval: null,
        icons: ["Fleeca Bank", "Настройки", "Навигатор", "Имущество", "Автомобиль", "Weazel News", "Сообщения"],
		calls: {
			input: "",
			caller: "",
			status: 1,
			statuses: ["идет вызов на сотовый...","говорите","вызов завершен","входящее","не отвечает"]
		},
		lockload: false,
		lastStyle: 0,
		BlockControl: false,

		playerName: "salaga",
    },
    methods:{
		goLoad: function() {
			this.lockload = true;
			this.loadpage = false;
		},
		openLoad: function() {
			this.loadpage = true;
			this.lockload = false;
		},
		removeDeliveryItem: function(a) {
			if (this.BlockControl == true) return;
			this.BlockControl = true;
			this.deliveryclub.basket.splice(a, 1);
			this.$forceUpdate();
			setTimeout(() => {
				this.BlockControl = false;
			}, 300);
		},
		goDeliveryStyle: function(a) {
			if (a == 4) {
				mp.trigger('client::phone::deliveryclub:ordersget');
				return;
			}
			this.deliveryclub.style = a;
		},
		goDeliveryModal: function(a) {
			this.deliveryclub.modal = a;
			this.deliveryclub.countadd = 1;
		},
		DeliveryVector: function(a) {
			if (a == true)
			{	
				this.deliveryclub.countadd++;
			}
			else {
				if (this.deliveryclub.countadd == 1) return;
				this.deliveryclub.countadd--;
			}
		},
		BuyAllDelivery: function() {
			if (this.deliveryclub.basket.length == 0) return;
			mp.trigger("client::phone::delivery:buyall", JSON.stringify(this.deliveryclub.basket));
		},
		selectDeliveryItem: function(a) {
			this.deliveryclub.select = a;
			this.deliveryclub.modal = 1;
		},
		addToBasket: function() {
			if (this.BlockControl) return;
			this.BlockControl = true;
			
			let item = this.deliveryclub.select;
			item.push(this.deliveryclub.countadd);
			this.deliveryclub.basket.push(item);
			this.deliveryclub.modal = 0;
			this.deliveryclub.countadd = 1;
			setTimeout(() => {
				this.BlockControl = false;
			}, 300);
		},
		takeOrderDelivery: function(a) {
			mp.trigger("client::phone::deliveryclub:takeorder", a);
		},
		downloadApp: function(a) {
			this.appstore.downloading[a] = 0;
			this.appstore.interval[a] = setInterval(() => {
				if (this.appstore.downloading[a] >= 120) {
					this.apps[a] = true;
					this.$forceUpdate();
					clearInterval(this.appstore.interval[a]);
					this.appstore.interval[a] = null;
					mp.trigger("client::phone:appstore", a, true);
					return
				}
				this.appstore.downloading[a] += 10;
				this.$forceUpdate();
			}, 600);  
			// this.apps[a] = true;
		},
		getMinGradus: function() {
			var result = 35;
			for (var i = 0; i < 30; i++) {
				if (result >= parseInt(this.degrees[i][1]))
					result = this.degrees[i][1]
			}
			return result;
		},
		getMaxGradus: function() {
			var result = 0;
			for (var i = 0; i < 30; i++) {
				if (result <= this.degrees[i][1])
					result = this.degrees[i][1]
			}
			return result;
		},
		LockLoadState: function(a) {
			setTimeout(() => {
			this.lockload = a;
			}, 100);
		},
		pushNoty: function(type, text, time) {
            last_idS++;
            this.list_notify.push([type, text, time, 0, last_idS])
            
            if (this.interval == null)
                this.interval = setInterval( this.worker, Number(COLLDOWNS))
        },

        worker: function() 
        {
            for(let i = this.list_notify.length - 1; i > -1; i--)
            {
                let notify = this.list_notify[i];

                notify[3] = notify[3] + COLLDOWNS;

                if (notify[2] <= notify[3])
                {
                    this.list_notify.splice(i, 1);
                }
            }

            this.$forceUpdate();

            if (this.list_notify.length == 0)
            {
                clearInterval(this.interval);
                this.interval = null;
            }
        },
		startCall: function() {
			if (this.calls.input.length >= 1)
			this.openCallMenu(this.calls.input, 0);
			mp.trigger('client::phone:callstart', this.calls.input);
		},
		acceptCall: function() {
			mp.trigger('client::phone:callaccept');
		},
		endCall: function() {
			mp.trigger('client::phone:callend');
		},
		openCallMenu: function(num, a) {
			this.lastStyle = this.style;
			this.calls.caller = num;
			this.style = 10;
			this.modal = 10;
			this.calls.status = a;
			this.$forceUpdate();
		},
		closeCallMenu: function(a) {
			this.style = this.lastStyle;
			this.calls.status = a;
			setTimeout(() => {
				this.modal = 0;
				this.$forceUpdate();
			}, 1000);
			this.$forceUpdate();
		},
		addCallInput: function(a) {
			if (this.calls.input.length >= 6) return;
			this.calls.input += a;
		},
		removeCallInput: function() {
			if (this.calls.input.length <= 0) return;
			this.calls.input = "";
		},
		Open: function (house, apart, veh, forbes, bank, money, sim, wall, air, voicen, actna, work, orderdelivery, itemsdelivery, appstore, name) {
			this.house = house;
			this.apartaments = apart;
			this.auto = veh;
			this.forbes = forbes;
			this.bank.card = bank;
			this.bank.money = money;
			this.settings.sim = sim;
			this.settings.wallpaper = wall;
			this.settings.airmode = air;
			this.navigator.voiceSound = voicen;
			this.navigator.voice = actna;
			this.workID = work;
			this.deliveryclub.orders = orderdelivery;
			this.deliveryclub.items = itemsdelivery;
			this.apps = appstore;

			this.playerName = name;
			
			this.deliveryclub.basket = [];
		},
		removeGalleryItem: function(a) {
			if (this.gallery.index == -1) return;
			this.gallery.list.splice(a, 1);
			this.modal = 0;
			this.modulemenu = 0;
			this.gallery.index = -1
		},
		addPhoto: function(img) {
			toDataURL(img, function(dataUrl) {
				phone.gallery.list.push(dataUrl);
				// mp.trigger('client::photo:sendcameralist', JSON.parse(dataUrl));
			  }
			);
			this.$forceUpdate();
		},
		selectVoiceNavigator: function(a) {
			this.navigator.voiceSound = a;
			mp.trigger('client::phone:setvoice', a)
		},
		selectVoiceStateNavigator: function() {
			setTimeout(() => {
				mp.trigger('client::phone:navigatorvoiceactive', this.navigator.voice)
			}, 100);
		},
		selectWallpaper: function(index) {
			this.pushNoty(1,"Обои установлены", 2000);
			this.settings.wallpaper = index;
			mp.trigger('client::phone:settingsset', this.settings.wallpaper);
		},		
        gostyle: function(index) {
            this.style = index;
			if (index == 0) {
				setTimeout(() => {
					this.navigator.input = '';
					this.navigator.index = 0;
					this.modal = 0;
					this.deliveryclub.style = 0;
					this.deliveryclub.modal = 0;
					this.deliveryclub.basket = [];
					this.deliveryclub.countadd = 1;
				}, 500);
			}
			if (index == 8) {
				setTimeout(() => {
					this.modal = 8;
				}, 4000);
			}
			if (index == 18) {
				this.deliveryclub.style = 0;
				setTimeout(() => {
					this.deliveryclub.style = 1;
				}, 2000);
			}
        },
		aviaMode: function() {
			setTimeout(() => {
				mp.trigger('client::phone:setaviamode', this.settings.airmode);
			}, 100);
		},			
		goNavigator: function(id) {
			this.pushNoty(2,"Метка установлена на GPS", 2000);
			mp.trigger('client::phone:gps', id)
		},
		openCameraMenu: function() {
			mp.trigger('client::phone:opencamera')
		},
		selectPhoto: function(id) {
			this.gallery.index = id;
			if (id >= 0) {
				this.modal = 5;
			}
			else {
				this.modal = 2;
			}
		},
		selectNewsTypeMsg: function(id) {
			this.news.typemsg = id;
		},
		valid: function(f) {
			if (this.bank.trmoney.length >= 9 && f.value.length < 9) {
				this.bank.trmoney = f.value;
				return;
			}
			if (this.bank.trmoney.length >= 9) 
			{
				f.value = this.bank.trmoney;
				return;
			}
			this.bank.trmoney = f.value;
		},
		selectauto: function(index) {
			this.selectautos = index;
			this.modal = 3;
		},
		gomodal: function(index) {
			if (index == 1 && !this.house.have) return;
			if (index == 2 && !this.apartaments.have) return;
			if (index == 0) this.autosinput = '';
			this.modal = index;
		},
		transferMoney: function() {
			if (this.bank.input != '' || this.bank.trmoney != '')
			mp.trigger('client::phone:transfermoney', this.bank.input, this.bank.trmoney);
		},
		selectHouseTab: function(a) {
			if (a == 3) 
			{
				this.modulemenu = 0;
				this.modal = 0;
			}
			mp.trigger('client::phone:switchhouse', a);
		},
		selectApartTab: function(a) {
			if (a == 3) 
			{
				this.modulemenu = 0;
				this.modal = 0;
			}
			mp.trigger('client::phone:switchapart', a);
		},
		goModule: function(index) {
			this.modulemenu = index;
		},
		btnSelectNavigator: function(id) {
			this.navigator.index = id;
		},
		selectVehicleTab: function(a) {
			if (this.selectautos == -1) return;
			mp.trigger('client::phone:switchvehicle', a, this.auto[this.selectautos].Number)
			if (a == 4) 
			{
				this.modulemenu = 0;
				this.modal = 0;
				this.selectautos = -1;
			}
		},
		reset: function() {
			setTimeout(() => {
				this.style = 0;
				this.modal = 0;
				this.modulemenu = 0;
				this.openLoad();
			}, 500);
		}
    }
});

function toDataURL(src, callback, outputFormat) {
  var img = new Image();
  img.crossOrigin = 'Anonymous';
  img.onload = function() {
    var canvas = document.createElement('CANVAS');
    var ctx = canvas.getContext('2d');
    var dataURL;
    canvas.height = this.naturalHeight;
    canvas.width = this.naturalWidth;
    ctx.drawImage(this, 0, 0);
    dataURL = canvas.toDataURL(outputFormat);
    callback(dataURL);
  };
  img.src = src;
  if (img.complete || img.complete === undefined) {
    img.src = "data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///ywAAAAAAQABAAACAUwAOw==";
    img.src = src;
  }
}