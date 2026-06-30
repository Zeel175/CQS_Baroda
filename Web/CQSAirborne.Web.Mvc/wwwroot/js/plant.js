var plantDataSource = new kendo.data.DataSource({
    transport: {
        read: {
            url: dataUrl,
            dataType: "json"
        }
    }
});

$(function () {
    $("#plantGrid tbody").kendoListView({
        dataSource: plantDataSource,
        template: kendo.template($('#tableTemplate').html()),
    });
});

