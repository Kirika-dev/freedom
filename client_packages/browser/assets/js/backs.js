var backs = new Vue({
    el: '.backs',
    data: {
        active: false,
        indexS: 0,
		index: 0,
        indexC: 0,
        styles: [105, 106],
        colors: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9],
        prices: [9, 99],
        btns: [true, false, false, false],
    },
    methods: {
        left: function (type) {
            if (type == 0) {
                this.indexS--
                if (this.indexS < 0) this.indexS = this.styles.length - 1
                mp.trigger('client::bag:change', 'style', this.indexS)
            } else {
                this.indexC--
                if (this.indexC < 0) this.indexC = this.colors.length - 1
                mp.trigger('client::bag:change', 'color', this.indexC)
            }
        },
        right: function (type) {
            if (type == 0) { 
                this.indexS++
                if (this.indexS == this.styles.length) this.indexS = 0
                mp.trigger('client::bag:change', 'style', this.indexS)
            } else {
                this.indexC++
                if (this.indexC == this.colors.length) this.indexC = 0
                mp.trigger('client::bag:change', 'color', this.indexC)
            }
        },
		getNameClothesR: function(c) {
			return getNameClothes(BagsNames, c);
			// return "";
		},
        buy: function (a) {
            mp.trigger('client::bag:buy')
        },
        exit: function () {
            this.reset()
            mp.trigger('client::bag:close')
        },
        reset: function () {
            this.price = -1
            this.indexS = -1
            this.indexC = 0
            this.styles = []
            this.colors = []
            this.prices = []
        },
        btn: function (id) {
            this.indexS = id;
            this.indexC = 0;
            mp.trigger('client::bag:change', 'style', this.indexS);
        },
    }
})

function backsSearch() {
    let input = document.getElementById("backsSearch");
    let filter = input.value.toUpperCase();
    let li = document.getElementsByClassName("backs_listitems__block");
  
    for (let i = 0; i < li.length; i++) {
        let a = li[i].getElementsByClassName("backs_listitems__block__name")[0];
        if (a.innerHTML.toUpperCase().indexOf(filter) > -1) {
          li[i].style.display = "";
        } else {
          li[i].style.display = "none";
        }
    }
};