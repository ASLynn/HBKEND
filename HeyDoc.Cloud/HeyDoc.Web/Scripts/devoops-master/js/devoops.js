//
//    Main script of DevOOPS v1.0 Bootstrap Theme
//
"use strict";
/*-------------------------------------------
	Dynamically load plugin scripts
---------------------------------------------*/
//
// Dynamically load  OpenStreetMap Plugin
// homepage: http://openlayers.org
//
function LoadOpenLayersScript(callback) {
    if (!$.fn.OpenLayers) {
        $.getScript('http://www.openlayers.org/api/OpenLayers.js', callback);
    }
    else {
        if (callback && typeof (callback) === "function") {
            callback();
        }
    }
}
//
// Dynamically load  Leaflet Plugin
// homepage: http://leafletjs.com
//
function LoadLeafletScript(callback) {
    if (!$.fn.L) {
        $.getScript('plugins/leaflet/leaflet.js', callback);
    }
    else {
        if (callback && typeof (callback) === "function") {
            callback();
        }
    }
}
//
//  Dynamically load jQuery DateTimePicker plugin
//  homepage: http://xdsoft.net/jqplugins/datetimepicker/
//
function LoadDateTimePickerScript(callback) {
    if (!$.fn.datetimepicker) {
        $.getScript('/Scripts/jquery.datetimepicker.js', callback);
    }
    else {
        if (callback && typeof (callback) === "function") {
            callback();
        }
    }
}
//
//  Dynamically load Bootstrap Validator Plugin
//  homepage: https://github.com/nghuuphuoc/bootstrapvalidator
//
function LoadBootstrapValidatorScript(callback) {
    if (!$.fn.bootstrapValidator) {
        $.getScript('/Scripts/devoops-master/plugins/bootstrapvalidator/bootstrapValidator.js', callback);
    }
    else {
        if (callback && typeof (callback) === "function") {
            callback();
        }
    }
}
//
//  Dynamically load jQurey Form Plugin
//  homepage: https://github.com/malsup/form/
//
function LoadjQueryFormScript(callback) {
    if (!$.fn.bootstrapValidator) {
        $.getScript('/Scripts/jquery.form.min.js', callback);
    }
    else {
        if (callback && typeof (callback) === "function") {
            callback();
        }
    }
}
//
//  Dynamically load jQuery Select2 plugin
//  homepage: https://github.com/ivaynberg/select2  v3.4.5  license - GPL2
//  https://select2.github.io/
//
function LoadSelect2Script(callback) {
    if (!$.fn.select2) {
        $.getScript('/Scripts/select2.js', callback);
    }
    else {
        if (callback && typeof (callback) === "function") {
            callback();
        }
    }
}
//
//  Dynamically load jQuery Select2 OptGroup Select plugin
//  homepage: https://github.com/bnjmnhndrsn/select2-optgroup-select
//
function LoadSelect2OptGroupSelectScript(callback) {

    LoadSelect2Script(function () {
        $.fn.select2.amd.define('optgroup-data', ['select2/data/select', 'select2/utils'], function (SelectAdapter, Utils) {
            function OptgroupData($element, options) {
                OptgroupData.__super__.constructor.apply(this, arguments);
                this._checkOptgroups();
            }

            Utils.Extend(OptgroupData, SelectAdapter);

            OptgroupData.prototype.current = function (callback) {
                var data = [];
                var self = this;
                this._checkOptgroups();
                this.$element.find(':not(.selected-custom) :selected, .selected-custom').each(function () {
                    var $option = $(this);
                    var option = self.item($option);

                    if (!option.hasOwnProperty('id')) {
                        option.id = 'optgroup';
                    }

                    data.push(option);
                });

                callback(data);
            };

            OptgroupData.prototype.bind = function (container, $container) {
                OptgroupData.__super__.bind.apply(this, arguments);
                var self = this;

                container.on('optgroup:select', function (params) {
                    self.optgroupSelect(params.data);
                });

                container.on('optgroup:unselect', function (params) {
                    self.optgroupUnselect(params.data);
                });
            };

            OptgroupData.prototype.select = function (data) {
                if ($(data.element).is('optgroup')) {
                    this.optgroupSelect(data);
                    return;
                }

                // Change selected property on underlying option element 
                data.selected = true;
                data.element.selected = true;

                this.$element.trigger('change');
                this.clearSearch();

                // Manually trigger dropdrop positioning handler
                $(window).trigger('scroll.select2');
            };

            OptgroupData.prototype.unselect = function (data) {
                if ($(data.element).is('optgroup')) {
                    this.optgroupUnselect(data);
                    return;
                }

                // Change selected property on underlying option element 
                data.selected = false;
                data.element.selected = false;

                this.$element.trigger('change');

                // Manually trigger dropdrop positioning handler
                $(window).trigger('scroll.select2');
            };

            OptgroupData.prototype.optgroupSelect = function (data) {
                data.selected = true;
                var vals = this.$element.val() || [];

                var newVals = $.map(data.element.children, function (child) {
                    return '' + child.value;
                });

                newVals.forEach(function (val) {
                    if ($.inArray(val, vals) == -1) {
                        vals.push(val);
                    }
                });

                this.$element.val(vals);
                this.$element.trigger('change');
                this.clearSearch();

                // Manually trigger dropdrop positioning handler
                $(window).trigger('scroll.select2');
            };

            OptgroupData.prototype.optgroupUnselect = function (data) {
                data.selected = false;
                var vals = this.$element.val() || [];
                var removeVals = $.map(data.element.children, function (child) {
                    return '' + child.value;
                });
                var newVals = [];

                vals.forEach(function (val) {
                    if ($.inArray(val, removeVals) == -1) {
                        newVals.push(val);
                    }
                });
                this.$element.val(newVals);
                this.$element.trigger('change');

                // Manually trigger dropdrop positioning handler
                $(window).trigger('scroll.select2');
            };

            // Check if all children of optgroup are selected. If so, select optgroup
            OptgroupData.prototype._checkOptgroups = function () {
                this.$element.find('optgroup').each(function () {
                    var children = this.children;

                    var allSelected = !!children.length;

                    for (var i = 0; i < children.length; i++) {
                        allSelected = children[i].selected;
                        if (!allSelected) { break; }
                    }

                    $(this).toggleClass('selected-custom', allSelected);
                });

            };

            OptgroupData.prototype.clearSearch = function () {
                if (!this.container) {
                    return;
                }

                if (this.container.selection.$search.val()) {
                    this.container.selection.$search.val('');
                    this.container.selection.handleSearch();
                }
            }

            return OptgroupData;
        });

        $.fn.select2.amd.define('optgroup-results', ['select2/results', 'select2/utils', 'select2/keys'], function OptgroupResults(ResultsAdapter, Utils, KEYS) {
            function OptgroupResults() {
                OptgroupResults.__super__.constructor.apply(this, arguments);
            };

            Utils.Extend(OptgroupResults, ResultsAdapter);

            OptgroupResults.prototype.option = function (data) {
                var option = OptgroupResults.__super__.option.call(this, data);

                if (data.children) {
                    var $label = $(option).find('.select2-results__group');
                    $label.attr({
                        'role': 'treeitem',
                        'aria-selected': 'false'
                    });
                    $label.data('data', data);
                }

                return option;
            };

            OptgroupResults.prototype.bind = function (container, $container) {
                OptgroupResults.__super__.bind.call(this, container, $container);
                var self = this;

                this.$results.on('mouseup', '.select2-results__group', function (evt) {
                    var $this = $(this);

                    var data = $this.data('data');

                    var trigger = ($this.attr('aria-selected') === 'true') ? 'optgroup:unselect' : 'optgroup:select';

                    self.trigger(trigger, {
                        originalEvent: evt,
                        data: data
                    });

                    return false;
                });

                this.$results.on('mouseenter', '.select2-results__group[aria-selected]', function (evt) {
                    var data = $(this).data('data');

                    self.getHighlightedResults()
                        .removeClass('select2-results__option--highlighted');

                    self.trigger('results:focus', {
                        data: data,
                        element: $(this)
                    });
                });

                container.on('optgroup:select', function () {
                    if (!container.isOpen()) {
                        return;
                    }

                    if (self.options.options.closeOnSelect) {
                        self.trigger('close');
                    }

                    self.setClasses();
                });

                container.on('optgroup:unselect', function () {
                    if (!container.isOpen()) {
                        return;
                    }

                    self.setClasses();
                });
            };

            OptgroupResults.prototype.setClasses = function (container, $container) {
                var self = this;

                this.data.current(function (selected) {
                    var selectedIds = [];
                    var optgroupLabels = [];

                    $.each(selected, function (i, obj) {
                        if (obj.children) {
                            optgroupLabels.push(obj.text);
                            $.each(obj.children, function (j, child) {
                                selectedIds.push(child.id.toString());
                            });
                        } else {
                            selectedIds.push(obj.id.toString());
                        }
                    });

                    var $options = self.$results.find('.select2-results__option[aria-selected]');

                    $options.each(function () {
                        var $option = $(this);

                        var item = Utils.GetData(this, 'data');

                        // id needs to be converted to a string when comparing
                        var id = '' + item.id;

                        if ((item.element != null && item.element.selected) ||
                            (item.element == null && $.inArray(id, selectedIds) > -1)) {
                            $option.attr('aria-selected', 'true');
                        } else {
                            $option.attr('aria-selected', 'false');
                        }
                    });


                    var $groups = self.$results.find('.select2-results__group[aria-selected]');

                    $groups.each(function () {
                        var $optgroup = $(this);
                        var item = Utils.GetData(this, 'data');
                        var text = item.text;
                        var $element = $(item.element);

                        if ($element.hasClass('selected-custom') || $.inArray(text, optgroupLabels) > -1) {
                            $optgroup.attr('aria-selected', 'true');
                        } else {
                            $optgroup.attr('aria-selected', 'false');
                        }
                    });

                    if (!self.getHighlightedResults().length) {
                        $('.select2-results__option[aria-selected]').first().trigger('mouseenter');
                    }
                });
            };

            return OptgroupResults;
        });

        callback();
    });
}
//
//  Dynamically load DataTables plugin
//  homepage: http://datatables.net v1.9.4 license - GPL or BSD
//
function LoadDataTablesScripts(callback) {
    function LoadDatatables() {
        $.getScript('/Scripts/DataTables/jquery.dataTables.min.js', function () {
            $.getScript('/Scripts/devoops-master/plugins/datatables/ZeroClipboard.js', function () {
                $.getScript('/Scripts/devoops-master/plugins/datatables/TableTools.js', function () {
                    $.getScript('/Scripts/devoops-master/plugins/datatables/dataTables.bootstrap.js', callback);
                });
            });
        });
    }

    if (!$.fn.dataTableExt.oApi.fnPagingInfo) {
        LoadDatatables();
    }
    else {
        if (callback && typeof (callback) === "function") {
            callback();
        }
    }
}
//
//  Dynamically load Widen FineUploader
//  homepage: https://github.com/Widen/fine-uploader  v5.0.5 license - GPL3
//
function LoadFineUploader(callback) {
    if (!$.fn.fineuploader) {
        $.getScript('plugins/fineuploader/jquery.fineuploader-5.0.5.min.js', callback);
    }
    else {
        if (callback && typeof (callback) === "function") {
            callback();
        }
    }
}
//
//  Dynamically load avatar
//
function LoadAvatar(callback) {
    if (!$.fn.avatar) {
        $.getScript('/Scripts/avatar.js', callback);
    }
    else {
        if (callback && typeof (callback) === "function") {
            callback();
        }
    }
}

//
//  Dynamically load Bootbox js
//  homepage: https://github.com/makeusabrew/bootbox  v4.4.0 license - MIT
//
function LoadBootbox(callback) {
    if (!$.fn.bootbox) {
        $.getScript('/Scripts/bootbox.min.js', callback);
    }
    else {
        if (callback && typeof (callback) === "function") {
            callback();
        }
    }
}

/*-------------------------------------------
	Main scripts used by theme
---------------------------------------------*/
//
//  Function for load content from url and put in $('.ajax-content') block
//
function LoadAjaxContent(url) {
    $('.preloader').show();
    $.ajax({
        mimeType: 'text/html; charset=utf-8', // ! Need set mimeType only when run from local file
        url: url,
        type: 'GET',
        success: function (data) {
            $('.xdsoft_datetimepicker').remove();
            $('#ajax-content').html(data);
            $('.preloader').hide();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert(errorThrown);
        },
        dataType: "html",
        async: false
    });
}
//
//  Function maked all .box selector is draggable, to disable for concrete element add class .no-drop
//
function WinMove() {
    $("div.box").not('.no-drop')
        .draggable({
            revert: true,
            zIndex: 2000,
            cursor: "crosshair",
            handle: '.box-name',
            opacity: 0.8
        })
        .droppable({
            tolerance: 'pointer',
            drop: function (event, ui) {
                var draggable = ui.draggable;
                var droppable = $(this);
                var dragPos = draggable.position();
                var dropPos = droppable.position();
                draggable.swap(droppable);
                setTimeout(function () {
                    var dropmap = droppable.find('[id^=map-]');
                    var dragmap = draggable.find('[id^=map-]');
                    if (dragmap.length > 0 || dropmap.length > 0) {
                        dragmap.resize();
                        dropmap.resize();
                    }
                    else {
                        draggable.resize();
                        droppable.resize();
                    }
                }, 50);
                setTimeout(function () {
                    draggable.find('[id^=map-]').resize();
                    droppable.find('[id^=map-]').resize();
                }, 250);
            }
        });
}
//
// Swap 2 elements on page. Used by WinMove function
//
jQuery.fn.swap = function (b) {
    b = jQuery(b)[0];
    var a = this[0];
    var t = a.parentNode.insertBefore(document.createTextNode(''), a);
    b.parentNode.insertBefore(a, b);
    t.parentNode.insertBefore(b, t);
    t.parentNode.removeChild(t);
    return this;
};
//
//  Screensaver function
//  used on locked screen, and write content to element with id - canvas
//
function ScreenSaver() {
    var canvas = document.getElementById("canvas");
    var ctx = canvas.getContext("2d");
    // Size of canvas set to fullscreen of browser
    var W = window.innerWidth;
    var H = window.innerHeight;
    canvas.width = W;
    canvas.height = H;
    // Create array of particles for screensaver
    var particles = [];
    for (var i = 0; i < 25; i++) {
        particles.push(new Particle());
    }
    function Particle() {
        // location on the canvas
        this.location = { x: Math.random() * W, y: Math.random() * H };
        // radius - lets make this 0
        this.radius = 0;
        // speed
        this.speed = 3;
        // random angle in degrees range = 0 to 360
        this.angle = Math.random() * 360;
        // colors
        var r = Math.round(Math.random() * 255);
        var g = Math.round(Math.random() * 255);
        var b = Math.round(Math.random() * 255);
        var a = Math.random();
        this.rgba = "rgba(" + r + ", " + g + ", " + b + ", " + a + ")";
    }
    // Draw the particles
    function draw() {
        // re-paint the BG
        // Lets fill the canvas black
        // reduce opacity of bg fill.
        // blending time
        ctx.globalCompositeOperation = "source-over";
        ctx.fillStyle = "rgba(0, 0, 0, 0.02)";
        ctx.fillRect(0, 0, W, H);
        ctx.globalCompositeOperation = "lighter";
        for (var i = 0; i < particles.length; i++) {
            var p = particles[i];
            ctx.fillStyle = "white";
            ctx.fillRect(p.location.x, p.location.y, p.radius, p.radius);
            // Lets move the particles
            // So we basically created a set of particles moving in random direction
            // at the same speed
            // Time to add ribbon effect
            for (var n = 0; n < particles.length; n++) {
                var p2 = particles[n];
                // calculating distance of particle with all other particles
                var yd = p2.location.y - p.location.y;
                var xd = p2.location.x - p.location.x;
                var distance = Math.sqrt(xd * xd + yd * yd);
                // draw a line between both particles if they are in 200px range
                if (distance < 200) {
                    ctx.beginPath();
                    ctx.lineWidth = 1;
                    ctx.moveTo(p.location.x, p.location.y);
                    ctx.lineTo(p2.location.x, p2.location.y);
                    ctx.strokeStyle = p.rgba;
                    ctx.stroke();
                    //The ribbons appear now.
                }
            }
            // We are using simple vectors here
            // New x = old x + speed * cos(angle)
            p.location.x = p.location.x + p.speed * Math.cos(p.angle * Math.PI / 180);
            // New y = old y + speed * sin(angle)
            p.location.y = p.location.y + p.speed * Math.sin(p.angle * Math.PI / 180);
            // You can read about vectors here:
            // http://physics.about.com/od/mathematics/a/VectorMath.htm
            if (p.location.x < 0) p.location.x = W;
            if (p.location.x > W) p.location.x = 0;
            if (p.location.y < 0) p.location.y = H;
            if (p.location.y > H) p.location.y = 0;
        }
    }
    setInterval(draw, 30);
}
//
//  Function for create 2 dates in human-readable format (with leading zero)
//
function PrettyDates() {
    var currDate = new Date();
    var year = currDate.getFullYear();
    var month = currDate.getMonth() + 1;
    var startmonth = 1;
    if (month > 3) {
        startmonth = month - 2;
    }
    if (startmonth <= 9) {
        startmonth = '0' + startmonth;
    }
    if (month <= 9) {
        month = '0' + month;
    }
    var day = currDate.getDate();
    if (day <= 9) {
        day = '0' + day;
    }
    var startdate = year + '-' + startmonth + '-01';
    var enddate = year + '-' + month + '-' + day;
    return [startdate, enddate];
}
//
//  Function set min-height of window (required for this theme)
//
function SetMinBlockHeight(elem) {
    elem.css('min-height', window.innerHeight - 49)
}
//
//  Helper for correct size of Messages page
//
function MessagesMenuWidth() {
    var W = window.innerWidth;
    var W_menu = $('#sidebar-left').outerWidth();
    var w_messages = (W - W_menu) * 16.666666666666664 / 100;
    $('#messages-menu').width(w_messages);
}
//
// Function for change panels of Dashboard
//
function DashboardTabChecker() {
    $('#content').on('click', 'a.tab-link', function (e) {
        e.preventDefault();
        $('div#dashboard_tabs').find('div[id^=dashboard]').each(function () {
            $(this).css('visibility', 'hidden').css('position', 'absolute');
        });
        var attr = $(this).attr('id');
        $('#' + 'dashboard-' + attr).css('visibility', 'visible').css('position', 'relative');
        $(this).closest('.nav').find('li').removeClass('active');
        $(this).closest('li').addClass('active');
    });
}
//
//  Helper for open ModalBox with requested header, content and bottom
//
//
function OpenModalBox(header, inner, bottom) {
    var modalbox = $('#modalbox');
    modalbox.find('.modal-header-name span').html(header);
    modalbox.find('.devoops-modal-inner').html(inner);
    modalbox.find('.devoops-modal-bottom').html(bottom);
    modalbox.fadeIn('fast');
    $('body').addClass("body-expanded");
}
//
//  Close modalbox
//
//
function CloseModalBox() {
    var modalbox = $('#modalbox');
    modalbox.fadeOut('fast', function () {
        modalbox.find('.modal-header-name span').children().remove();
        modalbox.find('.devoops-modal-inner').children().remove();
        modalbox.find('.devoops-modal-bottom').children().remove();
        $('body').removeClass("body-expanded");
    });
}
//
//  Beauty tables plugin (navigation in tables with inputs in cell)
//  Created by DevOOPS.
//
(function ($) {
    $.fn.beautyTables = function () {
        var table = this;
        var string_fill = false;
        this.on('keydown', function (event) {
            var target = event.target;
            var tr = $(target).closest("tr");
            var col = $(target).closest("td");
            if (target.tagName.toUpperCase() == 'INPUT') {
                if (event.shiftKey === true) {
                    switch (event.keyCode) {
                        case 37: // left arrow
                            col.prev().children("input[type=text]").focus();
                            break;
                        case 39: // right arrow
                            col.next().children("input[type=text]").focus();
                            break;
                        case 40: // down arrow
                            if (string_fill == false) {
                                tr.next().find('td:eq(' + col.index() + ') input[type=text]').focus();
                            }
                            break;
                        case 38: // up arrow
                            if (string_fill == false) {
                                tr.prev().find('td:eq(' + col.index() + ') input[type=text]').focus();
                            }
                            break;
                    }
                }
                if (event.ctrlKey === true) {
                    switch (event.keyCode) {
                        case 37: // left arrow
                            tr.find('td:eq(1)').find("input[type=text]").focus();
                            break;
                        case 39: // right arrow
                            tr.find('td:last-child').find("input[type=text]").focus();
                            break;
                        case 40: // down arrow
                            if (string_fill == false) {
                                table.find('tr:last-child td:eq(' + col.index() + ') input[type=text]').focus();
                            }
                            break;
                        case 38: // up arrow
                            if (string_fill == false) {
                                table.find('tr:eq(1) td:eq(' + col.index() + ') input[type=text]').focus();
                            }
                            break;
                    }
                }
                if (event.keyCode == 13 || event.keyCode == 9) {
                    event.preventDefault();
                    col.next().find("input[type=text]").focus();
                }
                if (string_fill == false) {
                    if (event.keyCode == 34) {
                        event.preventDefault();
                        table.find('tr:last-child td:last-child').find("input[type=text]").focus();
                    }
                    if (event.keyCode == 33) {
                        event.preventDefault();
                        table.find('tr:eq(1) td:eq(1)').find("input[type=text]").focus();
                    }
                }
            }
        });
        table.find("input[type=text]").each(function () {
            $(this).on('blur', function (event) {
                var target = event.target;
                var col = $(target).parents("td");
                if (table.find("input[name=string-fill]").prop("checked") == true) {
                    col.nextAll().find("input[type=text]").each(function () {
                        $(this).val($(target).val());
                    });
                }
            });
        })
    };
})(jQuery);
//
// Beauty Hover Plugin (backlight row and col when cell in mouseover)
//
//
(function ($) {
    $.fn.beautyHover = function () {
        var table = this;
        table.on('mouseover', 'td', function () {
            var idx = $(this).index();
            var rows = $(this).closest('table').find('tr');
            rows.each(function () {
                $(this).find('td:eq(' + idx + ')').addClass('beauty-hover');
            });
        })
            .on('mouseleave', 'td', function (e) {
                var idx = $(this).index();
                var rows = $(this).closest('table').find('tr');
                rows.each(function () {
                    $(this).find('td:eq(' + idx + ')').removeClass('beauty-hover');
                });
            });
    };
})(jQuery);
//
//  Function convert values of inputs in table to JSON data
//
//
function Table2Json(table) {
    var result = {};
    table.find("tr").each(function () {
        var oneRow = [];
        var varname = $(this).index();
        $("td", this).each(function (index) { if (index != 0) { oneRow.push($("input", this).val()); } });
        result[varname] = oneRow;
    });
    var result_json = JSON.stringify(result);
    OpenModalBox('Table to JSON values', result_json);
}
/*-------------------------------------------
    Function for File upload page (form_file_uploader.html)
---------------------------------------------*/
function FileUpload() {
    $('#bootstrapped-fine-uploader').fineUploader({
        template: 'qq-template-bootstrap',
        classes: {
            success: 'alert alert-success',
            fail: 'alert alert-error'
        },
        thumbnails: {
            placeholders: {
                waitingPath: "assets/waiting-generic.png",
                notAvailablePath: "assets/not_available-generic.png"
            }
        },
        request: {
            endpoint: 'server/handleUploads'
        },
        validation: {
            allowedExtensions: ['jpeg', 'jpg', 'gif', 'png']
        }
    });
}
/*-------------------------------------------
    Function for OpenStreetMap page (maps.html)
---------------------------------------------*/
//
// Load GeoIP JSON data and draw 3 maps
//
function LoadTestMap() {
    $.getJSON("http://www.telize.com/geoip?callback=?",
        function (json) {
            var osmap = new OpenLayers.Layer.OSM("OpenStreetMap");//создание слоя карты
            var googlestreets = new OpenLayers.Layer.Google("Google Streets", { numZoomLevels: 22, visibility: false });
            var googlesattelite = new OpenLayers.Layer.Google("Google Sattelite", { type: google.maps.MapTypeId.SATELLITE, numZoomLevels: 22 });
            var map1_layers = [googlestreets, osmap, googlesattelite];
            // Create map in element with ID - map-1
            var map1 = drawMap(json.longitude, json.latitude, "map-1", map1_layers);
            $("#map-1").resize(function () { setTimeout(map1.updateSize(), 500); });
            // Create map in element with ID - map-2
            var osmap1 = new OpenLayers.Layer.OSM("OpenStreetMap");//создание слоя карты
            var map2_layers = [osmap1];
            var map2 = drawMap(json.longitude, json.latitude, "map-2", map2_layers);
            $("#map-2").resize(function () { setTimeout(map2.updateSize(), 500); });
            // Create map in element with ID - map-3
            var sattelite = new OpenLayers.Layer.Google("Google Sattelite", { type: google.maps.MapTypeId.SATELLITE, numZoomLevels: 22 });
            var map3_layers = [sattelite];
            var map3 = drawMap(json.longitude, json.latitude, "map-3", map3_layers);
            $("#map-3").resize(function () { setTimeout(map3.updateSize(), 500); });
        }
    );
}

