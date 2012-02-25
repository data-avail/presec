(function() {
  $(function() {
    var LineModel, ViewModel, activePlacemark, createMap, createSelector, findStation, gCollection, ini, iniPlacemark, loadMap, map, resetActivePlacemark, setStartPoint, showPlacemarks, startLoading, stopLoading, subscribePlacemarkClick, viewModel;
    map = null;
    gCollection = new YMaps.GeoObjectCollection();
    activePlacemark = null;
    ini = function() {
      var gStyle, sStyle;
      gStyle = new YMaps.Style();
      gStyle.iconStyle = new YMaps.IconStyle(new YMaps.Template('<div class="place agreg">$[iconContent]</div>'));
      gStyle.iconStyle.offset = new YMaps.Point(-20, -40);
      sStyle = new YMaps.Style();
      sStyle.iconStyle = new YMaps.IconStyle(new YMaps.Template('<div class="place $[className]">$[iconContent]</div>'));
      sStyle.iconStyle.offset = new YMaps.Point(-20, -40);
      YMaps.Styles.add("user#agreg", gStyle);
      YMaps.Styles.add("user#station", sStyle);
      return YMaps.Placemark.prototype.setCustomIconContent = function(content, className) {
        this.iconContent = content;
        this.className = className;
        return this.update();
      };
    };
    startLoading = function() {
      return $("#loading").fadeIn("slow");
    };
    stopLoading = function() {
      return $("#loading").fadeOut("slow");
    };
    subscribePlacemarkClick = function(placemark) {
      return YMaps.Events.observe(placemark, placemark.Events.Click, function(placemark) {
        return findStation(placemark.id, false);
      });
    };
    iniPlacemark = function(placemark, className) {
      if (placemark.grpCount > 1) {
        placemark.name = "Участок: " + placemark.id + "...";
        return placemark.setCustomIconContent("" + placemark.id + "+", className);
      } else {
        placemark.name = "Участок: " + placemark.id;
        return placemark.setCustomIconContent(placemark.id, className);
      }
    };
    showPlacemarks = function(placemarks) {
      var descr, grp, prk, _i, _len, _ref, _results;
      _ref = _.toArray(_.groupBy(placemarks, function(prk) {
        return prk._point.toString();
      }));
      _results = [];
      for (_i = 0, _len = _ref.length; _i < _len; _i++) {
        grp = _ref[_i];
        descr = (grp.map(function(x) {
          return x.id;
        })).toString();
        prk = _.sortBy(grp, function(x) {
          return x.id;
        })[0];
        prk.grpCount = grp.length;
        iniPlacemark(prk, "station");
        prk.description = descr;
        subscribePlacemarkClick(prk);
        gCollection.add(prk);
        _results.push(resetActivePlacemark(activePlacemark));
      }
      return _results;
    };
    resetActivePlacemark = function(newActivePlacemark) {
      var prk;
      if (activePlacemark) {
        prk = gCollection._objects.filter(function(x) {
          return x.id === activePlacemark.id;
        })[0];
        if (prk) {
          iniPlacemark(prk, "station");
        }
      }
      if (newActivePlacemark) {
        prk = gCollection._objects.filter(function(x) {
          return x._point.equals(newActivePlacemark._point);
        })[0];
        if (prk) {
          prk.id = newActivePlacemark.id;
          iniPlacemark(prk, "home");
        }
      }
      return activePlacemark = newActivePlacemark;
    };
    setStartPoint = function() {
      if (navigator.geolocation) {
        return navigator.geolocation.getCurrentPosition(function(geo) {
          return loadMap(geo.coords);
        }, function(error) {
          return loadMap(YMaps.location ? YMaps.location : null);
        });
      }
    };
    loadMap = function(point) {
      var bounds, id, zoom;
      if (point) {
        map.setCenter(new YMaps.GeoPoint(point.longitude, point.latitude), 14);
      }
      bounds = map.getBounds();
      zoom = "street";
      if (map.getZoom() <= 10) {
        zoom = "city";
      } else if (map.getZoom() <= 13) {
        zoom = "district";
      }
      id = "" + bounds._left + ";" + bounds._bottom + ";" + bounds._right + ";" + bounds._top + ";" + zoom;
      return OData.read("/Service/PresecService.svc/MapRegions('" + id + "')?$expand=coords", function(result) {
        var prks;
        prks = [];
        gCollection.removeAll();
        $(result.coords).each(function() {
          var placemark, style;
          if (this.type === 2) {
            id = this.descr;
          }
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
            return gCollection.add(placemark);
          } else {
            placemark.id = id;
            return prks.push(placemark);
          }
        });
        if (prks.length) {
          return showPlacemarks(prks);
        }
      });
    };
    createMap = function() {
      map = new YMaps.Map($("#map")[0]);
      map.setCenter(new YMaps.GeoPoint(37.64, 55.76), 10);
      map.enableScrollZoom();
      map.addOverlay(gCollection);
      return YMaps.Events.observe(map, map.Events.BoundsChange, function(object) {
        return loadMap();
      });
    };
    findStation = function(search, setCenter) {
      if (!search || (activePlacemark && activePlacemark.id === search)) {
        return;
      }
      startLoading();
      return OData.read("/Service/PresecService.svc/Stations('" + search + "')?$expand=near,boundary/matches,similar/lines/matches,foundBy/found/matches,foundBy/point,twins", function(data) {
        var activePrk, geo, pt;
        ko.mapping.fromJS(data, {}, viewModel);
        activePrk = null;
        if (viewModel.matchType() !== "not_found") {
          geo = viewModel.station().geo;
          if (geo) {
            pt = new YMaps.GeoPoint(geo.lat(), geo.lon());
            activePrk = new YMaps.Placemark(pt, {
              draggable: false,
              hideIcon: false,
              style: "user#station"
            });
            activePrk.id = viewModel.id();
          }
          if (setCenter && pt && !pt.equals(map.getCenter())) {
            map.setCenter(pt, 15);
          }
        }
        resetActivePlacemark(activePrk);
        return stopLoading();
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
      return findStation($("#search_field").val(), true);
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
        this.twins = ko.observableArray();
        this.near = ko.observableArray();
        this.boundary = ko.observableArray();
        this.matchType = ko.observable();
        this.foundBy = ko.observable();
        this.similarToggle = ko.observable(false);
        this.nearToggle = ko.observable(false);
      }
      ViewModel.prototype.showStation = function(data) {
        return findStation(data.id(), true);
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
    return setStartPoint();
  });
}).call(this);
