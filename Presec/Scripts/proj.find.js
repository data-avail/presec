(function() {

  $(function() {
    var LineModel, ViewModel, createMap, createSelector, map, viewModel;
    map = null;
    createMap = function() {
      map = new YMaps.Map($("#map")[0]);
      return map.setCenter(new YMaps.GeoPoint(37.64, 55.76), 10);
    };
    createSelector = function() {
      var search;
      search = $("#search_field");
      return $("#search_field").autocomplete({
        minLength: 2,
        source: function(req, res) {
          var geocoder;
          geocoder = new YMaps.Geocoder("Россия, Московская область, город Москва, улица " + req.term);
          return YMaps.Events.observe(geocoder, geocoder.Events.Load, function() {
            return res(this._objects.map(function(x) {
              return {
                label: x.text,
                value: x.text,
                key: x._point
              };
            }));
          });
          /*
                    $.ajax
                      url : "http://geocode-maps.yandex.ru/1.x/"
                      data :
                        geocode : "Россия, город Москва, улица #{req.term}"
                        search_type : "all"
                        lang : "ru-RU"
                        key : "AOpIPk8BAAAAVW-PBgIAY2B20rPw1PcOX0Gkn6Ah7e15L9QAAAAAAAAAAACXy_5ZdWMyS9NOY137nkvh99lqew=="
                        format : "json"
                        ll : "37.617671000000016,55.75576799999372"
                        spn: "1.51062"
                      success: (data) ->
                        res data.response.GeoObjectCollection.featureMember
                          #.filter((x) -> x.GeoObject.metaDataProperty.GeocoderMetaData.kind == "street" || x.GeoObject.metaDataProperty.GeocoderMetaData.kind == "district")
                          .map( (x)-> label : x.GeoObject.name, value : x.GeoObject.name, key : x.GeoObject.Point )
          */
        }
      });
    };
    $(".toggle_layout").hide();
    $("#search_button").click(function() {
      var search;
      search = $("#search_field").val();
      return OData.read("/Service.svc/Stations?addr=" + search + "&$expand=lines,near/lines", function(data) {
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
