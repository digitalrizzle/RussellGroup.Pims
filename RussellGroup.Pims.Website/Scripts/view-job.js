var whenStarted = $("#WhenStarted").datepicker({
    format: "d/mm/yyyy"
}).on("changeDate", function (e) {
    whenStarted.hide();
}).data("datepicker");

var whenEnded = $("#WhenEnded").datepicker({
    format: "d/mm/yyyy"
}).on("changeDate", function (e) {
    whenEnded.hide();
}).data("datepicker");

$("#WhenStarted-icon").click(function () {
    whenStarted.show();
});

$("#WhenEnded-icon").click(function () {
    whenEnded.show();
});