/*-------------------------------------------
    Function for Doctor page (form doctor/index)
---------------------------------------------*/
//
// Doctor edit form validator function
//
function DoctorValidator() {
    $('form#doctorCreate').bootstrapValidator({      
        message: 'This value is not valid',
        fields: {
            Email: {
                validators: {                
                    remote: {
                        message: 'this email is already registered in our system as user or HCP. Please use other email.',
                        url: '/Doctor/ValidateEmail',
                        type: 'POST',
                        data: function (validator, $field, value) {
                            return {
                                email: validator.getFieldElements('Email').val()
                            };
                        },
                    }
                }
            },
            PhoneNumber: {
                validators: {
                    notEmpty: {
                        message: 'The Contact Number is required and can\'t be empty'
                    },
                    remote: {
                        message: 'this phone is already registered in our system as user or HCP. Please use other phone number.',
                        url: '/Doctor/ValidatePhone',
                        type: 'POST',
                        data: function (validator, $field, value) {
                            return {
                                phone: validator.getFieldElements('PhoneNumber').val()
                            };
                        },
                    }
                }
            },
            Password: {
                validators: {
                    notEmpty: {
                        message: 'The password is required and can\'t be empty'
                    },
                    identical: {
                        field: 'confirmPassword',
                        message: 'The password and its confirm are not the same'
                    }
                }
            },
            ConfirmPassword: {
                validators: {
                    notEmpty: {
                        message: 'The confirm password is required and can\'t be empty'
                    },
                    identical: {
                        field: 'password',
                        message: 'The password and its confirm are not the same'
                    }
                }
            },
            Specialty: {
                validators: {
                    notEmpty: {
                        message: 'The Specialty is required and can\'t be empty'
                    },
                }
            },
            FullName: {
                validators: {
                    notEmpty: {
                        message: 'The Full Name is required and can\'t be empty'
                    },
                }
            },
            //Practicing: {
            //    validators: {
            //        notEmpty: {
            //            message: 'The Practicing Since is required and can\'t be empty'
            //        },
            //    }
            //},
            //MedicalSch: {
            //    validators: {
            //        notEmpty: {
            //            message: 'The Medical School is required and can\'t be empty'
            //        },
            //    }
            //},
            //AboutMe: {
            //    validators: {
            //        notEmpty: {
            //            message: 'The About Me is required and can\'t be empty'
            //        },
            //    }
            //},
            //CategoryId: {
            //    validators: {
            //        notEmpty: {
            //            message: 'The Category is required and can\'t be empty'
            //        },
            //    }
            //},
            Qualification: {
                validators: {
                    notEmpty: {
                        message: 'The Qualification is required and can\'t be empty'
                    },
                }
            },
            //RegisterNumber: {
            //    validators: {
            //        notEmpty: {
            //            message: 'The Register Number is required and can\'t be empty'
            //        },
            //    }
            //},
            //Address: {
            //    validators: {
            //        notEmpty: {
            //            message: 'The Address is required and can\'t be empty'
            //        },
            //    }
            //},
            Terms: {
                validators: {
                    notEmpty: {
                        message: 'You must agree with the terms and conditions'
                    }
                }
            },
            //certificateFile: {
            //    validators: {
            //        notEmpty: {
            //            message: ' Please attach your certificates like basic degree and relevant documentations'
            //        }
            //    }
            //},
            //Title: {
            //    validators: {
            //        notEmpty: {
            //            message: ' The Title is required and can\'t be empty'
            //        }
            //    }
            //},
            Gender: {
                validators: {
                    notEmpty: {
                        message: 'The Gender is required and can\'t be empty'
                    }
                }
            },
            Birthday: {
                validators: {
                    notEmpty: {
                        message: 'The Birthday is required and can\'t be empty'
                    }
                }
            },
            //Language: {
            //    validators: {
            //        notEmpty: {
            //            message: 'The Languager is required and can\'t be empty'
            //        }
            //    }
            //},
            CountryId: {
                validators: {
                    notEmpty: {
                        message: 'The Country is required and can\'t be empty'
                    }
                }
            },
            
            IC: {
                validators: {
                    notEmpty: {
                        message: 'The IC No is required and can\'t be empty'
                    }
                }
            }
            //HospitalName: {
            //    validators: {
            //        notEmpty: {
            //            message: 'The Clinic Name is required and can\'t be empty'
            //        }
            //    }
            //},
        }
    });
    $('form#doctorEdit').bootstrapValidator({
        message: 'This value is not valid',
        fields: {
            Email: {
                validators: {
                    remote: {
                        message: 'this email is already registered in our system as user or HCP. Please use other email.',
                        url: '/Doctor/ValidateEmailForEdit',
                        type: 'POST',
                        data: function (validator, $field, value) {
                            return {
                                email: validator.getFieldElements('Email').val(),
                                userid: validator.getFieldElements('DoctorId').val()
                            };
                        },
                    }
                }
            },
            PhoneNumber: {
                validators: {
                    notEmpty: {
                        message: 'The Contact Number is required and can\'t be empty'
                    },
                    remote: {
                        message: 'this phone is already registered in our system as user or HCP. Please use other phone number.',
                        url: '/Doctor/ValidatePhoneForEdit',
                        type: 'POST',
                        data: function (validator, $field, value) {
                            return {
                                phone: validator.getFieldElements('PhoneNumber').val(),
                                userid: validator.getFieldElements('DoctorId').val()
                            };
                        },
                    }
                }
            },
            Password: {
                validators: {
                    notEmpty: {
                        message: 'The password is required and can\'t be empty'
                    },
                    identical: {
                        field: 'confirmPassword',
                        message: 'The password and its confirm are not the same'
                    }
                }
            },
            ConfirmPassword: {
                validators: {
                    notEmpty: {
                        message: 'The confirm password is required and can\'t be empty'
                    },
                    identical: {
                        field: 'password',
                        message: 'The password and its confirm are not the same'
                    }
                }
            },
            
            FullName: {
                validators: {
                    notEmpty: {
                        message: 'The Full Name is required and can\'t be empty'
                    },
                }
            },
           
           
            Terms: {
                validators: {
                    notEmpty: {
                        message: 'You must agree with the terms and conditions'
                    }
                }
            },
           
            Gender: {
                validators: {
                    notEmpty: {
                        message: 'The Gender is required and can\'t be empty'
                    }
                }
            },
            Birthday: {
                validators: {
                    notEmpty: {
                        message: 'The Birthday is required and can\'t be empty'
                    }
                }
            },
            IC: {
                validators: {
                    notEmpty: {
                        message: 'The IC No is required and can\'t be empty'
                    }
                }
            }
        }
    });
}
//
// Doctor create form validator function
//
function doctorCreateValidator() {
    $('form#doctorCreate').bootstrapValidator({
        message: 'This value is not valid',
        fields: {
            Email: {
                validators: {
                    notEmpty: {
                        message: 'The email address is required and can\'t be empty'
                    },
                    emailAddress: {
                        message: 'The input is not a valid email address'
                    }
                }
            },
            Password: {
                validators: {
                    notEmpty: {
                        message: 'The password is required and can\'t be empty'
                    },
                    identical: {
                        field: 'confirmPassword',
                        message: 'The password and its confirm are not the same'
                    }
                }
            },
            ConfirmPassword: {
                validators: {
                    notEmpty: {
                        message: 'The confirm password is required and can\'t be empty'
                    },
                    identical: {
                        field: 'password',
                        message: 'The password and its confirm are not the same'
                    }
                }
            },
            Practicing: {
                validators: {
                    notEmpty: {
                        message: 'The Practicing Since is required and can\'t be empty'
                    },
                }
            }
        }
    });
}
/*-------------------------------------------
    Function for Partner page (form partner/index)
---------------------------------------------*/
//
// Doctor edit form validator function
//
function PartnerValidator() {
    $('form#partnerEdit, form#partnerCreate').bootstrapValidator({
        message: 'This value is not valid',
        fields: {
            Email: {
                validators: {
                    notEmpty: {
                        message: 'The email address is required and can\'t be empty'
                    },
                    emailAddress: {
                        message: 'The input is not a valid email address'
                    },
                    remote: {
                        message: 'this email is already registered in our system as user or HCP. Please use other email.',
                        url: '/Partner/ValidateEmail',
                        type: 'POST',
                        data: function (validator, $field, value) {
                            return {
                                email: validator.getFieldElements('Email').val()
                            };
                        },
                    }
                }
            },
            Password: {
                validators: {
                    notEmpty: {
                        message: 'The password is required and can\'t be empty'
                    },
                    identical: {
                        field: 'confirmPassword',
                        message: 'The password and its confirm are not the same'
                    }
                }
            },
            ConfirmPassword: {
                validators: {
                    notEmpty: {
                        message: 'The confirm password is required and can\'t be empty'
                    },
                    identical: {
                        field: 'password',
                        message: 'The password and its confirm are not the same'
                    }
                }
            }
        }
    });
}
//
// Doctor create form validator function
//
function partnerCreateValidator() {
    $('form#partnerCreate').bootstrapValidator({
        message: 'This value is not valid',
        fields: {
            Email: {
                validators: {
                    notEmpty: {
                        message: 'The email address is required and can\'t be empty'
                    },
                    emailAddress: {
                        message: 'The input is not a valid email address'
                    }
                }
            },
            Password: {
                validators: {
                    notEmpty: {
                        message: 'The password is required and can\'t be empty'
                    },
                    identical: {
                        field: 'confirmPassword',
                        message: 'The password and its confirm are not the same'
                    }
                }
            },
            ConfirmPassword: {
                validators: {
                    notEmpty: {
                        message: 'The confirm password is required and can\'t be empty'
                    },
                    identical: {
                        field: 'password',
                        message: 'The password and its confirm are not the same'
                    }
                }
            }
        }
    });
}
/*-------------------------------------------
    Function for Notification page (form notification/create)
---------------------------------------------*/
//
// Notification create form validator function
//
function NotificationValidator() {
    $('#msgCreate form').bootstrapValidator({
        message: 'This value is not valid',
        fields: {
            Text: {
                message: 'The text is not valid',
                validators: {
                    notEmpty: {
                        message: 'The text is required and can\'t be empty'
                    },
                }
            },
        }
    });
}
/*-------------------------------------------
    Function for Setting Password page
---------------------------------------------*/
//
// Setting password form validator function
//
function PasswordValidator() {
    $('#setPassword form').bootstrapValidator({
        message: 'This value is not valid',
        fields: {
            OldPassword: {
                message: 'The old password is not valid',
                validators: {
                    notEmpty: {
                        message: 'The old password is required and can\'t be empty'
                    },
                }
            },
            NewPassword: {
                message: 'The new password is not valid',
                validators: {
                    notEmpty: {
                        message: 'The new password is required and can\'t be empty'
                    },
                    identical: {
                        field: 'ConfirmPassword',
                        message: 'The new password and its confirm are not the same'
                    }
                }
            },
            ConfirmPassword: {
                message: 'The confirm password is not valid',
                validators: {
                    notEmpty: {
                        message: 'The confirm password is required and can\'t be empty'
                    },
                    identical: {
                        field: 'NewPassword',
                        message: 'The new password and its confirm are not the same'
                    }
                }
            },
        }
    });
}

//
// Validator for start and end date inputs (Prescription/AllPrescriptions and PrescriptionRequest/RequestList)
//
function DateRangeInputValidator(form) {
    $(form).bootstrapValidator({
        fields: {
            startDate: {
                validators: {
                    notEmpty: {
                        message: 'Fill in Start Date'
                    },
                    date: {
                        format: 'YYYY-MM-DD',
                        separator: '-',
                        message: 'Invalid date or format, please use yyyy-mm-dd format'
                    },
                    callback: {
                        callback: function(value, validator, $field) {
                            const startDate = moment(value, 'YYYY-M-D');
                            const endDate = moment(validator.getFieldElements('endDate').val(), 'YYYY-M-D');
                            if (!endDate.isValid()) {
                                return true;
                            }

                            if (startDate.isAfter(endDate)) {
                                return {
                                    valid: false,
                                    message: 'Start Date must be earlier or same day as End Date'
                                };
                            } else if (startDate.clone().add(366, 'days').isSameOrBefore(endDate)) {
                                return {
                                    valid: false,
                                    message: 'Start Date and End Date must be less than 366 days apart'
                                };
                            }

                            return true;
                        }
                    }
                }
            },
            endDate: {
                validators: {
                    notEmpty: {
                        message: 'Fill in End Date'
                    },
                    date: {
                        format: 'YYYY-MM-DD',
                        separator: '-',
                        message: 'Invalid date or format'
                    },
                    callback: {
                        callback: function(value, validator, $field) {
                            validator.revalidateField('startDate');
                            return true;
                        }
                    }
                }
            }
        }
    });
}
/*-------------------------------------------
    Functions for Progressbar page (ui_progressbars.html)
---------------------------------------------*/
//
// Function for Knob clock
//
function RunClock() {
    var second = $(".second");
    var minute = $(".minute");
    var hour = $(".hour");
    var d = new Date();
    var s = d.getSeconds();
    var m = d.getMinutes();
    var h = d.getHours();
    if (h > 11) { h = h - 12; }
    $('#knob-clock-value').html(h + ':' + m + ':' + s);
    second.val(s).trigger("change");
    minute.val(m).trigger("change");
    hour.val(h).trigger("change");
}
//
// Function for create test sliders on Progressbar page
//
function CreateAllSliders() {
    $(".slider-default").slider();
    var slider_range_min_amount = $(".slider-range-min-amount");
    var slider_range_min = $(".slider-range-min");
    var slider_range_max = $(".slider-range-max");
    var slider_range_max_amount = $(".slider-range-max-amount");
    var slider_range = $(".slider-range");
    var slider_range_amount = $(".slider-range-amount");
    slider_range_min.slider({
        range: "min",
        value: 37,
        min: 1,
        max: 700,
        slide: function (event, ui) {
            slider_range_min_amount.val("$" + ui.value);
        }
    });
    slider_range_min_amount.val("$" + slider_range_min.slider("value"));
    slider_range_max.slider({
        range: "max",
        min: 1,
        max: 100,
        value: 2,
        slide: function (event, ui) {
            slider_range_max_amount.val(ui.value);
        }
    });
    slider_range_max_amount.val(slider_range_max.slider("value"));
    slider_range.slider({
        range: true,
        min: 0,
        max: 500,
        values: [75, 300],
        slide: function (event, ui) {
            slider_range_amount.val("$" + ui.values[0] + " - $" + ui.values[1]);
        }
    });
    slider_range_amount.val("$" + slider_range.slider("values", 0) +
        " - $" + slider_range.slider("values", 1));
    $("#equalizer > div.progress > div").each(function () {
        // read initial values from markup and remove that
        var value = parseInt($(this).text(), 10);
        $(this).empty().slider({
            value: value,
            range: "min",
            animate: true,
            orientation: "vertical"
        });
    });
}
/*-------------------------------------------
    Function for jQuery-UI page (ui_jquery-ui.html)
---------------------------------------------*/
//
// Function for make all Date-Time pickers on page
//
function AllTimePickers() {
    $('#datetime_example').datetimepicker({});
    $('#time_example').timepicker({
        hourGrid: 4,
        minuteGrid: 10,
        timeFormat: 'hh:mm tt'
    });
    $('#date3_example').datepicker({ numberOfMonths: 3, showButtonPanel: true });
    $('#date3-1_example').datepicker({ numberOfMonths: 3, showButtonPanel: true });
    $('#date_example').datepicker({});
}

