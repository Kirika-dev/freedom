var numberChanger = new Vue({
  el: '#app',
  data: {
    active: true,
	number: null,
	style: 0,
	price: 5000,
	typesmoney: ["Наличные","Карта"],
	typesindex: 0,
	showtypes: false,
	btns: [false,true,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false,false],
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
	showtypess: function() {
		if (this.showtypes) {
			this.showtypes = false;
		}
		else {
			this.showtypes = true;
		}
	},
	clickbtn: function(a) {
		this.typesindex = a;
		this.showtypes = false;
	},
	take() {
		if (this.number == null) return;
		mp.trigger("client::buynumbers", this.number.toUpperCase());
	},
	gen() {
		mp.trigger("client::randomvehnum", this.typesindex);
	},
	exit: function() {
		this.number = null;
		mp.trigger('closeChangeNumber');
	}
  }
})