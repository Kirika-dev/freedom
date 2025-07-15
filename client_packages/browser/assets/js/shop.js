var shop = new Vue({
    el: '.shop_menu',
    data: {
        active: false,
		header: "Магазин",
		street: "Альта мерумонд",
		selectedIndex: 0,
		products: 
		[
			[
				[0,"еКола",9,5000],
				[1,"Спрунк",10,5000],
			],
			[
				[2,"Коробка пиццы",5,5000],
				[3,"Бургер",6,5000],
				[4,"Пачка чипсов",3,12000],
				[5,"Пиво",4,12000],
				[5,"Пиво",22,12000],
				[5,"Пиво",25,12000],
				[5,"Пиво",27,12000],
				[5,"Пиво",31,12000],
			],
			[
				[2,"Коробка пиццы",5,5000],
				[2,"Бургер",6,5000],
			],
			[
				[2,"Коробка пиццы",5,5000],
				[2,"Бургер",6,5000],
			],
		],
		categories: 
		[
			"Вода","Еда","Инструменты","Разное"
		],
		basket: [],
		moneyall: 0,
    },
    mounted: function() {
		document.addEventListener('keyup', this.keyUp);
	},
	methods: {
		keyUp: () => {
			if (!shop.active) return;
			if (event.keyCode == 27) shop.close();
		},
		open: function(header, street, cats, items) {
			this.header = header;
			this.street = street;
			this.categories = cats;
			this.products = items;
			this.$forceUpdate();
		},
        selectCat: function(a) {
			this.selectedIndex = a;
			this.$forceUpdate();
		},
		nextCat: function() {
			if (this.selectedIndex == this.categories.length - 1) {
				this.selectedIndex = 0;
			}
			else if (this.selectedIndex < this.categories.length){
				this.selectedIndex++;
			}
			this.$forceUpdate();
		},
		getAllPriceBasket: function() {
			var money = 0;
			for (var i = 0; i < this.basket.length; i++) {
				money += this.basket[i][3] * this.basket[i][4];
			}
			this.moneyall = money
			this.$forceUpdate();
		},
		checkItemInBasket: function(index) {
            for (var i = 0; i < this.basket.length; i++) {
				if (this.basket[i][0] == index) 
					return i;
			}
            return -1;
        },
		PlusMinusBasketItem: function(index, a) {
			if (a > 0) {
				if (this.basket[index][4] >= 15) return;
				this.basket[index][4] += 1;
			}
			else {
				if (this.basket[index][4] == 1) {
					this.basket.splice(index, 1);
					this.getAllPriceBasket();
					this.$forceUpdate();
					return;
				}
				this.basket[index][4] -= 1;
			}
			this.getAllPriceBasket();
			this.$forceUpdate();
		},
		addToBasket: function(item) {
			if (this.checkItemInBasket(item[0]) != -1) return;
			item.push(1);
			this.basket.push(item)
			this.getAllPriceBasket();
		},
		buy: function() {
			// alert(this.basket)
			if (this.basket.length == 0) return;
			mp.trigger("client::shop:buy", JSON.stringify(this.basket));
			this.close();
		},
		close: function() {
			this.basket = [];
			this.active = false;
			this.moneyall = 0;
			this.selectedIndex = 0;
			mp.trigger('client::shop:close');
		},
    }
})