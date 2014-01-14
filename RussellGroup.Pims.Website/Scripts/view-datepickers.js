// WhenStarted/WhenEnded

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


// WhenPurchased/WhenDisused

var whenPurchased = $("#WhenPurchased").datepicker({
    format: "d/mm/yyyy"
}).on("changeDate", function (e) {
    whenPurchased.hide();
}).data("datepicker");

var whenDisused = $("#WhenDisused").datepicker({
    format: "d/mm/yyyy"
}).on("changeDate", function (e) {
    whenDisused.hide();
}).data("datepicker");

$("#WhenPurchased-icon").click(function () {
    whenPurchased.show();
});

$("#WhenDisused-icon").click(function () {
    whenDisused.show();
});