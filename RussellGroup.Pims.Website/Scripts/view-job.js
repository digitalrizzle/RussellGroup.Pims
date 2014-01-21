var SimpleEngine = {
    compile: function (template) {
        return {
            render: function (context) {
                return template.replace(/\{\{(\w+)\}\}/g,
                    function (match, p1) {
                        return $('<div/>').text(context[p1] || '').html();
                    });
            }
        };
    }
};

function wire(id, url) {
    $('#' + id).typeahead({
        name: id,
        remote: url,
        valueKey: "value",
        template: '<p class="typeahead-description">{{value}}</p>',
        engine: SimpleEngine
    });
}