// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

//LoginController
function RegisterNewUser2() {
    //var form = document.getElementsByName('register-form');
    var data1 = JSON.stringify({ IsPrimary: 'true'});

    $.ajax({
        type: "POST",
        url: "/Login/RegisterNewUser",
        data: data1,
        contentType: 'application/json',
        dataType: "json",
        async: true,
        success: function (msg) {
            ServiceSucceeded(msg);
        },
        error: function () {
            return "error";
        }
    });
}


function RegisterNewUser() {
    var registrationModel = {
        Username: document.getElementById('inputUsername').value,
         Password: document.getElementById('inputPassword').value,
         Email: document.getElementById('inputEmail').value,
         ApiKey: document.getElementById('apiKey').value,
         SecretKey: document.getElementById('secretKey').value,
         PaperApiKey: document.getElementById('paperApiKey').value,
         PaperSecretKey: document.getElementById('paperSecretKey').value
    };





    //var canvasField = {
    //    id: 56456,
    //    recStartx: 4565,
    //    recStarty: 6456,
    //    recWidth: 6456,
    //    recHeight: 6456
    //};
    //var canvasFields = new Array();
    //canvasFields.push(canvasField);

    //var registrationModels = new Array();
    //registrationModels.push(registrationModel);

    //var myJsonString = JSON.stringify(registrationModels);  //only for test

    
    $.ajax({
        url: "/Login/RegisterNewUser",
        type: 'post',
        contentType: 'application/json',
        data: JSON.stringify(registrationModel),
        //async: true,
        //data: JSON.stringify(canvasFields),
        success: function (result) {
            if (result.redirectUrl !== undefined) {
                window.location.replace(result.redirectUrl);
            }
            else {
                // No redirect found, do something else
            }
        },
        error: function (errMsg) {
            alert(errMsg);
        }
    });
    
}