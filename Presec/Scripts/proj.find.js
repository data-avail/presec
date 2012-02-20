(function() {

  $(function() {
    var LineModel, ViewModel, createMap, createSelector, map, viewModel;
    map = null;
    createMap = function() {
      map = new YMaps.Map($("#map")[0]);
      map.setCenter(new YMaps.GeoPoint(37.64, 55.76), 10);
      map.enableScrollZoom();
      return YMaps.Events.observe(map, map.Events.BoundsChange, function(object) {
        var bounds, id, zoom;
        bounds = map.getBounds();
        zoom = "street";
        if (map.getZoom() <= 10) {
          zoom = "city";
        } else if (map.getZoom() <= 14) {
          zoom = "district";
        }
        id = "" + bounds._left + ";" + bounds._bottom + ";" + bounds._right + ";" + bounds._top + ";" + zoom;
        return OData.read("/Service/PresecService.svc/MapRegions('" + id + "')?$expand=coords", function(result) {
          return $(result.coords).each(function() {
            var placemark;
            placemark = new YMaps.Placemark(new YMaps.GeoPoint(this.lat, this.lon));
            placemark.name = "test";
            placemark.setIconContent("test");
            placemark.description = "test";
            return map.addOverlay(placemark);
          });
        });
      });
    };
    createSelector = function() {
      $("#search_field").autocomplete({
        minLength: 3,
        autoFocus: true,
        source: function(req, res) {
          return OData.read("/Service/PresecService.svc/GeoSuggestions?term=россия, москва, " + req.term, function(data) {
            return res(data.results.map(function(x) {
              return {
                label: x.descr,
                value: x.term,
                gref: x.refer
              };
            }));
          });
        },
        select: function(e, ui) {
          return $(this).data("gref", ui.item.gref);
        }
      });
      return $("#search_field").keypress(function(e) {
        if (e.keyCode === 13) {
          e.preventDefault();
          return $("#search_button").click();
        }
      });
    };
    $(".toggle_layout").hide();
    $("#search_button").click(function() {
      var gref, search;
      search = $("#search_field").val();
      gref = $("#search_field").data("gref");
      return OData.read("/Service/PresecService.svc/Stations?addr=" + search + "&gref=" + gref + "&$expand=lines,near/lines", function(data) {
        var geo, placemark;
        ko.mapping.fromJS(data, {}, viewModel);
        viewModel.search(search);
        if (viewModel.first()) {
          geo = viewModel.first().station.geo;
          if (geo) map.setCenter(new YMaps.GeoPoint(geo.lat(), geo.lon()), 15);
          placemark = new YMaps.Placemark(map.getCenter(), {
            draggable: false,
            style: "default#storehouseIcon"
          });
          return map.addOverlay(placemark);
        }
      });
    });
    LineModel = (function() {

      function LineModel(id, lines) {
        this.id = id;
        this.lines = lines;
      }

      return LineModel;

    })();
    ViewModel = (function() {

      function ViewModel() {
        var _this = this;
        this.search = ko.observable();
        this.results = ko.observableArray();
        this.first = ko.computed(function() {
          return _this.results()[0];
        });
        this.similars = ko.computed(function() {
          var r, _i, _len, _ref, _results;
          _ref = _this.results().filter(function(x) {
            return x !== _this.results()[0];
          });
          _results = [];
          for (_i = 0, _len = _ref.length; _i < _len; _i++) {
            r = _ref[_i];
            _results.push(new LineModel(r.id(), r.lines().filter(function(x) {
              return new RegExp(".*" + (_this.search()) + ".*", "i").test(x.addr());
            }).map(function(x) {
              return x.addr();
            })));
          }
          return _results;
        });
        this.near = ko.computed(function() {
          var n, _i, _len, _ref, _results;
          if (_this.first()) {
            _ref = _this.first().near();
            _results = [];
            for (_i = 0, _len = _ref.length; _i < _len; _i++) {
              n = _ref[_i];
              _results.push(new LineModel(n.id(), n.lines().map(function(x) {
                return x.addr();
              })));
            }
            return _results;
          } else {
            return [];
          }
        });
        this.similarToggle = ko.observable(false);
        this.nearToggle = ko.observable(false);
      }

      return ViewModel;

    })();
    ko.bindingHandlers.toggle = {
      init: function(element, valueAccessor, allBindingsAccessor, viewModel) {
        var opts;
        opts = allBindingsAccessor().toggleOpts;
        return $(element).click(function() {
          $("." + opts.layout).toggle();
          $("i", element).toggleClass("icon-folder-open");
          return $("i", element).toggleClass("icon-folder-close");
        });
      }
    };
    viewModel = new ViewModel();
    ko.applyBindings(viewModel);
    createMap();
    return createSelector();
  });

}).call(this);
