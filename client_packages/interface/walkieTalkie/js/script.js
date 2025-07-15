var walkieTalkie = new Vue({
    el: ".wrapper",
    data:{
        active: true,
        voice: false, 
        input: 1,
        oldFrequency: ""
    },
    methods:{
        inputPlus: function(){
            this.input++
            if(this.input > 200){
                this.input = 1
            }
            this.checkFrequency()
        },
        inputMinus: function(){
            this.input--
            if(this.input < 1){
                this.input = 200
            }
            this.checkFrequency()
        },
        checkFrequency: function(){
            setInterval(() => {
                if(this.input != this.oldFrequency){
                    mp.trigger('walkie.frequencyChange', this.input)
                    this.oldFrequency = this.input
                }
            }, 500);
        },
        returnValue: function(){
            mp.trigger('walkie.enableWalkie', this.input)
        },
    }
})