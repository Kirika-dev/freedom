var casinoBar = new Vue({
  el: '#app',
  data: {
	active: false,
    buyitems: [
		{
			Price: 100,
			ID: 9,
			Name: "",
			Ordered: false,
		},
		{
			Price: 100,
			ID: 9,
			Name: "",
			Ordered: false,
		},
		{
			Price: 100,
			ID: 9,
			Name: "",
			Ordered: false,
		},
		{
			Price: 100,
			ID: 9,
			Name: "",
			Ordered: false,
		},
		{
			Price: 100,
			ID: 9,
			Name: "",
			Ordered: false,
		},
	],
	indexp: 0,
  },
  methods: {
	buy(indexp) {
		mp.trigger("client::casino:bar:buy", indexp, 1);
	},
    closeMenu() {
		this.active = false
		mp.trigger("client::casino:bar:close")
    },
	btn: function(id){
		this.indexp=id;
		this.$forceUpdate();
    },
	selectprod: function(index){
		this.indexp=index
	},
  }
})
