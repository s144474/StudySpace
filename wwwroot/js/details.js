//Spot details submit crowdsourced reports with AJAX and refresh the status panel (jQuery)
$(function () {
    const $form = $('#reportForm');
    const $msg = $('#reportMessage');
    const $panel = $('#statusPanel');

    function show(ok, text) {
        $msg.removeClass('d-none alert-success alert-danger').addClass(ok ? 'alert-success' : 'alert-danger').text(text);
    }

    $form.on('submit', function (e) {
        e.preventDefault();
        if (!$form.valid()) return;
        const $btn = $form.find('button[type=submit]').prop('disabled', true);

        $.post($form.attr('action'), $form.serialize())
            .done(function (res) {
                show(true, res.message);
                $form[0].reset();
                $panel.load($panel.data('url'));
            })
            .fail(function (xhr) {
                const text = xhr.responseJSON && xhr.responseJSON.message
                    ? xhr.responseJSON.message : 'Something went wrong while sending your report. Please try again.';
                show(false, text);
            })
            .always(function () { $btn.prop('disabled', false); });
    });

    setInterval(function () { $panel.load($panel.data('url')); }, 30000);
});