/*-------------------------------------------
	Scripts for page
---------------------------------------------*/
function LoadUserIndexScripts() {
    $(document).ready(function () {
        // Load Datatables and run plugin on tables
        LoadDataTablesScripts(function () {         
            var oTable = $('table#userList').DataTable({
                "pageLength":20,
                "processing": true,
                "serverSide": true,
                "ordering": true,
                "order": [[2, "asc"]],
                "bProcessing": true,
                "scrollX": true,
                "dom": "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/User/GetList',
                    "dataType": 'json',
                    "contentType": 'application/json; charset=utf-8',
                    "type": "POST",
                    "data": function (d, settings) {
                        d.Email = $('#Email').val();
                        d.Name = $('#Name').val();
                        d.UserType = $('#UserType').val();
                        d.CompanyId = $('#ddl_companies').val();
                        return JSON.stringify(d);
                    },
                },
                "columns": [
                    {
                        "class": "details-control",
                        "orderable": false,
                        "data": null,
                        "defaultContent": ""
                    },
                    {
                        "class": "rank",
                        "data": null, // #
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "name": "FullNameAndUserName",
                        "data": 'FullName',
                        "orderable": true,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var imgSrc = (row.Photo != null && row.Photo.ThumbnailUrl !== null) ? row.Photo.ThumbnailUrl : "../Images/user.png";
                            return '<img class="img-rounded" src="' + imgSrc + '" alt=""> ' + data + " ( " + row.UserName + " )";
                        }
                    },
                    {
                        "data": 'Gender',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var gender;
                            switch (data) {
                                case 1:
                                    gender = 'Male';
                                    break;
                                case 2:
                                    gender = 'Female';
                                    break;
                            }
                            return gender;
                        }
                    },
                    {
                        "data": 'Country.Name',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            if (row.IsCorporate == true) {
                                return row.CorporateName;
                            }
                            return data;
                        }
                    },
                    {
                        "name": "CreateDate",
                        "data": function (row, type, set, meta) {
                            var d = moment(row.CreateDate);
                            // Some users didn't have CreateDate recorded properly,
                            // so they end up having the database default value, which is 0001-01-01 00:00:00
                            if (d.isSame('0001-01-01T00:00:00Z')) {
                                return 'N/A';
                            }

                            var dateString = d.format('YYYY/MM/DD HH:mm');
                            return dateString;
                        },
                        "orderable": true,
                        "orderSequence": ['desc', 'asc']
                    },
                    {
                        "data": null, // #
                        "orderable": false,
                        "className": 'remark',
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var html = '';
                            html += `<a href="/User/Edit?userId=${ row.UserId }"><i class="fa fa-pencil-square-o"></i></a>`;
                            if (!data.IsVerified) {
                                html += `&nbsp;&nbsp;<a href="#" class="Verify" data-userId="${ row.UserId }" data-verify="true">Verify</a>`;
                            }
                            if (!data.IsBan) {
                                html += `&nbsp;&nbsp;<a href="#" class="ban" data-userId="${ row.UserId }" data-ban="true">Ban</a>`;
                            }
                            else {
                                html += `&nbsp;&nbsp;<a href="#" class="ban" data-userId="${ row.UserId }" data-ban="false">Un-ban</a>`;
                            }
                            html += `&nbsp;&nbsp;<a href="#" class="del" data-userId="${ row.UserId }"><i class="fa fa-trash-o"></i></a>`;

                            return html;
                        },
                    },

                ],
                "rowCallback": function (row, data) {
                    if (data.IsBan) {
                        $(row).addClass("danger");
                    }
                },
            });

            // Index column
            oTable.on('draw', function () {
                var start = oTable.page.info().start;
                oTable.column(1).nodes().each(function (cell, i) {
                    cell.innerHTML = start + i + 1;
                });
            });

            // Add event listener for Ban an Unban user
            $('table#userList tbody').on('click', 'a.ban', function () {
                var $this = $(this);
                var userId = $this.attr('data-userId');

                if ($this.attr('data-ban') == 'true') {
                    $.post('/user/ban?userId=' + userId, function () {
                        $this.closest('tr').addClass('danger');
                        $this.attr('data-ban', 'false').html('Un-ban');
                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
                else {
                    $.post('/user/unban?userId=' + userId, function () {
                        $this.closest('tr').removeClass('danger');
                        $this.attr('data-ban', 'true').html('Ban');
                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
            });
            // Add event listener for Verify User
            $('table#userList tbody').on('click', 'a.Verify', function () {
                var $this = $(this);
                var userId = $this.attr('data-userId');
                $.post('/user/Verify?userId=' + userId, function () {
                    $this.remove();
                    alert("User Verify Confirm");
                }).error(function (jsonError) {
                    alert("Unable to verify User");
                });
            });
            $('table#userList tbody').on('click', 'a.del', function () {
                var $this = $(this);
                var userId = $this.attr('data-userId');

                if (confirm("Are you sure you want to delete this doctor?")) {
                    $.post('/User/Delete?userId=' + userId, function () {
                        window.location.reload();
                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
            });

            // Row Detail start
            // Add event listener for opening and closing details
            function format(d) {
                var html = '';
                if (d.IsCorporate == false) {
                    html =
                        '<div class="detail">' +
                        '<div class="row">' +
                        '<div class="col-sm-3"><span><b>User ID:</b></span>' + d.UserId + '</div>' +
                        '</div>' +
                        '<hr/>' +
                        '<div class="row">' +
                        '<div class="col-sm-3"><span>Email:</span>' + d.UserName + '</div>' +
                        '<div class="col-sm-3"><span>Birthday:</span>' + moment(d.Birthday).format('YYYY/MM/DD') + '</div>' +
                        '<div class="col-sm-3"><span>Country:</span>' + d.Country.Name + '</div>' +
                        '<div class="col-sm-3"><span>Language:</span>' + d.Language + '</div>' +
                        '</div>' +
                        '<hr/>' +
                        '<div class="row">' +
                        '<div class="col-sm-3"><span>Package:</span>' + (d.Package != null ? d.Package.Title : "") + '</div>' +
                        '</div>' +
                        '<hr/>' +
                        '<div class="row">' +
                        '<div class="col-sm-3"><span>Weight:</span>' + (d.Weight != null ? d.Weight : "") + '</div>' +
                        '<div class="col-sm-3"><span>BMI:</span>' + (d.BMI != null ? d.BMI : "") + '</div>' +
                        '<div class="col-sm-3"><span>Body Temperature:</span>' + (d.BodyTemperature != null ? d.BodyTemperature : "") + '</div>' +
                        '<div class="col-sm-3"><span>Heart Rate:</span>' + (d.HeartRate != null ? d.HeartRate : "") + '</div>' +
                        '<div class="col-sm-3"><span>Blood Pressure:</span>' + (d.BloodPressure != null ? d.BloodPressure : "") + '</div>' +
                        '<div class="col-sm-3"><span>Blood Gluccose Fasting:</span>' + (d.BloodGluccoseFasting != null ? d.BloodGluccoseFasting : "") + '</div>' +
                        '<div class="col-sm-3"><span>Blood Gluccose:</span>' + (d.BloodGluccose != null ? d.BloodGluccose : "") + '</div>' +
                        '</div>' +
                        '</div>';
                }
                else {
                    html =
                        '<div class="detail">' +
                        '<div class="row">' +
                        '<div class="col-sm-3"><span><b>User ID:</b></span>' + d.UserId + '</div>' +
                        '</div>' +
                        '<hr/>' +
                        '<div class="row">' +
                        '<div class="col-sm-3"><span>Email:</span>' + d.UserName + '</div>' +
                        '</div>' +
                        '<hr/>' +
                        '<div class="row">' +
                        '<div class="col-sm-3"><span>Coporate :</span>' + (d.CorporateName != null ? d.CorporateName : "") + '</div>' +
                        '<div class="col-sm-6"><span>Branch :</span>' + (d.BranchName != null ? d.BranchName : "") + '<br />' + (d.BranchAdress != null ? d.BranchAdress : "") + ' </div>' +
                        '</div>' +
                        '</div>';
                }

                return html;
            }
            // Array to track the ids of the details displayed rows
            var detailRows = [];
            $('table#userList tbody').on('click', 'tr td.details-control', function () {
                var tr = $(this).closest('tr');
                var row = oTable.row(tr);
                var idx = $.inArray(tr.attr('id'), detailRows);

                if (row.child.isShown()) {
                    tr.removeClass('details');
                    row.child.hide();

                    // Remove from the 'open' array
                    detailRows.splice(idx, 1);
                }
                else {
                    tr.addClass('details');
                    row.child(format(row.data())).show();

                    // Add to the 'open' array
                    if (idx === -1) {
                        detailRows.push(tr.attr('id'));
                    }
                }
            });
            // On each draw, loop over the `detailRows` array and show any child rows
            oTable.on('draw', function () {
                $.each(detailRows, function (i, id) {
                    $('#' + id + ' td.details-control').trigger('click');
                });
            });
            // Row Detail end
            $('#search').click(function () {
                oTable.ajax.reload();
            });
        });
        WinMove();
        $('.download-link:first').click(function () {
            var downloadURL = function downloadURL(url) {
                var hiddenIFrameID = 'hiddenDownloader',
                    iframe = document.getElementById(hiddenIFrameID);
                if (iframe === null) {
                    iframe = document.createElement('iframe');
                    iframe.id = hiddenIFrameID;
                    iframe.style.display = 'none';
                    document.body.appendChild(iframe);
                }
                iframe.src = url;
            };
            downloadURL('/User/Export');
        });
    });
}
function LoadVaccinatedUserForApprovalScripts() {
    $(document).ready(function () {
        // Load Datatables and run plugin on tables
        
        LoadDataTablesScripts(function () {
            var oTable = $('table#vaccinatedUserTable').DataTable({
                "pageLength": 20,
                "processing": true,
                "serverSide": true,               
                "bProcessing": true,
                "scrollX": true,
                "dom": "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/Vaccine/GetVaccineInfoListForApproval',
                    "dataType": 'json',
                    "contentType": 'application/json; charset=utf-8',
                    "type": "POST",
                    "data": function (d, settings) {
                        //d.Email = $('#Email').val();
                        d.Name = $('#Name').val();
                        d.Status = $('#ddlStatus').val();

                        return JSON.stringify(d);
                    },
                },
                "columns": [
                    {
                        "name": "RecordID",
                        "data": 'Vaccination_Id'
                    },
                 
                    {
                        "name": "Name",
                        "data": 'VUserName'
                    },
                    {
                        "name": "Dose",
                        "data": 'Dose'
                    },
                    {
                        "name": "Status",
                        "data": 'Status'
                    },
                    {
                        "name": "Reject Reason",
                        "data": 'RemarkReason'
                    },
                    {
                        "name": "Submitted Date",                
                        "data": function (row, type, set, meta) {
                            var d = moment(row.CreatedDate);
                            var dateString = d.format('YYYY/MM/DD HH:mm');
                            return dateString;
                        },
                    },
                    {
                        "data": null, // #
                    
                        "className": 'remark',
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var html = '';
                            html += `<a href="/Vaccine/VaccineApprovalDetail?Vaccination_Id=${row.Vaccination_Id}">Details</a>`;
                            return html;
                        },
                    },
             
            
               
                    

                ],
              
            });


            // Array to track the ids of the details displayed rows
           
            // Row Detail end
            $('#search').click(function () {
                oTable.ajax.reload();
            });
        });
        //WinMove();
        
    });
}
function LoadMedicationIndexScripts() {
    $(document).ready(function () {
        // Load Datatables and run plugin on tables
        LoadDataTablesScripts(function () {
            var oTable = $('table#medList').DataTable({
                "processing": true,
                "serverSide": true,
                "ordering": false,
                "bProcessing": true,
                "scrollX": true,
                "dom": "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/Medication/GetList',
                    "dataType": 'json',
                    "type": "POST",
                    "data": function (d) {
                        d.searchKey = $("#searchName").val()
                    },
                },
                "columns": [
                    {
                        "class": "rank",
                        "data": null, // #
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'MedicationName',
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'CreatedDate',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            return moment(data).format('DD MMMM YYYY');
                        },
                    },
                    {
                        "data": null, // #
                        "orderable": false,
                        "className": 'remark',
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var html = '';
                            html += '<a href="/Medication/Edit?medicationId=' + row.MedicationId + '">Edit</a>';
                            html += ' <a href="#" class="del" data-medicationId="' + row.MedicationId + '">Delete</a>';
                            return html;
                        },
                    },
                ],
                "rowCallback": function (row, data) {
                    if (data.IsBan) {
                        $(row).addClass("danger");
                    }
                },
            });

            // Index column
            oTable.on('draw', function () {
                var start = oTable.page.info().start;
                oTable.column(0).nodes().each(function (cell, i) {
                    cell.innerHTML = start + i + 1;
                });
            });

            // Add event listener for Ban an Unban user
            $('table#medList tbody').on('click', 'a.del', function () {
                var $this = $(this);
                var medicationId = $this.attr('data-medicationId');

                if (confirm("Are you sure you want to delete this Medication?")) {
                    $.post('/Medication/Delete?medicationId=' + medicationId, function () {
                        window.location.reload();
                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
            });

            $("#searchBtn").on('click', function () {
                oTable.ajax.reload(null, true);
            });

        });
        WinMove();
    });
}
function LoadMedicationEditScripts() {
    $(document).ready(function () {

        var $tr;
        LoadAvatar(function () {
            $('#medEdit .thumbnail-wapper').avatar();
        });

        // Load doctor edit form validation
        //LoadBootstrapValidatorScript(DoctorValidator);
    });
}
function LoadMedicationAddScripts() {
    LoadAvatar(function () {
        $('#medAdd .thumbnail-wapper').avatar();
    });
}
function LoadDoctorIndexScripts() {
    
    $(document).ready(function () {
        var oTable = "";
        // Load Datatables and run plugin on tables
        LoadDataTablesScripts(function () {
      
            oTable = $('table#doctorList').DataTable({
                "pageLength": 20,
                "processing": true,
                "serverSide": true,
                "ordering": true,
                "order": [[3, "desc"]],
                "bProcessing": true,
                "scrollX": true,
                "dom": "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/Doctor/GetList',
                    "dataType": 'json',
                    "contentType": 'application/json; charset=utf-8',
                    "type": "POST",
                    "data": function (d) {
                        d.doctorType = $("#ddl_doctor").val();
                        d.category = $("#ddl_category").val();
                        d.searchKey = $("#searchName").val();
                        d.groupId = $("#ddl_group").val();
                        d.companyId = $("#ddl_companies").val();
                        return JSON.stringify(d);
                    },
                },
                "columns": [
                    {
                        "class": "details-control",
                        "orderable": false,
                        "data": null,
                        "defaultContent": ""
                    },
                    {
                        "class": "rank",
                        "data": null, // #
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "name": "FullName",
                        "data": 'FullName',
                        "orderable": true,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var imgSrc = (row.Photo != null && row.Photo.ThumbnailUrl !== null) ? row.Photo.ThumbnailUrl : "../Images/user.png";
                            return '<img class="img-rounded" src="' + imgSrc + '" alt=""> ' + data;
                        }
                    },
                    {
                        "name": "CreateDate",
                        "data": 'CreateDate',
                        "orderable": true,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            return '<span>' + moment(data).format('YYYY/MM/DD') + ' </span>';
                        }
                    },
                    {
                        "data": 'Gender',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var gender;
                            switch (data) {
                                case 1:
                                    gender = 'Male';
                                    break;
                                case 2:
                                    gender = 'Female';
                                    break;
                            }
                            return gender;
                        }
                    },
                    {
                        "data": null, // #
                        "orderable": false,
                        "className": 'remark',
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var html = '';
                            html += '<a href="/Doctor/Edit?doctorId=' + row.DoctorId + '">Edit</a>';
                            html += ' | <a href="/Doctor/Reviews?doctorId=' + row.DoctorId + '">Reviews</a>';
                            html += ' | <a href="#" class="del" data-userId="' + row.UserId + '">Delete</a>';
                            if (!row.IsBan) {
                                html += ' | <a href="#" class="ban" data-doctorId="' + row.DoctorId + '" data-ban="true">Ban</a>';
                            }
                            else {
                                html += ' | <a href="#" class="ban" data-doctorId="' + row.DoctorId + '" data-ban="false">Un-ban</a>';
                            }
                            if (!row.IsVerified) {
                                html += ' | <a href="javascript:void(0);" class="verify" data-doctorId="' + row.DoctorId + '"> Verify</a>';
                            }
                            return html;
                        },
                    },
                ],
                "createdRow": function (row, data, dataIndex) {
                    if (data.IsBan) {
                        $(row).addClass("danger");
                    }
                }
            });

            // Index column
            oTable.on('draw', function () {
                var start = oTable.page.info().start;
                oTable.column(1).nodes().each(function (cell, i) {
                    cell.innerHTML = start + i + 1;
                });
            });


            // Add event listener for Ban an Unban user
            $('table#doctorList tbody').on('click', 'a.ban', function () {
                var $this = $(this);
                var doctorId = $this.attr('data-doctorId');

                if ($this.attr('data-ban') == 'true') {
                    $.post('/doctor/ban?doctorId=' + doctorId, function () {
                        $this.closest('tr').addClass('danger');
                        $this.attr('data-ban', 'false').html('Un-ban');
                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
                else {
                    $.post('/doctor/unban?doctorId=' + doctorId, function () {
                        $this.closest('tr').removeClass('danger');
                        $this.attr('data-ban', 'true').html('Ban');
                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
            });

            // Add event listener for Ban an Unban user
            $('table#doctorList tbody').on('click', 'a.del', function () {
                var $this = $(this);
                var userId = $this.attr('data-userId');

                if (confirm("Are you sure you want to delete this doctor?")) {
                    $.post('/Doctor/Delete?doctorId=' + userId, function () {
                        oTable.ajax.reload(null, false);
                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
            });

            //$(".ddl_hcp").on('change', function () {
            //    oTable.ajax.reload(null, false);
            //});

            $("#ddl_category").on('change', function () {
                var categoryId = $(this).val();
                $.get('/Group/GetGroupDDL?categoryId=' + categoryId, function (data) {
                    $("#ddl_group").empty();
                    $("<option />", {
                        val: -1,
                        text: "All"
                    }).appendTo("#ddl_group");
                    for (var i = 0; i < data.length; i++) {
                        $("<option />", {
                            val: data[i].GroupId,
                            text: data[i].GroupName
                        }).appendTo("#ddl_group");
                    }
                }).error(function (jsonError) {
                    alert(jsonError);
                });
            });


            $("#searchBtn").on('click', function () {
                oTable.ajax.reload(null, true);
            });

            // Row Detail start
            // Add event listener for opening and closing details
            function format(d) {
                var html = '<div class="detail"><b>Doctor ID:</b> ' + d.DoctorId + '<br/>' +
                    '<b>Email:</b> ' + d.UserName + '<br/>' +
                    '<b>Birthday:</b> <span>' + moment(d.Birthday).format('YYYY/MM/DD') + '<br/>' +
                    '<b>Country:</b> ' + d.Country.Name + '<br/>' +
                    '<b>Language:</b> ' + d.Language + '<br/>' +
                    '<b>Number of Cancellation:</b> ' + d.NumberOfCancelRequest + '<br/>';
                if (d.Certificate) {
                    html += '<b>Certificate : </b><a target="_blank" href="' + d.Certificate + '" >Click Here<a/>';
                }
                html += '</div>';
                return html;
            }
            // Array to track the ids of the details displayed rows
            var detailRows = [];
            $('table#doctorList tbody').on('click', 'tr td.details-control', function () {
                var tr = $(this).closest('tr');
                var row = oTable.row(tr);
                var idx = $.inArray(tr.attr('id'), detailRows);

                if (row.child.isShown()) {
                    tr.removeClass('details');
                    row.child.hide();

                    // Remove from the 'open' array
                    detailRows.splice(idx, 1);
                }
                else {
                    tr.addClass('details');
                    row.child(format(row.data())).show();

                    // Add to the 'open' array
                    if (idx === -1) {
                        detailRows.push(tr.attr('id'));
                    }
                }
            });

            $('table#doctorList tbody').on('click', 'a.verify', function () {
                var $this = $(this);
                var userId = $this.attr('data-doctorId');

                if (confirm("Are you sure you want to verify this doctor?")) {
                    $.post('/Doctor/Verify?doctorId=' + userId, function () {
                        oTable.ajax.reload(null, false);
                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
            });

            // On each draw, loop over the `detailRows` array and show any child rows
            oTable.on('draw', function () {
                $.each(detailRows, function (i, id) {
                    $('#' + id + ' td.details-control').trigger('click');
                });
            });
            // Row Detail end
        });

        //function OpenPopup(dataObj) {
        //    $.magnificPopup.open({
        //        items: {
        //            src: '#dutyDoctor',
        //            type: 'inline',
        //        },
        //        preloader: false,
        //        focus: '#Cancel',


        //        callbacks: {
        //            beforeOpen: function () {
        //                var img = "/Images/placeholder.png";
        //                if (dataObj.Photo) {
        //                    img = dataObj.Photo.ImageUrl;
        //                }
        //                var gender = "";
        //                switch (dataObj.Gender) {
        //                    case 1:
        //                        gender = 'Male';
        //                        break;
        //                    case 2:
        //                        gender = 'Female';
        //                        break;
        //                }
        //                $("#docimage").attr("src", img);
        //                $("#docname").html(dataObj.FullName);
        //                $("#docemail").html(dataObj.UserName);
        //                $("#docgender").html(gender);
        //                $("#doclanguage").html(dataObj.Language);
        //                $("#starttime").html(moment(dataObj.DutyDate).format("DD-MM-YYYY hh:mm A"));
        //                // set button done
        //                $('#dutyDoctor').find('button.done').unbind('click');
        //                $('#dutyDoctor').find('button.done').click(function (e) {
        //                    $.magnificPopup.close();
        //                });


        //                $('#dutyDoctor').find('button.remove').unbind('click');
        //                $('#dutyDoctor').find('button.remove').click(function (e) {
        //                    $.post('/doctor/RemoveDoctorFromDuty', function (data) {
        //                        if (data) {
        //                            oTable.ajax.reload(null, false);
        //                            $.magnificPopup.close();
        //                        }
        //                    }).error(function (jsonError) {
        //                        alert(jsonError);
        //                    });
        //                });
        //            }
        //        }
        //    });
        //}

        WinMove();
    });
}
function LoadDoctorEditScripts(selectedGroup) {
    $(document).ready(function () {

        var $tr;
        LoadAvatar(function () {
            $('#doctorEdit .thumbnail-wapper').avatar();
        });

        function RefreshGroup(categoryId, firstTime) {
            if (!categoryId) {
                $("#ddl_group").empty();
                return;
            }
            $.get('/Group/GetGroupDDL?categoryId=' + categoryId, function (data) {
                $("#ddl_group").empty();
                for (var i = 0; i < data.length; i++) {
                    $("<option />", {
                        val: data[i].GroupId,
                        text: data[i].GroupName
                    }).appendTo("#ddl_group");
                }
                if (firstTime) {
                    $("#ddl_group").val(selectedGroup);
                }
            }).error(function (jsonError) {
                alert(jsonError);
            });
        }

        $("#CategoryId").on('change', function () {
            var categoryId = $(this).val();
            RefreshGroup(categoryId, false);
        });


        // Load jQuery select2
        LoadSelect2Script(function () {
            //$('#Gender').select2({ placeholder: "Select Gender" });
            $('#CountryId').select2({ placeholder: "Select Country" });
            //$('#CategoryId').select2({ placeholder: "Select Category" });
            //$('#ddl_group').select2({ placeholder: "Select Group" });
        });
        RefreshGroup($('#CategoryId').val(), true);


        // Load jQuery DatetimePicker
        LoadDateTimePickerScript(function () {
            $('#doctorEdit .hasDatepicker').each(function () {
                if (this.id === "Birthday" || this.id === "practicing") {
                    var date = $(this).val();
                    if (date !== '') {
                        $(this).val(moment(date).format("YYYY/MM/DD"));
                    }
                }
                else {
                    $(this).val("");
                }
            });
            $('#doctorEdit .hasDatepicker').datetimepicker({
                timepicker: false,
                format: 'Y/m/d',
                onChangeDateTime: function (current_time, $input) {
                    $input.trigger('input');
                },
            });
        });

        // Load doctor edit form validation
        LoadBootstrapValidatorScript(DoctorValidator);

        //$('#doctorEdit')
        //    .on('show.bs.modal', function (event) {

        //        var $trigger = $(event.relatedTarget);
        //        $tr = $trigger.closest('tr');
        //        var $modal = $(this), doctorId = $trigger.attr('data-doctorId');
        //        var $body = $modal.find('.modal-body'), $form = $modal.find('form:first');
        //        $.get('/Doctor/GetEdit', {
        //            doctorId: doctorId,
        //        }, function (jsonDoctor) {
        //            var imgSrc = jsonDoctor.PhotoUrl != null ? jsonDoctor.PhotoUrl : "/Images/placeholder.png";
        //            $body.find('img:first').attr('src', imgSrc);
        //            $body.find('input[name="DoctorId"]').val(jsonDoctor.DoctorId);
        //            $body.find('input[name="Name"]').val(jsonDoctor.Name);
        //            $body.find('input[name="Description"]').val(jsonDoctor.Description);
        //            var $DOB = $body.find('input[name="DOB"]');
        //            if (jsonDoctor.DOB != "") {
        //                $DOB.val(moment(jsonDoctor.DOB).format('YYYY/MM/DD'));
        //            }
        //            $body.find('input[name="Phone"]').val(jsonDoctor.Phone);
        //            $body.find('input[name="PhotoUrl"]').val(jsonDoctor.PhotoUrl);
        //            $body.find('input[name="Email"]').val(jsonDoctor.Email);
        //            $body.find('input[name="LicenseNumber"]').val(jsonDoctor.LicenseNumber);
        //            $body.find('input[name="BoardCertifications"]').val(jsonDoctor.BoardCertifications);
        //            $body.find('input[name="Training"]').val(jsonDoctor.Training);
        //        }).error(function (json) {
        //            alert(json);
        //        });
        //    })
        //    .on('hide.bs.modal', function (event) {
        //        $('#doctorEdit form').resetForm();
        //    });
    });
}
function LoadDoctorCreateScripts() {
    function RefreshGroup(categoryId) {
        if (!categoryId) {
            $("#ddl_group").empty();
            return;
        }
        $.get('/Group/GetGroupDDL?categoryId=' + categoryId, function (data) {
            $("#ddl_group").empty();
            for (var i = 0; i < data.length; i++) {
                $("<option />", {
                    val: data[i].GroupId,
                    text: data[i].GroupName
                }).appendTo("#ddl_group");
            }
        }).error(function (jsonError) {
            alert(jsonError);
        });
    }

    $("#CategoryId").on('change', function () {
        var categoryId = $(this).val();
        RefreshGroup(categoryId);
    });

    LoadAvatar(function () {
        $('#doctorCreate .thumbnail-wapper').avatar();
    });

    // Load jQuery select2
    LoadSelect2Script(function () {
        $('#CountryId').select2({ placeholder: "Select Country" });
        $('#CategoryId').select2({ placeholder: "Select Category" });
    });

    RefreshGroup($('#CategoryId').val());

    // Load jQuery DatetimePicker
    $('#Practicing').val(moment().format("YYYY/MM/DD"));
    LoadDateTimePickerScript(function () {
        $('#doctorCreate .hasDatepicker').datetimepicker({
            timepicker: false,
            format: 'Y/m/d',
            onChangeDateTime: function (current_time, $input) {
                $input.trigger('input');
            },
        });
    });

    // Load doctor edit form validation
    LoadBootstrapValidatorScript(DoctorValidator);
}
function LoadDoctorReviewScripts(doctorId) {
    $(document).ready(function () {
        LoadDataTablesScripts(function () {
            tReviews = $('table#reviewList').DataTable({
                "processing": true,
                "serverSide": true,
                "ordering": false,
                "bProcessing": true,
                "scrollX": true,
                "dom": "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/Doctor/GetReviews?doctorId=' + doctorId,
                    "dataType": 'json',
                    "type": "POST",
                    //"data": function (d) {
                    //    d.doctorId = doctorId;
                    //},
                },
                "columns": [
                    {
                        "data": null, // #
                        "orderable": false,
                        "class": 'rank',
                        "defaultContent": "",
                    },
                    {
                        "data": 'Patient',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var imgSrc = "../Images/user.png";
                            if (data.Photo != null && data.Photo.ThumbnailUrl != null) {
                                imgSrc = data.Photo.ThumbnailUrl;
                            }
                            return '<img class="img-rounded photoUrl" src="' + imgSrc + '" alt=""> ' + data.FullName;
                        }
                    },
                    {
                        "data": 'Rating',
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'Comment',
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'CreateDate', // #
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            return '<span>' + moment(data).format('YYYY/MM/DD') + ' </span>';
                        },
                    },
                    {
                        "data": null, // #
                        "orderable": false,
                        "className": 'remark',
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            return '<a href="#" class="del" data-doctorId="' + row.DoctorId + '" data-userId="' + row.Patient.UserId + '">Delete</a>';
                        },
                    },
                ],
                "rowCallback": function (row, data) {
                    //if (data.IsBan) {
                    //    $(row).addClass("danger");
                    //}
                },
            });

            // Index column
            tReviews.on('draw.dt', function () {
                var start = tReviews.page.info().start;
                tReviews.column(0).nodes().each(function (cell, i) {
                    cell.innerHTML = start + i + 1;
                });
            });

            // Add event listener for delete reviews
            $('table#reviewList tbody').on('click', 'a.del', function () {
                var $this = $(this);
                var userId = $this.attr('data-userId'), doctorId = $this.attr('data-doctorId');

                $.post('/Doctor/DeleteReview', {
                    doctorId: doctorId,
                    patientId: userId,
                }, function () {
                    tReviews.ajax.reload(null, false);
                }).error(function (jsonError) {
                    alert(jsonError);
                });
            });
        });
    });
}
function LoadPartnerIndexScripts() {
    $(document).ready(function () {
        // Load Datatables and run plugin on tables
        LoadDataTablesScripts(function () {
            var oTable = $('table#partnerList').DataTable({
                "processing": true,
                "serverSide": true,
                "ordering": false,
                "bProcessing": true,
                "scrollX": true,
                "dom": "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/Partner/GetList',
                    "dataType": 'json',
                    "type": "POST",
                    "data": function (d) { },
                },
                "columns": [
                    {
                        "class": "details-control",
                        "orderable": false,
                        "data": null,
                        "defaultContent": ""
                    },
                    {
                        "class": "rank",
                        "data": null, // #
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'FullName',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var imgSrc = (row.Photo != null && row.Photo.ThumbnailUrl !== null) ? row.Photo.ThumbnailUrl : "../Images/user.png";
                            return '<img class="img-rounded" src="' + imgSrc + '" alt=""> ' + data;
                        }
                    },
                    {
                        "data": 'Gender',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var gender;
                            switch (data) {
                                case 1:
                                    gender = 'Male';
                                    break;
                                case 2:
                                    gender = 'Female';
                                    break;
                            }
                            return gender;
                        }
                    },
                    {
                        "data": null, // #
                        "orderable": false,
                        "className": 'remark',
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var html = '';
                            html += '<a href="/Partner/Edit?partnerId=' + row.UserId + '">Edit</a>';
                            html += ' <a href="#" class="del" data-userId="' + row.UserId + '">Delete</a>';
                            if (!row.IsBan) {
                                html += '<a href="#" class="ban" data-userId="' + row.UserId + '" data-ban="true"> Ban</a>';
                            }
                            else {
                                html += '<a href="#" class="ban" data-userId="' + row.UserId + '" data-ban="false"> Un-ban</a>';
                            }
                            return html;
                        },
                    },
                ],
                "rowCallback": function (row, data) {
                    if (data.IsBan) {
                        $(row).addClass("danger");
                    }
                },
            });

            // Index column
            oTable.on('draw', function () {
                var start = oTable.page.info().start;
                oTable.column(1).nodes().each(function (cell, i) {
                    cell.innerHTML = start + i + 1;
                });
            });


            // Add event listener for Ban an Unban user
            $('table#partnerList tbody').on('click', 'a.ban', function () {
                var $this = $(this);
                var doctorId = $this.attr('data-userId');

                if ($this.attr('data-ban') == 'true') {
                    $.post('/Partner/ban?partnerId=' + doctorId, function () {
                        $this.closest('tr').addClass('danger');
                        $this.attr('data-ban', 'false').html('Un-ban');
                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
                else {
                    $.post('/Partner/unban?partnerId=' + doctorId, function () {
                        $this.closest('tr').removeClass('danger');
                        $this.attr('data-ban', 'true').html('Ban');
                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
            });

            // Add event listener for Ban an Unban user
            $('table#partnerList tbody').on('click', 'a.del', function () {
                var $this = $(this);
                var userId = $this.attr('data-userId');

                if (confirm("Are you sure you want to delete this partner?")) {
                    $.post('/Partner/Delete?partnerId=' + userId, function () {
                        window.location.reload();
                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
            });

            // Row Detail start
            // Add event listener for opening and closing details
            function format(d) {
                return '<div class="detail"><b>Email:</b> ' + d.UserName + '<br/>' +
                    '<b>Birthday:</b> <span>' + moment(d.Birthday).format('YYYY/MM/DD') + '<br/>' +
                    '<b>Country:</b> ' + d.Country.Name + '<br/>';
            }
            // Array to track the ids of the details displayed rows
            var detailRows = [];
            $('table#partnerList tbody').on('click', 'tr td.details-control', function () {
                var tr = $(this).closest('tr');
                var row = oTable.row(tr);
                var idx = $.inArray(tr.attr('id'), detailRows);

                if (row.child.isShown()) {
                    tr.removeClass('details');
                    row.child.hide();

                    // Remove from the 'open' array
                    detailRows.splice(idx, 1);
                }
                else {
                    tr.addClass('details');
                    row.child(format(row.data())).show();

                    // Add to the 'open' array
                    if (idx === -1) {
                        detailRows.push(tr.attr('id'));
                    }
                }
            });
            // On each draw, loop over the `detailRows` array and show any child rows
            oTable.on('draw', function () {
                $.each(detailRows, function (i, id) {
                    $('#' + id + ' td.details-control').trigger('click');
                });
            });
            // Row Detail end
        });
        WinMove();
    });
}
function LoadPartnerCreateScripts() {
    LoadAvatar(function () {
        $('#partnerCreate .thumbnail-wapper').avatar();
    });

    // Load jQuery select2
    LoadSelect2Script(function () {
        $('#Gender').select2({ placeholder: "Select OS" });
        $('#CountryId').select2({ placeholder: "Select Country" });
        $('#CompanyId').select2({ placeholder: "Select Company" });
        $('#PositionId').select2({ placeholder: "Select Position" });
    });


    LoadDateTimePickerScript(function () {
        $('#partnerCreate .hasDatepicker').datetimepicker({
            timepicker: false,
            format: 'Y/m/d',
            onChangeDateTime: function (current_time, $input) {
                $input.trigger('input');
            },
        });
    });

    // Load doctor edit form validation
    LoadBootstrapValidatorScript(PartnerValidator);
}
function LoadPartnerEditScripts() {
    $(document).ready(function () {

        var $tr;
        LoadAvatar(function () {
            $('#partnerEdit .thumbnail-wapper').avatar();
        });

        // Load jQuery select2
        LoadSelect2Script(function () {
            $('#Gender').select2({ placeholder: "Select OS" });
            $('#CountryId').select2({ placeholder: "Select Country" });
            $('#CompanyId').select2({ placeholder: "Select Company" });
            $('#PositionId').select2({ placeholder: "Select Position" });
        });

        // Load jQuery DatetimePicker
        LoadDateTimePickerScript(function () {
            $('#partnerEdit .hasDatepicker').each(function () {
                var date = $(this).val();
                if (date != '') {
                    $(this).val(moment(date).format("YYYY/MM/DD"));
                }
            });
            $('#partnerEdit .hasDatepicker').datetimepicker({
                timepicker: false,
                format: 'Y/m/d',
                onChangeDateTime: function (current_time, $input) {
                    $input.trigger('input');
                },
            });
        });


        // Load doctor edit form validation
        LoadBootstrapValidatorScript(PartnerValidator);



    });
}
function LoadChatIndexScripts() {
    $(document).ready(function () {
        //added this condition so only chat related with login doctor will show
        if ($('#doctor').val() == '') {           
            $("#doctor")[0].selectedIndex = 1;
        }

        LoadAvatar(function () {
            $(' .thumbnail-wapper').avatar();
        });
        $('#File').change(function () {
            var reader = new FileReader();
            if (this.files && this.files[0]) {
                var reader = new FileReader();
                reader.onload = function (e) {
                    imgObj = e.target.result;
                };
                reader.readAsDataURL(this.files[0]);
                //    imgObj.push(this.files[0]);
            }


        });
        function readURL(input) {
            if (input.files && input.files[0]) {
                reader.readAsDataURL(input.files[0]);
            }
        }
        var doctor, patient;
        // Load jQuery select2
        LoadSelect2Script(function () {
            $('#doctor').select2({ allowClear: true, placeholder: "Select Doctor" });
            $('#ReplyMode').select2({ placeholder: "Select Reply" });
            $('#ReplyMode').change();
        });

        $('#textReply').keypress(function (e) {
            var key = e.which;
            if (key == 13) {
                $('#Reply').click();
                return false;
            }
        })
        $('#ReplyMode').change(function () {
            if ($(this).val() == "1") {
                $('#txtReply').css('display', 'none');
                $('#textReply').val('');
                $('#imgRply').css('display', 'block');
            }
            else {
                $('#txtReply').css('display', 'block');
                $('#imgRply').css('display', 'none');
                $('#innerImg').attr('src', '/Images/placeholder.png');
                $('#File').val('');
                imgObj = '';
            }

        });
        // Load Datatables and run plugin on tables
        LoadDataTablesScripts(function () {        

            var tChat = $('table#chatList').DataTable({
                "processing": true,
                "serverSide": true,
                "stateSave": true,
                "ordering": false,
                "bProcessing": true,
                "scrollX": true,
                "dom": "rt<'box-content'<'col-sm-12'i><'col-sm-12 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/Chat/GetChatList?chatRoomId=0',
                    "dataType": 'json',
                    "type": "POST",
                    "data": function (d) {
                        //d.chatRoomId = 3;
                    },
                },
                "columns": [
                    {
                        "data": null,
                        "orderable": false,
                        "defaultContent": "",
                    },
                ],
                "rowCallback": function (row, data, index) {
                    var html = '', imgSrc = '';
                    var photo = data.Photo != null ? "<img style='width:150px;' src='" + data.Photo.ImageUrl + "' />" : "";
                    var msg = data.Message != null ? data.Message : "";

                    var delButton = '<a href="#" class="del"><i class="fa fa-times"></i></a> ';
                    if (data.IsDeleted) {
                        delButton = '<span style="font-size:10px;"><i>(Message is deleted)</i></span>';
                    }

                    var audio = data.Voice != null ? "<a href='" + data.Voice.VoiceFileUrl + "' target='_blank'>Audio</a>" : "";
                    if (data.FromUserId == doctor.UserId) {
                        imgSrc = doctor.Photo !== null ? doctor.Photo.ThumbnailUrl : "../Images/user.png";

                        html =
                            ('<div class="media doctor">' +
                                '<div class="media-left">' +
                                '<img class="img-rounded doctor" src="' + imgSrc + '" alt="">' +
                                '</div>' +
                                '<div class="media-body">' +
                                '<p class="media-heading">' +
                                '<strong class="primary-font">' + doctor.FullName + '</strong> ' +
                                '<small class="text-muted">' +
                                '<i class="fa fa-clock-o fa-fw"></i>' + moment(moment(data.CreateDate).format('MMMM Do YYYY, h:mm:ss a'), 'MMMM Do YYYY, h:mm:ss a').fromNow() +
                                '</small>' +
                                '</p>')
                        html += msg + photo + audio + delButton +
                            '</div>' +
                            '</div>';
                    }
                    else {
                        imgSrc = patient.Photo !== null ? patient.Photo.ThumbnailUrl : "../Images/user.png";
                        html =
                            ('<div class="media patient">' +
                                '<div class="media-body text-right">' +
                                '<div class="media-heading">' +
                                '<small class="text-muted">' +
                                '<i class="fa fa-clock-o fa-fw"></i>' + moment(moment(data.CreateDate).format('MMMM Do YYYY, h:mm:ss a'), 'MMMM Do YYYY, h:mm:ss a').fromNow() +
                                '</small>' +
                                '<strong class="primary-font">' + patient.FullName + '</strong>' +
                                '</div>')
                        if (isDoctor != "True") {
                            html += delButton + msg + photo + audio
                            '</div>' +
                                '<div class="media-right">' +
                                '<a href="#">' +
                                '<img class="img-rounded doctor" src="' + imgSrc + '" alt="">' +
                                '</a>' +
                                '</div>' +
                                '</div>';
                        }
                        else {
                            html += msg + photo + audio
                            '</div>' +
                                '<div class="media-right">' +
                                '<a href="#">' +
                                '<img class="img-rounded doctor" src="' + imgSrc + '" alt="">' +
                                '</a>' +
                                '</div>' +
                                '</div>';
                        }

                    }

                    $('td:eq(0)', row).html(html);

                    // Add event listener for delete chat
                    $('a.del', row).click(function () {
                        $(row).addClass('danger');
                        LoadBootbox(function () {
                            bootbox.dialog({
                                message: "You want to delete this item?",
                                title: "Are you sure",
                                buttons: {
                                    cancel: {
                                        label: "Cancel",
                                        className: "btn-default",
                                        callback: function () {
                                            $(row).removeClass('danger');
                                        }
                                    },
                                    del: {
                                        label: "Delete",
                                        className: "btn-danger",
                                        callback: function () {
                                            $.post('/Chat/DeleteChat', {
                                                chatId: data.ChatId,
                                            }, function () {
                                                $(row).remove();
                                                tChat.ajax.reload();
                                            }).error(function (jsonError) {
                                                alert('Unable To Delete The Chat!');
                                            });
                                        }
                                    },
                                }
                            });
                        });
                    });
                },

            });
            //   setInterval(function () { tChat.ajax.reload();}, 3000);
           
            var tChatRoom = $('table#chatRoomList').DataTable({
                "processing": true,
                "serverSide": true,
                "bProcessing": true,
                "scrollX": true,
                "stateSave": true,
                "dom": "rt<'box-content'<'col-sm-12'i><'col-sm-12 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/Chat/GetChatRoomList',
                    "dataType": 'json',
                    "type": "POST",
                    "data": function (d) {
                        d.doctorId = $('#doctor').val(),                         
                            d.PatientNameOrEmail = $('#PatientNameOrEmail').val()
                    },
                },
                "columns": [
                    {
                        "class": "rank",
                        "data": null, // #
                        "orderable": false,
                        "defaultContent": "",
                    },

                    {
                        "data": 'Patient.FullName',
                        "orderable": true,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var imgSrc = row.Patient.Photo !== null ? row.Patient.Photo.ThumbnailUrl : "../Images/user.png";
                            return '<img class="img-rounded patient" src="' + imgSrc + '" alt=""> ' + data;
                        }
                    },
                    {
                        "data": 'Doctor.FullName',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var imgSrc = row.Doctor.Photo !== null ? row.Doctor.Photo.ThumbnailUrl : "../Images/user.png";
                            return '<img class="img-rounded doctor" src="' + imgSrc + '" alt=""> ' + data;
                        }
                    }
                    //{
                    //    "data": 'UserPackage',
                    //    "orderable": false,
                    //    "defaultContent": "",
                    //},
                ],
                "rowCallback": function (row, data) {
                    $(row).click(function () {
                        selectedRow = $(this).index();
                        doctor = data.Doctor;
                        patient = data.Patient;
                        patientId = data.Patient.UserId;
                        $(this).siblings('.primary').removeClass('primary');
                        $(this).addClass('primary');
                        chatRoomId = data.ChatRoomId;
                        tChat.ajax.url('/Chat/GetChatList?chatRoomId=' + data.ChatRoomId).load();
                    });
                },
            });

            // Index column
            tChatRoom.on('draw', function () {
                var start = tChatRoom.page.info().start;
                tChatRoom.column(0).nodes().each(function (cell, i) {
                    cell.innerHTML = start + i + 1;
                });
                if (firstLoad) {
                    $('#chatRoomList tbody tr:eq(0)').trigger('click');
                    firstLoad = false;
                }
                if ($('#chatRoomList tr').find('.dataTables_empty').length == 0) {
                    $('#chatRoomList tr').removeClass('primary')
                    $('#chatRoomList tr').eq(selectedRow + 2).addClass('primary')
                }

            });
            //setInterval(function () { tChatRoom.ajax.reload(); }, 5000);
            //setInterval(function () { tChat.ajax.reload(); }, 5000);


            //
            $('#doctor').change(function () {
                tChatRoom.ajax.reload();
                firstLoad = true;
                selectedRow = 0;
            });
            $('#search').click(function () {
                tChatRoom.ajax.reload();
                firstLoad = true;
                selectedRow = 0;
            });
            $('#Reply').click(function () {
                if ($('#ReplyMode').val() == "2" && $('#textReply').val() == '') {
                    alert('No Text');
                    return;
                }
                else if ($('#ReplyMode').val() == "1" && $('#File').val() == '') {
                    alert('No Image');
                    return;
                }
                $.ajax({
                    type: "POST",
                    url: '/Chat/DoctorChat',
                    data: { image: imgObj, text: $('#textReply').val(), chatRoomId: chatRoomId, patientId: patientId, doctorId: $('#doctor').val() },
                    success: function (msg) {
                        tChat.ajax.reload();
                        $('#textReply').val('');
                        $('#innerImg').attr('src', '/Images/placeholder.png');
                        $('#File').val('');
                        imgObj = '';
                    },
                    error: function (XMLHttpRequest, textStatus, errorThrown) {
                        alert("Error Occured Try again !");
                    }
                });
            });
        });
        WinMove();
    });
}
function LoadPackageIndexScripts() {
    $(document).ready(function () {

        //Load the DateTime Picker
        LoadDateTimePickerScript(function () {
            $('#endDate').each(function () {
                var date = $(this).val();
                if (date != '') {
                    $(this).val(moment(date).format("YYYY/MM/DD"));
                }
            });
            $('#endDate').datetimepicker({
                timepicker: false,
                format: 'Y/m/d', minDate: new Date(),
                onChangeDateTime: function (current_time, $input) {
                    $input.trigger('input');
                },
            });
        });
        // Load Datatables and run plugin on tables
        LoadDataTablesScripts(function () {
            var oTable = $('table#packageList').DataTable({
                "processing": true,
                "serverSide": true,
                "ordering": false,
                "bProcessing": true,
                "scrollX": true,
                "dom": "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/Package/GetList',
                    "dataType": 'json',
                    "type": "POST",
                    "data": function (d) {
                        d.Email = $('#Email').val()
                    },
                },
                "columns": [
                    {
                        "class": "details-control",
                        "orderable": false,
                        "data": null,
                        "defaultContent": ""
                    },
                    {
                        "class": "rank",
                        "data": null, // #
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'PatientPackageId',
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'Patient',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var imgSrc = data.Photo.ThumbnailUrl !== null ? data.Photo.ThumbnailUrl : "../Images/user.png";
                            return '<img class="img-rounded" src="' + imgSrc + '" alt=""> ' + data.FullName;
                        }
                    },
                    {
                        "data": 'Package.Title',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            if (row.Package.Type != 4) {
                                return data + " ( <a class='upgrade' href='javascript:void(0);' title='Upgrade to Premium Account' data-patientPackageId='" + row.PatientPackageId + "'>Upgrade</a> )";
                            }
                            else {
                                return data + " ( <a class='downgrade' href='javascript:void(0);' title='Downgrade to a Free Package' data-patientPackageId='" + row.PatientPackageId + "'>DownGrade</a> )";
                            }
                        }
                    },
                ],
                "rowCallback": function (row, data) { },
            });

            // Index column
            oTable.on('draw', function () {
                var start = oTable.page.info().start;
                oTable.column(1).nodes().each(function (cell, i) {
                    cell.innerHTML = start + i + 1;
                });
            });

            // Row Detail start
            // Add event listener for opening and closing details
            function format(d) {
                var giftUser = '', purchaser = '';
                if (d.GiftUser != null) {
                    giftUser = d.GiftUser.Photo.ThumbnailUrl !== null ? d.GiftUser.Photo.ThumbnailUrl : "../Images/user.png";
                    giftUser = '<img class="img-rounded" src="' + imgSrc + '" alt=""> ' + d.GiftUser.FullName;
                }

                if (d.Purchaser != null) {
                    purchaser = d.Purchaser.Photo.ThumbnailUrl !== null ? d.Purchaser.Photo.ThumbnailUrl : "../Images/user.png";
                    purchaser = '<img class="img-rounded" src="' + imgSrc + '" alt=""> ' + d.Purchaser.FullName;
                }

                var html =
                    '<div class="detail">' +
                    '<div class="row">' +
                    '<div class="col-sm-3"><span>Gift User:</span>' + giftUser + '</div>' +
                    '<div class="col-sm-3"><span>Purchaser:</span>' + purchaser + '</div>' +
                    '</div>' +
                    '<hr/>' +
                    '<div class="row">' +
                    '<div class="col-sm-3"><span>Is Activate:</span>' + d.IsActivate + '</div>' +
                    '<div class="col-sm-3"><span>Is Reward:</span>' + (d.IsReward != null ? d.IsReward : "") + '</div>' +
                    '<div class="col-sm-3"><span>Is Expired:</span>' + d.IsExpired + '</div>' +
                    '<div class="col-sm-3"><span>Is Recur:</span>' + d.IsRecur + '</div>' +
                    '<div class="col-sm-3"><span>End Time:</span>' + moment(d.EndTime).format('YYYY/MM/DD') + '</div>' +
                    '<div class="col-sm-3"><span>Activate Date:</span>' + moment(d.ActivateDate).format('YYYY/MM/DD') + '</div>' +
                    '<div class="col-sm-3"><span>Create Date:</span>' + moment(d.CreateDate).format('YYYY/MM/DD') + '</div>' +
                    '</div>'
                '</div>';
                return html;
            }
            // Array to track the ids of the details displayed rows
            var detailRows = [];
            $('table#packageList tbody').on('click', 'tr td.details-control', function () {
                var tr = $(this).closest('tr');
                var row = oTable.row(tr);
                var idx = $.inArray(tr.attr('id'), detailRows);

                if (row.child.isShown()) {
                    tr.removeClass('details');
                    row.child.hide();

                    // Remove from the 'open' array
                    detailRows.splice(idx, 1);
                }
                else {
                    tr.addClass('details');
                    row.child(format(row.data())).show();

                    // Add to the 'open' array
                    if (idx === -1) {
                        detailRows.push(tr.attr('id'));
                    }
                }
            });
            $('table#packageList tbody').on('click', 'a.upgrade', function () {
                var td = $(this).closest('td');
                var patientPackageId = $(this).attr('data-patientPackageId');
                $.magnificPopup.open({
                    items: {
                        src: '#PremiumPackage',
                        type: 'inline',
                    },
                    preloader: false,
                    focus: '#Cancel',


                    callbacks: {
                        beforeOpen: function () {

                            // init


                            // set button cancel
                            $('#PremiumPackage').find('button.cancel').unbind('click');
                            $('#PremiumPackage').find('button.cancel').click(function (e) {
                                $('#endDate').val('');
                                $('#remark').val('');
                                $.magnificPopup.close();
                            });

                            // set button done
                            $('#PremiumPackage').find('button.done').unbind('click');
                            $('#PremiumPackage').find('button.done').click(function (e) {
                                if ($('#endDate').val() != null && $('#endDate').val() != "") {

                                    $.post('/package/upgrade?patientPackageId=' + patientPackageId + '&endDate=' + $('#endDate').val() + '&remark=' + $('#remark').val(), function (response) {
                                        $('#endDate').val('');
                                        $('#remark').val('');
                                        $.magnificPopup.close();
                                        var val = response + " ( <a class='downgrade' href='javascript:void(0);' title='Downgrade to a Free Package' data-patientPackageId='" + patientPackageId + "'>DownGrade</a> )"; td.html(val);
                                    }).error(function (jsonError) {
                                        alert(jsonError);
                                    });
                                }
                                else {
                                    alert('Please Select An End Date!');
                                }

                            });

                        }
                    }
                });
                //if(confirm("Are you sure you want to upgrade to premium account?")){
                //    $.post('/package/upgrade?patientPackageId=' + patientPackageId, function (response) {
                //        td.html(response);
                //    }).error(function (jsonError) {
                //        alert(jsonError);
                //    });
                //}
            });
            $('table#packageList tbody').on('click', 'a.downgrade', function () {
                var td = $(this).closest('td');
                var patientPackageId = $(this).attr('data-patientPackageId');
                if (confirm("Are you sure you want to downgrade ?")) {
                    $.post('/package/DownGrade?patientPackageId=' + patientPackageId, function (response) {
                        var val = response + " ( <a class='upgrade' href='javascript:void(0);' title='Upgrade to Premium Account' data-patientPackageId='" + patientPackageId + "'>Upgrade</a> )"; td.html(val);
                        td.html(val);

                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
            });
            // On each draw, loop over the `detailRows` array and show any child rows
            oTable.on('draw', function () {
                $.each(detailRows, function (i, id) {
                    $('#' + id + ' td.details-control').trigger('click');
                });
            });
            // Row Detail end
            $('#search').click(function () {
                oTable.ajax.reload(null, true);
            });
        });
        WinMove();
    });
}
function LoadOutletIndexScripts() {
    $(document).ready(function () {
        // Load Datatables and run plugin on tables
        LoadDataTablesScripts(function () {
            var oTable = $('table#outletList').DataTable({
                "processing": true,
                "serverSide": true,
                "ordering": true,
                "order": [[5, "desc"]],
                "bProcessing": true,
                "scrollX": true,
                "dom": "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/PharmacyOutlets/GetList',
                    "contentType": "application/json; charset=utf-8",
                    "dataType": 'json',
                    "type": "POST",
                    "data": function (d) {
                        d.searchKey = $("#searchName").val();
                        d.prescriptionSourceId = $("#outletType").val();
                        return JSON.stringify(d);
                    },
                },
                "columns": [
                    {
                        "class": "details-control",
                        "orderable": false,
                        "data": null,
                        "defaultContent": ""
                    },
                    {
                        "class": "rank",
                        "data": null, // #
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "name": "FullName",
                        "data": 'FullName',
                        "orderable": true,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var imgSrc = (row.Photo != null && row.Photo.ThumbnailUrl !== null) ? row.Photo.ThumbnailUrl : "../Images/user.png";
                            return '<img class="img-rounded" src="' + imgSrc + '" alt=""> ' + data;
                        }
                    },
                    {
                        "name": "UserName",
                        "data": 'UserName',
                        "orderable": true,
                        "defaultContent": ""
                    },
                    {
                        "name": "PrescriptionSourceName",
                        "data": "PrescriptionSource.PrescriptionSourceName",
                        "orderable": false,
                        "defaultContent": ""
                    },
                    {
                        "name": "CreateDate",
                        "data": 'CreateDate',
                        "orderable": true,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            return '<span>' + moment(data).format('YYYY/MM/DD') + ' </span>';
                        }
                    },
                    {
                        "data": null, // #
                        "orderable": false,
                        "className": 'remark',
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var html = '';
                            html += '<a href="/PharmacyOutlets/Edit?outletId=' + row.UserId + '">Edit</a>';
                            html += ' <a href="#" class="del" data-userId="' + row.UserId + '">Delete</a>';
                            if (!row.IsBan) {
                                html += '<a href="#" class="ban" data-userId="' + row.UserId + '" data-ban="true"> Ban</a>';
                            }
                            else {
                                html += '<a href="#" class="ban" data-userId="' + row.UserId + '" data-ban="false"> Un-ban</a>';
                            }
                            return html;
                        },
                    },
                ],
                "rowCallback": function (row, data) {
                    if (data.IsBan) {
                        $(row).addClass("danger");
                    }
                },
            });

            // Index column
            oTable.on('draw', function () {
                var start = oTable.page.info().start;
                oTable.column(1).nodes().each(function (cell, i) {
                    cell.innerHTML = start + i + 1;
                });
            });


            // Add event listener for Ban an Unban user
            $('table#outletList tbody').on('click', 'a.ban', function () {
                var $this = $(this);
                var userId = $this.attr('data-userId');

                if ($this.attr('data-ban') == 'true') {
                    $.post('/PharmacyOutlets/ban?userId=' + userId, function () {
                        $this.closest('tr').addClass('danger');
                        $this.attr('data-ban', 'false').html('Un-ban');
                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
                else {
                    $.post('/PharmacyOutlets/unban?userId=' + userId, function () {
                        $this.closest('tr').removeClass('danger');
                        $this.attr('data-ban', 'true').html('Ban');
                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
            });

            // Add event listener for Ban an Unban user
            $('table#outletList tbody').on('click', 'a.del', function () {
                var $this = $(this);
                var userId = $this.attr('data-userId');

                if (confirm("Are you sure you want to delete this outlet?")) {
                    $.post('/PharmacyOutlets/Delete?userId=' + userId, function () {
                        oTable.ajax.reload(null, false);
                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
            });

            $("#searchBtn").on('click', function () {
                oTable.ajax.reload(null, true);
            });

            // Row Detail start
            // Add event listener for opening and closing details
            function format(d) {
                var html = "";
                html = '<div class="detail"><b>Address:</b> ' + d.Address + '<br/>' +
                    '<b>Phone:</b> ' + d.PhoneNumber + '<br/>';

                html += '</div>';
                return html;
            }
            // Array to track the ids of the details displayed rows
            var detailRows = [];
            $('table#outletList tbody').on('click', 'tr td.details-control', function () {
                var tr = $(this).closest('tr');
                var row = oTable.row(tr);
                var idx = $.inArray(tr.attr('id'), detailRows);

                if (row.child.isShown()) {
                    tr.removeClass('details');
                    row.child.hide();

                    // Remove from the 'open' array
                    detailRows.splice(idx, 1);
                }
                else {
                    tr.addClass('details');
                    row.child(format(row.data())).show();

                    // Add to the 'open' array
                    if (idx === -1) {
                        detailRows.push(tr.attr('id'));
                    }
                }
            });
            // On each draw, loop over the `detailRows` array and show any child rows
            oTable.on('draw', function () {
                $.each(detailRows, function (i, id) {
                    $('#' + id + ' td.details-control').trigger('click');
                });
            });
            // Row Detail end

            var pharmTable = $('table#pharmacyList').DataTable({
                "processing": true,
                "serverSide": true,
                "ordering": false,
                "scrollX": true,
                "dom": "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/PharmacyOutlets/GetPharmacyList',
                    "contentType": "application/json; charset=utf-8",
                    "dataType": 'json',
                    "type": "GET",
                    "data": function (d) {
                        d.searchKey = $("#pharmacySearchName").val();
                        return d;
                    },
                },
                "columns": [
                    {
                        "class": "rank",
                        "data": null, // #
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "name": "PrescriptionSourceName",
                        "data": 'PrescriptionSourceName',
                        "orderable": false,
                        "defaultContent": ""
                    },
                    {
                        "data": null, // #
                        "orderable": false,
                        "className": 'remark',
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var html = '';
                            html += '<a href="/PharmacyOutlets/EditPharmacy?sourceId=' + row.PrescriptionSourceId + '">Edit</a>';
                            html += ' <a href="#" class="del" data-sourceId="' + row.PrescriptionSourceId + '">Delete</a>';
                            return html;
                        },
                    },
                ]
            });

            // Index column
            pharmTable.on('draw', function () {
                var start = pharmTable.page.info().start;
                pharmTable.column(0).nodes().each(function (cell, i) {
                    cell.innerHTML = start + i + 1;
                });
            });

            $('table#pharmacyList tbody').on('click', 'a.del', function () {
                var $this = $(this);
                var sourceId = $this.attr('data-sourceId');

                if (confirm("Are you sure you want to delete this pharmacy? All outlets associated will be deleted as well.")) {
                    $.post('/PharmacyOutlets/DeletePharmacy?sourceId=' + sourceId, function () {
                        pharmTable.ajax.reload(null, false);
                        oTable.ajax.reload(null, false);
                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
            });

            $("#pharmacySearchBtn").on('click', function () {
                pharmTable.ajax.reload(null, true);
            });
        });
        WinMove();
    });
}

function LoadOutletCreateScripts() {
    LoadAvatar(function () {
        $('#outletCreate .thumbnail-wapper').avatar();
    });
}

function LoadOutletEditScripts() {
    LoadAvatar(function () {
        $('#outletEdit .thumbnail-wapper').avatar();
    });
}


function LoadCashOutIndexScripts() {
    $(document).ready(function () {
        // Load Datatables and run plugin on tables
        LoadDataTablesScripts(function () {
            var oTable = $('table#cashoutList').DataTable({
                "processing": true,
                "serverSide": true,
                "order": [[5, "desc"]],
                "bProcessing": true,
                "scrollX": true,
                "dom": "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/Payment/GetCashOutList',
                    "dataType": 'json',
                    "type": "POST",
                    "data": function (d) { },
                },
                "columns": [
                    {
                        "class": "details-control",
                        "orderable": false,
                        "data": null,
                        "defaultContent": ""
                    },
                    {
                        "class": "rank",
                        "data": null, // #
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'Doctor.FullName',
                        "orderable": true,
                        "defaultContent": "",
                        //"render": function (data, type, row, meta) {
                        //    var html = "";
                        //    html += (row.Title!=null?row.Title:"") + ' ' + data;
                        //    return html;
                        //}
                    },
                    {
                        "data": 'Amount',
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'CashOutRequestStatus',
                        "orderable": true,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var html = "";
                            switch (data) {
                                case 0: html = "Requested"; break;
                                case 1: html = "Processed"; break;
                                case 2: html = "Rejected"; break;
                                default: html = ""; break;
                            }
                            return html;
                        }
                    },
                    {
                        "data": 'RequestDate',
                        "orderable": true,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            return moment(data).format('YYYY/MM/DD');
                        }
                    },
                    {
                        "data": 'CashOutDate',
                        "orderable": true,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            if (data) {
                                return moment(data).format('YYYY/MM/DD');
                            }
                            return "";
                        }
                    },
                    {
                        "data": null, // #
                        "orderable": false,
                        "className": 'remark',
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var html = '';
                            if (row.CashOutRequestStatus == 0) {
                                html += '<a href="javascript:void(0);" class="status" data-process="true" data-requestId="' + row.ChasOutRequestId + '"> Process</a>';
                                //html += '<a href="javascript:void(0);" class="status" data-process="false" data-requestId="' + row.ChasOutRequestId + '"> Reject</a>';
                            }

                            return html;
                        },
                    },
                ],
                "rowCallback": function (row, data) {

                },
            });

            // Index column
            oTable.on('draw', function () {
                var start = oTable.page.info().start;
                oTable.column(1).nodes().each(function (cell, i) {
                    cell.innerHTML = start + i + 1;
                });
            });


            // Add event listener for Ban an Unban user
            $('table#cashoutList tbody').on('click', 'a.status', function () {
                var $this = $(this);
                var requestId = $this.attr('data-requestId');
                if ($this.attr('data-process') == 'true') {
                    OpenPopup(requestId, oTable);
                }
                else {
                    $.post('/Payment/ChangeStatus?status=2&requestId=' + requestId, function () {
                        oTable.ajax.reload(null, false);
                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
            });

            function format(d) {
                var html = '<div class="detail"><b>Account Number:</b> ' + (d.Account.AccountNumber != null ? d.Account.AccountNumber : "") + '<br/>' +
                    '<b>Account Holder Name:</b> ' + (d.Account.AccountHolderName != null ? d.Account.AccountHolderName : "") + '<br/>' +
                    '<b>Bank:</b> ' + (d.Account.Bank != null ? d.Account.Bank.BankName : "") + '<br/>';
                if (d.Remark) {
                    html += "<b>Remark : </b>" + d.Remark + "<br/>";
                }
                if (d.ReceiptUrl) {
                    html += "<b>Receipt : </b><a href='" + d.ReceiptUrl + "' target='_blank' >file</a><br/>";
                }
                html += '</div>';
                return html;
            }
            // Array to track the ids of the details displayed rows
            var detailRows = [];
            $('table#cashoutList tbody').on('click', 'tr td.details-control', function () {
                var tr = $(this).closest('tr');
                var row = oTable.row(tr);
                var idx = $.inArray(tr.attr('id'), detailRows);

                if (row.child.isShown()) {
                    tr.removeClass('details');
                    row.child.hide();

                    // Remove from the 'open' array
                    detailRows.splice(idx, 1);
                }
                else {
                    tr.addClass('details');
                    row.child(format(row.data())).show();

                    // Add to the 'open' array
                    if (idx === -1) {
                        detailRows.push(tr.attr('id'));
                    }
                }
            });
        });
        WinMove();
    });

    function OpenPopup(requestId, oTable) {
        $.magnificPopup.open({
            items: {
                src: '#CashOutRemark',
                type: 'inline',
            },
            preloader: false,
            focus: '#Cancel',


            callbacks: {
                beforeOpen: function () {
                    $("#remark").val("");
                    $("#receipt").val("");

                    // set button cancel
                    $('#CashOutRemark').find('button.cancel').unbind('click');
                    $('#CashOutRemark').find('button.cancel').click(function (e) {
                        $.magnificPopup.close();
                    });

                    // set button done
                    $('#CashOutRemark').find('button.done').unbind('click');
                    $('#CashOutRemark').find('button.done').click(function (e) {
                        var remark = $("#remark").val();
                        var file = jQuery('#receipt')[0].files;
                        if (!remark || !file || remark == "" || file.length == 0) {
                            alert("Please check all values");
                            return;
                        }
                        var data = new FormData();
                        data.append('receipt', file[0]);
                        data.append('remark', remark);

                        jQuery.ajax({
                            url: '/Payment/ChangeStatus?status=1&requestId=' + requestId,
                            data: data,
                            cache: false,
                            contentType: false,
                            processData: false,
                            type: 'POST',
                            success: function (data) {
                                oTable.ajax.reload(null, false);
                                $.magnificPopup.close();
                            }
                        });
                    });
                }
            }
        });
    }
}

function LoadTransactionIndexScripts() {
    var oTable = "";
    $(document).ready(function () {
        // Load Datatables and run plugin on tables
        LoadDataTablesScripts(function () {
            LoadPlatformTotal();
            oTable = $('table#transactionList').DataTable({
                "processing": true,
                "serverSide": true,
                "order": [[9, "desc"]],
                "bProcessing": true,
                "scrollX": true,
                "dom": "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/Transaction/GetTransactionList',
                    "dataType": 'json',
                    "type": "POST",
                    "data": function (d) {
                        d.startDate = $("#from").val(),
                            d.endDate = $("#to").val(),
                            d.status = $("#ddl_status").val(),
                            d.categoryId = $("#ddl_category").val(),
                            d.groupId = $("#ddl_group").val(),
                            d.pName = $("#txt_patientName").val(),
                            d.dName = $("#txt_doctorName").val()
                    },
                },
                "columns": [
                    {
                        "class": "details-control",
                        "orderable": false,
                        "data": null,
                        "defaultContent": ""
                    },
                    {
                       
                        "data": null,
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var html = "";
                            html += "<a href='/patienttransaction/index?patientID=" + row.ChatRoom.Patient.UserId +"' target='_blank'>Details</a>";
                            return html;
                        }
                    },
                    {
                        "data": 'ChatRoom.Patient.FullName',
                        "orderable": false,
                        "defaultContent": "",
                       
                    },
                    {
                        "data": 'ChatRoom.Doctor.FullName',
                        "orderable": false,
                        "defaultContent": "",
                        //"render": function (data, type, row, meta) {
                        //    var html = "";
                        //    html += (row.ChatRoom.Doctor.Title != null ? row.ChatRoom.Doctor.Title : "") + ' ' + data;
                        //    return html;
                        //}
                    },
                    {
                        "data": 'Amount',
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'ActualAmount',
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'PlatformAmount',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var html = "";
                            html = data;
                            return html;
                        }
                    },
                    {
                        "data": 'HcpAmount',
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'PaymentStatus',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var html = "";
                            switch (data) {
                                case 0: html = "Authorised"; break;
                                case 1: html = "Canceled"; break;
                                case 2: html = "Paid"; break;
                                case 3: html = "Failed"; break;
                                case 4: html = "Pre-Authorisation"; break;
                                default: html = ""; break;
                            }
                            return html;
                        }
                    },
                    {
                        "data": 'CreateDate',
                        "orderable": true,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            return moment(data).format('YYYY/MM/DD') + ' ' + formatAMPM(moment(data));
                        }
                    }
                ],
                "rowCallback": function (row, data) {

                },
            });

            // Index column
            //oTable.on('draw', function () {
            //    var start = oTable.page.info().start;
            //    oTable.column(1).nodes().each(function (cell, i) {
            //        cell.innerHTML = start + i + 1;
            //    });
            //});

            function format(d) {
                var html =
                    '<div class="detail">' +
                    '<div class="row">' +
                    '<div class="col-md-3">Braintree Transaction Id :</div>' +
                    '<div class="col-md-3">' + d.BrainTreeTransactionId + '</div>' +
                    '<div class="col-md-3">Braintree Transaction Status :</div>' +
                    '<div class="col-md-3">' + d.BrainTreeTransactionStatus + '</div>' +
                    '</div>' +
                    '<br/>';
                if (d.PromoCode) {

                    html +=
                        '<div class="row">' +
                        '<div class="col-md-2">Code Applied :</div>' +
                        '<div class="col-md-2">' + d.PromoCode.PromoCode + '</div>' +
                        '<div class="col-md-2">Discount :</div>' +
                        '<div class="col-md-2">' + d.PromoCode.Discount + '</div>' +
                        '<div class="col-md-2">Type :</div>' +
                        '<div class="col-md-2">' + ((d.PromoCode.DiscountType == 0) ? "Amount" : "Percentage") + '</div>' +
                        '</div><br/>';
                }
                html += '<div class="row">' +
                    '<div class="col-md-6">' +
                    '<h4>USER</h4>' +
                    '<div class="row">' +
                    '<div class="col-md-3">Title :' +
                    '</div>' +
                    '<div class="col-md-6">' + (d.ChatRoom.Patient.Title ? d.ChatRoom.Patient.Title : "") +
                    '</div>' +
                    '</div>' +
                    '<div class="row">' +
                    '<div class="col-md-3">Full Name :' +
                    '</div>' +
                    '<div class="col-md-6">' + d.ChatRoom.Patient.FullName +
                    '</div>' +
                    '</div>' +
                    '<div class="row">' +
                    '<div class="col-md-3">Email :' +
                    '</div>' +
                    '<div class="col-md-6">' + d.ChatRoom.Patient.UserName +
                    '</div>' +
                    '</div>' +
                    '<div class="row">' +
                    '<div class="col-md-3">Birthday :' +
                    '</div>' +
                    '<div class="col-md-6">' + moment(d.ChatRoom.Patient.BirthDay).format('YYYY/MM/DD') +
                    '</div>' +
                    '</div>' +
                    '</div>' +
                    '<div class="col-md-6">' +
                    '<h4>HCP</h4>' +
                    '<div class="row">' +
                    '<div class="col-md-3">Title :' +
                    '</div>' +
                    '<div class="col-md-6">' + (d.ChatRoom.Doctor.Title ? d.ChatRoom.Doctor.Title : "") +
                    '</div>' +
                    '</div>' +
                    '<div class="row">' +
                    '<div class="col-md-3">Full Name :' +
                    '</div>' +
                    '<div class="col-md-6">' + d.ChatRoom.Doctor.FullName +
                    '</div>' +
                    '</div>' +
                    '<div class="row">' +
                    '<div class="col-md-3">Email :' +
                    '</div>' +
                    '<div class="col-md-6">' + d.ChatRoom.Doctor.UserName +
                    '</div>' +
                    '</div>' +
                    '<div class="row">' +
                    '<div class="col-md-3">Birthday :' +
                    '</div>' +
                    '<div class="col-md-6">' + moment(d.ChatRoom.Doctor.BirthDay).format('YYYY/MM/DD') +
                    '</div>' +
                    '</div>' +
                    '<div class="row">' +
                    '<div class="col-md-3">IsPartner :' +
                    '</div>' +
                    '<div class="col-md-6">' + d.ChatRoom.Doctor.IsPartner +
                    '</div>' +
                    '</div>' +
                    '</div>' +
                    '</div>' +
                    '<br/>';

                html += '</div>';
                return html;
            }
            // Array to track the ids of the details displayed rows
            var detailRows = [];
            $('table#transactionList tbody').on('click', 'tr td.details-control', function () {
                var tr = $(this).closest('tr');
                var row = oTable.row(tr);
                var idx = $.inArray(tr.attr('id'), detailRows);

                if (row.child.isShown()) {
                    tr.removeClass('details');
                    row.child.hide();

                    // Remove from the 'open' array
                    detailRows.splice(idx, 1);
                }
                else {
                    tr.addClass('details');
                    row.child(format(row.data())).show();

                    // Add to the 'open' array
                    if (idx === -1) {
                        detailRows.push(tr.attr('id'));
                    }
                }
            });
        });
        WinMove();

        LoadDateTimePickerScript(function () {
            $('#from,#to').datetimepicker({
                timepicker: false,
                format: 'Y/m/d',
                onChangeDateTime: function (current_time, $input) {
                    $input.trigger('input');
                },
            });
        });

        $("#search").on("click", function () {
            LoadPlatformTotal();
            oTable.ajax.reload(null, true);
        });

        $('#export').on("click", function () {
            var startDate = encodeURIComponent($('#from').val());
            var endDate = encodeURIComponent($('#to').val());
            var status = encodeURIComponent($('#ddl_status').val());
            var categoryId = encodeURIComponent($('#ddl_category').val());
            var groupId = encodeURIComponent($('#ddl_group').val());

            window.open(`/Transaction/ExportTransactionList?startDate=${startDate}&endDate=${endDate}&status=${status}&categoryId=${categoryId}&groupId=${groupId}`, '_blank')
        })

        function formatAMPM(data) {
            var date = new Date(data);
            var hours = date.getHours();
            var minutes = date.getMinutes();
            var ampm = hours >= 12 ? 'pm' : 'am';
            hours = hours % 12;
            hours = hours ? hours : 12; // the hour '0' should be '12'
            minutes = minutes < 10 ? '0' + minutes : minutes;
            var strTime = hours + ':' + minutes + ' ' + ampm;
            return strTime;
        }

        function LoadPlatformTotal() {
            var startDate = $("#from").val();
            var endDate = $("#to").val();
            var status = $("#ddl_status").val();
            var categoryId = $("#ddl_category").val();
            var groupId = $("#ddl_group").val();

            $.post('/Transaction/GetPlatformTotal?startDate=' + startDate + '&endDate=' + endDate + '&status=' + status + '&categoryId=' + categoryId + '&groupId=' + groupId, function (result) {
                $("#platformTotal").html(result);
                ;
            }).error(function (jsonError) {
                $("#platformTotal").html("");
                alert(jsonError);
            });
        }

        $("#ddl_category").on('change', function () {
            var categoryId = $(this).val();
            $.get('/Group/GetGroupDDL?categoryId=' + categoryId, function (data) {
                $("#ddl_group").empty();
                $("<option />", {
                    val: -1,
                    text: "All"
                }).appendTo("#ddl_group");
                for (var i = 0; i < data.length; i++) {
                    $("<option />", {
                        val: data[i].GroupId,
                        text: data[i].GroupName
                    }).appendTo("#ddl_group");
                }
            }).error(function (jsonError) {
                alert(jsonError);
            });
        });
    });
}

function LoadPatientTransactionIndexScripts() {
    var oTable = "";
    var zTable = "";
    $(document).ready(function () {
        // Load Datatables and run plugin on tables
        LoadDataTablesScripts(function () {
            oTable = $('table#transactionList').DataTable({
                "language": {
                    "infoFiltered": ""
                },
                "serverSide": true,
             
                "scrollX": true,
                "dom": "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/PatientTransaction/GetPatientTransaction',
                    "dataType": 'json',
                    "type": "POST",
                    "data": function (d) {
                        d.startDate = $("#from").val(),
                            d.endDate = $("#to").val(),
                            d.pName = $("#txt_patientName").val(),
                            d.userId = $("#hid_patientId").val()                                
                    },

                },
                "columns": [
                    {
                        "data": 'ID',

                        "defaultContent": ""
                    },
                    {
                        "data": 'FullName',
                        "defaultContent": "",
                    },
                    {
                        "data": 'PhoneNumber',
                        "defaultContent": "",
                    },
                    {
                        "data": 'RequestID',
                        "defaultContent": "",
                    },
                    {
                        "data": 'RequestSuccessDate',
                        "orderable": true,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            return moment(data).format('YYYY/MM/DD') + ' ' + formatAMPM(moment(data));
                        }
                    },
                    {
                        "data": 'Amount',

                        "defaultContent": ""
                    },
                    {
                        "data": 'PaymentMethod',

                        "defaultContent": "",
                    }
                  
                ],
                "rowCallback": function (row, data) {

                },
            });
           
        });
            // Load Datatables and run plugin on tables
            LoadDataTablesScripts(function () {
                zTable = $('table#transactionListSpend').DataTable({
                    "language": {
                        "infoFiltered": ""
                    },
                    "processing": true,
                    "serverSide": true,
                    "bProcessing": true,
                    "scrollX": true,
                    "dom": "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                    "ajax": {
                        "url": '/PatientTransaction/GetPatientTransactionSpend',
                        "dataType": 'json',
                        "type": "POST",
                        "data": function (d) {
                            d.startDate = $("#from").val(),
                                d.endDate = $("#to").val(),
                                d.pName = $("#txt_patientName").val(),
                                d.userId = $("#hid_patientId").val()

                        },

                    },
                    "columns": [
                        {
                            "data": 'ChatRoom.Patient.FullName',

                            "defaultContent": ""
                        },
                        {
                            "data": 'CreateDate',
                            "orderable": true,
                            "defaultContent": "",
                            "render": function (data, type, row, meta) {
                                return moment(data).format('YYYY/MM/DD') + ' ' + formatAMPM(moment(data));
                            }
                        },
                        {
                            "data": 'Amount',

                            "defaultContent": ""
                        },
                        {
                            "data": 'ChatRoom.Doctor.FullName',

                            "defaultContent": "",
                        }

                    ],
                    "rowCallback": function (row, data) {

                    },
                });

            });


        LoadDateTimePickerScript(function () {
            $('#from,#to').datetimepicker({
                timepicker: false,
                format: 'Y/m/d',
                onChangeDateTime: function (current_time, $input) {
                    $input.trigger('input');
                },
            });
        });
        function formatAMPM(data) {
            var date = new Date(data);
            var hours = date.getHours();
            var minutes = date.getMinutes();
            var ampm = hours >= 12 ? 'pm' : 'am';
            hours = hours % 12;
            hours = hours ? hours : 12; // the hour '0' should be '12'
            minutes = minutes < 10 ? '0' + minutes : minutes;
            var strTime = hours + ':' + minutes + ' ' + ampm;
            return strTime;
        }
        $("#search").on("click", function () {
            //LoadPlatformTotal();
            $("#hid_patientId").val(0)
            oTable.ajax.reload(null, true);
            zTable.ajax.reload(null, true);
        });

       
        });
  
}



function LoadDashBoardIndexScripts() {

    // Load jQuery DatetimePicker
    //$('#start_date,#end_date').val(moment().format("YYYY/MM/DD"));
    LoadDateTimePickerScript(function () {
        $('#start_date,#end_date').datetimepicker({
            timepicker: false,
            format: 'Y/m/d',
            onChangeDateTime: function (current_time, $input) {
                $input.trigger('input');
            },
        });
    });

    $(document).ready(function () {
        $("#search").on("click", function () {
            GenerateGraph();
        });
        GenerateGraph();
    });
    function CreateChart(data) {
        var chartData = [];
        var xAxisTicks = [];
        for (var i = 0; i < data.length; i++) {
            chartData.push([i + 1, data[i].Yvalue]);
            xAxisTicks.push([i + 1, data[i].Xvalue]);
        }

        var plot = jQuery.plot(jQuery("#graph"),
            [{
                data: chartData,
                label: "Transaction",
                color: "#d68fa8"
            }
            ],
            {
                series: {
                    lines: {
                        show: true,
                        fill: true,
                        lineWidth: 1,
                        fillColor: {
                            colors: [{ opacity: 0.5 },
                            { opacity: 0.5 }
                            ]
                        }
                    },
                    points: {
                        show: true
                    },
                    shadowSize: 0
                },
                legend: {
                    position: 'nw'
                },
                grid: {
                    hoverable: true,
                    clickable: true,
                    borderColor: '#ddd',
                    borderWidth: 1,
                    labelMargin: 10,
                    backgroundColor: '#fff'
                },
                yaxis: {
                    min: 0,
                    //max: [data.length - 1].Yvalue + 5,
                    color: '#eee'
                },
                xaxis: {
                    color: '#eee',
                    ticks: xAxisTicks
                }
            });

        var previousPoint = null;
        jQuery("#graph").bind("plothover", function (event, pos, item) {
            jQuery("#x").text(pos.x.toFixed(2));
            jQuery("#y").text(pos.y.toFixed(2));

            if (item) {
                if (previousPoint != item.dataIndex) {
                    previousPoint = item.dataIndex;

                    jQuery("#tooltip").remove();
                    var x = item.datapoint[0].toFixed(2),
                        y = item.datapoint[1].toFixed(2);

                    showTooltip(item.pageX, item.pageY,
                        "Total amount = " + y);
                }

            } else {
                jQuery("#tooltip").remove();
                previousPoint = null;
            }

        });

        jQuery("#graph").bind("plotclick", function (event, pos, item) {
            if (item) {
                plot.highlight(item.series, item.datapoint);
            }
        });

    }

    function showTooltip(x, y, contents) {
        jQuery('<div id="tooltip" style="z-index:100;" class="tooltipflot">' + contents + '</div>').css({
            position: 'absolute',
            display: 'none',
            top: y + 5,
            left: x + 5
        }).appendTo("body").fadeIn(200);
    }

    function GenerateGraph() {
        var startDate = $('#start_date').val();
        var endDate = $('#end_date').val();

        if (startDate == "" || endDate == "") {
            alert("Please fill the dates");
            return;
        }
        $.ajax({
            type: "POST",
            url: "/Home/GetGraphData",
            data: { startDate: startDate, endDate: endDate },
            success: function (response) {
                if (response) {
                    CreateChart(response)
                }
                else {
                    alert("error ! try again");
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                if (XMLHttpRequest.responseJSON) {
                    alert(XMLHttpRequest.responseJSON.error);
                }
                else {
                    alert("Error Occured Try agaian !");
                }
            }
        });
    }
}

function LoadSettingsIndexScripts() {
    $(document).ready(function () {
        $("#savePercent").click(function () {
            var percent = $("#percent").val();
            if (percent > 0 && percent <= 100) {
                $("#percent").attr("disabled", "disabled");
                $.post('/Settings/SavePercent?percent=' + percent, function () {
                    $("#percent").removeAttr("disabled");
                    alert("Saved successfully");
                }).error(function (jsonError) {
                    alert(jsonError);
                    $("#percent").removeAttr("disabled");
                });
            }
            else {
                alert("Invalid value");
            }
        });

        $("#saveUrl").click(function () {
            var url = $("#promoUrl").val();
            if (url) {
                $("#promoUrl").attr("disabled", "disabled");
                $.post('/Settings/SaveUrl?url=' + url, function () {
                    $("#promoUrl").removeAttr("disabled");
                    alert("Saved successfully");
                }).error(function (jsonError) {
                    alert(jsonError);
                    $("#promoUrl").removeAttr("disabled");
                });
            }
            else {
                alert("Invalid value");
            }
        });
    });
}

function LoadBannerIndexScripts() {
    $(document).ready(function () {
        // Load Datatables and run plugin on tables
        LoadDataTablesScripts(function () {
            var oTable = $('table#bannerList').DataTable({
                "processing": true,
                "serverSide": true,
                "ordering": false,
                "bProcessing": true,
                "scrollX": true,
                "dom": "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/Banner/GetBannerList',
                    "dataType": 'json',
                    "type": "POST",
                    "data": function (d) {

                    },
                },
                "columns": [
                    {
                        "class": "rank",
                        "data": null, // #
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var imgSrc = row.ImageUrl !== null ? row.ImageUrl : "../Images/user.png";
                            return '<img class="img-rounded" src="' + imgSrc + '" alt=""> ';
                        }
                    },
                    {
                        "data": 'LinkUrl',
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'Sequence',
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": null, // #
                        "orderable": false,
                        "className": 'remark',
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var html = "";
                            html += ' <a href="#" class="del" data-catId="' + row.BannerId + '"><i class="fa fa-trash-o"></i></a>';
                            html += ' <a href="/Banner/Edit/' + row.BannerId + '" ><i class="fa fa-edit"></i></a>';
                            return html;
                        },
                    },

                ],
                "rowCallback": function (row, data) {

                },
            });

            // Index column
            oTable.on('draw', function () {
                var start = oTable.page.info().start;
                oTable.column(0).nodes().each(function (cell, i) {
                    cell.innerHTML = start + i + 1;
                });
            });


            $('table#bannerList tbody').on('click', 'a.del', function () {
                var $this = $(this);
                var bannerId = $this.attr('data-catId');

                if (confirm("Are you sure you want to delete this Banner?")) {
                    $.post('/Banner/Delete?bannerId=' + bannerId, function () {
                        oTable.ajax.reload(null, false);
                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
            });
        });
    });
}

function LoadDoctorDutyScripts() {
    $(document).ready(function () {

        if ($("#hid_userid").val() > 0) {
            $("#UserId").val($("#hid_userid").val());            
        }

        LoadDateTimePickerScript(function () {

            $('#from,#to').datetimepicker({
                datepicker: false,
                format: 'H:i'
            });
        });
       
        // Load Datatables and run plugin on tables
        LoadDataTablesScripts(function () {
            var oTable = $('table#dutytime').DataTable({
                "processing": true,
                "serverSide": true,
                "ordering": false,
                "bProcessing": true,
                "scrollX": true,
                "dom": "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/DoctorDuty/GetDutyTime',
                    "dataType": 'json',
                    "type": "POST",
                    "data": function (d) {
                        d.userid = $("#UserId").val()
                    },
                },
                "columns": [
                    {                      
                        "data": 'UserId', // #
                        "orderable": false,
                        "defaultContent": "",
                    },
                    
                    {
                        "data": 'DayId',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            switch (data) {
                                case 1: return 'Monday'; break;
                                case 2: return 'Tuesday'; break;
                                case 3: return 'Wednesday'; break;
                                case 4: return 'Thursday'; break;
                                case 5: return 'Friday'; break;
                                case 6: return 'Saturday'; break;
                                case 7: return 'Sunday'; break;
                            }
                            
                        }
                    },
                    {
                        "data": 'FromTime',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            return '<span>' + moment(data).format('HH:mm') + ' </span>';
                        }
                    },
                    {
                        "data": 'ToTime',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            return '<span>' + moment(data).format('HH:mm') + ' </span>';
                        }
                    },
                    

                ],
                "rowCallback": function (row, data) {

                },
            });

            $('#UserId').change(function () {

                oTable.ajax.reload();
            });


            //$('table#bannerList tbody').on('click', 'a.del', function () {
            //    var $this = $(this);
            //    var bannerId = $this.attr('data-catId');

            //    if (confirm("Are you sure you want to delete this Banner?")) {
            //        $.post('/Banner/Delete?bannerId=' + bannerId, function () {
            //            oTable.ajax.reload(null, false);
            //        }).error(function (jsonError) {
            //            alert(jsonError);
            //        });
            //    }
            //});
        });
    });
}

