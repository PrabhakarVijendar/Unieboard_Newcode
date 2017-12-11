/*!
* Global
* version: 1.0
* Copyright (c) 2013 Cognite Ltd
* Global UniEBoard Functions
*/

// Reset a Form By Id
function ResetForm(formId) {
    return FormManager.Reset(formId);
}

// Show a reveal Modal Dialog
function RevealModal(dataRevealId) {
    RevealManager.Show(dataRevealId);
}

// Reset Form and display modal
function ResetFormAndRevealModal(formId, dataRevealId) {
    if (ResetForm(formId)) {
        RevealManager.Show(dataRevealId);
    }
}

//***********************************************************************************
//* RevealManager - manage all Foundation Reveal Method Calls
//***********************************************************************************
var RevealManager = {};
$(function () {

    RevealManager.Show = function (dataRevealId) {
        try {
            if (dataRevealId != undefined && dataRevealId != '') {
                $('#' + dataRevealId).foundation('reveal', 'open');
            }
        } catch (err) {
        }
    };

});

//***********************************************************************************
//* FormManager - manage all Form Functions
//***********************************************************************************
var FormManager = {};
$(function () {

    FormManager.Reset = function (formId) {
        var reset = false;
        try {
            // Get Form Instance
            var formToReset = $('#' + formId)[0];

            //Check If Exists
            if (formToReset != undefined && formToReset != null) {
                //Reset Form
                formToReset.reset();
                reset = true;
            }
        } catch (err) {
        }
        return reset;
    };



});
