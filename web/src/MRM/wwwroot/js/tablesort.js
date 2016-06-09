(function () {
    // Write on keyup event of keyword input element
    $("#search").keyup(function () {
        // Show only matching TR, hide rest of them
        $.each($("#recordsTable tbody tr"), function () {
            if ($(this).text().toLowerCase().indexOf($(_this).val().toLowerCase()) === -1)
                $(this).hide();
            else
                $(this).show();
        });
    });
})();


(function () {
    $(document).ready(function () {
        $("#recordsTable").tablesorter();
        }
    );
})();
