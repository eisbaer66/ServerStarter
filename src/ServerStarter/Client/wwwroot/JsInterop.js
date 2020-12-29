window.uiFunctions = {
    focusElement: function (element) {
        if (!element)
            return;
        if (!element.focus)
            return;
        element.focus();
    },
    scrollElementIntoView: function (element) {
        if (!element)
            return;
        if (!element.scrollIntoView)
            return;
        element.scrollIntoView();
    },
    prompt: function (text, defaultText) {
        window.prompt(text, defaultText);
    }
}
window.Play = function (id) {
    document.getElementById(id).play();
}
window.openNewWindow = function (link) {
    window.open(link, "_blank");
}