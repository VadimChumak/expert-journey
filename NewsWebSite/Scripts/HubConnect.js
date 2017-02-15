    if (typeof ArticlePage === 'undefined') {
        var hub = $.connection.commentsHub;
        $.connection.hub.start();
    }


hub.client.notify = function (commentId, fromWho, message, articleId, notifiId) {
    $('.nCnt').each(function (index, item) {
        var number = parseInt($(item).text(), 10) + 1;
        $(item).text(number);
    });
    Materialize.toast(fromWho + ': ' + htmlEncode(message), 5000);
    if (typeof NotificationsPage !== 'undefined')
    {
        AddNotify(commentId, fromWho, message, articleId, notifiId);
    }
}