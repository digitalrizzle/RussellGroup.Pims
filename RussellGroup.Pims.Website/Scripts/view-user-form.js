    $(document).ready(function () {
        $('#roles-table').dataTable({
            "bStateSave": false,
            "bFilter": false,
            "bInfo": false,
            "bPaginate": false,
            "aoColumns": [
                { "sWidth": "0%" },
                { "sWidth": "5%" },
                { "sWidth": "20%" },
                { "sWidth": "75%" }
            ],
            "aoColumnDefs": [
                { "bVisible": false, "aTargets": [0] },
                { "bSortable": false, "aTargets": [1] }
            ]
        });
    });

// adjusts the datatable container because pagination is hidden
$('[id$="-table"]').on('draw.dt', function () {
    $('[id$="-table_wrapper"]').css('padding-bottom', 0);
});

$('.check-all').on('click', function () {
    $(this).closest('table').find(':checkbox').prop('checked', this.checked);
});