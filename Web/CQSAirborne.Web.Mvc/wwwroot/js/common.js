function generateBreadCrumbPath(data, parentId) {
    var clickablePath = "";
    if (parentId == 0)
        return "";
    var dataToProcess = data.reverse();
    $.each(dataToProcess, function (i, v) {
        var listTemplate = kendo.template($("#clickablePathTemplate").html());
        clickablePath += listTemplate({ id: v.id, name: v.name, isLastElement: v.isLastElement });
    });

    var template = kendo.template($("#breadCrumbTemplate").html());
    return template({ generatedPath: clickablePath });
}