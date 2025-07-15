let casinoBarLoaded = false;

mp.events.add("client::casino:bar:open", (json) => {
	if (!loggedin || chatActive || editing || cuffed) return;
	if(!casinoBarLoaded)
	{
		global.menuOpen();
		global.casinoBar = mp.browsers.new('http://package/interface/modules/Casino/Bar/index.html');
		global.casinoBar.active = true;
		casinoBarLoaded = true;
	}
	global.casinoBar.execute(`casinoBar.active=true`);
	global.casinoBar.execute(`casinoBar.buyitems=${json}`);
});

mp.events.add("client::casino:bar:close", () => {
	global.menuClose();
	global.casinoBar.active = false;
	global.casinoBar.destroy();
	casinoBarLoaded = false;
});

mp.events.add("client::casino:bar:buy", (id, value) => {
	mp.events.callRemote("server::casino:bar:buy", id, value);
});

mp.keys.bind(Keys.VK_ESCAPE, false, function () {
	if (casinoBarLoaded == true) {
		mp.events.call('client::casino:bar:close');
	}
});