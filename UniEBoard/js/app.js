$(document).foundation();
/*
	 _____     _ _____ _____               _ 
	|  |  |___|_|   __| __  |___ ___ ___ _| |
	|  |  |   | |   __| __ -| . | .'|  _| . |
	|_____|_|_|_|_____|_____|___|__,|_| |___|
                        
	UniEBoard
	Author    : Andy Proios
	Version   : 1.2
*/


/* Imports
--------------------------------------*/
//CKEDITOR.disableAutoInline = true;

/* Bindings */

/* Filter Controls */
BindFilterControls();

/* Asset List and Tags */
BindAssetControls();
BindAssetLinkControls();
BindAutocompleteTagAsset();

BindAutocomplete();
BindReveal();

/* Units */
BindingUnits();
BindingUnitsDashboard();
BindAddAsset();
BindRemoveAsset();

BindToggleControls();

BindDatePicker();

BindTableDragNDrop();

BindToggleSubmission();
BindSubmitGrades();

BindFilterSubmissions();
BindFilterSubmissionsDropList();


BindFilterToggleSubmission();

/* Validation Ajax Forms */
function validateForm() {
    return $('form').validate().form();
}

function ResetAddTaskForm() {
    $('#Title').val('');
    $('#Note').val('');
    $('#Deadline').val('');
    $('.validation-summary-errors').remove();
    BindDatePicker();
}

function ResetMessageForm() {
    $('#textMessage').val('');
    $('#users').html('');
    $('#users').hide();
    $('#ddCourses :nth-child(0)').prop("selected", true);
}


