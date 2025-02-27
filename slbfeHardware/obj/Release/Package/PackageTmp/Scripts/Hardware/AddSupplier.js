var phoneElement = document.getElementById("supplier_phone_primary");
var phone1Element = document.getElementById("supplier_phone_optional_1");
var phone2Element = document.getElementById("supplier_phone_optional_2");

function validatePhone(input){
    if (input.value.length != 10) {
        alert("Invalid Phone Number");
        return false;
    } else {
        return true;
    }
}

function handleSubmit(e){
    var phone = phoneElement.value;
    console.log("test", phone.length);
    console.log("test 2", validatePhone(phone));
    return false;
}
