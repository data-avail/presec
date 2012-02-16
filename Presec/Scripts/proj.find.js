(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };
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
            return this.results().filter(__bind(function(x) {
              return x !== this.results()[0];
            }, this));
          },
          owner: this
        });
      }
      return ViewModel;
    })();
    viewModel = new ViewModel();
    return ko.applyBindings(viewModel);
  });
}).call(this);
