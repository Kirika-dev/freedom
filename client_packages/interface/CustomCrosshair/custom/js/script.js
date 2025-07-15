var wrapper = new Vue({
    el: '.wrapper',
    data:{
        active: false,
        page: 0,
        vert: {
            background: '#000',
            width: "3px",
            height: "40px",
            opacity: 1,
        },
        gor: {
            background: '#000',
            width: "40px",
            height: "3px",
            opacity: 1,
        },
        getWidth: 3,
        getheight: 40,
        getOpacity: 100,
        colors:[
            ['FF0000'],
            ['24FC00'],
            ['0000FF'],
            ['FF3D00'],
            ['00FFA3'],
            ['4200FF'],
            ['FF6B00'],
            ['00FFD1'],
            ['8000FF'],
            ['FFA800'],
            ['00D1FF'],
            ['FFC700'],
            ['0057FF'],
            ['FF00F5'],
            ['FFF500'],
            ['002DA1'],
            ['FF00A8'],
            ['FFFFFF'],
            ['323232'],
            ['7A7A7A'],
        ]
    },
    methods:{
        pages: function (a) {
            this.page = a
        },
        forColor: function(a){
            this.vert.background='#' + a
            this.gor.background='#' + a
        },
        save: function(){
            mp.trigger("saveCrosshair", this.vert.background, this.vert.width, this.vert.height, this.vert.opacity, this.gor.background, this.gor.width, this.gor.height, this.gor.opacity)
        },
        open: function(vert, gor){
            this.vert = vert
            this.gor = gor
        },
        exit: function (){
            this.active = false
        },
    },
});
setInterval(() => {
    wrapper.vert.width = wrapper.getWidth + 'px'
    wrapper.gor.height = wrapper.getWidth + 'px'
    wrapper.vert.height = wrapper.getheight + 'px'
    wrapper.gor.width = wrapper.getheight + 'px'
    wrapper.vert.opacity = wrapper.getOpacity / 100
    wrapper.gor.opacity = wrapper.getOpacity / 100
}, 300);