$(document).ready(function () {
    /* DateTime picker fix for chrome and safari / webkit browsers */
    if ($.validator) {
        $.validator.methods.date = function (value, element) {
            var dateRegex = /^(0?[1-9]\/|[12]\d\/|3[01]\/){2}(19|20)\d\d$/;
            console.log(dateRegex);
            return this.optional(element) || dateRegex.test(value);
        };
    }
    
    /* sticky header */
    $(window).scroll(function () {
        if ($(window).scrollTop() > 60) {
            $('#main-nav').addClass('fixed');
        }
        else if ($(window).scrollTop() < 60 && $('#main-nav').hasClass('fixed')) {
            $('#main-nav').removeClass('fixed');
            $('#main-nav').css('display', 'block');
        }
    });

    /*disable submission for ajax forms*/
    $('#addTaskForm').on('submit', false);

    $('body').on('click', '.submitLink', function () {
        var formId = $(this).data("form");
        $("#" + formId).submit();
    });

    /* Filter Controls */
    $('input.FilterTextBox').keyup(function () {
        var value = $(this).val();
        var url = $(this).attr('data-url');
        var target = $(this).attr('data-target');

        $.ajax({
            url: url,
            type: 'POST',
            data: "{ filter:'" + value + "'}",
            contentType: "application/json; charset=utf-8",
            dataType: "html",
            success: function (response, data) {
                $(target).html(response);
            },
            error: function (jXhr) {
                if (typeof console != 'undefined')
                    alert("Error Updating Object")
            }
        });
    });

    $('.changeToOneDayBefore').on('click', function () {
        var dateField = $(this).attr('data-target');
        var parts = unformatDate($(dateField).val()).split('/');

        var date = new Date(parts[2], parseInt(parts[1]) - 1, parts[0]);

        var d = date.getDate() - 1;
        var m = date.getMonth();
        var y = date.getFullYear();

        var newDate = new Date(y, m, d);
        var newDateStr = ('0' + newDate.getDate()).slice(-2) + '/' + ('0' + (newDate.getMonth() + 1)).slice(-2) + '/' + newDate.getFullYear();

        $(dateField).val(newDateStr);
        Filtering($(dateField));
        $(dateField).val(formatDate(newDateStr));

    });

    $('.changeToOneDayAfter').on('click', function () {
        var dateField = $(this).attr('data-target');
        var parts = unformatDate($(dateField).val()).split('/');

        var date = new Date(parts[2], parseInt(parts[1]) - 1, parts[0]);

        var d = date.getDate() + 1;
        var m = date.getMonth();
        var y = date.getFullYear();

        var newDate = new Date(y, m, d);
        var newDateStr = ('0' + newDate.getDate()).slice(-2) + '/' + ('0' + (newDate.getMonth() + 1)).slice(-2) + '/' + newDate.getFullYear();

        $(dateField).val(newDateStr);
        Filtering($(dateField));
        $(dateField).val(formatDate(newDateStr));
    });

    /* Fake upload
    --------------------------------------*/
    $('.upload-file').on('focus', function () {
        var filename = $('input[type=file]').val().replace(/C:\\fakepath\\/i, '');
        $('.upload-fake').html(filename);
    });

    /* Reveal alerts
	--------------------------------------*/

    $('.alert-reveal').on('click', function () {
        $(this).siblings('.alert-panel').toggleClass('alert-open');
    });



    /* Add New button Toggle
    --------------------------------------*/
    $('.create-new').on('click', function () {
        $('#AddNew').toggleClass('hide');
        $(this).children('i').toggleClass('icon-minus');
    });




    /* DateTime
    --------------------------------------*/
    // avoid filling datetime field from default datetime
    //$('input.datetimepicker').val("");

    /*$('input.datetimepicker').on('focus', function () {
        // hack to avoid double clicks on randomly generated date fields
        // not recomended though
        $(this).datepicker({ dateFormat: "dd/mm/yy" }).datepicker("show");
    });
    $('input.dateandtimepicker').on('focus', function () {
        // hack to avoid double clicks on randomly generated date fields
        // not recomended though
        $(this).datetimepicker({ dateFormat: "dd/mm/yy" }).datetimepicker("show");
    });*/




    $('[data-focus-id]').click(function () {
        var targetElemId = $(this).attr("data-focus-id");
        $('#' + targetElemId).focus();
    });




    /*
    Table row drag and drop
    -------------------------------------
    */


    /*
        Ajax dropdown control
        -----------------------------
    */
    //console.log("just before ajaxDropDown");
    $('.ajaxDropDown').each(function (i, el) {
        console.log("over here!");
        var el = $(el);
        var url = el.data('source');
        var target = el.data('targetelementid');
        var selectedFilter = $("#SelectedFilter").val();

        el.change('on', function () {
            var selectedIndex = el.val();
            $.post(url,
                    {
                        id: selectedIndex,
                        filter: selectedFilter
                    }, function (data, textStatus, jhx) {
                        $('#' + target).html(data);
                        BindTableDragNDrop();
                        $(document).foundation();
                    }).fail(function (data, status, jhx) {
                        console.log("there was an error " + data + status + jhx);
                    });
        });

    });

    /*
        Confirmation dialog
        ----------------------------------
    */

    $('.confirmation').click(function (e) {
        var confirm = confirm("Are you sure you want to delete this record?");
        return confirm;
    });




    /*
        Toggle Submission
    */
    //Hide All Submission Sections


    /*
        Assets
    */
    $('#ddAssetFormat').change(function () {
        var dataId = $('#ddAssetType').data('id');
        var anchor = $("div[data-id='" + dataId + "']:first-child")
        if ($(this).val() == 4) {
            $('#ddAssetType').val(1);
            div.text('Web Url');
            $('#ddAssetType').prop('disabled', true);
        } else {
            $('#ddAssetType').prop('disabled', false);
        }
    });

    /* Junk left from the demo remove this?
    --------------------------------------
    $('.edit-view').live('click', editMode);
    $('#loginBtn').live('click', loginUser);

    function getParameterByName(name) {
    var match = RegExp('[?&]' + name + '=([^&]*)')
    .exec(window.location.search);
    return match ?
    decodeURIComponent(match[1].replace(/\+/g, ' '))
    : null;
    }


    function loginUser() {
    var who = $('#loginType option:selected').text();

    if (who == 'Student') {
    window.location = 'dashboard.htm';
    }
    else if (who == 'Teacher') {
    window.location = 'teacher-dashboard.htm';
    }
    else {
    $('#alert').css('display', 'block');
    }

    }

    function editMode() {
    if (editing == false) {
    //console.log("enable");
    $('#editable').attr('contentEditable', 'true');
    var editor = CKEDITOR.inline(document.getElementById('editable'));
    $('.edit-view').toggleClass('success');
    $('.edit-view').toggleClass('alert');
    $('.edit-view').html('Save');
    editing = true;
    }
    else {
    CKEDITOR.instances.editable.destroy();
    $('#editable').attr('contentEditable', 'false');
    $('.edit-view').toggleClass('success');
    $('.edit-view').toggleClass('alert');
    $('.edit-view').html('Edit Page');
    editing = false;
    }
    }

    editable = getParameterByName('mode');

    editing = false;

    $('#editable').attr('contentEditable', 'false');

    if (editable == 'edit') {
    $('.edit-view').css('display', 'block');
    $('.title-area h1').html('<a href="teacher-dashboard.htm">Professor Andy Pro <small>logout</small></a>');
    $('.top-bar-section').html('<ul class="right"> <li class="divider"></li> <li><a class="active" href="teacher-courses.htm">Manage Courses</a></li> <li class="divider"></li> <li><a class="" href="teacher-assignments.htm">Manage Assignments</a></li> <li class="divider"></li> <li><a class="" href="teacher-grades.htm">Grade Work</a></li> <li class="divider"></li> <li><a class="" href="teacher-discussions.htm">Discussions</a></li> <li class="divider"></li> <li class="has-form"> <form> <div class="row collapse"> <div class="large-10 small-8 columns"> <input type="text"> </div> <div class="large-2 small-3 columns"> <a class="postfix button expand" href="#"><i class="icon-search"></i></a> </div> </div> </form> </li> </ul>');
    $('.breadcrumbs').html('<li><a href="teacher-dashboard.htm">My Dashboard</a></li><li><a href="teacher-courses.htm">Manage Courses</a></li><li class="current"><a href="#">Internet History, Technology, and Security</a></li>');
    }

    */


    $('body').on('change', '[data-moduleId]', function () {
        UpdateModule($(this));
    });

    $(document).on("click", ".ajaxLink", function () {
        RemoveCourseFromModule($(this));
    });

    $('body').on('click', '.btnClick', function () {
        var url = $(this).data('source');
        var dd = $(this).data('dependent-control-id');
        $.ajax({
            type: 'POST',
            cache: false,
            url: url,
            //data: "{draggedItemId:'" + itemIds + "', type:'" + t + "'}",
            data: {
                courseId: dd
            },
            contentType: 'application/json; charset=utf-8',
            async: false,
            success: function (data, textStatus, jhx) {
                console.log("successfully finished...");
            },
            error: function (data) {
                console.debug(data);
            }
});
    })

});


