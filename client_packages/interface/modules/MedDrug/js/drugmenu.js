var drugMenu = new Vue({
    el: '.drugmenu',
    data:{
        active: true,
        page: 0,
    },
    mounted: function() {
		document.addEventListener('keyup', this.keyUp);
	},
    methods: {
		keyUp: () => {
			if (!drugMenu.active) return;
			if (event.keyCode == 27) 
			{
				mp.trigger('client::drug:close')
			}
		},
        pages: function(page){
            this.page = page;
        },
        btn: function(id){
            if (id == 2)
            {
                mp.trigger('client::drug:close')
            }
            else
            {
                mp.trigger('client::drug:buy', id)
            }
        }
    }
})