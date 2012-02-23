(function() {
  $(function() {
    var LineModel, ViewModel, createMap, createSelector, fCollection, findStation, gCollection, ini, iniStationPlacemark, loadMap, map, viewModel;
    map = null;
    gCollection = new YMaps.GeoObjectCollection();
    fCollection = new YMaps.GeoObjectCollection();
    ini = function() {
      var gStyle, hStyle, sStyle;
      gStyle = new YMaps.Style();
      gStyle.iconStyle = new YMaps.IconStyle(new YMaps.Template('<div class="place agreg">$[iconContent]</div>'));
      gStyle.iconStyle.offset = new YMaps.Point(-20, -40);
      sStyle = new YMaps.Style();
      sStyle.iconStyle = new YMaps.IconStyle(new YMaps.Template('<div class="place station">$[iconContent]</div>'));
      sStyle.iconStyle.offset = new YMaps.Point(-20, -40);
      hStyle = new YMaps.Style();
      hStyle.iconStyle = new YMaps.IconStyle(new YMaps.Template('<div class="place home">$[iconContent]</div>'));
      hStyle.iconStyle.offset = new YMaps.Point(-20, -40);
      YMaps.Styles.add("user#agreg", gStyle);
      YMaps.Styles.add("user#station", sStyle);
      YMaps.Styles.add("user#home", hStyle);
      return YMaps.Placemark.prototype.setCustomIconContent = function(content) {
        this.iconContent = content;
        return this.update();
      };
    };
    iniStationPlacemark = function(placemark, id) {
      placemark.id = id;
      placemark.name = "Участок: " + id;
      placemark.setCustomIconContent(id);
      return YMaps.Events.observe(placemark, placemark.Events.Click, function(prk) {
        return findStation(prk.id);
      });
    };
    loadMap = function() {
      var bounds, id, zoom;
      bounds = map.getBounds();
      zoom = "street";
      if (map.getZoom() <= 10) {
        zoom = "city";
      } else if (map.getZoom() <= 13) {
        zoom = "district";
      }
      id = "" + bounds._left + ";" + bounds._bottom + ";" + bounds._right + ";" + bounds._top + ";" + zoom;
      gCollection.removeAll();
      return OData.read("/Service/PresecService.svc/MapRegions('" + id + "')?$expand=coords", function(result) {
        return $(result.coords).each(function() {
          var placemark, style;
          if (this.type === 2) {
            id = this.descr;
          }
          if (fCollection.filter(function(x) {
            return x.id.toString() === id;
          }).length === 0) {
            style = this.type !== 2 ? "user#agreg" : "user#station";
            placemark = new YMaps.Placemark(new YMaps.GeoPoint(this.lat, this.lon), {
              draggable: false,
              hideIcon: false,
              style: style
            });
            if (this.type !== 2) {
              placemark.name = this.descr;
              placemark.description = "Всего участков: " + this.count;
              placemark.setCustomIconContent(this.count);
            } else {
              iniStationPlacemark(placemark, id);
            }
            return gCollection.add(placemark);
          }
        });
      });
    };
    createMap = function() {
      map = new YMaps.Map($("#map")[0]);
      map.setCenter(new YMaps.GeoPoint(37.64, 55.76), 10);
      map.enableScrollZoom();
      map.addOverlay(gCollection);
      map.addOverlay(fCollection);
      return YMaps.Events.observe(map, map.Events.BoundsChange, function(object) {
        return loadMap();
      });
    };
    findStation = function(search) {
      return OData.read("/Service/PresecService.svc/Stations('" + search + "')?$expand=near,boundary/matches,similar/lines/matches,foundBy/found/matches,foundBy/point", function(data) {
        var geo, placemark;
        fCollection.removeAll();
        ko.mapping.fromJS(data, {}, viewModel);
        geo = viewModel.station().geo;
        if (geo) {
          map.setCenter(new YMaps.GeoPoint(geo.lat(), geo.lon()), 15);
        }
        placemark = new YMaps.Placemark(map.getCenter(), {
          draggable: false,
          style: "user#home"
        });
        iniStationPlacemark(placemark, viewModel.id());
        return fCollection.add(placemark);
      });
    };
    createSelector = function() {
      $("#search_field").autocomplete({
        minLength: 3,
        autoFocus: true,
        source: function(req, res) {
          return OData.read("/Service/PresecService.svc/GeoSuggestions('россия, москва, " + req.term + "')?$expand=suggestions", function(data) {
            return res(data.suggestions.map(function(x) {
              return {
                label: x.descr,
                value: x.term,
                gref: x.refer
              };
            }));
          });
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
      return findStation($("#search_field").val());
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
        this.id = ko.observable();
        this.key = ko.observable();
        this.station = ko.observable();
        this.uik = ko.observable();
        this.similar = ko.observableArray();
        this.near = ko.observableArray();
        this.boundary = ko.observableArray();
        this.matchType = ko.observable();
        this.foundBy = ko.observable();
        this.similarToggle = ko.observable(false);
        this.nearToggle = ko.observable(false);
      }
      ViewModel.prototype.showStation = function(data) {
        return findStation(data.id());
      };
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
    ini();
    createMap();
    createSelector();
    return loadMap();
  });
}).call(this);
