//Spot finder live filtering with AJAX list/map toggle and auto-refresh (jQuery)
$(function () {
    const $form = $('#filterForm');
    const $results = $('#results');
    let view = 'list';
    let timer = null;

    function applyView() {
        $('#listView').toggleClass('d-none', view !== 'list');
        $('#mapView').toggleClass('d-none', view !== 'map');
        $('[data-view]').each(function () { $(this).toggleClass('active', $(this).data('view') === view); });
    }

    function refresh() {
        $results.addClass('loading');
        $.get($form.data('cards-url'), $form.serialize())
            .done(function (html) { $results.html(html); applyView(); })
            .fail(function () {
                $results.html('<div class="alert alert-danger">Could not load study spots. Check your connection and try again.</div>');
            })
            .always(function () { $results.removeClass('loading'); });
    }

    $form.on('input change', 'input, select', function () {
        clearTimeout(timer);
        timer = setTimeout(refresh, 250);
    });
    $form.on('submit', function (e) { e.preventDefault(); refresh(); });

    $('#resetFilters').on('click', function () {
        $form.find('input[type=text], input[type=number], select').val('');
        $form.find('input[type=checkbox]').prop('checked', false);
        refresh();
    });

    $('[data-view]').on('click', function () { view = $(this).data('view'); applyView(); });

    $results.on('click', '.map-marker', function () {
        $('#Filter_BuildingId').val($(this).data('building'));
        view = 'list';
        refresh();
    });

    setInterval(refresh, 30000);
});
