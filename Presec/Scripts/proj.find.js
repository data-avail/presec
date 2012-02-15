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
        this.search = ko.observable();
        this.results = ko.observableArray();
        this.first = ko.computed({
          read: function() {
            return this.results()[0];
          },
          deferEvaluation: true,
          owner: this
        });
        this.similars = ko.computed({
          read: function() {
            var line, res, x, _i, _len, _ref,
              _this = this;
            if (this.results().length > 1) {
              res = [];
              _ref = this.results().splice(0, 1);
              for (_i = 0, _len = _ref.length; _i < _len; _i++) {
                x = _ref[_i];
                line = new LineModel(x.id(), x.lines().filter(function(y) {
                  return new RegExp(".*" + (_this.search()) + ".*", "i").test(y.addr());
                }));
                res.concat(line);
              }
              return res;
            }
          },
          deferEvaluation: true,
          owner: this
        });
      }

      return ViewModel;

    })();
    viewModel = new ViewModel();
    return ko.applyBindings(viewModel);
  });

}).call(this);
