var numberChanger = new Vue({
  el: '#app',
  data: {
    active: true,
	numbers: [ ],
	num: null,
	oldnum: null,
	btns: [false,false,false,false,false,false,false,false,false,false,false,false],
  },
  mounted: function() {
		document.addEventListener('keyup', this.keyUp);
		document.addEventListener('keydown', this.keyDown);
	},
  methods: {
	keyDown: () => {
		if (event.keyCode == 27) numberChanger.pressButton(document.querySelector('.btnesc'), true);
	},
	keyUp: () => {
		if (event.keyCode == 27) numberChanger.pressButton(document.querySelector('.btnesc'), false);
		if (event.keyCode == 27) numberChanger.exit();
	},
	pressButton: function(pressButtonContainer, pressed) {
		let className = 'active';
		if (pressed) pressButtonContainer.classList.add(className)
		else pressButtonContainer.classList.remove(className)
	},
	gostyle: function(index) {
		this.style = index;
		this.number = null;
	},
	change: function() {
		if (this.num == this.oldnum) return;
		mp.trigger('client::changevehnummenuChangenum', this.num);
		this.exit();
	},
	btn: function(a) {
		this.num = this.numbers[a];
		let ind = this.btns.indexOf(true);
		if (ind > -1) this.btns[ind] = false;
		this.btns[a] = true;
		this.active=false;
		this.active=true;
	},
	exit: function() {
		this.numbers = null;
		this.oldnum = null;
		this.num = null;
		mp.trigger('client::closemenuchangenumveh');
	}
  }
})