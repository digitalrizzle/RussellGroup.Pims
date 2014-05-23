function wire(id, url) {
    var bloodhound = new Bloodhound({
        datumTokenizer: Bloodhound.tokenizers.obj.whitespace('value'),
        queryTokenizer: Bloodhound.tokenizers.whitespace,
        remote: url
    });

    bloodhound.initialize();

    $('#' + id).typeahead(null, {
        displayKey: "value",
        source: bloodhound.ttAdapter()
    });
}