window.uiFunctions = {
    focusElement: function (element) {
        element.focus();
    },
    scrollElementIntoView: function (element) {
        element.scrollIntoView();
    }
}
window.Play = function (id) {
    document.getElementById(id).play();
}