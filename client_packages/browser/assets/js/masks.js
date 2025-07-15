var masks = new Vue({
    el: '.masks',
    data: {
        active: false,
        indexM: -1,
		indexC: 0,
        styles: [1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23],
        colors: ["Colors-1", "Colors-2"],
        prices: [9, 99, 99, 99, 99, 34, 99, 265, 99, 78, 99, 99, 99, 99, 90109, 912019, 54646, 87354, 535, 555, 444, 2312, 120000],
    },
	mounted: function() {
		document.addEventListener('keyup', this.keyUp);
		document.addEventListener('keydown', this.keyDown);
	},
    methods: {
		keyDown: () => {
            if (!masks.active) return
			if (event.keyCode == 27) 
			{				
				masks.pressButton(document.querySelector('.logoblocks_btn'), true);
			}
        },
        keyUp: () => {
            if (!masks.active) return
            if (event.keyCode == 27) 
			{				
				masks.pressButton(document.querySelector('.logoblocks_btn'), false);
				masks.exit();
			}
		},
		getNameClothesM: function(c) {
			return getNameClothes(MasksNames, c);
		},
		select: function(a) {
			this.indexC = 0;
			this.indexM = a;
			mp.trigger('masks', 'style', a);
		},
        left: function () {
			this.indexC--
			if (this.indexC < 0) this.indexC = this.colors.length - 1
			mp.trigger('masks', 'color', this.indexC)
        },
        right: function () {
			this.indexC++
			if (this.indexC == this.colors.length) this.indexC = 0
			mp.trigger('masks', 'color', this.indexC)
        },
		buy: function (a) {
            mp.trigger('buyMasks', a)
        },
		reset: function () {
			this.active = false;
            this.indexM = -1;
            this.indexC = 0;
            this.styles = [];
            this.colors = [];
            this.prices = [];
        },
		pressButton: function(pressButtonContainer, pressed) {
            let className = 'active';
            if (pressed) pressButtonContainer.classList.add(className)
            else pressButtonContainer.classList.remove(className)
        },
		exit: function () {
            this.reset();
            mp.trigger('closeMasks');
        },
    }
})