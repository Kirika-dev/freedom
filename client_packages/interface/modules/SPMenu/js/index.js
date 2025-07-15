
var app = new Vue({
    el: "#app",
    data: {
		active: false,
		data: [],
    },
	 mounted: function() {
		document.addEventListener('keyup', this.keyUp);
		document.addEventListener('keydown', this.keyDown);
	},
    methods: {
		keyDown: () => {
			if (!app.active) return;
			if (event.keyCode == 27) app.pressButton(document.querySelector('.btnesc'), true);
		},
		keyUp: () => {
			if (!app.active) return;
			switch(event.keyCode)
			{
				case 27:
					app.pressButton(document.querySelector('.btnesc'), false);
					app.exit();
				break;
				case 37:
					mp.trigger('Client:SpectateSelect', 0)
				break;
				case 39:
					mp.trigger('Client:SpectateSelect', 1)
				break;
				case 82:
					app.pressButton(document.querySelector('.reload'), false);
					mp.trigger('Client:Refresh')
				break;
			}
		},
		pressButton: function(pressButtonContainer, pressed) {
			let className = 'active';
			if (pressed) pressButtonContainer.classList.add(className)
			else pressButtonContainer.classList.remove(className)
		},
		open: function(data) {
			this.data = data;
			this.active = true;
		},
		reload: function() {
			mp.trigger('Client:Refresh')
		},
		exit: function() {
			this.active = false;
			mp.trigger('Client:UnSpectate')
		},
    }
});