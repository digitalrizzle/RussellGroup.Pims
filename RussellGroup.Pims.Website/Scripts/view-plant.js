var photo = $('#photo');

$(document).ready(function () {
    var photoSrc = $('#FileViewModel_Src').attr('value');
    photo.attr('src', photoSrc);
});

$('#uploadPhoto').change(function () {
    read(this);
});

function read(input) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.file = input.files[0];

        reader.onload = function (e) {
            if (input.id === "uploadPhoto") {
                photo.attr('src', e.target.result);
                $('#FileViewModel_Src').attr('value', e.target.result);
                $('#FileViewModel_FileName').attr('value', this.file.name);
            }
        }

        reader.readAsDataURL(input.files[0]);
    }
}

$('#resetPhoto').click(function () {
    $('#FileViewModel_Src').attr('value', '');
    photo.attr('src', '');
});
