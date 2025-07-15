global.walkieTalkie = mp.browsers["new"]('http://package/interface/walkieTalkie/index.html');
global.walkieTalkie.active = false;

mp.events.add('walkie.open', (fractionfrequency) => {
    if(localplayer.getVariable('LISTENER_RADIO')){
        global.walkieTalkie.active = true;
        global.walkieTalkie.execute(`walkieTalkie.input='${fractionfrequency}'`);
        global.walkieTalkie.execute(`walkieTalkie.active=true`);
    }
});

mp.events.add('walkie.talking', (player) => {
    if(player.getVariable('LISTENER_RADIO')){
        global.walkieTalkie.execute(`playSound("talking");`);
        player.voiceVolume = 1.0;
        player.voice3d = true;
        localplayer.voice3d = true;
        localplayer.voiceVolume = 1.0;
        localplayer.isListening = false;
        mp.voiceChat.muted = false; 
    }
});

mp.events.add('walkie.disableTalking', (player) => {
    if(player.getVariable('LISTENER_RADIO')){
        player.voiceVolume = 0.0;
        player.voice3d = false;
        localplayer.voice3d = false;
        localplayer.voiceVolume = 0.0;
        localplayer.isListening = true;
        mp.voiceChat.muted = true;
    }
});

mp.events.add('walkie.close', () => {
    global.walkieTalkie.execute(`walkieTalkie.active=false`);
    global.walkieTalkie.active = false;
});

mp.keys.bind(Keys.VK_UP, true, function () {
    if(!global.walkieTalkie.active) return;
    mp.events.callRemote("talkingInWalkie")
    global.walkieTalkie.execute(`walkieTalkie.voice=true`);
});

mp.keys.bind(Keys.VK_UP, false, function () {
    if(!global.walkieTalkie.active) return;
    mp.events.callRemote("DisableTalkingWalkie")
    global.walkieTalkie.execute(`walkieTalkie.voice=false`);
});

mp.events.add('walkie.playSound', () => {
    global.walkieTalkie.execute(`playSound("notalking");`);
});

mp.events.add('walkie.frequencyChange', (frequency) => {
    mp.events.callRemote("ChangeFrequency", parseInt(frequency))
});