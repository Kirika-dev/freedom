let casinoMarketLoaded = false;

mp.events.add('client::casino::chips:open', (a,b) => {
	if (global.menuCheck() || localplayer.getVariable('InDeath') == true || !loggedin || chatActive || editing || cuffed) return;
	mp.gui.execute(`CasinoChips.Open(${Number(a)},${Number(b)})`);
	global.menuOpen(false);
});

mp.events.add('client::casino::chips:buy', (a) => {
	mp.gui.chat.push(a);
	mp.events.callRemote('server::casino::chips:buy', Number(a));
});

mp.events.add('client::casino::chips:sell', (a) => {
	mp.events.callRemote('server::casino::chips:sell', Number(a));
});

mp.events.add('client::casino::chips:close', () => {
	mp.gui.execute(`CasinoChips.Reset()`);
	global.menuClose(false);
});