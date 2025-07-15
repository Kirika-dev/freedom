var radar = new Vue({
    el: ".wrapper",
    data: {
        active: false,
		modal: true,
		radar: false,
		front: false,
		rear: false,
		beep: false,
		input: 0,
		modalinput: false,
        info: {
            buttons: { radar: false, front: false, rear: false, beep: false},
            up: { number: "XX000XXX", front: 170, fast: 170, patrol: 0 },
            down: { number: "XX000XXX", rear: 0, fast: 0, limit: 0 },
        },
        default: {
            buttons: { radar: false, front: false, rear: false, beep: false},
            up: { number: "XX000XXX", front: 0, fast: 0, patrol: 0 },
            down: { number: "XX000XXX", rear: 0, fast: 0, limit: 0 },
        },
    },
    methods: {
        open: function() {
            this.toDefault();
            this.active = true;
            this.modal = true;
            this.beep = false;
            this.rear = false;
            this.front = false;
            this.radar = false;
        },
        toDefault: function() {
            for(let sub in this.info)
                for(let key in this.info[sub] )
                    this.info[sub][key] = this.default[sub][key];
        },
        close: function() {
            this.active = false;
        },
		modalclose: function() {
			this.modal = false
		},
		funcradar: function() {
			if(this.radar == false) {
				this.radar = true
			}
			else {
				this.radar = false
			}
			mp.events.call("client::changeradarstate", this.radar);
		},
		funcfront: function() {
			if(this.front == false) {
				this.front = true
			}
			else {
				this.front = false
			}
			mp.events.call("client::changeradarfrontstate", this.front);
		},
		funcrear: function() {
			if(this.rear == false) {
				this.rear = true
			}
			else {
				this.rear = false
			}
			mp.events.call("client::changeradarrearstate", this.rear);
		},
		funcbeep: function() {
			if(this.beep == false) {
				this.beep = true
			}
			else {
				this.beep = false
			}
			mp.events.call("client::changeradarbeepstate", this.beep);
		},
		reset: function() {
			this.radar = false
			this.front = false
			this.rear = false
			this.beep = false
			mp.events.call("client::changeradarbeepstate", this.beep);
			mp.events.call("client::changeradarrearstate", this.rear);
			mp.events.call("client::changeradarfrontstate", this.front);
			mp.events.call("client::changeradarstate", this.radar);
		},
		openmodalinput: function() {
			if(this.modalinput == false) {
				this.modalinput = true
			}
			else {
				this.modalinput = false
			}
		},
		inputselect: function() {
			this.info.down.limit = this.input
			mp.events.call("client::changeradarlimitstate", this.input)
			this.modalinput = false
			this.input = 0;
		}
    }
});