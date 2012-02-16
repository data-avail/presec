(function() {

  $(function() {
    var LineModel, ViewModel, viewModel;
    $("#search_button").click(function() {
      var search;
      search = $("#search_field").val();
      return OData.read("/Service.svc/Stations?addr=" + search + "&$expand=lines", function(data) {
        ko.mapping.fromJS(data, {}, viewModel);
        return viewModel.search(search);
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
      }

      return ViewModel;

    })();
    viewModel = new ViewModel();
    return ko.applyBindings(viewModel);
  });

}).call(this);
