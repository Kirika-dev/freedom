var updateGameTime = true;
var setTimeCMDused = false;

var nowTime = { Hour: 0, Minute: 0 };
var nowDate = { Day: 7, Month: 4, Year: 2018 };
var nowWeather = "EXTRASUNNY";

mp.events.add('Enviroment_Time', (data) => {
    if (!global.loggedin) return;
	if (data == undefined) return;
    if (updateGameTime)
        mp.game.time.setClockTime(data[0], data[1], 0);

    nowTime.Hour = data[0];
    nowTime.Minute = data[1];

    let time = `${global.formatIntZero(nowTime.Hour, -2)}:${global.formatIntZero(nowTime.Minute, -2)}`;
	var weekDays = ["Восскресенье", "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота"]
    mp.gui.execute(`HUD.time='${time}'; chat.stime = '${time}'; HUD.week=${weekDays[new Date().getDay()]}`);
    global.phone.execute(`phone.time = '${time}'; phone.hour = '${nowTime.Hour + 1}'`);
})

mp.events.add('Enviroment_weather_set', (a, b) => {
	mp.gui.execute(`HUD.gradusWeather='${a}';HUD.weather='${b}'`);
});

mp.events.add('Enviroment_Date', (data) => {
    if (!global.loggedin) return;
	if (data == undefined) return;
    nowDate.Day = data[0];
    nowDate.Month = data[1];
    nowDate.Year = data[2];

    let date = `${global.formatIntZero(nowDate.Day, -2)}.${global.formatIntZero(nowDate.Month, -2)}.${nowDate.Year}`;
    mp.gui.execute(`HUD.date='${date}'`);
})

mp.events.add('Enviroment_Weather', (weather) => {
	if (!global.auth.state) return;
    if (updateGameTime) mp.game.gameplay.setWeatherTypeTransition(mp.game.gameplay.getHashKey(nowWeather), mp.game.gameplay.getHashKey(weather), 0.5);
	else if(setTimeCMDused) mp.game.gameplay.setWeatherTypeNow(weather);
    nowWeather = weather;
})

mp.events.add('doomsday', (bool) => {
    if(bool){
        for (let i = 0; i <= 16; i++) mp.game.graphics.setLightsState(i, true);
    }
    else{
        for (let i = 0; i <= 16; i++) mp.game.graphics.setLightsState(i, false);
    }
})

mp.events.add('Enviroment_Set', (timeData, dateData, weather) => {
	if (!global.loggedin) return;
	mp.game.time.setClockTime(timeData[0], timeData[1], 0);
    
    nowTime.Hour = timeData[0];
    nowTime.Minute = timeData[1];

    let time = `${global.formatIntZero(nowTime.Hour, -2)}:${global.formatIntZero(nowTime.Minute, -2)}`;
    mp.gui.execute(`HUD.time='${time}'`);

    nowDate.Day = dateData[0];
    nowDate.Month = dateData[1];
    nowDate.Year = dateData[2];

    let date = `${global.formatIntZero(nowDate.Day, -2)}.${global.formatIntZero(nowDate.Month, -2)}.${nowDate.Year}`;
    mp.gui.execute(`HUD.date='${date}'`);

    mp.game.gameplay.setWeatherTypeTransition(mp.game.gameplay.getHashKey(nowWeather), mp.game.gameplay.getHashKey(weather), 0.5);
    nowWeather = weather;
});

mp.events.add('Enviroment_Start', (timeData, dateData, weather) => {
    let time = `${global.formatIntZero(nowTime.Hour, -2)}:${global.formatIntZero(nowTime.Minute, -2)}`;
    mp.gui.execute(`HUD.time='${time}'`);

    let date = `${global.formatIntZero(nowDate.Day, -2)}.${global.formatIntZero(nowDate.Month, -2)}.${nowDate.Year}`;
    mp.gui.execute(`HUD.date='${date}'`);

    mp.events.call('authready');
})

setInterval(() => {
    if (updateGameTime) {
        mp.game.gameplay.setWeatherTypeNow(nowWeather);
    }
}, 1000);
mp.events.add('setTimeCmd', (hour, minute, second) => {
	if(hour == -1 && minute == -1 && second == -1) {
		setTimeCMDused = false;
		updateGameTime = true;
		mp.game.gameplay.setWeatherTypeNow(nowWeather);
		mp.game.time.setClockTime(nowTime.Hour, nowTime.Minute, 0);
	} else {
		setTimeCMDused = true;
		updateGameTime = false;
		mp.game.time.setClockTime(hour, minute, second);
	}
})
mp.events.add('stopTime', () => {
    updateGameTime = false;

    mp.game.gameplay.setWeatherTypeNow('EXTRASUNNY');
    mp.game.time.setClockTime(0, 0, 0);
})
mp.events.add('resumeTime', () => {
    updateGameTime = true;

    mp.game.gameplay.setWeatherTypeNow(nowWeather);
    mp.game.time.setClockTime(nowTime.Hour, nowTime.Minute, 0);
})