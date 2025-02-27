var total_amount_element = document.getElementById("total_amt");

total_amount_element.addEventListener('input', validateTotalAmount);

function validateTotalAmount(e)
{
    var value = e.target.value;
    
    if (value.length > 3) {
        total_amount_element.value = value 
    }


}

var input = "10000000";

function formatInput(input)
{

}

formatInput(input);


