window.StyleGenerator = {
    create: function (id, style) {
        let styleElement = document.getElementById(id);
        if (!styleElement) {
            styleElement = document.createElement('style');
            styleElement.type = 'text/css';
            document.getElementsByTagName('head')[0].appendChild(styleElement);
        }
        styleElement.innerHTML = style;
    }
}