var popup = new Vue({
    el: '.intercation',
    data: {
        active: false,
		boomboxplaced: false,
    },
    mounted: function() {
		document.addEventListener('keyup', this.keyUp);
	},
    methods: {
		keyUp: () => {
			if (!popup.active) return;
			if (event.keyCode == 27) 
			{
				mp.trigger('client::toservbtnAltPopUp', 99)
			}
		},
        btn: function(id) {
			mp.trigger('client::toservbtnAltPopUp', id)
        },
    }
})