var select = document.querySelectorAll("select[id='ProfilePictureUrl']")[0];
if (select.value) {
    document.getElementById('avatar-preview').src = select.value;
}

select.onchange = (e) => {
    document.getElementById('avatar-preview').src = e.srcElement.value;
}