function BindTableDragNDrop() {

    $('.tableRowDnD tbody').each(function (i, el) {
        var el = $(el);
        var url = el.parent().data('source');
        var t = el.parent().data('type');
        var prevIndex, newIndex;

        el.sortable({
            start: function (event, ui) {
                prevIndex = ui.item.index();
            },
            update: function (event, ui) {

                /*var itemId = ui.item.attr("id");
                newIndex = ui.item.index();
                var draggedDown = newIndex > prevIndex;
                console.log("newIndex: " + newIndex + " prevIndex: " + prevIndex + " draggedDown: " + draggedDown);*/
                var itemIds = [];
                el.children('tr').each(function () {
                    var row = $(this);
                    itemIds.push(row.attr("id"));
                });

                /*$.post(url,
                    {
                        draggedItemId: itemIds,
                        draggedDown: draggedDown,
                        prevIndex: prevIndex,
                        newIndex: newIndex
                    }, function (data, textStatus, jhx) {
                    console.log("successfully finished...");
                });*/
                $.ajax({
                    type: 'POST',
                    cache: false,
                    url: url,
                    //data: "{draggedItemId:'" + itemIds + "', type:'" + t + "'}",
                    data: JSON.stringify({ draggedItemId: itemIds, type: t }),
                    contentType: 'application/json; charset=utf-8',
                    async: false,
                    success: function (data, textStatus, jhx) {
                        console.log("successfully finished...");
                    },
                    error: function (data) {
                        console.debug(data);
                    }
                });

            },
            revert: true
        });
    });
}

function BindToggleSubmission() {
    $('.toggleSubmission').each(function () {
        // Hide when page first loads
        var toggleForm = '#' + $(this).attr('data-toggle-id');
        $(toggleForm).hide();

        // Add click event to toggle display
        $(this).click(function (event) {
            event.preventDefault();
            var toggleForm = '#' + $(this).attr('data-toggle-id');
            $(toggleForm).fadeToggle("slow");
        });
    });
}

/* Binding toggleSubmission Grades */
function BindFilterToggleSubmission() {
    $('body').on('click', '.toggleSubmission', function () {
        var assignmentId = $(this).attr('data-assignmentid');
        var action = $(this).attr('data-action');
        var target = $(this).attr('data-target');

        if (action == 'show') {
            console.log("showing");
            console.log(target);
            $(this).attr('data-action', 'hide');
            $('#iconForAssign_' + assignmentId).toggleClass('icon-plus icon-minus');

            //$('#'+ target).css('display', 'block');
            $('#' + target).attr('style', 'display:block');
            //$('#'+ target).show();
            
        }
        else if (action == 'hide') {
            console.log("hiding");
            console.log(target);
            $(this).attr('data-action', 'show');
            $('#iconForAssign_' + assignmentId).toggleClass('icon-minus icon-plus');

            //$('#'+ target).css('display', 'none');
            $('#' + target).attr('style', 'display:none');
            //$('#'+ target).hide();
           
        }

    });
}

/* Binding Filtering Controls */

function BindFilterControls() {
    $('body').on('keyup', '.FilterTextBox', function () {
        Filtering($(this));
    });

    $('body').on('change', '.DateFilterTextBox', function () {

        //----------------------
        //Date must come dd/mm/yyyy, if it comes in other way, we will format it to dd/mm/yyyy
        var date = $(this).val();
        var dateSplitted = date.split(' ');
        if (typeof dateSplitted[1] != 'undefined') {
            $(this).val(unformatDate($(this).val()));
        }
        //----------------------

        Filtering($(this));
        $(this).val(formatDate($(this).val()));

    });
}

/* Binding Toggle Controls */
function BindToggleControls() {
    $('body').on('click', '.icon-collapse', function () {
        $(this).parent().parent().next('.detail').toggle();
        $(this).toggleClass('icon-collapse-top');
    });
}

/* Binding DatePicker Function */
function BindDatePicker() {
    //console.log("Binding Date Picker");
    $('body').on('focus', '.datetimepicker', function () {
        // hack to avoid double clicks on randomly generated date fields
        // not recomended though
        $(this).datepicker({ dateFormat: "dd/mm/yy" }).datepicker("show");
    });

    $('body').on('focus', '.dateandtimepicker', function () {
        // hack to avoid double clicks on randomly generated date fields
        // not recomended though
        $(this).datetimepicker({ dateFormat: "dd/mm/yy" }).datetimepicker("show");
    });

}

/* Asset Functions */

function BindAssetControls() {
    $('[data-assetId]').bind('change', function () {
        UpdateAsset($(this));
    });
}

function BindAssetLinkControls() {
    $(document).on("click", ".ajaxTagRemoveLink", function () {
        RemoveTagFromAsset($(this));
    });
}

function BindAutocompleteTagAsset() {
    $('input.autocompleteTagAsset').each(function (i, el) {
        el = $(el);
        var operationType = el.attr('data-type');
        el.autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: el.attr('data-source'),
                    dataType: 'json',
                    data: {
                        term: request.term
                    },
                    type: 'POST',
                    success: function (data) {
                        response($.map(data, function (item, i) {
                            return {
                                label: item.label,
                                value: item.id
                            }
                        }));
                    }
                });
            },
            change: function (event, ui) {
                switch (operationType) {
                    case 'updateAsset':
                        UpdateAsset($(this));
                        break;
                    case 'filterAsset':
                        filterAsset($(this));
                        break;
                }
            },
            select: function (event, ui) {
                event.preventDefault();
                el.val(ui.item.label);
                el.attr('data-sourceid', ui.item.value.toString());

                switch (operationType) {
                    case 'updateAsset':
                        UpdateAsset($(this));
                        break;
                    case 'filterAsset':
                        filterAsset($(this));
                        break;
                }
            },
            minLength: 1
        }).data("ui-autocomplete")._renderItem = function (ul, item) {
            return $("<li />")
                .data("ui-autocomplete-item", item)
                .append("<a>" + item.label + "</a>")
                .appendTo(ul);
        };
    });
}

