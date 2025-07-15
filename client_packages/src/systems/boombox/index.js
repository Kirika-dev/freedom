var AltmenuPopUpOpen = false;

mp.events.add('client::toservbtnAltPopUp', (id) => {
    AltmenuPopUpOpen = false
    global.menuClose(false)
	global.menu.execute(`popup.active = false`);
    mp.events.callRemote('server::toservbtnAltPopUp', id);
});

mp.events.add('client::openboombox', () => {
    global.boombox = mp.browsers.new("http://package/interface/modules/Boombox/index.html")
    global.boombox.active = true
    global.menuOpen(false);
});

mp.events.add('client::closeboombox', () => {
    global.boombox.active = false
    global.boombox = null;
    global.menuClose(false);
});

mp.events.add('client::addaudioonBoomBox', (url) => {
    mp.events.call('client::closeboombox');
    mp.events.callRemote('server::addmusiconboombox', url)
});