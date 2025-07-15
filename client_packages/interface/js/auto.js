var auto = new Vue({
    el: ".autoshop",
    data: {
        active: false,
		selectFilter: -1,
		AllMarksAuto: ["audi","bentley","bmw","bugatti","chevrolet","dodge","ferrari","ford","infiniti","jaguar","jeep","kia","lada","lamboragini","lexus","mercedes","mustang","nissan","porsche","rolls","romeo","rover","skoda","toyota","tesla","volkswagen"],
        models: ["Audi R8"],
        colors: ["Red","Red","Red","Red","Red"],
        prices: [12],
		allShowMarksAuto: [],
		indexM: 0,
		indexC: 0,
		
		header: "",
		cur: "",
		
		numpas: 25,
		
		sortState: false,
		allpr: [],
		
		modal: false,
		
		maxCars: 0,
		indCars: 1,
		getModelDesc(model) {
            return getModelDescript[model.toLowerCase()] || null;
        }
    },
	mounted: function() {
		document.addEventListener('keyup', this.keyUp);
		document.addEventListener('keydown', this.keyDown);
	},
    methods: {
		open: function() {
			// this.reset()
			this.active = true;
			for (var i = 0; i < this.models.length; i++) {
				var result = [];
				result.push(this.models[i])
				result.push(this.prices[i])
				this.allpr.push(result)
			}
			
			for (var i = 0; i < this.allpr.length; i++) {
				for (var a = 0; a < this.AllMarksAuto.length; a++)
				{
					if (this.allpr[i][0].toLowerCase().includes(this.AllMarksAuto[a].toLowerCase()))
						if (!this.allShowMarksAuto.find(x => x == this.AllMarksAuto[a]))
							this.allShowMarksAuto.push(this.AllMarksAuto[a].toLowerCase())
				}
			}
			
			for (var i = 0; i < this.AllMarksAuto.length; i++) {
				for (var b = 0; b < this.allShowMarksAuto.length; b++) {
					if (this.AllMarksAuto[i] == this.allShowMarksAuto[b]) {
						this.selectFilter = i;
						return;
					}
				}
			}
		},
		keyDown: () => {
			if (!auto.active) return;
			// if (event.keyCode == 27) auto.pressButton(document.querySelector('.auto_btnesc'), true);
			// if (event.keyCode == 13) auto.pressButton(document.querySelector('.auto_btnenter'), true);
			// if (event.keyCode == 38) auto.pressButton(document.querySelector('.auto_btnarrow1'), true);
			// if (event.keyCode == 40) auto.pressButton(document.querySelector('.auto_btnarrow2'), true);
		},
		keyUp: () => {
			if (!auto.active) return;
			// if (event.keyCode == 27) auto.pressButton(document.querySelector('.auto_btnesc'), false);
			// if (event.keyCode == 13) auto.pressButton(document.querySelector('.auto_btnenter'), false);
			// if (event.keyCode == 38) auto.pressButton(document.querySelector('.auto_btnarrow1'), false);
			// if (event.keyCode == 40) auto.pressButton(document.querySelector('.auto_btnarrow2'), false);
			if (event.keyCode == 27) auto.exit();
			if (event.keyCode == 13) auto.openBuyMenu();
			if (event.keyCode == 39) auto.goNextColor(1);
			if (event.keyCode == 37) auto.goNextColor(-1);
		},
		selectMark: function(a) {
			this.selectFilter = a;
			this.getAllModelsInCat();
		},
		openBuyMenu: function() {
			this.modal = true;
		},
		modalClose: function() {
			this.modal = false;
		},
		getThisModelInCar: function() {
			this.indCars = 0;
			var result = 0;
			for(var i = 0; i < this.allpr.length; i++) {			
				if (this.allpr[i][0].toLowerCase().includes(this.AllMarksAuto[this.selectFilter].toLowerCase())) {
					result+=1;
					if (this.allpr[i][0].toLowerCase() == this.allpr[this.indexM][0].toLowerCase()) {
						this.indCars = result;
						return;
					}
				}
			}
		},
		getAllModelsInCat: function() {
			this.maxCars = 0;
			for(var i = 0; i < this.allpr.length; i++) {			
				if (this.allpr[i][0].toLowerCase().includes(this.AllMarksAuto[this.selectFilter].toLowerCase())) {
					this.maxCars += 1;
				}
			}
			
			for(var i = 0; i < this.allpr.length; i++) {			
				if (this.allpr[i][0].toLowerCase().includes(this.AllMarksAuto[this.selectFilter].toLowerCase())) {
					this.selectCar(i);
					return;
				}
			}
		},
		changeSort: function() {
			if (this.sortState)
				this.reverse();
			else 
				this.sort();
		},
		sort: function() {
			this.allpr.sort(( a, b ) => a[1] - b[1]);
			this.sortState = true;
			this.getAllModelsInCat();
		},
		reverse: function() {
			this.allpr.reverse(( a, b ) => a[1] - b[1]);
			this.sortState = false;
			this.getAllModelsInCat();
		},
		selectColor: function(a) {
			this.indexC = a;
			mp.trigger('auto','color', this.indexC);
		},
		goNextColor(a = 0) {
            if (a == 0 || this.modal) return;
            if (a > 0) {
                if (this.indexC == this.colors.length - 1) this.indexC = 0;
                else this.indexC++;    
            }
            else {
                if (this.indexC <= 0) this.indexC = this.colors.length - 1
                else this.indexC--;
            }
            mp.trigger('auto', 'color', this.indexC);
        },		
		selectCar: function(a) {
			this.indexM = a;
			this.getThisModelInCar();
			mp.trigger('auto','model', this.indexM)
		},
		pressButton: function(pressButtonContainer, pressed) {
			let className = 'active';
			if (pressed) pressButtonContainer.classList.add(className)
			else pressButtonContainer.classList.remove(className)
		},
		testdrive: function() {
			this.reset()
			mp.trigger('testAuto', false);
		},
		buy: function(){
			mp.trigger('buyAuto', false)
        },
		exit: function(){
			if (this.modal == true) {
				this.modal = false;
				return;
			}
            this.reset()
            mp.trigger('closeAuto')
        },
        reset: function(){
			this.active = false;
			this.modal = false;
            this.indexM=0;
            this.indexC=0;
            this.allpr=[];
            this.allShowMarksAuto=[];
            this.models=[];
            this.colors=[];
            this.prices=[];
			this.$forceUpdate();
        }
    }
})
// auto.open();
// auto.getAllModelsInCat();