function UpdateAsset(element) {
    var assetId = $(element).attr("data-assetId");
    var name = $('#Name_' + assetId).val();
    var tag = $('#Tag_' + assetId).val();
    var target = element.data("target-id");

    $.ajax({
        url: '/Teacher/UpdateAsset',
        type: 'POST',
        data: "{ name:'" + name + "', tagName:'" + tag + "', assetId:'" + assetId + "'}",
        contentType: "application/json; charset=utf-8",
        dataType: "html",
        success: function (response, data) {
            $('#assetTagsDiv_' + assetId).html(response);
            $('#Tag_' + assetId).val('');
        },
        error: function (jXhr) {
            if (typeof console != 'undefined')
                alert("Error Updating Object")
        }
    });
}

function filterAsset(element) {
    var tagFilter = element.val();

    $.ajax({
        url: '/Teacher/AddFilterTagAsset',
        type: 'POST',
        data: "{ tagFilter:'" + tagFilter + "'}",
        contentType: "application/json; charset=utf-8",
        dataType: "html",
        success: function (response, data) {
            $('#tagFiltering').html(response);
            element.val('');
            RefreshAssetList();
        },
        error: function (jXhr) {
            if (typeof console != 'undefined')
                alert("Error Updating Object")
        }
    });
}

function RemoveTagFromAsset(element) {
    var url = element.data("source");
    var target = element.data("target-id");
    var action = element.data("action");
    var callback = element.data("callback");

    $.ajax({
        url: url,
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        dataType: "html",
        success: function (response, data) {
            $('#' + target).html(response);
            if (callback != 'undefined')
                CallBackFunctions[callback](response, data, target);
            RefreshAssetList();
        },
        complete: function () {
            BindAssetLinkControls();
        },
        error: function (jXhr) {
            if (typeof console != 'undefined')
                alert("Error Updating Object")
        }
    });
}

function RefreshAssetList() {
    $.ajax({
        url: '/Teacher/AssetsPartial',
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        dataType: "html",
        success: function (response, data) {
            $('#assetList').html(response);
        },
        complete: function () {
            BindAssetLinkControls();
            BindAutocompleteTagAsset();
        },
        error: function (jXhr) {
            if (typeof console != 'undefined')
                alert("Error Updating Object")
        }
    });
}

/* Filtering Ajax */
function Filtering(element, unit) {
    var value = element.val();
    var url = element.attr('data-url');
    var target = element.attr('data-target');
    unit = (typeof unit === "undefined") ? 0 : unit;

    $.ajax({
        url: url,
        type: 'POST',
        data: "{ filter:'" + value + "', unitId:" + unit + "}",
        contentType: "application/json; charset=utf-8",
        dataType: "html",
        success: function (response, data) {
            $(target).html(response);
            $(document).foundation();
        },
        complete: function () {
            /*BindDatePicker();
            BindFilterControls();
            BindToggleControls();
            BindReveal();
            BindingUnitsDashboard();
            BindAutocomplete();
            BindAddAsset();
            BindRemoveAsset();*/
        },
        error: function (jXhr) {
            if (typeof console != 'undefined')
                alert("Error Updating Object")
        }
    });

}

function UpdateList(url, unit) {

    unit = (typeof unit === "undefined") ? 0 : unit;
    var target = '#unitList';
    $.ajax({
        url: url,
        type: 'POST',
        data: "{ unitId:" + unit + "}",
        contentType: "application/json; charset=utf-8",
        dataType: "html",
        success: function (response, data) {
            $(target).html(response);
            $(document).foundation();
        },
        complete: function () {
            /*BindDatePicker();
            BindFilterControls();
            BindToggleControls();
            BindReveal();
            BindingUnitsDashboard();
            BindAutocomplete();
            BindAddAsset();
            BindRemoveAsset();*/
        },
        error: function (jXhr) {
            if (typeof console != 'undefined')
                alert("Error Updating Object")
        }
    });

}


/*
        Update Units/Classes via Ajax call
        -------------------------------------
    */

$('[data-focus-id]').click(function () {
    var targetElemId = $(this).attr("data-focus-id");
    $('#' + targetElemId).focus();
});


function BindingUnitsDashboard() {
    $('body').on('change', '[data-dbUnitId]', function () {

        // Get Values
        var unitId = $(this).attr("data-dbUnitId");
        var url = $(this).attr("data-action");
        var title = $('#Title_' + unitId).val();
        var description = $('#Description_' + unitId).val();
        var publishFrom = $('#PublishFrom_' + unitId).val();
        //var publishTo = $('#PublishTo_' + unitId).val();
        var duration = $('#Duration_' + unitId).val();
        var quizId = $('#QuizId_' + unitId).val();

        console.log(duration);

        $.ajax({
            url: url,
            type: 'POST',
            data: "{ name:'" + title + "', description:'" + description + "', publishFrom: '" + publishFrom + "', duration: '" + duration + "', unitId:'" + unitId + "', quizId:'" + quizId + "'}",
            contentType: "application/json; charset=utf-8",
            error: function (jXhr) {
                if (typeof console != 'undefined')
                    console.log("Error Updating Object")
            }
        });
    });
}


