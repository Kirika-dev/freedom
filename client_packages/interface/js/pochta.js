var PostalMenu = new Vue({
    el: '.pochta',
    data:{
        active: false,
        indexSelect: -1,
        parcels: [
            {"Sender":"Администрация","Name":"Награда за время","Date":"24.4.2022"}
        ]
    },
    mounted: function() {
		document.addEventListener('keyup', this.keyUp);
		document.addEventListener('keydown', this.keyDown);
	},
    methods: {
        keyDown: () => {
            if (!PostalMenu.active) return
            if (event.keyCode == 27) PostalMenu.pressButton(document.querySelector('.btn_esc'), true);
            if (event.keyCode == 13) PostalMenu.pressButton(document.querySelector('.btn_enter'), true);
        },
        keyUp: () => {
            if (!PostalMenu.active) return
            if (event.keyCode == 27) PostalMenu.pressButton(document.querySelector('.btn_esc'), false);
            if (event.keyCode == 27) PostalMenu.exit();
            if (event.keyCode == 13) PostalMenu.pressButton(document.querySelector('.btn_enter'), false);
            if (event.keyCode == 13) PostalMenu.take();
        },
        pressButton: function(pressButtonContainer, pressed) {
            let className = 'active';
            if (pressed) pressButtonContainer.classList.add(className)
            else pressButtonContainer.classList.remove(className)
        },
        btn: function(id) {
            this.indexSelect = id;
        },
        take: function() {
            if (this.indexSelect == -1) return;
            mp.trigger('client::postalmenu:take', this.indexSelect)
        },
        exit: function() {
            mp.trigger('client::postalmenu:close');
        },
        reset: function() {
            this.active = false;
            this.indexSelect = -1;  
        },
    }

});

var Metro = new Vue({
    el: '#metro',
    data:{
		active: false,
		statinons: ["Не выбран","1. LSIA Terminal","2. LSIA Parking","3. Puerto Del Sol","4. Strawberry","6. Burton","7. Portola Drive","8. Del Perro","9. Little Seoul","10. Pillbox South","11. Davis","5. Pillbox North"], 
		thisStation: 3,
		selectStation: 0,
		price: 50,
	},
	mounted: function() {
		document.addEventListener('keyup', this.keyUp);
	},
	methods: {
		keyUp: () => {
            if (!Metro.active) return
            if (event.keyCode == 27) Metro.exit();
        },
		open: function(b,a) {
			this.thisStation = a;
			this.price = b;
			this.active = true;
		},
		btnselect: function(a) {
			if (this.selectStation == a || this.thisStation == a) return;
			this.selectStation = a;
		},
		buy: function() {
			if (this.selectStation == 0) return;
			mp.trigger('metro::buy_ticket_trigger', this.selectStation);
		},
		reset: function() {
			this.active = false;
			this.thisStation = 0;
			this.selectStation = 0;
		},
		exit: function() {
			mp.trigger('metro::close_menu');
		}
	},
});

var logo = new Vue({
    el: '.logoblocks',
    data:{
        active: false,
		hint: false,
		modal: 0,
		allText: [
			"",
			"Дорогой игрок, ты попал на Freedom Project",
			"Freedom - Штат свободы, тут ты можешь быть тем, кем захочешь",
			"Стань обычным рабочим и потом езди на Mercedes ",
			"Или же стань шефом Полиции и имей связи в каждой точке штата",
			"Встань на черную сторону вступив в банду",
			"Продавай оружия и торгуй наркотой в мафии",
			"Купи бизнес чтобы стать предпринимателем и получать миллионы долларов",
			"Скупай тачки по дешевой цене и продавай их дороже",
			"Играй в BlackJack вместе с друзьями в Diamond Casino",
			"Участвуй в мероприятиях или турнирах и создавай свои",
			"Здесь ты можешь быть кем угодно",
			""
		],
		textIndex: 0,
    },
	mounted: function() {
		document.addEventListener('keyup', this.keyUp);
		document.addEventListener('keydown', this.keyDown);
	},
	methods: {
		keyDown: () => {
            if (!logo.hint) return
			if (event.keyCode == 32) 
			{				
				logo.pressButton(document.querySelector('.logoblocks_btn'), true);
			}
        },
        keyUp: () => {
            if (!logo.hint) return
            if (event.keyCode == 32) 
			{				
				logo.pressButton(document.querySelector('.logoblocks_btn'), false);
			}
		},
		addTextIndex: function() {
			if (this.textIndex == 12) return;
			this.textIndex += 1;
		},
		openHint: function() {
			this.hint = true;
			this.textIndex = 0;
		},
		pressButton: function(pressButtonContainer, pressed) {
            let className = 'active';
            if (pressed) pressButtonContainer.classList.add(className)
            else pressButtonContainer.classList.remove(className)
        },
	},
});

var PostCard = new Vue({
    el: '.postmenu',
    data:{
        active: false,
        input: "",
        sender: "",
		see: false,
		stylebg: 1,
		opened: {
			from: "",
			to: "",
		}
    },
    mounted: function() {
		document.addEventListener('keyup', this.keyUp);
		document.addEventListener('keydown', this.keyDown);
	},
    methods: {
		open: function(a, b, c, d, e) {
			this.input = a;
			this.opened.from = b;
			this.opened.to = c;
			this.stylebg = d;
			this.see = e;
		},
        keyDown: () => {
            if (!PostCard.active) return
        },
        keyUp: () => {
            if (!PostCard.active) return
            if (event.keyCode == 27) PostCard.exit();
        },
        pressButton: function(pressButtonContainer, pressed) {
            let className = 'active';
            if (pressed) pressButtonContainer.classList.add(className)
            else pressButtonContainer.classList.remove(className)
        },
		done: function() {
			if (this.input.length > 1)
			mp.trigger("client::postal:card", this.input, this.sender, this.stylebg);
		},
		exit: function() {
			this.active = false;
			this.sender = null;
			mp.trigger('client::postal:cardclose');
		},
    }

});