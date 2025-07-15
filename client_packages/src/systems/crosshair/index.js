global.crosshair = mp.browsers["new"]('http://package/interface/CustomCrosshair/custom/index.html');
var crosshair__browser = mp.browsers["new"]('http://package/interface/CustomCrosshair/browser/index.html');
global.crosshair.active = false

global.crosshair2 = {
    vert: {
        background: '#000',
        width: "3px",
        height: "40px",
        opacity: 1
    },
    gor: {
        background: '#000',
        width: "3px",
        height: "40px",
        opacity: 1
    }
}

if(mp.storage.data.crosshair != undefined){
    crosshair2.vert.background = mp.storage.data.crosshair.vert.background
    crosshair2.vert.width = mp.storage.data.crosshair.vert.width
    crosshair2.vert.height = mp.storage.data.crosshair.vert.height
    crosshair2.vert.opacity = mp.storage.data.crosshair.vert.opacity
    crosshair2.gor.background = mp.storage.data.crosshair.gor.background
    crosshair2.gor.width = mp.storage.data.crosshair.gor.width
    crosshair2.gor.height = mp.storage.data.crosshair.gor.height
    crosshair2.gor.opacity = mp.storage.data.crosshair.gor.opacity
}
else{
    mp.storage.data.crosshair = {
        vert: {
            background: '#000',
            width: "3px",
            height: "10px",
            opacity: 1
        },
        gor: {
            background: '#000',
            width: "3px",
            height: "10px",
            opacity: 1
        }
    };
}

mp.events.add('closeCrosshairMenu', () => {
    global.crosshair.active = false 
    global.crosshair.execute(`wrapper.active=false`)
    global.menuClose()
});

mp.events.add('openCrosshairMenu', () => {
    global.crosshair.active = true
    crosshair2.vert.background = mp.storage.data.crosshair.vert.background
	crosshair2.vert.width = mp.storage.data.crosshair.vert.width
	crosshair2.vert.height = mp.storage.data.crosshair.vert.height
	crosshair2.vert.opacity = mp.storage.data.crosshair.vert.opacity
	crosshair2.gor.background = mp.storage.data.crosshair.gor.background
	crosshair2.gor.width = mp.storage.data.crosshair.gor.width
	crosshair2.gor.height = mp.storage.data.crosshair.gor.height
	crosshair2.gor.opacity = mp.storage.data.crosshair.gor.opacity
	crosshair__browser.execute(`crosshair_browser.crosshairAdd('${crosshair2.vert.background}','${crosshair2.vert.width}','${crosshair2.vert.height}','${crosshair2.vert.opacity}','${crosshair2.gor.background}','${crosshair2.gor.width}','${crosshair2.gor.height}','${crosshair2.gor.opacity}')`)
});

mp.events.add('saveCrosshair', (vertBack, vertWidth, vertHeight, vertOp, gorBack, gorWidth, gorheight, gorOp) => {
    crosshair__browser.execute(`crosshair_browser.crosshairAdd('${vertBack}','${vertWidth}','${vertHeight}','${vertOp}','${gorBack}','${gorWidth}','${gorheight}','${gorOp}')`)
    crosshair2.vert.background = vertBack
    crosshair2.gor.background = gorBack
    crosshair2.vert.width = vertWidth
    crosshair2.gor.width = gorWidth
    crosshair2.gor.height = gorheight
    crosshair2.vert.height = vertHeight
    crosshair2.vert.opacity = vertOp
    crosshair2.gor.opacity = gorOp
    mp.storage.data.crosshair = {
        vert: {
            background: vertBack,
            width: vertWidth,
            height: vertHeight,
            opacity: vertOp,
        },
        gor: {
            background: gorBack,
            width: gorWidth,
            height: gorheight,
            opacity: gorOp,
        }
    };
    mp.events.call('notify', 0, 2, "Изменения сохранены.", 3000);
});

mp.events.add('click', (x, y, upOrDown, leftOrRight, relativeX, relativeY, worldPosition, hitEntity) => {
    if(global.menuOpened) return;
    if(leftOrRight == "right"){
        if (upOrDown == "up"){
            crosshair__browser.execute(`crosshair_browser.active=false`)
        }
        else{
            if(mp.players.local.weapon == 2725352035) return;
            crosshair__browser.execute(`crosshair_browser.active=true`)
        }
    }
});

crosshair__browser.execute(`crosshair_browser.crosshairAdd('${crosshair2.vert.background}','${crosshair2.vert.width}','${crosshair2.vert.height}','${crosshair2.vert.opacity}','${crosshair2.gor.background}','${crosshair2.gor.width}','${crosshair2.gor.height}','${crosshair2.gor.opacity}')`)