﻿

var mcomponents = (function () {

    var elementReferences = [];

    return {
        registerKeyListener: function (element) {
            if (element == null) {
                return;
            }

            elementReferences.push(element);
            document.addEventListener('keydown', mcomponents.onKeyDownEvent);
            console.log(elementReferences.length);
        },

        unRegisterKeyListener: function (element) {
            if (element == null) {
                return;
            }

            elementReferences = elementReferences.filter(function (value, index, arr) {
                return value._id != element._id;
            });

            if (elementReferences.length == 0) {
                document.removeEventListener('keydown', mcomponents.onKeyDownEvent);
            }
            console.log(elementReferences.length);
        },

        onKeyDownEvent: function (args) {
            if (elementReferences != null && elementReferences.length > 0) {
                elementReferences[elementReferences.length-1].invokeMethodAsync('JsInvokeKeyDown', args.key);
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

        clearFocus: function () {
            if (document.activeElement) {
                document.activeElement.blur();
            }

            window.focus();
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

        getColumnSizes: function (element, columns) {
            var ret = [];

            if (element != null && element.children != null) {

                var tableBorderTop = getComputedStyle(element, null).getPropertyValue('border-top-width');
                var tableBorderLeft = getComputedStyle(element, null).getPropertyValue('border-left-width');
                ret.push(tableBorderTop);
                ret.push(tableBorderLeft);

                //we grab the last td, because the first may be the filter row

                var childs = element.children[1].children;

                if (childs[childs.length - 1] == null) { //Table is empty
                    childs = element.children[0].children;
                }

                var lastTd = childs[childs.length - 1].children[0];

                if (lastTd == undefined) { //table has data, but no columns
                    lastTd = childs[childs.length - 1];
                }

                var rowheight = lastTd.getBoundingClientRect().height;

                var borderRight = getComputedStyle(lastTd, null).getPropertyValue('border-right-width');
                var borderTop = getComputedStyle(lastTd, null).getPropertyValue('border-top-width');
                var borderSpacing = getComputedStyle(lastTd, null).getPropertyValue('border-spacing');
                var borderCollapse = getComputedStyle(lastTd, null).getPropertyValue('border-collapse');

                ret.push(borderRight);
                ret.push(borderTop);
                ret.push(borderSpacing);
                ret.push(borderCollapse);
                ret.push(rowheight.toString());

                var children = Array.from(element.children[0].children[0].children);

                for (var i = 0; i < columns.length; i++) {

                    var tableChild = children.find(c => c.getAttribute("data-identifier") == columns[i]);

                    if (tableChild == null) {
                        ret.push("75");
                        continue;
                    }

                    var rect = tableChild.getBoundingClientRect();
                    ret.push(rect.width.toString());
                }
            }

            return ret;
        },

        scrollToSelectedEntry: function () {

            window.setTimeout(function () {

                var elements = document.getElementsByClassName("m-select-options-entry--highlighted");

                if (elements.length == 0) {
                    return;
                }

                var selected = elements[0];

                var offset = selected.offsetTop;

                offset -= selected.clientHeight * 4;

                document.getElementsByClassName("m-select-options-list")[0].scrollTop = offset;
            }, 150);
        },

        invokeClick: function (id) {
            var elem = document.getElementById(id);
            if (elem && document.createEvent) {
                var evt = document.createEvent("MouseEvents");
                evt.initEvent("click", true, false);
                elem.dispatchEvent(evt);
            }
        }
    };
})();

