function onCategoryChange(data) {
    //debugger;
    var primaryCategory = data.sender.dataItem().primaryCategoryName;
    $("#PrimaryCategory").val(primaryCategory);
}

function onDocumentTypeChange(data) {
    var documentType = $("#DocumentTypeId").data("kendoDropDownList");

    if (oldDocumentType != "" && uploadGrid.data().length > 0) {
        md.confirmDialog('Changing the document type will remove the uploaded documents. Do you want to continue?', function () {
            $.ajax({
                type: 'POST',
                url: deleteSessionUrl,
                success: function () {
                    uploadGrid.ajax.reload();
                }
            });
            setPlantValue([]);
            newFunction();
        }, function () {
            documentType.value(oldDocumentType);
            newFunction();
        })
    } else {
        newFunction();
    }

    function newFunction() {
        if (data.sender.dataItem().code == "DOC_PLNT") {
            setPlantValue([]);
            enablePlant(true);
            setPlantMultiselect(false);
        }
        else if (data.sender.dataItem().code == "DOC_GLBL") {
            setPlantValue([]);
            enablePlant(false);
        }
        else {
            enablePlant(true);
            setPlantMultiselect(true);
        }
        oldDocumentType = documentType.value();
    }
}

function setPlantValue(val) {
    $("#Plants").val(val);
    $("#Plants").multiselect('refresh')
}

function rebuildPlant() {
    $("#Plants").multiselect('rebuild')
}

function enablePlant(isEnable) {
    var plantElem = $("#Plants")
    if (isEnable == true) {
        plantElem.multiselect('enable')
    } else {
        plantElem.multiselect('disable')
    }
}

function setPlantMultiselect(isMultiselect) {
    var plantElem = $("#Plants")
    if (isMultiselect == true) {
        plantElem.attr('multiple', 'multiple');
    } else {
        plantElem.removeAttr('multiple');
    }
    rebuildPlant();
}
function clearUploadForm() {
    $("#DocumentId_uploader").data("uploader").removeAllFiles();
    setPlantValue([]);
    $("#DocumentId").val('0');
}

var uploadGrid;
$(document).ready(function () {
    uploadGrid = $('#list-documents').DataTable({
        "searching": false,
        "processing": true,
        "serverSide": true,
        "paging": false,
        "ordering": false,
        "columns": [
            { "data": "displayName" },
            { "data": "plants" },
            {
                "data": null,
                //"defaultContent": "<button type='button' class='btn btn-primary' onclick='getEditData(data)'>Edit</button>",
                "mRender": function (data, type, row) {
                    var template = kendo.template($("#uploadGridControlsTemplate").html());
                    return template({ id: row.pictureId, sessionId: sessionId })
                    //return "<a href='javascript:void(0)' onclick='deleteDocumentFromSession(" + row.pictureId + ", '" + sessionId + "')'> <i class='fa fa-remove'></i></a>"
                }
            }
        ],
        "ajax": {
            "url": dataUrl,
            "type": "POST",
        }
    });
});

function deleteDocumentFromSession(pictureId, sessionId) {
    $.ajax({
        url: deleteSingleFromSessionUrl,
        type: 'POST',
        data: { pictureId: pictureId, sessionId: sessionId },
        success: function (r) {
            uploadGrid.ajax.reload();
        }
    })
}

function uploadDocumentTosession() {
    var documentType = $("#DocumentTypeId").data("kendoDropDownList").dataItem();
    if (documentType == undefined) {
        md.showNotification("Please select document type", "info");
        return;
    }

    if (documentType.code == "DOC_COM" || documentType.code == "DOC_GLBL") {
        if (uploadGrid.data().length >= 1) {
            md.showNotification("You cannot upload multiple documents for this document type", "info");
            return;
        }
    }

    var plantElement = $("#Plants").data("kendoListView")
    var plantIds = $("#Plants").val();

    if (documentType.code != "DOC_GLBL") {
        if (plantIds == undefined || plantIds.length == 0) {
            md.showNotification("Select plant to upload document", "info");
            return;
        }
    }

    if ($("#DocumentId").val() == "0") {
        md.showNotification("Select file to upload", "info");
        return;
    }

    if (plantIds == null || plantIds == undefined) {
        plantIds = [];
    }
    if (!Array.isArray(plantIds)) {
        plantIds = plantIds.split(',');
    }
    var plants = plantElement.dataSource.data().filter(w=> plantIds.includes(w.id.toString())).map(function (s) {
        return { id: s.id, name: s.name };
    });
    
    var fileName = $("#DocumentId_uploader").data("uploader").files[0].name;
    $.ajax({
        url: uploadUrl,
        type: 'POST',
        data: { pictureId: $("#DocumentId").val(), plants: plants, displayName: fileName },
        success: function (r) {
            uploadGrid.ajax.reload();
            clearUploadForm();
        }
    })
}
