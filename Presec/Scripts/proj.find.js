(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };
  $(function() {
    var LineModel, ViewModel, createMap, createSelector, map, viewModel;
    map = null;
    createMap = function() {
      map = new YMaps.Map($("#map")[0]);
      return map.setCenter(new YMaps.GeoPoint(37.64, 55.76), 10);
    };
    createSelector = function() {
      return $("#search_field").autocomplete({
        minLength: 2,
        source: function(req, res) {
          return OData.read("/Service/PresecService.svc/GeoSuggestions?term=россия, москва, " + req.term, function(data) {
            return res(data.results.map(function(x) {
              return {
                label: x.descr,
                value: x.term
              };
            }));
          });
        }
      });
    };
    $(".toggle_layout").hide();
    $("#search_button").click(function() {
      var search;
      search = $("#search_field").val();
      return OData.read("/Service/PresecService.svc/Stations?addr=" + search + "&$expand=lines,near/lines", function(data) {
        var geo, placemark;
        ko.mapping.fromJS(data, {}, viewModel);
        viewModel.search(search);
        if (viewModel.first()) {
          geo = viewModel.first().station.geo;
          if (geo) {
            map.setCenter(new YMaps.GeoPoint(geo.lat(), geo.lon()), 15);
          }
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
        this.search = ko.observable();
        this.results = ko.observableArray();
        this.first = ko.computed(__bind(function() {
          return this.results()[0];
        }, this));
        this.similars = ko.computed(__bind(function() {
          var r, _i, _len, _ref, _results;
          _ref = this.results().filter(__bind(function(x) {
            return x !== this.results()[0];
          }, this));
          _results = [];
          for (_i = 0, _len = _ref.length; _i < _len; _i++) {
            r = _ref[_i];
            _results.push(new LineModel(r.id(), r.lines().filter(__bind(function(x) {
              return new RegExp(".*" + (this.search()) + ".*", "i").test(x.addr());
            }, this)).map(__bind(function(x) {
              return x.addr();
            }, this))));
          }
          return _results;
        }, this));
        this.near = ko.computed(__bind(function() {
          var n, _i, _len, _ref, _results;
          if (this.first()) {
            _ref = this.first().near();
            _results = [];
            for (_i = 0, _len = _ref.length; _i < _len; _i++) {
              n = _ref[_i];
              _results.push(new LineModel(n.id(), n.lines().map(__bind(function(x) {
                return x.addr();
              }, this))));
            }
            return _results;
          } else {
            return [];
          }
        }, this));
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
