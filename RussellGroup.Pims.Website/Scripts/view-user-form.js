$(document).ready(function () {
    $('#roles-table').dataTable({
        "order": [[1, "asc"]],
        "processing": false,
        "stateSave": false,
        "lengthChange": false,
        "searching": false,
        "paging": false,
        "columnDefs": [
            { "visible": false, "targets": [0] },
            { "sortable": false, "targets": [1] }
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