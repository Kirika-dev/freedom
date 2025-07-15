$('.popit__circle').on('click', function() {
    $(this).toggleClass('active');
	var sound = new Howl({
		src: ['./sounds/1.mp3'],
		loop: false,
		volume: 1
	})
	sound.play()
});

var enable = true;
$('.volume').on('click', function() {
	if (enable) {
		document.getElementById("loading").volume = 0;
		enable = false;
		$(this).toggleClass('active');
	}
	else  {
		document.getElementById("loading").volume = 1;
		enable = true;
		$(this).toggleClass('active');
	}
});

function exit() {
	mp.trigger('client::closepopitmenu')	
}