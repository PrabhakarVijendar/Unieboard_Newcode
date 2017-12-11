/*!
* Cognite Ajax Plugin
* Author: Chika Chidi
* version: 1.0.0
* @requires jQuery v1.5 or later and JQuery Form Plugin
* Copyright (c) 2013 Cognite
*/



var cognite_ajax = {

    

    postRemoveCallback: function (targetUrl, removeTarget, callback) {
        $.ajax({
            type: "POST",
            url: targetUrl,
            success: function (response, data) {
                $('#' + removeTarget).remove();
                if (typeof callback == "function") {
                    callback();
                }
            }
        });
    },
    postReplaceCallback: function (targetUrl, replaceTarget, callback) {
        $.ajax({
            type: "POST",
            url: targetUrl,
            success: function (response, data) {
                $('#' + replaceTarget).html(response);
                if (typeof callback == "function") {
                    callback();
                }
            }
        });
    },
    postAppendCallback: function (targetUrl, appendTarget, callback) {
        $.ajax({
            type: "POST",
            url: targetUrl,
            success: function (response, data) {
                $('#' + appendTarget).append(response);
                if (typeof callback == "function") {
                    callback();
                }
            }
        });
    },
    postPrependCallback: function (targetUrl, prependTarget, callback) {
        $.ajax({
            type: "POST",
            url: targetUrl,
            success: function (response, data) {
                $('#' + prependTarget).prepend(response);
                if (typeof callback == "function") {
                    callback();
                }
            }
        });
    },
    initAjaxForm: function (formId) {
        $('#' + formId).ajaxForm(cognite_ajax.ajaxOptions('', '', '', false, false, false, false));
    },
    initAjaxFormCallback: function (formId, successCallback, errorCallback) {
        $('#' + formId).ajaxForm(cognite_ajax.ajaxOptions(successCallback, errorCallback, '', false, false, false, false));
    },
    initAjaxFormReplace: function (formId, updateTargetId, successCallback, errorCallback) {
        $('#' + formId).ajaxForm(cognite_ajax.ajaxOptions(successCallback, errorCallback, updateTargetId, true, false, false, false));
    },
    initAjaxFormAppend: function (formId, updateTargetId, successCallback, errorCallback) {
        $('#' + formId).ajaxForm(cognite_ajax.ajaxOptions(successCallback, errorCallback, updateTargetId, false, true, false, false));
    },
    initAjaxFormPrepend: function (formId, updateTargetId, successCallback, errorCallback) {
        $('#' + formId).ajaxForm(cognite_ajax.ajaxOptions(successCallback, errorCallback, updateTargetId, false, false, true, false));
    },
    initAjaxFormRemove: function (formId, updateTargetId, successCallback, errorCallback) {
        $('#' + formId).ajaxForm(cognite_ajax.ajaxOptions(successCallback, errorCallback, updateTargetId, false, false, false, true));
    },
    ajaxSubmitForm: function (formId, formTargetAction) {
        cognite_ajax.ajaxSetFormAction(formId, formTargetAction);
        $('#' + formId).ajaxSubmit(cognite_ajax.ajaxOptions('', '', '', false, false, false, false));
    },
    ajaxSubmitFormCallback: function (formId, formTargetAction, successCallback, errorCallback) {
        cognite_ajax.ajaxSetFormAction(formId, formTargetAction);
        $('#' + formId).ajaxSubmit(cognite_ajax.ajaxOptions(successCallback, errorCallback, '', false, false, false, false));
    },
    ajaxSubmitReplace: function (formId, formTargetAction, updateTargetId, successCallback, errorCallback) {
        cognite_ajax.ajaxSetFormAction(formId, formTargetAction);
        $('#' + formId).ajaxSubmit(cognite_ajax.ajaxOptions(successCallback, errorCallback, updateTargetId, true, false, false, false));
    },
    ajaxSubmitAppend: function (formId, formTargetAction, updateTargetId, successCallback, errorCallback) {
        cognite_ajax.ajaxSetFormAction(formId, formTargetAction);
        $('#' + formId).ajaxSubmit(cognite_ajax.ajaxOptions(successCallback, errorCallback, updateTargetId, false, true, false, false));
    },
    ajaxSubmitPrepend: function (formId, formTargetAction, updateTargetId, successCallback, errorCallback) {
        cognite_ajax.ajaxSetFormAction(formId, formTargetAction);
        $('#' + formId).ajaxSubmit(cognite_ajax.ajaxOptions(successCallback, errorCallback, updateTargetId, false, false, true, false));
    },
    ajaxSubmitRemove: function (formId, formTargetAction, updateTargetId, successCallback, errorCallback) {
        cognite_ajax.ajaxSetFormAction(formId, formTargetAction);
        $('#' + formId).ajaxSubmit(cognite_ajax.ajaxOptions(successCallback, errorCallback, updateTargetId, false, false, false, true));
    },
    ajaxOptions: function (successCallback, errorCallback, updateTargetId, isReplace, isAppend, isPrepend, isRemove) {
        return {
            datype: "json",
            type: "POST",
            success: function (response, data) {
                if (data == "success") {
                    if (isReplace == true) {
                        $('#' + updateTargetId).html(response);
                    }
                    else if (isAppend == true) {
                        $('#' + updateTargetId).append(response);
                    }
                    else if (isPrepend == true) {
                        $('#' + updateTargetId).prepend(response);
                    }
                    else if (isRemove == true) {
                        $('#' + updateTargetId).remove();
                    }

                    // post-submit callback   
                    if (typeof successCallback == "function") {
                        successCallback();
                    }
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
    ajaxSetFormAction: function (form, action) {
        if (action != undefined && action != null && action != '') {
            $('#' + form).get(0).setAttribute('action', action);
        }
    }
};