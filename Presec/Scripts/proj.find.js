(function() {
  $(function() {
    var LineModel, ViewModel, createMap, createSelector, gCollection, loadMap, map, viewModel;
    map = null;
    gCollection = new YMaps.GeoObjectCollection();
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
      return OData.read("/Service/PresecService.svc/MapRegions('" + id + "')?$expand=coords", function(result) {
        gCollection.removeAll();
        return $(result.coords).each(function() {
          var placemark, txt;
          placemark = new YMaps.Placemark(new YMaps.GeoPoint(this.lat, this.lon), {
            draggable: false,
            style: "default#storehouseIcon"
          });
          txt = this.descr;
          if (this.count > 1) {
            txt = "" + txt + " (" + this.count + ")";
          }
          placemark.id = placemark.name = placemark.description = txt;
          return gCollection.add(placemark);
        });
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
      return OData.read("/Service/PresecService.svc/Stations('" + search + "')?$expand=near,boundary/matches,similar/lines/matches", function(data) {
        var geo, placemark;
        ko.mapping.fromJS(data, {}, viewModel);
        geo = viewModel.station().geo;
        if (geo) {
          map.setCenter(new YMaps.GeoPoint(geo.lat(), geo.lon()), 15);
        }
        placemark = new YMaps.Placemark(map.getCenter(), {
          draggable: false,
          style: "default#storehouseIcon"
        });
        return map.addOverlay(placemark);
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
        this.id = ko.observable();
        this.key = ko.observable();
        this.station = ko.observable();
        this.uik = ko.observable();
        this.similar = ko.observableArray();
        this.near = ko.observableArray();
        this.boundary = ko.observableArray();
        /*
                    @results = ko.observableArray()
                    @first = ko.computed => @results()[0]
                    @similars = ko.computed =>
                      for r in @results().filter((x) => x != @results()[0])
                        new LineModel r.id(), r.lines().filter((x) => new RegExp(".*" + (@search()) + ".*", "i").test(x.addr())).map((x)=>x.addr())
                    @near = ko.computed =>
                      if @first()
                        for n in @first().near()
                            new LineModel n.id(), n.lines().map (x) => x.addr()
                      else
                        []
                    */
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
    createSelector();
    return loadMap();
  });
}).call(this);
