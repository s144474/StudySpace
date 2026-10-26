//Booking form show opening hours and already booked slots for the chosen room/date (jQuery)
$(function () {
    const $form = $('#bookingForm');
    const $box = $('#availability');

    function check() {
        const spotId = $('#Input_SpotId').val();
        const date = $('#Input_Date').val();
        if (!spotId || !date) { $box.addClass('d-none'); return; }

        $.getJSON($form.data('availability-url'), { spotId: spotId, date: date })
            .done(function (res) {
                let text = 'Open ' + res.hours + '. ';
                text += res.booked.length
                    ? 'Already booked: ' + res.booked.map(function (b) { return b.start + ' to ' + b.end; }).join(', ') + '.'
                    : 'No bookings yet on this day.';
                $box.text(text).removeClass('d-none');
            })
            .fail(function () { $box.text('Could not load availability for this room.').removeClass('d-none'); });
    }

    $('#Input_SpotId, #Input_Date').on('change', check);
    check();
});
