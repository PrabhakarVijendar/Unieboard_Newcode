/*!
* ClientSide JQuery Ajax Plugin
* Author: Chika Chidi
* version: 1.0.0
* @requires jQuery v1.5 or later.
* Copyright (c) 2013
*/

var cx = {

    // Define Property Names
    propertyUrlName: "data-cx-url",
    propertyFormName: "data-cx-formid",
    propertyReplaceName: "data-cx-replacetargetid",
    propertyAppendName: "data-cx-appendtargetid",
    propertyPrependName: "data-cx-prependtargetid",
    propertyRemoveName: "data-cx-removetargetid",
    propertyOnSuccessName: "data-cx-successcallback",
    propertyOnErrorName: "data-cx-errorcallback",

    //Initialise method
    init: function () {
        // Fetch all Custom Elements to Trigger Asynchronous posts
        $("[" + cx.propertyUrlName + "]").each(function () {

            // Get properties
            var asyncformid = $(this).attr(cx.propertyFormName);
            var asyncurl = $(this).attr(cx.propertyUrlName);
            var asyncreplacetargetid = $(this).attr(cx.propertyReplaceName);
            var asyncappendtargetid = $(this).attr(cx.propertyAppendName);
            var asyncprependtargetid = $(this).attr(cx.propertyPrependName);
            var asyncremovetargetid = $(this).attr(cx.propertyRemoveName);
            var asyncsuccesscallback = $(this).attr(cx.propertyOnSuccessName);
            var asyncerrorcallback = $(this).attr(cx.propertyOnErrorName);

            // Check if element is should post specified for asynchronously
            if (asyncformid != "") {
                // Post Form
                $(this).click(function () {
                    // Get StartUp Action
                    var formstartupactionurl = cx.getFormAction(asyncformid);
                    //Set Target Action
                    cx.setFormAction(asyncformid, asyncurl);
                    // Do Ajax Submit
                    $('#' + asyncformid).ajaxSubmit(cx.formOptions(asyncformid, asyncsuccesscallback, asyncerrorcallback, asyncreplacetargetid, asyncappendtargetid, asyncprependtargetid, asyncremovetargetid, formstartupactionurl));
                });
            }
            else {
                // Post Link
                $(this).click(function () {
                    $.ajax(cx.options(asyncurl, asyncsuccesscallback, asyncerrorcallback, asyncreplacetargetid, asyncappendtargetid, asyncprependtargetid, asyncremovetargetid));
                });
            }
        });
    },
    formOptions: function (asyncformid, successCallback, errorCallback, replacetargetid, appendtargetid, prependtargetid, removetargetid, formstartupurl) {
        return {
            datype: "json",
            type: "POST",
            success: function (response, data) {
                if (data == "success") {

                    //Set Target Action
                    cx.setFormAction(asyncformid, formstartupurl);

                    //Insertion Mode
                    cx.processInsertionMode(response, replacetargetid, appendtargetid, prependtargetid, removetargetid);

                    // post-submit callback   
                    if (successCallback != "undefined" && successCallback != null && successCallback != "") {
                        eval(successCallback);
                    }
//                    if (typeof successCallback == "function") {
//                        successCallback();
//                    }
                }
            },
            error: function (e, data) {

                //Set Target Action
                cx.setFormAction(asyncformid, formstartupurl);

                // post-submit callback 
                if (errorCallback != "undefined" && errorCallback != null && errorCallback != "") {
                    eval(errorCallback);
                }  
//                if (typeof errorCallback == "function") {
//                    errorCallback();
//                }
            }
        };
    },
    options: function (targetUrl, successCallback, errorCallback, replacetargetid, appendtargetid, prependtargetid, removetargetid) {
        return {
            type: "POST",
            url: targetUrl,
            success: function (response, data) {

                //Insertion Mode
                cx.processInsertionMode(response, replacetargetid, appendtargetid, prependtargetid, removetargetid);

                if (typeof successCallback == "function") {
                    successCallback();
                }
            },
            error: function (e, data) {
                // post-submit callback   
                if (typeof errorCallback == "function") {
                    errorCallback();
                }
            }
        };
    },
    setFormAction: function (form, action) {
        if (action != undefined && action != null && action != '') {
            $('#' + form).get(0).setAttribute('action', action);
        }
    },
    getFormAction: function (form) {
        return $('#' + form).get(0).getAttribute('action');
    },
    processInsertionMode: function (response, replacetargetid, appendtargetid, prependtargetid, removetargetid) {
        // Replace
        if (replacetargetid != null && replacetargetid != "") {
            $('#' + replacetargetid).html(response);
        }

        // Append
        if (appendtargetid != null && appendtargetid != "") {
            $('#' + appendtargetid).append(response);
        }

        // Prepend
        if (prependtargetid != null && prependtargetid != "") {
            $('#' + prependtargetid).prepend(response);
        }

        // Remove
        if (removetargetid != null && removetargetid != "") {
            $('#' + removetargetid).remove();
        }
    }
};

$(function () {
    // Initialise Plugin
    cx.init();
});