function LoadCategoryIndexScripts() {
    $(document).ready(function () {
        // Load Datatables and run plugin on tables
        LoadDataTablesScripts(function () {
            var oTable = $('table#categoryList').DataTable({
                "processing": true,
                "serverSide": true,
                "ordering": false,
                "bProcessing": true,
                "scrollX": true,
                "dom": "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/Category/GetCategoryList',
                    "dataType": 'json',
                    "type": "POST",
                    "data": function (d) {

                    },
                },
                "columns": [
                    {
                        "class": "rank",
                        "data": null, // #
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'CategoryName',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var imgSrc = row.ImageUrl !== null ? row.ImageUrl : "../Images/user.png";
                            return '<img class="img-rounded" src="' + imgSrc + '" alt=""> ' + row.CategoryName;
                        }
                    },
                    {
                        "data": 'CategoryPrice',
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'MidnightPrice',
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'IsFreeChat',
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'Sequence',
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'IsHiddenFromPublic',
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": null, // #
                        "orderable": false,
                        "className": 'remark',
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var html = "";
                            html += ' <a href="#" class="del" data-catId="' + row.CategoryId + '"><i class="fa fa-trash-o"></i></a>';
                            html += ' <a href="/Category/Edit/' + row.CategoryId + '" ><i class="fa fa-edit"></i></a>';
                            return html;
                        },
                    },

                ],
                "rowCallback": function (row, data) {

                },
            });

            // Index column
            oTable.on('draw', function () {
                var start = oTable.page.info().start;
                oTable.column(0).nodes().each(function (cell, i) {
                    cell.innerHTML = start + i + 1;
                });
            });


            $('table#categoryList tbody').on('click', 'a.del', function () {
                var $this = $(this);
                var categoryId = $this.attr('data-catId');

                if (confirm("Are you sure you want to delete this category?")) {
                    $.post('/Category/Delete?categoryId=' + categoryId, function () {
                        oTable.ajax.reload(null, false);
                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
            });
        });
    });
}

