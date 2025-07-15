var BusinessInfo = new Vue({
    el: '.bussinesinfo',
    data:{
        active: false,
		page: 0,
		owner: false,
		data: ["Магазин 24/7",81,"Государство", 200000, 20,"Russian Mafia", 100, 1000, 888],
    },
    mounted: function() {
		document.addEventListener('keyup', this.keyUp);
	},
    methods: {
        keyUp: () => {
            if (!BusinessInfo.active) return
            if (event.keyCode == 27) BusinessInfo.exit();
        },
		gostyle: function(a) {
			this.page = a;
		},
		buy: function() {
			mp.trigger('client::biz:info:buy');
		},
		sell: function() {
			mp.trigger('client::biz:info:sell');
		},
		exit: function() {
			this.active = false;
			this.page = 0;
			mp.trigger('client::biz:info:close');
		},
    }
});