var HMMenu = new Vue({
    el: ".housemenu",
    data: {
		active: false,
		owner: false,

		houseid: 14,
		houseowner: "Государство",
		price: 500100,
		garage: 6,
		roomates: 1,
		type: 2,
		locked: false,
		
		listtypes: ["Трейлер","Эконом","Эконом+","Комфорт","Комфорт+","Премиум","Премиум+","Элитный"],
	},
	mounted: function() {
		document.addEventListener('keyup', this.keyUp);
	},
	methods: {
		keyUp: () => {
			if (!HMMenu.active) return;
			if (event.keyCode == 27) HMMenu.exit();
		},
		Open: function(id, owner, price, garage, roomates, type, lock, player) {
			this.houseid = id;
			this.houseowner = owner;
			this.price = price;
			this.garage = garage;
			this.roomates = roomates;
			this.type = type;
			this.locked = lock;
			this.owner = player;
			this.$forceUpdate();
		},
		interaction: function(a) {
			mp.trigger('client::house:interaction', a);
			this.exit();
		},
		exit: function() {
			mp.trigger('client::house:exit');
		}
	}
});

var PostalHouse = new Vue({
    el: ".postalhouse",
    data: {
		active: false,
		list: [
			{
				Sender: "Delivery Club",
				Name: "Бургер х4",
				Date: "02.06.2022",
				Item: {
					ID: 6,
					Type: 6,
					Count: 4,
					Data: [],
					IsActive: false,
					Wear: 100,
					subData: [],
					FastSlots: -1,
				},
				Time: 0,
			},
		],
		selected: -1,
	},
	mounted: function() {
		document.addEventListener('keyup', this.keyUp);
	},
	methods: {
		keyUp: () => {
			if (!PostalHouse.active) return;
			if (event.keyCode == 27) PostalHouse.exit();
		},
		Open: function(list) {
			this.list = list;
			this.$forceUpdate();
		},
		selectItem: function(a) {
			if (this.selected == a) {
				this.selected = -1;
				return;
			}
			this.selected = a;
		},
		takeItem: function() {
			if (this.selected == -1) return;
			mp.trigger('client::house::postal:take', this.selected);
		},
		exit: function() {
			mp.trigger('client::house::postal:exit');
		}
	}
});