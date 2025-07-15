var wrapper = new Vue({
    el: '.wrapper',
    data:{
        active: false,
        num: 0,
        cars:[
            ["1","A666ET11"],
            ["3","A666ET11"],
            ["Range Rover SVR","A666ET11"],
            ["Range Rover SVR","A666ET11"],
            ["Range Rover SVR","A666ET11"],
            ["Range Rover SVR","A666ET11"],
            ["Range Rover SVR","A666ET11"],
            ["Range Rover SVR","A666ET11"],
            ["Range Rover SVR","A666ET11"],
            ["Range Rover SVR","A666ET11"],
            ["Range Rover SVR","A666ET11"],
            ["Range Rover SVR","A666ET11"],
            ["Range Rover SVR","A666ET11"],
            ["Range Rover SVR","A666ET11"],
            ["Range Rover SVR","A666ET11"],
            ["Range Rover SVR","A666ET11"],
            ["Range Rover SVR","A666ET11"],
            ["Range Rover SVR","A666ET11"],
            ["Range Rover SVR","A666ET11"],
            ["Range Rover SVR","A666ET11"],
            ["Range Rover SVR","A666ET11"],
            ["Range Rover SVR","A666ET11"],
            ["Range Rover SVR","A666ET11"],
            ["Range Rover SVR","A666ET11"],
        ],
    },
    methods:{
        exit: function(){
            this.active = false;
			mp.trigger('client::closegarage')
        },
		select: function(a) {
            if(this.num == 1)
            {
			    mp.trigger('menu', "garage", a);
                this.num = 0
            }else if(this.num == 2)
            {
                mp.trigger('menu', "garagearmy", a);
                this.num = 0
            }else if(this.num == 3){
                mp.trigger('menu', "fracauto", a);
                this.num = 0
            }
		}			
    }
});
function myFunction() {
    let input = document.getElementById("mySearch");
    let filter = input.value.toUpperCase();
    let li = document.getElementsByClassName("main__item");
  
    for (let i = 0; i < li.length; i++) {
        let a = li[i].getElementsByClassName("name")[0];
        if (a.innerHTML.toUpperCase().indexOf(filter) > -1) {
          li[i].style.display = "";
        } else {
          li[i].style.display = "none";
        }
    }
};