﻿var markers = [];

mp.events.add('createCheckpoint', function (uid, type, position, scale, dimension, r, g, b, dir) {
    if (typeof markers[uid] != "undefined") 
    {
        markers[uid].destroy();
        markers[uid] = undefined;
    }
    if (dir != undefined) {
        markers[uid] = mp.checkpoints.new(type, position, scale,
            {
                direction: dir,
                color: [r, g, b, 200],
                visible: true,
                dimension: dimension
            });
    }
    else {
        markers[uid] = mp.markers.new(type, position, scale,
            {
                visible: true,
                dimension: dimension,
                color: [r, g, b, 255]
            });
    }
});

mp.events.add('deleteCheckpoint', function (uid) {
    if (typeof markers[uid] == "undefined") return;
    markers[uid].destroy();
    markers[uid] = undefined;
});

mp.events.add('createWaypoint', function (x, y) {
    mp.game.ui.setNewWaypoint(x, y);
});

var workBlip = null;
mp.events.add('createWorkBlip', function (position) {
    if (workBlip != null)
        workBlip.destroy();
    workBlip = mp.blips.new(0, position,
        {
            name: "Чекпоинт",
            scale: 1,
            color: 5,
            alpha: 255,
            drawDistance: 100,
            shortRange: false,
            rotation: 0,
            dimension: 0,
        });
}); 
mp.events.add('deleteWorkBlip', function () {
    if (workBlip != null)
        workBlip.destroy();
    workBlip = null;
});

var garageBlip = null;
mp.events.add('createGarageBlip', function (position) {
    if (garageBlip != null)
        garageBlip.destroy();
    garageBlip = mp.blips.new(473, position,
        {
            name: "Гараж",
            scale: 0.7,
            color: 2,
            alpha: 255,
            drawDistance: 100,
            shortRange: true,
            rotation: 0,
            dimension: 0,
        });
});

mp.events.add('client::blip:shortrange', function(blip, state) {
	blip.shortRange = state;
});

mp.events.add('deleteGarageBlip', function () {
    if (garageBlip != null)
        garageBlip.destroy();
    garageBlip = null;
});

mp.events.add('changeBlipColor', function (blip, color) {
    try {
        if (blip == null) return;
        blip.setColour(color);
    } catch (e) { } 
});

mp.events.add('changeBlipAlpha', function (blip, alpha) {
    try {
        if (blip == null) return;
        blip.setAlpha(alpha);
    } catch (e) { } 
});

var sproute = null;

mp.events.add('createRoute', function (x, y)
{
    sproute = mp.blips.new(478, new mp.Vector3(x, y), { alpha: 255, name: "Точка разгрузки", scale: 1, color: 1 });
    sproute.setRoute(true);
    sproute.setRouteColour(5);
});

mp.events.add('removeRoute', function(){
    sproute.destroy();
});

var houseBlip = null;
mp.events.add('createHouseBlip', function (position) {
    if (houseBlip != null)
        houseBlip.destroy();
    houseBlip = mp.blips.new(40, position,
        {
            name: "Мой дом",
            scale: 0.9,
            color: 5,
            alpha: 255,
            drawDistance: 100,
            shortRange: false,
            rotation: 0,
            dimension: 0,
        });
});

mp.events.add('deleteHouseBlip', function () {
    if (houseBlip != null)
        houseBlip.destroy();
    houseBlip = null;
});