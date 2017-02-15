ArticlePage = true;

var hub = $.connection.commentsHub;

var Data;

var myId;
var MaxCommentLength;
var ArticleId;;
var MyName;
var commentBlockLoaded = false;
var CommentsBlock;
var toReset;
var MyImage;
var CommentId;

var noComments = false;
$(document).ready(function () {
    CommentsBlock = $('#CommentsBlock');
    Data = $('#articledata');
    CommentId = $(Data).data('commentid');
   // alert(CommentId);
    myId = $(Data).data('curuserid');
    MaxCommentLength = $(Data).data('maxlen');
    ArticleId = $(Data).data('articleid');
    MyName = $(Data).data('myname');
    MyImage = $(Data).data('image');
    $.connection.hub.start().done(function () {
        if (CommentId > 0) {
            $('body, html').scrollTo('#loaderBlock');
            //console.log('scrollTo loader');
        }
        if ($(document).height() - 120 <= $(window).height() || $(window).scrollTop() >= $(document).height() - $(window).height() - 120) {
            commentBlockLoaded = true;
            LoadComments();
        }
        $(window).scroll(function () {
            if (!commentBlockLoaded && $(window).scrollTop() >= $(document).height() - $(window).height() - 120) {
                commentBlockLoaded = true;
                LoadComments();
            }
        });
        $('#comments').on("click", ".comment .contentComment", function () {
            var parent = $(this).parent();
            if (!$(parent).hasClass('green lighten-5')) {
                $('.comment.green.lighten-5').removeClass('green').removeClass('lighten-5');
            }
            $(parent).toggleClass('green lighten-5');
        });
    });

    $('#send').on("click", function () {
        var item = $('#message');
        var text = $(item).val().trim();
        var result = ValidateComment(text);
        if (result == "ok") {
            SendComment(0, text);
            $(item).val('');
            ResetDefaults();
        }
        else {
            var validationMsg = $('#sendBlock').find('.validComment:first');
            $(validationMsg).text(result).show();
            setTimeout(FadeOut, 5000, validationMsg);
        }
    });
    $('#message').on("click", function () {
        ResetDefaults();
    });
    var CommentsBlock = $('#CommentsBlock');
    $(CommentsBlock).on('click', '.buttonsBlock .showSendBlock', function () {
        ResetDefaults();
        var buttons = $(this).parent();
        var comment = $(buttons).parent();
        $(buttons).find('.hidegroup:not(.hidden)').addClass('hidden').addClass('dis');
        $(comment).find('.buttonsBlock .sendBlock:first').removeClass('hidden').addClass('act');
        toReset = $(comment);
    });

    $(CommentsBlock).on('click', '.buttonsBlock .replyBtn', function () {
        var sendBlock = $(this).parent();
        toReset = $(sendBlock).parent().parent();
        var id = $(sendBlock).parent().attr('commentid');
        //id = id.replace('send-', '');
        var textArea = $(this).parent().find('.messageTextBox:first');
        var text = textArea.val().trim();
        var result = ValidateComment(text);
        if (result == "ok") {
            SendComment(id, text)
            $(textArea).val('');
            ResetDefaults();
        }
        else {
            var validationMsg = $(sendBlock).find('.validComment:first');
            $(validationMsg).text(result).show();
            setTimeout(FadeOut, 5000, validationMsg);
            //Materialize.toast('Invalid input', 2000, 'red');
        }
    });
    $(CommentsBlock).on('click', '.buttonsBlock .editButton', function () {
        ResetDefaults();
        $(this).addClass('hidden').addClass('dis');
        var commentsBlock = $(this).parent();
        $(commentsBlock).find('.hidegroup').addClass('hidden').addClass('dis');
        var comment = $(commentsBlock).parent();
        toReset = $(comment);
        var text = $(comment).find('.contentComment .staticCommentText:first').addClass('hidden').addClass('dis').text();
        var edit = $(comment).find('.contentComment .editor:first');
        $(edit).text(text);
        $(edit).removeClass('hidden').addClass('act');
        $(comment).find('.buttonsBlock .saveButton:first').removeClass('hidden').addClass('act');
    });

    $(CommentsBlock).on('click', '.buttonsBlock .saveButton', function () {
        var comment = $(this).parent().parent();
        toReset = $(comment);
        var contentComment = $(comment).find('.contentComment:first');
        var id = $(comment).attr('id');
        var value = $(contentComment).find('.commentText.editor:first').val().trim();
        var oldValue = $(contentComment).find('.staticCommentText:first');
        if (value == $(oldValue).text()) {
            ResetDefaults();
        }
        else {
            var result = ValidateComment(value);
            if (result == "ok") {
                $(oldValue).text(value);
                hub.server.edit(id, value);
                ResetDefaults();
            }
            else {
                var validationMsg = $(comment).find('.contentComment .validEdit:first');
                $(validationMsg).text(result).show();
                setTimeout(FadeOut, 5000, validationMsg);
                //Materialize.toast('Invalid input', 2000, 'red');
            }
        }
    });

    $(CommentsBlock).on('click', '.deleteButton', function () {
        var id = $(this).parent().addClass('hidden').attr('commentid');
        hub.server.delete(id);
        ResetDefaults();
    });
});


