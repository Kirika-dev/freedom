var DialogMenu = new Vue({
    el: ".dialognpc",
    data: {
		active: false,
		style: 0,
		textdialog: "",
		header: "Чарли и Берии",
		descheader: "Два бомжа",
		answers: [],
    },
	mounted: function() {
		document.addEventListener('keyup', this.keyUp);
		document.addEventListener('keydown', this.keyDown);
	},
    methods: {
        keyDown: () => {
            if (!DialogMenu.active) return
        },
        keyUp: () => {
            if (!DialogMenu.active) return
            if (event.keyCode == 27) DialogMenu.exit();
        },
        gostyle: function(index) {
            this.style = index;
        },
		set: function(state, header, descheader, txt, ans) {
			this.active = state;
			this.header = header;
			this.descheader = descheader;
			this.textdialog = txt;
			this.answers = ans;
		},
		closemenu: function(state) {
			this.active = false;
		},
		exit: function() {
			mp.trigger('client::closedialog');
		},
		goclient: function(name) {
			mp.trigger("client::dialoganswer", name);
		},
    }
});