function LoadGroupIndexScripts() {
    $(document).ready(function () {
        // Load Datatables and run plugin on tables
        LoadDataTablesScripts(function () {
            var oTable = $('table#groupList').DataTable({
                "processing": true,
                "serverSide": true,
                "ordering": false,
                "bProcessing": true,
                "scrollX": true,
                "dom": "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/Group/GetList',
                    "dataType": 'json',
                    "type": "POST",
                    "data": function (d) {
                        d.category = $("#ddl_category").val()
                    },
                },
                "columns": [
                    {
                        "class": "rank",
                        "data": null, // #
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'GroupName',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var imgSrc = (row.Photo != null && row.Photo.ThumbnailUrl !== null) ? row.Photo.ThumbnailUrl : "../Images/user.png";
                            return '<img class="img-rounded" src="' + imgSrc + '" alt=""> ' + data;
                        }
                    },
                    {
                        "data": 'Category.CategoryName',
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'PlatformCut',
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": null, // #
                        "orderable": false,
                        "className": 'remark',
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var html = "";
                            html += ' <a href="#" class="del" data-groupId="' + row.GroupId + '"><i class="fa fa-trash-o"></i></a>';
                            html += ` <a href="Edit?groupId=${row.GroupId}" class="edit" ><i class="fa fa-edit"></i></a>`;
                            return html;
                        },
                    },

                ]
            });

            // Index column
            oTable.on('draw', function () {
                var start = oTable.page.info().start;
                oTable.column(0).nodes().each(function (cell, i) {
                    cell.innerHTML = start + i + 1;
                });
            });


            $('table#groupList tbody').on('click', 'a.del', function () {
                var $this = $(this);
                var groupId = $this.attr('data-groupId');

                if (confirm("Are you sure you want to delete this group?")) {
                    $.post('/Group/Delete?groupId=' + groupId, function () {
                        oTable.ajax.reload(null, false);
                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
            });

            $("#ddl_category").on('change', function () {
                oTable.ajax.reload();
            });
        });
    });
}

function LoadPromoCodeIndexScripts() {
    var oTable = "";
    $(document).ready(function () {
        // Load Datatables and run plugin on tables
        LoadDataTablesScripts(function () {
            oTable = $('table#codeList').DataTable({
                "processing": true,
                "serverSide": true,
                //"order": [[7, "desc"]],
                "ordering": false,
                //"bStateSave": true,
                "bProcessing": true,
                "scrollX": true,
                "dom": "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/PromoCode/GetList',
                    "dataType": 'json',
                    "type": "POST",
                    "data": function (d) {
                        d.partner = $("#partner").val(),
                            d.code = $("#code").val()
                    },
                },
                "columns": [
                    {
                        "class": "details-control",
                        "orderable": false,
                        "data": null,
                        "defaultContent": ""
                    },
                    {
                        "class": "rank",
                        "data": null, // #
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'PromoCode',
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'Discount',
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'DiscountType',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var html = "";
                            switch (data) {
                                case 0: html = "Amount"; break;
                                case 1: html = "Percent"; break;
                                default: html = ""; break;
                            }
                            return html;
                        }
                    },
                    {
                        "data": 'StartDate',
                        "orderable": true,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            if (data) {
                                return moment(data).format('YYYY/MM/DD');
                            }
                            return "";
                        }
                    },
                    {
                        "data": 'EndDate',
                        "orderable": true,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            if (data) {
                                return moment(data).format('YYYY/MM/DD');
                            }
                            return "";
                        }
                    },
                    {
                        "data": null, // #
                        "orderable": false,
                        "className": 'remark',
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var html = '';
                            html += '<a href="javascript:void(0);" class="edit"> <i class="fa fa-edit"></i></a>';
                            html += ' <a href="/PromoCode/History/' + row.PromoCodeId + '" class="histroy" title="Redemption history" ><i class="fa fa-folder-o"></i></a>';
                            html += ' <a href="javascript:void(0);" class="del" ><i class="fa fa-trash-o"></i></a>';
                            return html;
                        },
                    },
                ],
                "rowCallback": function (row, data) {
                    var $editBtn = $(row).find('.edit');
                    $editBtn.click(function () {
                        OpenPopup(data);
                    });

                    var $deleteBtn = $(row).find('.del');
                    $deleteBtn.click(function () {
                        if (confirm("Are you sure you want to delete this promo code?")) {
                            $.post('/PromoCode/Delete?promoCodeId=' + data.PromoCodeId, function () {
                                oTable.ajax.reload(null, false);
                            }).error(function (jsonError) {
                                alert(jsonError);
                            });
                        }
                    });
                },
            });

            // Index column
            oTable.on('draw', function () {
                var start = oTable.page.info().start;
                oTable.column(1).nodes().each(function (cell, i) {
                    cell.innerHTML = start + i + 1;
                });
            });

            function format(d) {
                var html =
                    '<div class="detail">' +
                    '<div class="row">' +
                    '<div class="col-md-2">Description :</div>' +
                    '<div class="col-md-3">' + d.Description + '</div>' +
                    '<div class="col-md-2">Partner :</div>' +
                    '<div class="col-md-3">' + d.PartnerName + '</div>' +
                    '</div>' +
                    '<br/>' +
                    '<div class="row">' +
                    '<div class="col-md-2">Usage per User :</div>' +
                    '<div class="col-md-3">' + ((d.UserUsageLimit) ? d.UserUsageLimit : '') + '</div>' +
                    '<div class="col-md-2">Max Redemptions :</div>' +
                    '<div class="col-md-3">' + ((d.MaxUserLimit) ? d.MaxUserLimit : '') + '</div>' +
                    '</div>' +
                    '<br/>' +
                    '</div>';
                return html;
            }
            // Array to track the ids of the details displayed rows
            var detailRows = [];
            $('table#codeList tbody').on('click', 'tr td.details-control', function () {
                var tr = $(this).closest('tr');
                var row = oTable.row(tr);
                var idx = $.inArray(tr.attr('id'), detailRows);

                if (row.child.isShown()) {
                    tr.removeClass('details');
                    row.child.hide();

                    // Remove from the 'open' array
                    detailRows.splice(idx, 1);
                }
                else {
                    tr.addClass('details');
                    row.child(format(row.data())).show();

                    // Add to the 'open' array
                    if (idx === -1) {
                        detailRows.push(tr.attr('id'));
                    }
                }
            });
        });
        WinMove();

        LoadDateTimePickerScript(function () {
            $('#startDate,#endDate').datetimepicker({
                timepicker: false,
                format: 'Y/m/d',
                onChangeDateTime: function (current_time, $input) {
                    $input.trigger('input');
                },
            });
        });

        $("#search").on("click", function () {
            oTable.ajax.reload(null, true);
        });
        $("#addNew").on("click", function () {
            OpenPopup();
        });

        $("#genCode").on("click", function () {
            var code = GenerateCode();
            $("#txtCode").val(code);
        });

        $("#bulkGen").on("change", function () {
            if ($(this).is(":checked")) {
                $("#codeCount").val('').removeAttr("disabled");
                $("#txtCode").val('').attr("disabled", "disabled");
                $("#genCode").attr("disabled", "disabled");
            }
            else {
                $("#codeCount").val('').attr("disabled", "disabled");
                $("#txtCode").val('').removeAttr("disabled");
                $("#genCode").removeAttr("disabled");
            }
        });

    });

    function RefreshHCP(categoryId, selectedUserId) {
        $("#ddl_hcp").empty();
        $("<option />", {
            val: "0",
            text: "All"
        }).appendTo("#ddl_hcp");
        if (categoryId == "0") {
            return;
        }
        $.get('/User/GetAllHCP?categoryId=' + categoryId, function (data) {

            for (var i = 0; i < data.length; i++) {
                $("<option />", {
                    val: data[i].UserId,
                    text: data[i].FullName
                }).appendTo("#ddl_hcp");
            }
            if (selectedUserId) {
                $("#ddl_hcp").val(selectedUserId);
            }
        }).error(function (jsonError) {
            alert(jsonError);
        });
    }

    $("#ddl_category_popup").on('change', function () {
        var categoryId = $(this).val();
        RefreshHCP(categoryId);
    });

    function GenerateCode() {
        var text = "";
        var possible = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";

        for (var i = 0; i < 6; i++)
            text += possible.charAt(Math.floor(Math.random() * possible.length));

        return text;
    }

    function OpenPopup(codeObj) {
        $.magnificPopup.open({
            items: {
                src: '#codePopup',
                type: 'inline',
            },
            preloader: false,
            focus: '#Cancel',


            callbacks: {
                beforeOpen: function () {
                    FillPopUp(codeObj);
                    // set button cancel
                    $('#codePopup').find('button.cancel').removeAttr("disabled").unbind('click');
                    $('#codePopup').find('button.cancel').click(function (e) {
                        $.magnificPopup.close();
                    });

                    // set button done
                    $('#codePopup').find('button.done').removeAttr("disabled").unbind('click');
                    $('#codePopup').find('button.done').click(function (e) {
                        SavePromoCode((codeObj) ? codeObj.PromoCodeId : 0);

                    });
                }
            }
        });
    }
    function FillPopUp(codeObj) {
        if (codeObj) {
            $("#txtCode").val(codeObj.PromoCode).attr("disabled", "disabled");
            $("#txtDiscount").val(codeObj.Discount).attr("disabled", "disabled");
            $("input[name=codeType][value='" + codeObj.DiscountType + "']").prop("checked", true);
            $('input:radio[name=codeType]').attr("disabled", "disabled");
            $("#genCode").attr("disabled", "disabled");
            $("#divBulk").hide();
            $("#startDate").val(moment(codeObj.StartDate).format('YYYY/MM/DD'));
            if (codeObj.EndDate) {
                $("#endDate").val(moment(codeObj.EndDate).format('YYYY/MM/DD'));
            }
            else {
                $("#endDate").val("");
            }
            $("#txtPartner").val(codeObj.PartnerName);
            $("#codeDesc").val(codeObj.Description);
            $("#userUsage").val(codeObj.UserUsageLimit);
            $("#maxUsage").val(codeObj.MaxUserLimit);
            $("#ddl_category_popup").val(codeObj.CategoryId);
            RefreshHCP(codeObj.CategoryId.toString(), codeObj.DoctorId);
        }
        else {
            $("#txtCode").val("").removeAttr("disabled");
            $("#txtDiscount").val("").removeAttr("disabled");
            $("input[name=codeType][value='0']").prop("checked", true);
            $('input:radio[name=codeType]').removeAttr("disabled");
            $("#genCode").removeAttr("disabled");
            $("#divBulk").show();
            $("#bulkGen").removeAttr("checked");
            $("#codeCount").val("").attr("disabled", "disabled");

            $("#startDate").val(moment(new Date()).format('YYYY/MM/DD'));
            $("#endDate").val("");
            $("#txtPartner").val("");
            $("#codeDesc").val("");
            $("#userUsage").val("");
            $("#maxUsage").val("");
            $("#ddl_category_popup").val("0");
            RefreshHCP("0");
        }
    }

    function SavePromoCode(codeId) {
        var startDate = $("#startDate").val();
        var endDate = $("#endDate").val();
        var partner = $("#txtPartner").val();
        var desc = $("#codeDesc").val();
        var userUsage = $("#userUsage").val();
        var maxUsage = $("#maxUsage").val();
        var categoryId = $("#ddl_category_popup").val();
        var doctorId = $("#ddl_hcp").val();

        if (!partner || partner == '') {
            alert("Please check partner name");
            return;
        }
        var model = {
            PromoCodeId: codeId,
            StartDate: startDate,
            EndDate: endDate,
            PartnerName: partner,
            Description: desc,
            UserUsageLimit: userUsage,
            MaxUserLimit: maxUsage,
            DoctorId: doctorId,
            CategoryId: categoryId
        }
        var url = "/PromoCode/Edit";
        if (codeId == 0) {
            url = "/PromoCode/Add";
            var code = $("#txtCode").val();
            var discount = $("#txtDiscount").val();
            var isBulkGenerate = $("#bulkGen").is(":checked");
            var codeCount = $("#codeCount").val();
            var discountType = $('input:radio[name=codeType]:checked').val();
            if (discount == "") {
                alert("Please fill discount");
                return;
            }
            if (isBulkGenerate) {
                if (codeCount == "" || !(codeCount > 0)) {
                    alert("Please check bulk generation count");
                    return;
                }
            }
            else {
                if (code == "") {
                    alert("Please fill Code");
                    return;
                }
            }

            model.PromoCode = code;
            model.Discount = discount;
            model.DiscountType = discountType;
            model.isBulkGenerate = isBulkGenerate;
            model.codeCount = (codeCount) ? codeCount : 0;
        }

        jQuery.ajax({
            url: url,
            data: model,
            type: 'POST',
            success: function (data) {
                oTable.ajax.reload(null, false);
                $.magnificPopup.close();
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                if (XMLHttpRequest.responseJSON) {
                    alert(XMLHttpRequest.responseJSON.error);
                }
                else {
                    alert("Error Occured Try agaian !");
                }
            }
        });
    }
}

