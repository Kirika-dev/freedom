
//дефолтные проценты, которые мы будем отнимать от входящего урона
let defaultPercent = { "max": 85, "min": 40 , "head": 99};

//список оружий и их процент, который мы будем снимать с входящего урона
const weaponDamages = {
	// Пистолеты
	// хеш оружия
	3249783761: {
		//название оружия, это для нас, чтобы в будущем смогли быстро найти нужное нам оружие
		"name": "Heavy Revolver",
		//максимальный процент
		"max": 85,
		//минимальный процент
		"min": 65,
		//эти проценты нужны для функции рандома
		"head": 80
	},
	// Пистолет пулеметы
	324215364: {
		"name": "Micro SMG",
		"max": 8,
		"min": 50,
		"head": 14
	},
	736523883: {
		"name": "SMG",
		"max": 80,
		"min": 50,
		"head": 22
	},
	171789620: {
		"name": "Combat PDW", // вписать название
		"max": 60, // не трогай 
		"min": 40, // не трогай 
		"head": 15 // вписать урон в голову
	},
	// Пулеметы
	2144741730: {
		"name": "Combat MG",
		"max": 65,
		"min": 35,
		"head": 35
	},
	// Карабины
	3220176749: {
		"name": "Assault Rifle",
		"max": 70,
		"min": 45,
		"head": 25
	},
	// Дробовики
	487013001: {
		"name": "Pump Shotgun",
		"max": 80,
		"min": 30,
		"head": -1
	},
	// Снайперы
	100416529: {
		"name": "Sniper Rifle",
		"max": 80,
		"min": 50,
		"head": 105
	},
	// Холодное оружие
	3441901897: {
		"name": "Battle Axe",
		"max": 50,
		"min": 40,
		"head": -1
	}
};

//Если какое-либо оружие окажется в этом списке, мы не выполним скрипт
const ignoreWeapons = {
	911657153: "Stun Gun",
};

//функция генерации рандомного числа
let randomInt = (min, max) => Math.random() * (max - min) + min;

//Событие принятия входящего попадания игроком
mp._events.add('incomingDamage', (sourceEntity, sourcePlayer, targetEntity, weapon, boneIndex, damage) => {
	mp.console.logInfo(`boneIndex: ${boneIndex}`);
	if (targetEntity.type === 'player' && sourcePlayer && !(weapon in ignoreWeapons)) {
		
		//Если у игрока поставлена админская неуязвимость не выполняем скрипт
		if (global.admingm || global.localplayer.getVariable('green')) return true;
		//Ставим стандартный процент гасения урона
		let max = defaultPercent.max;
		let min = defaultPercent.min;
		let head = defaultPercent.head;
		let wp = "";
		//Если оружие, с которого стреляли, есть у нас в списке, то берем его процент гасения
		if (weapon in weaponDamages) {
			max = weaponDamages[weapon].max;
			min = weaponDamages[weapon].min;
			head = weaponDamages[weapon].head;
			wp = weaponDamages[weapon].name;
		}
		//Полученный значения используем для генерации случайного значения в их диапазоне
		let percent = randomInt(min, max) / 100;
		//Получаем кастомный урон, который будем применять
		

		let cDamage = damage - (damage * percent);
		if (wp == "Heavy Revolver") {
			if (boneIndex === 20) cDamage = head;
			else  cDamage = 34;
		} else if (wp == "Combat PDW"){
			if (boneIndex === 20) cDamage = head;
			else  cDamage = 9;
		}else if (wp == "Assault Rifle"){
			if (boneIndex === 20) cDamage = head;
			else  cDamage = 18;
		}else if (wp == "Combat MG"){
			if (boneIndex === 20) cDamage = head;
			else  cDamage = 14;
		}else if (wp == "SMG"){
			if (boneIndex === 20) cDamage = head;
			else  cDamage = 12;
		}else if (wp == "Micro SMG"){
			if (boneIndex === 20) cDamage = head;
			else  cDamage = 8;
		}else if (wp == "Sniper Rifle"){
			if (boneIndex === 20) cDamage = head;
			else  cDamage = 65;
		}
		else {
			if (boneIndex === 20)cDamage = damage - (damage * percent) / 10;
		}
		//если попадание в голову, делим урон ещё на 10, дабы уменьшить ещё, так как в голову идет очень большой урон
		//Применяем к игроку полученный урон
		targetEntity.applyDamageTo(parseInt(cDamage), true);
		/* 
		Узнаем сколько здоровья у игрока после урона
		Если игрок не умер, то отменяем стандартное событие
		Если игрок умер, то не отменяем, т.к. если отменим
		То не сработает событие playerDeath как должно
		*/
		let currentHealth = mp.players.local.getHealth();
		//Отменяем стандартное событие
		if (currentHealth > 0) {
			return true;
		}
	}
});
