window.StyleGenerator = {
    create: function (style) {
        let styleElement = document.getElementById('holidayTimeslineStyle');
        if (!styleElement) {
            styleElement = document.createElement('style');
            styleElement.type = 'text/css';
            document.getElementsByTagName('head')[0].appendChild(styleElement);
        }
        styleElement.innerHTML = style;
    }
}