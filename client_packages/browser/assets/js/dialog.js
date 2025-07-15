var dialog = new Vue({
    el: '.dialog',
    data: {
        active: false,
        title: "Вы действительно хотите приобрести авиакомпанию?",
    },
    methods: {
        yes: function () {
            mp.trigger('dialogCallback',true)
        },
        no: function () {
            mp.trigger('dialogCallback',false)
        }
    }
});

var deathMenu = new Vue({
    el: '.deathmenu',
    data: {
        active: false,
        title: "Вы действительно хотите приобрести авиакомпанию?",
        title2: "",
		time: 9,
		disableBTN: false,
    },
    methods: {
		yes: function () {
			if (this.disableBTN == true) return;
            mp.trigger('dialogCallbackMED',true)
			this.disableBTN = true;
        },
        no: function () {
			if (this.disableBTN == true) return;
            mp.trigger('dialogCallbackMED',false)
			this.disableBTN = true;
        }
    }
});