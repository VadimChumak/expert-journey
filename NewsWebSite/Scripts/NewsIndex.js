var Data = $('#dataArticleList');
var PageCnt = $(Data).data('pagecnt');
var Type = $(Data).data('showbytags');
var LastId = $(Data).data('lastid');

if (PageCnt == 1)
{
    $('#loaderBlock').addClass('hidden');
}

var inProgress = false;
var startFrom = 1;
var userId = 0;
var onlyInterestingNews = false;

var loader = $('#loader');

function RequestArticles() {
    $.ajax({
        url: '/News/GetArticles',
        //              timeout: 3000,
        method: 'POST',
        dataType: 'Json',
        async: true,
        data: { "page": startFrom, "lastId": LastId, "type": Type },
        beforeSend: function () {
            $(loader).removeClass('hidden');
           // $('body, html').scrollTop($(document).height());
            inProgress = true;
        }
    }).done(function (data) {
        console.log(data);
        if (data.length > 0) {
            $.each(data, function (index, data) {
                var templ = ($("#template").html().split("[Id]").join(data.Id));
                templ = templ.split("[Title]").join(data.Title);
                templ = templ.replace('[ShortDescription]', data.ShortDescription);
                templ = templ.split("[Date]").join(data.CreateDate.replace("T", " "));
                templ = templ.split('[UpdateDate]').join(data.LastUpdateDate.replace("T", " "));
                if (data.Image != 'Empty') {
                    var image = $('#imageTempl').html();
                    image = image.split('[Id]').join(data.Id);
                    image = image.split('[Image]').join(data.Image);
                    templ = templ.replace('[ImagePlaceholder]', image);
                }
                else {
                    var placeholder = $('#placeholderTemplate').html();
                    placeholder = placeholder.replace('[Id]', data.Id);
                    templ = templ.replace('[ImagePlaceholder]', placeholder);
                }
                var templ = $(templ);
                var grid = $('#grid');
                $('#grid').append(templ).masonry('appended', templ);
                if (data.CreateDate == data.LastUpdateDate)
                {
                    $(templ).find('.updatedate').hide();
                }
                $(templ).find("abbr.timeago").timeago();
                $(templ).find('.tooltipped').tooltip({ delay: 50 });
            });
            setTimeout(SetTime, 200);
            setTimeout(CallAdaptive, 300);
            LastId = data[data.length - 1].Id;
        }

        $(loader).addClass('hidden');
        startFrom++;
        if (startFrom == PageCnt) {
            $('#loaderBlock').addClass('hidden');
        }
        inProgress = false;
    });
}

function CallAdaptive() {
    $('#grid').masonry({
        itemSelector: '.grid-item',
        singleMode: false,
        isResizable: true,
        isAnimated: true,
        animationOptions: {
            queue: false,
            duration: 500
        }
    });

}
function SetTime() {
    $("abbr.timeago").timeago();
    $('.tooltipped').tooltip({ delay: 50 });
}

$(document).ready(function () {
    CallAdaptive();
    if (startFrom < PageCnt) {
        if ($(document).height() - 180 <= $(window).height() || $(window).scrollTop() >= $(document).height() - $(window).height() - 2180) {
            RequestArticles();
        }

        $(window).scroll(function () {
            if (!inProgress && startFrom < PageCnt && $(window).scrollTop() >= $(document).height() - $(window).height() - 180) {
                RequestArticles();
            }
        });
    }
});
