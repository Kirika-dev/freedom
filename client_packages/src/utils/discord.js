setInterval(function () {
    let label = 'на Freedom Project';
    try {
        if (localplayer.getVariable('REMOTE_ID') == undefined) {
            label = 'Рождается';
        }
        else
			{
			if (localplayer.getVariable('ON_WORK'))
                label = 'Работает где-то на';
            else if (mp.players.local.isDiving())
                label = 'Занимается дайвингом во';
            else if (mp.players.local.isSwimming() || mp.players.local.isSwimmingUnderWater())
                label = 'Плавает в штате';
            else if (mp.players.local.isFalling())
                label = 'Падает с высоты в штате';
            else if (mp.players.local.isRagdoll())
                label = 'Лежит на земле в штате';
            else if (mp.players.local.isDead())
                label = 'Умирает в штате';
            else if (mp.players.local.isInAnyVehicle(false))
                label = 'Ездит по штату';
            else if (mp.players.local.isRunning() || mp.players.local.isSprinting())
                label = 'Бегает по штату';
            else if (mp.players.local.isShooting())
                label = 'Стреляется в штате';
            else if (mp.players.local.isWalking())
                label = 'Бродит по штату';
            else
                label = 'Проводит время на';
        }    
    }
    catch (e) {}
    mp.discord.update(label, 'на Freedom Project');
}, 10000);	