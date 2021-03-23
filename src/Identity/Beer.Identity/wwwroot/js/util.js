function setVisibility(elem) {
    elem.style.visibility = 'visible';
}

function setLabelColor(label) {
    label.style.color = '#d50000';
}

var elements = document.querySelectorAll('.mdl-textfield__error.field-validation-error');
for (var i = 0; i < elements.length; i++) {
    setVisibility(elements[i]);
}

elements = document.querySelectorAll('.mdl-textfield__input.input-validation-error + label');
for (var i = 0; i < elements.length; i++) {
    setLabelColor(elements[i]);
}

