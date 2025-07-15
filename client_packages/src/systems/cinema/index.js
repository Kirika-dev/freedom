const cinema_camera_pos = [-1426.763427734375, -230.83377075195312, 21.399110794067383];
const cinema_camera_lookat = [-1426.56396484375, -258.2504577636719, 21.399110794067383];
global.cinema = {
	browser: mp.gui,
	open: false,
	camera: null,
}
mp.events.add('client::cinema:open', (url, time, urls, playing, admin, votes, maxvotes) => {
	global.cinema.browser.active = true;
	global.cinema.browser.execute(`cinema.opencinema(${url}, ${time}, ${urls}, ${playing}, ${admin}, ${votes}, ${maxvotes}); cinema.active = true`);
	global.cinema.open = true;
	global.menuOpen();
	// mp.gui.chat.push(`URL: ${url} Time: ${time} Urls: ${urls} Playing ${playing} Admin: ${admin} Vote: ${votes} MaxVotes: ${maxvotes}`)

	global.cinema.camera = mp.cameras.new("default", new mp.Vector3(cinema_camera_pos[0], cinema_camera_pos[1], cinema_camera_pos[2]), new mp.Vector3(0,0,0), 40);
	global.cinema.camera.pointAtCoord(cinema_camera_lookat[0], cinema_camera_lookat[1], cinema_camera_lookat[2]);
	global.cinema.camera.setActive(true);
	mp.game.cam.renderScriptCams(true, false, 0, true, false);
	localplayer.setVelocity(0.0, 0.0, 0.0);
	localplayer.freezePosition(true);
});

mp.events.add('client::cinema:sendinfo', (time, url, urls, votes, maxvotes) => {
	global.cinema.browser.execute(`cinema.sendinfo(${time}, ${url}, ${urls}, ${votes}, ${maxvotes})`);
});

mp.events.add('client::cinema:sendvote', (votes, maxvotes) => {
	global.cinema.browser.execute(`cinema.setvote(${votes}, ${maxvotes})`);
});

mp.events.add('client::cinema:skipvideo', (time, url, urls, vote, votesm) => {
	global.cinema.browser.execute(`cinema.skipvideo(${time}, ${url}, ${urls}, ${vote}, ${votesm})`);
});

mp.events.add('client::cinema:btnurl', (url) => {
	mp.events.callRemote('server::cinema:seturl', url);
});

mp.events.add('client::cinema:skip', () => {
	mp.events.callRemote('server::cinema:skip');
});

mp.events.add('client::cinema:close', () => {
	global.menuClose();
	global.cinema.open = false;
	mp.events.callRemote('server::cinema:close');
	global.cinema.browser.active = false;
	setTimeout(() => {
		if(global.cinema.camera != null)
		{
			global.cinema.camera.setActive(false);
			global.cinema.camera.destroy();
			global.cinema.camera = null;
			mp.game.cam.renderScriptCams(false, false, 0, true, false);
		}
	}, 600);
	
	localplayer.freezePosition(false);
});