function LoadRedemptionHistoryScripts(promoId) {
    var oTable = "";
    $(document).ready(function () {
        // Load Datatables and run plugin on tables
        LoadDataTablesScripts(function () {
            oTable = $('table#historyList').DataTable({
                "processing": true,
                "serverSide": true,
                "ordering": false,
                "bStateSave": true,
                "bProcessing": true,
                "scrollX": true,
                "dom": "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/PromoCode/GetRedemtionHistory?promoId=' + promoId,
                    "dataType": 'json',
                    "type": "POST",
                    "data": function (d) {
                        d.startDate = $("#from").val(),
                            d.endDate = $("#to").val()
                    },
                },
                "columns": [
                    {
                        "class": "details-control",
                        "orderable": false,
                        "data": null,
                        "defaultContent": ""
                    },
                    {
                        "class": "rank",
                        "data": null, // #
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'ChatRoom.Patient.FullName',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var imgSrc = row.ChatRoom.Patient.Photo !== null ? row.ChatRoom.Patient.Photo.ThumbnailUrl : "../../Images/user.png";
                            return '<img class="img-rounded patient" src="' + imgSrc + '" alt=""> ' + data;
                        }
                    },
                    {
                        "data": 'ChatRoom.Doctor.FullName',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var imgSrc = row.ChatRoom.Doctor.Photo !== null ? row.ChatRoom.Doctor.Photo.ThumbnailUrl : "../../Images/user.png";
                            return '<img class="img-rounded patient" src="' + imgSrc + '" alt=""> ' + data;
                        }
                    },
                    {
                        "data": 'CreateDate',
                        "orderable": true,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            if (data) {
                                return moment(data).format('YYYY/MM/DD');
                            }
                            return "";
                        }
                    },
                    {
                        "data": 'ActualAmount',
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            return row.ActualAmount - row.Amount;
                        }
                    },
                    {
                        "data": 'Amount',
                        "orderable": false,
                        "defaultContent": "",
                    },
                ],
                "rowCallback": function (row, data) {

                },
            });

            // Index column
            oTable.on('draw', function () {
                var start = oTable.page.info().start;
                oTable.column(1).nodes().each(function (cell, i) {
                    cell.innerHTML = start + i + 1;
                });
            });

            function format(d) {
                var html = "";
                html =
                    '<div class="detail">' +
                    '<div class="row">' +
                    '<div class="col-md-3">Braintree Transaction Type :</div>' +
                    '<div class="col-md-3">' + d.BrainTreeTransactionType + '</div>' +
                    '<div class="col-md-3">Braintree Transaction Status :</div>' +
                    '<div class="col-md-3">' + d.BrainTreeTransactionStatus + '</div>' +
                    '</div>' +
                    '<br/>' +
                    '<div class="row">' +
                    '<div class="col-md-6">' +
                    '<h4>USER</h4>' +
                    '<div class="row">' +
                    '<div class="col-md-3">Title :' +
                    '</div>' +
                    '<div class="col-md-6">' + (d.ChatRoom.Patient.Title ? d.ChatRoom.Patient.Title : "") +
                    '</div>' +
                    '</div>' +
                    '<div class="row">' +
                    '<div class="col-md-3">Full Name :' +
                    '</div>' +
                    '<div class="col-md-6">' + d.ChatRoom.Patient.FullName +
                    '</div>' +
                    '</div>' +
                    '<div class="row">' +
                    '<div class="col-md-3">Email :' +
                    '</div>' +
                    '<div class="col-md-6">' + d.ChatRoom.Patient.UserName +
                    '</div>' +
                    '</div>' +
                    '<div class="row">' +
                    '<div class="col-md-3">Birthday :' +
                    '</div>' +
                    '<div class="col-md-6">' + moment(d.ChatRoom.Patient.BirthDay).format('YYYY/MM/DD') +
                    '</div>' +
                    '</div>' +
                    '</div>' +
                    '<div class="col-md-6">' +
                    '<h4>HCP</h4>' +
                    '<div class="row">' +
                    '<div class="col-md-3">Title :' +
                    '</div>' +
                    '<div class="col-md-6">' + (d.ChatRoom.Doctor.Title ? d.ChatRoom.Doctor.Title : "") +
                    '</div>' +
                    '</div>' +
                    '<div class="row">' +
                    '<div class="col-md-3">Full Name :' +
                    '</div>' +
                    '<div class="col-md-6">' + d.ChatRoom.Doctor.FullName +
                    '</div>' +
                    '</div>' +
                    '<div class="row">' +
                    '<div class="col-md-3">Email :' +
                    '</div>' +
                    '<div class="col-md-6">' + d.ChatRoom.Doctor.UserName +
                    '</div>' +
                    '</div>' +
                    '<div class="row">' +
                    '<div class="col-md-3">Birthday :' +
                    '</div>' +
                    '<div class="col-md-6">' + moment(d.ChatRoom.Doctor.BirthDay).format('YYYY/MM/DD') +
                    '</div>' +
                    '</div>' +
                    '<div class="row">' +
                    '<div class="col-md-3">IsPartner :' +
                    '</div>' +
                    '<div class="col-md-6">' + d.ChatRoom.Doctor.IsPartner +
                    '</div>' +
                    '</div>' +
                    '</div>' +
                    '</div>' +
                    '</div>';
                return html;
            }
            // Array to track the ids of the details displayed rows
            var detailRows = [];
            $('table#historyList tbody').on('click', 'tr td.details-control', function () {
                var tr = $(this).closest('tr');
                var row = oTable.row(tr);
                var idx = $.inArray(tr.attr('id'), detailRows);

                if (row.child.isShown()) {
                    tr.removeClass('details');
                    row.child.hide();

                    // Remove from the 'open' array
                    detailRows.splice(idx, 1);
                }
                else {
                    tr.addClass('details');
                    row.child(format(row.data())).show();

                    // Add to the 'open' array
                    if (idx === -1) {
                        detailRows.push(tr.attr('id'));
                    }
                }
            });
        });
        WinMove();

        LoadDateTimePickerScript(function () {
            $('#from,#to').datetimepicker({
                timepicker: false,
                format: 'Y/m/d',
                onChangeDateTime: function (current_time, $input) {
                    $input.trigger('input');
                },
            });
        });

        $("#search").on("click", function () {
            oTable.ajax.reload(null, true);
        });

    });
}

function LoadCorporateUserIndexScripts() {
    $(document).ready(function () {
        // Load Datatables and run plugin on tables
        LoadDataTablesScripts(function () {
            var oTable = $('table#outletList').DataTable({
                "processing": true,
                "serverSide": true,
                "ordering": true,
                "order": [[8, "desc"]],
                "bProcessing": true,
                "scrollX": true,
                "dom": "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/PrescriptionRequest/GetList',
                    "dataType": 'json',
                    "type": "POST",
                    "data": function (d) {
                        d.searchKey = $("#searchName").val();
                        d.corporateId = $("#corporateId").val();
                        d.tpaId = $("#tpaId").val();
                        d.CreatedSource = $("#createdSource").val();
                    },
                },
                "columns": [
                    {
                        "class": "details-control",
                        "orderable": false,
                        "data": null,
                        "defaultContent": ""
                    },
                    {
                        "class": "rank",
                        "data": null, // #
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'FullName',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var imgSrc = (row.Photo != null && row.Photo.ThumbnailUrl !== null) ? row.Photo.ThumbnailUrl : "../Images/user.png";
                            return '<img class="img-rounded" src="' + imgSrc + '" alt=""> ' + data;
                        }
                    },
                    {
                        "data": 'IC',
                        "orderable": false,
                        "defaultContent": ""
                    },
                    {
                        "data": 'UserName',
                        "orderable": false,
                        "defaultContent": ""
                    },
                    {
                        "data": 'CorporateName',
                        "name": "CorporateName",
                        "orderable": true,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            if (row.IsCorporate == true) {
                                return row.CorporateName;
                            }
                            return data;
                        }
                    },
                    {
                        "name": "TPAName",
                        "data": 'CorporateTPAName',
                        "orderable": true,
                        "defaultContent": "N/A"                                       
                    },
                    {
                        "name": "CreatedSource",
                        "data": 'CreatedSourceName',
                        "orderable": true,
                        "defaultContent": "N/A"
                    },
                    {
                        "data": 'CreateDate',
                        "name": "CreateDate",
                        "orderable": true,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            return '<span>' + moment(data).format('YYYY/MM/DD') + ' </span>';
                        }
                    },
                    {
                        "data": null, // #
                        "orderable": false,
                        "className": 'remark',
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var html = '';
                            html += '<a href="/CorporateUser/UserCorporate?userId=' + row.UserId + '">View</a>';
                            return html;
                        },
                    },
                ],
                "rowCallback": function (row, data) {
                    if (data.IsBan) {
                        $(row).addClass("danger");
                    }
                },
            });

            // Index column
            oTable.on('draw', function () {
                var start = oTable.page.info().start;
                oTable.column(1).nodes().each(function (cell, i) {
                    cell.innerHTML = start + i + 1;
                });
            });


            // Add event listener for Ban an Unban user
            $('table#outletList tbody').on('click', 'a.ban', function () {
                var $this = $(this);
                var userId = $this.attr('data-userId');

                if ($this.attr('data-ban') == 'true') {
                    $.post('/CorporateUser/ban?userId=' + userId, function () {
                        $this.closest('tr').addClass('danger');
                        $this.attr('data-ban', 'false').html('Un-ban');
                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
                else {
                    $.post('/CorporateUser/unban?userId=' + userId, function () {
                        $this.closest('tr').removeClass('danger');
                        $this.attr('data-ban', 'true').html('Ban');
                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
            });

            // Add event listener for Ban an Unban user
            $('table#outletList tbody').on('click', 'a.del', function () {
                var $this = $(this);
                var userId = $this.attr('data-userId');

                if (confirm("Are you sure you want to delete this user?")) {
                    $.post('/CorporateUser/Delete?userId=' + userId, function () {
                        oTable.ajax.reload(null, false);
                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
            });

            $("#searchBtn").on('click', function () {
                oTable.ajax.reload(null, true);
            });

            // Row Detail start
            // Add event listener for opening and closing details
            function format(d) {
                var html = "";
                html = '<div class="detail"><b>Address:</b> ' + d.Address + '<br/>' +
                    '<b>Phone:</b> ' + d.PhoneNumber + '<br/>';

                html += '</div>';
                return html;
            }
            // Array to track the ids of the details displayed rows
            var detailRows = [];
            $('table#outletList tbody').on('click', 'tr td.details-control', function () {
                var tr = $(this).closest('tr');
                var row = oTable.row(tr);
                var idx = $.inArray(tr.attr('id'), detailRows);

                if (row.child.isShown()) {
                    tr.removeClass('details');
                    row.child.hide();

                    // Remove from the 'open' array
                    detailRows.splice(idx, 1);
                }
                else {
                    tr.addClass('details');
                    row.child(format(row.data())).show();

                    // Add to the 'open' array
                    if (idx === -1) {
                        detailRows.push(tr.attr('id'));
                    }
                }
            });
            // On each draw, loop over the `detailRows` array and show any child rows
            oTable.on('draw', function () {
                $.each(detailRows, function (i, id) {
                    $('#' + id + ' td.details-control').trigger('click');
                });
            });
            // Row Detail end
        });
        WinMove();
    });
}

function loadCorporatePrescriptionList(corporateUserId) {
    var oTable = "";
    $(document).ready(function () {
        // Load Datatables and run plugin on tables
        LoadDataTablesScripts(function () {
            oTable = $('table#codeList').DataTable({
                "processing": true,
                "serverSide": true,
                //"order": [[7, "desc"]],
                "ordering": false,
                //"bStateSave": true,
                "bProcessing": true,
                "scrollX": true,
                "dom": "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/PrescriptionRequest/GePrescriptiontList',
                    "dataType": 'json',
                    "type": "POST",
                    "data": function (d) {
                        d.searchKey = $("#searchName").val(),
                            d.userId = corporateUserId,
                            d.prescriptionStatus = $("#status").val()
                    },
                },
                "columns": [
                    {
                        "class": "details-control",
                        "orderable": false,
                        "data": null,
                        "defaultContent": ""
                    },
                    {
                        "class": "rank",
                        "data": null, // #
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'PrescriptionId',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            if (row.PrescriptionStatus == 0) {
                                return '#' + data;
                            }
                            return '<a href="/PrescriptionRequest/Detail?Id=' + data + '" > #' + data + '</a>';
                            //return '#' + data;
                        }
                    },
                    {
                        "data": 'Doctor.FullName',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            //return '<a href="../Doctor/Reviews?doctorId=' + row.Doctor.UserId + '" >' + data + '</a>';
                            return data;
                        }
                    },
                    {
                        "data": 'CreateDate',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            if (data) {
                                return moment(data).format('DD/MM/YYYY') + ' ' + formatAMPM(moment(data));
                            }
                            return "N/A";
                        }
                    },
                    {
                        "data": 'PrescriptionStatus',
                        "orderable": true,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            if (data == 0) {
                                return 'No Status'
                            }
                            else if (data == 1) {
                                return '<font color="Blue">Self Collection</font>';
                            }
                            else if (data == 2) {
                                return '<font color="Blue">Delivery</font>';
                            }
                            else if (data == 3) {
                                return '<font color="Blue">On Site Collection</font><br />' +
                                    'Next Dispense Date : <br />' +
                                    moment(row.NextDispenseDateTime).format('DD/MM/YYYY') + ' ' + formatAMPM(moment(row.NextDispenseDateTime));
                            }
                        }
                    },
                    {
                        "data": 'LatestUpdate',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            if (data) {
                                var html = moment(data).format('DD/MM/YYYY') + ' ' + formatAMPM(moment(data));
                                return html;
                            }
                            return 'N/A';
                        }
                    },
                    {
                        "data": null, // #
                        "orderable": false,
                        "className": 'remark',
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var _dispatchId = 0;
                            if (data.Dispatch !== null) {
                                _dispatchId = data.Dispatch.DispatchId;
                            }
                            return '<a href="/PrescriptionRequest/PrescriptionLog?PrescriptionId=' + data.PrescriptionId + '&Id=' + _dispatchId + '" class="log"><button class="btn btn-primary" type="button"> Request Log </button></a>';

                        },
                    },
                ],
                "rowCallback": function (row, data) {

                    var $editBtn = $(row).find('.edit');
                    $editBtn.click(function () {
                        OpenPopupEdit(data);
                    });

                    var $approveBtn = $(row).find('.Approve');
                    $approveBtn.click(function () {
                        if (confirm("Are you sure want to confirm Approve?")) {
                            $.post('/PrescriptionRequest/Approve?prescriptionDispatchId=' + data.Dispatch.DispatchId, function () {
                                oTable.ajax.reload(null, false);
                            }).error(function (jsonError) {
                                alert(jsonError);
                            });
                        }
                    });

                    var $ReadyBtn = $(row).find('.Ready');
                    $ReadyBtn.click(function () {
                        if (confirm("Are you sure want to confirm Ready?")) {
                            $.post('/PrescriptionRequest/Ready?prescriptionDispatchId=' + data.Dispatch.DispatchId, function () {
                                oTable.ajax.reload(null, false);
                            }).error(function (jsonError) {
                                alert(jsonError);
                            });
                        }
                    });
                },
            });

            // Index column
            oTable.on('draw', function () {
                var start = oTable.page.info().start;
                oTable.column(1).nodes().each(function (cell, i) {
                    cell.innerHTML = start + i + 1;
                });
            });

            function formatAMPM(data) {
                var date = new Date(data);
                var hours = date.getHours();
                var minutes = date.getMinutes();
                var ampm = hours >= 12 ? 'pm' : 'am';
                hours = hours % 12;
                hours = hours ? hours : 12; // the hour '0' should be '12'
                minutes = minutes < 10 ? '0' + minutes : minutes;
                var strTime = hours + ':' + minutes + ' ' + ampm;
                return strTime;
            }

            function format(d) {
                var packageList = '';
                for (let i = 0; i < d.Drugs.length; i++) {
                    packageList += '<div class="row"><div class="col-md-4"><b>' + d.Drugs[i].MedicationName + '</b></div>' +
                        '<div class="col-md-2"><b> Amount :' + d.Drugs[i].Amount + '</b></div>' +
                        '<div class="col-md-2"><b> Dosage :' + d.Drugs[i].Dosage + '</b></div>' +
                        '<div class="col-md-2"><b> Status :' + d.Drugs[i].Status + '</b></div>' +
                        '</div>' +
                        '<div class="row"><div class="col-md-8">Remark : <b>' + d.Drugs[i].Remark + '</b></div></div>';
                }

                var ModeDetail = '';
                if (d.Dispatch != null && d.Dispatch.PrescriptionStatus == 2) {
                    ModeDetail +=
                        '<div class="row">' +
                        '<div class="col-md-2">Delivery Address :</div>' +
                        '<div class="col-md-8"><b>' + d.Dispatch.DeliveryAddress + '</b></div>' +
                        '</div>' +
                        '<br/>';
                }

                if (d.Dispatch != null && d.Dispatch.PrescriptionStatus == 3) {
                    ModeDetail +=
                        '<div class="row">' +
                        '<div class="col-md-2">OnSite Address :</div>' +
                        '<div class="col-md-8"><b>' + d.Dispatch.OnSite.OnSiteName + '<br />' + d.Dispatch.OnSite.OnSiteAddress + '</b></div>' +
                        '</div>' +
                        '<br/>';
                }

                if (d.Dispatch != null && d.Dispatch.PrescriptionStatus == 1) {
                    ModeDetail +=
                        '<div class="row">' +
                        '<div class="col-md-2">Outlet Address :</div>' +
                        '<div class="col-md-8"><b>' + d.Dispatch.PharmacyOutlet.FullName + '<br />' + d.Dispatch.PharmacyOutlet.Address + '</b></div>' +
                        '</div>' +
                        '<br/>';
                }

                var html =
                    '<div class="detail">' +
                    '<div class="row">' +
                    '<div class="col-md-1">Height:</div>' +
                    '<div class="col-md-2"><b>' + d.Height + 'cm</b></div>' +
                    '<div class="col-md-1">Weight :</div>' +
                    '<div class="col-md-2"><b>' + d.Weight + 'Kg</b></div>' +
                    '<div class="col-md-1">Allergy :</div>' +
                    '<div class="col-md-2"><b>' + d.Allergy + '</b></div>' +
                    '<div class="col-md-1">Document </div>' +
                    '<div class="col-md-2"><a href="' + d.FileUrl + '" target="_blank"><b>View</b></a></div>' +
                    '</div>' +
                    '<br/>' +
                    ModeDetail +
                    '<div class="row">' +
                    '<div class="col-md-2">Drug List :</div>' +
                    '</div>' +
                    packageList +
                    '<br/>' +
                    '</div>';
                return html;
            }
            // Array to track the ids of the details displayed rows
            var detailRows = [];
            $('table#codeList tbody').on('click', 'tr td.details-control', function () {
                var tr = $(this).closest('tr');
                var row = oTable.row(tr);
                var idx = $.inArray(tr.attr('id'), detailRows);

                if (row.child.isShown()) {
                    tr.removeClass('details');
                    row.child.hide();

                    // Remove from the 'open' array
                    detailRows.splice(idx, 1);
                }
                else {
                    tr.addClass('details');
                    row.child(format(row.data())).show();

                    // Add to the 'open' array
                    if (idx === -1) {
                        detailRows.push(tr.attr('id'));
                    }
                }
            });
        });
        WinMove();

        LoadDateTimePickerScript(function () {
            $('#startDate,#endDate').datetimepicker({
                timepicker: false,
                format: 'Y/m/d',
                onChangeDateTime: function (current_time, $input) {
                    $input.trigger('input');
                },
            });
        });

        $("#search").on("click", function () {
            oTable.ajax.reload(null, true);
        });

    });


    function OpenPopupEdit(codeObj) {
        $.magnificPopup.open({
            items: {
                src: '#codePopupEdit',
                type: 'inline',
            },
            preloader: false,
            focus: '#Cancel',

            callbacks: {
                beforeOpen: function () {
                    //FillPopUpEdit(codeObj);
                    // set button cancel
                    $('#codePopupEdit').find('button.cancel').removeAttr("disabled").unbind('click');
                    $('#codePopupEdit').find('button.cancel').click(function (e) {
                        $.magnificPopup.close();
                    });

                    // set button done
                    $('#codePopupEdit').find('button.done').removeAttr("disabled").unbind('click');
                    $('#codePopupEdit').find('button.done').click(function (e) {
                        SaveNextDispense((codeObj) ? codeObj.PrescriptionId : 0);
                    });
                }
            }
        });
    }

    function SaveNextDispense(codeId) {
        var nextDate = $("#nextDate").val();
        var nextTime = $("#nextTime").val();

        if (!nextDate || nextDate == '') {
            alert("Please fill  Remark");
            return;
        }

        var model = {
            PrescriptionId: codeId,
            nextDispenseDate: nextDate,
            nextDispenseTime: nextTime
        }

        var url = "/PrescriptionRequest/NextDispense";
        jQuery.ajax({
            url: url,
            data: model,
            type: 'POST',
            success: function (data) {
                alert("Updated");
                oTable.ajax.reload(null, false);
                $.magnificPopup.close();
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                if (XMLHttpRequest.responseJSON) {
                    alert(XMLHttpRequest.responseJSON.error);
                }
                else {
                    alert("Error Occured Try agaian !");
                }
            }
        });
    }

    LoadDateTimePickerScript(function () {
        $('#nextDispenseDate .hasDatepicker').datetimepicker({
            timepicker: false,
            format: 'Y/m/d',
            onChangeDateTime: function (current_time, $input) {
                $input.trigger('input');
            },
        });
    });
}

