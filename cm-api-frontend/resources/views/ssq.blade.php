<!DOCTYPE html>
<html>

<head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <style>
        body {
            background: #1b1b1e;
        }
        #s-chat-plugin {
            max-height: 118vh !important;
            height: 118% !important;
        }
    </style>
</head>

<body>
    <script src="https://example.invalid/js/project_202983_209493_1734777016.js"></script>
    <script type="text/javascript">
        var urlParams = new URLSearchParams(window.location.search);
        const userId = urlParams.get("userId");
        const username = urlParams.get("username");
        const phone = urlParams.get("phone");
        /*
        if (!userId || !username || !phone) {
            throw new Error("Missing required parameters");
        }
        */
        window.__ssc.setting = {
            hideIcon: true
        };
        ssq.push("onCloseChat", function() {
            if (window.flutter_inappwebview) {
                window.flutter_inappwebview.callHandler("sendToFlutter", {
                    event: "CHAT_MINIMIZED",
                });
            }
        });
        ssq.push("setLoginInfo", {
            user_id: userId,
            user_name: username,
            phone: phone,
        });

        ssq.push("onReady", () => {
            ssq.push("chatOpen");
        });
    </script>
</body>

</html>
