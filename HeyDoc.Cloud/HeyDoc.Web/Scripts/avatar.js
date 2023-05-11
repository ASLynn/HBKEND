/* Avatar
----------------------------------------------------------*/
(function ($) {
    var Avatar = function (_options) {
        var _this = this;
        this.options = $.extend({
            $wapper: $('div'),
        }, _options);

        this.init();
    };

    Avatar.prototype.init = function () {
        var _this = this;
        var $img = this.options.$wapper.find('img:first');
        var $input = this.options.$wapper.find('input:first');

        $img.click(function () {
            $input.trigger('click');
        });
        $input.change(function (evt) {
            var $this = $(this);
            var files = evt.target.files; // FileList object

            if (window.FileReader) {
                oFReader = new window.FileReader(),
                rFilter = /^(?:image\/bmp|image\/cis\-cod|image\/gif|image\/ief|image\/jpeg|image\/jpeg|image\/jpeg|image\/pipeg|image\/png|image\/svg\+xml|image\/tiff|image\/x\-cmu\-raster|image\/x\-cmx|image\/x\-icon|image\/x\-portable\-anymap|image\/x\-portable\-bitmap|image\/x\-portable\-graymap|image\/x\-portable\-pixmap|image\/x\-rgb|image\/x\-xbitmap|image\/x\-xpixmap|image\/x\-xwindowdump)$/i;

                oFReader.onload = (function (theFile) {
                    return function (e) {
                        // Render thumbnail.
                        $img.attr('src', e.target.result);
                    };
                })(files[0]);

                if (files.length === 0) { return; }
                if (!rFilter.test(files[0].type)) { alert("You must select a valid image file!"); return; }
                oFReader.readAsDataURL(files[0]);
            }
        });
    }

    $.fn.avatar = function () {
        var avatar = new Avatar({
            $wapper: $(this),
        });
        return avatar; // return javascript object
    };
}(jQuery));