function BindingUnits() {
    $('[data-unitId]').bind('change', function () {
        console.log("editing unit");
        // Get Values
        var unitId = $(this).attr("data-unitId");
        var url = $(this).attr("data-action");
        var title = $('#Title_' + unitId).val();
        var video = $('#VideoId_' + unitId).val();
        var document = $('#DocumentId_' + unitId).val();
        var assignment = $('#AssignmentId_' + unitId).val();
        var quiz = $('#QuizId_' + unitId).val();
        console.log("url: " + url);

        $.ajax({
            url: url,
            type: 'POST',
            data: "{ name:'" + title + "', unitId:'" + unitId + "', videoId:'" + video + "', documentId:'" + document + "', assignmentId:'" + assignment + "', quizId:'" + quiz + "'}",
            contentType: "application/json; charset=utf-8",
            error: function (jXhr) {
                if (typeof console != 'undefined')
                    console.log("Error Updating Object")
            }
        });
    });
}


$('[data-userId]').bind('click', function () {
    var userId = $(this).attr("data-userId");
    var isOn = $('#a1_' + userId).is(':checked');
    var url = $(this).parent().attr("data-source");

    $.ajax({
        url: url,
        type: 'POST',
        data: "{userId:'" + userId + "', isEnabled:'" + isOn + "'}",
        contentType: "application/json; charset=utf-8",
        //dataType: "html",
        error: function (jXhr) {
            if (typeof console != 'undefined')
                Conso.log("Error Updating Object")
        }
    });
});

$('body').on('click', '[data-courseId]', function () {
    var courseId = $(this).attr("data-courseId");
    var isOn = $('#a1_' + courseId).is(':checked');
    var url = $(this).parent().attr("data-source");

    $.ajax({
        url: url,
        type: 'POST',
        data: "{courseId:'" + courseId + "', isEnabled:'" + isOn + "'}",
        contentType: "application/json; charset=utf-8",
        error: function (jXhr) {
            if (typeof console != 'undefined')
                Conso.log("Error Updating Object")
        }
    });

});

/*
 Add Asset
----------------------------------------
*/
function BindAddAsset() {
    $('body').on('click', '.AddAssetIcon', function () {
        var unitId = $(this).attr("data-unitid");
        var url = $(this).attr("data-action");
        var targetId = $(this).attr("data-target-id");
        var assetName = $('#AsseFieldtForUnit_' + unitId).val();
        if (!assetName) {
            return;
        }
        /*
        console.log("unitId: "+unitId);
        console.log("url: "+url);
        console.log("assetName: " + assetName);
        */
        $.ajax({
            url: url,
            type: 'POST',
            data: "{assetName:'" + assetName + "', unitId:'" + unitId + "'}",
            contentType: "application/json; charset=utf-8",
            success: function (response, data) {
                $('.' + targetId).html(response);
                $('.asset-search').val('');
            },
            error: function (jXhr) {
                if (typeof console != 'undefined')
                    console.log("Error Updating Object")
            }
        });

    });
}

/*
 Add Asset
----------------------------------------
*/
function BindRemoveAsset() {
    $('body').on('click', '.RemoveAssetIcon', function () {
        var unitId = $(this).attr("data-unitid");
        var assetId = $(this).attr("data-assetid");
        var url = $(this).attr("data-action");
        var targetId = $(this).attr("data-target-id");
        /*
        console.log("unitId: "+unitId);
        console.log("assetId: "+assetId);
        console.log("url: "+url);
            */
        $.ajax({
            url: url,
            type: 'POST',
            data: "{assetId:'" + assetId + "', unitId:'" + unitId + "'}",
            contentType: "application/json; charset=utf-8",
            success: function (response, data) {
                $('.' + targetId).html(response);
            },
            error: function (jXhr) {
                if (typeof console != 'undefined')
                    Conso.log("Error Updating Object")
            }
        });

    });
}



/*
 Autocomplete
----------------------------------------
*/

$('body').on('keyup.autocomplete', '.autocomplete', function () {
    console.log('new autocomplete');
    var el = $(this);
    callback = el.attr('data-callback');
    el.autocomplete({
        source: function (request, response) {
            $.ajax({
                url: el.attr('data-source'),
                dataType: 'json',
                data: {
                    term: request.term
                },
                type: 'POST',
                success: function (data) {
                    response($.map(data, function (item, i) {
                        return {
                            label: item.userName,
                            value: item.id,
                            type: item.type
                        }
                    }));

                }
            });
        },
        change: function (event, ui) {
            if (callback) {
                //CallBackFunctions[callback](event, ui, el);
            }
        },
        select: function (event, ui) {
            console.log("I am removing it...");
                event.preventDefault();
                el.val(ui.item.label);
            if (callback) {
                CallBackFunctions[callback](event, ui, el);
            } else {
                el.attr('data-sourceid', ui.item.value.toString());
                console.log(ui.item.label);
            }
        },
        minLength: 1
    }).data("ui-autocomplete")._renderItem = function (ul, item) {
        var icon = "";
        if (item.type === 1)
            icon = 'icon-youtube-play';
        else if (item.type === 2)
            icon = 'icon-file-alt'
        else if (item.type === 3)
            icon = 'icon-picture'
        else if (item.type === 4)
            icon = 'icon-globe'
        return $("<li />")
            .data("item.autocomplete", item)
            .append("<a><i class='" + icon + "' style='margin-right:10px'></i>" + item.label + "</a>")
            .appendTo(ul);
    };
});

