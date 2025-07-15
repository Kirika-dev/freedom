var clothes = new Vue({
    el: '.clothes',
    data:{
        active: false,
        index: 0,
        indexS: -1,
        indexC: 0,
        styles: [400,11],
		names: ["Толстовка","Джоггеры"],
        colors: [1,2,3,4,5,6,7,8,9,10],
        prices: [9,99],
        btns: [true,false,false,false],
		catId: -1,
		typeBiz: 1,
		gender: true,
		category: ["Головные уборы", "Верхняя одежда", "Майки", "Низ", "Обувь", "Перчатки", "Часы", "Очки", "Украшения"]
    },
    methods: {
        left: function(type){
            if(type==0){ 
                this.indexS--
                if(this.indexS < 0) this.indexS = this.styles.length - 1
                mp.trigger('clothes','style',this.indexS)
            } else {
                this.indexC--
                if(this.indexC < 0) this.indexC = this.colors.length - 1
                mp.trigger('clothes','color',this.indexC)
            }
        },
		getNameClothesR: function(c) {
			if (this.getCatId() == 0) {
				return "Комплект #" + Number(c.split('_')[0]);
			}
			return getNameClothes(this.getCatId(), c);
		},
        right: function(type){
            if(type==0){
                this.indexS++
                if(this.indexS == this.styles.length) this.indexS = 0
                mp.trigger('clothes','style',this.indexS)
            } else { 
                this.indexC++
                if(this.indexC == this.colors.length) this.indexC = 0
                mp.trigger('clothes','color',this.indexC)
            }
        },
		getCatId: function() {
			switch(this.catId) {
				default: 
					return 0;
				case 0:
					return this.gender ? MaleHats : FemaleHats;
				case 1:
					return this.gender ? MaleTops : FemaleTops;
				case 2:
					return this.gender ? MaleTops : FemaleTops;
				case 3:
					return this.gender ? MaleLegs : FemaleLegs;
				case 4:
					return this.gender ? MaleShoes : FemaleShoes;
				case 5:
					return this.gender ? MaleGloves : FemaleGloves;
				case 6:
					return this.gender ? MaleWatches : FemaleWatches;
				case 7:
					return this.gender ? MaleGlasses : FemaleGlasses;
				case 8:
					return this.gender ? MaleAccessoires : FemaleAccessoires;
			};
		},
        buy: function(){
            mp.trigger('buyClothes')
        },
        exit: function(){
            this.reset()
            mp.trigger('closeClothes')
        },
        reset: function(){
            this.price=-1
            this.indexS=-1
            this.indexC=0
            this.styles=[]
            this.colors=[]
            this.prices=[]
        },
        btn: function(id){
            this.catId = id;
			this.indexS=-1;
			if (id != -1)
            mp.trigger('clothes', 'cat', this.catId);
        },
		btnclothes: function(id){
            this.indexS = id;
			this.indexC=0
            mp.trigger('clothes', 'style', Number(this.styles[id]));
        },
    }
})

function getNameClothes(type, a) {
	var result = type[Number(a.split('_')[0])] != null ? type[Number(a.split('_')[0])][Number(a.split('_')[1])] != null ? type[Number(a.split('_')[0])][Number(a.split('_')[1])].Name_RU : null : null;
	if (result != undefined && result != "NULL")
		return result;
	else
		return "Комплект #" + Number(a.split('_')[0]);
};

function clothesSearch() {
    let input = document.getElementById("clothesSearch");
    let filter = input.value.toUpperCase();
    let li = document.getElementsByClassName("clothes_listitems__block");
  
    for (let i = 0; i < li.length; i++) {
        let a = li[i].getElementsByClassName("clothes_listitems__block__name")[0];
        if (a.innerHTML.toUpperCase().indexOf(filter) > -1) {
          li[i].style.display = "";
        } else {
          li[i].style.display = "none";
        }
    }
};