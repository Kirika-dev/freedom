let rot = 0;
let carcasin;
const podiumHash = 2733879850;
const pedHash = 0xDBB17082;

mp.game.entity.createModelHideExcludingScriptObjects(1100.077,219.9723,-50.04865, 10.0, podiumHash, true);
let podium = mp.objects.new(podiumHash, new mp.Vector3(1100.077,219.9723,-50.084865));

let ped = mp.peds.new(
    pedHash, 
    new mp.Vector3(1104.9641, 220.13695, -48.995),
    270.0,
    0
);

async function rotate()
{
    rot+=0.05;
    if(rot >= 360) rot = 0;
	await podium.setRotation(0, 0, rot, 2, true);
    await carcasin.setHeading(rot);
}

mp.events.add("CAR_LOTTERY::PODIUM_LOAD_CAR_MODEL", (vModel) => {
	if(carcasin) {
		carcasin.destroy();
		carcasin = null;
	}
	carcasin = mp.vehicles.new(mp.game.joaat(vModel), new mp.Vector3(1100.077,219.9723,-50.07865),
	{
		color: [[255, 255, 255],[255,255,255]],
		locked: true
	});
	carcasin.doNotChangeAlpha = true;
	carcasin.setAllowNoPassengersLockon(true);    //no passangers
    carcasin.setCanBeVisiblyDamaged(false);       //no damages
    carcasin.setCanBreak(false);                  //can break
    carcasin.setDeformationFixed();               //fixed deformation
	carcasin.setDirtLevel(0);                     //clear
	carcasin.setDisablePetrolTankDamage(true);    //disable fueltank damage
	carcasin.setDisablePetrolTankFires(true);     //disable fire fuel
	carcasin.setDoorsLockedForAllPlayers(true);   //locked door
	carcasin.freezePosition(true);                //freeze
	carcasin.setInvincible(true);                 //godmode
	carcasin.setDoorsLocked(2);			
	mp.events.add("render", rotate);
});

mp.events.add('client::cameracasinoenter', () =>  {
	cameraCasinoEnter();
});
mp.events.add('client::fadescreen', (a) => {
	FadeScreen(a)
});
async function FadeScreen(a) {
	mp.game.cam.doScreenFadeOut(a);
	global.menuOpen();
		await global.sleep(a + 100)
		global.menuClose();
		mp.game.cam.doScreenFadeIn(a);
}
async function cameraCasinoEnter() {
	global.menuOpen();
	mp.players.local.taskGoToCoordAnyMeans(934.05237, 40.54505, 81.20575, 1, 0, false, 12, 1000);
	casinoCamera2 = mp.cameras.new('default', new mp.Vector3(917.12604, 61.332897, 82.19652), new mp.Vector3(0,0,0), 45);
	casinoCamera2.pointAtPedBone(localplayer.handle, 31086, 0, 0, 0, true);
	casinoCamera2.setActive(true);
	mp.game.cam.renderScriptCams(true, true, 0, true, true);
		await global.sleep(3500)
		casinoCamera2.destroy(true);
		casinoCamera2 = null;
		mp.game.cam.renderScriptCams(true, true, 0, true, true);
		casinoCamera = mp.cameras.new('default', new mp.Vector3(810.8696, 37.823143, 94.42292), new mp.Vector3(0,0,0), 45);
		casinoCamera.setRot(0, 0.0, 270.0, 2);
		casinoCamera.setActive(true);
			await global.sleep(2000)
			mp.game.cam.doScreenFadeOut(800);
				await global.sleep(1500);
				mp.events.callRemote('server::setposCasino');
}
mp.events.add('cameracasinoexit', () =>  {
	CameraCasinoExit();
});
async function CameraCasinoExit() {
	casinoCamera.destroy(true);
	global.menuClose();
	casinoCamera = null;
	mp.game.cam.renderScriptCams(false, true, 0, true, false);
		await global.sleep(100);
		mp.game.cam.doScreenFadeIn(800);
}