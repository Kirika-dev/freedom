var input = new Vue({
    el: '.input_menu',
    data: {
        active: false,
        title: "Header",
        plholder: "Text",
        input: "",
        len: 99
    },
    methods: {
        set : function(title,help,length){
            this.title = title
            this.plholder = help
            this.len = length
            this.input = ""
        },
        send : function(){
            mp.trigger('input',this.input)
        },
		exit: function() {
			mp.trigger('closeinp');
			this.hide();
		}
    }
})