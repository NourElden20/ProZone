$(document).ready(function () {
    $('#Photo').on('change', function () {
        $('.cover-preview').attr('src', window.URL.createObjectURL(this.files[0])).removeClass('d-none');
    });
});

////document.getElementById("#catch").addEventListener('typing')
//const input = document.getElementById('desc');

//input.addEventListener('input', () => {
//    document.getElementById("catch").innerText = input.value;
//});