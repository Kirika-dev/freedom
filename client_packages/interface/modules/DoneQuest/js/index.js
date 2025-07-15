var menu = new Vue({
    el: "#app",
    data: {
		active: false,
		style: 0,
		namequest: "Первое знакомство",
		descquest: "Сюжетное",
		money: 150,
		exp: 1,
		itemsbonus: [11,0,0,0],
		itemsbonustwo: [0,0]
    },
	mounted: function() {
		// if (this.active == true) {
			document.addEventListener('keyup', this.keyUp);
		// }
	},
    methods: {
		keyUp(event) {
		  if(event.keyCode == 13) this.exit();
		},
		set: function(state, namequest, descquest, money, exp, itemsbonus, itemsbonustwo) {
			this.active = state;
			this.namequest = namequest;
			this.descquest = descquest;
			this.money = money;
			this.exp = exp;
			this.itemsbonus = itemsbonus;
			this.itemsbonustwo = itemsbonustwo;
		},
		exit: function() {
			this.active = false;
			mp.trigger('client::closedonemenuquest');
		},
    }
});