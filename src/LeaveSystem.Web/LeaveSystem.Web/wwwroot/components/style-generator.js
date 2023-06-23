window.StyleGenerator = {
    create: function (style) {
        let styleElement = document.createElement('style');
        styleElement.type = 'text/css';
        styleElement.innerHTML = style;
        document.getElementsByTagName('head')[0].appendChild(styleElement);
        console.log(styleElement)
    }
}