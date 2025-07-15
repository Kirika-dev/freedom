var PlayerMenu = new Vue({
    el: '.playermenu',
    data: {
        active: false,
		style: 0, 
		modal: 0,
		
		promoused: "START",
		settings: {
			account: [false, true, false],
		},
		
		achievements: [
			{
				ID: 1,
				Name: "Молодость",
				Description: "Достичь 5 уровня",
				Progress: 2,
				Max: 5,
				Rewards: 600,
				Complete: false
			},
			{
				ID: 2,
				Name: "Молодость",
				Description: "Достичь 5 уровня",
				Progress: 2,
				Max: 5,
				Rewards: 600,
				Complete: false
			},
		],
		
		statistica: {
			Arrest: 0,
			Warns: 0,
			Kills: 0,
			Deaths: 0,
			MoneyEarn: 10300,
			MoneySpent: 5000,
		},
		stats: [2,3,5,"20.08.2022",35235,24555,24,"Нет прописки","Курьер Delivery Club","Не женат","Novizio","The Families","Platinum","11.02.2022"],
		transport: {
			counts: 
			[
				{
					Name: "BMW M8 Gran Coupe",
					Number: "L123OX",
					Mile: 12,
				},				
			],
			max: 6,
		},
		house: {
			have: false,
			ID: "12",
		},
		apartament: {
			have: false,
			name: "Бульвар",
		},
		business: {
			have: false,
			id: 4,
			name: "Магазин 24/7",
		},
		
		battlepass: {
			active: false,
			havepass: false,
			style: 0,
			modal: 0,
			giftID: "",
			lvl: 2,
			dayLEFT: 56,
			exp: 1860,
			buyEXP: [0,0,0],
			buyEXPprices: [900,1500,2700],
			timeEXP: 5,
			maxTimeEXP: 75,
			topPlayers: [
				{
					Name: "Ilya_Merumond",
					Lvl: "950",
				},
			],
			freelvls: {
				1: {
					items: 
					[
						{
							Name: "до 25000$",
							Taken: false,
							Picture: "money2.png", 
							Type: "money",
						}
					],
				},
				5: {
					items: 
					[
						{
							Name: "до 25000$",
							Taken: false,
							Picture: "money2.png", 
							Type: "money",
						}
					],
				},
			},
			premiumlvl: {
				2: {
					items: 
					[
						{
							Name: "до 25000$",
							Taken: true,
							Picture: "money1.png", 
							Type: "money",
						},
						{
							Name: "до 25000$",
							Taken: false,
							Picture: "money1.png", 
							Type: "money",
						},
						{
							Name: "до 25000$",
							Taken: false,
							Picture: "money1.png", 
							Type: "money",
						},
						{
							Name: "до 25000$",
							Taken: false,
							Picture: "money1.png", 
							Type: "money",
						},
					],
				},
				3: {
					items: 
					[
						{
							Name: "до 25000$",
							Taken: true,
							Picture: "money1.png", 
							Type: "money",
						}
					],
				},
			},
			
			quests: [
				{
					ID: 1,
					Name: "Шахтер",
					Description: "Добудьте руды в карьере",
					Progress: 80,
					Max: 80,
					Rewards: 600,
					Complete: false
				},
				{
					ID: 1,
					Name: "Шахтер",
					Description: "Добудьте руды в карьере",
					Progress: 2,
					Max: 2,
					Rewards: 600,
					Complete: true
				},
				{
					ID: 1,
					Name: "Шахтер",
					Description: "Добудьте руды в карьере",
					Progress: 2,
					Max: 80,
					Rewards: 600,
					Complete: false
				},
				{
					ID: 1,
					Name: "Шахтер",
					Description: "Добудьте руды в карьере",
					Progress: 2,
					Max: 80,
					Rewards: 600,
					Complete: false
				},
				{
					ID: 1,
					Name: "Шахтер",
					Description: "Добудьте руды в карьере",
					Progress: 2,
					Max: 80,
					Rewards: 600,
					Complete: false
				},
				{
					ID: 1,
					Name: "Шахтер",
					Description: "Добудьте руды в карьере",
					Progress: 2,
					Max: 80,
					Rewards: 600,
					Complete: false
				},
			],
		},
    },
	mounted: function() {
		document.addEventListener('keyup', this.keyUp);
	},
	methods: {
		keyUp: () => {
			if (PlayerMenu.battlepass.active) {
				if (event.keyCode == 27) { mp.trigger('client::battlepass:close'); mp.trigger('client::playermenu:close'); }
			}
			if (PlayerMenu.active && !PlayerMenu.battlepass.active) {
				if (event.keyCode == 27) { mp.trigger('client::battlepass:close'); mp.trigger('client::playermenu:close'); }
			}
		},
		gostyle: function(a) {
			this.style = a;
			this.modal = 0;
		},
		Open: function(a, b, c, d, e, f, g) {
			this.statistica = a;
			this.stats = b;
			this.house = c;
			this.apartament = d;
			this.business = e;
			this.transport.counts = f;
			this.transport.max = g;
		},
		Reset: function() {
			this.style = 0;
			this.modal = 0;
		},
		Exit: function() {
			mp.trigger('client::playermenu:close');
		},
		btnOpenBattlePass: function() {
			mp.trigger('client::battlepass:getrequest');
		},
		gomodal: function(a) {
			this.modal = a;
		},
        bp_gostyle: function(a) {
			this.battlepass.style = a;
		}, 
		bp_gomodal: function(a) {
			this.battlepass.modal = a;
			if (a == 0) {
				setTimeout(() => {
					this.battlepass.buyEXP = [0,0,0];
					this.battlepass.giftID = "";
				}, 500);
			}
		},
		bp_getmin: function() {
			var min = (this.battlepass.maxTimeEXP - this.battlepass.timeEXP) > 60 ? (this.battlepass.maxTimeEXP - this.battlepass.timeEXP) - parseInt((this.battlepass.maxTimeEXP - this.battlepass.timeEXP) / 60) * 60 : (this.battlepass.maxTimeEXP - this.battlepass.timeEXP);
			if (min <= 9) {
				min = "0" + min;
			}
			return min;
		},
		addToCountExp: function(list, a) {
			if (a == true)
				this.battlepass.buyEXP[list] += 1;
			else {
				if (this.battlepass.buyEXP[list] <= 0) return;
				this.battlepass.buyEXP[list] -= 1;
			}
			this.$forceUpdate();
		},
		bp_open: function(free, prem, qlist, tops, lvl, exp, buyed, timetogiveexp, pricesbuy, daytoend, maxtimeexp) {
			this.battlepass.freelvls = free;
			this.battlepass.premiumlvl = prem;
			this.battlepass.quests = qlist;
			this.battlepass.topPlayers = tops;
			this.battlepass.lvl = lvl;
			this.battlepass.exp = exp;
			this.battlepass.havepass = buyed;
			this.battlepass.timeEXP = timetogiveexp;
			this.battlepass.buyEXPprices = pricesbuy;
			this.battlepass.dayLEFT = daytoend;
			this.battlepass.maxTimeEXP = maxtimeexp;
		},
		bp_buyexp: function() {
			mp.trigger('client::battlepass:buyexp', this.battlepass.buyEXP[0], this.battlepass.buyEXP[1], this.battlepass.buyEXP[2])
		},
		bp_buypass: function(a) {
			mp.trigger('client::battlepass:buypass', a);
			this.battlepass.modal = 0;
		},
		bp_giftpass: function() {
			mp.trigger('client::battlepass:gift', parseInt(this.battlepass.giftID));
		},
		bp_takeitem: function(a,b,c) {
			// if (this.battlepass.lvl < b) return;
			// alert(a + "," + b + "," + c)
			mp.trigger('client::battlepass:takeitem', a, b, c);
		},
    }
});

const scrollContainer = document.querySelector(".battlepass_premitems_items");
const scrollContainers = document.querySelector(".battlepass_freeitems_items");
const scrollContainerss = document.querySelector(".bp_scoreline");

scrollContainer.addEventListener("wheel", (evt) => {
    evt.preventDefault();
    scrollContainer.scrollLeft += evt.deltaY;
    scrollContainers.scrollLeft += evt.deltaY;
    scrollContainerss.scrollLeft += evt.deltaY;
});

scrollContainers.addEventListener("wheel", (evt) => {
    evt.preventDefault();
    scrollContainer.scrollLeft += evt.deltaY;
    scrollContainers.scrollLeft += evt.deltaY;
    scrollContainerss.scrollLeft += evt.deltaY;
});
