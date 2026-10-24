


/*
    重新封装Uploadify,主要是为了实现自定义上传界面
        内容:
            1.让flash浮动在指定的容器上(需要指定容器的style="posotion:relative;")
            2.此方法中定义一些公共配置,之后根据配置key来获取
            3.改写了事件onDialogClose如果返回false则不执行后面的上传文件操作,为true时则继续
            4.改写了事件onUploadSuccess直接返回服务端返回的数据,且有错误时直接报错
*/
(function (w) {

    var i = 0;
    var config = {
        "article_img": { uploader: "/common/UpLoadFile?key=article_img", fileTypeExts: "*.jpg;*.jpeg;*.png;*.gif;*.bmp", fileTypeDesc: "图片文件" },
        "ad_img": { uploader: "/common/UpLoadFile?key=ad_img", fileTypeExts: "*.jpg;*.png", fileTypeDesc: "图片文件" },
        "goods_img": { uploader: "/common/UpLoadFile?key=goods_img", fileTypeExts: "*.jpg;*.png", fileTypeDesc: "图片文件" },
        "matchlogo_img": { uploader: "/common/UpLoadFile?key=matchlogo_img", fileTypeExts: "*.jpg;*.png", fileTypeDesc: "图片文件" },
        "award_img": { uploader: "/common/UpLoadFile?key=award_img", fileTypeExts: "*.jpg", fileTypeDesc: "图片文件" },
        "adzip_img": { uploader: "/common/UpLoadFile?key=adzip_img", fileTypeExts: "*.png", fileTypeDesc: "图片文件" },
        "renter_img": { uploader: "/common/UpLoadFile?key=renter_img", fileTypeExts: "*.jpg;*.png", fileTypeDesc: "图片文件" },//主播头像
    };    
    var FloatUpload = function (obj, c) {
        i++;
        if (typeof (c.configkey) != "undefined") {
            var chooseconfig = config[c.configkey];
            if (typeof (chooseconfig) != "undefined") {
                chooseconfig.uploader += c.extendparas || "";
                for (var att in chooseconfig) {
                    c[att] = c[att] || chooseconfig[att];
                }
            }
        }
        var style = "position:absolute;filter:alpha(opacity=0);cursor:pointer;z-index:10;top:0px;left:0px;opacity:0;-moz-opacity:0;"
        var containerID = "div_FloatUpload" + i;
        var div_swf = "<div id='" + containerID + "'></div>";
        obj.append(div_swf);

        //上传控件配置预处理
        c["height"] = obj.outerHeight();
        c["width"] = obj.outerWidth();
        c["swf"] = '/Plugins/uploadify/uploadify.swf';
        c["onSelectError"] = function (file, errorCode, errorMsg) {
            if (errorCode == SWFUpload.QUEUE_ERROR.QUEUE_LIMIT_EXCEEDED) {
                this.queueData.errorMsg = "只能选择{0}个文件".format(c.uploadLimit);
            }
            if (SWFUpload.QUEUE_ERROR.INVALID_FILETYPE) {
                this.queueData.errorMsg = "文件类型不对".format(c.uploadLimit);
            }
        };
        
        c["onUploadError"] = function (file,errorCode,errorMsg,errorString) {
            alert("上传出错");
        };
        if (typeof (c.onDialogClose) == "function") {
            var ondialogclose = c.onDialogClose;
            c.onDialogClose = function (file) {
                if (!ondialogclose()) {
                    $("#" + containerID).uploadify("cancel", "*");
                }
            };
        }
        if (typeof (c.onUploadSuccess == "function")) {
            var onuploadsuccess = c.onUploadSuccess;
            c.onUploadSuccess = function (file, data, response) {                
                try {
                    var result;
                    eval("result = " + data);
                    if (result.IsSuccess) {
                        onuploadsuccess(result, obj);
                    } else {
                        alert(result.Message);
                    }
                } catch (e) {
                    alert("上传报错");
                }
            };
        }
        
        $("#" + containerID).uploadify(c);
        $("#" + containerID).attr("style", style);
        $("#" + containerID + " object").css("position", "static");
        $('.uploadify-button').css("display", "none");
    };


    $().ready(function () {

        $("head").append('<link href="/Plugins/Uploadify/uploadify.css" rel="stylesheet" />');
        $("head").append('<style>\
                                .uploadify-queue {\
                                display: none;\
                            }\
                            </style>');
        $("head").append('<script src="/Plugins/Uploadify/jquery.uploadify.js"></script>');

        jQuery.fn.extend({
            FloatUpload: function (c) {
                $(this).each(function () {
                    return new FloatUpload($(this), c);
                });
            }
        });
    });

})(window);