function loadRequestedList(processingStatusEnum, shipmentStatusEnum, shouldShowStatus) {
    var oTable = "";
    $(document).ready(function () {
        // Load Datatables and run plugin on tables
        LoadDataTablesScripts(function () {
            oTable = $('table#codeList')
                .on('processing.dt', function (e, setttings, processing) {
                    $('.edit-payment').css('pointer-events', processing ? 'none' : 'click').css('color', processing ? 'lightgray' : '#337ab7')
                }).DataTable({
                "processing": true,
                "serverSide": true,
                "order": [[6, "desc"]],
                "ordering": true,
                "bProcessing": true,
                "scrollX": true,
                "dom": "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/PrescriptionRequest/GetRequestedList',
                    "dataType": 'json',
                    "type": "POST",
                    "data": function (d) {
                        d.searchKey = $("#searchName").val();
                        d.searchName = $("#searchFullname").val();
                        d.prescriptionStatus = $("#status").val();
                        d.responsiblePharmacy = $("#responsiblePharmacy").val();
                        d.startDate = moment($("#searchStartDate").val(), 'YYYY-MM-DD').toISOString();
                        d.endDate = moment($("#searchEndDate").val(), 'YYYY-MM-DD').toISOString();
                        d.corporateId = $("#corporate").val();
                        d.tpaId = $("#tpa").val();
                        d.status = $('#processingStatus').val();
                    },
                },
                "columns": [
                    {
                        "data": null,
                        "orderable": false,
                        "defaultContent": '<input type="checkbox" class="select-prescription-checkbox" />'
                    },
                    {
                        "class": "details-control",
                        "orderable": false,
                        "data": null,
                        "defaultContent": ""
                    },
                    {
                        "class": "rank",
                        "data": null, // #
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'PrescriptionId',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            if (row.PrescriptionStatus === 0) {
                                return '#' + data;
                            }
                            return '<a href="/PrescriptionRequest/Detail?Id=' + data + '" > #' + data + '</a>';
                            //return '#' + data;
                        }
                    },
                    {
                        "data": 'Patient.FullName',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            //return '<a href="../Doctor/Reviews?doctorId=' + row.Doctor.UserId + '" >' + data + '</a>';
                            return data;
                        }
                    },
                    {
                        "data": 'Doctor.FullName',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            //return '<a href="../Doctor/Reviews?doctorId=' + row.Doctor.UserId + '" >' + data + '</a>';
                            return data;
                        }
                    },
                    {
                        "data": 'CreateDate',
                        "name": "CreateDate",
                        "orderable": true,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            if (data) {
                                return moment(data).format('DD/MM/YYYY h:mm a');
                            }
                            return "N/A";
                        }
                    },
                    {
                        "data": 'Dispatch.CreatedDate',
                        "name": "RequestDate",
                        "orderable": true,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            if (data) {
                                return moment(data).format('DD/MM/YYYY h:mm a');
                            }
                            return "N/A";
                        }
                    },
                    {
                        "data": 'PrescriptionStatus',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var html = "";
                            if (data === 0) {
                                html += 'No Status';
                            }
                            else if (data === 1) {
                                html += '<font color="Blue">Self Collection</font>';
                            }
                            else if (data === 2) {
                                html += '<font color="Blue">Delivery</font>';
                                if (row.ConsignmentNumber != null) {
                                    html += ` (${ row.ConsignmentNumber })`;
                                } else {
                                    html += ` (<a style="cursor: pointer;" class="create-cn" href="#create-cn-form">Create CN</a>)`;
                                }
                            }
                            else if (data === 3) {
                                html += '<font color="Blue">On Site Collection</font><br />' +
                                    'Next Dispense Date : <br />' +
                                    moment(row.NextDispenseDateTime).format('DD/MM/YYYY h:mm a');
                            }
                            if (row.Dispatch) {

                                switch (row.Dispatch.PackingType) {
                                    case 0: html += ""; break;
                                    case 1: html += "<br/><span style='color: #f5a817e6;'> (PILcube Packing)</span>"; break;
                                }
                            }
                            return html;
                        }
                    },
                    {
                        "data": "ProcessingStatus",
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var html = "";
                            html += `<select class="status-select" ${ data === processingStatusEnum.Shipped || data === processingStatusEnum.Rejected ? 'disabled' : '' }>`; // do not allow changing status once handed over to courier
                            html += `<option value="0" ${ data == null && row.LatestShipmentStatus == null ? 'selected' : '' } ${ data != null ? 'hidden' : '' }>Unknown</option>`;
                            html += `<option value="1" ${ data === processingStatusEnum.Processing ? 'selected' : '' }>Processing</option>`;
                            html += `<option value="2" ${ data === processingStatusEnum.ToShip ? 'selected' : '' }>To Ship</option>`;
                            html += `<option value="3" ${ data === processingStatusEnum.Shipped && (row.LatestShipmentStatus === shipmentStatusEnum.Pending || row.LatestShipmentStatus === shipmentStatusEnum.Pickup) ? 'selected' : '' }>Shipped</option>`;
                            html += `<option disabled ${ data === processingStatusEnum.Shipped && row.LatestShipmentStatus === shipmentStatusEnum.InTransit ? 'selected' : '' }>In Transit</option>`;
                            html += `<option disabled ${ data === processingStatusEnum.Shipped && row.LatestShipmentStatus === shipmentStatusEnum.OutForDelivery ? 'selected' : '' }>Out for Delivery</option>`;
                            html += `<option disabled ${ data === processingStatusEnum.Shipped && row.LatestShipmentStatus === shipmentStatusEnum.Delivered ? 'selected' : '' }>Delivered</option>`;
                            html += `<option disabled ${ data === processingStatusEnum.Shipped && row.LatestShipmentStatus === shipmentStatusEnum.Returned ? 'selected' : ''}>Returned</option>`;
                            html += `<option value="4" ${ data === processingStatusEnum.Rejected ? 'selected' : '' }>Rejected</option>`;
                            html += `</select>`;
                            return html;
                        }
                    },
                    {
                        "data": 'LatestUpdate',
                        "name": "LastUpdateDate",
                        "orderable": true,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            if (data) {
                                var html = moment(data).format('DD/MM/YYYY h:mm a');
                                return html;
                            }
                            return 'N/A';
                        }
                    },
                    {
                        "data": "Patient.CorporateName",
                        "orderable": false,
                        "defaultContent": "-"
                    },
                    {
                        "data": "Patient.CorporateTPAName",
                        "orderable": false,
                        "defaultContent": "-"
                    },
                    {
                        "data": function (row, type, set, meta) {
                            switch (row.MedicationType) {
                                case 0:
                                    return '-';
                                case 1:
                                    return 'Minor Ailment';
                                case 2:
                                    return 'LTM';
                            }
                        },
                        "orderable": false
                    },
                    {
                        "data": null, // #
                        "orderable": false,
                        "className": 'remark',
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var html = '';
                            if (data.IsDigitallySigned) {
                                html = '<a href="/Prescription/DigitalPrescription?id=' + data.PrescriptionId + '" target="_blank"><i class="fa fa-list-alt"></i></a>';
                            }
                            return html;
                        }
                    },
                    {
                        "data": null, // #
                        "orderable": false,
                        "className": 'remark',
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var _dispatchId = 0;
                            if (data.Dispatch !== null) {
                                _dispatchId = data.Dispatch.DispatchId;
                            }
                            return '<a href="/PrescriptionRequest/PrescriptionLog?PrescriptionId=' + data.PrescriptionId + '&Id=' + _dispatchId + '" class="log"><button class="btn btn-primary" type="button"> Request Log </button></a>';
                        }
                    },
                    {
                        "data": null,
                        "orderable": false,
                        "defaultContent": '<input type="checkbox" class="processed-checkbox" />'
                    },
                    {
                        "data": null,
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            if (data.ConsultationFees != null) {
                                return `<p>Consultation: ${ data.ConsultationFees.toFixed(2) }</p>
                                        <p>Medication: ${ data.MedicationFees.toFixed(2) }</p>
                                        <p>Delivery: ${ data.DeliveryFees.toFixed(2) }</p>
                                        <p><b>Total: ${ (data.ConsultationFees + data.MedicationFees + data.DeliveryFees).toFixed(2) }</b></p>
                                        <a class="edit-payment" href="#edit-payment-form"><i class="fa fa-pencil"></i></a>`
                            } else {
                                return `<a class="edit-payment" href="#edit-payment-form"><i class="fa fa-money"></i></a>`;
                            }
                        },
                    },
                    {
                        "data": "Patient.UserId",
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            return `<a href="/PrescriptionRequest/ViewPatientMedia?patientId=${ data }">View Patient Media</a>`;
                        }
                    }
                ],
                "rowCallback": function (row, data) {
                    var thisRow = $(row);

                    var editBtn = thisRow.find('.edit-payment');
                    editBtn.magnificPopup({
                        type: 'inline',
                        preloader: false,
                        callbacks: {
                            open: function () {
                                $('#edit-payment-form').resetForm();
                                if (data.ConsultationFees != null) {
                                    $('#consultation').val(data.ConsultationFees);
                                } else {
                                    $('#consultation').prop('disabled', true);
                                    $.ajax({
                                        url: `/PrescriptionRequest/GetDoctorConsultationRate?doctorId=${data.Doctor.DoctorId}&consultationDateTimeUtc=${moment(data.CreateDate).toISOString()}`,
                                        type: 'GET',
                                        success: rate => {
                                            $('#consultation').val(rate);
                                            $('#consultation').prop('disabled', false);
                                        }
                                    })
                                }
                                if (data.MedicationFees != null) {
                                    $('#medication').val(data.MedicationFees);
                                }
                                if (data.DeliveryFees != null) {
                                    $('#delivery').val(data.DeliveryFees);
                                }

                                $('#submit-payment-edit').click(function () {
                                    $.ajax({
                                        url: `/PrescriptionRequest/RecordOrEditPrescriptionPaymentAmount?prescriptionId=${data.PrescriptionId}&consultationFees=${$('#consultation').val()}&medicationFees=${$('#medication').val()}&deliveryFees=${$('#delivery').val()}`,
                                        type: 'POST',
                                        success: () => {
                                            oTable.ajax.reload(null, false);
                                            $.magnificPopup.close();
                                        },
                                        error: (XMLHttpRequest) => {
                                            if (XMLHttpRequest.responseJSON) {
                                                alert(XMLHttpRequest.responseJSON.error);
                                            } else {
                                                alert("Error occured while trying to update processed status. Please try again");
                                            }
                                        }
                                    })
                                })
                            },
                            close: function () {
                                $('#submit-payment-edit').off('click');
                            }
                        }
                    });

                    var createCnBtn = thisRow.find('.create-cn');
                    createCnBtn.magnificPopup({
                        type: 'inline',
                        preloader: false,
                        callbacks: {
                            open: function () {
                                $('#create-cn-form').resetForm();

                                var fullAddress = data.Dispatch.DeliveryAddress;
                                var regex = /\d{5}/;
                                var postcode = fullAddress.match(regex);
                                var address1 = fullAddress.replace(postcode, '');
                                $('#address').val(address1);
                                if (postcode != null) {
                                    $('#postcode').val(postcode);
                                }

                                var phoneNumber = data.Patient.PhoneNumber;
                                $('#phone').val(phoneNumber);

                                $('#remarks').val('urgent');

                                $('#submit-create-cn').click(function () {
                                    var thisButton = $(this);
                                    thisButton.prop('disabled', true);

                                    var prescriptionId = data.PrescriptionId;
                                    var receiverName = data.Patient.FullName;
                                    var phoneNumber = $('#phone').val();
                                    var address = $('#address').val();
                                    var postcode = $('#postcode').val();
                                    var shipmentWeight = $('#weight').val();
                                    var remarks = $('#remarks').val();

                                    $.ajax({
                                        url: `/PrescriptionRequest/CreateGdexCn?prescriptionId=${prescriptionId}&receiverName=${receiverName}&phoneNumber=${phoneNumber}&address=${address}&postcode=${postcode}&shipmentWeight=${shipmentWeight}&remarks=${remarks}`,
                                        type: 'POST',
                                        success: cn => {
                                            $.magnificPopup.close();
                                            thisRow.find('.create-cn').replaceWith(cn);
                                            thisRow.find('.status-select').val(processingStatusEnum.ToShip);
                                        },
                                        error: (XMLHttpRequest) => {
                                            if (XMLHttpRequest.responseJSON) {
                                                alert(XMLHttpRequest.responseJSON.error);
                                            } else {
                                                alert("Error occured while trying to create consignment note. Please try again");
                                            }
                                        },
                                        complete: () => {
                                            thisButton.prop('disabled', false);
                                        }
                                    })
                                })
                            },
                            close: function () {
                                $('#submit-create-cn').off('click');
                            }
                        }
                    });

                    var statusSelect = thisRow.find('.status-select');
                    statusSelect.data('prev', statusSelect.val());
                    statusSelect.change(function () {
                        var thisSelect = $(this);
                        var status = thisSelect.val();
                        var prescriptionIds = data.PrescriptionId;

                        if (confirm(`Change status of prescription ${ prescriptionIds } to ${ Object.keys(processingStatusEnum).find(k => processingStatusEnum[k].toString() === status) }?`)) {
                            $.ajax({
                                url: `/PrescriptionRequest/EditPrescriptionProcessingStatus?prescriptionIds=${prescriptionIds}&status=${status}`,
                                type: 'POST',
                                success: () => {
                                    if (status === processingStatusEnum.Shipped.toString() || status === processingStatusEnum.Rejected.toString()) {
                                        thisSelect.prop('disabled', true);
                                    }
                                    thisSelect.data('prev', status);
                                },
                                error: (XMLHttpRequest) => {
                                    if (XMLHttpRequest.responseJSON) {
                                        alert(XMLHttpRequest.responseJSON.error);
                                    } else {
                                        alert("Error updating status. Please try again");
                                    }
                                    thisSelect.val(thisSelect.data('prev'));
                                }
                            });
                        } else {
                            thisSelect.val(thisSelect.data('prev'));
                        }
                    });
                },
                "createdRow": function (row, data, dataIndex) {
                    var $processedCheckbox = $(row).find('.processed-checkbox');
                    $processedCheckbox.prop('checked', data.CheckoutProcessed);
                    $processedCheckbox.change(function () {
                        var confirmMessage = this.checked ?
                            `Mark prescription ${data.PrescriptionId} as processed?`
                            : `Reset prescription ${data.PrescriptionId} to not processed?`;
                        if (!confirm(confirmMessage)) {
                            this.checked = !this.checked;
                            return;
                        }
                        this.disabled = true;
                        $.ajax({
                            url: "/PrescriptionRequest/ChangeCheckoutProcessed",
                            type: 'POST',
                            data: {
                                prescriptionId: data.PrescriptionId,
                                processed: this.checked
                            },
                            success: () => {
                                this.disabled = false;
                            },
                            error: (XMLHttpRequest) => {
                                this.checked = !this.checked;
                                this.disabled = false;
                                if (XMLHttpRequest.responseJSON) {
                                    alert(XMLHttpRequest.responseJSON.error);
                                } else {
                                    alert("Error occured while trying to update processed status. Please try again");
                                }
                            }
                        });
                    });

                    var $prescriptionSelected = $(row).find('.select-prescription-checkbox');
                    $prescriptionSelected.change(function () {
                        var prescriptionId = data.PrescriptionId;
                        if (this.checked) {
                            if (!selectedPrescriptionIds.includes(prescriptionId)) {
                                selectedPrescriptionIds.push(prescriptionId);
                            }
                        } else {
                            var indexOfSelected = selectedPrescriptionIds.indexOf(prescriptionId);
                            if (indexOfSelected > -1) {
                                selectedPrescriptionIds.splice(indexOfSelected, 1);
                            }
                        }

                        // change 'select all' checkbox status
                        if ($('.select-prescription-checkbox:checked').length === 0) {
                            $('#select-all-prescription-checkbox').prop('checked', false);
                            $('#select-all-prescription-checkbox').prop('indeterminate', false);
                            $('#bulk-status-button').prop('disabled', true);
                            $('#bulk-print-button').prop('disabled', true);
                        } else if ($('.select-prescription-checkbox:checked').length < $('.select-prescription-checkbox').length) {
                            $('#select-all-prescription-checkbox').prop('checked', false);
                            $('#select-all-prescription-checkbox').prop('indeterminate', true);
                            $('#bulk-status-button').prop('disabled', false);
                            $('#bulk-print-button').prop('disabled', false);
                        } else {
                            $('#select-all-prescription-checkbox').prop('checked', true);
                            $('#select-all-prescription-checkbox').prop('indeterminate', false);
                            $('#bulk-status-button').prop('disabled', false);
                            $('#bulk-print-button').prop('disabled', false);
                        }
                    });
                },
                "drawCallback": function (settings) {
                    $('#select-all-prescription-checkbox').prop('checked', false);
                    $('#select-all-prescription-checkbox').prop('indeterminate', false);
                    $('#bulk-status-button').prop('disabled', true);
                    $('#bulk-print-button').prop('disabled', true);

                    // update 'Status' filter dropdown counts
                    var dtApi = new $.fn.dataTable.Api(settings);
                    var statusCounts = dtApi.ajax.json().statusCounts;
                    if (statusCounts != null) {
                        for (let { Status, Count } of statusCounts) {
                            var optionElem = $('#processingStatus').children(`option[value="${ Status }"]`).first();
                            optionElem.text(optionElem.text().replace(/\(\d+\)/g, `(${ Count })`));
                        }
                    }
                },
                "initComplete": function (settings, json) {
                    var api = this.api();
                    if (!shouldShowStatus) {
                        api.column(0).visible(false);
                        api.column(9).visible(false);
                    }
                }
            });

            let selectedPrescriptionIds = [];

            $('#bulk-status-button').click(function () {
                var status = $('#bulk-status-select').val();
                var prescriptionIdsParam = "prescriptionIds=" + selectedPrescriptionIds.join("&prescriptionIds=");

                $.ajax({
                    url: `/PrescriptionRequest/EditPrescriptionProcessingStatus?status=${status}&${prescriptionIdsParam}`,
                    type: 'POST',
                    success: () => {
                        oTable.ajax.reload(null, false);
                    },
                    error: (XMLHttpRequest) => {
                        if (XMLHttpRequest.responseJSON) {
                            alert(XMLHttpRequest.responseJSON.error);
                        } else {
                            alert("Error updating statuses. Please try again");
                        }
                    }
                });
            });

            $('#bulk-print-button').click(function () {
                var prescriptionIdsParam = "prescriptionIds=" + selectedPrescriptionIds.join("&prescriptionIds=");
                window.open(`/PrescriptionRequest/BulkPrintConsignmentNotes?${prescriptionIdsParam}`, '_blank');
            });

            $('#select-all-prescription-checkbox').click(function () {
                if ($(this).prop('checked')) {
                    $('.select-prescription-checkbox').each(function () {
                        $(this).prop('checked', true).change();
                    });
                } else {
                    $('.select-prescription-checkbox').each(function () {
                        $(this).prop('checked', false).change();
                    });
                }
            });

            // Index column
            oTable.on('draw', function () {
                var start = oTable.page.info().start;
                oTable.column(2).nodes().each(function (cell, i) {
                    cell.innerHTML = start + i + 1;
                });
            });

            function format(d) {
                var packageList = '';
                for (let i = 0; i < d.Drugs.length; i++) {
                    packageList += '<div class="row"><div class="col-md-4"><b>' + d.Drugs[i].MedicationName + '</b></div>' +
                        '<div class="col-md-2"><b> Amount :' + d.Drugs[i].Amount + '</b></div>' +
                        '<div class="col-md-2"><b> Dosage :' + d.Drugs[i].Dosage + '</b></div>' +
                        '<div class="col-md-2"><b> Status :' + d.Drugs[i].Status + '</b></div>' +
                        '</div>' +
                        '<div class="row"><div class="col-md-8">Remark : <b>' + d.Drugs[i].Remark + '</b></div></div>';
                }

                var ModeDetail = '';
                if (d.Dispatch !== null) {
                    var packingType = "";
                    switch (d.Dispatch.PackingType) {
                        case 0: packingType = "Not Specified"; break;
                        case 1: packingType = "PillCube Packing"; break;
                    }
                    ModeDetail +=
                        '<div class="row">' +
                        '<div class="col-md-2">Packing Type :</div>' +
                        '<div class="col-md-8"><b>' + packingType + '</b></div>' +
                        '</div>' +
                        '<br/>';
                }
                if (d.Dispatch != null && d.Dispatch.PrescriptionStatus == 2) {
                    ModeDetail +=
                        '<div class="row">' +
                        '<div class="col-md-2">Delivery Address :</div>' +
                        '<div class="col-md-8"><b>' + d.Dispatch.DeliveryAddress + '</b></div>' +
                        '</div>' +
                        '<br/>';
                }

                if (d.Dispatch != null && d.Dispatch.PrescriptionStatus == 3) {
                    ModeDetail +=
                        '<div class="row">' +
                        '<div class="col-md-2">OnSite Address :</div>' +
                        '<div class="col-md-8"><b>' + d.Dispatch.OnSite.OnSiteName + '<br />' + d.Dispatch.OnSite.OnSiteAddress + '</b></div>' +
                        '</div>' +
                        '<br/>';
                }

                if (d.Dispatch != null && d.Dispatch.PrescriptionStatus == 1) {
                    ModeDetail +=
                        '<div class="row">' +
                        '<div class="col-md-2">Outlet Address :</div>' +
                        '<div class="col-md-8"><b>' + d.Dispatch.PharmacyOutlet.FullName + '<br />' + d.Dispatch.PharmacyOutlet.Address + '</b></div>' +
                        '</div>' +
                        '<br/>';
                }

                var html =
                    '<div class="detail">' +
                    '<div class="row">' +
                    '<div class="col-md-1">Height:</div>' +
                    '<div class="col-md-2"><b>' + d.Height + '</b></div>' +
                    '<div class="col-md-1">Weight :</div>' +
                    '<div class="col-md-2"><b>' + d.Weight + '</b></div>' +
                    '<div class="col-md-1">Allergy :</div>' +
                    '<div class="col-md-2"><b>' + d.Allergy + '</b></div>' +
                    '<div class="col-md-1">Document </div>' +
                    '<div class="col-md-2"><a href="' + d.FileUrl + '" target="_blank"><b>View</b></a></div>' +
                    '</div>' +
                    '<br/>' +
                    '<div class="row">' +
                    '<div class="col-md-1">QR ID:</div>' +
                    '<div class="col-md-6"><b>' + d.PrescriptionId + '_' + d.Identifier1 + '</b></div>' +
                    '</div>' +
                    '<br/>' +
                    ModeDetail +
                    '<div class="row">' +
                    '<div class="col-md-2">Drug List :</div>' +
                    '</div>' +
                    packageList +
                    '<br/>' +
                    '</div>';
                return html;
            }
            // Array to track the ids of the details displayed rows
            var detailRows = [];
            $('table#codeList tbody').on('click', 'tr td.details-control', function () {
                var tr = $(this).closest('tr');
                var row = oTable.row(tr);
                var idx = $.inArray(tr.attr('id'), detailRows);

                if (row.child.isShown()) {
                    tr.removeClass('details');
                    row.child.hide();

                    // Remove from the 'open' array
                    detailRows.splice(idx, 1);
                }
                else {
                    tr.addClass('details');
                    row.child(format(row.data())).show();

                    // Add to the 'open' array
                    if (idx === -1) {
                        detailRows.push(tr.attr('id'));
                    }
                }
            });
        });
        WinMove();

        LoadBootstrapValidatorScript(function () {
            const $form = $('#requestExportForm');
            DateRangeInputValidator($form);
            $form.on('success.form.bv', function(e) {
                e.preventDefault();

                const startDate = moment(document.getElementById('startDate').value, 'YYYY-M-D');
                const endDate = moment(document.getElementById('endDate').value, 'YYYY-M-D');
                const responsiblePharmacyEl = document.getElementById('exportResponsiblePharmacy');

                // Pass true to toISOString to keep offset
                // This is so the server can return a file named based on client local time
                const startDateString = startDate.toISOString(true);
                const endDateString = endDate.toISOString(true);
                let url = `/PrescriptionRequest/GetDownloadRequest?startDate=${encodeURIComponent(startDateString)}&endDate=${encodeURIComponent(endDateString)}`;
                if (responsiblePharmacyEl != null) {
                    url += `&responsiblePharmacy=${responsiblePharmacyEl.value}`;
                }
                window.open(url);

                // For whatever reason, Bootstrap Validator likes to disable the button after
                // the form has been submitted, so forcibly re-enable it
                $(e.target).data('bootstrapValidator').disableSubmitButtons(false);
            })
            // Remove successful validation styling
            .on('success.field.bv', function(e, data) {
                data.element.parents('.form-group').removeClass('has-success');
            });

            const $searchForm = $('#searchRequestList');
            $searchForm.on('submit', function (e) {
                e.preventDefault();
                oTable.ajax.reload();
            });
        })
    });
}

function loadPrescriptionLogList(logId, prescriptionId) {
    var oTable = "";
    $(document).ready(function () {
        // Load Datatables and run plugin on tables
        LoadDataTablesScripts(function () {
            oTable = $('table#codeList').DataTable({
                "processing": true,
                "serverSide": true,
                //"order": [[7, "desc"]],
                "ordering": false,
                //"bStateSave": true,
                "bProcessing": true,
                "scrollX": true,
                "dom": "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/PrescriptionRequest/GetPrescriptionLogList',
                    "dataType": 'json',
                    "type": "POST",
                    "data": function (d) {
                        d.dispatchId = logId
                    },
                },
                "columns": [
                    {
                        "class": "",
                        "orderable": false,
                        "data": null,
                        "defaultContent": ""
                    },
                    {
                        "class": "rank",
                        "data": null, // #
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'LogText',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            return data;
                        }
                    },
                    {
                        "data": 'LogUrl',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            return data;
                        }
                    },
                    {
                        "data": 'CreatedDate',
                        "orderable": true,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            if (data) {
                                return moment(data).format('DD/MM/YYYY h:mm a');
                            }
                            return "N/A";
                        }
                    },
                    {
                        "data": null, // #
                        "orderable": false,
                        "className": 'remark',
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var html = '';
                            return html;
                          },
                    },
                ]
            });

            // Index column
            oTable.on('draw', function () {
                var start = oTable.page.info().start;
                oTable.column(1).nodes().each(function (cell, i) {
                    cell.innerHTML = start + i + 1;
                });
            });
        });
        WinMove();

        $("input[name='status']").change(function () {
            if ($(this).val() == "URL") {
                $('#url').show();
            }
            else {
                $('#logurl').val('');
                $('#url').hide();
            }
        });

        $("#sending").on("click", function () {
            var logType = $("input[name='status']:checked").val();

            if (!$('#logtxt').val()) {
                alert("Please fill up");
                return false;
            }

            if (!$('#logurl').val() && logType == "URL") {
                alert("Please fill up");
                return false;
            }

            var model = {
                LogId: logId,
                LogText: $('#logtxt').val(),
                LogUrl: $('#logurl').val(),
                LogType: logType
            };

            var url = "/PrescriptionRequest/AddLogPrescription?prescriptionId=" + prescriptionId;
            jQuery.ajax({
                url: url,
                data: model,
                type: 'POST',
                success: function (data) {
                    if (data !== logId) {
                        logId = data;
                    }
                    alert("Updated");
                    oTable.ajax.reload(null, false);
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    if (XMLHttpRequest.responseJSON) {
                        alert(XMLHttpRequest.responseJSON.error);
                    }
                    else {
                        alert("Error Occured Try agaian !");
                    }
                }
            });
        });
    });
}