function BindAutocomplete() {
    /*$('input.autocomplete').each(function (i, el) {
        el = $(el);
        callback = el.attr('data-callback');
        el.autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: el.attr('data-source'),
                    dataType: 'json',
                    data: {
                        term: request.term
                    },
                    type: 'POST',
                    success: function (data) {
                        response($.map(data, function (item, i) {
                            return {
                                label: item.userName,
                                value: item.id,
                                type: item.type
                            }
                        }));

                    }
                });
            },
            select: function (event, ui) {
                if (callback) {
                    CallBackFunctions[callback](event, ui, el);
                } else {
                    event.preventDefault();
                    el.val(ui.item.label);
                    el.attr('data-sourceid', ui.item.value.toString());
                    console.log(ui.item.label);
                }
            },
            minLength: 1
        }).data("ui-autocomplete")._renderItem = function (ul, item) {
            var icon = "";
            if (item.type === 1)
                icon = 'icon-youtube-play';
            else if (item.type === 2)
                icon = 'icon-file-alt'
            else if (item.type === 3)
                icon = 'icon-picture'
            else if (item.type === 4)
                icon = 'icon-globe'
            return $("<li />")
                .data("item.autocomplete", item)
                .append("<a><i class='" + icon + "' style='margin-right:10px'></i>" + item.label + "</a>")
                .appendTo(ul);
        };
    });*/
}

var CallBackFunctions = {

    AddAssetToAssignmentList: function (event, ui, el) {
        var controller = el.attr('data-callback-controller');
        var target = el.attr('data-callback-target-id');
        $.ajax({
            url: controller,
            data: {
                assetName: ui.item.value.toString()
            },
            type: 'POST',
            success: function (data) {
                $('#' + target).html(data);
                $('#' + target).css('display', 'block');
                el.val('');
            }
        });
    },

    RemoveAssetFromAssignmentList: function (response, data, target) {
        console.log(target);
        console.log(data);
        console.log(response);
        if ($('#' + target).html().trim() === '') {
            $('#' + target).css('display', 'none');
        }
    },

    AddCourseToModule: function (event, ui, el) {
        UpdateModule(el);
    },

    AddUserToMessageUserList: function (event, ui, el) {
        console.log('i am here...');
        var controller = el.attr('data-callback-controller');
        var target = el.attr('data-callback-target-id');
        $.ajax({
            url: controller,
            data: {
                id: ui.item.value
            },
            type: 'POST',
            success: function (data) {
                $('#' + target).html(data);
                $('#' + target).css('display', 'block');
                el.val('');
            }
        });
    },
    UpdateSubmissionFilters: function (parameters) {
        console.log("in the callback");
        console.log(parameters);
        var url = "RefreshingSubmissionFilters";
        var target = "GradeFilter";

        $.ajax({
            url: url,
            type: 'POST',
            contentType: "application/json; charset=utf-8",
            dataType: "html",
            data: parameters,
            success: function (response, data) {
                $('#' + target).html(response);                
            },
            complete: function () {

            },
            error: function (jXhr) {
                //if (typeof console != 'undefined')
                //    alert("Error Updating Object")
            }
        });
    }
}

/* module */
function UpdateModule(element) {
    var moduleId = element.attr("data-moduleId");
    var title = $('#Title_' + moduleId).val();
    var description = $('#Description_' + moduleId).val();
    var publishFrom = $('#PublishFrom_' + moduleId).val();
    var publishTo = $('#PublishTo_' + moduleId).val();
    var courseName = $('#Course_' + moduleId).val();

    // Post Values
    $.ajax({
        url: '/Teacher/UpdateModule',
        type: 'POST',
        data: "{ name:'" + title + "', description:'" + description + "', moduleId:'" + moduleId + "', publishFrom:'" + publishFrom + "', publishTo:'" + publishTo + "', courseName:'" + courseName + "'}",
        contentType: "application/json; charset=utf-8",
        dataType: "html",
        success: function (response, data) {
            $('#courseTags_' + moduleId).html(response);
            $('#Course_' + moduleId).val('');
        },
        error: function (jXhr) {
            if (typeof console != 'undefined')
                alert("Error Updating Object")
        }
    });
}

function RemoveCourseFromModule(element) {
    var url = element.data("source");
    var target = element.data("target-id");

    $.ajax({
        url: url,
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        dataType: "html",
        success: function (response, data) {
            $('#' + target).html(response);
        },
        complete: function () {

        },
        error: function (jXhr) {
            if (typeof console != 'undefined')
                alert("Error Updating Object")
        }
    });
}

/* Date Formatting */
function unformatDate(dateFormatted) {
    var parts = dateFormatted.split(' ');
    var newFormat;
    switch (parts[2]) {
        case 'January':
            newFormat = '01';
            break;
        case 'February':
            newFormat = '02';
            break;
        case 'March':
            newFormat = '03';
            break;
        case 'April':
            newFormat = '04';
            break;
        case 'May':
            newFormat = '05';
            break;
        case 'June':
            newFormat = '06';
            break;
        case 'July':
            newFormat = '07';
            break;
        case 'August':
            newFormat = '08';
            break;
        case 'September':
            newFormat = '09';
            break;
        case 'October':
            newFormat = '10';
            break;
        case 'November':
            newFormat = '11';
            break;
        case 'December':
            newFormat = '12';
            break;
    }

    newFormat = parts[1] + '/' + newFormat + '/' + parts[3];
    return newFormat;

}

function formatDate(dateFormatted) {
    var parts = dateFormatted.split('/');
    var newFormat;
    switch (parts[1]) {
        case '01':
            newFormat = 'January';
            break;
        case '02':
            newFormat = 'February';
            break;
        case '03':
            newFormat = 'March';
            break;
        case '04':
            newFormat = 'April';
            break;
        case '05':
            newFormat = 'May';
            break;
        case '06':
            newFormat = 'June';
            break;
        case '07':
            newFormat = 'July';
            break;
        case '08':
            newFormat = 'August';
            break;
        case '09':
            newFormat = 'September';
            break;
        case '10':
            newFormat = 'October';
            break;
        case '11':
            newFormat = 'November';
            break;
        case '12':
            newFormat = 'December';
            break;
    }

    var d = new Date(parts[2], parts[1] - 1, parts[0]);

    var weekday = new Array(7);
    weekday[0] = "Sunday";
    weekday[1] = "Monday";
    weekday[2] = "Tuesday";
    weekday[3] = "Wednesday";
    weekday[4] = "Thursday";
    weekday[5] = "Friday";
    weekday[6] = "Saturday";

    var newWeekday = weekday[d.getDay()];

    newFormat = newWeekday + ' ' + parts[0] + ' ' + newFormat + ' ' + parts[2];
    return newFormat;

}


