var crosshair_browser = new Vue({
    el: "#crosshair_browser",
    data:{
        active: false,
        style: "width: 98vw; height: 98vh; overflow: hidden; overflow-x: hidden; overflow-y: hidden;",
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
    },
    methods:{
      crosshairAdd: function(vertBack, vertWidth, vertHeight, vertOp, gorBack, gorWidth, gorheight, gorOp){
        this.vert.background = vertBack
        this.gor.background = gorBack
        this.vert.width = vertWidth
        this.gor.width = gorWidth
        this.gor.height = gorheight
        this.vert.height = vertHeight
        this.vert.opacity = vertOp
        this.gor.opacity = gorOp
      },
    }
});