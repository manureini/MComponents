

var mcomponents = (function () {

    var elementReferencesKeyDown = [];
    var elementReferencesKeyUp = [];

    var mSelectReference = null;

    return {
        registerKeyListener: function (identifier, element) {
            if (identifier == null || element == null) {
                return;
            }

            elementReferencesKeyDown.push({
                key: identifier,
                value: element
            });

            document.addEventListener('keydown', mcomponents.onKeyDownEvent);
        },

        unRegisterKeyListener: function (identifier) {
            if (identifier == null) {
                return;
            }

            elementReferencesKeyDown = elementReferencesKeyDown.filter(function (value, index, arr) {
                return value.key != identifier;
            });

            if (elementReferencesKeyDown.length == 0) {
                document.removeEventListener('keydown', mcomponents.onKeyDownEvent);
            }
        },

        onKeyDownEvent: function (args) {
            if (elementReferencesKeyDown != null && elementReferencesKeyDown.length > 0) {
                elementReferencesKeyDown[elementReferencesKeyDown.length - 1].value.invokeMethodAsync('JsInvokeKeyDown', args.key);
            }
        },

        ///////////////////////////////////////

        registerKeyUpListener: function (identifier, element) {
            if (identifier == null || element == null) {
                return;
            }

            elementReferencesKeyUp.push({
                key: identifier,
                value: element
            });

            document.addEventListener('keyup', mcomponents.onKeyUpEvent);
        },

        unRegisterKeyUpListener: function (identifier) {
            if (identifier == null) {
                return;
            }

            elementReferencesKeyUp = elementReferencesKeyUp.filter(function (value, index, arr) {
                return value.key != identifier;
            });

            if (elementReferencesKeyUp.length == 0) {
                document.removeEventListener('keyup', mcomponents.onKeyUpEvent);
            }
        },

        onKeyUpEvent: function (args) {
            if (elementReferencesKeyUp != null && elementReferencesKeyUp.length > 0) {
                elementReferencesKeyUp[elementReferencesKeyUp.length - 1].value.invokeMethodAsync('JsInvokeKeyUp', args.key);
            }
        },


        ///////////////////////////////////////

        registerMSelect: function (element) {
            if (element == null) {
                return;
            }

            if (mSelectReference == element) {
                return;
            }

            if (mSelectReference != null) {
                mSelectReference.invokeMethodAsync('JsInvokeMSelectFocusOut');
            }

            mSelectReference = element;
            document.addEventListener('click', mcomponents.onMSelectClickEvent);
        },

        unRegisterMSelect: function (element) {
            if (mSelectReference == null || element == null || element._id != mSelectReference._id) {
                return;
            }

            mSelectReference = null;
            document.removeEventListener('click', mcomponents.onMSelectClickEvent);
        },

        onMSelectClickEvent: function (args) {
            if (mSelectReference == null) {
                return;
            }

            if (!args.target.matches('.m-select-options-container *')) {
                mSelectReference.invokeMethodAsync('JsInvokeMSelectFocusOut');
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

            return JSON.stringify(ret);
        },

        scrollToSelectedEntry: function () {

            window.setTimeout(function () {

                var elements = document.getElementsByClassName("m-select-options-entry--highlighted");

                if (elements.length == 0) {

                    var containers = document.getElementsByClassName("m-select-options-list-container");

                    if (containers.length == 0) {
                        setTimeout(mcomponents.scrollToSelectedEntry, 50);
                        return;
                    }

                    containers[0].style.visibility = 'visible';
                    return;
                }

                var selected = elements[0];

                var offset = selected.offsetTop;

                offset -= selected.clientHeight * 4;

                document.getElementsByClassName("m-select-options-list")[0].scrollTop = offset;

                document.getElementsByClassName("m-select-options-list-container")[0].style.visibility = 'visible';

            }, 30);
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

