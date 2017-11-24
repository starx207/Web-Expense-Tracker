// Write your JavaScript code.
$(document).ready(function() {
    $(".datepicker").datepicker();

    $(".expandDropdown").click(function() {
        var id = $(this).attr("for");
        $("#detail" + id).toggle(250);
    });
});