function LoadComments() {
    $.ajax({
        url: '/News/GetComments',
        method: 'POST',
        dataType: 'Json',
        async: true,
        data: { "articleId": ArticleId },
        beforeSend: function () {
            $('#loader').removeClass("hidden");
            hub.server.connect(ArticleId);
        }
    }).done(function (data) {
        if (data.length > 0) {
            $.each(data, function (index, data) {
                var templ = TemplateReplace(data.Id, data.UserId, data.UserName, data.Text, data.Created.replace('T', ' '), data.UserImage);
                if (data.Depth == 0) {
                    $('#comments').prepend(templ);
                }
                else {
                    $('#' + data.ReplyCommentId).find('.replyBlock:first').prepend(templ);
                }
                if (data.Deleted) {
                    var comment = $('#' + data.Id);
                    DeleteItem(comment);
                }
            });
        }
        $('#loaderBlock').addClass("hidden");
        $('#sendBlock').removeClass("hidden");
        if (CommentId > 0)
        {
                $('body, html').scrollTo('#' + CommentId);
                $('#' + CommentId).toggleClass('green lighten-5');
        }
    });
}

hub.client.result = function (commentId, sendId, result, date) {
    var comment = $('#' + sendId);
    if (result == "ok") {
        $(comment).attr('id', commentId);
        $(comment).find('.commentDate:first').text(date);
        $(comment).find('.buttonsBlock:first').removeClass('hidden').attr('commentid', commentId);
        $(comment).find('.lineloaderplace:first').remove();
    }
    console.log(result);
}

hub.client.addMessage = function (id, userId, name, message, date, reply, userimage) {
   // alert('got it!');
    var templ = TemplateReplace(id, userId, name, message, date, userimage);
    //console.log("id: " + id + " userid:" + userId + " message: " + message + " date: " + date + " img: " + userimage);
    if (reply == 0) {
        $('#comments').prepend(templ);
    }
    else {
        $('#' + reply).find('.replyBlock:first').prepend(templ);
    }
}



hub.client.edit = function (commentId, text, date) {
    var commentContent = $('#' + commentId).find('.contentComment:first');
    $(commentContent).find('.staticCommentText:first').text(text);
    $(commentContent).find('.commentDate:first').text(date);
}
hub.client.delete = function (commentId) {
    var comment = $('#' + commentId);
    DeleteItem(comment);
}

function SendComment(replyId, text) {
    var sendId = guid();
    hub.server.send(ArticleId, replyId, text, sendId);
    var templ = TemplateReplace(sendId, myId, MyName, text, "", MyImage);
    templ = $(templ);
    if (replyId == 0) {
        $('#comments').prepend(templ);
    }
    else {
        $('#' + replyId).find('.replyBlock:first').prepend(templ);
    }

    $(templ).addClass('loading');
    var progressLine = $('<div />').addClass("progress").append($('<div />').addClass('indeterminate'));
    $(templ).find('.lineloaderplace:first').append($('<div />').addClass('progressline').append(progressLine));
    $(templ).find('.buttonsBlock:first').addClass('hidden');
}


function ValidateComment(text) {
    if (text.length > MaxCommentLength) {
        return "The max length of the comment is " + MaxCommentLength;
    }
    else if (text.length == 0) {
        return "Comment can not be empty";
    }
    return "ok";
}

function TemplateReplace(id, userId, name, message, date, image) {
    var templ = $("#template").html();
    templ = templ.split('[Id]').join(id);
    templ = templ.replace('[Image]', image);
    templ = templ.split('[userId]').join(userId);
    templ = templ.replace('[Name]', htmlEncode(name));
    templ = templ.replace('[Text]', htmlEncode(message));
    templ = templ.replace('[Date]', date.replace('T', ' '));
    if (userId == myId) {
        templ = templ.split('[Class]').join('commentsBtn');
    }
    else {
        templ = templ.split('[Class]').join('hidden');
    }
    return templ;
}


function ResetDefaults() {
    var block = $(toReset);
    $(block).find('.editor.act:first').addClass('hidden').removeClass('act');
    $(block).find('.staticCommentText.hidden:first').removeClass('hidden');
    $(block).find('.saveButton.act:first').addClass('hidden').removeClass('act');
    $(block).find('.sendBlock.act:first').addClass('hidden').removeClass('act');
    $(block).find('.buttonsBlock:first .hidegroup.hidden.dis').removeClass('hidden').removeClass('dis');
}

function guid() {
    return s4() + s4() + s4();
}

function s4() {
    return Math.floor((1 + Math.random()) * 0x10000)
      .toString(16)
      .substring(1);
}

function FadeOut(item) {
    $(item).fadeOut(1000, function () {
        $(item).hide();
    });
}

function DeleteItem(comment)
{
    $(comment).find('.buttonsBlock:first').addClass('hidden');
    var content = $(comment).find('.contentComment:first');
    $(content).find('.NameImage img:first').addClass('hidden').attr('src', '');
    $(content).find('.staticCommentText:first').text('Comment has been deleted');
    $(content).find('.NameImage .commentName:first').text('');
}