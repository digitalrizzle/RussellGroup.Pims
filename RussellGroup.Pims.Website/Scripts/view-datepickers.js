// WhenStarted/WhenEnded

$("#WhenStarted").datepicker({
    format: "d/mm/yyyy",
    autoclose: true
});

$("#WhenStarted-icon").click(function () {
    $("#WhenStarted").datepicker("show");
});

$("#WhenEnded").datepicker({
    format: "d/mm/yyyy",
    autoclose: true
});

$("#WhenEnded-icon").click(function () {
    $("#WhenEnded").datepicker("show");
});


// WhenPurchased/WhenDisused

$("#WhenPurchased").datepicker({
    format: "d/mm/yyyy",
    autoclose: true
});

$("#WhenPurchased-icon").click(function () {
    $("#WhenPurchased").datepicker("show");
});

$("#WhenDisused").datepicker({
    format: "d/mm/yyyy",
    autoclose: true
});

$("#WhenDisused-icon").click(function () {
    $("#WhenDisused").datepicker("show");
});