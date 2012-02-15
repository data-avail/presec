(function() {
  $(function() {
    var ViewModel;
    $("#search_button").click(function() {
      return OData.read("/Service.svc/Stations?addr=" + ($("#search_field").val()), function(data) {
        var viewModel;
        viewModel = new ViewModel();
        ko.mapping.fromJS(data, {}, viewModel);
        return ko.applyBindings(viewModel);
      });
    });
    return ViewModel = (function() {
      function ViewModel() {
        this.results = ko.observableArray();
        this.first = ko.computed({
          read: function() {
            return this.results()[0];
          },
          deferEvaluation: true,
          owner: this
        });
      }
      return ViewModel;
    })();
  });
}).call(this);
