var petrol = new Vue({
    el: ".petrol",
    data: {
        active: false,
        input: 0,
		price: 12,
		type: 0,
		fuel: 0,
		maxfuel: 120,
		move: 120,
		interval: null,
		load: {
			active: false,
		}
    },
    mounted: function() {
		document.addEventListener('keyup', this.keyUp);
	},
	methods: {
		keyUp: () => {
			if (!petrol.active) return;
			if (event.keyCode == 27) mp.trigger('client::petrol:close');
		},
		buy: function() {
			mp.trigger('client::petrol:buy', this.input, this.type);
		},
		buyOther: function(a) {
			mp.trigger('client::petrol:buyother', a);
		},
		movefuel: function (index){
            let value = 0;
            if (this.fuel > index)
                value = -1;
            else
                value = 1;
            this.fuel = index;
            if (this.interval != null)
                clearInterval(this.interval);
			setTimeout(() => {
				this.interval = setInterval(() => {
					if (this.fuel == this.move) {
						clearInterval(this.interval);
						mp.trigger('client::soundplayPetrol', './sounds/stopPetrol.mp3', 1);
						setTimeout(() => {
							mp.trigger('client::petrol:endload');
						}, 1000);
					}
					else
						this.move += value;
				}, 150);
			}, 500);

        },
		getMultiplayer: function() {
			switch(this.type) {
				case 0:
					return 1;
				case 1:
					return 1.65;
				case 2:
					return 0.78;
				default: 
					return 1;
			}
		},
		selectType: function(a) {
			this.type = a;
		},
		getNum: function(a) {
			var result = 0;
			var end = ((this.price * this.input * this.getMultiplayer()).toFixed(0) + "").split("").reverse().join("");			
			if (end.toString().length > 5 - a) {
				result = end.toString()[5 - a];
			}
			return result;
		},
    }
});
    