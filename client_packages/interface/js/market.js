var market = new Vue({
    el: ".market_menu",
    data: {
        active: false,
        itemsbuy: [],
		itemssell: [],
		itemsorders: [],
		header: "Форза Флеееекс",
		description: "Починка великов",
		page: 0,
		typeMarket: 0,
		
		types: 
		{
			money: ["$", "MC", "мат."]
		}
    },
    mounted: function() {
		document.addEventListener('keyup', this.keyUp);
	},
    methods: {
		keyUp: () => {
			if (!market.active) return;
			if (event.keyCode == 27) market.exit()
		},
		Open: function(a, b, c, d, e, f) {
			this.typeMarket = a;
			this.header = b;
			this.description = c;
			this.itemsbuy = d;
			this.itemssell = e;
			this.itemsorders = f;
			if (this.itemsbuy.length == 0) {
				if (this.itemssell.length == 0) {
					this.page = 2;
				}
				else {
					this.page = 1;
				}
			}
			else {
				this.page = 0;
			}
		},
		exit: function() {
			this.active = false;
			mp.trigger('client::market:close');
		},
		gopage: function(a) {
			switch (a) {
				case 0:
					if (this.itemsbuy.length == 0) return;
					break;
				case 1:
					if (this.itemssell.length == 0) return;
					break;
				case 2:
					if (this.itemsorders.length == 0) return;
					break;
			}
			this.page = a;
		},
		getTypePage() {
			var result = "";
			switch (this.page) {
				case 0:
					result = this.itemsbuy;
					break;
				case 1:
					result = this.itemssell;
					break;
				case 2:
					result = this.itemsorders;
					break;
			}
			return result
		},
		btn: function(item) {
			switch (this.page) {
				case 0:
					mp.trigger('client::market:buy', item, this.typeMarket);
					break;
				case 1:
					mp.trigger('client::market:sell', item, this.typeMarket);
					break;
				case 2:
					mp.trigger('client::market:order', item, this.typeMarket);
					break;
			}
		},
    }
})