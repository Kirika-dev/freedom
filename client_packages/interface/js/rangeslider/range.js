
var ranges = setInterval(function() {
    setTimeout(function() { 
         $('input[type=range]').rangeslider({
            polyfill: false,
              onSlide: function (position) {
                  const calc = $(this)[0].rangeDimension / position;
                  const rangeLeft = $(this)[0].$range[0].querySelector('.range-fill__left');
                  const rangeRight = $(this)[0].$range[0].querySelector('.range-fill__right');
                  if (Math.floor(calc) <= 1) {
                      rangeRight.style.width = '0px';
                      rangeLeft.style.width = `${Math.floor($(this)[0].$handle[0].offsetLeft - ($(this)[0].$range[0].offsetWidth / 2)) / ($(this)[0].$range[0].offsetWidth / 2) * $(this)[0].$range[0].offsetWidth / 2}px`; //  fx / 112
                  } else {
                      rangeLeft.style.width = '0px';
                      rangeRight.style.width = `${(Math.floor($(this)[0].$handle[0].offsetLeft - ($(this)[0].$range[0].offsetWidth / 2)) / ($(this)[0].$range[0].offsetWidth / 2) * $(this)[0].$range[0].offsetWidth / 2) * -1}px`; // fx / 112
                  }
              }
          });
          $('.rangeslider').append('<div class="range-fill__right"></div><div class="range-fill__left"></div>');

    }, 10);
  }, 500);



