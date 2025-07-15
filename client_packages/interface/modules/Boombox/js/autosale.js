var boombox = new Vue({
    el: ".main",
    data: {
	active: false,
	menu: 0,
	style: 0,
	input: null,
    },
    methods:{
        gostyle: function(index) {
            this.style = index;
        },
		btn: function(){
			if(this.input.endsWith(".mp3")){
				//mp.trigger('client::addaudioonBoomBox', this.input)
			}
			else{
				mp.trigger('notify', 1, 4, "Неправильная ссылка. Введите ссылку с .mp3 в конце.", 3000);
			}
        },
		btnreset: function() {
			mp.trigger('client::addaudioonBoomBox', " ")
		},
		closemenu: function() {
			this.input = null;
			mp.trigger('client::closeboombox');
		},
    }
});