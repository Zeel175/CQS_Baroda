
$("#openpopup").click(function () {

    $(".popup").fadeToggle();//This will show the popup on the clicking on the button

});

$("#close-Popup").click(function () {

    $(".popup").fadeOut();//This will close the popup by clicking the close button

});

$(".popup .popup-inner").click(function (e) {

    e.stopPropagation();// this will do nothing if clicked on the inner content container

});

$(".popup").click(function () {

    $(".popup").fadeOut();//this will close the popup if clicked on the outside of the inner content container.

});
function OpenPopupWithUrl(url) {
    $("#inner-content").load("../../Document/EmailDocument/" + url)
    $(".popup").fadeToggle();
}
function closePopup() {
    $(".popup").fadeOut();//This will close the popup by clicking the close button
}
