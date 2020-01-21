

var mcomponents = (function () {

    var elementReference = null;

    return {
        registerKeyListener: function (element) {
            elementReference = element;
            document.addEventListener('keydown', mcomponents.onKeyDownEvent);
        },

        unRegisterKeyListener: function () {
            document.removeEventListener('keydown', mcomponents.onKeyDownEvent);
            elementReference = null;
        },

        onKeyDownEvent: function (args) {
            if (elementReference != null) {
                elementReference.invokeMethodAsync('JsInvokeKeyDown', args.key);
            }
        },

        getPosition: function (element) {
            var pos = element.getBoundingClientRect();

            // because blazor will serialize it with the implemented .ToJson() method and the new attributes will be lost!
            pos = JSON.parse(JSON.stringify(pos));

            pos.absbottom = pos.bottom + window.scrollY;
            pos.abstop = pos.top + window.scrollY;
            pos.absleft = pos.left + window.scrollX;
            pos.absright = pos.right + window.scrollX;

            return pos;
        },

        focusElement: function (element) {
            if (element != null) {
                element.focus();
            }
        },

        toDataUrl: function (element) {
            return element.toDataURL();
        },

        stopEvent: function (element, event) {
            element.addEventListener(event, function (event) {
                event.preventDefault();
                event.stopPropagation();
            }, false);
        },

        registerMPaintOnTouchMove: function (reference, element) {
            element.addEventListener("touchmove", function (event) {
                if (event.touches) {
                    if (event.touches.length == 1) {

                        var pos = element.getBoundingClientRect();

                        var touch = event.touches[0];
                        touchX = touch.clientX - pos.left;
                        touchY = touch.clientY - pos.top;

                        reference.invokeMethodAsync('OnJsTouchMove', touchX, touchY);
                    }
                }

                event.preventDefault();
                event.stopPropagation();
            }, false);
        },

        saveAsFile: function (filename, bytesBase64) {
            var link = document.createElement('a');
            link.download = filename;
            link.href = "data:application/octet-stream;base64," + bytesBase64;
            document.body.appendChild(link); // Needed for Firefox
            link.click();
            document.body.removeChild(link);
        },

        getColumnsWith: function (element) {

            var ret = [];

            if (element != null && element.children != null) {
                var children = element.children[0].children[0].children;

                for (var i = 0; i < children.length; i++) {
                    var tableChild = children[i];
                    ret.push(tableChild.offsetWidth);
                }
            }

            return ret;
        }
    };
})();

