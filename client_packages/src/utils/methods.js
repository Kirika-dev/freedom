global.drawText = function (text, xPos, yPos, scale = 0.3, r = 255, g = 255, b = 255, a = 255, font = 0, justify = 1, shadow = false, outline = false) {
	if (!mp.game.ui.isHudComponentActive(0)) return false;
	mp.game.ui.setTextFont(font);
	if (typeof scale === "number") mp.game.ui.setTextScale(1, scale);else mp.game.ui.setTextScale(scale[0], scale[1]);
	mp.game.ui.setTextColour(r, g, b, a);
	if (shadow) mp.game.invoke('0x1CA3E9EAC9D93E5E');
	if (outline) mp.game.invoke('0x2513DFB0FB8400FE');

	switch (justify) {
	  case 1:
		mp.game.ui.setTextCentre(true);
		break;

	  case 2:
		mp.game.ui.setTextRightJustify(true);
		mp.game.ui.setTextWrap(0, xPos);
		break;
		
	  case 3:
		mp.game.ui.setTextRightJustify(false);
		mp.game.ui.setTextWrap(xPos, 1);
		break;
	}
	
	mp.game.ui.setTextEntry('STRING');
	mp.game.ui.addTextComponentSubstringPlayerName(text);
	mp.game.ui.drawText(xPos, yPos);
}

mp.events.add('client::screen:transition', (durenter, wait, durleave, eventname) => {
	global.menuOpen(true, false, false);
	mp.game.cam.doScreenFadeOut(durenter);
	mp.game.wait(wait);
	mp.game.cam.doScreenFadeIn(durleave);
	global.menuClose(true, false, false);
	setTimeout(() => {
		localplayer.freezePosition(false);
	}, 100);
	if (eventname != null)
		mp.events.callRemote(eventname);
});

global.drawSprite = function (dict, name, scale, heading, colour, x, y, layer = 0) {
  let resolution = mp.game.graphics.getScreenActiveResolution(0, 0),
      textureResolution = mp.game.graphics.getTextureResolution(dict, name),
      textureScale = [scale[0] * textureResolution.x / resolution.x, scale[1] * textureResolution.y / resolution.y];

  if (mp.game.graphics.hasStreamedTextureDictLoaded(dict)) {
    if (typeof layer === 'number') mp.game.graphics.set2dLayer(layer);
    mp.game.graphics.drawSprite(dict, name, x, y, textureScale[0], textureScale[1], heading, colour.r, colour.g, colour.b, colour.a);
  } else mp.game.graphics.requestStreamedTextureDict(dict, true);
}

mp.events.add('client::createradiusblip', function (state, pos, color, radius) {
	if (state) {
	   const blip = mp.game.ui.addBlipForRadius(pos.x, pos.y, pos.z, radius);
	   mp.game.invoke(getNative("SET_BLIP_SPRITE"), blip, 4);
	   mp.game.invoke(getNative("SET_BLIP_ALPHA"), blip, 255);
	   mp.game.invoke(getNative("SET_BLIP_COLOUR"), blip, color);
	   blips2 = blip;
	}
	else {
		mp.game.invoke(getNative("SET_BLIP_ALPHA"), blips2, 0);
	}
});		