/* Reveal buttons
   --------------------------------------*/
function BindReveal() {
    $('body').on('click', '.reveal-bottom', function () {
        $('.st-menu').css('width', '100%');
        $('.st-menu').css('height', '100%');
        $('.st-menu').css('padding-top', '20px');
        $('.st-menu').css('top', '65%');
        $('.no-csstransforms3d .st-pusher').css('padding-bottom', '35%');
        $('.no-js .st-pusher').css('padding-bottom', '35%');

        //insert content
        var contentID = $(this).data('id');
        if (contentID) {
            var contentBody = $('#' + $(this).data('id')).html();
            $('#menu-2').html(contentBody);
        } else {
            var contentBody = $('#newPost').html();
            $('#menu-2').html(contentBody);
        }
    });
    $('body').on('click', '.reveal-side', function () {
        console.log("hello reveal");
        $('.st-menu').css('width', '95%');
        $('.st-menu').css('height', '100%');
        $('.st-menu video').css('width', '100%');
        $('.st-menu iframe').css('width', '100%');

        $('.no-csstransforms3d .st-pusher').css('padding-bottom', '35%');
        $('.no-js .st-pusher').css('padding-bottom', '35%');

        $('.close-reveal').css('display', 'block');
        $('.close-reveal').data('video', $(this).data('video'));

        //insert content
        var contentID = $(this).data('id');
        if (contentID) {
            var contentBody = $('#' + $(this).data('id')).html();
            $('#menu-1').html(contentBody);
        } else {
            var contentBody = $('#newPost').html();
            $('#menu-1').html(contentBody);
        }

        //play video
        var myVideo = $('#' + $(this).data('video')).get(0);
        if (myVideo != undefined && myVideo != null) {
            myVideo.play();
        }

    });
    $('body').on('click', '.close-reveal', function () {
        //stop video
        $(this).css('display', 'none');
        var myVideo = $('#' + $(this).data('video')).get(0);
        myVideo.pause();
    });
}

function BindSubmitGrades() {

    $('body').on('click', '.submitGrade', function () {          
        var id = $(this).attr('data-submissionid');
        var url = $(this).attr('data-url');
        var gradeValue = $("#grade_" + id).val();
        var comment = $("#comments_" + id).val();      
        var assignment = $(this).attr('data-assignmentid');

        if (gradeValue == "" || gradeValue > 100 || isNaN(gradeValue)) {
            console.log("grade not valid");
            return;
        }


        $("#grade_" + id).prop('disabled', true);
        $("#comments_" + id).prop('disabled', true);
        $("#label_" + id).removeClass("label success radius").addClass("label secondary radius");
        $("#label_" + id).text("Marked");
        $("#submitGrade_" + id).text("Marked");
        $("#submitGrade_" + id).addClass("disabled")

        //console.log("id: " + id);
        //console.log("comment: " + comment);
        //console.log("gradeValue: " + gradeValue);
        //console.log("url: " + url);

   
        $.ajax({
            url: url,
            type: 'POST',
            data: "{ id:'" + id + "', comment:'" + comment + "', gradeValue: '" + gradeValue + "'}",
            contentType: "application/json; charset=utf-8",
            dataType: "html",
            error: function (jXhr) {
                if (typeof console != 'undefined')
                    alert("Error Updating Object")
            }

        });
    });
}

function BindFilterSubmissions() {
    $('body').on('click', '.FilterSelector', function () {
        var filterValue = $(this).attr('data-value');
        $("#AllSelector").attr('class', '');
        $("#SubmittedSelector").attr('class', '');
        $("#NotSubmittedSelector").attr('class', '');
        $("#MarkedSelector").attr('class', '');

        $("#FilterHidden").val(filterValue);

        switch (filterValue) {
            case "All":
                $("#AllSelector").attr('class', 'active');
                break;
            case "Submitted":
                $("#SubmittedSelector").attr('class', 'active');
                break;
            case "NotSubmitted":
                $("#NotSubmittedSelector").attr('class', 'active');
                break;
            case "Marked":
                $("#MarkedSelector").attr('class', 'active');
                break;
        }

        FilterSubmissions();
    });
}

function BindFilterSubmissionsDropList() {

    $('body').on('change', '.FilterDropList', function () {
        FilterSubmissions();
    });
}

function FilterSubmissions() {
    var course = $("#CoursesFilterDropList").val();
    var module = $("#ModulesFilterDropList").val();
    var assignment = $("#AssignmentsFilterDropList").val();
    var student = $("#StudentsFilterDropList").val();
    var filter = $("#FilterHidden").val();



    var url = "RefreshingSubmissionList";
    var parameters = "{ course:'" + course + "', module:'" + module + "', assignment: '" + assignment + "' , student: '" + student + "', filter: '" + filter + "'}";
    var target = "SubmissionsList";

    //console.log("course: " + course);
    //console.log("module: " + module);
    //console.log("assignment: " + assignment);
    //console.log("student: " + student);
    //console.log("filter: " + filter);

    $.ajax({
        url: url,
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        dataType: "html",
        data: parameters,
        success: function (response, data) {
            $('#' + target).html(response);            
            CallBackFunctions['UpdateSubmissionFilters'](parameters);
        },
        complete: function () {
            //BindAssetLinkControls();
        },
        error: function (jXhr) {
            //if (typeof console != 'undefined')
            //    alert("Error Updating Object")
        }
    });
}

