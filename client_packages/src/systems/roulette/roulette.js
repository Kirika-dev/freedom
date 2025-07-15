let roulette;

mp.keys.bind(Keys.VK_F10, false, function () {
    if(roulette == null){
        if (!loggedin || global.chatActive || editing || global.menuOpened) return;
        roulette = mp.browsers.new('http://package/interface/modules/roulette/index.html');
        mp.events.callRemote("r:SendCasePrize");
        global.menuOpen()
    }else{
        mp.events.call('r:rouletteClose');
    }
});
mp.events.add('r:SendCasePrize', (prize,money) => {
    if (roulette != null){ roulette.execute(`roulette.prizes=${prize}`);
    roulette.execute(`roulette.money=${money}`);
}
})
mp.events.add('r:setcase', (c) => {
    if (roulette != null) roulette.execute(`roulette.setCase(${c})`)
});
mp.events.add('r:getCase', (c) => {
    caseid = c;
    mp.events.callRemote('r:GetCase', c);
});
mp.events.add('r:getWinId', (type) => {
    if (caseid == undefined) return;
    mp.events.callRemote('r:getWinId', type, caseid);
});
mp.events.add('r:getprize', (type, id) => {
    if (caseid == undefined) return;
    mp.events.callRemote('r:getPrize', type, id, caseid);
});
mp.events.add('r:getWinIdCallback', (e, type) => {
    if (roulette != null) roulette.execute(`roulette.getWinIdCallback(${e},'${type}')`);
});

mp.events.add('HelpMenu:SendNotify', (type, layout, msg, time)=>{
    if (roulette != null) roulette.execute(`notify(${type},${layout},'${msg}',${time})`);
});
mp.events.add('SetRedBucksInMenu', (money)=>{
    roulette.execute(`roulette.money='${money}'`);
});
mp.events.add('r:rouletteClose', () => {
    if (roulette != null) {
        roulette.destroy();
        roulette = null;
    }
    global.menuClose();
    mp.gui.cursor.visible = false;
})
