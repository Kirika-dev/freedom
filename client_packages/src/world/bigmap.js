global.bigmap = [];

bigmap.status = 0;
bigmap.timer = null;

mp.game.ui.setRadarZoom(1.0);
mp.game.ui.setRadarBigmapEnabled(false, false);
var _altpressed = false;
mp.keys.bind(Keys.VK_X, false, () => {
	if (_altpressed) {
		if (bigmap.status === 3) {
			bigmap.status = 0;
		}
		else {
			bigmap.status++;
		}
		mp.gui.execute(`HUD.radar.state=${bigmap.status}`);
	}
});
mp.events.add('render', () => {
    if (!loggedin || chatActive || editing || cuffed || global.menuCheck() || localplayer.getVariable('InDeath') == true || localplayer.getVariable('PLAYER_IN_CASINO') == true || !global.showhud) return;
	
	let key = mp.keys.isDown(18);
    mp.game.controls.disableControlAction(0, 48, true);
	if (key) {
		_altpressed = true;
		mp.gui.execute(`HUD.radar.state=${bigmap.status}`);
		
		var minimap = global.getMinimapAnchor();
		if (bigmap.status === 1) {
			mp.game.ui.setRadarBigmapEnabled(false, false);
			mp.game.ui.setRadarZoom(0.0);
			mp.game.ui.displayRadar(true);
			mp.gui.execute(`HUD.minimapFix=${minimap.rightX * 100}`);
			mp.gui.execute(`HUD.minimapFix2=${minimap.rightX * 110}`);
		} 
		else if (bigmap.status === 0) {
			mp.game.ui.setRadarBigmapEnabled(false, false);
			mp.game.ui.setRadarZoom(1.0);
			mp.game.ui.displayRadar(true);
			mp.gui.execute(`HUD.minimapFix=${minimap.rightX * 100}`);
			mp.gui.execute(`HUD.minimapFix2=${minimap.rightX * 110}`);
		}
		else if (bigmap.status === 2) {
			mp.game.ui.setRadarBigmapEnabled(true, false);
			mp.game.ui.setRadarZoom(0.0);
			mp.game.ui.displayRadar(true);
			mp.gui.execute(`HUD.minimapFix=${(minimap.rightX * 100) * 1.56}`);
			mp.gui.execute(`HUD.minimapFix2=${(minimap.rightX * 100) * 2.76}`);
		} 
		else if (bigmap.status === 3) {
			mp.game.ui.displayRadar(false);
			mp.gui.execute(`HUD.minimapFix=0`);
			mp.gui.execute(`HUD.minimapFix2=${minimap.rightX * 110}`);
		}
	}
	else {
		_altpressed = false;
		mp.gui.execute(`HUD.radar.state='-1'`);
	}
});