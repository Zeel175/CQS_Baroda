CKEDITOR.plugins.add('serverupload', {
    icons: 'serverupload',
    init: function (editor) {
        //Plugin logic goes here.
        editor.addCommand('openUpload', {
            exec: function (editor) {
                var myWindow = $("#uploadDialog")
                myWindow.kendoWindow({
                    width: "600px",
                    title: "Upload Image",
                    visible: false,
                    actions: [
                        "Close"
                    ],
                    content: editorUploadUrl,
                    open: function () {
                        myWindow.data('editorInstance', editor);
                    }
                }).data("kendoWindow").center().open();
            }
        });


        editor.ui.addButton('ServerUpload', {
            label: 'Server Upload',
            command: 'openUpload',
            toolbar: 'insert,100'
        });
    }
});