/* Quiz Question Validation */
$('#savequestionbtn').click(function () {
    $('.validation-summary-errors').remove();
    var isValid = true;
    var errormsg = "";
    if ($("#Name").val() == "") {
        errormsg += "Question<br />";
        $("#Name").addClass('input-validation-error');
        isValid = false;
    }

    var qACount = 0;
    var correctedAnswersSelected = 0;

    var obj = $("input[dataid='quesChoice']");
    var fieldsAnswers = new Array();
    var fieldPoints = new Array();
    var fieldcheckbox = new Array();

    $.each(obj, function (key, value) {
        if ($("#" + value.id).val() == "") {
            qACount += 1;
            fieldsAnswers.push("#" + value.id);
            fieldPoints.push("#QuestionChoices_" + key + "__PointsValue");
            fieldcheckbox.push("input[name='QuestionChoices[" + key + "].CorrectAnswer']");
        }
        else {
            if ($("#QuestionChoices_" + key + "__PointsValue").val() == "") {
                var cnt = key + 1;
                isValid = false;
                $("#QuestionChoices_" + key + "__PointsValue").addClass('input-validation-error');
                errormsg += "Point value " + cnt + " must be entered<br />";
            }

            if ($("input[name = 'QuestionChoices[" + key + "].CorrectAnswer']").is(":checked")) {
                correctedAnswersSelected += 1;
            } else {
                fieldcheckbox.push("input[name='QuestionChoices[" + key + "].CorrectAnswer']");
            }
        }

    });


    var QuestTypeValue = $("select[id='QuestionQuizTypeSelected'] option:selected").val();
    //QuestTypeValue = 1: multiple selection
    //QuestTypeValue = 2: single selection
    //QuestTypeValue = 3: free text

    if (correctedAnswersSelected != 1 && (QuestTypeValue == 2 || QuestTypeValue == 3)) {
        isValid = false;
        errormsg += "There must be only one correct answer selected<br />";
    } else if (correctedAnswersSelected < 2 && QuestTypeValue == 1) {
        isValid = false;
        errormsg += "There must be more than one correct answer selected for multiple selection questions<br />";
    }



    if (QuestTypeValue == "1" || QuestTypeValue == "2") { //Multiple/Single selection
        if (qACount > 2) {
            isValid = false;
            errormsg += "Answer selection must be 2 or more<br />";
            for (var i = 0; i != 2; i++) {
                //for (var i = 0; i != fieldsHighlight.length; i++) {
                $(fieldsAnswers[i]).addClass('input-validation-error');
                $(fieldPoints[i]).addClass('input-validation-error');
                $(fieldcheckbox[i]).addClass('input-validation-error');
                $(fieldcheckbox[i]).parent().children('span.checkbox').addClass('input-validation-error');
            }

        }
    }
    else if ($("select[id='QuestionType_Id'] option:selected").val() == "3") { //Free Text selection
        if (qACount > 3) {
            isValid = false;
            errormsg += "Answer selection must be 1 or more<br />";
            for (var i = 0; i != 1; i++) {
                //for (var i = 0; i != fieldsHighlight.length; i++) {
                $(fieldsAnswers[i]).addClass('input-validation-error');
                $(fieldPoints[i]).addClass('input-validation-error');
                $(fieldcheckbox[i]).addClass('input-validation-error');
                $(fieldcheckbox[i]).parent().children('span.checkbox').addClass('input-validation-error');
            }
        }
    }

    if (!isValid) {
        //alert("Following fields are mandatory :  \n\n" + errormsg);
        $("#validation").append('<div class="validation-summary-errors"><p>Following fields are mandatory: </p>' + errormsg + '</div>')
    }
    return isValid;
    //return false;
});



/* Toggle help text on nav with cookieee
  --------------------------------------*/

$(function () {
    $('.toggle-help').on('click', function () {
        //console.log('toggled!');                            

        $('.top-bar-section').toggleClass('help-off');
        //console.log('cookie has: ' + readCookie('ShowHideHelpText'));

        if (readCookie('ShowHideHelpText') == 'help-off') {
            createCookie('ShowHideHelpText', 'help-on');
            //console.log('cookie now has: ' + readCookie('ShowHideHelpText'));
        }
        else if (readCookie('ShowHideHelpText') == 'help-on') {
            createCookie('ShowHideHelpText', 'help-off');
            //console.log('cookie now has: ' + readCookie('ShowHideHelpText'));
        }
        return false;

    });
    //console.log('im here!');
    if (readCookie('ShowHideHelpText') != null) {
        //console.log('found cookie!');
        $('.top-bar-section').removeClass('help-off');

        //console.log('cookie has: ' + readCookie('ShowHideHelpText'));

        $('.top-bar-section').addClass(readCookie('ShowHideHelpText'));
        //console.log('help should now be fixed');
    }
    else {
        //console.log('no cookie set...');
        createCookie('ShowHideHelpText', 'help-off');
        //console.log('set cookie (help is currently off)');
    }

});




function createCookie(name, value, days) {
    if (days) {
        var date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        var expires = "; expires=" + date.toGMTString();
    }
    else var expires = "";
    document.cookie = name + "=" + value + expires + "; path=/";
}

function readCookie(name) {
    var nameEQ = name + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}

function eraseCookie(name) {
    createCookie(name, "", -1);
}