function loadCorporateList() {
    var oTable = "";
    $(document).ready(function () {
        // Load Datatables and run plugin on tables
        LoadDataTablesScripts(function () {
            oTable = $('table#codeList').DataTable({
                "processing": true,
                "serverSide": true,
                "order": [[2, "asc"]],
                "ordering": true,
                //"bStateSave": true,
                "bProcessing": true,
                "scrollX": true,
                "dom": "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/Corporate/GetCorporateList',
                    "dataType": 'json',
                    "type": "POST",
                    "data": function (d) {
                        d.searchKey = $("#searchName").val()
                        d.tpaId = $("#tpa").val()
                    },
                },
                "columns": [
                    {
                        "class": "details-control",
                        "orderable": false,
                        "data": null,
                        "defaultContent": ""
                    },
                    {
                        "class": "rank",
                        "data": null, // #
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'BranchName',
                        "name": 'BranchName',
                        "orderable": true,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var html = '<a href="/Corporate/BranchList?corporateId=' + row.CorporateId + '">' + data + '</a>';
                            return html;
                        }
                    },
                    {
                        "data": 'CreatedDate',
                        "name": 'CreatedDate',
                        "orderable": true,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            if (data) {
                                return moment(data).format('DD/MM/YYYY') + ' ' + formatAMPM(moment(data));
                            }
                            return "N/A";
                        }
                    },
                    {
                        "data": "TPAName",
                        "name": "TPAName",
                        "orderable": true,
                        "defaultContent": "-"
                    },
                    {
                        "data": "",
                        "orderable": false,
                        "defaultContent": "-",
                        "render": function (data, type, row, meta) {
                            if (row.TPAName != null) {
                                return row.TPASupplyingPharmacyNames.join('<br />');
                            } else {
                                return row.SupplyingPharmacyNames.join('<br />');
                            }
                        }
                    },
                    {
                        "data": null, // #
                        "orderable": false,
                        "className": 'remark',
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var html = '';

                            //html += '<a href="javascript:void(0);" class="edit"><button class="btn btn-primary" type="button">New Selected Date </button></a>';
                            //html += '<a href="/Corporate/Edit?corporateId=' + row.CorporateId + '">Edit</a>';
                            html += '&nbsp;&nbsp;<a href="/Corporate/Edit?corporateId=' + row.CorporateId + '"><i class="fa fa-edit"></i></a>';
                            if (row.IsBan) {
                                html += '&nbsp;&nbsp;<a href="#" class="unban" data-corporateId="' + row.CorporateId + '">Unban</a>';
                            } else {
                                html += '&nbsp;&nbsp;<a href="#" class="ban" data-corporateId="' + row.CorporateId + '">Ban</a>';
                            }
                            html += '&nbsp;&nbsp;<a href="#" class="del" data-corporateId="' + row.CorporateId + '"><i class="fa fa-trash-o"></i></a>';

                            return html;
                        },
                    },
                ]
            });

            // Index column
            oTable.on('draw', function () {
                var start = oTable.page.info().start;
                oTable.column(1).nodes().each(function (cell, i) {
                    cell.innerHTML = start + i + 1;
                });
            });

            function formatAMPM(data) {
                var date = new Date(data);
                var hours = date.getHours();
                var minutes = date.getMinutes();
                var ampm = hours >= 12 ? 'pm' : 'am';
                hours = hours % 12;
                hours = hours ? hours : 12; // the hour '0' should be '12'
                minutes = minutes < 10 ? '0' + minutes : minutes;
                var strTime = hours + ':' + minutes + ' ' + ampm;
                return strTime;
            }

            function format(d) {
                var html =
                    '<div class="detail">' +
                    '<div class="row">' +
                    '<div class="col-md-3">Address :</div>' +
                    '<div class="col-md-7"><b>' + d.BranchAddress + '</b></div>' +
                    '</div>' +
                    '<div class="row">' +
                    '<div class="col-md-3">Policy - Has Same Day Delivery :</div>' +
                    '<div class="col-md-7"><b>' + (d.PolicyHasSameDayDelivery ? 'Yes' : 'No') + '</b></div>' +
                    '</div>' +
                    '<div class="row">' +
                    '<div class="col-md-3">Policy - Supply Duration :</div>' +
                    '<div class="col-md-7"><b>' + d.PolicySupplyDurationInMonths + ' month(s)</b></div>' +
                    '</div>' +
                    '<div class="row">' +
                    '<div class="col-md-3">Policy - E-MC :</div>' +
                    '<div class="col-md-7"><b>' + d.PolicyEMC + '</b></div>' +
                    '</div>' +
                    '<div class="row">' +
                    '<div class="col-md-3">Policy - Remarks :</div>' +
                    '<div class="col-md-7"><b>' + d.PolicyRemarks + '</b></div>' +
                    '</div>' +
                    '</div>';
                return html;
            }
            // Array to track the ids of the details displayed rows
            var detailRows = [];
            $('table#codeList tbody').on('click', 'tr td.details-control', function () {
                var tr = $(this).closest('tr');
                var row = oTable.row(tr);
                var idx = $.inArray(tr.attr('id'), detailRows);

                if (row.child.isShown()) {
                    tr.removeClass('details');
                    row.child.hide();

                    // Remove from the 'open' array
                    detailRows.splice(idx, 1);
                }
                else {
                    tr.addClass('details');
                    row.child(format(row.data())).show();

                    // Add to the 'open' array
                    if (idx === -1) {
                        detailRows.push(tr.attr('id'));
                    }
                }
            });

            $('table#codeList tbody').on('click', 'a.del', function () {
                var $this = $(this);
                var branchId = $this.attr('data-corporateId');

                if (confirm("Are you sure you want to delete this Corporate?")) {
                    $.post('/Corporate/DeleteCorporate?corporateId=' + branchId, function () {
                        oTable.ajax.reload(null, false);
                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
            });

            $('table#codeList tbody').on('click', 'a.ban', function () {
                var $this = $(this);
                var branchId = $this.attr('data-corporateId');

                if (confirm("Are you sure you want to ban this corporate?")) {
                    $.post('/Corporate/BanCorporate?corporateId=' + branchId, function () {
                        oTable.ajax.reload(null, false);
                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
            });

            $('table#codeList tbody').on('click', 'a.unban', function () {
                var $this = $(this);
                var branchId = $this.attr('data-corporateId');

                if (confirm("Are you sure you want to unban this corporate?")) {
                    $.post('/Corporate/UnbanCorporate?corporateId=' + branchId, function () {
                        oTable.ajax.reload(null, false);
                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
            });
        });
        WinMove();

        LoadDateTimePickerScript(function () {
            $('#startDate,#endDate').datetimepicker({
                timepicker: false,
                format: 'Y/m/d',
                onChangeDateTime: function (current_time, $input) {
                    $input.trigger('input');
                },
            });
        });

        $("#search").on("click", function () {
            oTable.ajax.reload(null, true);
        });

    });

    LoadDateTimePickerScript(function () {
        $('#nextDispenseDate .hasDatepicker').datetimepicker({
            timepicker: false,
            format: 'Y/m/d',
            onChangeDateTime: function (current_time, $input) {
                $input.trigger('input');
            },
        });
    });
}

function loadBranchList(corporateId) {
    var oTable = "";
    $(document).ready(function () {
        // Load Datatables and run plugin on tables
        LoadDataTablesScripts(function () {
            oTable = $('table#codeList').DataTable({
                "processing": true,
                "serverSide": true,
                //"order": [[7, "desc"]],
                "ordering": false,
                //"bStateSave": true,
                "bProcessing": true,
                "scrollX": true,
                "dom": "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/Corporate/GetBranchList',
                    "dataType": 'json',
                    "type": "POST",
                    "data": function (d) {
                        d.searchKey = $("#searchName").val(),
                            d.corporateId = corporateId
                    },
                },
                "columns": [
                    {
                        "class": "details-control",
                        "orderable": false,
                        "data": null,
                        "defaultContent": ""
                    },
                    {
                        "class": "rank",
                        "data": null, // #
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'BranchName',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            return data;
                        }
                    },
                    {
                        "data": 'CreatedDate',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            if (data) {
                                return moment(data).format('DD/MM/YYYY') + ' ' + formatAMPM(moment(data));
                            }
                            return "N/A";
                        }
                    },
                    {
                        "data": null, // #
                        "orderable": false,
                        "className": 'remark',
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var html = '';

                            html += '&nbsp;&nbsp;<a href="/Corporate/EditBranch?branchId=' + row.BranchId + '"><i class="fa fa-edit"></i></a>';
                            html += '&nbsp;&nbsp;<a href="#" class="del" data-branchId="' + row.BranchId + '"><i class="fa fa-trash-o"></i></a>';


                            return html;
                        },
                    },
                ]
            });

            // Index column
            oTable.on('draw', function () {
                var start = oTable.page.info().start;
                oTable.column(1).nodes().each(function (cell, i) {
                    cell.innerHTML = start + i + 1;
                });
            });

            function formatAMPM(data) {
                var date = new Date(data);
                var hours = date.getHours();
                var minutes = date.getMinutes();
                var ampm = hours >= 12 ? 'pm' : 'am';
                hours = hours % 12;
                hours = hours ? hours : 12; // the hour '0' should be '12'
                minutes = minutes < 10 ? '0' + minutes : minutes;
                var strTime = hours + ':' + minutes + ' ' + ampm;
                return strTime;
            }

            function format(d) {
                var html =
                    '<div class="detail">' +
                    '<div class="row">' +
                    '<div class="col-md-1">Address :</div>' +
                    '<div class="col-md-8"><b>' + d.BranchAdress + '</b></div>' +
                    '</div>' +
                    '</div>';
                return html;
            }
            // Array to track the ids of the details displayed rows
            var detailRows = [];
            $('table#codeList tbody').on('click', 'tr td.details-control', function () {
                var tr = $(this).closest('tr');
                var row = oTable.row(tr);
                var idx = $.inArray(tr.attr('id'), detailRows);

                if (row.child.isShown()) {
                    tr.removeClass('details');
                    row.child.hide();

                    // Remove from the 'open' array
                    detailRows.splice(idx, 1);
                }
                else {
                    tr.addClass('details');
                    row.child(format(row.data())).show();

                    // Add to the 'open' array
                    if (idx === -1) {
                        detailRows.push(tr.attr('id'));
                    }
                }
            });

            $('table#codeList tbody').on('click', 'a.del', function () {
                var $this = $(this);
                var branchId = $this.attr('data-branchId');

                if (confirm("Are you sure you want to delete this Branch?")) {
                    $.post('/Corporate/DeleteBranch?branchId=' + branchId, function () {
                        oTable.ajax.reload(null, false);
                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
            });
        });
        WinMove();

        LoadDateTimePickerScript(function () {
            $('#startDate,#endDate').datetimepicker({
                timepicker: false,
                format: 'Y/m/d',
                onChangeDateTime: function (current_time, $input) {
                    $input.trigger('input');
                },
            });
        });

        $("#search").on("click", function () {
            oTable.ajax.reload(null, true);
        });

    });

    LoadDateTimePickerScript(function () {
        $('#nextDispenseDate .hasDatepicker').datetimepicker({
            timepicker: false,
            format: 'Y/m/d',
            onChangeDateTime: function (current_time, $input) {
                $input.trigger('input');
            },
        });
    });
}

function loadPositionList(corporateId) {
  
    var xTable = "";
    $(document).ready(function () {
        // Load Datatables and run plugin on tables
        LoadDataTablesScripts(function () {
            xTable = $('table#tbl_PositionList').DataTable({
                "processing": true,
                "serverSide": true,
                //"order": [[7, "desc"]],
                "ordering": false,
                //"bStateSave": true,
                "bProcessing": true,
                "scrollX": true,
                "dom": "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/Corporate/GetPositionList',
                    "dataType": 'json',
                    "type": "POST",
                    "data": function (d) {
                       
                            d.corporateId = corporateId
                    },
                },
                "columns": [
                  
                    {
                        "data": 'PositionId',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            return data;
                        }
                    },
                    {
                        "data": 'Position',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            return data;
                        }
                    },
                    {
                        "data": 'Active',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            return data;
                        }
                    },
                    {
                        "data": null, // #
                        "orderable": false,
                        "className": 'remark',
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var html = '';

                            html += '&nbsp;&nbsp;<a href="/Corporate/EditPosition?positionId=' + row.PositionId + '"><i class="fa fa-edit"></i></a>';
                            html += '&nbsp;&nbsp;<a href="#" class="del" data-positionId="' + row.PositionId + '"><i class="fa fa-trash-o"></i></a>';


                            return html;
                        },
                    },
                ]
            });

       

           

        
            // Array to track the ids of the details displayed rows
         
            $('table#tbl_PositionList tbody').on('click', 'a.del', function () {
                var $this = $(this);
                var positionId = $this.attr('data-positionId');

                if (confirm("Are you sure you want to delete this Position?")) {
                    $.post('/Corporate/DeletePosition?positionId=' + positionId, function () {
                        xTable.ajax.reload(null, false);
                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
            });

          
        });
        WinMove(); 
    });  
}

function LoadPrescriptionList() {
    $(document).ready(function () {
        // Load Datatables and run plugin on tables
        var oTable = "";
        LoadDataTablesScripts(function () {
            oTable = $('table#prescriptionList').DataTable({
                "processing": true,
                "serverSide": true,
                "ordering": true,
                "order": [[8, "desc"]],
                "pageLength": 30,
                "lengthMenu": [10, 30, 50, 100],
                "bProcessing": true,
                "scrollX": true,
                "dom": "rt<'box-content'<'col-sm-2'i><'col-sm-4'l><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/Prescription/PrescriptionListForSource',
                    "dataType": 'json',
                    "contentType": 'application/json; charset=utf-8',
                    "type": "POST",
                    "data": function (d) {
                        d.prescriptionNo = $("#prescriptionNo").val();
                        d.searchName = $("#fullName").val();
                        d.prescriptionSourceId = $("#prescriptionSource").val();
                        return JSON.stringify(d);
                    }
                },
                "columns": [
                    {
                        "class": "rank",
                        "data": null, // #
                        "orderable": false,
                        "defaultContent": ""
                    },
                    {
                        "name": "PrescriptionId",
                        "data": 'PrescriptionId',
                        "orderable": true,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {

                            return '<a target="_blank" href="' + row.FileUrl + '">' + data + '</a>';
                            //return '#' + data;
                        }
                    },
                    {
                        "name": "PatientName",
                        "orderable": true,
                        "defaultContent": "",
                        "data": function (row, type, set, meta) {
                            if (row.Patient != null) {
                                return row.Patient.FullName;
                            } else {
                                return "N/A";
                            }
                        }
                    },
                    {
                        "name": "DoctorName",
                        "data": 'Doctor.FullName',
                        "orderable": true,
                        "defaultContent": "",
                    },
                    {
                        "data": function (row, type, set, meta) {
                            if (row.PrescribedBy != null) {
                                return row.PrescribedBy.FullName;
                            } else {
                                return row.Doctor.FullName;
                            }
                        },
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'PrescriptionAvailabilityStatus',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            switch (data) {
                                case 1: return "New";
                                case 2: return "Approved";
                                case 3: return "Rejected";
                                case 4: return "Cancelled";
                                default: return "";
                            }
                        }
                    },
                    {
                        "data": 'IsDispensed',
                        "orderable": false,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {

                            if (data) {
                                return "Supplied";
                            } else {
                                return "Not Supplied"
                            }

                        }
                    },
                    {
                        "data": 'DoctorRemarks',
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "name": "CreateDate",
                        "data": 'CreateDate',
                        "orderable": true,
                        "orderSequence": ["desc", "asc"],
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            return '<span>' + moment(data).format('YYYY/MM/DD HH:mm') + ' </span>';
                        }
                    },
                    {
                        "data": null, // #
                        "orderable": false,
                        "className": 'remark',
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var html = '';
                            if (data.IsDigitallySigned) {
                                html = '<a href="/Prescription/DigitalPrescription?id=' + data.PrescriptionId + '" target="_blank"><i class="fa fa-list-alt"></i></a>';
                            }
                            return html;
                        }
                    },
                    {
                        "data": null, // #
                        "orderable": false,
                        "className": 'remark',
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var html = "";
                            html += ` <a target="_blank" href="${row.FileUrl}#"><i class="fa fa-file-o"></i></a>`;
                            if (!data.IsDispensed) {
                                html += `&nbsp;&nbsp;<a href="#" class="del" data-prescription-id="${data.PrescriptionId}"><i class="fa fa-trash"></i></a>`;
                            }
                            return html;
                        }
                    }

                ],
                "rowCallback": function (row, data) {

                }
            });

            // Index column
            oTable.on('draw', function () {
                var start = oTable.page.info().start;
                oTable.column(0).nodes().each(function (cell, i) {
                    cell.innerHTML = start + i + 1;
                });
            });

            $('table#prescriptionList tbody').on('click', 'a.del', function () {
                var prescriptionId = $(this).attr('data-prescription-id');

                if (confirm("Are you sure you want to delete this prescription?")) {
                    $.post('/Prescription/Delete?prescriptionId=' + prescriptionId, () => {
                        oTable.ajax.reload();
                    }).error(err => {
                        alert('An error has occurred. Please try again.');
                    });
                }
            });
        });
        $("#search").on("click", function () {
            oTable.ajax.reload(null, true);
        });

        LoadBootstrapValidatorScript(function () {
            const $form = $('#statsExportForm');
            DateRangeInputValidator($form);
            $form.on('success.form.bv', function(e) {
                e.preventDefault();

                const startDate = moment(document.getElementById('startDate').value, 'YYYY-M-D');
                const endDate = moment(document.getElementById('endDate').value, 'YYYY-M-D');
                const prescriptionSource = document.getElementById('prescriptionStatsSource').value;

                // Pass true to toISOString to keep offset
                // This is so the server can return a file named based on client local time
                const startDateString = startDate.toISOString(true);
                const endDateString = endDate.toISOString(true);
                const url = `/Prescription/PrescriptionCSVStats?startDate=${encodeURIComponent(startDateString)}&endDate=${encodeURIComponent(endDateString)}&prescriptionSourceId=${prescriptionSource}`;
                window.open(url);

                // For whatever reason, Bootstrap Validator likes to disable the button after
                // the form has been submitted, so forcibly re-enable it
                $(e.target).data('bootstrapValidator').disableSubmitButtons(false);
            })
            // Remove successful validation styling
            .on('success.field.bv', function(e, data) {
                data.element.parents('.form-group').removeClass('has-success');
            });
        })
    });
}

function LoadAuditLogList() {
    $(document).ready(function () {
        // Load Datatables and run plugin on tables
        var oTable = "";
        LoadDataTablesScripts(function () {
            oTable = $('table#auditLogList').DataTable({
                "processing": true,
                "serverSide": true,
                "ordering": true,
                "order": [[4, "desc"]],
                "bProcessing": true,
                "scrollX": true,
                "dom": "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/AuditLog/AuditLogList',
                    "dataType": 'json',
                    "type": "POST",
                    "data": function (d) {
                        d.searchTerm = $("#search").val();
                    }
                },
                "columns": [
                    {
                        "class": "rank",
                        "data": null, // #
                        "orderable": false,
                        "defaultContent": ""
                    },
                    {
                        "data": 'UserFullName',
                        "orderable": true,
                        "defaultContent": "",
                    },
                    {
                        "data": "AuditLogTypeStr",
                        "defaultContent": "",
                        "orderable": false
                    },
                    {
                        "data": 'Description',
                        "orderable": false,
                        "defaultContent": ""
                    },
                    {
                        "data": 'CreateDate',
                        "orderable": true,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            return '<span>' + moment(data).format('YYYY/MM/DD hh:mm a') + ' </span>';
                        }
                    }

                ],
                "rowCallback": function (row, data) {

                }
            });

            // Index column
            oTable.on('draw', function () {
                var start = oTable.page.info().start;
                oTable.column(0).nodes().each(function (cell, i) {
                    cell.innerHTML = start + i + 1;
                });
            });



        });
        $("#searchBtn").on("click", function () {
            oTable.ajax.reload(null, true);
        });

        $("#search").keypress(function (e) {
            if (e.which == 13) {
                oTable.ajax.reload(null, true);
            }
        })

        $("#search").on('input', function () {
            if ($(this).val() == "") {
                $("#clearSearchBtn").hide();
            } else {
                $("#clearSearchBtn").show();
            }
        })

        $("#clearSearchBtn").click(function () {
            $("#search").val("");
            oTable.ajax.reload(null, true);
        })
    });
}

function LoadOnSiteEventIndexScripts() {
    $().ready(function() {
        LoadDataTablesScripts(function () {
            var oTable = $('#onSiteEventTable').DataTable({
                processing: true,
                serverSide: true,
                ordering: true,
                order: [[4, "desc"]],
                bProcessing: true,
                scrollX: true,
                dom: "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                ajax: {
                    url: '/OnSiteEvent/GetList',
                    dataType: 'json',
                    contentType: 'application/json; charset=utf-8',
                    type: "POST",
                    data: function (d) {
                        return JSON.stringify(d);
                    },
                },
                columns: [
                    {
                        name: "Code",
                        data: 'Code',
                        orderable: true
                    },
                    {
                        name: "Description",
                        data: "Description",
                        orderable: true
                    },
                    {
                        name: "CorporateName",
                        data: "CorporateName",
                        orderable: true
                    },
                    {
                        name: "EventType",
                        data: "EventType",
                        orderable: true
                    },
                    {
                        name: "CreateDate",
                        data: function(row, type, set, meta) {
                            var date = moment(row.CreateDate);
                            console.log(date)
                            return type === 'display' ? date.format('YYYY/MM/DD hh:mm a') : date;
                        },
                        orderable: true
                    },
                    {
                        name: "IsActive",
                        data: "IsActive",
                        orderable: false,
                        render: function(data, type, row, meta) {
                            return data ? '<i class="fa fa-check"></i>' : "-";
                        }
                    },
                    {
                        name: "UsersCheckedInCount",
                        data: "UsersCheckedInCount",
                        orderable: true
                    },
                    {
                        orderable: false,
                        render: function(data, type, row, meta) {
                            var id = row.Id;
                            return `<a href="/OnSiteEvent/CheckIn?id=${id}">Check in user</a> | <a href="/OnSiteEvent/Details?id=${id}">View checked in users</a> | <a href="/OnSiteEvent/Edit?id=${id}">Edit</a> | <a href="#" class="deleteButton">Delete</a>`;
                        }
                    }
                ],
                createdRow: function(row, data, dataIndex) {
                    $(row).find('.deleteButton').click(function() {
                        if (data.UsersCheckedInCount > 0) {
                            alert('Cannot delete event with checked in users');
                            return;
                        }
                        if (!confirm(`Are you sure you want to delete this event (${data.Code})?`)) {
                            return;
                        }
                        $.ajax({
                            type: 'POST',
                            url: `/OnSiteEvent/Delete?id=${data.Id}`,
                            data: {
                                '__RequestVerificationToken': $('input[name=__RequestVerificationToken]').val()
                            },
                            success: function() {
                                oTable.ajax.reload();
                            },
                            error: function() {
                                alert('Error deleting event. Please try again');
                            }
                        });
                    });
                }
            });
        });
    });
}

function LoadOnSiteEventDetailScripts(eventId) {
    // Convert Create Date to local time
    var date = $("#createDate");
    var dateMoment = moment(date.text().trim(), moment.ISO_8601);
    date.text(dateMoment.format('YYYY/MM/DD hh:mm a'));

    LoadDataTablesScripts(function () {
        var oTable = $('table#userList').DataTable({
            "processing": true,
            "serverSide": true,
            "ordering": true,
            "order": [[3, "asc"]],
            "bProcessing": true,
            "scrollX": true,
            "dom": "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
            "ajax": {
                "url": '/OnSiteEvent/GetUserList',
                "dataType": 'json',
                "contentType": 'application/json; charset=utf-8',
                "type": "POST",
                "data": function(d) {
                    d.eventId = eventId;
                    return JSON.stringify(d);
                }
            },
            "columns": [
                {
                    "name": "FullNameAndUserName",
                    "data": 'FullName',
                    "orderable": true,
                    "defaultContent": "",
                    "render": function (data, type, row, meta) {
                        var imgSrc = (row.Photo != null && row.Photo.ThumbnailUrl !== null) ? row.Photo.ThumbnailUrl : "../Images/user.png";
                        return '<img class="img-rounded" src="' + imgSrc + '" alt=""> ' + data + " ( " + row.UserName + " )";
                    }
                },
                {
                    "data": 'Gender',
                    "orderable": false,
                    "defaultContent": "",
                    "render": function (data, type, row, meta) {
                        var gender = '-';
                        switch (data) {
                            case 1:
                                gender = 'Male';
                                break;
                            case 2:
                                gender = 'Female';
                                break;
                        }
                        return gender;
                    }
                },
                {
                    "data": 'BranchName',
                    "orderable": false,
                    "defaultContent": "",
                },
                {
                    "name": "CheckInDateTime",
                    "data": function (row, type, set, meta) {
                        var d = moment(row.CheckInDateTime);
                        var dateString = d.format('YYYY/MM/DD HH:mm');
                        return dateString;
                    },
                    "orderable": true,
                    "orderSequence": ['desc', 'asc']
                },
            ],
            "createdRow": function (row, data) {
                if (data.IsBan) {
                    $(row).addClass("danger");
                }
            },
        });
    });
}

function LoadManagementAccountIndexScripts(roleToNameMap) {
    $(document).ready(function () {
        // Load Datatables and run plugin on tables
        LoadDataTablesScripts(function () {
            var oTable = $('table#accountList').DataTable({
                "processing": true,
                "serverSide": true,
                "ordering": true,
                "order": [[5, "desc"]],
                "bProcessing": true,
                "scrollX": true,
                "dom": "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/ManagementAccounts/GetList',
                    "contentType": "application/json; charset=utf-8",
                    "dataType": 'json',
                    "type": "POST",
                    "data": function (d) {
                        d.searchKey = $("#searchName").val();
                        d.roleFilter = $("#managementRole").val();
                        return JSON.stringify(d);
                    },
                },
                "columns": [
                    {
                        "class": "details-control",
                        "orderable": false,
                        "data": null,
                        "defaultContent": ""
                    },
                    {
                        "class": "rank",
                        "data": null, // #
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "name": "FullName",
                        "data": 'FullName',
                        "orderable": true,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var imgSrc = (row.Photo != null && row.Photo.ThumbnailUrl !== null) ? row.Photo.ThumbnailUrl : "../Images/user.png";
                            return '<img class="img-rounded" src="' + imgSrc + '" alt=""> ' + data;
                        }
                    },
                    {
                        "name": "UserName",
                        "data": 'UserName',
                        "orderable": true,
                        "defaultContent": ""
                    },
                    {
                        "data": function(row, type, set, meta) {
                            return roleToNameMap[row.Role];
                        },
                        "orderable": false,
                        "defaultContent": ""
                    },
                    {
                        "name": "CreateDate",
                        "data": 'CreateDate',
                        "orderable": true,
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            return '<span>' + moment(data).format('YYYY/MM/DD') + ' </span>';
                        }
                    },
                    {
                        "data": null, // #
                        "orderable": false,
                        "className": 'remark',
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var html = '';
                            html += '<a href="/ManagementAccounts/Edit?userId=' + row.UserId + '">Edit</a>';
                            html += ' <a href="#" class="del" data-userId="' + row.UserId + '">Delete</a>';
                            if (!row.IsBan) {
                                html += '<a href="#" class="ban" data-userId="' + row.UserId + '" data-ban="true"> Ban</a>';
                            }
                            else {
                                html += '<a href="#" class="ban" data-userId="' + row.UserId + '" data-ban="false"> Un-ban</a>';
                            }
                            return html;
                        },
                    },
                ],
                "rowCallback": function (row, data) {
                    if (data.IsBan) {
                        $(row).addClass("danger");
                    }
                },
            });

            // Index column
            oTable.on('draw', function () {
                var start = oTable.page.info().start;
                oTable.column(1).nodes().each(function (cell, i) {
                    cell.innerHTML = start + i + 1;
                });
            });


            // Add event listener for Ban an Unban user
            $('table#accountList tbody').on('click', 'a.ban', function () {
                var $this = $(this);
                var userId = $this.attr('data-userId');

                if ($this.attr('data-ban') == 'true') {
                    $.post('/ManagementAccounts/ban?userId=' + userId, function () {
                        $this.closest('tr').addClass('danger');
                        $this.attr('data-ban', 'false').html('Un-ban');
                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
                else {
                    $.post('/ManagementAccounts/unban?userId=' + userId, function () {
                        $this.closest('tr').removeClass('danger');
                        $this.attr('data-ban', 'true').html('Ban');
                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
            });

            // Add event listener for delete user
            $('table#accountList tbody').on('click', 'a.del', function () {
                var $this = $(this);
                var userId = $this.attr('data-userId');

                if (confirm("Are you sure you want to delete this account?")) {
                    $.post('/ManagementAccounts/Delete?userId=' + userId, function () {
                        oTable.ajax.reload(null, false);
                    }).error(function (jsonError) {
                        alert(jsonError);
                    });
                }
            });

            $("#searchBtn").on('click', function () {
                oTable.ajax.reload(null, true);
            });

            // Row Detail start
            // Add event listener for opening and closing details
            function format(d) {
                var html = "";
                html = '<div class="detail"><b>Address:</b> ' + d.Address + '<br/>' +
                    '<b>Phone:</b> ' + d.PhoneNumber + '<br/>';

                html += '</div>';
                return html;
            }
            // Array to track the ids of the details displayed rows
            var detailRows = [];
            $('table#accountList tbody').on('click', 'tr td.details-control', function () {
                var tr = $(this).closest('tr');
                var row = oTable.row(tr);
                var idx = $.inArray(tr.attr('id'), detailRows);

                if (row.child.isShown()) {
                    tr.removeClass('details');
                    row.child.hide();

                    // Remove from the 'open' array
                    detailRows.splice(idx, 1);
                }
                else {
                    tr.addClass('details');
                    row.child(format(row.data())).show();

                    // Add to the 'open' array
                    if (idx === -1) {
                        detailRows.push(tr.attr('id'));
                    }
                }
            });
            // On each draw, loop over the `detailRows` array and show any child rows
            oTable.on('draw', function () {
                $.each(detailRows, function (i, id) {
                    $('#' + id + ' td.details-control').trigger('click');
                });
            });
            // Row Detail end
        });
        WinMove();
    });
}

function LoadManagementCreateScripts() {
    LoadAvatar(function () {
        $('#managementCreate .thumbnail-wapper').avatar();
    });
}

function LoadManagementEditScripts() {
    LoadAvatar(function () {
        $('#managementEdit .thumbnail-wapper').avatar();
    });
}

function LoadTPAList() {
    var oTable = "";
    $(document).ready(function () {
        // Load Datatables and run plugin on tables
        LoadDataTablesScripts(function () {
            oTable = $('table#tpaList').DataTable({
                "processing": true,
                "serverSide": true,
                "order": [[1, "asc"]],
                "ordering": true,
                "bProcessing": true,
                "scrollX": true,
                "dom": "rt<'box-content'<'col-sm-6'i><'col-sm-6 text-right'p><'clearfix'>>", // UI layout
                "ajax": {
                    "url": '/ThirdPartyAdministrator/GetTPAList',
                    "dataType": 'json',
                    "type": "POST",
                    "data": function (d) {
                        d.pharmacyRoleId = $("#pharmacy").val();
                    },
                },
                "columns": [
                    {
                        "class": "rank",
                        "data": null, // #
                        "orderable": false,
                        "defaultContent": "",
                    },
                    {
                        "data": 'TPAName',
                        "orderable": true,
                        "defaultContent": ""
                    },
                    {
                        "data": 'SupplyingPharmacyNames[<br />]',
                        "orderable": false,
                        "defaultContent": ""
                    },
                    {
                        "data": null, // #
                        "orderable": false,
                        "className": 'actions',
                        "defaultContent": "",
                        "render": function (data, type, row, meta) {
                            var html = '';
                            html += '&nbsp;<a href="/ThirdPartyAdministrator/Edit?tpaId=' + row.TPAId + '"><i class="fa fa-edit"></i></a>';
                            html += '&nbsp;&nbsp;<a href="#" class="del" data-tpaId="' + row.TPAId + '"><i class="fa fa-trash-o"></i></a>';
                            return html;
                        },
                    },
                ]
            });

            // Index column
            oTable.on('draw', function () {
                var start = oTable.page.info().start;
                oTable.column(0).nodes().each(function (cell, i) {
                    cell.innerHTML = start + i + 1;
                });
            });
        });
        WinMove();

        $('table#tpaList tbody').on('click', 'a.del', function () {
            var $this = $(this);
            var tpaId = $this.attr('data-tpaId');

            if (confirm("Are you sure you want to delete this TPA?")) {
                $.post('/ThirdPartyAdministrator/Delete?tpaId=' + tpaId, function () {
                    window.location.reload();
                }).error(function (jsonError) {
                    alert(jsonError);
                });
            }
        });

        $("#search").on("click", function () {
            oTable.ajax.reload(null, true);
        });

    });
}

//////////////////////////////////////////////////////
//////////////////////////////////////////////////////
//
//      MAIN DOCUMENT READY SCRIPT OF DEVOOPS THEME
//
//      In this script main logic of theme
//
//////////////////////////////////////////////////////
//////////////////////////////////////////////////////
$(document).ready(function () {
    $('body').on('click', '.show-sidebar', function (e) {
        e.preventDefault();
        $('div#main').toggleClass('sidebar-show');
        setTimeout(MessagesMenuWidth, 250);
    });
    var height = window.innerHeight - 49;
    $('#main').css('min-height', height)
        .on('click', '.expand-link', function (e) {
            var body = $('body');
            e.preventDefault();
            var box = $(this).closest('div.box');
            var button = $(this).find('i');
            button.toggleClass('fa-expand').toggleClass('fa-compress');
            box.toggleClass('expanded');
            body.toggleClass('body-expanded');
            var timeout = 0;
            if (body.hasClass('body-expanded')) {
                timeout = 100;
            }
            setTimeout(function () {
                box.toggleClass('expanded-padding');
            }, timeout);
            setTimeout(function () {
                box.resize();
                box.find('[id^=map-]').resize();
            }, timeout + 50);
        })
        .on('click', '.collapse-link', function (e) {
            e.preventDefault();
            var box = $(this).closest('div.box');
            var button = $(this).find('i');
            var content = box.find('div.box-content');
            content.slideToggle('fast');
            button.toggleClass('fa-chevron-up').toggleClass('fa-chevron-down');
            setTimeout(function () {
                box.resize();
                box.find('[id^=map-]').resize();
            }, 50);
        })
        .on('click', '.close-link', function (e) {
            e.preventDefault();
            var content = $(this).closest('div.box');
            content.remove();
        });
    $('#locked-screen').on('click', function (e) {
        e.preventDefault();
        $('body').addClass('body-screensaver');
        $('#screensaver').addClass("show");
        ScreenSaver();
    });

    $('#sidebar-left a').click(function () {
        $('.preloader').show();
    });
});

