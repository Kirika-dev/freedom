let localplayer = mp.players.local;

var raycasting = {

    camera: mp.cameras.new("gameplay"),

    directions: {
        front: 0,
        rear: 1
    },

    getEntity: function(directionf, distance = 10)
    {
        let veh = localplayer.vehicle;
        const rotation = veh.getRotation(5);

        const size = mp.game.gameplay.getModelDimensions(veh.model);
        /*const position = mp.game.object.getObjectOffsetFromCoords(veh.position.x, veh.position.y, veh.position.z, veh.getHeading(), 0, directionf == 0 ? size.max.y + 0.1 : size.min.y - 0.1 , 0);
        const target = mp.game.object.getObjectOffsetFromCoords(veh.position.x, veh.position.y, veh.position.z, veh.getHeading(), 0, 
            directionf == 0 ? size.max.y + 0.1 + distance : size.min.y - 0.1 - distance, 
            rotation.y * 1.25);*/

        if (directionf == 0) {
            var position = veh.getOffsetFromInWorldCoords(0.0, 5.0, 1.0);
            var target = veh.getOffsetFromInWorldCoords(0.0, 70.0, 0.0);
        }
        else {
            var position = veh.getOffsetFromInWorldCoords(0.0, -5.0, 1.0);
            var target = veh.getOffsetFromInWorldCoords(0.0, -70.0, 0.0);
        }

        let result = mp.raycasting.testCapsule(position, target, 10.0, [localplayer.vehicle.handle], [10]);
        if (frontcar.entity && typeof result !== 'undefined' && frontcar.entity.type == "vehicle" && result.entity.handle != localplayer.vehicle.handle) {
            if (typeof result.entity.type === 'undefined') return null;
            if (result.entity != null) {
                return result.entity;
            } 
        }
    }

}