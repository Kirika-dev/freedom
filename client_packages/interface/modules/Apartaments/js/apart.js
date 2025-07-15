var aparts = new Vue({
    el: "#app",
    data: {
        active: false,
		id: 2,
		name: "4 Intergity way",
		apartlist: [ [ 1, "Государство", "50000", 3, "0/3" ],[ 1, "", "50000", 3, "0/3" ] ],
		selectapart: null,
		modal: false,
    },
    methods: {
		hide: function () {
			mp.trigger('client::closeapart');
        },
		hides: function () {
			this.active = false;
			this.modal = false;
			this.apartlist = []; // optimize
        },
		openmodal: function(a) {
			this.selectapart = a;
			this.modal = true;
		},
        show: function (data, name, id) {
			this.active = true;
			this.name = name;
			this.id = id
			this.apartlist = data;
        },
		interact: function(index) {
			mp.trigger('client::sendapart', index);
		}
    }
})