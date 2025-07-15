
const COLLDOWN = 50;
var last_id = -1;

var notify = new Vue({
    el: ".notify-list",
    data: {
        list_notify: [],
        interval: null,
        icons: ["error", "warning", "info", "succes"]
    },
    methods: {
        push: function(type, text, time) {
            last_id++;
            this.list_notify.push([type, text, time, 0, last_id])
            
            if (this.interval == null)
                this.interval = setInterval( this.worker, Number(COLLDOWN))
        },

        worker: function() 
        {
            for(let i = this.list_notify.length - 1; i > -1; i--)
            {
                let notify = this.list_notify[i];

                notify[3] = notify[3] + COLLDOWN;

                if (notify[2] <= notify[3])
                {
                    this.list_notify.splice(i, 1);
                }
            }

            this.$forceUpdate();

            if (this.list_notify.length == 0)
            {
                clearInterval(this.interval);
                this.interval = null;
            }
        }
    }
})

var transition = new Vue({
    el: "#transition",
    data: {
        list: [],
        interval: null,
    },
    methods: {
        push: function(time) {
            last_id++;
            this.list.push([time, 0, last_id])
            
            if (this.interval == null)
                this.interval = setInterval( this.worker, Number(COLLDOWN))
        },

        worker: function() 
        {
            for(let i = this.list.length - 1; i > -1; i--)
            {
                let notify = this.list[i];

                notify[1] = notify[1] + COLLDOWN;

                if (notify[0] <= notify[1])
                {
                    this.list.splice(i, 1);
                }
            }

            this.$forceUpdate();

            if (this.list.length == 0)
            {
                clearInterval(this.interval);
                this.interval = null;
            }
        }
    }
})