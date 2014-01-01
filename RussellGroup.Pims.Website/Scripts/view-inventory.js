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