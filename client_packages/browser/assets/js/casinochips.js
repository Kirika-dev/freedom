var CasinoChips = new Vue({
    el: '.casinochips',
    data: {
        active: false,
        buyC: 100,
        sellC: 90,

        sellValue: '',
    },
    mounted: function() {
		document.addEventListener('keyup', this.keyUp);
	},
	methods: {
		keyUp: () => {
            if (!CasinoChips.active) return;
            if (event.keyCode == 27) CasinoChips.Close();
        },
        Open(a,b) {
            this.buyC = Number(a);
            this.sellC = Number(b);

            this.active = true;
            // this.$forceUpdate();
        },
        Buy(a) {
            mp.trigger("client::casino::chips:buy", Number(a));
        },
        Sell() {
            mp.trigger('client::casino::chips:sell', Number(this.sellValue));
        },
        Close() {
            mp.trigger('client::casino::chips:close');
        },
        Reset() {
            this.active = false;
            this.sellValue = '';
        }
    }
});