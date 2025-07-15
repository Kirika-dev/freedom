global.apartments = mp.browsers.new('http://package/interface/modules/Apartaments/apartments.html');

mp.events.add('client::sendapart', function (index) {
	menuClose(false, true);
	apartments.execute(`aparts.hides()`);
	mp.events.callRemote("server::interact", index);
});

mp.events.add('client::closeapart', function () {
	menuClose(false, true);
	apartments.execute(`aparts.hides()`);
});

mp.events.add('client::openapart', function (data, a, b) {
	if (global.menuCheck()) return;
    menuOpen(false, true);
	apartments.execute(`aparts.show(${data}, ${a}, ${b})`);
});